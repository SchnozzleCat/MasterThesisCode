using Schnozzle.Core.Extensions;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.Core.SchnozzleObject.Interfaces;
using Schnozzle.ECS.Console;
using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components;
using Spark.Status.Components.Intrinsics;
using Spark.Status.Components.Stats;
using Spark.Status.Jobs.Intrinsics;
using Spark.Status.SchnozzleObjects;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Spark.Status.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(StatusSystemGroup))]
    [UpdateAfter(typeof(StatSystem))]
    public partial struct IntrinsicsSystem : ISystem
    {
        [InjectBlob]
        private SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs _statBlobs;

        [InjectBlob]
        private SchnozzleObject<DamageTypeSettings, DamageTypeBlob>.Blobs _damageTypeBlobs;

        [InjectBlob]
        private SchnozzleObject<IntrinsicSettings, IntrinsicBlob>.Blobs _intrinsicBlobs;

        private int _intrinsicCount;

        private NativeArray<SchnozzleReference<IntrinsicSettings>> _orderedIntrinsics;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<ClientServerTickRate>();

            _orderedIntrinsics = IntrinsicSettings.GetAll().Ordered().AsNativeArray(x => x.AsRef, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _orderedIntrinsics.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var networkTick = GetSingleton<NetworkTime>().ServerTick;

            var query = QueryBuilder().WithAll<ModifyIntrinsic>().WithAll<Stat>().Build();
            //Check if we have any possible modifications this frame.
            query.SetChangedVersionFilter(ComponentType.ReadOnly<ModifyIntrinsic>());

            JobHandle updateEventsHandle = default;

            if (!query.IsEmpty)
            {
                var modificationStream = new UnsafeStream(
                    query.CalculateChunkCountWithoutFiltering(),
                    state.WorldUpdateAllocator
                );

                state.Dependency = new ProcessIntrinsicModificationsJob
                {
                    Tick = networkTick.TickIndexForValidTick,
                    DamageTypeBlobs = _damageTypeBlobs,
                    IntrinsicBlobs = _intrinsicBlobs,
                    StatLookup = GetBufferLookup<Stat>(true),
                    CommandBuffer = ecb.AsParallelWriter(),
                    NativeStreamWriter = modificationStream.AsWriter()
                }.ScheduleParallel(state.Dependency);

                state.Dependency = new ModifyIntrinsicsJob
                {
                    IntrinsicBlobs = _intrinsicBlobs,
                    IntrinsicBufferLookup = GetBufferLookup<Intrinsic>(),
                    NativeStreamReader = modificationStream.AsReader()
                }.Schedule(state.Dependency);

                updateEventsHandle = new UpdateIntrinsicModificationEventsJob
                {
                    Tick = networkTick.TickIndexForValidTick,
                    TickRate = GetSingleton<ClientServerTickRate>().SimulationTickRate
                }.ScheduleParallel(state.Dependency);
            }

            var processIntrinsicsJob = new ProcessIntrinsicsJob
            {
                IntrinsicBlobs = _intrinsicBlobs,
                DeltaTime = Time.DeltaTime,
                IntrinsicBufferTypeHandle = GetBufferTypeHandle<Intrinsic>(),
                StatBufferTypeHandle = GetBufferTypeHandle<Stat>(true),
                IntrinsicChunkTypeHandle = GetComponentTypeHandle<IntrinsicChunk>(),
                OrderedIntrinsics = _orderedIntrinsics,
                LastSystemVersion = state.LastSystemVersion
            }.ScheduleParallel(
                QueryBuilder().WithAll<Intrinsic, Stat>().WithAllChunkComponent<IntrinsicChunk>().Build(),
                state.Dependency
            );

            state.Dependency = JobHandle.CombineDependencies(processIntrinsicsJob, updateEventsHandle);
        }

        [ConsoleCommand]
        public static void DealDamage(
            EntityManager entityManager,
            Entity entity,
            float value,
            string intrinsic,
            string damageType
        )
        {
            IntrinsicSettings intrinsicSettings = IntrinsicSettings.Get(intrinsic);
            DamageTypeSettings damageTypeSettings = DamageTypeSettings.Get(damageType);

            var buffer = entityManager.GetBuffer<ModifyIntrinsic>(entity);
            buffer.Add(
                new ModifyIntrinsic
                {
                    Delta = -value,
                    Intrinsic = intrinsicSettings.AsRef,
                    DamageType = damageTypeSettings.AsRef,
                    Target = entity
                }
            );
        }
    }
}

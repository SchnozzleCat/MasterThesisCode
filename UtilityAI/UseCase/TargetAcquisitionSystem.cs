using System.Collections.Generic;
using BovineLabs.Core.Spatial;
using Schnozzle.AI.Components;
using Schnozzle.AI.Systems;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Schnozzle.UseCase
{
    [BurstCompile]
    public partial struct SetTargetsJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        [ReadOnly]
        public SpatialMap.ReadOnly SpatialMap;

        [ReadOnly]
        public NativeArray<Entity> Entities;

        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;

        public int MaxTargets;

        public int DetectionRadius;

        public int Size;

        private struct PotentialTarget
        {
            public Entity Target;
            public float Distance;
        }

        [NativeDisableContainerSafetyRestriction]
        private NativeList<PotentialTarget> _list;

        private struct DistanceComparer : IComparer<PotentialTarget>
        {
            public int Compare(PotentialTarget x, PotentialTarget y)
            {
                return x.Distance.CompareTo(y.Distance);
            }
        }

        public unsafe void Execute(Entity e, in LocalToWorld transform, ref DynamicBuffer<AITargetBuffer> targets)
        {
            targets.Clear();
            _list.Clear();

            var min = SpatialMap.Quantized(math.max(transform.Position.xz - DetectionRadius, -Size / 2));
            var max = SpatialMap.Quantized(math.min(transform.Position.xz + DetectionRadius, Size / 2));

            for (int x = min.x; x <= max.x; x++)
                for (int y = min.y; y <= max.y; y++)
                {
                    var hash = SpatialMap.Hash(new int2(x, y));

                    if (!SpatialMap.Map.TryGetFirstValue(hash, out int key, out var iterator))
                        continue;

                    do
                    {
                        var entity = Entities[key];
                        AddTarget(entity, LocalToWorldLookup[entity]);
                    } while (SpatialMap.Map.TryGetNextValue(out key, ref iterator));
                }

            _list.Sort(new DistanceComparer());

            targets.Length = math.min(_list.Length, MaxTargets);

            UnsafeUtility.MemCpyStride(
                targets.GetUnsafePtr(),
                UnsafeUtility.SizeOf<AITargetBuffer>(),
                &_list.GetUnsafePtr()->Target,
                UnsafeUtility.SizeOf<PotentialTarget>(),
                UnsafeUtility.SizeOf<AITargetBuffer>(),
                targets.Length
            );
        }

        private void AddTarget(Entity entity, in LocalToWorld transform)
        {
            _list.Add(
                new PotentialTarget
                {
                    Target = entity,
                    Distance = math.distancesq(transform.Position.xy, LocalToWorldLookup[entity].Position.xy)
                }
            );
        }

        public bool OnChunkBegin(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            if (!_list.IsCreated)
                _list = new NativeList<PotentialTarget>(10, Allocator.Temp);

            return true;
        }

        public void OnChunkEnd(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted
        ) { }
    }

    [UpdateInGroup(typeof(AISystemGroup), OrderFirst = true)]
    public partial struct TargetAcquisitionSystem : ISystem
    {
        private int _entityFieldOffset;

        private PositionBuilder _positionBuilder;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TestAISystem.Singleton>();

            _positionBuilder = new(ref state, QueryBuilder().WithAll<AITargetBuffer>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = _positionBuilder.Gather(ref state, state.Dependency, out var positions);

            var spatialMap = new SpatialMap<SpatialPosition>(
                4,
                GetSingleton<TestAISystem.Singleton>().AreaSize * 2,
                state.WorldUpdateAllocator
            );

            state.Dependency = spatialMap.Build(positions, state.Dependency);

            state.Dependency = new SetTargetsJob
            {
                SpatialMap = spatialMap.AsReadOnly(),
                Entities = QueryBuilder().WithAll<AITargetBuffer>().Build().ToEntityArray(state.WorldUpdateAllocator),
                MaxTargets = GetSingleton<TestAISystem.Singleton>().MaxTargetCount,
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                DetectionRadius = GetSingleton<TestAISystem.Singleton>().DetectionRadius,
                Size = GetSingleton<TestAISystem.Singleton>().AreaSize * 2,
            }.ScheduleParallel(state.Dependency);
        }
    }
}

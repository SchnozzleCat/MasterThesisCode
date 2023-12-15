using Schnozzle.Core.SchnozzleObject;
using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components;
using Spark.Status.Components.Items;
using Spark.Status.Components.Stats;
using Spark.Status.Jobs.Stats;
using Spark.Status.SchnozzleObjects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace Spark.Status.Systems
{
    public static class DynamicBufferExtension
    {
        public static Stat GetStat(
            this DynamicBuffer<Stat> statBuffer,
            SchnozzleReference<StatSettings> stat,
            NativeHashMap<SchnozzleReference<StatSettings>, int> lookup
        )
        {
            return statBuffer[lookup[stat]];
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(StatusSystemGroup))]
    public partial struct StatSystem : ISystem
    {
        public struct InitializedFinalStats : IComponentData { }

        [InjectBlob]
        private SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs _statSettingsBlobs;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IndexerSystem.HasInitializedStatusIndices>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var statSettingsBlobs = _statSettingsBlobs;

            var orderedStats = GetSingleton<IndexerSystem.State>().OrderedStats;

            //Create initial buffer and force recalculation when e.g. starting the game.
            state.Dependency = new InitializeStatCalculationJob
            {
                CommandBuffer = ecb,
                StatCount = orderedStats.Length,
                RequiresStatCalculationLookup = GetComponentLookup<RequireStatCalculation>()
            }.Schedule(state.Dependency);

            state.Dependency = new WriteBaseStatsToFinalStatsJob
            {
                EquipmentBufferHandle = GetBufferTypeHandle<Equipment>(true),
                BaseStatBufferHandle = GetBufferTypeHandle<BaseStat>(true),
                StatBufferHandle = GetBufferTypeHandle<Stat>(),
                LastSystemVersion = state.LastSystemVersion,
                StatSettingsBlobs = statSettingsBlobs
            }.ScheduleParallel(QueryBuilder().WithAll<BaseStat, Stat>().Build(), state.Dependency);

            state.Dependency = new WriteEquipmentToFinalStatsJob
            {
                StatSettingsBlobs = statSettingsBlobs,
                BaseStatBufferLookup = GetBufferLookup<BaseStat>(true)
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new FinalizeStatCalculationJob
            {
                OrderedStats = orderedStats,
                StatSettingsBlobs = statSettingsBlobs,
                RequireStatCalculationLookup = GetComponentLookup<RequireStatCalculation>()
            }.ScheduleParallel(state.Dependency);
        }
    }
}

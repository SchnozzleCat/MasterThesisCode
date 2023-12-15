using Spark.Status.Components;
using Spark.Status.Components.Stats;
using Spark.Status.Systems;
using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;

namespace Spark.Status.Jobs.Stats
{
    [BurstCompile]
    [WithAll(typeof(BaseStat), typeof(RequireStatCalculation))]
    [WithNone(typeof(StatSystem.InitializedFinalStats))]
    internal partial struct InitializeStatCalculationJob : IJobEntity
    {
        public int StatCount;

        public ComponentLookup<RequireStatCalculation> RequiresStatCalculationLookup;

        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, ref DynamicBuffer<Stat> finalStats)
        {
            Assert.IsTrue(finalStats.Length == 0);

            finalStats.ResizeUninitialized(StatCount);
            finalStats.TrimExcess();

            for (int i = 0; i < StatCount; i++)
                finalStats[i] = new Stat { Value = 0 };

            CommandBuffer.AddComponent(entity, new StatSystem.InitializedFinalStats());

            RequiresStatCalculationLookup.SetComponentEnabled(entity, true);
        }
    }
}

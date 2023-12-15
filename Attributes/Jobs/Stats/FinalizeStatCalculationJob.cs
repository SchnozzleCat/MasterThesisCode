using System;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components;
using Spark.Status.Components.Stats;
using Spark.Status.SchnozzleObjects;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Spark.Status.Jobs.Stats
{
    [BurstCompile]
    [WithChangeFilter(typeof(Stat))]
    internal partial struct FinalizeStatCalculationJob : IJobEntity
    {
        [ReadOnly]
        public SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs StatSettingsBlobs;

        [ReadOnly]
        public NativeArray<SchnozzleReference<StatSettings>> OrderedStats;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<RequireStatCalculation> RequireStatCalculationLookup;

        public void Execute(Entity entity, ref DynamicBuffer<Stat> finalStats)
        {
            Assert.IsTrue(finalStats.Length == StatSettingsBlobs.Count);

            for (int i = 0; i < OrderedStats.Length; i++)
            {
                ref var statBlob = ref StatSettingsBlobs[OrderedStats[i]].Value;
                statBlob.RunModifications(ref finalStats, ref StatSettingsBlobs);
            }

            for (int i = 0; i < finalStats.Length; i++)
            {
                var stat = finalStats[i];
                ref var statBlob = ref StatSettingsBlobs[OrderedStats[i]].Value;

                switch (statBlob.MultiplierType)
                {
                    case StatSettings.MultiplierType.Additive:
                        // Do nothing.
                        break;
                    case StatSettings.MultiplierType.Multiplicative:
                        stat.Value = math.clamp(1 - stat.Value, 0, 1);
                        break;
                    default:
                        throw new Exception("Unknown MultiplierType!");
                }

                stat.Value = math.clamp(
                    math.select(stat.Value, math.round(stat.Value), statBlob.RoundToInt),
                    statBlob.MinValue,
                    statBlob.MaxValue
                );

                finalStats[i] = stat;
            }

            RequireStatCalculationLookup.SetComponentEnabled(entity, false);
        }
    }
}

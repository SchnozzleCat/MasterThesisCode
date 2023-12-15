using System;
using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components.Items;
using Spark.Status.Components.Stats;
using Spark.Status.SchnozzleObjects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Spark.Status.Jobs.Stats
{
    [BurstCompile]
    [WithChangeFilter(typeof(Stat))]
    internal partial struct WriteEquipmentToFinalStatsJob : IJobEntity
    {
        [ReadOnly]
        public SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs StatSettingsBlobs;

        [ReadOnly]
        public BufferLookup<BaseStat> BaseStatBufferLookup;

        public void Execute(ref DynamicBuffer<Stat> finalStats, in DynamicBuffer<Equipment> equipment)
        {
            for (int i = 0; i < equipment.Length; i++)
            {
                var equipmentStats = BaseStatBufferLookup[equipment[i].Item];

                for (int j = 0; j < equipmentStats.Length; j++)
                {
                    var equipmentStat = equipmentStats[j];
                    ref var finalStatBlob = ref StatSettingsBlobs[equipmentStat.Stat].Value;
                    var finalStatIndex = finalStatBlob.Index;

                    var currentFinalStat = finalStats[finalStatIndex];

                    switch (finalStatBlob.MultiplierType)
                    {
                        case StatSettings.MultiplierType.Additive:
                            currentFinalStat.Value += equipmentStat.Value;
                            break;
                        case StatSettings.MultiplierType.Multiplicative:
                            currentFinalStat.Value *= (1 - equipmentStat.Value);
                            break;
                        default:
                            throw new Exception("Unknown MultiplierType!");
                    }

                    finalStats[finalStatIndex] = currentFinalStat;
                }
            }
        }
    }
}

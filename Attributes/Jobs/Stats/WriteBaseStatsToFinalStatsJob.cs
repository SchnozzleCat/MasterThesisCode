using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components.Items;
using Spark.Status.Components.Stats;
using Spark.Status.SchnozzleObjects;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Spark.Status.Jobs.Stats
{
    [BurstCompile]
    internal struct WriteBaseStatsToFinalStatsJob : IJobChunk
    {
        [ReadOnly]
        public BufferTypeHandle<Equipment> EquipmentBufferHandle;

        [ReadOnly]
        public BufferTypeHandle<BaseStat> BaseStatBufferHandle;

        [ReadOnly]
        public SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs StatSettingsBlobs;

        public BufferTypeHandle<Stat> StatBufferHandle;

        public uint LastSystemVersion;

        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            Assert.IsFalse(useEnabledMask);

            // Check if either the base stats or the equipment (if it has one) has changed for this chunk.
            if (
                !chunk.DidChange(ref BaseStatBufferHandle, LastSystemVersion)
                && !chunk.DidChange(ref EquipmentBufferHandle, LastSystemVersion)
            )
                return;

            var baseStatAccessors = chunk.GetBufferAccessor(ref BaseStatBufferHandle);
            var statAccessors = chunk.GetBufferAccessor(ref StatBufferHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                var baseStats = baseStatAccessors[i];
                var stats = statAccessors[i];

                HandleEntity(ref stats, in baseStats);
            }
        }

        private void HandleEntity(ref DynamicBuffer<Stat> finalStats, in DynamicBuffer<BaseStat> baseStats)
        {
            Assert.IsTrue(finalStats.Length == StatSettingsBlobs.Count);

            for (int i = 0; i < finalStats.Length; i++)
                finalStats[i] = new Stat { Value = 0 };

            for (int i = 0; i < baseStats.Length; i++)
            {
                var baseStat = baseStats[i];
                var finalStatIndex = StatSettingsBlobs[baseStat.Stat].Value.Index;

                finalStats[finalStatIndex] = new Stat { Value = baseStat.Value };
            }
        }
    }
}

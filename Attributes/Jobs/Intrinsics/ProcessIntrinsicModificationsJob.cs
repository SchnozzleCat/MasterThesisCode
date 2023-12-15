using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components;
using Spark.Status.Components.Stats;
using Spark.Status.SchnozzleObjects;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Spark.Status.Jobs.Intrinsics
{
    [BurstCompile]
    [WithChangeFilter(typeof(ModifyIntrinsic))]
    internal partial struct ProcessIntrinsicModificationsJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        [ReadOnly]
        public SchnozzleObject<IntrinsicSettings, IntrinsicBlob>.Blobs IntrinsicBlobs;

        [ReadOnly]
        public SchnozzleObject<DamageTypeSettings, DamageTypeBlob>.Blobs DamageTypeBlobs;

        [ReadOnly]
        public BufferLookup<Stat> StatLookup;

        public uint Tick;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public UnsafeStream.Writer NativeStreamWriter;

        public bool OnChunkBegin(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            NativeStreamWriter.BeginForEachIndex(unfilteredChunkIndex);
            return true;
        }

        public void OnChunkEnd(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted
        )
        {
            NativeStreamWriter.EndForEachIndex();
        }

        public void Execute(
            ref DynamicBuffer<ModifyIntrinsic> modifications,
            in DynamicBuffer<Stat> stats,
            [ChunkIndexInQuery] int chunkIndexInQuery
        )
        {
            var modificationCount = modifications.Length;

            if (modificationCount == 0)
                return; //Early out if the change filter was triggered for entities that do not actually have any modifications.

            for (int i = 0; i < modificationCount; i++)
            {
                var modification = modifications[i];

                var delta = modification.Delta;

                if (modification.DamageType.IsValid)
                {
                    ref var damageTypeBlob = ref DamageTypeBlobs[modification.DamageType].Value;

                    var damageIncreasingStat = stats.TryGetStat(damageTypeBlob.DamageIncreasingStatIndex);

                    delta += damageIncreasingStat.Value;
                    delta *= 1 + damageIncreasingStat.Value;

                    if (StatLookup.TryGetBuffer(modification.Target, out var targetStats))
                    {
                        var damageReducingStat = targetStats.TryGetStat(damageTypeBlob.DamageReducingStatIndex);

                        //Do 1 - <value> here, as a value of 100% "resistance" means immunity (1 - 1 = 0),
                        //and a value of 200% resistance means heal for 100% of the damage (1 - 2 = -1, inverting the value)
                        delta *= 1 - damageReducingStat.Value;
                        delta -= damageReducingStat.Value;
                    }
                }

                NativeStreamWriter.Write(
                    new ModifyIntrinsicData
                    {
                        Intrinsic = modification.Intrinsic,
                        Target = modification.Target,
                        Value = delta
                    }
                );

                ref var intrinsicBlob = ref IntrinsicBlobs[modification.Intrinsic].Value;

                if (intrinsicBlob.SendEvents)
                {
                    CommandBuffer.AppendToBuffer(
                        chunkIndexInQuery,
                        modification.Target,
                        new ModifyIntrinsicEvent
                        {
                            Delta = (int)delta,
                            Tick = Tick,
                            DamageType = modification.DamageType,
                            Intrinsic = modification.Intrinsic
                        }
                    );
                }
            }

            modifications.Clear();
        }
    }
}

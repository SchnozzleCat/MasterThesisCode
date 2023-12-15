using System.Runtime.CompilerServices;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.ECS.Extensions;
using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components.Intrinsics;
using Spark.Status.Components.Stats;
using Spark.Status.SchnozzleObjects;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Spark.Status.Jobs.Intrinsics
{
    [BurstCompile]
    internal struct ProcessIntrinsicsJob : IJobChunk
    {
        [ReadOnly]
        public SchnozzleObject<IntrinsicSettings, IntrinsicBlob>.Blobs IntrinsicBlobs;

        [ReadOnly]
        public NativeArray<SchnozzleReference<IntrinsicSettings>> OrderedIntrinsics;

        [ReadOnly]
        public BufferTypeHandle<Stat> StatBufferTypeHandle;

        public BufferTypeHandle<Intrinsic> IntrinsicBufferTypeHandle;
        public ComponentTypeHandle<IntrinsicChunk> IntrinsicChunkTypeHandle;

        public float DeltaTime;

        public uint LastSystemVersion;

        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            Assert.IsTrue(!useEnabledMask);

            //Check if the intrinsic has changed, or if the stats has changed (maybe regeneration values have gone from e.g. 0 to 1).
            var hasChanged =
                chunk.DidChange(ref IntrinsicBufferTypeHandle, LastSystemVersion)
                | chunk.DidChange(ref StatBufferTypeHandle, LastSystemVersion)
                | chunk.DidOrderChange(LastSystemVersion);

            var chunkComponent = chunk.GetChunkComponentData(ref IntrinsicChunkTypeHandle);

            if (!hasChanged && !chunkComponent.RequiresUpdate)
                return;

            var statBuffers = chunk.GetBufferAccessor(ref StatBufferTypeHandle);
            var intrinsicBuffers = chunk.GetBufferAccessor(ref IntrinsicBufferTypeHandle);

            chunkComponent.IsRegenerating = false;

            for (int i = 0; i < chunk.Count; i++)
            {
                var stats = statBuffers[i];
                var intrinsics = intrinsicBuffers[i];

                HandleEntity(ref intrinsics, in stats, ref chunkComponent);
            }

            chunk.SetChunkComponentData(ref IntrinsicChunkTypeHandle, chunkComponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleEntity(
            ref DynamicBuffer<Intrinsic> intrinsics,
            in DynamicBuffer<Stat> stats,
            ref IntrinsicChunk chunkComponent
        )
        {
            var length = intrinsics.Length;

            if (length == 0)
                InitializeIntrinsics(ref intrinsics);

            for (int i = 0; i < length; i++)
            {
                ref var intrinsicBlob = ref IntrinsicBlobs[OrderedIntrinsics[i]].Value;

                ref var intrinsic = ref intrinsics.GetAsRef(i);

                var maximumStatValue = stats.TryGetStat(intrinsicBlob.MaximumValueStatIndex).Value;

                if (intrinsic.Value < maximumStatValue)
                {
                    var regenerationPerSecond = stats.TryGetStat(intrinsicBlob.RegenerationPerSecondStatIndex).Value;

                    if (regenerationPerSecond <= 0)
                        continue;

                    chunkComponent.IsRegenerating = true;

                    //Get regeneration delay from stats if applicable.
                    var regenerationDelay = stats.TryGetStat(intrinsicBlob.RegenerationDelayStatIndex).Value;
                    var regenerationMultiplier = math.select(
                        0,
                        1,
                        intrinsic.RegenerationDelayTimer >= regenerationDelay
                    );
                    //Increase regeneration timer.
                    intrinsic.RegenerationDelayTimer += DeltaTime;

                    intrinsic.Value = math.clamp(
                        intrinsic.Value + regenerationPerSecond * DeltaTime * regenerationMultiplier,
                        float.MinValue,
                        maximumStatValue
                    );
                }
                else
                    intrinsic.Value = maximumStatValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeIntrinsics(ref DynamicBuffer<Intrinsic> intrinsics)
        {
            var intrinsicCount = OrderedIntrinsics.Length;

            intrinsics.ResizeUninitialized(intrinsicCount);

            for (int i = 0; i < intrinsicCount; i++)
                intrinsics[i] = new Intrinsic { Value = float.MaxValue };

            intrinsics.TrimExcess();
        }
    }
}

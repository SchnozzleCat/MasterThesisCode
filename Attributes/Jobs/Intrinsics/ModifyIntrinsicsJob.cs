using Schnozzle.Core.SchnozzleObject;
using Schnozzle.ECS.Extensions;
using Schnozzle.ECS.SchnozzleObject;
using Spark.Status.Components.Intrinsics;
using Spark.Status.SchnozzleObjects;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Spark.Status.Jobs.Intrinsics
{
    public struct ModifyIntrinsicData
    {
        public Entity Target;
        public float Value;
        public SchnozzleReference<IntrinsicSettings> Intrinsic;
    }

    [BurstCompile]
    public struct ModifyIntrinsicsJob : IJob
    {
        [ReadOnly]
        public SchnozzleObject<IntrinsicSettings, IntrinsicBlob>.Blobs IntrinsicBlobs;

        public BufferLookup<Intrinsic> IntrinsicBufferLookup;

        public UnsafeStream.Reader NativeStreamReader;

        public void Execute()
        {
            for (int bufferIndex = 0; bufferIndex < NativeStreamReader.ForEachCount; bufferIndex++)
            {
                var count = NativeStreamReader.BeginForEachIndex(bufferIndex);

                for (int i = 0; i < count; i++)
                {
                    var data = NativeStreamReader.Read<ModifyIntrinsicData>();

                    var targetBuffer = IntrinsicBufferLookup[data.Target];

                    ref var intrinsicBlob = ref IntrinsicBlobs[data.Intrinsic].Value;
                    ref var value = ref targetBuffer.GetAsRef(intrinsicBlob.Index);
                    value.Value += data.Value;
                    //If the target did not take any damage, do not reset the regeneration timer.
                    value.RegenerationDelayTimer = math.select(0, value.RegenerationDelayTimer, data.Value >= 0);
                }

                NativeStreamReader.EndForEachIndex();
            }
        }
    }
}

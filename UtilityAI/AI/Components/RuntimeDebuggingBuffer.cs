using System;
using Schnozzle.AI.Data;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Schnozzle.AI.Components
{
#if UNITY_EDITOR
    [ChunkSerializable]
    public unsafe struct RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData> : IBufferElementData
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public enum MessageType : byte
        {
            DecisionSet,
            Decision,
            Consideration,
            Score
        }

        public RuntimeDebuggingBuffer(ref DecisionSetBlob<TExecutionContext, TEnum, TCustomData> decisionSet)
        {
            Type = MessageType.DecisionSet;
            Ptr = UnsafeUtility.AddressOf(ref decisionSet);
            Entity = default;
            Score = default;
        }

        public RuntimeDebuggingBuffer(ref DecisionBlob<TExecutionContext, TEnum, TCustomData> decision, Entity target)
        {
            Type = MessageType.Decision;
            Ptr = UnsafeUtility.AddressOf(ref decision);
            Score = default;
            Entity = target;
        }

        public RuntimeDebuggingBuffer(ref ConsiderationBlob<TExecutionContext, TEnum> consideration, float score)
        {
            Type = MessageType.Consideration;
            Ptr = UnsafeUtility.AddressOf(ref consideration);
            Entity = default;
            Score = score;
        }

        public RuntimeDebuggingBuffer(float score)
        {
            Type = MessageType.Score;
            Ptr = null;
            Entity = default;
            Score = score;
        }

        public int Depth =>
            Type switch
            {
                MessageType.DecisionSet => 1,
                MessageType.Decision => 1,
                MessageType.Consideration => 0,
                MessageType.Score => -1,
                _ => throw new NotImplementedException()
            };

        public string Message =>
            Type switch
            {
                MessageType.DecisionSet
                    => $"{UnsafeUtility.AsRef<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>>(Ptr).Name}",
                MessageType.Decision
                    => $"{UnsafeUtility.AsRef<DecisionBlob<TExecutionContext, TEnum, TCustomData>>(Ptr).Name} - {Entity}",
                MessageType.Consideration
                    => $"{UnsafeUtility.AsRef<ConsiderationBlob<TExecutionContext, TEnum>>(Ptr).Name} - {Score}",
                MessageType.Score => Score.ToString(),
                _ => throw new NotImplementedException()
            };

        public MessageType Type;

        [NativeDisableUnsafePtrRestriction]
        public void* Ptr;

        public float Score;

        public Entity Entity;
    }
#endif
}

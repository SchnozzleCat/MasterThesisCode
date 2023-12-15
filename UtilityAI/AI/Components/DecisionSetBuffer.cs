using System;
using Schnozzle.AI.Data;
using Unity.Entities;

namespace Schnozzle.AI.Components
{
    public interface IDecisionSetBuffer<TExecutionContext, TEnum, TCustomData> : IBufferElementData
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public BlobAssetReference<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>> Set { get; }
    }

    [InternalBufferCapacity(1)]
    public struct DecisionSetBuffer<TExecutionContext, TEnum, TCustomData>
        : IDecisionSetBuffer<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public BlobAssetReference<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>> Value;

        public BlobAssetReference<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>> Set => Value;
    }
}

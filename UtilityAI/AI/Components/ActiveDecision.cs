using System;
using Schnozzle.AI.Data;
using Unity.Entities;

namespace Schnozzle.AI.Components
{
    public interface IActiveDecision<TExecutionContext, TEnum, TCustomData> : IComponentData
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public DecisionReference<TExecutionContext, TEnum, TCustomData> Value { get; set; }
        public Entity? Target { get; set; }
    }

    [ChunkSerializable]
    public struct ActiveDecision<TExecutionContext, TEnum, TCustomData>
        : IActiveDecision<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public DecisionReference<TExecutionContext, TEnum, TCustomData> Value { get; set; }
        public Entity? Target { get; set; }
    }
}

using System;
using System.Diagnostics.Contracts;
using Unity.Entities;

namespace Schnozzle.AI.Data
{
    public interface IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
    {
        public void OnCreate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }
        public void OnUpdate(ref SystemState state) { }

        public void OnChunkBegin(in ArchetypeChunk chunk) { }
        public void OnIterateEntity(int index) { }

        [Pure]
        public float GetInputValue(ref InputHeader<TEnum> header);

        public EntityQuery ScheduleQuery { get; }

        public Entity? Target { get; set; }
    }
}

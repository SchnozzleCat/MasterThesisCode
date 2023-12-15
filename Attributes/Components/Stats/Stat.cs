using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components.Stats
{
    [InternalBufferCapacity(0)]
    public struct Stat : IBufferElementData
    {
        [GhostField]
        public float Value;
    }

    public static class StatExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stat TryGetStat(this DynamicBuffer<Stat> buffer, int index) =>
            index >= 0 ? buffer[index] : default;
    }
}

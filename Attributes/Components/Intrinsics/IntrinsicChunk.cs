using Unity.Entities;

namespace Spark.Status.Components.Intrinsics
{
    public struct IntrinsicChunk : IComponentData
    {
        public bool IsRegenerating;

        public bool RequiresUpdate => IsRegenerating;
    }
}

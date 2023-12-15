using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components.Intrinsics
{
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    [InternalBufferCapacity(0)]
    public struct Intrinsic : IBufferElementData
    {
        [GhostField]
        public float Value;

        [GhostField]
        public float RegenerationDelayTimer;
    }
}

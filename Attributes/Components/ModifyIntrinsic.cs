using Schnozzle.Core.SchnozzleObject;
using Spark.Status.SchnozzleObjects;
using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components
{
    [InternalBufferCapacity(0)]
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
    public struct ModifyIntrinsic : IBufferElementData
    {
        public SchnozzleReference<IntrinsicSettings> Intrinsic;
        public SchnozzleReference<DamageTypeSettings> DamageType;
        public float Delta;
        public Entity Target;
    }
}

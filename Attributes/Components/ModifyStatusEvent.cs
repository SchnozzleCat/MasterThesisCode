using Schnozzle.Core.SchnozzleObject;
using Spark.Status.SchnozzleObjects;
using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components
{
    [InternalBufferCapacity(0)]
    public struct ModifyIntrinsicEvent : IBufferElementData
    {
        [GhostField]
        public SchnozzleReference<IntrinsicSettings> Intrinsic;

        [GhostField]
        public SchnozzleReference<DamageTypeSettings> DamageType;

        [GhostField]
        public int Delta;

        [GhostField]
        public uint Tick;

        public bool HasInitialized => Tick > 0;
    }
}

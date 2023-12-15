using Schnozzle.Core.SchnozzleObject;
using Spark.Status.SchnozzleObjects;
using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components.Stats
{
    [InternalBufferCapacity(0)]
    public struct BaseStat : IBufferElementData
    {
        [GhostField]
        public SchnozzleReference<StatSettings> Stat;

        [GhostField]
        public float Value;
    }
}

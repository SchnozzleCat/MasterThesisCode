using Unity.Entities;
using UnityEngine.Serialization;

namespace Schnozzle.GameSpace.Components
{
    [InternalBufferCapacity(0)]
    public struct SpatialConnection : IBufferElementData
    {
        public Entity Entity;
        public float Cost;
    }
}

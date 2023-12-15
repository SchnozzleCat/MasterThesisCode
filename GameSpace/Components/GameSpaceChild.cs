using System;
using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    [InternalBufferCapacity(0)]
    public struct GameSpaceChild : IBufferElementData, IEquatable<GameSpaceChild>
    {
        public Entity Entity;

        public bool Equals(GameSpaceChild other)
        {
            return Entity.Equals(other.Entity);
        }

        public override bool Equals(object obj)
        {
            return obj is GameSpaceChild other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode();
        }
    }
}

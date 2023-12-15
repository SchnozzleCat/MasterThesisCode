using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    public struct Traveling : IComponentData
    {
        public static Traveling Uninitialized => new() { Progress = 0 };

        public ushort PathNodeIndex;
        public float DistanceToNextNode;
        public float Progress;
    }
}

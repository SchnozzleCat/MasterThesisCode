using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    public struct GameSpaceNode : IComponentData
    {
        public ushort Depth;
    }
}

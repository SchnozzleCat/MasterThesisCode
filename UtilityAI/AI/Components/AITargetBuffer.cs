using Unity.Entities;

namespace Schnozzle.AI.Components
{
    public struct AITargetBuffer : IBufferElementData
    {
        public Entity Value;
    }
}

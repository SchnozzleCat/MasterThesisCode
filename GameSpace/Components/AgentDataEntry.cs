using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    /// <summary>
    /// Optional agent data buffer for games that require higher degrees of consistency for specific actors.
    /// </summary>
    public struct AgentDataEntry : IBufferElementData
    {
        public Entity Entity;
    }
}

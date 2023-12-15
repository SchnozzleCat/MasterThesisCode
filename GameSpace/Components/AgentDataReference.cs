using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    /// <summary>
    /// A reference from an atomic actor to its agent data.
    /// This is used in games that require stricter consistency so that
    /// atomic actors can base their fine grained simulation off of the
    /// persistent data that is stored.
    /// </summary>
    public struct AgentDataReference : IComponentData
    {
        public Entity Value;
    }
}

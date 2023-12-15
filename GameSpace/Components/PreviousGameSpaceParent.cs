using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    /// <summary>
    /// The previous game space parent. This is used to detect when the parent has changed and update the new and old parent's child buffers.
    /// </summary>
    public struct PreviousGameSpaceParent : ICleanupComponentData
    {
        public Entity Entity;
    }
}

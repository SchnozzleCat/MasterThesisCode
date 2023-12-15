using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    /// <summary>
    /// Marks an entity as being able to collapse other game spaces.
    /// </summary>
    public struct Collapser : IComponentData
    {
        // The distance to neighbouring nodes to collapse.
        public int CollapseDistance;
    }
}

using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components.Items
{
    [GhostEnabledBit]
    public struct EquippedToEntity : IComponentData, IEnableableComponent
    {
        [GhostField]
        public Entity Target;
    }
}

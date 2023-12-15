using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Components.Singleton;
using Unity.Entities;

namespace Schnozzle.GameSpace.Systems
{
    [UpdateAfter(typeof(SpatialLocatorSystem))]
    public partial class CollapseSystemGroup : ComponentSystemGroup { }
}

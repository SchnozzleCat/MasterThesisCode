using Unity.Entities;

namespace Spark.Status.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class StatusSystemGroup : ComponentSystemGroup { }
}

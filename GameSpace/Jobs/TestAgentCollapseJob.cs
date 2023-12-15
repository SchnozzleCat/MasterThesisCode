using Schnozzle.GameSpace.Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Schnozzle.GameSpace.Jobs
{
    [WithAll(typeof(SimulatedAgent), typeof(Collapsing))]
    [WithNone(typeof(CollapsedAgent))]
    public partial struct TestAgentCollapseJob : IJobEntity
    {
        public void Execute(Entity entity, in LocalToWorld pos)
        {
            Debug.Log($"Found collapsing entity {entity}");
        }
    }

    [WithAll(typeof(SimulatedAgent), typeof(Decollapsing))]
    public partial struct TestAgentDecollapseJob : IJobEntity
    {
        public void Execute(Entity entity, in LocalToWorld pos)
        {
            Debug.Log($"Found decollapsing entity {entity}");
        }
    }
}

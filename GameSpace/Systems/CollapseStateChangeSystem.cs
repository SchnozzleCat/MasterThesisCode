using Schnozzle.GameSpace.Components;
using Unity.Burst;
using Unity.Entities;

namespace Schnozzle.GameSpace.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct CollapseStateChangeSystem : ISystem
    {
        internal struct CancelCollapse : IComponentData { }

        /// <summary>
        /// Handle cancellation requests from collapsing agents.
        /// </summary>
        /// <param name="state"></param>
        private void HandleCancelledCollapseAgents(ref SystemState state)
        {
            SystemAPI.QueryBuilder().WithAllRW<CollapseSchedulerSystem.Singleton>().Build().CompleteDependency();
            ref var singleton = ref SystemAPI.GetSingletonRW<CollapseSchedulerSystem.Singleton>().ValueRW;

            state.EntityManager.AddComponent<CancelCollapse>(singleton.CancelCollapseList);

            singleton.CancelCollapseList.Clear();
        }

        /// <summary>
        /// Handle state changes for collapsed agents and game space nodes.
        /// </summary>
        /// <param name="state"></param>
        private void HandleStateChanges(ref SystemState state)
        {
            var em = state.EntityManager;

            em.AddComponent<CollapsedSpace>(SystemAPI.QueryBuilder().WithAll<Collapsing, GameSpaceNode>().Build());
            em.RemoveComponent<CollapsedSpace>(SystemAPI.QueryBuilder().WithAll<Decollapsing, GameSpaceNode>().Build());

            // Find entities at playback so that collapsing / de-collapsing can be canceled.
            em.AddComponent<CollapsedAgent>(
                SystemAPI.QueryBuilder().WithAll<Collapsing, SimulatedAgent>().WithNone<CancelCollapse>().Build()
            );
            em.RemoveComponent<CollapsedAgent>(
                SystemAPI.QueryBuilder().WithAll<Decollapsing, SimulatedAgent>().WithNone<CancelCollapse>().Build()
            );

            // Remove all collapsing / decollapsing components even if they are disabled.
            em.RemoveComponent<Collapsing>(SystemAPI.QueryBuilder().WithAll<Collapsing>().Build());
            em.RemoveComponent<Decollapsing>(SystemAPI.QueryBuilder().WithAll<Decollapsing>().Build());
            em.RemoveComponent<CancelCollapse>(SystemAPI.QueryBuilder().WithAll<CancelCollapse>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            HandleCancelledCollapseAgents(ref state);
            HandleStateChanges(ref state);
        }
    }
}

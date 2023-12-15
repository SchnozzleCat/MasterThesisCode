using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Jobs;
using Unity.Collections;
using Unity.Entities;

namespace Schnozzle.GameSpace.Systems
{
    [UpdateBefore(typeof(SpatialLocatorSystem))]
    internal partial struct AgentHierarchySystem : ISystem
    {
        private EntityQuery _changeParentQuery;
        private EntityQuery _addPreviousQuery;
        private EntityQuery _cleanupQuery;

        public void OnCreate(ref SystemState state)
        {
            _changeParentQuery = SystemAPI
                .QueryBuilder()
                .WithAll<GameSpaceParent>()
                .WithNone<Atomic, GameSpaceNode>()
                .Build();
            _changeParentQuery.SetChangedVersionFilter(ComponentType.ReadWrite<GameSpaceParent>());

            _addPreviousQuery = SystemAPI
                .QueryBuilder()
                .WithAll<GameSpaceParent>()
                .WithNone<PreviousGameSpaceParent, GameSpaceNode>()
                .Build();
            _cleanupQuery = SystemAPI
                .QueryBuilder()
                .WithAll<PreviousGameSpaceParent>()
                .WithNone<Simulate, GameSpaceNode>()
                .Build();

            state.RequireAnyForUpdate(
                new NativeArray<EntityQuery>(3, Allocator.Temp)
                {
                    [0] = _changeParentQuery,
                    [1] = _addPreviousQuery,
                    [2] = _cleanupQuery
                }
            );
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            ecb.AddComponent<PreviousGameSpaceParent>(_addPreviousQuery);
            ecb.RemoveComponent<PreviousGameSpaceParent>(_cleanupQuery);

            state.Dependency = new UpdateGameSpaceParentJob
            {
                ChildLookup = SystemAPI.GetBufferLookup<GameSpaceChild>()
            }.Schedule(state.Dependency);

            state.Dependency = new CleanupGameSpaceParentJob
            {
                ChildLookup = SystemAPI.GetBufferLookup<GameSpaceChild>()
            }.Schedule(state.Dependency);

            state.Dependency = new CheckCollapseOnParentChangeJob
            {
                CommandBuffer = ecb,
                CollapsedSpaceLookup = SystemAPI.GetComponentLookup<CollapsedSpace>(true),
                EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
                GameSpaceParentHandle = SystemAPI.GetComponentTypeHandle<GameSpaceParent>(true)
            }.Schedule(_changeParentQuery, state.Dependency);
        }
    }
}

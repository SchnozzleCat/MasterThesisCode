using GameSpace.Systems;
using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Schnozzle.GameSpace.Systems
{
    [UpdateInGroup(typeof(AgentSimulationSystemGroup), OrderLast = true)]
    public partial struct PathfindingSystem<T> : ISystem
        where T : unmanaged, IPathfindingProcessor
    {
        private T _processor;

        private EntityQuery _pathfindingQuery;
        private EntityQuery _moverQuery;
        
        
        

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndAgentSimulationEntityCommandBufferSystem.Singleton>();
            _pathfindingQuery = QueryBuilder().WithAll<Pathfinder, Path>().Build();
            _pathfindingQuery.SetChangedVersionFilter(ComponentType.ReadWrite<Pathfinder>());

            _moverQuery = QueryBuilder().WithAll<TravelSpeed, Traveling>().Build();

            state.RequireForUpdate(_pathfindingQuery);

            _processor.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _processor.OnDestroy(ref state);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _processor.OnUpdate(ref state);

            var pathfinderCommandBuffer = GetSingleton<EndAgentSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new PathfindingJob<T>
            {
                SpatialConnectionLookup = GetBufferLookup<SpatialConnection>(true),
                EntityHandle = GetEntityTypeHandle(),
                PathfinderHandle = GetComponentTypeHandle<Pathfinder>(),
                PathBufferHandle = GetBufferTypeHandle<Path>(),
                Processor = _processor,
                CommandBuffer = pathfinderCommandBuffer.AsParallelWriter()
            }.ScheduleParallel(_pathfindingQuery, state.Dependency);

            if (!_moverQuery.IsEmptyIgnoreFilter)
            {
                var moverCommandBuffer = GetSingleton<EndAgentSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);

                state.Dependency = new MoverJob
                {
                    GameSpaceParentLookup = GetComponentLookup<GameSpaceParent>(),
                    CommandBuffer = moverCommandBuffer.AsParallelWriter(),
                    TransformLookup = GetComponentLookup<LocalTransform>(true)
                }.ScheduleParallel(state.Dependency);
            }
        }
    }
}

using System;
using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Jobs;
using Unity.Collections;
using Unity.Entities;

namespace Schnozzle.GameSpace.Systems
{
    public struct CollapseCanceler
    {
        internal NativeList<Entity>.ParallelWriter _list;

        public void CancelCollapse(Entity entity) => _list.AddNoResize(entity);
    }

    [UpdateInGroup(typeof(CollapseSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    public partial struct CollapseSchedulerSystem : ISystem
    {
        public struct Singleton : IComponentData
        {
            internal NativeList<Entity> CancelCollapseList;

            public CollapseCanceler Canceler => new() { _list = CancelCollapseList.AsParallelWriter() };

            internal void Dispose()
            {
                CancelCollapseList.Dispose();
            }
        }

        private EntityQuery _collapseScheduleQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            _collapseScheduleQuery = SystemAPI
                .QueryBuilder()
                .WithAll<Collapser, GameSpaceParent>()
                .WithNone<OutOfBounds>()
                .Build();

            state.EntityManager.CreateSingleton(
                new Singleton { CancelCollapseList = new NativeList<Entity>(1, Allocator.Persistent) }
            );
        }

        public void OnDestroy(ref SystemState state)
        {
            SystemAPI.GetSingleton<Singleton>().Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            ref var singleton = ref SystemAPI.GetSingletonRW<Singleton>().ValueRW;

            var requiredLength =
                SystemAPI
                    .QueryBuilder()
                    .WithAny<Collapsing, Decollapsing>()
                    .Build()
                    .CalculateChunkCountWithoutFiltering() * 128;

            if (singleton.CancelCollapseList.Capacity < requiredLength)
                singleton.CancelCollapseList.Capacity = requiredLength;

            var collapseSpaceEcb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var uncollapseSpaceEcb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var collapseAgentsEcb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var uncollapseAgentsEcb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new CollapseSpaceJob
            {
                GameSpaceParentHandle = SystemAPI.GetComponentTypeHandle<GameSpaceParent>(true),
                CommandBuffer = collapseSpaceEcb.AsParallelWriter(),
                CollapsedSpaceLookup = SystemAPI.GetComponentLookup<CollapsedSpace>(true),
                CollapserHandle = SystemAPI.GetComponentTypeHandle<Collapser>(true),
                SpatialConnectionLookup = SystemAPI.GetBufferLookup<SpatialConnection>(true)
            }.ScheduleParallel(_collapseScheduleQuery, state.Dependency);

            state.Dependency = new UncollapseSpaceJob
            {
                CommandBuffer = uncollapseSpaceEcb.AsParallelWriter(),
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new CollapseAgentsJob
            {
                CommandBuffer = collapseAgentsEcb.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new UncollapseAgentsJob
            {
                CommandBuffer = uncollapseAgentsEcb.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new TestAgentCollapseJob().ScheduleParallel(state.Dependency);

            state.Dependency = new TestAgentDecollapseJob().ScheduleParallel(state.Dependency);
        }
    }
}

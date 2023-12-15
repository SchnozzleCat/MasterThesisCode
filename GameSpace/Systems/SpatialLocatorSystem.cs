using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Components.Singleton;
using Schnozzle.GameSpace.Jobs;
using Schnozzle.GameSpace.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Schnozzle.GameSpace.Systems
{
    [BurstCompile]
    internal partial struct SpatialLocatorSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpatialConfigurationSingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            if (GetSingleton<SpatialConfigurationSingleton>().DisableAutoLocation)
                state.Enabled = false;
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            if (GetSingleton<SpatialConfigurationSingleton>().DisableAutoLocation)
                state.Enabled = false;

            var collisionWorld = GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var ecb = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new SpatialLocatorJob
            {
                CollisionWorld = collisionWorld,
                Filter = new CollisionFilter
                {
                    BelongsTo = CollisionLayers.GameSpaceNode,
                    CollidesWith = CollisionLayers.GameSpaceNode
                },
                NodeLookup = GetComponentLookup<GameSpaceNode>(true),
                EntityTypeHandle = GetEntityTypeHandle(),
                LocalTransformHandle = GetComponentTypeHandle<LocalTransform>(true),
                GameSpaceParentHandle = GetComponentTypeHandle<GameSpaceParent>(),
                CommandBuffer = ecb.AsParallelWriter(),
                ColliderDistance = 0.1f
            }.ScheduleParallel(QueryBuilder().WithAll<Atomic, LocalTransform>().Build(), state.Dependency);
        }
    }
}

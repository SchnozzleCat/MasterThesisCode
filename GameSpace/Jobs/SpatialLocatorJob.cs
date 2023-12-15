using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Utilities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Schnozzle.GameSpace.Jobs
{
    [BurstCompile]
    internal struct SpatialLocatorJob : IJobChunk
    {
        [ReadOnly]
        public CollisionWorld CollisionWorld;

        [ReadOnly]
        public ComponentLookup<GameSpaceNode> NodeLookup;

        [ReadOnly]
        public ComponentTypeHandle<LocalTransform> LocalTransformHandle;

        [ReadOnly]
        public EntityTypeHandle EntityTypeHandle;

        public ComponentTypeHandle<GameSpaceParent> GameSpaceParentHandle;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public CollisionFilter Filter;

        public float ColliderDistance;

        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            var localTransforms = chunk.GetNativeArray(ref LocalTransformHandle);
            // TODO: Change this so it does not trigger a version change each time. We want to update some systems with a change filter on GameSpaceParent.
            var gameSpaceParents = chunk.GetNativeArray(ref GameSpaceParentHandle);
            var entities = chunk.GetNativeArray(EntityTypeHandle);

            if (!chunk.Has<OutOfBounds>())
                ExecuteInBounds(
                    chunk.Count,
                    ref entities,
                    ref localTransforms,
                    ref gameSpaceParents,
                    unfilteredChunkIndex
                );
            else
                ExecuteOutOfBounds(
                    chunk.Count,
                    ref entities,
                    ref localTransforms,
                    ref gameSpaceParents,
                    unfilteredChunkIndex
                );
        }

        private void ExecuteInBounds(
            int count,
            ref NativeArray<Entity> entities,
            ref NativeArray<LocalTransform> localTransforms,
            ref NativeArray<GameSpaceParent> gameSpaceParents,
            int unfilteredChunkIndex
        )
        {
            var distance = new float3(ColliderDistance, ColliderDistance, ColliderDistance);
            for (var i = 0; i < count; i++)
            {
                var worldPosition = localTransforms[i];
                var entity = entities[i];

                var collector = new GameSpaceNodeCollector(ref NodeLookup);

                var pos = worldPosition.Position;
                if (
                    !CollisionWorld.OverlapBoxCustom(
                        pos,
                        quaternion.identity,
                        distance,
                        ref collector,
                        Filter
                    )
                )
                {
                    CommandBuffer.AddComponent<OutOfBounds>(unfilteredChunkIndex, entity); // Add out of bounds component.
                    continue;
                }

                gameSpaceParents[i] = new GameSpaceParent { Entity = collector.BestEntity };
            }
        }

        private void ExecuteOutOfBounds(
            int count,
            ref NativeArray<Entity> entities,
            ref NativeArray<LocalTransform> localTransforms,
            ref NativeArray<GameSpaceParent> gameSpaceParents,
            int unfilteredChunkIndex
        )
        {
            var distance = new float3(ColliderDistance, ColliderDistance, ColliderDistance);
            for (var i = 0; i < count; i++)
            {
                var worldPosition = localTransforms[i];
                var entity = entities[i];

                var collector = new GameSpaceNodeCollector(ref NodeLookup);

                var pos = worldPosition.Position;

                if (
                    !CollisionWorld.OverlapBoxCustom(
                        pos,
                        quaternion.identity,
                        distance,
                        ref collector,
                        Filter
                    )
                )
                    continue;

                gameSpaceParents[i] = new GameSpaceParent { Entity = collector.BestEntity };
                CommandBuffer.RemoveComponent<OutOfBounds>(unfilteredChunkIndex, entity); // Remove out of bounds component.
            }
        }
    }
}

using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Systems;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.Assertions;

namespace Schnozzle.GameSpace.Jobs
{
    [UpdateInGroup(typeof(CollapseSystemGroup), OrderFirst = true)]
    internal struct CollapseSpaceJob : IJobChunk
    {
        [ReadOnly]
        public ComponentTypeHandle<GameSpaceParent> GameSpaceParentHandle;

        [ReadOnly]
        public ComponentTypeHandle<Collapser> CollapserHandle;

        [ReadOnly]
        public ComponentLookup<CollapsedSpace> CollapsedSpaceLookup;

        [ReadOnly]
        public BufferLookup<SpatialConnection> SpatialConnectionLookup;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [NativeDisableContainerSafetyRestriction]
        private NativeHashSet<Entity> _visited;

        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            Assert.IsFalse(useEnabledMask);

            if (!_visited.IsCreated)
                _visited = new NativeHashSet<Entity>(5, Allocator.Temp);

            var gameSpaceParents = chunk.GetNativeArray(ref GameSpaceParentHandle);

            var collapsers = chunk.GetNativeArray(ref CollapserHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                _visited.Clear();
                GameSpaceParent parent = gameSpaceParents[i];
                Collapser collapser = collapsers[i];

                BasicCollapse(parent.Entity, 0, collapser.CollapseDistance, unfilteredChunkIndex);
            }
        }

        /// <summary>
        /// This method will just collapse the entity that the
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="currentDepth"></param>
        /// <param name="maxDepth"></param>
        /// <param name="unfilteredChunkIndex"></param>
        private void BasicCollapse(in Entity entity, int currentDepth, int maxDepth, int unfilteredChunkIndex)
        {
            if (entity == Entity.Null)
                return;

            _visited.Add(entity);

            if (!CollapsedSpaceLookup.HasComponent(entity)) // Collapse if not already collapsed.
                CommandBuffer.AddComponent<Collapsing>(unfilteredChunkIndex, entity);
            else // Reset timer to 0.
                CommandBuffer.SetComponent<CollapsedSpace>(unfilteredChunkIndex, entity, default);

            if (currentDepth < maxDepth)
            {
                var connections = SpatialConnectionLookup[entity];
                foreach (var connection in connections)
                {
                    if (_visited.Contains(connection.Entity))
                        continue;
                    BasicCollapse(connection.Entity, currentDepth + 1, maxDepth, unfilteredChunkIndex);
                }
            }
        }
    }
}

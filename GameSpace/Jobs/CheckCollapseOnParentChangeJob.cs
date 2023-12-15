using Schnozzle.GameSpace.Components;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Schnozzle.GameSpace.Jobs
{
    public partial struct CheckCollapseOnParentChangeJob : IJobChunk
    {
        [ReadOnly]
        public ComponentLookup<CollapsedSpace> CollapsedSpaceLookup;

        public EntityTypeHandle EntityTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<GameSpaceParent> GameSpaceParentHandle;

        public EntityCommandBuffer CommandBuffer;

        public void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var parents = chunk.GetNativeArray(ref GameSpaceParentHandle);

            if (chunk.Has<CollapsedAgent>())
                ExecutePotentialDecollapse(in chunk, in entities, in parents);
            else
                ExecutePotentialCollapse(in chunk, in entities, in parents);
        }

        private void ExecutePotentialCollapse(
            in ArchetypeChunk chunk,
            in NativeArray<Entity> entities,
            in NativeArray<GameSpaceParent> parents
        )
        {
            for (int i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
                var parent = parents[i];

                if (CollapsedSpaceLookup.HasComponent(parent.Entity))
                    CommandBuffer.AddComponent<Collapsing>(entity);
            }
        }

        private void ExecutePotentialDecollapse(
            in ArchetypeChunk chunk,
            in NativeArray<Entity> entities,
            in NativeArray<GameSpaceParent> parents
        )
        {
            for (int i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
                var parent = parents[i];

                if (!CollapsedSpaceLookup.HasComponent(parent.Entity))
                    CommandBuffer.AddComponent<Decollapsing>(entity);
            }
        }
    }
}

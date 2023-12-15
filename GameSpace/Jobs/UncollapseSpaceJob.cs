using Schnozzle.GameSpace.Components;
using Unity.Entities;

namespace Schnozzle.GameSpace.Jobs
{
    internal partial struct UncollapseSpaceJob : IJobEntity
    {
        public const float UncollapseDuration = 1;

        public float DeltaTime;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(
            Entity entity,
            ref CollapsedSpace collapsed,
            [ChunkIndexInQuery] int chunkIndexInQuery
        )
        {
            collapsed.Timer += DeltaTime;
            if (collapsed.Timer >= UncollapseDuration)
            {
                CommandBuffer.AddComponent<Decollapsing>(chunkIndexInQuery, entity);
            }
        }
    }
}

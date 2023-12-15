using Schnozzle.GameSpace.Components;
using Unity.Entities;

namespace Schnozzle.GameSpace.Jobs
{
    [WithAll(typeof(Decollapsing), typeof(CollapsedSpace))]
    public partial struct UncollapseAgentsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(
            in DynamicBuffer<GameSpaceChild> children,
            [ChunkIndexInQuery] int chunkIndexInQuery
        )
        {
            foreach (var child in children.Reinterpret<Entity>())
                CommandBuffer.AddComponent<Decollapsing>(chunkIndexInQuery, child);
        }
    }
}

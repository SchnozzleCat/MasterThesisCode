using Schnozzle.GameSpace.Components;
using Unity.Entities;

namespace Schnozzle.GameSpace.Jobs
{
    [WithAll(typeof(Collapsing), typeof(GameSpaceNode))]
    [WithNone(typeof(CollapsedSpace))]
    public partial struct CollapseAgentsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(
            in DynamicBuffer<GameSpaceChild> children,
            [ChunkIndexInQuery] int chunkIndexInQuery
        )
        {
            foreach (var child in children.Reinterpret<Entity>())
                CommandBuffer.AddComponent<Collapsing>(chunkIndexInQuery, child);
        }
    }
}

using Schnozzle.GameSpace.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Schnozzle.GameSpace.Jobs
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct MoverJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        /// <summary>
        /// Use lookup instead of requesting component to avoid constant version bumping of GameSpaceParent component.
        /// </summary>
        [NativeDisableParallelForRestriction]
        public ComponentLookup<GameSpaceParent> GameSpaceParentLookup;

        [ReadOnly]
        public ComponentLookup<LocalTransform> TransformLookup;

        public void Execute(
            Entity entity,
            in TravelSpeed speed,
            ref Traveling traveling,
            in DynamicBuffer<Path> path,
            in SimulatedAgent simulation,
            [ChunkIndexInQuery] int chunkIndexInQuery
        )
        {
            if (traveling.Progress >= traveling.DistanceToNextNode)
            {
                GameSpaceParentLookup[entity] = new GameSpaceParent { Entity = path[traveling.PathNodeIndex].Node };
                traveling.Progress = 0;
                traveling.PathNodeIndex++;

                if (traveling.PathNodeIndex >= path.Length)
                {
                    CommandBuffer.RemoveComponent<Traveling>(chunkIndexInQuery, entity);
                    return;
                }

                var startNode = GameSpaceParentLookup[entity].Entity;
                var destinationNode = path[traveling.PathNodeIndex].Node;

                traveling.DistanceToNextNode = math.distance(
                    TransformLookup[startNode].Position,
                    TransformLookup[destinationNode].Position
                );
            }

            traveling.Progress += speed.Value * simulation.CurrentDeltaTime;
        }
    }
}

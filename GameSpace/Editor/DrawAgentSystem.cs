using Drawing;
using Schnozzle.GameSpace.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Schnozzle.GameSpace.Editor
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    internal partial class DrawAgentSystem : EditorGizmoSystem
    {
        protected override void DrawGizmos()
        {
            var builder = DrawingManager.GetBuilder();

            EntityManager
                .CreateEntityQuery(ComponentType.ReadOnly<LocalToWorld>())
                .CompleteDependency();

            var handle = new DrawJob
            {
                Builder = builder,
                OutOfBoundsLookup = SystemAPI.GetComponentLookup<OutOfBounds>(true),
                LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true)
            }.ScheduleParallel(default(JobHandle));

            builder.DisposeAfter(handle);

            handle.Complete();
        }

        [BurstCompile]
        [WithAll(typeof(SimulatedAgent))]
        private partial struct DrawJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<OutOfBounds> OutOfBoundsLookup;

            [ReadOnly]
            public ComponentLookup<LocalToWorld> LocalToWorldLookup;

            public CommandBuilder Builder;

            public void Execute(Entity entity, in LocalToWorld transform, in GameSpaceParent parent)
            {
                var isOutOfBounds = OutOfBoundsLookup.HasComponent(entity);
                Builder.SolidBox(
                    transform.Position,
                    0.3f,
                    isOutOfBounds ? Color.yellow : Color.magenta
                );

                if (LocalToWorldLookup.TryGetComponent(parent.Entity, out var localToWorldParent))
                {
                    Builder.PushLineWidth(2, false);
                    Builder.Arrow(transform.Position, localToWorldParent.Position, new Color32(0, 180, 0, 255));
                    Builder.PopLineWidth();
                }
            }
        }
    }
}

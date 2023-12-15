using Drawing;
using Schnozzle.GameSpace.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor;
using static Unity.Entities.SystemAPI;
using Color = UnityEngine.Color;

namespace Schnozzle.GameSpace.Editor
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    internal partial class DrawGameSpaceNodeSystem : EditorGizmoSystem
    {
        private void Arrow(float3 from, float3 to, Color color)
        {
            var reverseDir = from - to;
            Draw.Arrow(from, to + math.normalize(reverseDir), new float3(0, 1, 0), 0.1f, color);
        }

        protected override void DrawGizmos()
        {
            var filterDepth = GameSpaceEditorOverlay.FilterDepth;
            var filteredDepth = GameSpaceEditorOverlay.Depth;

            var parentLookup = SystemAPI.GetComponentLookup<Parent>(true);

            foreach (
                var (transform, connections, name, node, collider, entity) in Query<
                    LocalToWorld,
                    DynamicBuffer<SpatialConnection>,
                    GameSpaceNodeName,
                    GameSpaceNode,
                    PhysicsCollider
                >()
                    .WithEntityAccess()
            )
            {
                if (filterDepth && node.Depth != filteredDepth)
                    continue;

                var aabb = collider.Value.Value.CalculateAabb();

                using (Draw.WithLineWidth(2, false))
                {
                    Draw.WireBox(transform.Position + aabb.Center, aabb.Extents, new Color(1, 0.6f, 0.3f, 1f));
                    
                    
                foreach (var connection in connections)
                    Arrow(
                        transform.Position,
                        SystemAPI.GetComponent<LocalToWorld>(connection.Entity).Position,
                        Color.blue
                    );

                var isCollapsed = SystemAPI.HasComponent<CollapsedSpace>(entity);

                Draw.SolidCircle(
                    transform.Position,
                    SceneView.lastActiveSceneView.camera.transform.forward,
                    0.05f,
                    isCollapsed ? Color.green : Color.red
                );
                Draw.Label2D(transform.Position + new float3(0.05f, 0, 0), $"{name.Value} - ({node.Depth})", 25, Color.black);
                if (parentLookup.TryGetComponent(entity, out var parent))
                    Arrow(
                        transform.Position,
                        SystemAPI.GetComponent<LocalToWorld>(parent.Value).Position,
                        Color.blue
                    );
                }

            }
        }
    }
}

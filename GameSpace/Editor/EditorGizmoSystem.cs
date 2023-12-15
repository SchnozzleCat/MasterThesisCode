using Schnozzle.GameSpace.Utilities;
using Unity.Entities;

namespace Schnozzle.GameSpace.Editor
{
    internal abstract partial class EditorGizmoSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            GizmoProxy.Instance.OnDrawGizmos += DrawGizmos;
        }

        protected override void OnStopRunning()
        {
            GizmoProxy.Instance.OnDrawGizmos -= DrawGizmos;
        }

        protected abstract void DrawGizmos();

        protected override void OnUpdate() { }
    }
}

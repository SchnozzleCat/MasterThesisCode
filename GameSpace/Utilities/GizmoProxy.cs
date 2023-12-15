#if UNITY_EDITOR
using System;
using Drawing;
using UnityEngine;

namespace Schnozzle.GameSpace.Utilities
{
    [ExecuteAlways]
    public class GizmoProxy : MonoBehaviourGizmos
    {
        private static GizmoProxy _Instance;

        public static GizmoProxy Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<GizmoProxy>();
                    if (_Instance == null)
                    {
                        var obj = new GameObject("EditorGizmoProxy");
                        obj.tag = "EditorOnly";
                        _Instance = obj.AddComponent<GizmoProxy>();
                    }
                }

                return _Instance;
            }
        }

        private void Awake()
        {
            _Instance = this;
        }

        public event Action OnDrawGizmos;

        public override void DrawGizmos()
        {
            OnDrawGizmos?.Invoke();
        }
    }
}
#endif

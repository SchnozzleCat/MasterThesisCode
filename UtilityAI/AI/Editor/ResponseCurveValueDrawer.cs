using Schnozzle.AI.Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Schnozzle.AI.Editor
{
    public class ResponseCurveValueDrawer : OdinValueDrawer<ResponseCurve>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            const int PointCount = 250;

            GUILayout.Space(10);

            var rect = EditorGUILayout.GetControlRect(false, 250);
            SirenixEditorGUI.DrawBorders(rect, 1, Color.gray);

            var curve = ValueEntry.SmartValue;
            var delta = rect.max - rect.min;
            var start = new Vector3(rect.xMin, rect.yMax);

            Handles.BeginGUI();

            var positions = new Vector3[PointCount];
            var colors = new Color[PointCount];

            for (int i = 0; i < PointCount; i++)
            {
                var x = i / (float)PointCount;
                positions[i] = start + new Vector3(x * delta.x, -curve.EvaluateClamped(x) * delta.y);
                colors[i] = new Color(0.05f, 0.8f, 0.05f);
            }

            Handles.DrawAAPolyLine(5, colors, positions);

            Handles.EndGUI();

            CallNextDrawer(label);
        }
    }
}

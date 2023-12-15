using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Schnozzle.GameSpace.Editor
{
    [Overlay(typeof(SceneView), "Game Space", defaultLayout = Layout.HorizontalToolbar)]
    public class GameSpaceEditorOverlay : Overlay
    {
        private const string DepthEditorPrefsKey = "GameSpaceNodeDepth";
        private const string FilterDepthEditorPrefsKey = "GameSpaceFilterNodeDepth";

        public static int Depth
        {
            get => EditorPrefs.GetInt(DepthEditorPrefsKey, 0);
            set => EditorPrefs.SetInt(DepthEditorPrefsKey, value);
        }

        public static bool FilterDepth
        {
            get => EditorPrefs.GetBool(FilterDepthEditorPrefsKey, false);
            set => EditorPrefs.SetBool(FilterDepthEditorPrefsKey, value);
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "My Toolbar Root" };
            root.style.width = 200;
            var filter = new Toggle("Filter Depth");
            filter.value = FilterDepth;
            filter.RegisterValueChangedCallback(evt => FilterDepth = evt.newValue);
            var label = new Label(Depth.ToString());
            var slider = new SliderInt(0, 10);
            slider.value = Depth;
            slider.RegisterValueChangedCallback(evt =>
            {
                label.text = evt.newValue.ToString();
                Depth = evt.newValue;
            });
            root.Add(filter);
            root.Add(label);
            root.Add(slider);
            return root;
        }
    }
}

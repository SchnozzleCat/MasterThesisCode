using System;
using System.Collections.Generic;
using System.Linq;
using Schnozzle.AI.Components;
using Schnozzle.AI.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Schnozzle.AI.Editor
{
    public class AIDebuggingEditorWindow : OdinEditorWindow
    {
        public static void ShowWindow<TExecutionContext, TEnum, TCustomData>()
            where TExecutionContext : unmanaged, IExecutionContext<TEnum>
            where TEnum : unmanaged, Enum
            where TCustomData : unmanaged
        {
            var window = GetWindow<AIDebuggingEditorWindow>();
            window._debuggingData = new AIDebuggingData<TExecutionContext, TEnum, TCustomData>();
        }

        [ShowInInspector, HideLabel, HideReferenceObjectPicker, InlineProperty]
        private IAIDebuggingData _debuggingData;

        protected override void OnGUI()
        {
            base.OnGUI();
        }
    }

    public interface IAIDebuggingData { }

    public class AIDebuggingData<TExecutionContext, TEnum, TCustomData> : IAIDebuggingData
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        [ValueDropdown(nameof(Worlds))]
        public string SelectedWorld;

        private IEnumerable<string> Worlds
        {
            get
            {
                foreach (var world in World.All)
                    yield return world.Name;
            }
        }

        private World CurrentWorld
        {
            get
            {
                foreach (var world in World.All)
                    if (world.Name == SelectedWorld)
                        return world;
                return null;
            }
        }

        [ValueDropdown(nameof(Entities))]
        [HideIf("@CurrentWorld == null")]
        [ShowInInspector]
        private Entity _currentEntity;

        private IEnumerable<Entity> Entities
        {
            get
            {
                if (CurrentWorld == null)
                    return Enumerable.Empty<Entity>();
                var query = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>>()
                    .Build(CurrentWorld.EntityManager);
                return query.ToEntityArray(Allocator.Temp).ToArray();
            }
        }

        private bool[] _foldouts;
        private Stack<int> _depthStack = new();

        [OnInspectorGUI]
        private void DrawGUI()
        {
            var world = CurrentWorld;

            if (world == null || _currentEntity == Entity.Null || !world.EntityManager.Exists(_currentEntity))
                return;

            var buffer = CurrentWorld.EntityManager.GetBuffer<
                RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>
            >(_currentEntity);

            if (_foldouts == null || _foldouts.Length != buffer.Length)
                _foldouts = new bool[buffer.Length];

            _depthStack.Clear();

            SirenixEditorGUI.HorizontalLineSeparator();

            var depth = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                var entry = buffer[i];
                if (entry.Type == RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>.MessageType.DecisionSet)
                    depth = 0;
                if (!_depthStack.TryPeek(out var result) || _foldouts[result] || depth == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(depth * 20);
                    if (entry.Depth > 0)
                    {
                        _foldouts[i] = SirenixEditorGUI.Foldout(_foldouts[i], entry.Message);
                        _depthStack.Push(i);
                    }
                    else
                        EditorGUILayout.LabelField(entry.Message);

                    EditorGUILayout.EndHorizontal();
                    depth += entry.Depth;
                    if (entry.Depth < 0)
                        _depthStack.Pop();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Utilities;
using Hash128 = Unity.Entities.Hash128;

namespace Schnozzle.AI.Data
{
    public struct DecisionSetBlob<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
#if UNITY_EDITOR
        public FixedString32Bytes Name;
#endif
        public BlobArray<DecisionBlob<TExecutionContext, TEnum, TCustomData>> Decisions;
    }

    public class DecisionSetSettings<TExecutionContext, TEnum, TCustomData> : SerializedScriptableObject
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        [SerializeField, Sirenix.OdinInspector.ReadOnly]
#if UNITY_EDITOR
        [OnInspectorInit(nameof(SetGuid))]
#endif
        private string _guid;

        public string Guid => _guid;

#if UNITY_EDITOR

        private void ValueChanged()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }

        [InlineEditor, OnValueChanged(nameof(ValueChanged))]
#endif
        [SerializeField]
        private List<DecisionSettings<TExecutionContext, TEnum, TCustomData>> _decisions = new();

#if UNITY_EDITOR
        private void SetGuid()
        {
            if (!string.IsNullOrEmpty(_guid))
                return;
            _guid = UnityEditor.AssetDatabase
                .GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(this))
                .ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public void PopulateBlob(
            ref DecisionSetBlob<TExecutionContext, TEnum, TCustomData> blob,
            ref BlobBuilder builder
        )
        {
#if UNITY_EDITOR
            blob.Name = name;
#endif
            var arrayBuilder = builder.Allocate(ref blob.Decisions, _decisions.Count);

            for (int i = 0; i < _decisions.Count; i++)
            {
                _decisions[i].PopulateBlob(ref arrayBuilder[i], ref builder);
            }
        }
    }
}

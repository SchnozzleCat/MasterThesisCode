using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Schnozzle.AI.Components;
using Schnozzle.AI.Systems;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.AI.Data
{
    public struct DecisionBlob<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
#if UNITY_EDITOR
        public FixedString32Bytes Name;
#endif

        public float Weight;
        public bool RequiresTarget;

        public BlobArray<ConsiderationBlob<TExecutionContext, TEnum>> Considerations;

        public TCustomData Data;

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal float Score<T>(in TExecutionContext context, float cutOff, ref T debuggingContainer)
            where T : unmanaged, RuntimeDebugging<TExecutionContext, TEnum, TCustomData>.IDebuggingContainer
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal float Score(in TExecutionContext context, float cutOff)
#endif
        {
            var finalScore = 1f;

            var considerationCount = Considerations.Length;

            for (int i = 0; i < considerationCount; i++)
            {
#if UNITY_EDITOR
                if (finalScore < cutOff && !debuggingContainer.IgnoreCutoff)
                    break;
#else
                if (finalScore < cutOff)
                    break;
#endif

                ref var consideration = ref Considerations[i];

#if UNITY_EDITOR
                var score = consideration.Score(in context);
                debuggingContainer.AddDebuggingData(
                    new RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>(ref consideration, score)
                );
                finalScore *= score;
#else
                finalScore *= consideration.Score(in context);
#endif
            }

            // Compensation factor
            var modificationFactor = 1 - 1f / considerationCount;
            var makeUpValue = (1 - finalScore) * modificationFactor;

            return finalScore + makeUpValue * finalScore * Weight;
        }
    }

    public abstract class DecisionSettings<TExecutionContext, TEnum, TCustomData> : SerializedScriptableObject
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        [SerializeField, Range(1, 5)]
        private float _weight = 1;

        [SerializeField]
        private bool _requiresTarget;

        [SerializeField, InlineEditor]
        private List<ConsiderationSettings<TExecutionContext, TEnum>> _considerations = new();

        public void PopulateBlob(ref DecisionBlob<TExecutionContext, TEnum, TCustomData> blob, ref BlobBuilder builder)
        {
#if UNITY_EDITOR
            blob.Name = name;
#endif
            blob.RequiresTarget = _requiresTarget;
            blob.Weight = _weight;
            var arrayBuilder = builder.Allocate(ref blob.Considerations, _considerations.Count);

            for (int i = 0; i < _considerations.Count; i++)
                _considerations[i].PopulateBlob(ref arrayBuilder[i], ref builder);

            PopulateData(ref blob.Data);
        }

        public abstract void PopulateData(ref TCustomData data);
    }
}

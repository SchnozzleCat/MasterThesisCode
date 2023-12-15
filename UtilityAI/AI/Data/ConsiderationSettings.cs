using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Schnozzle.AI.Data
{
    public struct ConsiderationBlob<TExecutionContext, TEnum>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
    {
#if UNITY_EDITOR
        public FixedString32Bytes Name;
#endif
        public BlobPtr<InputHeader<TEnum>> Input;
        public ResponseCurve Curve;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Score(in TExecutionContext context)
        {
            //TODO: Introducing a caching mechanism here per frame could improve performance.
            return Curve.EvaluateClamped(context.GetInputValue(ref Input.Value));
        }
    }

    public abstract class ConsiderationSettings<TExecutionContext, TEnum> : SerializedScriptableObject
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
    {
        [SerializeField, InlineProperty, HideLabel]
        private InputSettings<TExecutionContext, TEnum> _input;

        [FormerlySerializedAs("_curveParameters")]
        [SerializeField, InlineProperty, HideLabel]
        private ResponseCurve _responseCurve;

        public void PopulateBlob(ref ConsiderationBlob<TExecutionContext, TEnum> blob, ref BlobBuilder builder)
        {
            _input.CreateBlobPtr(ref blob.Input, ref builder);
            blob.Curve = _responseCurve;
#if UNITY_EDITOR
            blob.Name = name;
#endif
        }
    }
}

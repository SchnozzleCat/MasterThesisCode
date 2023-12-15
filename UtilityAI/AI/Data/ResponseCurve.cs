using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Schnozzle.AI.Data
{
    public struct ResponseCurve
    {
        public enum CurveType : byte
        {
            Linear,
            Quadratic,
            Logistic,
            Logit
        }

        [EnumToggleButtons, HideLabel]
        public CurveType Type;

        public float M,
            K,
            B,
            C;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EvaluateClamped(float x) => math.clamp(Evaluate(x), 0, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Evaluate(float x) =>
            Type switch
            {
                CurveType.Linear => M * math.pow((x - C), K) + B,
                CurveType.Quadratic => M * math.pow((x * x - C), K) + B,
                CurveType.Logistic => K / (1 + math.pow(1000 * math.E * M, -1 * x + C)) + B,
                CurveType.Logit => M * 0.05f * math.log2((K * x + C) / (1 - (K * x + C))) + B,
                _ => throw new ArgumentException()
            };
    }
}

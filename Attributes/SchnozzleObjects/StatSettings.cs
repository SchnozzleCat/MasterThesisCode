using System;
using System.Collections.Generic;
using System.Linq;
using AOT;
using Newtonsoft.Json;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.Core.SchnozzleObject.Interfaces;
using Schnozzle.ECS.Extensions;
using Schnozzle.ECS.SchnozzleObject;
using Sirenix.OdinInspector;
using Spark.Status.Components.Stats;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.SchnozzleObjects
{
    public struct StatSettingsBlob : ISchnozzleBlob
    {
        [BurstCompile]
        internal struct StatModifier
        {
            #region ModifierFunctions

            public delegate float Delegate(float statValue, float modifierValue, float targetStatValue);

            [BurstCompile, MonoPInvokeCallback(typeof(Delegate))]
            public static float ScaleThenAdd(float statValue, float modifierValue, float targetStatValue) =>
                targetStatValue + statValue * modifierValue;

            [BurstCompile, MonoPInvokeCallback(typeof(Delegate))]
            public static float ScaleAndMultiplyWithTargetThenAdd(
                float statValue,
                float modifierValue,
                float targetStatValue
            ) => targetStatValue + statValue * modifierValue * targetStatValue;

            #endregion

            public SchnozzleReference<StatSettings> Stat;
            public float Value;
            public FunctionPointer<Delegate> ModifierFunction;

            internal void RunModification(
                ref Stat stat,
                ref DynamicBuffer<Stat> stats,
                ref SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs blobs
            )
            {
                ref var targetBlob = ref blobs[Stat].Value;
                ref var targetStat = ref stats.GetAsRef(targetBlob.Index);
                var modification = ModifierFunction.Invoke(stat.Value, Value, targetStat.Value);
                targetStat.Value = modification;
            }
        }

        public bool RoundToInt;
        public StatSettings.MultiplierType MultiplierType;
        public bool BaseStat;
        public int Index;
        public float MinValue;
        public float MaxValue;

        internal BlobArray<StatModifier> StatModifiers;

        public void RunModifications(
            ref DynamicBuffer<Stat> stats,
            ref SchnozzleObject<StatSettings, StatSettingsBlob>.Blobs blobs
        )
        {
            ref var stat = ref stats.GetAsRef(Index);

            for (int i = 0, length = StatModifiers.Length; i < length; i++)
            {
                ref var modifier = ref StatModifiers[i];
                modifier.RunModification(ref stat, ref stats, ref blobs);
            }
        }

        public void Dispose() { }
    }

    [CreateAssetMenu(menuName = "Gameplay/Stat")]
    public class StatSettings : SchnozzleObject<StatSettings, StatSettingsBlob>, IOrderable
    {
        public static IEnumerable<StatSettings> Sorted => GetAll().Ordered();

        public enum MultiplierType
        {
            Additive,
            Multiplicative
        }

        [ShowInInspector, PropertyOrder(-1)]
        public string Name => name;

        [SerializeField, JsonProperty]
        private bool _baseStat;

        [SerializeField, JsonProperty]
        private bool _roundToInt;

        [SerializeField, JsonProperty]
        private MultiplierType _multiplierType = MultiplierType.Additive;

        [SerializeField, JsonProperty]
        private int _order;

        [SerializeField, JsonProperty]
        private float _minValue = float.MinValue;

        [SerializeField, JsonProperty]
        private float _maxValue = float.MaxValue;

        [SerializeField, JsonProperty]
        private List<StatModifierSettings> _statModifiers = new();

        [InlineProperty, HideReferenceObjectPicker]
        internal class StatModifierSettings
        {
            [ClearOnReload]
            private static Dictionary<
                ModifyMode,
                FunctionPointer<StatSettingsBlob.StatModifier.Delegate>
            > _functionCache = new();

            public enum ModifyMode
            {
                ScaleThenAdd,
                ScaleAndMultiplyThenAdd
            }

            public SchnozzleObjectReference<StatSettings> Stat = new();
            public float Value;
            public ModifyMode Mode;

            public StatSettingsBlob.StatModifier AsBlob =>
                new()
                {
                    Value = Value,
                    Stat = Stat.AsRef,
                    ModifierFunction = GetBurstFunction(Mode)
                };

            private StatSettingsBlob.StatModifier.Delegate GetFunction(ModifyMode mode)
            {
                return mode switch
                {
                    ModifyMode.ScaleThenAdd => StatSettingsBlob.StatModifier.ScaleThenAdd,
                    ModifyMode.ScaleAndMultiplyThenAdd
                        => StatSettingsBlob.StatModifier.ScaleAndMultiplyWithTargetThenAdd,
                    _ => throw new Exception("Unhandled case!")
                };
            }

            private FunctionPointer<StatSettingsBlob.StatModifier.Delegate> GetBurstFunction(ModifyMode mode)
            {
                _functionCache ??= new();
                if (!_functionCache.TryGetValue(mode, out var functionPointer))
                {
                    var function = GetFunction(mode);
                    functionPointer = BurstCompiler.CompileFunctionPointer(function);
                    _functionCache.Add(mode, functionPointer);
                }

                return functionPointer;
            }
        }

        public int Order => _order;

        [ShowInInspector, ReadOnly]
        public int Index => Sorted.ToList().IndexOf(this);

        public bool BaseStat => _baseStat;

        public override void PopulateBlob(ref StatSettingsBlob blob, ref BlobBuilder builder, World world)
        {
            blob.Index = Sorted.ToList().IndexOf(this);
            blob.BaseStat = _baseStat;
            blob.MinValue = _minValue;
            blob.MaxValue = _maxValue;
            blob.RoundToInt = _roundToInt;
            blob.MultiplierType = _multiplierType;
            _statModifiers.ToBlobSelect(x => x.AsBlob, ref blob.StatModifiers, ref builder);
        }
    }
}

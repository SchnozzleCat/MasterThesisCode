using System;
using System.Runtime.CompilerServices;
using Schnozzle.AI.Components;
using Schnozzle.AI.Data;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.Assertions;

namespace Schnozzle.AI.Systems
{
#if UNITY_EDITOR
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class RuntimeDebugging<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        internal interface IDebuggingContainer
        {
            public bool IgnoreCutoff { get; }

            public void AddDebuggingData(RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData> data);
        }

        internal struct NoOpDebuggingContainer : IDebuggingContainer
        {
            public bool IgnoreCutoff => false;

            public void AddDebuggingData(RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData> data) { }
        }

        internal struct RecordDebuggingContainer : IDebuggingContainer
        {
            public DynamicBuffer<RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>> Buffer;

            public RecordDebuggingContainer(
                DynamicBuffer<RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>> buffer
            )
            {
                Buffer = buffer;
                Buffer.Clear();
            }

            public bool IgnoreCutoff => true;

            public void AddDebuggingData(RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData> data)
            {
                Buffer.Add(data);
            }
        }
    }
#endif

    [BurstCompile]
    public struct AIExecutorJob<TExecutionContext, TDecisionSetBuffer, TActiveDecisionComponent, TEnum, TCustomData>
        : IJobChunk
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TDecisionSetBuffer : unmanaged, IDecisionSetBuffer<TExecutionContext, TEnum, TCustomData>
        where TActiveDecisionComponent : unmanaged, IActiveDecision<TExecutionContext, TEnum, TCustomData>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        [ReadOnly, NativeDisableUnsafePtrRestriction, NativeDisableContainerSafetyRestriction]
        public TExecutionContext Context;

        [ReadOnly]
        public BufferTypeHandle<TDecisionSetBuffer> DecisionSetBufferHandle;

        [ReadOnly]
        public BufferTypeHandle<AITargetBuffer> TargetBuffer;

        public ComponentTypeHandle<TActiveDecisionComponent> ActiveDecisionTypeHandle;

#if UNITY_EDITOR
        public BufferTypeHandle<
            RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>
        > RuntimeDebuggingBufferHandle;
#endif

        public unsafe void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            Context.OnChunkBegin(in chunk);

            var decisionSetBufferAccessor = chunk.GetBufferAccessor(ref DecisionSetBufferHandle);
            var targetBufferAccessor = chunk.GetBufferAccessor(ref TargetBuffer);
            var activeDecisions = chunk.GetComponentDataPtrRW(ref ActiveDecisionTypeHandle);

            var len = chunk.Count;

#if UNITY_EDITOR
            if (chunk.Has<RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>>())
            {
                var runtimeDebuggingBufferAccessor = chunk.GetBufferAccessor(ref RuntimeDebuggingBufferHandle);
                for (int i = 0; i < len; i++)
                {
                    var decisionSets = decisionSetBufferAccessor[i];
                    var targetBuffer = targetBufferAccessor[i];
                    ref var activeDecision = ref activeDecisions[i];
                    HandleEntity(
                        i,
                        in decisionSets,
                        in targetBuffer,
                        ref activeDecision,
                        new RuntimeDebugging<TExecutionContext, TEnum, TCustomData>.RecordDebuggingContainer(
                            runtimeDebuggingBufferAccessor[i]
                        )
                    );
                }

                return;
            }
#endif

            for (int i = 0; i < len; i++)
            {
                var decisionSets = decisionSetBufferAccessor[i];
                var targetBuffer = targetBufferAccessor[i];
                ref var activeDecision = ref activeDecisions[i];
#if UNITY_EDITOR
                HandleEntity(
                    i,
                    in decisionSets,
                    in targetBuffer,
                    ref activeDecision,
                    new RuntimeDebugging<TExecutionContext, TEnum, TCustomData>.NoOpDebuggingContainer()
                );
#else
                HandleEntity(i, in decisionSets, in targetBuffer, ref activeDecision);
#endif
            }
        }

#if UNITY_EDITOR

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleEntity<T>(
            int index,
            in DynamicBuffer<TDecisionSetBuffer> decisionSets,
            in DynamicBuffer<AITargetBuffer> targetBuffer,
            ref TActiveDecisionComponent activeDecision,
            T debuggingContainer
        )
            where T : unmanaged, RuntimeDebugging<TExecutionContext, TEnum, TCustomData>.IDebuggingContainer
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleEntity(
            int index,
            in DynamicBuffer<TDecisionSetBuffer> decisionSets,
            in DynamicBuffer<AITargetBuffer> targetBuffer,
            ref TActiveDecisionComponent activeDecision
        )
#endif
        {
            Context.OnIterateEntity(index);

            var decisionSetCount = decisionSets.Length;

            (DecisionReference<TExecutionContext, TEnum, TCustomData> Decision, float Score, Entity? Target) result =
                default;

            Assert.IsTrue(result.Decision.IsPanic);

            for (int i = 0; i < decisionSetCount; i++)
            {
                ref var decisionSet = ref decisionSets[i].Set.Value;

#if UNITY_EDITOR
                debuggingContainer.AddDebuggingData(
                    new RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>(ref decisionSet)
                );
                var (decision, score, target) = HandleDecisionSet(
                    ref decisionSet,
                    in targetBuffer,
                    ref debuggingContainer
                );
#else
                var (decision, score, target) = HandleDecisionSet(ref decisionSet, in targetBuffer);
#endif

                if (score > result.Score)
                {
                    result.Score = score;
                    result.Decision = decision;
                    result.Target = target;
                }
            }

            activeDecision = new() { Value = result.Decision, Target = result.Target };
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (
            DecisionReference<TExecutionContext, TEnum, TCustomData> Decision,
            float Score,
            Entity? Target
        ) HandleDecisionSet<T>(
            ref DecisionSetBlob<TExecutionContext, TEnum, TCustomData> decisionSet,
            in DynamicBuffer<AITargetBuffer> targetBuffer,
            ref T debuggingContainer
        )
            where T : unmanaged, RuntimeDebugging<TExecutionContext, TEnum, TCustomData>.IDebuggingContainer
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (
            DecisionReference<TExecutionContext, TEnum, TCustomData> Decision,
            float Score,
            Entity? Target
        ) HandleDecisionSet(
            ref DecisionSetBlob<TExecutionContext, TEnum, TCustomData> decisionSet,
            in DynamicBuffer<AITargetBuffer> targetBuffer
        )
#endif
        {
            var decisionCount = decisionSet.Decisions.Length;

            (DecisionReference<TExecutionContext, TEnum, TCustomData> Decision, float Score, Entity? Target) result =
                default;

            for (int i = 0; i < decisionCount; i++)
            {
                ref var decision = ref decisionSet.Decisions[i];

                if (decision.RequiresTarget)
                {
                    foreach (var target in targetBuffer)
                    {
                        Context.Target = target.Value;
#if UNITY_EDITOR
                        debuggingContainer.AddDebuggingData(
                            new RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>(
                                ref decision,
                                target.Value
                            )
                        );
                        var score = decision.Score(in Context, result.Score, ref debuggingContainer);
                        debuggingContainer.AddDebuggingData(
                            new RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>(score)
                        );
#else
                        var score = decision.Score(in Context, result.Score);
#endif
                        if (score > result.Score)
                        {
                            result.Score = score;
                            result.Decision = new DecisionReference<TExecutionContext, TEnum, TCustomData>(
                                ref decision
                            );
                            result.Target = target.Value;
                        }
                    }
                }
                else
                {
                    Context.Target = null;
#if UNITY_EDITOR
                    debuggingContainer.AddDebuggingData(
                        new RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>(ref decision, Entity.Null)
                    );
                    var score = decision.Score(in Context, result.Score, ref debuggingContainer);
                    debuggingContainer.AddDebuggingData(
                        new RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>(score)
                    );
#else
                    var score = decision.Score(in Context, result.Score);
#endif

                    if (score > result.Score)
                    {
                        result.Score = score;
                        result.Decision = new DecisionReference<TExecutionContext, TEnum, TCustomData>(ref decision);
                        result.Target = null;
                    }
                }
            }

            return result;
        }
    }

    [UpdateInGroup(typeof(AISystemGroup))]
    public partial struct AIExecutionSystem<
        TExecutionContext,
        TDecisionSetBuffer,
        TActiveDecisionComponent,
        TEnum,
        TDecisionData
    > : ISystem
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TDecisionSetBuffer : unmanaged, IDecisionSetBuffer<TExecutionContext, TEnum, TDecisionData>
        where TActiveDecisionComponent : unmanaged, IActiveDecision<TExecutionContext, TEnum, TDecisionData>
        where TEnum : unmanaged, Enum
        where TDecisionData : unmanaged
    {
        private TExecutionContext _gameContext;

        private EntityQuery _query;

        //TODO: Replace this with a SystemAPI call once the incorrectly emitting source generator is fixed.
        private BufferTypeHandle<TDecisionSetBuffer> _decisionSetBufferTypeHandle;
#if UNITY_EDITOR
        private BufferTypeHandle<
            RuntimeDebuggingBuffer<TExecutionContext, TEnum, TDecisionData>
        > _runtimeDebuggingBufferTypeHandle;
#endif

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _gameContext.OnCreate(ref state);
            _query = _gameContext.ScheduleQuery;

            _decisionSetBufferTypeHandle = state.GetBufferTypeHandle<TDecisionSetBuffer>(true);

#if UNITY_EDITOR
            _runtimeDebuggingBufferTypeHandle = state.GetBufferTypeHandle<
                RuntimeDebuggingBuffer<TExecutionContext, TEnum, TDecisionData>
            >();
#endif
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _gameContext.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            _runtimeDebuggingBufferTypeHandle.Update(ref state);
#endif
            _decisionSetBufferTypeHandle.Update(ref state);

            _gameContext.OnUpdate(ref state);

            state.Dependency = new AIExecutorJob<
                TExecutionContext,
                TDecisionSetBuffer,
                TActiveDecisionComponent,
                TEnum,
                TDecisionData
            >
            {
                Context = _gameContext,
                TargetBuffer = SystemAPI.GetBufferTypeHandle<AITargetBuffer>(true),
                DecisionSetBufferHandle = _decisionSetBufferTypeHandle,
                ActiveDecisionTypeHandle = SystemAPI.GetComponentTypeHandle<TActiveDecisionComponent>(),
#if UNITY_EDITOR
                RuntimeDebuggingBufferHandle = _runtimeDebuggingBufferTypeHandle
#endif
            }.ScheduleParallel(_query, state.Dependency);
        }
    }
}

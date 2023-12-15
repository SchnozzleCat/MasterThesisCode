using System;
using System.Runtime.CompilerServices;
using Drawing;
using Schnozzle.AI.Components;
using Schnozzle.AI.Data;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Schnozzle.AI.Systems
{
    [UpdateInGroup(typeof(AISystemGroup))]
    public partial struct AIDebuggingSystem<TExecutionContext, TEnum, TCustomData> : ISystem
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public struct EnableComponent : IComponentData { }

        [BurstCompile]
        public struct Job : IJobChunk
        {
            public CommandBuilder Builder;
            public FixedString64Bytes PanicString;

            [ReadOnly]
            public ComponentTypeHandle<ActiveDecision<TExecutionContext, TEnum, TCustomData>> ActiveDecisionTypeHandle;

            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> LocalToWorldTypeHandle;

            [ReadOnly]
            public BufferTypeHandle<AITargetBuffer> AITargetBufferHandle;

            [ReadOnly]
            public ComponentLookup<LocalToWorld> LocalToWorldLookup;

            public int DetectionRadius;

            public void Execute(
                in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask
            )
            {
                var decisions = chunk.GetNativeArray(ref ActiveDecisionTypeHandle);
                var transforms = chunk.GetNativeArray(ref LocalToWorldTypeHandle);
                var aiTargetBufferAccessor = chunk.GetBufferAccessor(ref AITargetBufferHandle);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var decision = decisions[i];
                    var transform = transforms[i];
                    var aiTargets = aiTargetBufferAccessor[i];

                    var str = decision.Value.IsPanic ? PanicString : decision.Value.Value.Name;
                    Builder.Label2D(transform.Position, ref str);

                    Builder.CircleXZ(transform.Position, DetectionRadius);

                    foreach (var target in aiTargets)
                        Builder.Line(transform.Position, LocalToWorldLookup[target.Value].Position);
                }
            }
        }

        private ComponentTypeHandle<ActiveDecision<TExecutionContext, TEnum, TCustomData>> _activeDecisionTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // state.RequireForUpdate<EnableComponent>();
            _activeDecisionTypeHandle = state.GetComponentTypeHandle<
                ActiveDecision<TExecutionContext, TEnum, TCustomData>
            >();
        }

        public void OnUpdate(ref SystemState state)
        {
            _activeDecisionTypeHandle.Update(ref state);

            var builder = DrawingManager.GetBuilder();

            state.Dependency = new Job
            {
                Builder = builder,
                PanicString = "PANIC",
                ActiveDecisionTypeHandle = _activeDecisionTypeHandle,
                LocalToWorldTypeHandle = GetComponentTypeHandle<LocalToWorld>(true),
                LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                AITargetBufferHandle = GetBufferTypeHandle<AITargetBuffer>(true),
                DetectionRadius = 10
            }.ScheduleParallel(
                QueryBuilder().WithAll<ActiveDecision<TExecutionContext, TEnum, TCustomData>, LocalToWorld>().Build(),
                state.Dependency
            );

            builder.DisposeAfter(state.Dependency);
        }
    }
}

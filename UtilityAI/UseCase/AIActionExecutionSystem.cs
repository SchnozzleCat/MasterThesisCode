using System;
using Schnozzle.AI.Components;
using Schnozzle.AI.Systems;
using Schnozzle.UseCase.Bootstrapping;
using Schnozzle.UseCase.Stats;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

namespace Schnozzle.UseCase
{
    [WithAll(typeof(ActiveDecision<GameContext, BasicAIInputType, DecisionData>))]
    [BurstCompile]
    public unsafe partial struct AIActionExecutionJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public float DeltaTime;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<LocalTransform> LocalTransformLookup;

        public EntityCommandBuffer.ParallelWriter Writer;

        public Random Random;

        [ReadOnly]
        public ComponentTypeHandle<ActiveDecision<GameContext, BasicAIInputType, DecisionData>> ActiveDecisionHandle;

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableUnsafePtrRestriction]
        private ActiveDecision<GameContext, BasicAIInputType, DecisionData>* _activeDecisions;

        public int Size;

        public void Execute(
            Entity entity,
            ref CombatData data,
            [ChunkIndexInQuery] int chunkIndexInQuery,
            [EntityIndexInChunk] int entityIndexInQuery
        )
        {
            data.ReuseDelay -= DeltaTime;
            data.ReuseDelay = math.max(0, data.ReuseDelay);

            ref var activeDecision = ref _activeDecisions[entityIndexInQuery];

            if (!activeDecision.Value.IsPanic)
            {
                ref var value = ref activeDecision.Value.Value;

                value.Data.Runner.Execute(
                    entity,
                    ref Random,
                    ref data,
                    activeDecision.Target,
                    ref LocalTransformLookup,
                    ref Writer,
                    chunkIndexInQuery,
                    DeltaTime,
                    new float3(Size / 2, 0, Size / 2)
                );
            }
        }

        public bool OnChunkBegin(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            _activeDecisions = (ActiveDecision<GameContext, BasicAIInputType, DecisionData>*)
                chunk.GetNativeArray(ref ActiveDecisionHandle).GetUnsafeReadOnlyPtr();

            return true;
        }

        public void OnChunkEnd(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted
        ) { }
    }

    [UpdateInGroup(typeof(AISystemGroup), OrderLast = true)]
    public partial struct AIActionExecutionSystem : ISystem
    {
        private ComponentTypeHandle<ActiveDecision<GameContext, BasicAIInputType, DecisionData>> _activeDecisionHandle;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TestAISystem.Singleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            _activeDecisionHandle = state.GetComponentTypeHandle<
                ActiveDecision<GameContext, BasicAIInputType, DecisionData>
            >(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _activeDecisionHandle.Update(ref state);

            state.Dependency = new AIActionExecutionJob
            {
                Writer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged)
                    .AsParallelWriter(),
                DeltaTime = Time.DeltaTime,
                LocalTransformLookup = GetComponentLookup<LocalTransform>(),
                ActiveDecisionHandle = _activeDecisionHandle,
                Random = new Random((uint)(Time.ElapsedTime * 100) + 1),
                Size = GetSingleton<TestAISystem.Singleton>().AreaSize
            }.Schedule(state.Dependency);
        }
    }
}

using System;
using System.Diagnostics.Contracts;
using Schnozzle.AI.Components;
using Schnozzle.AI.Data;
using Schnozzle.UseCase.Bootstrapping;
using Schnozzle.UseCase.Inputs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEditor;

namespace Schnozzle.UseCase
{
    public enum BasicAIInputType : byte
    {
        MyHealth,
        IsTargetSelf,
        DistanceToTarget,
        AmmoCount,
        ReuseDelay
    }

    public struct GameContext : IExecutionContext<BasicAIInputType>
    {
        public AIAspect Aspect;
        public AIAspect.Lookup Lookup;
        public EntityStorageInfoLookup EntityStorage;

        private AIAspect.TypeHandle _typeHandle;
        private AIAspect.ResolvedChunk _resolvedChunk;

        private EntityQuery _query;

        public float GetInputValue(ref InputHeader<BasicAIInputType> header) =>
            header.Type switch
            {
                BasicAIInputType.MyHealth => header.GetValue<GameContext, BasicAIInputType, MyHealthInputBlob>(in this),
                BasicAIInputType.IsTargetSelf
                    => header.GetValue<GameContext, BasicAIInputType, IsTargetSelfInputBlob>(in this),
                BasicAIInputType.DistanceToTarget
                    => header.GetValue<GameContext, BasicAIInputType, DistanceToTargetInputBlob>(in this),
                BasicAIInputType.AmmoCount
                    => header.GetValue<GameContext, BasicAIInputType, AmmoCountInputBlob>(in this),
                BasicAIInputType.ReuseDelay
                    => header.GetValue<GameContext, BasicAIInputType, ReuseDelayInputBlob>(in this),
                _ => throw new ArgumentOutOfRangeException($"Missing case for {(byte)header.Type}")
            };

        public EntityQuery ScheduleQuery => _query;

        public Entity? Target { get; set; }

        public void OnChunkBegin(in ArchetypeChunk chunk) => _resolvedChunk = _typeHandle.Resolve(chunk);

        public void OnIterateEntity(int index) => Aspect = _resolvedChunk[index];

        public void OnCreate(ref SystemState state)
        {
            Lookup = new AIAspect.Lookup(ref state);
            EntityStorage = state.GetEntityStorageInfoLookup();
            _typeHandle = new AIAspect.TypeHandle(ref state);
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<AIAspect>()
                .WithAll<
                    ActiveDecision<GameContext, BasicAIInputType, DecisionData>,
                    DecisionSetBuffer<GameContext, BasicAIInputType, DecisionData>,
                    AITargetBuffer
                >()
                .Build(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityStorage.Update(ref state);
            Lookup.Update(ref state);
            _typeHandle.Update(ref state);
        }
    }
}

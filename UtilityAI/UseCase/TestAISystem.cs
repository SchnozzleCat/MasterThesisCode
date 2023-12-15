using Schnozzle.AI.Components;
using Schnozzle.AI.Data;
using Schnozzle.UseCase.Bootstrapping;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using static Unity.Entities.SystemAPI;

namespace Schnozzle.UseCase
{
    public partial struct TestAISystem : ISystem
    {
        public struct Singleton : IComponentData
        {
            public int EntityCount;
            public int AreaSize;
            public uint Seed;
            public int MaxTargetCount;
            public int DetectionRadius;
            public Entity Prefab;
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var singleton = GetSingleton<Singleton>();

            var prefab = singleton.Prefab;

            var result = Addressables
                .LoadAssetAsync<DecisionSetSettings<GameContext, BasicAIInputType, DecisionData>>("BasicCombatSet")
                .WaitForCompletion();

            Assert.IsTrue(result != null);

            var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var blob = ref blobBuilder.ConstructRoot<DecisionSetBlob<GameContext, BasicAIInputType, DecisionData>>();

            result.PopulateBlob(ref blob, ref blobBuilder);

            var blobReference = blobBuilder.CreateBlobAssetReference<DecisionSetBlob<GameContext, BasicAIInputType, DecisionData>>(
                Allocator.Persistent
            );

            state.EntityManager
                .GetBuffer<DecisionSetBuffer<GameContext, BasicAIInputType, DecisionData>>(prefab)
                .Add(new DecisionSetBuffer<GameContext, BasicAIInputType, DecisionData> { Value = blobReference });

            var entities = state.EntityManager.Instantiate(prefab, singleton.EntityCount, Allocator.Temp);

            var rand = new Random(singleton.Seed);

            var min = new float3(-singleton.AreaSize / 2f, 0, -singleton.AreaSize / 2f);
            var max = new float3(singleton.AreaSize / 2f, 0, singleton.AreaSize / 2f);

            foreach (var entity in entities)
            {
                SetComponent(entity, LocalTransform.FromPosition(rand.NextFloat3(min, max)));
            }

            state.Enabled = false;
        }
    }
}

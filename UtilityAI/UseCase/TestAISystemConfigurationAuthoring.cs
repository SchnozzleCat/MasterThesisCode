using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.UseCase
{
    public class TestAISystemConfigurationAuthoring : SerializedMonoBehaviour
    {
        [SerializeField]
        private int _entityCount;

        [SerializeField]
        private int _areaSize;

        [SerializeField]
        private uint _seed;

        [SerializeField]
        private int _maxTargetCount;

        [SerializeField]
        private int _detectionRadius;

        [SerializeField]
        private GameObject _prefab;

        public class AuthoringBaker : Baker<TestAISystemConfigurationAuthoring>
        {
            public override void Bake(TestAISystemConfigurationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(
                    entity,
                    new TestAISystem.Singleton
                    {
                        EntityCount = authoring._entityCount,
                        Seed = authoring._seed,
                        AreaSize = authoring._areaSize,
                        Prefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                        MaxTargetCount = authoring._maxTargetCount,
                        DetectionRadius = authoring._detectionRadius
                    }
                );
            }
        }
    }
}

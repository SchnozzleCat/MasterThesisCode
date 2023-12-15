using Unity.Entities;
using UnityEngine;

namespace Debugging
{
    public class PerformanceStatSystemSingletonAuthoring : MonoBehaviour
    {
        [SerializeField]
        private PerformanceStatSystem.Singleton _singleton;

        public class AuthoringBaker : Baker<PerformanceStatSystemSingletonAuthoring>
        {
            public override void Bake(PerformanceStatSystemSingletonAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring._singleton);
            }
        }
    }
}

using Spark.Status.Components.Stats;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.Authoring
{
    public class BaseStatAuthoring : MonoBehaviour
    {
        public class AuthoringBaker : Baker<BaseStatAuthoring>
        {
            public override void Bake(BaseStatAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<BaseStat>(entity);
            }
        }
    }
}

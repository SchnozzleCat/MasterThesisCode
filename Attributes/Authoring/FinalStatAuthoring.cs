using Spark.Status.Components;
using Spark.Status.Components.Stats;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.Authoring
{
    public class FinalStatAuthoring : MonoBehaviour
    {
        public class AuthoringBaker : Baker<FinalStatAuthoring>
        {
            public override void Bake(FinalStatAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<Stat>(entity);
                AddComponent<RequireStatCalculation>(entity);
            }
        }
    }
}

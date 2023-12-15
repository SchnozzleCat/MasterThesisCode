using Spark.Status.Components;
using Spark.Status.Components.Intrinsics;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.Authoring
{
    public class IntrinsicAuthoring : MonoBehaviour
    {
        public class Baker : Baker<IntrinsicAuthoring>
        {
            public override void Bake(IntrinsicAuthoring authoring)
            {
                AddBuffer<Intrinsic>();
                AddBuffer<ModifyIntrinsic>();
                AddBuffer<ModifyIntrinsicEvent>();
            }
        }
    }
}

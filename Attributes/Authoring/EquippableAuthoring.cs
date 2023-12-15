using Spark.Status.Components.Items;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.Authoring
{
    public class EquippableAuthoring : MonoBehaviour
    {
        public class AuthoringBaker : Baker<EquippableAuthoring>
        {
            public override void Bake(EquippableAuthoring authoring)
            {
                AddComponent<EquippedToEntity>();
            }
        }
    }
}

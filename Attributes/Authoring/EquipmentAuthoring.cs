using Spark.Status.Components.Items;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.Authoring
{
    public class EquipmentAuthoring : MonoBehaviour
    {
        public class AuthoringBaker : Baker<EquipmentAuthoring>
        {
            public override void Bake(EquipmentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<Equipment>(entity);
            }
        }
    }
}

using Schnozzle.GameSpace.Components;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Authoring
{
    public class AgentDataReferenceAuthoring : MonoBehaviour
    {
        public class AuthoringBaker : Baker<AgentDataReferenceAuthoring>
        {
            public override void Bake(AgentDataReferenceAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent<AgentDataReference>(entity);
            }
        }
    }
}

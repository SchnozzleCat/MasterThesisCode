using Schnozzle.GameSpace.Components;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Authoring
{
    public class AgentDataEntryAuthoring : MonoBehaviour
    {
        public class AuthoringBaker : Baker<AgentDataEntryAuthoring>
        {
            public override void Bake(AgentDataEntryAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<AgentDataEntry>(entity);
            }
        }
    }
}

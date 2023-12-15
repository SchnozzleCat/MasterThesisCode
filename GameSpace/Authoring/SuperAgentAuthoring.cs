using System.Collections;
using System.Collections.Generic;
using Schnozzle.GameSpace.Authoring;
using Schnozzle.GameSpace.Components;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Authoring
{
    public class SuperAgentAuthoring : MonoBehaviour
    {
        [SerializeField]
        private bool _hasDataReference;
        
        [SerializeField]
        private float _travelSpeed;

        [SerializeField] private GameSpaceNodeAuthoring _parent;

        public class AuthoringBaker : Baker<SuperAgentAuthoring>
        {
            public override void Bake(SuperAgentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                if (authoring._hasDataReference) AddBuffer<AgentDataEntry>(entity);
                AddComponent<SimulatedAgent>(entity);
                AddComponent<SuperAgent>(entity);
                AddBuffer<SuperAgentChild>(entity);
                AddComponent<GameSpaceParent>(entity, new GameSpaceParent {Entity = authoring._parent != null ? GetEntity(authoring._parent, TransformUsageFlags.None) : Entity.Null});
                if (authoring._travelSpeed > 0)
                {
                    AddComponent(entity, new TravelSpeed { Value = authoring._travelSpeed });
                    AddComponent<Pathfinder>(entity);
                    AddBuffer<Path>(entity);
                }
            }
        }
    }
}

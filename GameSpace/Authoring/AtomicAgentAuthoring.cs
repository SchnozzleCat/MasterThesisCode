using System;
using Schnozzle.GameSpace.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Authoring
{
    public class AtomicAgentAuthoring : MonoBehaviour
    {
        [SerializeField] public bool _hasDataReference;
        
        [SerializeField]
        public bool _collapser;

        [SerializeField]
        private int _collapseDistance;

        public class AuthoringBaker : Baker<AtomicAgentAuthoring>
        {
            public override void Bake(AtomicAgentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                
                if (authoring._hasDataReference)
                    AddComponent<AgentDataReference>(entity);
                AddComponent<SimulatedAgent>(entity);
                AddComponent<Atomic>(entity);
                AddComponent<GameSpaceParent>(entity);
                AddComponent<SuperAgentParent>(entity);
                if (authoring._collapser)
                    AddComponent(entity, new Collapser { CollapseDistance = authoring._collapseDistance });
            }
        }
    }
}

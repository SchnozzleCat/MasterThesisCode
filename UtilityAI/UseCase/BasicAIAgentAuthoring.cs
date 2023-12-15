using Schnozzle.AI.Authoring;
using Schnozzle.UseCase.Bootstrapping;
using Schnozzle.UseCase.Stats;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.UseCase
{
    public class BasicAIAgentAuthoring : AIAgentAuthoring<GameContext, BasicAIInputType, DecisionData>
    {
        [SerializeField]
        private int _maxHealth;

        public class AuthoringBaker : AIAgentAuthoringBaker<BasicAIAgentAuthoring, GameContext, BasicAIInputType, DecisionData>
        {
            public override void Bake(BasicAIAgentAuthoring authoring)
            {
                base.Bake(authoring);

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Health { Value = authoring._maxHealth });
                AddComponent(entity, new MaxHealth { Value = authoring._maxHealth });
                AddComponent(entity, new CombatData { Ammo = 10 });
                AddBuffer<DamageBuffer>(entity);
            }
        }
    }
}

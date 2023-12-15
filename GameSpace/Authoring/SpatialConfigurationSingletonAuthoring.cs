using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Components.Singleton
{
    public class SpatialConfigurationSingletonAuthoring : MonoBehaviour
    {
        [SerializeField]
        private bool _disableAutoLocation;

        public class AuthoringBaker : Baker<SpatialConfigurationSingletonAuthoring>
        {
            public override void Bake(SpatialConfigurationSingletonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(
                    entity,
                    new SpatialConfigurationSingleton { DisableAutoLocation = authoring._disableAutoLocation }
                );
            }
        }
    }
}

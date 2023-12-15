using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Components.Singleton;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Authoring
{
    public class ConfigurationSingletonAuthoring : MonoBehaviour
    {
        [SerializeField]
        private ConfigurationSingleton _configuration = new() { MaxCatchUpDeltaTime = 1 };

        public class AuthoringBaker : Baker<ConfigurationSingletonAuthoring>
        {
            public override void Bake(ConfigurationSingletonAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), authoring._configuration);
            }
        }
    }
}

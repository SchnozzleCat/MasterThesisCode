using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Components.Singleton
{
    public struct SpatialConfigurationSingleton : IComponentData
    {
        public bool DisableAutoLocation;
    }
}

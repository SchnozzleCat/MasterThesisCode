using System;
using Unity.Entities;

namespace Schnozzle.GameSpace.Components.Singleton
{
    [Serializable]
    public struct ConfigurationSingleton : IComponentData
    {
        /// <summary>
        ///     The maximum time in seconds that an agent can simulate per catch-up step.
        /// </summary>
        public float MaxCatchUpDeltaTime;
    }
}

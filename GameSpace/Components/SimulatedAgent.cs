using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    public struct SimulatedAgent : IComponentData
    {
        public float LastSimulationTime;
        public float CurrentDeltaTime;
    }
}

using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    /// <summary>
    /// Specifies that an agent has collapsed. This means it will stop simulating, run its collapse code, and wait until it un-collapses,
    /// at which point it will run its un-collapse code and continue simulating again.
    /// The enabled bit is used to schedule collapse systems running.
    /// </summary>
    public struct CollapsedAgent : IComponentData { }
}

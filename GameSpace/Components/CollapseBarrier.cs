using Unity.Entities;
using UnityEngine;

namespace Schnozzle.GameSpace.Components
{
    /// <summary>
    /// Signifies that this game space node halts collapsing upwards or downwards.
    /// For example, a room without windows would not need to be collapsed downwards if
    /// the player is outside, and it would not need to be collapsed upwards if the player is inside.
    /// </summary>
    public struct CollapseBarrier : IComponentData { }
}

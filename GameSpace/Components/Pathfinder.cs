using System.Collections.Generic;
using Schnozzle.GameSpace.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    public struct Pathfinder : IComponentData
    {
        public static Pathfinder FromTo(Entity from, Entity to) =>
            new()
            {
                CurrentState = State.Request,
                Start = from,
                End = to
            };

        public enum State
        {
            None,
            Request,
            Success,
            FailureStartNodeNotValid,
            FailureEndNodeNotValid,
            FailureEmptySet
        }

        /// <summary>
        /// The current state of this pathfinder.
        /// </summary>
        public State CurrentState;

        /// <summary>
        /// The starting and ending game space node entities.
        /// </summary>
        public Entity Start,
            End;
    }
}

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Schnozzle.GameSpace.Components.Singleton
{
    /// <summary>
    ///     Component used to store the state of the catch up simulation.
    /// </summary>
    public struct CatchUpSimulationStateSingleton : IComponentData
    {
        /// <summary>
        ///     This list is written from a job to check if we need another simulation run because some agents have not caught up
        ///     yet for this frame.
        ///     If this set is empty, we are finished running the catch-up loop.
        ///     This is using a list instead of something else to avoid lock contention while running a job.
        /// </summary>
        public NativeArray<byte> RequiresSimulationRunForThreadIndex;

        public bool RequiresCatchUpSimulation()
        {
            // We could increase performance very marginally here by using UnsafeUtility.MemCmp here, but this would
            // require storing a second, empty array.
            foreach (var element in RequiresSimulationRunForThreadIndex)
                if (element != 0)
                    return true;

            return false;
        }

        public unsafe void ResetState()
        {
            UnsafeUtility.MemClear(
                RequiresSimulationRunForThreadIndex.GetUnsafePtr(),
                RequiresSimulationRunForThreadIndex.Length
            );
        }
    }
}

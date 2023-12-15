using Schnozzle.GameSpace.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace Schnozzle.GameSpace.Jobs
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [WithNone(typeof(Paused))]
    internal partial struct FindAgentsToSimulateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public TimeData Time;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> RequiresSimulationRunForThreadIndex;

        [NativeSetThreadIndex]
        private int _threadIndex;

        //TODO: Replace this with a properly padded array in the future.
        private bool _requireSimulation;

        //TODO: Convert this to manual chunk iteration and process entities in batches for SIMD.
        public void Execute(ref SimulatedAgent agent, EnabledRefRW<Simulate> simulateEnabled)
        {
            agent.LastSimulationTime = math.select(
                agent.LastSimulationTime,
                (float)Time.ElapsedTime,
                agent.LastSimulationTime == 0
            );
            var simulationDelta = (float)Time.ElapsedTime - agent.LastSimulationTime;
            if (simulationDelta > 0) // This agent needs to be simulated.
            {
                // Enable this agent to simulate in subsequent systems.
                simulateEnabled.ValueRW = true;
                if (Time.DeltaTime > simulationDelta) // We need to simulate less than the current delta time step.
                {
                    agent.CurrentDeltaTime = simulationDelta;
                    agent.LastSimulationTime = (float)Time.ElapsedTime;
                }
                else // Simulate the entire time step.
                {
                    agent.CurrentDeltaTime = Time.DeltaTime;
                    agent.LastSimulationTime += Time.DeltaTime;
                    // Alert the AgentSimulationSystemGroup that we require an additional simulation step after this one.
                    _requireSimulation = true;
                    
            if (_requireSimulation)
                RequiresSimulationRunForThreadIndex[_threadIndex] = 1;
                }
            }
        }

        public bool OnChunkBegin(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            _requireSimulation = false;
            return true;
        }

        public void OnChunkEnd(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted
        )
        {
        }
    }
}

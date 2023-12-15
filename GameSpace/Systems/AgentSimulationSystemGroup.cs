using System;
using System.Threading;
using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Components.Singleton;
using Schnozzle.GameSpace.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Transforms;
using UnityEngine;

namespace GameSpace.Systems
{
    /// <summary>
    ///     Rate manager that facilitates the catch up simulation for agents that have not been simulated for some time.
    /// </summary>
    internal class AgentCatchUpSimulationRateManager : IRateManager
    {
        private readonly EntityQuery _catchUpStateSingletonQuery;
        private readonly EntityQuery _configurationSingletonQuery;
        private readonly EntityQuery _simulateAgentsQuery;

        private bool _isFirstRunThisFrame = true;

        internal AgentCatchUpSimulationRateManager(ComponentSystemGroup group)
        {
            _configurationSingletonQuery = group.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<ConfigurationSingleton>()
            );
            _catchUpStateSingletonQuery = group.EntityManager.CreateEntityQuery(
                ComponentType.ReadWrite<CatchUpSimulationStateSingleton>()
            );
            _simulateAgentsQuery = group.EntityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ReadWrite<Simulate>(), ComponentType.ReadOnly<SimulatedAgent>() },
                    Options = EntityQueryOptions.IgnoreComponentEnabledState
                }
            );
        }

        public bool ShouldGroupUpdate(ComponentSystemGroup group)
        {
            // Get configuration and time.
            ref readonly var time = ref group.World.Time;
            var configuration = _configurationSingletonQuery.GetSingleton<ConfigurationSingleton>();

            // Complete any previous jobs writing the singleton.
            _catchUpStateSingletonQuery.CompleteDependency();
            ref var catchUpState = ref _catchUpStateSingletonQuery
                .GetSingletonRW<CatchUpSimulationStateSingleton>()
                .ValueRW;

            // Ensure the state hash set has enough capacity for concurrent writing.
            var simulateThreads = JobsUtility.JobWorkerMaximumCount * 2;
            if (catchUpState.RequiresSimulationRunForThreadIndex.Length < simulateThreads)
            {
                catchUpState.RequiresSimulationRunForThreadIndex.Dispose();
                catchUpState.RequiresSimulationRunForThreadIndex = new NativeArray<byte>(
                    simulateThreads,
                    Allocator.Persistent
                );
            }

            if (_isFirstRunThisFrame)
            {
                // Push time to world. Elapsed time is set to current elapsed time as this value is different per entity anyways.
                group.World.PushTime(new TimeData(time.ElapsedTime, configuration.MaxCatchUpDeltaTime));

                // Disable simulation for all agents. They are selectively re-enabled in the first job that runs in this system.
                group.EntityManager.SetComponentEnabled<Simulate>(_simulateAgentsQuery, false);
                // Clear set for next simulation run this frame.
                catchUpState.ResetState();
                _isFirstRunThisFrame = false;
                return true;
            }

            // We require additional runs.
            // ReSharper disable once UseMethodAny.0
            if (catchUpState.RequiresCatchUpSimulation())
            {
                // Disable simulation for all agents. They are selectively re-enabled in the first job that runs in this system.
                group.EntityManager.SetComponentEnabled<Simulate>(_simulateAgentsQuery, false);
                // Clear set for next simulation run this frame.
                catchUpState.ResetState();
                return true;
            }

            // Finalize and stop the update loop.
            // Re-enable simulation for all agents.
            group.EntityManager.SetComponentEnabled<Simulate>(_simulateAgentsQuery, true);
            group.World.PopTime();
            _isFirstRunThisFrame = true;
            return false;
        }

        /// <summary>
        ///     Unsupported.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public float Timestep
        {
            get =>
                throw new NotImplementedException(
                    $"This property cannot be used on {nameof(AgentCatchUpSimulationRateManager)} as time is set via the configuration singleton."
                );
            set =>
                throw new NotImplementedException(
                    $"This property cannot be used on {nameof(AgentCatchUpSimulationRateManager)} as time is set via the configuration singleton."
                );
        }
    }

    /// <summary>
    ///     The simulation system group where systems that act on agents should be placed.
    ///     Systems in this group automatically catch any agents up when simulating if they haven't been simulated for some
    ///     time.
    /// </summary>
    public partial class AgentSimulationSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<ConfigurationSingleton>();
            RequireForUpdate<CatchUpSimulationStateSingleton>();

            RateManager = new AgentCatchUpSimulationRateManager(this);
        }
    }

    /// <summary>
    ///     Debugging System to spawn agents.
    ///     TODO: Remove this or move it somewhere else.
    /// </summary>
    internal partial struct SpawnAgentSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Break();
                var entity = state.EntityManager.CreateEntity(
                    typeof(SimulatedAgent),
                    typeof(LocalTransform)
                );

                ref readonly var time = ref SystemAPI.Time;

                SystemAPI.SetComponent(
                    entity,
                    new SimulatedAgent { LastSimulationTime = (float)time.ElapsedTime - 10 }
                );
                SystemAPI.SetComponent(entity, LocalTransform.FromPosition(0, 0, 0));

                state.EntityManager.Instantiate(entity, 99999, state.WorldUpdateAllocator);

                Debug.Log("Spawning Agents...");
            }
        }
    }

    /// <summary>
    ///     Debugging system to show at which frame the agent simulation system group ran.
    ///     TODO: Remove this or move it somewhere else.
    /// </summary>
    [UpdateInGroup(typeof(AgentSimulationSystemGroup))]
    [BurstCompile]
    internal partial struct DebugAgentSimulationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            Thread.Sleep(TimeSpan.FromTicks(1000));
        }
    }

    /// <summary>
    ///     System that finds agents that need to be simulated this time step.
    ///     If any agents are found that are not caught up completely, this system will modify the state of the
    ///     <see cref="CatchUpSimulationStateSingleton" /> so that the <see cref="AgentCatchUpSimulationRateManager" /> knows
    ///     to keep simulating for this frame.
    /// </summary>
    [UpdateInGroup(typeof(AgentSimulationSystemGroup), OrderFirst = true)]
    internal partial struct FindAgentsToSimulateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CatchUpSimulationStateSingleton>();

            state.EntityManager.CreateSingleton(
                new CatchUpSimulationStateSingleton
                {
                    RequiresSimulationRunForThreadIndex = new NativeArray<byte>(
                        JobsUtility.JobWorkerMaximumCount + 1,
                        Allocator.Persistent
                    )
                }
            );
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            SystemAPI.GetSingleton<CatchUpSimulationStateSingleton>().RequiresSimulationRunForThreadIndex.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FindAgentsToSimulateJob
            {
                Time = SystemAPI.Time,
                RequiresSimulationRunForThreadIndex = SystemAPI
                    .GetSingletonRW<CatchUpSimulationStateSingleton>()
                    .ValueRW.RequiresSimulationRunForThreadIndex
            }.ScheduleParallel(state.Dependency);
        }
    }
}

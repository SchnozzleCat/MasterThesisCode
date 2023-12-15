using GameSpace.Systems;
using NUnit.Framework;
using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Components.Singleton;
using Schnozzle.GameSpace.Systems;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using Unity.Entities;

namespace Schnozzle.GameSpace.Tests
{
    internal partial class TestAgentSimulation : GameSpaceTest
    {
        private struct TestComponent : IComponentData
        {
            public int RunCount;

            public UnsafeHashMap<float, int> SimulatedTimes;
        }

        [WithAll(typeof(Simulate))]
        private partial struct TestJob : IJobEntity
        {
            public void Execute(ref TestComponent component, in SimulatedAgent simulation)
            {
                component.RunCount++;

                if (!component.SimulatedTimes.TryAdd(simulation.CurrentDeltaTime, 1))
                    component.SimulatedTimes[simulation.CurrentDeltaTime]++;
            }
        }

        [DisableAutoCreation]
        [UpdateInGroup(typeof(AgentSimulationSystemGroup))]
        private partial struct TestSystem : ISystem
        {
            public void OnCreate(ref SystemState state)
            {
                state.RequireForUpdate<TestComponent>();
            }

            public void OnUpdate(ref SystemState state)
            {
                state.Dependency = new TestJob().ScheduleParallel(state.Dependency);
            }
        }

        [Test]
        public void TestAgentIsSimulatedCorrectNumberOfTimesAtCorrectDeltaTimes()
        {
            // -- Arrange
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(
                World,
                typeof(AgentSimulationSystemGroup),
                typeof(TestSystem),
                typeof(FindAgentsToSimulateSystem),
                typeof(EndAgentSimulationEntityCommandBufferSystem)
            );

            var config = EntityManager.CreateEntity(ComponentType.ReadWrite<ConfigurationSingleton>());
            EntityManager.SetComponentData(config, new ConfigurationSingleton { MaxCatchUpDeltaTime = 1 });

            // We are expecting 5 simulations at delta time 1 and one simulation at delta time 0.25.
            // Use 1/4 as the remainder here as it can accurately be represented by a floating point number.
            World.SetTime(new TimeData(5.25f, 0.1f));

            var entity = EntityManager.CreateEntity(
                ComponentType.ReadWrite<SimulatedAgent>(),
                ComponentType.ReadWrite<TestComponent>(),
                ComponentType.ReadWrite<Simulate>()
            );
            EntityManager.SetComponentData(
                entity,
                new TestComponent { SimulatedTimes = new UnsafeHashMap<float, int>(0, Allocator.Persistent) }
            );

            // -- Act
            World.Update();

            // -- Assert
            var simulation = EntityManager.GetComponentData<SimulatedAgent>(entity);
            Assert.AreEqual(simulation.LastSimulationTime, World.Time.ElapsedTime);

            var test = EntityManager.GetComponentData<TestComponent>(entity);
            Assert.AreEqual(test.RunCount, 6);
            Assert.AreEqual(test.SimulatedTimes[1.0f], 5);
            Assert.AreEqual(test.SimulatedTimes[0.25f], 1);

            // -- Cleanup
            test.SimulatedTimes.Dispose();
        }
    }
}

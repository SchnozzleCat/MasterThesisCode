using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.LowLevel;

namespace Schnozzle.GameSpace.Tests
{
    internal abstract class GameSpaceTest
    {
        // Fields to store previous state and reset in teardown.
        private World _previousWorld;
        private EntityManager.EntityManagerDebug _entityManagerDebug;
        private PlayerLoopSystem _playerLoopSystem;
        private bool _debuggingJobs;

        // Easy access for tests.
        protected World World { get; private set; }
        protected EntityManager EntityManager { get; private set; }

        [SetUp]
        public virtual void Setup()
        {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !DISABLE_ENTITIES_JOURNALING
            // Reset journaling so we only have the results from the test.
            EntitiesJournaling.Clear();
#endif
            // Manage player loop for test.
            _playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());

            // Create test world and initialize members.
            _previousWorld = World.DefaultGameObjectInjectionWorld;
            World = new World("Testing World");
            World.UpdateAllocatorEnableBlockFree = true;
            World.DefaultGameObjectInjectionWorld = World;
            EntityManager = World.EntityManager;
            _entityManagerDebug = new EntityManager.EntityManagerDebug(EntityManager);

            // Force enabling of job debugger in case the current editor session does not have it enabled.
            _debuggingJobs = JobsUtility.JobDebuggerEnabled;
            JobsUtility.JobDebuggerEnabled = true;
        }

        [TearDown]
        public virtual void TearDown()
        {
            // Make sure systems are destroyed.
            while (World.Systems.Count > 0)
                World.DestroySystemManaged(World.Systems[0]);

            // Check consistency and dispose test world.
            _entityManagerDebug.CheckInternalConsistency();
            World.Dispose();

            // Restore previous state.
            World.DefaultGameObjectInjectionWorld = _previousWorld!;
            JobsUtility.JobDebuggerEnabled = _debuggingJobs;
            PlayerLoop.SetPlayerLoop(_playerLoopSystem);
        }
    }
}

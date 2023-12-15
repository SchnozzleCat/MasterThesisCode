using System;
using System.Linq;
using Schnozzle.Core.Extensions;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.Core.SchnozzleObject.Interfaces;
using Spark.Status.Components;
using Spark.Status.SchnozzleObjects;
using Unity.Collections;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace Spark.Status.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(StatusSystemGroup))]
    public partial struct IndexerSystem : ISystem, ISystemStartStop
    {
        public struct HasInitializedStatusIndices : IComponentData { }

        public struct State : ICleanupComponentData, IDisposable
        {
            public NativeArray<SchnozzleReference<StatSettings>> OrderedStats;
            public NativeArray<SchnozzleReference<IntrinsicSettings>> OrderedIntrinsics;
            public NativeHashMap<SchnozzleReference<StatSettings>, int> StatIndices;
            public NativeHashMap<SchnozzleReference<IntrinsicSettings>, int> IntrinsicIndices;

            public int ReachStatIndex;
            public int MoveSpeedStatIndex;
            public int RecoveryIntrinsicIndex;

            private bool _hasInitialized;

            public bool InitializeSpecificStatusIndices(SpecificStatusSingleton singleton)
            {
                if (_hasInitialized)
                    return false;

                ReachStatIndex = OrderedStats.IndexOf(singleton.ReachStat);
                MoveSpeedStatIndex = OrderedStats.IndexOf(singleton.MoveSpeedStat);
                RecoveryIntrinsicIndex = OrderedIntrinsics.IndexOf(singleton.RecoveryIntrinsic);

                _hasInitialized = true;

                return true;
            }

            public void Dispose()
            {
                OrderedStats.Dispose();
                OrderedIntrinsics.Dispose();
                StatIndices.Dispose();
                IntrinsicIndices.Dispose();
            }
        }

        public void OnCreate(ref SystemState state)
        {
            var stats = StatSettings.GetAll().Ordered();
            var intrinsics = IntrinsicSettings.GetAll().Ordered();

            var orderedStats = new NativeArray<SchnozzleReference<StatSettings>>(stats.Count(), Allocator.Persistent);
            var orderedIntrinsics = new NativeArray<SchnozzleReference<IntrinsicSettings>>(
                intrinsics.Count(),
                Allocator.Persistent
            );
            var statIndices = new NativeHashMap<SchnozzleReference<StatSettings>, int>(
                stats.Count(),
                Allocator.Persistent
            );
            var intrinsicIndices = new NativeHashMap<SchnozzleReference<IntrinsicSettings>, int>(
                intrinsics.Count(),
                Allocator.Persistent
            );

            foreach (var (stat, index) in stats.WithIndex())
            {
                orderedStats[index] = stat.AsRef;
                statIndices[stat.AsRef] = stat.Index;
            }

            foreach (var (intrinsic, index) in intrinsics.WithIndex())
            {
                orderedIntrinsics[index] = intrinsic.AsRef;
                intrinsicIndices[intrinsic.AsRef] = intrinsic.Index;
            }

            state.EntityManager.AddComponentData(
                state.SystemHandle,
                new State
                {
                    OrderedStats = orderedStats,
                    StatIndices = statIndices,
                    OrderedIntrinsics = orderedIntrinsics,
                    IntrinsicIndices = intrinsicIndices
                }
            );

            state.RequireForUpdate<SpecificStatusSingleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            if (
                GetComponentRW<State>(state.SystemHandle).ValueRW.InitializeSpecificStatusIndices(
                    GetSingleton<SpecificStatusSingleton>()
                )
            )
            {
                state.EntityManager.AddComponentData(state.SystemHandle, new HasInitializedStatusIndices());
            }
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnDestroy(ref SystemState state)
        {
            GetComponent<State>(state.SystemHandle).Dispose();
            state.EntityManager.RemoveComponent<State>(state.SystemHandle);
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
        }
    }
}

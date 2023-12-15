using Interfaces;
using Schnozzle.GameSpace.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Schnozzle.GameSpace.Systems
{
    [UpdateInGroup(typeof(CollapseSystemGroup))]
    public abstract partial class CollapseSystem<TComponent> : SystemBase
        where TComponent : unmanaged, IComponentData
    {
        private EntityQuery _collapseQuery;
        private EntityQuery _uncollapseQuery;

        protected override void OnCreate()
        {
            _collapseQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TComponent, Collapsing>()
                .Build(ref CheckedStateRef);
            _uncollapseQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TComponent, Decollapsing>()
                .Build(ref CheckedStateRef);

            RequireAnyForUpdate(_collapseQuery, _uncollapseQuery);
        }

        protected override void OnUpdate()
        {
            Dependency = ScheduleCollapseJobs(Dependency);
        }

        protected JobHandle ScheduleCollapseJobs(JobHandle handle)
        {
            if (!_collapseQuery.IsEmptyIgnoreFilter)
                handle = ScheduleCollapseJob(handle);

            if (!_uncollapseQuery.IsEmptyIgnoreFilter)
                handle = ScheduleUncollapseJob(handle);

            return handle;
        }

        protected abstract JobHandle ScheduleCollapseJob(JobHandle handle);
        protected abstract JobHandle ScheduleUncollapseJob(JobHandle handle);
    }

    [UpdateInGroup(typeof(CollapseSystemGroup))]
    public partial struct CollapseSystem<TComponent, TCollapse, TUncollapse, TSystemState> : ISystem, ISystemStartStop
        where TComponent : unmanaged, IComponentData
        where TCollapse : unmanaged, IJobEntity
        where TUncollapse : unmanaged, IJobEntity
        where TSystemState : unmanaged, ICollapseSystemState<TComponent, TCollapse, TUncollapse>
    {
        private EntityQuery _collapseQuery;
        private EntityQuery _uncollapseQuery;
        private EntityQuery _stateQuery;

        private TSystemState _systemState;
        private bool _initialized;

        public void OnCreate(ref SystemState state)
        {
            _collapseQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<TComponent, Collapsing>().Build(ref state);
            _uncollapseQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TComponent, Decollapsing>()
                .Build(ref state);

            _systemState = new TSystemState();
            _stateQuery = _systemState.RequireForUpdate(ref state);

            state.RequireAnyForUpdate(_collapseQuery, _uncollapseQuery);
            state.RequireForUpdate(_stateQuery);
        }

        public void OnStartRunning(ref SystemState state)
        {
            if (_initialized)
                return;
            _initialized = true;

            _systemState.OnCreate(ref state);
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnDestroy(ref SystemState state)
        {
            _systemState.OnDestroy(ref state);
            _stateQuery.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            _systemState.OnUpdate(ref state);

            if (!_collapseQuery.IsEmptyIgnoreFilter)
                state.Dependency = _systemState.CollapseJob(ref state).ScheduleParallel(state.Dependency);

            if (!_uncollapseQuery.IsEmptyIgnoreFilter)
                state.Dependency = _systemState.UncollapseJob(ref state).ScheduleParallel(state.Dependency);
        }
    }
}

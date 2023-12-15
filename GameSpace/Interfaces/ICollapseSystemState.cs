using System;
using Unity.Collections;
using Unity.Entities;

namespace Interfaces
{
    public interface ICollapseSystemState<TComponent, TCollapse, TUncollapse>
        where TComponent : unmanaged, IComponentData
        where TCollapse : unmanaged, IJobEntity
        where TUncollapse : unmanaged, IJobEntity
    {
        public void OnCreate(ref SystemState state);

        public void OnDestroy(ref SystemState state);

        public void OnUpdate(ref SystemState state);

        public EntityQuery RequireForUpdate(ref SystemState state)
        {
            return new EntityQueryBuilder(Allocator.Temp).Build(ref state);
        }

        public TCollapse CollapseJob(ref SystemState state);
        public TUncollapse UncollapseJob(ref SystemState state);
    }
}

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Schnozzle.GameSpace.Authoring
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct AssignParentBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI
                .QueryBuilder()
                .WithAll<GameSpaceNodeAuthoring.BakedParentData>()
                .Build();

            state.EntityManager.AddComponent<Parent>(query);
            state.EntityManager.AddComponent<Child>(query);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (
                var (managed, parent, entity) in SystemAPI.Query<
                    GameSpaceNodeAuthoring.BakedParentData,
                    RefRW<Parent>
                >().WithEntityAccess()
            )
            {
                parent.ValueRW.Value = managed.Parent;
                if (managed.Parent != Entity.Null)
                    state.EntityManager.GetBuffer<Child>(managed.Parent).Add(new Child{Value = entity});
                else ecb.RemoveComponent<Parent>(entity);
            }
            
            ecb.Playback(state.EntityManager);

            state.EntityManager.RemoveComponent<GameSpaceNodeAuthoring.BakedParentData>(query);
        }
    }
}

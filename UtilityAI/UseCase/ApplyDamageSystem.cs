using Unity.Burst;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace Schnozzle.UseCase
{
    [BurstCompile]
    [WithChangeFilter(typeof(DamageBuffer))]
    internal partial struct ApplyDamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(
            Entity entity,
            ref DynamicBuffer<DamageBuffer> incomingDamage,
            ref Health health,
            [ChunkIndexInQuery] int chunkIndexInQuery
        )
        {
            foreach (var damage in incomingDamage)
                health.Value -= damage.Value;

            if (health.Value <= 0)
                CommandBuffer.DestroyEntity(chunkIndexInQuery, entity);

            incomingDamage.Clear();
        }
    }

    public partial struct ApplyDamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ApplyDamageJob
            {
                CommandBuffer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged)
                    .AsParallelWriter(),
            }.ScheduleParallel(state.Dependency);
        }
    }
}

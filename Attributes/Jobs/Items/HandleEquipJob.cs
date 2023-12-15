using Spark.Status.Components;
using Spark.Status.Components.Items;
using Spark.Status.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Spark.Status.Jobs.Items
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    internal partial struct HandleEquipJob : IJobEntity
    {
        public BufferLookup<Equipment> EquipmentBufferLookup;
        public ComponentLookup<RequireStatCalculation> RequireStatCalculationLookup;

        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, ref EquippedToEntity equipped, ref EquipToEntity equip)
        {
            EquipmentSystem.Equip(
                ref CommandBuffer,
                entity,
                ref EquipmentBufferLookup,
                ref RequireStatCalculationLookup,
                ref equipped,
                ref equip
            );
        }
    }
}

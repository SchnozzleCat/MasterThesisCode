using System;
using System.Runtime.CompilerServices;
using Schnozzle.Core.Console;
using Spark.Status.Components;
using Spark.Status.Components.Items;
using Spark.Status.Jobs.Items;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Spark.Status.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(StatusSystemGroup))]
    [CommandPrefix("Equipment")]
    public partial struct EquipmentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(
                QueryBuilder().WithAllRW<EquippedToEntity>().WithAnyRW<EquipToEntity, UnequipFromEntity>().Build()
            );
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new HandleEquipJob
            {
                EquipmentBufferLookup = GetBufferLookup<Equipment>(),
                RequireStatCalculationLookup = GetComponentLookup<RequireStatCalculation>(),
                CommandBuffer = ecb
            }.Schedule(state.Dependency);

            state.Dependency = new HandleUnequipJob
            {
                EquipmentBufferLookup = GetBufferLookup<Equipment>(),
                RequireStatCalculationLookup = GetComponentLookup<RequireStatCalculation>(),
                CommandBuffer = ecb
            }.Schedule(state.Dependency);
        }

        [Schnozzle.ECS.Console.ConsoleCommand]
        public static void EquipToEntity(EntityManager entityManager, Entity item, Entity target)
        {
            entityManager.AddComponentData(item, new EquipToEntity { Target = target });
        }

        [Schnozzle.ECS.Console.ConsoleCommand]
        public static void Unequip(EntityManager entityManager, Entity item)
        {
            entityManager.AddComponent<UnequipFromEntity>(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Unequip(
            ref EntityCommandBuffer ecb,
            Entity item,
            ref BufferLookup<Equipment> equipmentBufferLookup,
            ref ComponentLookup<RequireStatCalculation> requireStatCalculationLookup,
            ref EquippedToEntity equipped
        )
        {
            ecb.RemoveComponent<UnequipFromEntity>(item);

            if (equipped.Target == Entity.Null)
            {
                Debug.LogWarning("Attempted to unequip an item that is not equipped...");
                return;
            }

            var index = -1;

            var equipment = equipmentBufferLookup[equipped.Target];

            for (int i = 0; i < equipment.Length; i++)
                if (equipment[i].Item == item)
                    index = i;

            if (index == -1)
                throw new Exception("Could not unequip item!");

            equipment.RemoveAt(index);

            requireStatCalculationLookup.SetComponentEnabled(equipped.Target, true);

            equipped.Target = Entity.Null;
            ecb.SetComponentEnabled<EquippedToEntity>(item, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Equip(
            ref EntityCommandBuffer ecb,
            Entity item,
            ref BufferLookup<Equipment> equipmentBufferLookup,
            ref ComponentLookup<RequireStatCalculation> requireStatCalculationLookup,
            ref EquippedToEntity equipped,
            ref EquipToEntity equip
        )
        {
            ecb.RemoveComponent<EquipToEntity>(item);

            if (equip.Target == Entity.Null)
                return;

            var equipment = equipmentBufferLookup[equip.Target];

            if (equipment.HasItemInSlot(equip.TargetSlot.Slot))
                return;

            equipment.Add(new Equipment { Item = item, TargetSlotIndex = equip.TargetSlot.Slot });

            requireStatCalculationLookup.SetComponentEnabled(equip.Target, true);

            equipped.Target = equip.Target;
            ecb.SetComponentEnabled<EquippedToEntity>(item, true);
        }
    }
}

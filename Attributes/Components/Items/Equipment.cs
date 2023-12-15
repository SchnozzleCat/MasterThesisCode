using System;
using Unity.Entities;
using Unity.NetCode;

namespace Spark.Status.Components.Items
{
    [InternalBufferCapacity(0)]
    public struct Equipment : IBufferElementData, IEquatable<Equipment>
    {
        [GhostField]
        public Entity Item;

        [GhostField]
        public byte TargetSlotIndex;

        public bool Equals(Equipment other) => other.Item == Item;
    }

    public static class EquipmentExtensions
    {
        public static Entity ItemInSlot(this in DynamicBuffer<Equipment> equipmentBuffer, byte slot)
        {
            for (int i = 0; i < equipmentBuffer.Length; i++)
                if (equipmentBuffer[i].TargetSlotIndex == slot)
                    return equipmentBuffer[i].Item;

            return Entity.Null;
        }

        public static bool HasItemInSlot(this in DynamicBuffer<Equipment> equipmentBuffer, byte slot) =>
            equipmentBuffer.ItemInSlot(slot) != Entity.Null;
    }
}

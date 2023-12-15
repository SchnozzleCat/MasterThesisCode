using Unity.Entities;
using UnityEngine.Assertions;

namespace Spark.Status.Components.Items
{
    public struct EquipToEntity : IComponentData
    {
        public struct EquipmentSlot
        {
            private const int AutoIndex = -1;

            public static EquipmentSlot Auto => new() { _slot = AutoIndex };

            private int _slot;
            public bool IsAuto => _slot == AutoIndex;

            public byte Slot
            {
                get
                {
                    Assert.IsTrue(_slot >= byte.MinValue && _slot <= byte.MaxValue);
                    return (byte)_slot;
                }
            }

            public static implicit operator EquipmentSlot(byte slot) => new() { _slot = slot };

            public static implicit operator byte(EquipmentSlot slot) => slot.Slot;
        }

        public Entity Target;

        public EquipmentSlot TargetSlot;
    }
}

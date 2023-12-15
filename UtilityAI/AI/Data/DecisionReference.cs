using System;
using Sirenix.Utilities.Unsafe;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Schnozzle.AI.Data
{
    public unsafe struct DecisionReference<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private void* _ptr;

        public DecisionReference(ref DecisionBlob<TExecutionContext, TEnum, TCustomData> decision)
        {
            _ptr = UnsafeUtility.AddressOf(ref decision);
        }

        public bool IsPanic => _ptr == null;

        public ref DecisionBlob<TExecutionContext, TEnum, TCustomData> Value
        {
            get
            {
                Assert.IsFalse(IsPanic);
                return ref UnsafeUtility.AsRef<DecisionBlob<TExecutionContext, TEnum, TCustomData>>(_ptr);
            }
        }
    }
}

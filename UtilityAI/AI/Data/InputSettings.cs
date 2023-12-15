using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Schnozzle.AI.Data
{
    public interface IInput<TExecutionContext, TEnum> : IInput
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
    {
        public InputHeader<TEnum> Header { get; set; }

        public float GetValue(in TExecutionContext context);
    }

    public interface IInput { }

    public static class IInputExtensions
    {
        public static float Clamp<T>(this ref T input, float value)
            where T : unmanaged, IInput => math.clamp(value, 0, 1);
    }

    public abstract class InputSettings<TBlob, TExecutionContext, TEnum> : InputSettings<TExecutionContext, TEnum>
        where TBlob : unmanaged, IInput<TExecutionContext, TEnum>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
    {
        public abstract void PopulateBlob(ref TBlob blob, ref BlobBuilder builder);

        internal override void CreateBlobPtr(ref BlobPtr<InputHeader<TEnum>> ptr, ref BlobBuilder builder)
        {
            ref var castPtr = ref UnsafeUtility.As<BlobPtr<InputHeader<TEnum>>, BlobPtr<TBlob>>(ref ptr);
            ref var blob = ref builder.Allocate(ref castPtr);
            blob.Header = new InputHeader<TEnum> { Type = Type };
            PopulateBlob(ref blob, ref builder);
        }
    }

    public abstract class InputSettings<TExecutionContext, TEnum>
        where TExecutionContext : IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
    {
        public abstract TEnum Type { get; }

        internal abstract void CreateBlobPtr(ref BlobPtr<InputHeader<TEnum>> ptr, ref BlobBuilder builder);
    }
}

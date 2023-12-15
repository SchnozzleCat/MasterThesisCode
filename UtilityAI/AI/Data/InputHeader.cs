using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Schnozzle.AI.Data
{
    public struct InputHeader<T>
        where T : unmanaged, Enum
    {
        public T Type;
    }

    public static class InputHeaderExtensions
    {
        public static float GetValue<TExecutionContext, TEnum, T>(
            this ref InputHeader<TEnum> header,
            in TExecutionContext context
        )
            where TExecutionContext : unmanaged, IExecutionContext<TEnum>
            where TEnum : unmanaged, Enum
            where T : unmanaged, IInput<TExecutionContext, TEnum> =>
            math.clamp(UnsafeUtility.As<InputHeader<TEnum>, T>(ref header).GetValue(in context), 0, 1);
    }
}

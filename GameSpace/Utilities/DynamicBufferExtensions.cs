using System;
using Unity.Entities;

namespace Schnozzle.GameSpace.Utilities
{
    public static class DynamicBufferExtensions
    {
        public static bool Remove<T>(this ref DynamicBuffer<T> buffer, in T item)
            where T : unmanaged, IEquatable<T>
        {
            var length = buffer.Length;
            for (int i = 0; i < length; i++)
            {
                if (buffer[i].Equals(item))
                {
                    buffer.RemoveAtSwapBack(i);
                    return true;
                }
            }

            return true;
        }
    }
}

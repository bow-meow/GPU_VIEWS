using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wgpu
{
    internal sealed class RentalStorage<T>
    {
        private readonly List<T?> _buffer = new();
        private readonly Stack<int> _freeList = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Rent(T item)
        {
            if (_freeList.TryPop(out int idx))
            {
                _buffer[idx] = item;
            }
            else
            {
                idx = _freeList.Count;
                _buffer.Add(item);
            }

            return idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Get(int key)
        {
            return _buffer[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetAndReturn(int key)
        {
            T? value = _buffer[key];
            _freeList.Push(key);
            _buffer[key] = default;
            return value;
        }
    }
}

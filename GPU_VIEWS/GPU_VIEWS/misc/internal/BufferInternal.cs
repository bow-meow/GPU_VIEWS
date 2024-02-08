using System;
using Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly struct BufferInternal
    {
        public BufferPtr Buffer { get; }
        public ulong BufferSize { get; }

        public BufferInternal(BufferPtr buffer, ulong size)
        {
            Buffer = buffer;
            BufferSize = size;
        }
    }
}
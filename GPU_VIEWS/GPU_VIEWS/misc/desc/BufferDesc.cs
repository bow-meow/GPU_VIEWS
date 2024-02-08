using Silk.NET.WebGPU;

namespace GPU_VIEWS.misc
{
    public readonly struct BufferDesc
    {
        public BufferUsage BufferUsage { get; }
        public ulong Size { get; }

        public BufferDesc(BufferUsage bufferUsage, ulong size)
        {
            BufferUsage = bufferUsage;
            Size = size;
        }
    }
}
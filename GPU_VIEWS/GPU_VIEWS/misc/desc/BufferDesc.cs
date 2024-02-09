using Silk.NET.WebGPU;

namespace GPU_VIEWS.misc
{
    public readonly struct BufferDesc
    {
        public BufferUsage BufferUsage { get; }
        public ulong Size { get; }

        public bool MappedAtCreation { get; }

        public BufferDesc(BufferUsage bufferUsage, ulong size, bool mappedAtCreation = false)
        {
            BufferUsage = bufferUsage;
            Size = size;
            MappedAtCreation = mappedAtCreation;
        }
    }
}
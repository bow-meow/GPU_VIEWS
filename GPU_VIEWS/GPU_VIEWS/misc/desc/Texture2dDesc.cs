using Silk.NET.WebGPU;

namespace GPU_VIEWS.misc
{
    public readonly struct Texture2dDesc
    {
        public TextureUsage TextureUsage { get; }
        public Extent3D Extent3D { get; }
        public TextureFormat TextureFormat { get; }

        public Texture2dDesc(TextureUsage usage, Extent3D size, TextureFormat format)
        {
            TextureUsage = usage;
            Extent3D = size;
            TextureFormat = format;
        }
    }
}
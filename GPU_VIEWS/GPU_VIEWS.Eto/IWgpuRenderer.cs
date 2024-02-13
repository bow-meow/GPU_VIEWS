using System;
using System.Runtime.CompilerServices;
using FontStashSharp.Interfaces;
using SixLabors.ImageSharp.PixelFormats;
using Wgpu;

namespace GPU_VIEWS.Eto
{
    public interface IWgpuRenderer
    {
        void Initialize(SixLabors.ImageSharp.Image<Rgba32> image);
        void Render(Action<RenderPassEncoderPtr> callback);
        ITextRenderer TextRenderer { get; }
    }
}
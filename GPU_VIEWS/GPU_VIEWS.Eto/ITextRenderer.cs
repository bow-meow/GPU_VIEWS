using FontStashSharp.Interfaces;
using SixLabors.ImageSharp.PixelFormats;
using Wgpu;

namespace GPU_VIEWS.Eto
{
    public interface ITextRenderer : IFontStashRenderer2
    {
        void Render(RenderPassEncoderPtr render_pass);
        void Initialize(SixLabors.ImageSharp.Image<Rgba32> image);
         RenderPassEncoderPtr RenderPass { get; set; }
    }
}
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace GPU_VIEWS.Eto
{
    public interface IWgpuRenderer
    {
        void Initialize(SixLabors.ImageSharp.Image<Rgba32> image);
        void Render();
    }
}
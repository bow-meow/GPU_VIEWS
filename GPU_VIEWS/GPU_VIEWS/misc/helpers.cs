using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace GPU_VIEWS.misc
{
    public static class AlignmentHelper
    {
        public static (uint bytes_per_row, uint padded_bytes_per_row) GetBufferDimensions(int width, int componentsPerPixel)
        {
            uint bytes_per_row = (uint)(componentsPerPixel * width);
            uint padded_bytes_per_row = 0;

            var padding = (256 - bytes_per_row % 256) % 256;

            padded_bytes_per_row = bytes_per_row + padding;

            return (bytes_per_row, padded_bytes_per_row);
        }
    }
}
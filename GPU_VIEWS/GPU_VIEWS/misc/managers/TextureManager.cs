using System;
using System.Drawing;
using FontStashSharp.Interfaces;
using GPU_VIEWS.misc;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp.PixelFormats;
using Wgpu;

namespace GPU_VIEWS.renderers
{
    public interface ITextureManager : ITexture2DManager
    {
        TextureInternal CreateTexture(Texture2dDesc textureDesc);
    }

    public class TextureManager : ITextureManager
    {
        private readonly DevicePtr _device;
        private readonly TextureFormat _format;
        public TextureManager(DevicePtr device, TextureFormat format)
        {
            _device = device;
            _format = format;
        }
        public TextureInternal CreateTexture(Texture2dDesc textureDesc)
        {
            var tex = _device.CreateTexture(
                usage: textureDesc.TextureUsage,
                dimension: TextureDimension.Dimension2D,
                size: textureDesc.Extent3D,
                format: textureDesc.TextureFormat,
                mipLevelCount: 1, // TODO:(ALEX) add mipmaps
                sampleCount: 1,
                new ReadOnlySpan<TextureFormat>(new[]{textureDesc.TextureFormat})

            );
            return new TextureInternal(tex);
        }

        #region  Font Rendering Methods
        public object CreateTexture(int width, int height)
        {
            return _device.CreateTexture(TextureUsage.TextureBinding,
            TextureDimension.Dimension2D,
            new Extent3D{
                Width = (uint)width,
                Height = (uint)height,
                DepthOrArrayLayers = 1
            }, _format,
            mipLevelCount: 1,
            sampleCount: 1,
            new ReadOnlySpan<TextureFormat>(new[]{_format}),
            "font_tex");
        }

        public Point GetTextureSize(object texture)
        {
            var tex = (TexturePtr)texture;
            
            return new Point((int)tex.GetWidth(), (int)tex.GetHeight());
        }

        public unsafe void SetTextureData(object texture, Rectangle bounds, byte[] data)
        {
            var tex = (TexturePtr)texture;

            var pixels = new Span<byte>(data);

            var queue = _device.GetQueue();

            queue.WriteTexture<byte>(new Wgpu.ImageCopyTexture
            {
                Texture = tex,
                Aspect = TextureAspect.All,
                MipLevel = 0,
                Origin = new Origin3D { X = 0, Y = 0, Z = 0 },
            },
            data: pixels,
            new TextureDataLayout
            {
                BytesPerRow = (uint)(sizeof(Rgba32) * bounds.Size.Width),
                RowsPerImage = (uint)bounds.Size.Height,
                Offset = 0,
            },
            new Extent3D
            {
                Width = (uint)bounds.Size.Width,
                Height = (uint)bounds.Size.Height,
                DepthOrArrayLayers = 1
            });
        }

        #endregion
    }
}
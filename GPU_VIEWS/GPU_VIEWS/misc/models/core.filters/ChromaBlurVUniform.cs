using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Runtime.CompilerServices;
using Wgpu;

namespace GPU_VIEWS.msic.models.core.filters
{
    public struct ChromaBlurVUniform : IUniform
    {
        public ChromaBlurVUniform(Vector4D<float> pixelSize, int radiusV)
        {
            PixelSize = pixelSize;
            RadiusV = radiusV;
        }

        public Vector4D<float> PixelSize { get; }
        public int RadiusV { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaBlurVUniform>() + sizeof(float) * 4);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaBlurVUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

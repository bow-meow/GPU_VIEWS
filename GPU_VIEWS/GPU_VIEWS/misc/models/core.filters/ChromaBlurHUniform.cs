using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace GPU_VIEWS.msic.models.core.filters
{
    public struct ChromaBlurHUniform : IUniform
    {
        public ChromaBlurHUniform(Vector4D<float> pixelSize, int radiusH)
        {
            this.pixelSize = pixelSize;
            this.radiusH = radiusH;
        }

        public Vector4D<float> pixelSize { get; }
        public int radiusH { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaBlurHUniform>() + sizeof(float) * 4);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaBlurHUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

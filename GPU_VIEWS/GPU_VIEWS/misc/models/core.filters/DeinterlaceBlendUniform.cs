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
    public struct DeinterlaceBlendUniform : IUniform
    {
        public DeinterlaceBlendUniform(Vector4D<float> pixelSize)
        {
            PixelSize = pixelSize;
        }

        public Vector4D<float> PixelSize { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<DeinterlaceBlendUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<DeinterlaceBlendUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

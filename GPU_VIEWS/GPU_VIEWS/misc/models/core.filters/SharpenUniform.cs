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

namespace WGPU_TEST.models.core.filters
{
    public struct SharpenUniform : IUniform
    {
        public SharpenUniform(Vector4D<float> pixelSize, float factor)
        {
            PixelSize = pixelSize;
            Factor = factor;
        }

        public Vector4D<float> PixelSize { get; }
        public float Factor { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<SharpenUniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<SharpenUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

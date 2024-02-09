using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using WGPU_TEST.models.core.filters;
using Wgpu;
using System.Runtime.CompilerServices;
using GPU_VIEWS.misc;

namespace GPU_VIEWS.msic.models.image
{
    public struct YvyuToRgbUniform : IUniform
    {
        public YvyuToRgbUniform(Vector4D<float> quadScreenSize)
        {
            QuadScreenSize = quadScreenSize;
        }

        public Vector4D<float> QuadScreenSize { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<YvyuToRgbUniform>() + sizeof(float));

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<YvyuToRgbUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

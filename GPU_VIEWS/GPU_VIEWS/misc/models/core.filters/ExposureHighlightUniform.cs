using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Wgpu;

namespace GPU_VIEWS.msic.models.core.filters
{
    public struct ExposureHighlightUniform : IUniform
    {
        public ExposureHighlightUniform(float tolerance, Vector4D<float> newWhite, Vector4D<float> newBlack)
        {
            Tolerance = tolerance;
            NewWhite = newWhite;
            NewBlack = newBlack;
        }

        public float Tolerance { get; }
        public Vector4D<float> NewWhite { get; }
        public Vector4D<float> NewBlack { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ExposureHighlightUniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ExposureHighlightUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

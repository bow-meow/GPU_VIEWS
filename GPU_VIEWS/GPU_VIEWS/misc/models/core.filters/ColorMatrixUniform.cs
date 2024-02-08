using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public struct ColorMatrixUniform : IUniform
    {
        public ColorMatrixUniform(Matrix4X4<float> colorMatrix)
        {
            ColorMatrix = colorMatrix;
        }

        public Matrix4X4<float> ColorMatrix { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<ColorMatrixUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ColorMatrixUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using WGPU_TEST.models.core.filters;
using Wgpu;

namespace WGPU_TEST.models.image
{
    public struct YvyuToRgbUniform : IBuffer
    {
        public YvyuToRgbUniform(Vector4D<float> quadScreenSize)
        {
            QuadScreenSize = quadScreenSize;
        }

        public Vector4D<float> QuadScreenSize { get; }

        public unsafe (BufferPtr, ulong buffer_size) CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(sizeof(YvyuToRgbUniform) + sizeof(float));

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<YvyuToRgbUniform>(new[] { this }));

            return (buffer, buffer_size);
        }
    }
}

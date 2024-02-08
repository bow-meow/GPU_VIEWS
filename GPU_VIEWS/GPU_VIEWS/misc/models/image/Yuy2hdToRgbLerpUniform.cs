using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGPU_TEST.models.core.filters;
using Wgpu;
using System.Runtime.CompilerServices;
using GPU_VIEWS.misc;

namespace WGPU_TEST.models.image
{
    public struct Yuy2hdToRgbLerpUniform : IUniform
    {
        public Yuy2hdToRgbLerpUniform(Vector4D<float> quadScreenSize, float quadTexOffset)
        {
            QuadScreenSize = quadScreenSize;
            QuadTexOffset = quadTexOffset;
        }

        public Vector4D<float> QuadScreenSize { get; }
        public float QuadTexOffset { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<Yuy2hdToRgbLerpUniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<Yuy2hdToRgbLerpUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

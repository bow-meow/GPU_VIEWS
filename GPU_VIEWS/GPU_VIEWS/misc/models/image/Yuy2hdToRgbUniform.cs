using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGPU_TEST.models.core.filters;
using Wgpu;
using GPU_VIEWS.misc;
using System.Runtime.CompilerServices;

namespace GPU_VIEWS.msic.models.image
{
    public struct Yuy2hdToRgbUniform : IUniform
    {
        public Yuy2hdToRgbUniform(Vector4D<float> quadScreenSize)
        {
            QuadScreenSize = quadScreenSize;
        }

        public Vector4D<float> QuadScreenSize { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<Yuy2hdToRgbUniform>() + sizeof(float));

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<Yuy2hdToRgbUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

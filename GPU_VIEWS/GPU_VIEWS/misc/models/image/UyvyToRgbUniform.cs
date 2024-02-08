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
    public struct UyvyToRgbUniform : IUniform
    {
        public UyvyToRgbUniform(Vector4D<float> quadScreenSize)
        {
            QuadScreenSize = quadScreenSize;
        }

        public Vector4D<float> QuadScreenSize { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<UyvyToRgbUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<UyvyToRgbUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

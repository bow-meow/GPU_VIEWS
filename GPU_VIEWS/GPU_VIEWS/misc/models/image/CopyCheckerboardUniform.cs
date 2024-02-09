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

namespace GPU_VIEWS.msic.models.image
{
    public struct CopyCheckerboardUniform : IUniform
    {
        public CopyCheckerboardUniform(Vector2D<float> windowSize)
        {
            WindowSize = windowSize;
        }

        public Vector2D<float> WindowSize { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<CopyCheckerboardUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<CopyCheckerboardUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

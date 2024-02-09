using GPU_VIEWS.misc;
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
    public struct QuantiseUniform : IUniform
    {
        public QuantiseUniform(float factor)
        {
            Factor = factor;
        }

        public float Factor { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<QuantiseUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<QuantiseUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

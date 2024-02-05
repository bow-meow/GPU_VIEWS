using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public struct RgbBalanceUniform : IBuffer
    {
        private float _padding1;
        private float _padding2;
        private float _padding3;
        public unsafe (BufferPtr, ulong buffer_size) CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)sizeof(RgbBalanceUniform);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<RgbBalanceUniform>(new[] { this }));

            return (buffer, buffer_size);
        }
    }
}

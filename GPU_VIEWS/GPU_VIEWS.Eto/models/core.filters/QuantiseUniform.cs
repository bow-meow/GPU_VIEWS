﻿using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public struct QuantiseUniform : IBuffer
    {
        public QuantiseUniform(float factor)
        {
            Factor = factor;
        }

        public float Factor { get; }

        public unsafe (BufferPtr, ulong buffer_size) CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)sizeof(QuantiseUniform);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<QuantiseUniform>(new[] { this }));

            return (buffer, buffer_size);
        }
    }
}

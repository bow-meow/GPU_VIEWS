﻿using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public struct BrightnessContrastUniform : IBuffer
    {
        public BrightnessContrastUniform(float brightness, float contrast)
        {
            this.brightness = brightness;
            this.contrast = contrast;
        }
        public float brightness { get; }
        public float contrast { get; }

        public unsafe (BufferPtr, ulong buffer_size) CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)sizeof(BrightnessContrastUniform);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<BrightnessContrastUniform>(new[] { this }));

            return (buffer, buffer_size);
        }
    }
}

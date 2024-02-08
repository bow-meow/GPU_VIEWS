using GPU_VIEWS.misc;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public struct BrightnessContrastUniform : IUniform
    {
        public BrightnessContrastUniform(float brightness, float contrast)
        {
            this.brightness = brightness;
            this.contrast = contrast;
        }
        public float brightness { get; }
        public float contrast { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<BrightnessContrastUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<BrightnessContrastUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

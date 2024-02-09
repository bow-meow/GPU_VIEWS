using GPU_VIEWS.misc;
using Silk.NET.Maths;
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
    public struct ChannelMixerUniform : IUniform
    {
        public ChannelMixerUniform(Vector3D<float> rGain, Vector3D<float> gGain, Vector3D<float> bGain)
        {
            r_gain = rGain;
            g_gain = gGain;
            b_gain = bGain;
        }

        public Vector3D<float> r_gain { get; }
        public Vector3D<float> g_gain { get; }
        public Vector3D<float> b_gain { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChannelMixerUniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChannelMixerUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

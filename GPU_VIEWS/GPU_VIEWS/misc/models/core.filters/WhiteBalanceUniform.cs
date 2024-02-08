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

namespace WGPU_TEST.models.core.filters
{
    public struct WhiteBalanceUniform : IUniform
    {
        public WhiteBalanceUniform(float red, float green, float blue, float strength, float keepWhite, Vector2D<float> refD65)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Strength = strength;
            KeepWhite = keepWhite;
            RefD65 = refD65;
        }

        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }
        public float Strength { get; }
        public float KeepWhite { get; }
        public Vector2D<float> RefD65 { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<WhiteBalanceUniform>() + sizeof(float) * 5);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<WhiteBalanceUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

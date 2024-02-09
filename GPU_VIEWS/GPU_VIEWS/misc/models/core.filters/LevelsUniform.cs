using GPU_VIEWS.misc;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace GPU_VIEWS.msic.models.core.filters
{
    //[StructLayout(LayoutKind.Explicit)]
    public struct LevelsUniform : IUniform
    {
        public LevelsUniform(float inBlack, float inWhite, float inGamma, float outWhite, float outBlack)
        {
            InBlack = inBlack;
            InWhite = inWhite;
            InGamma = inGamma;
            OutWhite = outWhite;
            OutBlack = outBlack;
        }
        
        public float InBlack { get; }
        public float InWhite { get; }
        public float InGamma { get; }
        public float OutWhite { get; }
        public float OutBlack { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<LevelsUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<LevelsUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

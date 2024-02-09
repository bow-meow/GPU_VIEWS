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

namespace GPU_VIEWS.msic.models.chromakey
{
    public struct ChromaKeyAntiSpill3Uniform : IUniform
    {
        public ChromaKeyAntiSpill3Uniform(Vector3D<float> keyHSV, float edgeDistance, float edgeFeather, float decrease)
        {
            KeyHSV = keyHSV;
            EdgeDistance = edgeDistance;
            EdgeFeather = edgeFeather;
            Decrease = decrease;
        }

        public Vector3D<float> KeyHSV { get; }
        public float EdgeDistance { get; }
        public float EdgeFeather { get; }
        public float Decrease { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaKeyAntiSpill3Uniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaKeyAntiSpill3Uniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

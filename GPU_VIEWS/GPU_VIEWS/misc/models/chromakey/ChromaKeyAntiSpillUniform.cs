using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGPU_TEST.models.core.filters;
using Wgpu;
using GPU_VIEWS.misc;
using System.Runtime.CompilerServices;

namespace WGPU_TEST.models.chromakey
{
    public struct ChromaKeyAntiSpillUniform : IUniform
    {
        public ChromaKeyAntiSpillUniform(Vector3D<float> keyHSV, float edgeDistance, float edgeFeather)
        {
            KeyHSV = keyHSV;
            EdgeDistance = edgeDistance;
            EdgeFeather = edgeFeather;
        }

        public Vector3D<float> KeyHSV { get; }
        public float EdgeDistance { get; }
        public float EdgeFeather { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaKeyAntiSpillUniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaKeyAntiSpillUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

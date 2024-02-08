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

namespace WGPU_TEST.models.chromakey
{
    public struct ChromaKeyFastUniform : IUniform
    {
        public ChromaKeyFastUniform(float keyHue, float spillEdgeDistance, float spillEdgeFeather, Vector3D<float> startDists, Vector3D<float> endDists) 
        {
            KeyHue = keyHue;
            SpillEdgeDistance = spillEdgeDistance;
            SpillEdgeFeather = spillEdgeFeather;
            StartDists = startDists;
            EndDists = endDists;
        }

        public float KeyHue { get; }
        public float SpillEdgeDistance { get; }
        public float SpillEdgeFeather { get; }
        public Vector3D<float> StartDists { get; }
        public Vector3D<float> EndDists { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaKeyFastUniform>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaKeyFastUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

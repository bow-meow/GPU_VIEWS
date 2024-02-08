using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using WGPU_TEST.models.core.filters;
using Wgpu;
using System.Runtime.CompilerServices;
using GPU_VIEWS.misc;

namespace WGPU_TEST.models.chromakey
{
    public struct ChromaKeyUnifrom : IUniform
    {
        public ChromaKeyUnifrom(Vector3D<float> keyHSV, float edgeFeather, float edgeDistance, float spillEdgeFeather, float spillEdgeDistance, float spillLuma, float spillAmount)
        {
            KeyHSV = keyHSV;
            EdgeFeather = edgeFeather;
            EdgeDistance = edgeDistance;
            SpillEdgeFeather = spillEdgeFeather;
            SpillEdgeDistance = spillEdgeDistance;
            SpillLuma = spillLuma;
            SpillAmount = spillAmount;
        }

        public Vector3D<float> KeyHSV { get; }
        public float EdgeFeather { get; }
        public float EdgeDistance { get; }
        public float SpillEdgeFeather { get; }
        public float SpillEdgeDistance { get; }
        public float SpillLuma { get; }
        public float SpillAmount { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaKeyUnifrom>() + sizeof(float) * 3);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaKeyUnifrom>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

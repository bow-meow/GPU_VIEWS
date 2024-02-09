using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using WGPU_TEST.models.core.filters;
using Wgpu;
using System.Runtime.CompilerServices;
using GPU_VIEWS.misc;

namespace GPU_VIEWS.msic.models.chromakey
{
    public struct ChromaKeyNewUniform : IUniform
    {
        public ChromaKeyNewUniform(Vector3D<float> keyHSV, float hueDistance, float satDistance, float valDistance, float hueFeather, float hueFeather2, float satFeather, float valFeather, bool isAlphaPremultiplied) 
        {
            KeyHSV = keyHSV;
            HueDistance = hueDistance;
            SatDistance = satDistance;
            ValDistance = valDistance;
            HueFeather = hueFeather;
            HueFeather2 = hueFeather2;
            SatFeather = satFeather;
            ValFeather = valFeather;
            IsAlphaPremultiplied = isAlphaPremultiplied ? 1 : 0;

        }

        public Vector3D<float> KeyHSV { get; }
        public float HueDistance { get; }
        public float SatDistance { get; }
        public float ValDistance { get; }
        public float HueFeather { get; }
        public float SatFeather { get; }
        public float ValFeather { get; }
        public float HueFeather2 { get; }
        public int IsAlphaPremultiplied { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<ChromaKeyNewUniform>() + sizeof(float) * 5);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<ChromaKeyNewUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

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

namespace GPU_VIEWS.msic.models.image
{
    public struct Yuy2ToRgbUniform : IUniform
    {
        public Yuy2ToRgbUniform(float textureAspect, Vector2D<float> textureScale, Vector4D<float> quadScreenSize)
        {
            TextureAspect = textureAspect;
            TextureScale = textureScale;
            QuadScreenSize = quadScreenSize;
        }

        public float TextureAspect { get; }
        public Vector2D<float> TextureScale { get; }
        public Vector4D<float> QuadScreenSize { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<Yuy2ToRgbUniform>() + sizeof(float));

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<Yuy2ToRgbUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

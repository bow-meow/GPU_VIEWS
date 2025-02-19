﻿using Silk.NET.Maths;
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

namespace GPU_VIEWS.msic.models.image
{
    public struct Yuy2ToRgbLerpUniform : IUniform
    {
        public Yuy2ToRgbLerpUniform(float textureAspect, Vector2D<float> textureScale, Vector4D<float> quadScreenSize, float quadTexOffset)
        {
            TextureAspect = textureAspect;
            TextureScale = textureScale;
            QuadScreenSize = quadScreenSize;
            QuadTexOffset = quadTexOffset;
        }

        public float TextureAspect { get; }
        public Vector2D<float> TextureScale { get; }
        public Vector4D<float> QuadScreenSize { get; }
        public float QuadTexOffset { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)(Unsafe.SizeOf<Yuy2ToRgbLerpUniform>() + sizeof(float) * 4);

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<Yuy2ToRgbLerpUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Runtime.CompilerServices;
using Wgpu;

namespace GPU_VIEWS.msic.models.core.filters
{
    public struct BorderUniform : IUniform
    {
        public BorderUniform(Vector4D<float> _color, Vector4D<float> _color_mask, float _top, float _left, float _bottom, float _right)
        {
            color = _color;
            color_mask = _color_mask;
            top = _top;
            left = _left;
            bottom = _bottom;
            right = _right;
        }

        public Vector4D<float> color { get; }
        public Vector4D<float> color_mask { get; }
        public float top { get; }
        public float left { get; }
        public float bottom { get; }
        public float right { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<BorderUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<BorderUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

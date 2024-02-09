using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Wgpu;
using WGPU_TEST.models.core.filters;

namespace GPU_VIEWS.misc
{
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct FontVertexTransformUniform : IUniform
	{
		public FontVertexTransformUniform(Matrix4X4<float> transform, Vector4D<float> color)
		{
            Color = color;
            Transform = transform;
		}

		public Vector4D<float> Color { get; }
        public Matrix4X4<float> Transform { get; }
	}
}
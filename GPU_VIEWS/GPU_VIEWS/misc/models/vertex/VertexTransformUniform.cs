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
	public readonly struct VertexTransformUniform : IUniform
	{
		public VertexTransformUniform(Matrix4X4<float> transform)
		{
			Transform = transform;
		}

		public Matrix4X4<float> Transform { get; }
	}
}

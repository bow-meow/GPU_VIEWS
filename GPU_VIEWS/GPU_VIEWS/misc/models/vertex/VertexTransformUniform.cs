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

		public unsafe BufferInternal CreateBuffer(DevicePtr device)
		{
			var buffer_size = (ulong)Unsafe.SizeOf<VertexTransformUniform>();

			var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

			var queue = device.GetQueue();

			queue.WriteBuffer(buffer, 0, new ReadOnlySpan<VertexTransformUniform>(new[] { this }));

			return new BufferInternal(buffer, buffer_size);
		}
	}
}

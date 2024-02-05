using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System;
using Wgpu;
using WGPU_TEST.models.core.filters;

namespace TestEtoVeldrid.models.vertex
{
	public struct VertexTransformUniform : IBuffer
	{
		public VertexTransformUniform(Matrix4X4<float> transform)
		{
			Transform = transform;
		}

		public Matrix4X4<float> Transform { get; }

		public unsafe (BufferPtr, ulong buffer_size) CreateBuffer(DevicePtr device)
		{
			var buffer_size = (ulong)(sizeof(VertexTransformUniform));

			var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

			var queue = device.GetQueue();

			queue.WriteBuffer(buffer, 0, new ReadOnlySpan<VertexTransformUniform>(new[] { this }));

			return (buffer, buffer_size);
		}
	}
}

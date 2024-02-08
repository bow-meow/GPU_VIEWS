using GPU_VIEWS.misc;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public struct CameraBarrelFixUniform : IUniform
    {
        public CameraBarrelFixUniform(float focalLength, float principalX, float principalY)
        {
            focal_length = focalLength;
            principal_x = principalX;
            principal_y = principalY;
        }

        public float focal_length { get; }
        public float principal_x { get; }
        public float principal_y { get; }

        public BufferInternal CreateBuffer(DevicePtr device)
        {
            var buffer_size = (ulong)Unsafe.SizeOf<CameraBarrelFixUniform>();

            var buffer = device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, buffer_size);

            var queue = device.GetQueue();

            queue.WriteBuffer(buffer, 0, new ReadOnlySpan<CameraBarrelFixUniform>(new[] { this }));

            return new BufferInternal(buffer, buffer_size);
        }
    }
}

using Wgpu;

namespace WGPU_TEST.models.core.filters
{
    public interface IBuffer
    {
        (BufferPtr, ulong buffer_size) CreateBuffer(DevicePtr device);
    }
}

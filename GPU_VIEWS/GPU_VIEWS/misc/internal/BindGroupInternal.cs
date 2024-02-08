using Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly struct BindGroupInternal
    {
        public BindGroupLayoutPtr BindGroupLayout { get; }
        public BindGroupPtr BindGroup { get; }

        public BindGroupInternal(BindGroupLayoutPtr layout, BindGroupPtr group)
        {
            BindGroupLayout = layout;
            BindGroup = group;
        }
    }
}
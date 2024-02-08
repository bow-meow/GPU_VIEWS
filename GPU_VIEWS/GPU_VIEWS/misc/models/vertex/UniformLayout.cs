using Wgpu;
using WGPU_TEST.models.core.filters;

namespace GPU_VIEWS.misc
{
    public readonly struct UniformLayout
    {
        public UniformLayout(IUniform uniform,
        BufferInternal buffer,
        ShaderModulePtr shaderModule)
        {
            Uniform = uniform;
            BufferInternal = buffer;
            ShaderModule = shaderModule;
        }

        public IUniform Uniform { get; }
        public BufferInternal BufferInternal { get; }
        public ShaderModulePtr ShaderModule { get; }
    }
}
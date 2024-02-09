using GPU_VIEWS.msic;
using Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly struct UniformLayout
    {
        public UniformLayout(object uniform,
        BufferInternal buffer,
        ShaderModulePtr shaderModule)
        {
            Uniform = uniform;
            BufferInternal = buffer;
            ShaderModule = shaderModule;
        }

        public object Uniform { get; }
        public BufferInternal BufferInternal { get; }
        public ShaderModulePtr ShaderModule { get; }
    }
}
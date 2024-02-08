using System;
using Silk.NET.WebGPU;
using Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly ref struct RenderPipelineDesc
    {
        public ReadOnlySpan<BindGroupLayoutPtr> BindGroupLayoutEntries { get; }
        public ShaderModulePtr VertexShader { get; }
        public ShaderModulePtr FragmentShader { get; }
        public TextureFormat TextureFormat { get; }

        public RenderPipelineDesc(ReadOnlySpan<BindGroupLayoutPtr> layoutEntries,
        ShaderModulePtr vertexShader,
        ShaderModulePtr fragmentShader,
        TextureFormat textureFormat)
        {
            BindGroupLayoutEntries = layoutEntries;
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;
            TextureFormat = textureFormat;
        }
    }
}
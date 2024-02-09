using System;
using Silk.NET.WebGPU;
using Wgpu;
using wgpu = Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly ref struct RenderPipelineDesc
    {
        public ReadOnlySpan<BindGroupLayoutPtr> BindGroupLayoutEntries { get; }
        public ShaderModulePtr FragmentShader { get; }
        public TextureFormat TextureFormat { get; }
        public wgpu.VertexState VertexState { get; }

        public RenderPipelineDesc(ReadOnlySpan<BindGroupLayoutPtr> layoutEntries,
        wgpu.VertexState vertexState,
        ShaderModulePtr fragmentShader,
        TextureFormat textureFormat)
        {
            BindGroupLayoutEntries = layoutEntries;
            VertexState = vertexState;
            FragmentShader = fragmentShader;
            TextureFormat = textureFormat;
        }
    }
}
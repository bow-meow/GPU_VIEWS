using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Wgpu;
using WGPU_TEST;
using WGPU_TEST.models.core.filters;

namespace GPU_VIEWS.misc
{
    public interface IResourceManager
    {
        UniformLayout CreateUniformLayout<TUniform>(TUniform uniform, ShaderType shaderType) where TUniform : unmanaged, IUniform;
        BufferInternal CreateBuffer(BufferDesc desc);
        BindGroupInternal CreateBindGroup(ReadOnlySpan<BindGroupLayoutEntry> layoutEntries, ReadOnlySpan<BindGroupEntry> groupEntries);
        SamplerPtr CreateSampler();
        RenderPiplineInternal CreateRenderPipeline(RenderPipelineDesc desc);
    }

    public class ResourceManager : IResourceManager
    {
        private readonly DevicePtr _device;
        private readonly ShaderFactory _shaderFactory;
        public ResourceManager(DevicePtr device)
        {
            _device = device;
            _shaderFactory = new ShaderFactory();
        }

        public BindGroupInternal CreateBindGroup(ReadOnlySpan<BindGroupLayoutEntry> layoutEntries, ReadOnlySpan<BindGroupEntry> groupEntries)
        {
            var layout = _device.CreateBindGroupLayout(layoutEntries);
            var group = _device.CreateBindGroup(layout, groupEntries);
            
            return new BindGroupInternal(layout, group);
        }

        public UniformLayout CreateUniformLayout<TUniform>(TUniform uniform, ShaderType shaderType) where TUniform : unmanaged, IUniform 
        {
            var buffer_size = (ulong)Unsafe.SizeOf<TUniform>();

            var bufferInteral = CreateBuffer(new BufferDesc(
                 bufferUsage: BufferUsage.Uniform | BufferUsage.CopyDst,
                 size: buffer_size
            ));

            var queue = _device.GetQueue();

            queue.WriteBuffer(bufferInteral.Buffer, 0, new ReadOnlySpan<TUniform>(new[] { uniform }));

            var module = _shaderFactory.GetOrCreate(_device, shaderType);

            return new UniformLayout(uniform, bufferInteral, module);
        }

        public BufferInternal CreateBuffer(BufferDesc desc)
        {
            var buffer = _device.CreateBuffer(usage: desc.BufferUsage,
            size: desc.Size,
            mappedAtCreation: false,
            label: "debug_label");

            return new BufferInternal(buffer, desc.Size);
        }

        public SamplerPtr CreateSampler()
        {
            return _device.CreateSampler(addressModeU: AddressMode.ClampToEdge,
                addressModeV: AddressMode.ClampToEdge,
                addressModeW: AddressMode.ClampToEdge,
                magFilter: FilterMode.Linear,
                minFilter: FilterMode.Nearest,
                mipmapFilter: MipmapFilterMode.Nearest,
                lodMinClamp: 0,
                lodMaxClamp: 1,
                compare: CompareFunction.Undefined,
                maxAnisotropy: 1);
        }

        public RenderPiplineInternal CreateRenderPipeline(RenderPipelineDesc desc)
        {
            var pipelineLayout = _device.CreatePipelineLayout(desc.BindGroupLayoutEntries);

            var render_pipeline = _device.CreateRenderPipeline(layout: pipelineLayout,
            vertex: new Wgpu.VertexState
            {
                ShaderModule = desc.VertexShader,
                EntryPoint = "vs_main",
                Constants = new (string key, double value)[] { },
                Buffers = new Wgpu.VertexBufferLayout[]
                {
                    new Wgpu.VertexBufferLayout((ulong)Unsafe.SizeOf<Vertex>(), VertexStepMode.Vertex,
                    new VertexAttribute[]
                    {
                        new VertexAttribute(VertexFormat.Float32x3, 0, 0),
                        new VertexAttribute(VertexFormat.Float32x2, (uint)Unsafe.SizeOf<Vector3D<float>>(), 1)
                    })
                }
            },
            primitive: new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                StripIndexFormat = IndexFormat.Undefined,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.Back,
            },
            null,
            multisample: new MultisampleState
            {
                Count = 1,
                Mask = ~0u,
                AlphaToCoverageEnabled = false,
            },
            new Wgpu.FragmentState(
                shaderModule: desc.FragmentShader,
                entryPoint: "fs_main",
                new (string key, double value)[] { },
                colorTargets: new Wgpu.ColorTargetState[]
                {
                    new(
                    desc.TextureFormat,
                    (
                        color: new(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero),
                        alpha: new(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero)
                    ),
                    ColorWriteMask.All
                    )
                }));
            return new RenderPiplineInternal(pipelineLayout, render_pipeline);
        }
    }
}
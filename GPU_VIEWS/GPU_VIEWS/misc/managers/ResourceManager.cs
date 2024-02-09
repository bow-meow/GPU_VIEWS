using System;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using wgpu = Wgpu;
using WGPU_TEST;
using Wgpu;

namespace GPU_VIEWS.misc
{
    public interface IResourceManager
    {
        UniformLayout CreateUniformLayout<TUniform>(TUniform uniform, ShaderType shaderType) where TUniform : unmanaged;
        BufferInternal CreateBuffer(BufferDesc desc);
        BindGroupInternal CreateBindGroup(ReadOnlySpan<BindGroupLayoutEntry> layoutEntries, ReadOnlySpan<BindGroupEntry> groupEntries);
        SamplerPtr CreateSampler();
        RenderPiplineInternal CreateRenderPipeline(RenderPipelineDesc desc);
        CommandEncoderPtr CreateCommandEncoder(string? name = null);
        QueuePtr GetQueue();
        ShaderModulePtr CreateShader(ShaderType shaderType);
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

        public QueuePtr GetQueue()
        {
            return _device.GetQueue();
        }

        public CommandEncoderPtr CreateCommandEncoder(string? name = null)
        {
            return _device.CreateCommandEncoder(name);
        }

        public BindGroupInternal CreateBindGroup(ReadOnlySpan<BindGroupLayoutEntry> layoutEntries, ReadOnlySpan<BindGroupEntry> groupEntries)
        {
            var layout = _device.CreateBindGroupLayout(layoutEntries);
            var group = _device.CreateBindGroup(layout, groupEntries);
            
            return new BindGroupInternal(layout, group);
        }

        public UniformLayout CreateUniformLayout<TUniform>(TUniform uniform, ShaderType shaderType) where TUniform : unmanaged 
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
            mappedAtCreation: desc.MappedAtCreation,
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

        public ShaderModulePtr CreateShader(ShaderType shaderType)
        {
            return _shaderFactory.GetOrCreate(_device, shaderType);
        }

        public RenderPiplineInternal CreateRenderPipeline(RenderPipelineDesc desc)
        {
            var pipelineLayout = _device.CreatePipelineLayout(desc.BindGroupLayoutEntries);

            var render_pipeline = _device.CreateRenderPipeline(layout: pipelineLayout,
            vertex: desc.VertexState,
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
            new wgpu.FragmentState(
                shaderModule: desc.FragmentShader,
                entryPoint: "fs_main",
                new (string key, double value)[] { },
                colorTargets: new wgpu.ColorTargetState[]
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
using System;
using System.Runtime.CompilerServices;
using FontStashSharp.Interfaces;
using GPU_VIEWS.Eto;
using GPU_VIEWS.Eto.Controls;
using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp.PixelFormats;
using Wgpu;
using wgpu = Wgpu;

namespace GPU_VIEWS.renderers
{
    public class WgpuThumbnailRenderer : IWgpuRenderer
    {
        private readonly Vertex[] _quad;
        private readonly uint[] _indexMap;
        private readonly BufferInternal _vertexBuffer;
        private readonly BufferInternal _indexBuffer;

        private UniformLayout _quadUniformLayout;
        private UniformLayout _sampleUniformLayout;

        private BindGroupInternal _vertexBindGroup;

        private TextureInternal _texture;

        private BindGroupInternal _textureBindGroup;
        private BindGroupInternal _sampleBindGroup;

        private RenderPiplineInternal _renderPipeline;


        // TODO:(ALEX) these need to be removed
        private WgpuView _view;
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        // END TODO
        public WgpuThumbnailRenderer(WgpuView view)
        {
            _view = view;

            TextRenderer = new TextRenderer(view);
            TextureManager = new TextureManager(view.Device, view.TextureFormat);
            ResourceManager = new ResourceManager(view.Device);

            _view.Device.SetUncapturedErrorCallback(new wgpu.ErrorCallback((err, str) =>
            {
                var a = 1;
            }));

            (_quad, _indexMap) = Vertex.GetFullScreenQuad();

            _vertexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Vertex | BufferUsage.CopyDst,
                size: (ulong)(Unsafe.SizeOf<Vertex>() * _quad.Length)));
            
            _indexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Index | BufferUsage.CopyDst,
                size: (ulong)(sizeof(int) * _indexMap.Length)
            ));

            var queue = view.Device.GetQueue();
			queue.WriteBuffer<Vertex>(_vertexBuffer.Buffer, 0, _quad.AsSpan());
			queue.WriteBuffer<uint>(_indexBuffer.Buffer, 0, _indexMap.AsSpan());
        }

        private void CreateUniforms()
        {
            // TODO:(Alex) likely want to store shadertype as an attrubute.

            // Vertex
            var scale = Matrix4X4.CreateScale<float>(Math.Min(_view.Width / (float)ImageWidth, _view.Height / (float)ImageHeight));

            var transform = new VertexTransformUniform(scale);
            _quadUniformLayout = ResourceManager.CreateUniformLayout(transform, WGPU_TEST.ShaderType.Quad);

            // Simple shader
            var sampleShader = new SampleUniform();
            _sampleUniformLayout = ResourceManager.CreateUniformLayout(sampleShader, WGPU_TEST.ShaderType.Sample);
        }

        public ITextureManager TextureManager { get; }
        public IResourceManager ResourceManager { get; }
        public ITextRenderer TextRenderer { get; }

        private unsafe void CreateVertexBindgroup()
		{
            // TODO:(ALEX) Simplify this
			_vertexBindGroup = ResourceManager.CreateBindGroup(
            new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
            {
				new BindGroupLayoutEntry{
					Binding = 0,
					Buffer = new BufferBindingLayout
					{
						Type = BufferBindingType.Uniform,
						HasDynamicOffset = false,
						MinBindingSize = _quadUniformLayout.BufferInternal.BufferSize
					},
					Visibility = ShaderStage.Vertex
				}
            }),
            new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
			{
				new BindGroupEntry
				{
					Binding = 0,
					Buffer = _quadUniformLayout.BufferInternal.Buffer,
					Offset = 0,
					Size = _quadUniformLayout.BufferInternal.BufferSize,
				}
			}));
		}

        private void CreateBindGroup0(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            _texture = TextureManager.CreateTexture(new Texture2dDesc(
                usage: TextureUsage.CopyDst | TextureUsage.TextureBinding,
                size: new Extent3D
                {
                    Width = (uint)image.Size.Width,
                    Height = (uint)image.Size.Height,
                    DepthOrArrayLayers = 1,
                },
                format: TextureFormat.Rgba8UnormSrgb
            ));

            var pixels = new Span<byte>(new byte[image.Width * Unsafe.SizeOf<Rgba32>() * image.Height]);

            image.CopyPixelDataTo(pixels);

            var queue = _view.Device.GetQueue();

            queue.WriteTexture<byte>(new wgpu.ImageCopyTexture
            {
                Texture = _texture.Texture,
                Aspect = TextureAspect.All,
                MipLevel = 0,
                Origin = new Origin3D { X = 0, Y = 0, Z = 0 },
            },
            data: pixels,
            new TextureDataLayout
            {
                BytesPerRow = (uint)(Unsafe.SizeOf<Rgba32>() * image.Size.Width),
                RowsPerImage = (uint)image.Size.Height,
                Offset = 0,
            },
            new Extent3D
            {
                Width = (uint)image.Size.Width,
                Height = (uint)image.Size.Height,
                DepthOrArrayLayers = 1
            });

            var sampler = ResourceManager.CreateSampler();

            _textureBindGroup = ResourceManager.CreateBindGroup(new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
            {
                new BindGroupLayoutEntry
                {
                    Binding = 0,
                    Visibility = ShaderStage.Fragment,
                    Texture =  new TextureBindingLayout
                    {
                        ViewDimension = TextureViewDimension.Dimension2D,
                        Multisampled = false,
                        SampleType = TextureSampleType.Float
                    },
                },
                new BindGroupLayoutEntry
                {
                    Binding = 1,
                    Sampler = new SamplerBindingLayout
                    {
                        Type = SamplerBindingType.Filtering,
                    },
                    Visibility = ShaderStage.Fragment,
                } 
            }),
            new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
            {
                new BindGroupEntry
                {
                    Binding = 0,
                    TextureView = _texture.View,
                },
                new BindGroupEntry
                {
                    Binding = 1,
                    Sampler = sampler
                }
            }));
        }

        private void CreateBindGroup1()
        {
            _sampleBindGroup = ResourceManager.CreateBindGroup(
            new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
            {
                new BindGroupLayoutEntry{
                    Binding = 0,
                    Buffer = new BufferBindingLayout
                    {
                        Type = BufferBindingType.Uniform,
                        HasDynamicOffset = false,
                        MinBindingSize = _sampleUniformLayout.BufferInternal.BufferSize
                    },
                    Visibility = ShaderStage.Fragment
                }
            }),
            new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
            {
                new BindGroupEntry
                {
                    Binding = 0,
                    Buffer = _sampleUniformLayout.BufferInternal.Buffer
                    ,
                    Offset = 0,
                    Size = _sampleUniformLayout.BufferInternal.BufferSize,
                }
            }));
        }

        private void CreateRenderPipeline()
        {
            _renderPipeline = ResourceManager.CreateRenderPipeline(new RenderPipelineDesc(
                new ReadOnlySpan<BindGroupLayoutPtr>(new BindGroupLayoutPtr[]
                {
                    _textureBindGroup.BindGroupLayout,
                    _sampleBindGroup.BindGroupLayout,
                    _vertexBindGroup.BindGroupLayout
                }),
                vertexState: new wgpu.VertexState
                {
                    ShaderModule = _quadUniformLayout.ShaderModule,
                    EntryPoint = "vs_main",
                    Constants = new (string key, double value)[] { },
                    Buffers = new wgpu.VertexBufferLayout[]
                    {
                        new wgpu.VertexBufferLayout((ulong)Unsafe.SizeOf<Vertex>(), VertexStepMode.Vertex,
                        new VertexAttribute[]
                        {
                            new VertexAttribute(VertexFormat.Float32x3, 0, 0),
                            new VertexAttribute(VertexFormat.Float32x2, (uint)Unsafe.SizeOf<Vector3D<float>>(), 1)
                        })
                    }
                },
                _sampleUniformLayout.ShaderModule,
                _view.TextureFormat));
        }

        public void Initialize(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            TextRenderer.Initialize(image);
            
			ImageWidth = image.Width;
			ImageHeight = image.Height;

            CreateUniforms();

			CreateVertexBindgroup();

			// Bind Tex
			CreateBindGroup0(image);

			CreateBindGroup1();

			// Create pipeline
			CreateRenderPipeline();
        }

        public virtual void Render(Action<RenderPassEncoderPtr> callback = null)
        {
            CreateVertexBindgroup();

			CreateBindGroup1();

			CreateRenderPipeline();

			var commandEncoder = ResourceManager.CreateCommandEncoder("encoder");
			var surface_tex = _view.Surface.GetCurrentTexture();

			var render_pass = commandEncoder.BeginRenderPass(colorAttachments: new ReadOnlySpan<wgpu.RenderPassColorAttachment>(new wgpu.RenderPassColorAttachment[]
            {
                new wgpu.RenderPassColorAttachment
                {
                    View = surface_tex.Texture.CreateView(),
                    ResolveTarget = default,
                    LoadOp = LoadOp.Clear,
                    StoreOp = StoreOp.Store,
                    ClearValue = new Silk.NET.WebGPU.Color
                    {
                        R = 1.0,
                        G = 1.0,
                        B = 1.0,
                        A = 1.0
                    },
                }

            } ));
            
            render_pass.SetPipeline(_renderPipeline.RenderPipeline);
            render_pass.SetBindGroup(0, _textureBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetBindGroup(1, _sampleBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
			render_pass.SetBindGroup(2, _vertexBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetVertexBuffer(0, _vertexBuffer.Buffer, 0, _vertexBuffer.BufferSize);
            render_pass.SetIndexBuffer(_indexBuffer.Buffer, IndexFormat.Uint32, 0, _indexBuffer.BufferSize);

            render_pass.DrawIndexed((uint)_indexMap.Length, 1, 0, 0, 0);

            callback?.Invoke(render_pass);

            render_pass.End();

            var queue = ResourceManager.GetQueue();

            var commandBuffer = commandEncoder.Finish(null);
            ;
            queue.Submit(new ReadOnlySpan<wgpu.CommandBufferPtr>(new[] { commandBuffer }));
            _view.Surface.Present();

			_view.RecreateSwapchain();
		    
            //unsafe
            //{
            //    wgpu.TryGetDeviceExtension(null, out Wgpu wgpuSpecific);
            //    wgpuSpecific.DevicePoll(device, true, null);
            //}
            //swapchain = device.CreateSwapChain(surface, TextureUsage.RenderAttachment, tex_format, (uint)size.Width, (uint)size.Height, PresentMode.Fifo);
            //swapchain = device.CreateSwapChain(surface, TextureUsage.RenderAttachment, texture_format, (uint)500, (uint)500, PresentMode.Fifo);
            //bind_tex.Destroy();
            //output_buffer.Unmap();
            //_uniform_buffer.Destroy();

            //wgpuSpecific.TextureDrop(tex);
            //wgpuSpecific.TextureViewDrop(tex_view);
            //wgpuSpecific.RenderPipelineDrop(pipeline);
            //wgpuSpecific.BindGroupDrop(tex_sampler_bindgroup);
            //wgpuSpecific.BindGroupDrop(frag_uniform_bindgroup);
            //wgpu.BufferUnmap(output_buffer);
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Silk.NET.WebGPU;
using Silk.NET.Maths;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using Wgpu;
using SixLabors.ImageSharp.Processing;
using GPU_VIEWS.Eto.Controls;
using TestEtoVeldrid.models.vertex;
using WGPU_TEST.models.core.filters;
using System.Threading.Tasks;
using Eto.Forms;

namespace WGPU_TEST.Processor
{
    public class WgpuImageProcessor
    {
        public readonly struct Vertex
        {
            public Vertex(Vector3D<float> pos, Vector2D<float> texCoord)
            {
                position = pos;
                tex_coords = texCoord;
            }

            public Vector3D<float> position { get; }
            public Vector2D<float> tex_coords { get; }
        }

		Vertex[] quad_vert = new[] {
				// Positions  // Texture Coords
				new Vertex(new Vector3D<float>(1.0f, 1.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f - 1.0f)),
				new Vertex(new Vector3D<float>(-1.0f, 1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f - 1.0f)),
				new Vertex(new Vector3D<float>(-1.0f, -1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f - 0.0f)),
				new Vertex(new Vector3D<float>(1.0f, -1.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f - 0.0f))
		};

        uint[] quad_index_map =
        {
                    0, 1, 2,
                    3, 0, 2,
                };

        //WebGPU wgpu;

		DevicePtr Device;
		private Dictionary<ShaderType, ShaderModulePtr> _shaders = new Dictionary<ShaderType, ShaderModulePtr>();

        TexturePtr bind_tex;
        TextureViewPtr bind_tex_view;
        BindGroupLayoutPtr bind_tex_group_layout;
        BindGroupPtr bind_tex_group;

        BindGroupLayoutPtr uniform_bindgroup_layout;
        BindGroupPtr uniform_bindgroup;

        BindGroupLayoutPtr vertex_uniform_bindgroup_layout;
		BindGroupPtr vertex_uniform_bindgroup;

		RenderPipelinePtr render_pipeline;

        BufferPtr _vertex_buffer;
        BufferPtr _index_buffer;
        BufferPtr _uniform_buffer;
		BufferPtr _vertex_uniform_buffer;

        ulong _vertex_buffer_size;
        ulong _index_buffer_size;
        ulong _uniform_buffer_size;
		ulong _vertex_uniform_buffer_size;


		TextureFormat tex_format = TextureFormat.Rgba8UnormSrgb;

        public AdapterProperties AdapterProperties { get; }

		private WgpuView _view;

        public WgpuImageProcessor(WgpuView view)
        {
			_view = view;
			Device = view.Device;
            var scrollable = _view.FindParent<Scrollable>();

            view.Draw += (s, e) =>
            {
                if(view.IsScreenVisible)
                    Render();
            };
            

			tex_format = _view.TextureFormat;
			Device.SetUncapturedErrorCallback(new Wgpu.ErrorCallback((err, str) =>
            {
                var a = 1;
            }));

			unsafe
			{
				_vertex_buffer_size = (ulong)(sizeof(Vertex) * quad_vert.Length); // unsafe
				_index_buffer_size = (ulong)(sizeof(int) * quad_index_map.Length); // safe
			}


			_vertex_buffer = Device.CreateBuffer(BufferUsage.Vertex | BufferUsage.CopyDst, _vertex_buffer_size, false);
            _index_buffer = Device.CreateBuffer(BufferUsage.Index | BufferUsage.CopyDst, _index_buffer_size, false);

            var queue = Device.GetQueue();
			queue.WriteBuffer<Vertex>(_vertex_buffer, 0, quad_vert.AsSpan());
			queue.WriteBuffer<uint>(_index_buffer, 0, quad_index_map.AsSpan());
        }

        private unsafe void CreateUniform(ShaderType shaderType)
        {
            switch(shaderType)
            {
				case ShaderType.Quad:
				{
					var ortho = Matrix4X4.CreateOrthographic<float>((float)_view.Width / ImageWidth, (float)_view.Height / ImageHeight, 0.0f, 1.0f);

						//let position = self.position - self.scaled() / 2.0;
					var scale = Matrix4X4.CreateScale<float>(Math.Min(_view.Width / (float)ImageWidth, _view.Height / (float)ImageHeight));


					var position =  new Vector2D<float>(ImageWidth, ImageHeight) / 2.0f;

					var translation = Matrix4X4.CreateTranslation(new Vector3D<float>(position.X, position.Y, 0.0f));


						var s = new Matrix4X4<float>(
							0.5f, 0.0f, 0.0f, 0.0f,
							0.0f, 1.0f, 0.0f, 0.0f,
							0.0f, 0.0f, 1.0f, 0.0f,
							0.0f, 0.0f, 0.0f, 1.0f);

						var matrix = s * scale;


					var transform = new VertexTransformUniform(scale);

					(_vertex_uniform_buffer, _vertex_uniform_buffer_size) = transform.CreateBuffer(Device);


				}
				break;
                case ShaderType.RgbBalance:
                    {
                        var rbg_balance = new RgbBalanceUniform();
                        (_uniform_buffer, _uniform_buffer_size) = rbg_balance.CreateBuffer(Device);
                    }
                    break;
            }
        }

        public void Init_Shader(string embedPath, ShaderType shaderType)
        {
            string wgsl = string.Empty;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embedPath))
            using (var sr = new StreamReader(stream))
                wgsl = sr.ReadToEnd();

            _shaders[shaderType] = Device.CreateShaderModuleWGSL(wgsl, new Wgpu.ShaderModuleCompilationHint[]
            {
                new Wgpu.ShaderModuleCompilationHint { EntryPoint = "vs_main"},
                new Wgpu.ShaderModuleCompilationHint { EntryPoint = "fs_main"}
            },
            $"{shaderType}");
        }

        private void CreateBindGroup0(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            bind_tex = Device.CreateTexture(
           usage: TextureUsage.CopyDst | TextureUsage.TextureBinding,
           dimension: TextureDimension.Dimension2D,
           size: new Extent3D
           {
               Width = (uint)image.Size.Width,
               Height = (uint)image.Size.Height,
               DepthOrArrayLayers = 1,
           },
           format: TextureFormat.Rgba8UnormSrgb,
           mipLevelCount: 5,
           sampleCount: 1,
           viewFormats: new ReadOnlySpan<TextureFormat>(new[] { TextureFormat.Rgba8UnormSrgb }));

            bind_tex_view = bind_tex.CreateView(format: TextureFormat.Rgba8UnormSrgb,
                dimension: TextureViewDimension.Dimension2D,
                aspect: TextureAspect.All,
                baseMipLevel: 0,
                mipLevelCount: 1,
                baseArrayLayer: 0,
                arrayLayerCount: 1,
                "bind tex view");


            unsafe
            {
                var pixels = new Span<byte>(new byte[image.Width * sizeof(Rgba32) * image.Height]);

				image.CopyPixelDataTo(pixels);

                var queue = Device.GetQueue();

                queue.WriteTexture<byte>(new Wgpu.ImageCopyTexture
                {
                    Texture = bind_tex,
                    Aspect = TextureAspect.All,
                    MipLevel = 0,
                    Origin = new Origin3D { X = 0, Y = 0, Z = 0 },
                },
                data: pixels,
                new TextureDataLayout
                {
                    BytesPerRow = (uint)(sizeof(Rgba32) * image.Size.Width),
                    RowsPerImage = (uint)image.Size.Height,
                    Offset = 0,
                },
                new Extent3D
                {
                    Width = (uint)image.Size.Width,
                    Height = (uint)image.Size.Height,
                    DepthOrArrayLayers = 1
                });
            }
            


            var sampler = Device.CreateSampler(addressModeU: AddressMode.ClampToEdge,
                addressModeV: AddressMode.ClampToEdge,
                addressModeW: AddressMode.ClampToEdge,
                magFilter: FilterMode.Linear,
                minFilter: FilterMode.Nearest,
                mipmapFilter: MipmapFilterMode.Nearest,
                lodMinClamp: 0,
                lodMaxClamp: 1,
                compare: CompareFunction.Undefined,
                maxAnisotropy: 1);

            bind_tex_group_layout = Device.CreateBindGroupLayout(new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
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
            new BindGroupLayoutEntry{
                Binding = 1,
                Sampler = new SamplerBindingLayout
                {
                    Type = SamplerBindingType.Filtering,
                },
                Visibility = ShaderStage.Fragment,
            } }));

            bind_tex_group = Device.CreateBindGroup(bind_tex_group_layout, new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
            {
                new BindGroupEntry
                {
                    Binding = 0,
                    TextureView = bind_tex_view,
                },
                new BindGroupEntry
                {
                    Binding = 1,
                    Sampler = sampler
                }
            }));
        }

        private void CreateRenderPipeline(ShaderType shaderType)
        {
            var pip = Device.CreatePipelineLayout(new ReadOnlySpan<BindGroupLayoutPtr>(new BindGroupLayoutPtr[]
            {
                bind_tex_group_layout,
                uniform_bindgroup_layout,
				vertex_uniform_bindgroup_layout
			}));

            unsafe
            {
                render_pipeline = Device.CreateRenderPipeline(layout: pip,
                vertex: new Wgpu.VertexState
                {
                    ShaderModule = _shaders[ShaderType.Quad],
                    EntryPoint = "vs_main",
                    Constants = new (string key, double value)[] { },
                    Buffers = new Wgpu.VertexBufferLayout[]
                    {
                        new Wgpu.VertexBufferLayout((ulong)sizeof(Vertex), VertexStepMode.Vertex,
                        new VertexAttribute[]
                        {
                            new VertexAttribute(VertexFormat.Float32x3, 0, 0),
                            new VertexAttribute(VertexFormat.Float32x2, (uint)sizeof(Vector3D<float>), 1)
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
                    shaderModule: _shaders[shaderType],
                    entryPoint: "fs_main",
                    new (string key, double value)[] { },
                    colorTargets: new Wgpu.ColorTargetState[]
                    {
                        new(
                        tex_format,
                        (
                            color: new(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero),
                            alpha: new(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero)
                        ),
                        ColorWriteMask.All
                        )
                    }));
            }
        }

        private void CreateBindGroup1(ShaderType type)
        {
            CreateUniform(type);

            uniform_bindgroup_layout = Device.CreateBindGroupLayout(new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
            {
                new BindGroupLayoutEntry{
                    Binding = 0,
                    Buffer = new BufferBindingLayout
                    {
                        Type = BufferBindingType.Uniform,
                        HasDynamicOffset = false,
                        MinBindingSize = _uniform_buffer_size
                    },
                    Visibility = ShaderStage.Fragment
                }
            }));

            uniform_bindgroup = Device.CreateBindGroup(uniform_bindgroup_layout, new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
            {
                new BindGroupEntry
                {
                    Binding = 0,
                    Buffer = _uniform_buffer,
                    Offset = 0,
                    Size = _uniform_buffer_size,
                }
            }));
        }

        private unsafe void CreateVertexBindgroup()
		{
			CreateUniform(ShaderType.Quad);

			vertex_uniform_bindgroup_layout = Device.CreateBindGroupLayout(new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
{
				new BindGroupLayoutEntry{
					Binding = 0,
					Buffer = new BufferBindingLayout
					{
						Type = BufferBindingType.Uniform,
						HasDynamicOffset = false,
						MinBindingSize = _vertex_uniform_buffer_size
					},
					Visibility = ShaderStage.Vertex
				}
}));

			vertex_uniform_bindgroup = Device.CreateBindGroup(vertex_uniform_bindgroup_layout, new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
			{
				new BindGroupEntry
				{
					Binding = 0,
					Buffer = _vertex_uniform_buffer,
					Offset = 0,
					Size = _vertex_uniform_buffer_size,
				}
			}));
		}

		public int ImageWidth { get; set; }
		public int ImageHeight { get; set; }

		public void Setup()
		{
            var image = _view.Image;

			ImageWidth = image.Width;
			ImageHeight = image.Height;

			CreateVertexBindgroup();

			// Bind Tex
			CreateBindGroup0(image);

			CreateUniform(ShaderType.RgbBalance);

			CreateBindGroup1(ShaderType.RgbBalance);

			Init_Shader("GPU_VIEWS.Eto.shaders.quadih.wgsl", ShaderType.Quad);
			Init_Shader("GPU_VIEWS.Eto.shaders.rgb_balance.wgsl", ShaderType.RgbBalance);

			// Create pipeline
			CreateRenderPipeline(ShaderType.RgbBalance);
			
		}

        public void Render()
        {
			CreateVertexBindgroup();

			CreateBindGroup1(ShaderType.RgbBalance);

			CreateRenderPipeline(ShaderType.RgbBalance);

			var commandEncoder = Device.CreateCommandEncoder("encoder");

			var surface_tex = _view.Surface.GetCurrentTexture(); // WAS VIEW

			var render_pass = commandEncoder.BeginRenderPass(colorAttachments: new ReadOnlySpan<Wgpu.RenderPassColorAttachment>(new Wgpu.RenderPassColorAttachment[]
            {
                new Wgpu.RenderPassColorAttachment
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
            
            //render_pass.RenderPassEncoderSetViewport(5, 30, _view.Width, _view.Height, 0, 1);

            render_pass.SetPipeline(render_pipeline);
            render_pass.SetBindGroup(0, bind_tex_group, ReadOnlySpan<uint>.Empty);
            render_pass.SetBindGroup(1, uniform_bindgroup, ReadOnlySpan<uint>.Empty);
			render_pass.SetBindGroup(2, vertex_uniform_bindgroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetVertexBuffer(0, _vertex_buffer, 0, _vertex_buffer_size);
            render_pass.SetIndexBuffer(_index_buffer, IndexFormat.Uint32, 0, _index_buffer_size);

            render_pass.DrawIndexed((uint)quad_index_map.Length, 1, 0, 0, 0);
            render_pass.End();

            var queue = Device.GetQueue();

            var commandBuffer = commandEncoder.Finish(null);
            ;
            queue.Submit(new ReadOnlySpan<CommandBufferPtr>(new[] { commandBuffer }));
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

        private unsafe (uint bytes_per_row, uint padded_bytes_per_row) GetBufferDimensions(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            uint bytes_per_row = (uint)(sizeof(Rgba32) * image.Size.Width);
            uint padded_bytes_per_row = 0;

            var padding = (256 - bytes_per_row % 256) % 256;

            padded_bytes_per_row = bytes_per_row + padding;

            return (bytes_per_row, padded_bytes_per_row);
        }
    }
}

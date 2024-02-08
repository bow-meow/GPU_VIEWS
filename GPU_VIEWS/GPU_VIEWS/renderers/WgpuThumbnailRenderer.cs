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
using WGPU_TEST.models.core.filters;

namespace GPU_VIEWS.renderers
{
    public class WgpuThumbnailRenderer : IWgpuRenderer, IFontStashRenderer2
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


        // TODO:(ALEX) these need to be removed
        private WgpuView _view;
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        // END TODO
        public WgpuThumbnailRenderer(WgpuView view)
        {
            _view = view;

            (_quad, _indexMap) = Vertex.GetFullScreenQuad();
            TextureManager = new TextureManager(view.Device, view.TextureFormat);

            _vertexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Vertex | BufferUsage.CopyDst,
                size: (ulong)(Unsafe.SizeOf<Vertex>() * _quad.Length)));
            
            _indexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Index | BufferUsage.CopyDst,
                size: (ulong)(sizeof(int) * _quad.Length)
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

        ITexture2DManager IFontStashRenderer2.TextureManager => TextureManager;

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
					Buffer = _vertexBuffer.Buffer,
					Offset = 0,
					Size = _vertexBuffer.BufferSize,
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

            queue.WriteTexture<byte>(new Wgpu.ImageCopyTexture
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
            ResourceManager.CreateRenderPipeline(new RenderPipelineDesc(
                new ReadOnlySpan<BindGroupLayoutPtr>(new BindGroupLayoutPtr[]
                {
                    _textureBindGroup.BindGroupLayout,
                    _sampleBindGroup.BindGroupLayout,
                    _vertexBindGroup.BindGroupLayout
                }),
                _quadUniformLayout.ShaderModule,
                _sampleUniformLayout.ShaderModule,
                TextureFormat.Rgba8UnormSrgb));
        }

        public void Initialize(SixLabors.ImageSharp.Image<Rgba32> image)
        {
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

        public virtual void Render()
        {
            throw new System.NotImplementedException();
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            
        }
    }
}
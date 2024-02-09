using System;
using System.Runtime.CompilerServices;
using FontStashSharp.Interfaces;
using GPU_VIEWS.Eto;
using GPU_VIEWS.Eto.Controls;
using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Wgpu;
using wgpu = Wgpu;

namespace GPU_VIEWS.renderers
{
    public class TextRenderer : ITextRenderer
    {
        private const int MAX_SPRITES = 100;
		private const int MAX_VERTICES = MAX_SPRITES * 4;
		private const int MAX_INDICES = MAX_SPRITES * 6;
		private BufferInternal _vertexBuffer;
		private BufferInternal _indexBuffer;
		private readonly VertexPositionColorTexture[] _vertexData = new VertexPositionColorTexture[MAX_VERTICES];
		private TexturePtr _lastTexture;
        private UniformLayout _sampleUniformLayout;
        private BindGroupInternal _sampleBindGroup;
		private int _vertexIndex = 0;
        private static readonly uint[] indexData = GenerateIndexArray();

        private WgpuView _view;
        public int ImageWidth { get; set; }
	    public int ImageHeight { get; set; }
        public TextRenderer(WgpuView view)
        {
            _view = view;
            TextureManager = new TextureManager(view.Device, view.TextureFormat);
            ResourceManager = new ResourceManager(view.Device);
        }

        public ITexture2DManager TextureManager { get; }
        public IResourceManager ResourceManager { get; }

        private void CreateUniforms()
        {
            // Simple shader
            var sampleShader = new SampleUniform();
            _sampleUniformLayout = ResourceManager.CreateUniformLayout(sampleShader, WGPU_TEST.ShaderType.Font);

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

        public void Initialize(Image<Rgba32> image)
        {
            ImageWidth = image.Width;
			ImageHeight = image.Height;

            CreateUniforms();
        }

        public void Render(RenderPassEncoderPtr render_pass)
        {
            // Write index and vertex buffers
            _vertexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Vertex | BufferUsage.CopyDst | BufferUsage.Uniform,
                size: (ulong)(Unsafe.SizeOf<VertexPositionColorTexture>() * MAX_VERTICES)));

            var idx_data = indexData;
            
            _indexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Index | BufferUsage.CopyDst,
                size: (ulong)(sizeof(int) * MAX_INDICES)
            ));

            var queue = _view.Device.GetQueue();
            queue.WriteBuffer<VertexPositionColorTexture>(_vertexBuffer.Buffer, 0, _vertexData.AsSpan());
			queue.WriteBuffer<uint>(_indexBuffer.Buffer, 0, idx_data.AsSpan()); 

            // Create Vertex Bindgroup

            var vertexBindGroup = ResourceManager.CreateBindGroup(
            new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
            {
				new BindGroupLayoutEntry{
					Binding = 0,
					Buffer = new BufferBindingLayout
					{
						Type = BufferBindingType.Uniform,
						HasDynamicOffset = false,
						MinBindingSize = _vertexBuffer.BufferSize
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

            // Create Texture Bindgroup

            var sampler = ResourceManager.CreateSampler();

            var textureBindGroup = ResourceManager.CreateBindGroup(new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
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
                    TextureView = _lastTexture.CreateView(),
                },
                new BindGroupEntry
                {
                    Binding = 1,
                    Sampler = sampler
                }
            }));

            // Create pipeline

            var renderPipeline = ResourceManager.CreateRenderPipeline(new RenderPipelineDesc(
                new ReadOnlySpan<BindGroupLayoutPtr>(new BindGroupLayoutPtr[]
                {
                    textureBindGroup.BindGroupLayout,
                    _sampleBindGroup.BindGroupLayout,
                    vertexBindGroup.BindGroupLayout
                }),
                vertexState: new wgpu.VertexState
                {
                    ShaderModule =  ResourceManager.CreateShader(WGPU_TEST.ShaderType.QuadFont),
                    EntryPoint = "vs_main",
                    Constants = new (string key, double value)[] { },
                    Buffers = new wgpu.VertexBufferLayout[]
                    {
                        new wgpu.VertexBufferLayout((ulong)Unsafe.SizeOf<Vertex>(), VertexStepMode.Vertex,
                        new VertexAttribute[]
                        {
                            new VertexAttribute(VertexFormat.Float32x3, 0, 0),
                            new VertexAttribute(VertexFormat.Float32x4, (uint)Unsafe.SizeOf<Vector3D<float>>(), 1),
                            new VertexAttribute(VertexFormat.Float32x2, (uint)Unsafe.SizeOf<Vector4D<float>>(), 2)
                        })
                    }
                },
                _sampleUniformLayout.ShaderModule,
                _view.TextureFormat));

                // Render
            
            render_pass.SetPipeline(renderPipeline.RenderPipeline);
            render_pass.SetBindGroup(0, textureBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetBindGroup(1, _sampleBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
			render_pass.SetBindGroup(2, vertexBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetVertexBuffer(0, _vertexBuffer.Buffer, 0, _vertexBuffer.BufferSize);
            render_pass.SetIndexBuffer(_indexBuffer.Buffer, IndexFormat.Uint32, 0, _indexBuffer.BufferSize);

            render_pass.DrawIndexed((uint)indexData.Length, 1, 0, 0, 0);

            _vertexIndex = 0;
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            _vertexData[_vertexIndex++] = topLeft;
			_vertexData[_vertexIndex++] = topRight;
			_vertexData[_vertexIndex++] = bottomLeft;
			_vertexData[_vertexIndex++] = bottomRight;

			_lastTexture = (TexturePtr)texture;
        }

        private static uint[] GenerateIndexArray()
		{
			uint[] result = new uint[MAX_INDICES];
			for (uint i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
			{
				result[i] = j;
				result[i + 1] = j + 1;
				result[i + 2] = j + 2;
				result[i + 3] = j + 3;
				result[i + 4] = j + 2;
				result[i + 5] = j + 1;
			}
			return result;
		}
    }
}
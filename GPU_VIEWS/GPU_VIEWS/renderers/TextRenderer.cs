using System;
using System.Buffers;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using FontStashSharp.Interfaces;
using GPU_VIEWS.Eto;
using GPU_VIEWS.Eto.Controls;
using GPU_VIEWS.misc;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Wgpu;
using WGPU_TEST;
using wgpu = Wgpu;

namespace GPU_VIEWS.renderers
{
    public class TextRenderer : ITextRenderer
    {
        private const int MAX_SPRITES = 2048;
		private const int MAX_VERTICES = MAX_SPRITES * 4;
		private const int MAX_INDICES = MAX_SPRITES * 6;
		private BufferInternal _vertexBuffer;
		private BufferInternal _indexBuffer;
		private readonly PosColTex[] _vertexData = new PosColTex[MAX_VERTICES];
		private TexturePtr _lastTexture;
        private UniformLayout _sampleUniformLayout;
        private BindGroupInternal _sampleBindGroup;
		private int _vertexIndex = 0;
        private static readonly uint[] indexData = GenerateIndexArray();
        private BindGroupInternal _vertexBindGroup;
        private UniformLayout _vertexUniformLayout;

        private WgpuView _view;
        public int ImageWidth { get; set; }
	    public int ImageHeight { get; set; }
        public TextRenderer(WgpuView view)
        {
            _view = view;
            TextureManager = new TextureManager(view.Device, view.TextureFormat);
            ResourceManager = new ResourceManager(view.Device);

            _vertexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Vertex | BufferUsage.CopyDst | BufferUsage.Uniform,
                size: (ulong)(Unsafe.SizeOf<VertexPositionColorTexture>() * MAX_VERTICES)));

            var idx_data = indexData;
            
            _indexBuffer = ResourceManager.CreateBuffer(new BufferDesc(
                bufferUsage: BufferUsage.Index | BufferUsage.CopyDst,
                size: (ulong)(sizeof(int) * idx_data.Length)
            ));

            var queue = ResourceManager.GetQueue();
            queue.WriteBuffer<uint>(_indexBuffer.Buffer, 0, idx_data.AsSpan());
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

            _vertexUniformLayout = ResourceManager.CreateUniformLayout(new FontVertexTransformUniform(Silk.NET.Maths.Matrix4X4.CreateOrthographicOffCenter<float>(0, 1280, 800, 0, 0, -1)), ShaderType.QuadFont);

            _vertexBindGroup = ResourceManager.CreateBindGroup(
            new ReadOnlySpan<BindGroupLayoutEntry>(new BindGroupLayoutEntry[]
            {
				new BindGroupLayoutEntry{
					Binding = 0,
					Buffer = new BufferBindingLayout
					{
						Type = BufferBindingType.Uniform,
						HasDynamicOffset = false,
						MinBindingSize = _vertexUniformLayout.BufferInternal.BufferSize
					},
					Visibility = ShaderStage.Vertex
				}
            }),
            new ReadOnlySpan<BindGroupEntry>(new BindGroupEntry[]
			{
				new BindGroupEntry
				{
					Binding = 0,
					Buffer = _vertexUniformLayout.BufferInternal.Buffer,
					Offset = 0,
					Size = _vertexUniformLayout.BufferInternal.BufferSize,
				}
			}));
        }

        public void Initialize(Image<Rgba32> image)
        {
            ImageWidth = image.Width;
			ImageHeight = image.Height;

            CreateUniforms();
        }

        public RenderPassEncoderPtr RenderPass { get; set; }

        public void Render(RenderPassEncoderPtr render_pass)
        {
            if (_vertexIndex == 0 || _lastTexture == default)
			{
				return;
			}

            
            // Write index and vertex buffers
            
            var queue = _view.Device.GetQueue();
            var v_data = _vertexData.Take(_vertexIndex).ToArray().AsSpan();
            queue.WriteBuffer<PosColTex>(_vertexBuffer.Buffer, 0, v_data);

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
                    _vertexBindGroup.BindGroupLayout
                }),
                vertexState: new wgpu.VertexState
                {
                    ShaderModule =  _vertexUniformLayout.ShaderModule,
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

                // Render
            render_pass.SetPipeline(renderPipeline.RenderPipeline);
            render_pass.SetBindGroup(0, textureBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetBindGroup(1, _sampleBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
			render_pass.SetBindGroup(2, _vertexBindGroup.BindGroup, ReadOnlySpan<uint>.Empty);
            render_pass.SetVertexBuffer(0, _vertexBuffer.Buffer, 0, _vertexBuffer.BufferSize);
            render_pass.SetIndexBuffer(_indexBuffer.Buffer, IndexFormat.Uint32, 0, _indexBuffer.BufferSize);
            var idx = _vertexIndex * 6 / 4;
            render_pass.DrawIndexed((uint)idx, 1, 0, 0, 0);
            _vertexIndex = 0;
        }
        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            var tex = (TexturePtr)texture;

            var m_topleft = topLeft.ToMe();
            var m_topright = topRight.ToMe();
            var m_bottomleft = bottomLeft.ToMe();
            var m_bottomright = bottomRight.ToMe();

            // m_topleft.Position = Convert(m_topleft.Position);
            // m_topright.Position = Convert(m_topright.Position);
            // m_bottomleft.Position = Convert(m_bottomleft.Position);
            // m_bottomright.Position = Convert(m_bottomright.Position);

            // m_topleft.TextureCoordinate.Y = 1 - m_topleft.TextureCoordinate.Y;
            // m_topright.TextureCoordinate.Y = 1 - m_topright.TextureCoordinate.Y;
            // m_bottomleft.TextureCoordinate.Y = 1 - m_bottomleft.TextureCoordinate.Y;
            // m_bottomright.TextureCoordinate.Y = 1 - m_bottomright.TextureCoordinate.Y;
        
            _vertexData[_vertexIndex++] = m_topleft;
			_vertexData[_vertexIndex++] = m_topright;
			_vertexData[_vertexIndex++] = m_bottomleft;
			_vertexData[_vertexIndex++] = m_bottomright;

            // _vertexData[_vertexIndex++] = m_topleft;
			// _vertexData[_vertexIndex++] = m_topright;
			// _vertexData[_vertexIndex++] = m_bottomright;
			// _vertexData[_vertexIndex++] = m_bottomleft;

            _lastTexture = tex;
        }

        private Vector4D<float> Convert(Vector4D<float> v)
        {
            var mat = Silk.NET.Maths.Matrix4X4.CreateOrthographicOffCenter<float>(0, 1280, 800, 0, 0, -1);

            var t1 = Vector4D.Transform(v, mat);
            return t1;
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

using Eto;
using Eto.Drawing;
using Eto.Forms;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using Wgpu;
using WGPU_TEST.Processor;

namespace GPU_VIEWS.Eto.Controls
{
    /// <summary>
    /// A simple control that allows drawing with Veldrid.
    /// </summary>
    [Handler(typeof(WgpuView.IHandler))]
    public class WgpuView : Control
    {
        public new interface IHandler : Control.IHandler
        {
            Size RenderSize { get; }
            SurfacePtr CreateSurface(InstancePtr instance);
            bool IsScreenVisible { get; }
        }

        new IHandler Handler => (IHandler)base.Handler;

        public override Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                if (Image != null)
                    Image.Mutate(m => m.Resize(RenderSize.Width, RenderSize.Height));
            }
        }

        public bool IsScreenVisible
        {
            get => Handler.IsScreenVisible;
        }

        public new interface ICallback : Control.ICallback
        {
            void OnInitializeBackend(WgpuView s, InitializeEventArgs e);
            void OnDraw(WgpuView s, EventArgs e);
            void OnResize(WgpuView s, ResizeEventArgs e);
        }

        protected new class Callback : Control.Callback, ICallback
        {
            public void OnInitializeBackend(WgpuView s, InitializeEventArgs e) => s?.InitializeGraphicsBackend(e);
            public void OnDraw(WgpuView s, EventArgs e) => s?.OnDraw(e);
            public void OnResize(WgpuView s, ResizeEventArgs e) => s?.OnResize(e);
        }

        protected override object GetCallback() => new Callback();

        /// <summary>
        /// The render area's size, which may differ from the control's size
        /// (e.g. with high DPI displays).
        /// </summary>
        public Size RenderSize => Handler.RenderSize;
        /// <summary>
        /// The render area's width, which may differ from the control's width
        /// (e.g. with high DPI displays).
        /// </summary>
        public int RenderWidth => RenderSize.Width;
        /// <summary>
        /// The render area's height, which may differ from the control's height
        /// (e.g. with high DPI displays).
        /// </summary>
        public int RenderHeight => RenderSize.Height;

        public const string WgpuInitializedEvent = "WgpuSurface.VeldridInitialized";
        public const string DrawEvent = "WgpuSurface.Draw";
        public const string ResizeEvent = "WgpuSurface.Resize";

        public event EventHandler<InitializeEventArgs> Wgpuninitialized
        {
            add { Properties.AddHandlerEvent(WgpuInitializedEvent, value); }
            remove { Properties.RemoveEvent(WgpuInitializedEvent, value); }
        }
        public event EventHandler<EventArgs> Draw
        {
            add { Properties.AddHandlerEvent(DrawEvent, value); }
            remove { Properties.RemoveEvent(DrawEvent, value); }
        }
        public event EventHandler<ResizeEventArgs> Resize
        {
            add { Properties.AddHandlerEvent(ResizeEvent, value); }
            remove { Properties.RemoveEvent(ResizeEvent, value); }
        }
        private SurfaceCapabilities _surfaceCapabilities;

        private WgpuImageProcessor _renderer;
        public WgpuImageProcessor Renderer
        {
            get
            {
                if(_renderer == null)
                    _renderer = new WgpuImageProcessor(this);
                return _renderer;
            } 
            set
            {
                // DISPOSE
                _renderer = value;
            }
        }

        public WebGPU Wgpu { get; private set; }
        public SurfacePtr Surface { get; private set; }
        public InstancePtr Instance { get; private set; }
        public AdapterPtr Adapter { get; private set; }
        public DevicePtr Device { get; private set; }
        public TextureFormat TextureFormat { get; private set; }

        public WgpuView()
        {

        }
        public void RecreateSwapchain()
        {
            RecreateSwapchain(Handler.RenderSize.Width, Handler.RenderSize.Height);
        }


        private void RecreateSwapchain(int width, int height)
        {
            unsafe
            {
                var c = new SurfaceConfiguration
                {
                    Usage = TextureUsage.RenderAttachment,
                    Device = Device,
                    Format = TextureFormat,
                    PresentMode = PresentMode.Fifo,
                    AlphaMode = _surfaceCapabilities.AlphaModes[0], // unsafe
                    Width = (uint)width,
                    Height = (uint)height,
                };

                Surface.Configure(&c);
            }
        }

        public BackendType Backend { get; private set; }
        public SixLabors.ImageSharp.Image<Rgba32> Image { get; set; }

        private void InitializeGraphicsBackend(InitializeEventArgs e)
        {
            Wgpu = WgpuContext.CreateWgpuContext();
            Instance = InstancePtr.Create(Wgpu, default);

            Surface = Handler.CreateSurface(Instance);

            Adapter = Instance.RequestAdapter(new RequestAdapterOptions
            {
                CompatibleSurface = Surface,
                PowerPreference = PowerPreference.HighPerformance
            }).Result;

            Backend = Adapter.GetProperties().BackendType;

            Device = Adapter.RequestDevice(default).Result;

            TextureFormat = Surface.GetPreferredFormat(Adapter);

            _surfaceCapabilities = Surface.GetCapabilities(Adapter);

            RecreateSwapchain();

            OnVeldridInitialized(e);
        }

        protected virtual void OnDraw(EventArgs e) => Properties.TriggerEvent(DrawEvent, this, e);

        protected virtual void OnResize(ResizeEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            RecreateSwapchain(e.Width, e.Height);

            Properties.TriggerEvent(ResizeEvent, this, e);
        }

        protected virtual void OnVeldridInitialized(InitializeEventArgs e)
        {
            Renderer.Setup();
            Renderer.Render();
            Properties.TriggerEvent(WgpuInitializedEvent, this, e);
        }
    }
}

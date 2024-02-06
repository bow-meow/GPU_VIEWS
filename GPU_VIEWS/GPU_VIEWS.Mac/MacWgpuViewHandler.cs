using Eto.Mac.Forms;
using System;
using Eto.Drawing;
using AppKit;
using GPU_VIEWS.Eto.Controls;
using System.Runtime.InteropServices;
using Wgpu;
using CoreVideo;
using GPU_VIEWS.Mac;
using CoreGraphics;
using Eto.Forms;
using Eto.Mac;

namespace GPU_VIEWS.Mac
{
	public class MacWgpuViewHandler : MacView<MacWgpuView, WgpuView, WgpuView.ICallback>, WgpuView.IHandler
	{
		CVDisplayLink _displayLink;
		Size? _newRenderSize;

		public Size RenderSize => Size.Round((SizeF)Widget.Size * Scale);

		float Scale => Widget.ParentWindow?.LogicalPixelSize ?? 1;

		public override NSView ContainerControl => Control;

		public override bool Enabled { get; set; }

        public bool IsScreenVisible
		{
			get
			{
                if (!Widget.Visible)
                    return false;

                var view = Widget.GetContainerView();

				var scroller = Widget.FindParent<Scrollable>();

				if (scroller == null)
					return true;

                var bounds = view.ConvertRectToView(view.Bounds, Control);

                var scrollPos = scroller.ScrollPosition.ToNS();

                var y_offset = scrollPos.Y;

                var final_x = bounds.X;
                var final_y = y_offset - (bounds.Y * -1); // bounds coordinates appear to be flipped... so unflip them.

                var controlRect = new CGRect(final_x, final_y, bounds.Width, bounds.Height);
                var visibleRect = new CGRect(scrollPos, scroller.ClientSize.ToNS());
                return visibleRect.IntersectsWith(controlRect);
            }
		}

        NSScrollView FindScrollViewContaining(NSView view)
        {
            if (view is NSScrollView scrollView)
            {
                return scrollView;
            }
            else if (view.Superview != null)
            {
                return FindScrollViewContaining(view.Superview);
            }
            else
            {
                return null;
            }
        }

        public MacWgpuViewHandler()
		{
			Control = new MacWgpuView();

            Control.Draw += Control_Draw;
		}

        public SurfacePtr CreateSurface(InstancePtr instance)
        {
            return instance.CreateSurfaceFromMetalLayer(Control.Handle);
        }

		private void Control_Draw(object sender, EventArgs e)
		{
			Callback.OnInitializeBackend(Widget, new InitializeEventArgs(RenderSize));

			if (Widget.Backend == Silk.NET.WebGPU.BackendType.Metal)
			{
				_displayLink = new CVDisplayLink();
				_displayLink.SetOutputCallback(HandleDisplayLinkOutputCallback);
				_displayLink.Start();
			}

			Control.Draw -= Control_Draw;
			Widget.SizeChanged += Widget_SizeChanged;
		}

		private void Widget_SizeChanged(object sender, EventArgs e)
		{
            if (Widget.Backend == Silk.NET.WebGPU.BackendType.OpenGL)
            {
                Callback.OnResize(Widget, new ResizeEventArgs(RenderSize));
            }
            else
            {
                _newRenderSize = RenderSize;
            }
        }

		private CVReturn HandleDisplayLinkOutputCallback(CVDisplayLink displayLink, ref CVTimeStamp inNow, ref CVTimeStamp inOutputTime, CVOptionFlags flagsIn, ref CVOptionFlags flagsOut)
		{
			if (_newRenderSize != null)
			{
				Callback.OnResize(Widget, new ResizeEventArgs(_newRenderSize.Value));
				_newRenderSize = null;
			}

			Callback.OnDraw(Widget, EventArgs.Empty);
			return CVReturn.Success;
		}

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				case WgpuView.DrawEvent:
                    if (Widget.Backend == Silk.NET.WebGPU.BackendType.OpenGL)
                    {
                        Control.Draw += (sender, e) => Callback.OnDraw(Widget, e);
                    }
                    break;
				default:
					base.AttachEvent(id);
					break;
			}
		}
	}
}

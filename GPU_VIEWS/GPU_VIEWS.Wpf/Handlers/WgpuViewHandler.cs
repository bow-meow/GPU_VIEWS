using Eto.Drawing;
using Eto.Veldrid;
using Eto.Veldrid.Wpf;
using GPU_VIEWS.Eto;
using GPU_VIEWS.Eto.Controls;
using GPU_VIEWS.Wpf;
using Microsoft.Extensions.DependencyModel;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Wgpu;

namespace GPU_VIEW.Wpf
{
	public class WgpuViewHandler : ManualBubbleWindowsFormsHostHandler<WinFormsUserControl, WgpuView, WgpuView.ICallback>, WgpuView.IHandler
	{
		public Eto.Drawing.Size RenderSize => Eto.Drawing.Size.Round((SizeF)Widget.Size * Scale);

		float Scale => Widget.ParentWindow?.LogicalPixelSize ?? 1;

		public WgpuViewHandler() : base(new WinFormsUserControl())
		{
			Control.Loaded += Control_Loaded;
		}

		public SurfacePtr CreateSurface(InstancePtr instance)
		{
			return instance.CreateSurfaceFromWindowsHWND(WinFormsControl.Handle, Marshal.GetHINSTANCE(typeof(WgpuView).Module));
		}

		private void Control_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Callback.OnInitializeBackend(Widget, new InitializeEventArgs(RenderSize));

			Control.Loaded -= Control_Loaded;
			Widget.SizeChanged += Widget_SizeChanged;
            Widget.MouseWheel += Widget_MouseWheel;
		}

        private void Widget_MouseWheel(object sender, Eto.Forms.MouseEventArgs e)
        {

        }

        private void Widget_SizeChanged(object sender, EventArgs e)
		{
			Callback.OnResize(Widget, new ResizeEventArgs(RenderSize));
		}

		public bool IsScreenVisible
		{
			get
			{
				var depObj = Widget.ControlObject as WindowsFormsHost;
				var scrollViewer = FindParent<ScrollViewer>(depObj);
				if (scrollViewer == null)
				{
					// No ScrollViewer found in the visual tree.
					return false;
				}

				// Get the control's position relative to the ScrollViewer
				var childTransform = depObj.TransformToAncestor(scrollViewer);
				var rectangle = childTransform.TransformBounds(new Rect(new System.Windows.Point(0, 0), depObj.RenderSize));

				// Create a rect representing the visible area of the ScrollViewer
				Rect visibleRect = new Rect(0, 0, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);

				// Check if the control's position intersects with the ScrollViewer's viewport
				return visibleRect.IntersectsWith(rectangle);
			}
		}

	public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T matched)
            {
                return matched;
            }
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				case WgpuView.DrawEvent:
					WinFormsControl.Paint += (sender, e) =>
					{
						Callback.OnDraw(Widget, EventArgs.Empty);
					};
					break;
				default:
					base.AttachEvent(id);
					break;
			}
		}
	}
}

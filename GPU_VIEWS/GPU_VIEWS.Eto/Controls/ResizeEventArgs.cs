using Eto.Drawing;
using System;

namespace GPU_VIEWS.Eto.Controls
{
	public class InitializeEventArgs : ResizeEventArgs
	{
		public InitializeEventArgs(Size size) : base(size)
		{
		}
	}

	public class ResizeEventArgs : EventArgs
	{
		public Size Size { get; }
		public int Width => Size.Width;
		public int Height => Size.Height;

		public ResizeEventArgs(Size size)
		{
			Size = size;
		}
	}
}

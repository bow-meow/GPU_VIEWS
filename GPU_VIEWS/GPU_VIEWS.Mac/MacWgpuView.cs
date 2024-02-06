using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Mac.Forms;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GPU_VIEWS.Mac
{
	public class MacWgpuView : NSView, IMacControl
	{
		public override bool AcceptsFirstMouse(NSEvent theEvent) => CanFocus;

		public override bool AcceptsFirstResponder() => CanFocus;

		public bool CanFocus { get; set; } = true;

		public WeakReference WeakHandler { get; set; }

		public event EventHandler Draw;

		public override void DrawRect(CGRect dirtyRect)
		{
			Draw?.Invoke(this, EventArgs.Empty);
		}

		public MacWgpuView()
		{
		}
    }
}

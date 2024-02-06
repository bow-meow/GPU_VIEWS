using Eto.Forms;
using GPU_VIEWS.Eto.Controls;
using System;
using plat = Eto.Platforms;

namespace GPU_VIEWS.Mac
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application(plat.macOS);
            app.Platform.Add<WgpuView.IHandler>(() => new MacWgpuViewHandler());
            app.Run(new MainForm());
        }
    }
}

using Eto.Forms;
using Eto.Veldrid.Wpf;
using GPU_VIEW.Wpf;
using GPU_VIEWS.Eto;
using GPU_VIEWS.Eto.Controls;
using System;
using ep = Eto.Platform;

namespace GPU_VIEWS.Wpf
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application(ep.Detect);
            app.Platform.Add<WgpuView.IHandler>(() => new WgpuViewHandler());
            app.Run(new MainForm());
        }
    }
}

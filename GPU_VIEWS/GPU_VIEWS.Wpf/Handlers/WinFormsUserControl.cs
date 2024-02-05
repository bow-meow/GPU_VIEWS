using Eto.Veldrid;
using Eto.Veldrid.Wpf;
using System.Windows.Forms;
using System.Windows.Media;

namespace Eto.Veldrid.Wpf
{
	public class WinFormsUserControl : UserControl
	{
		public WinFormsUserControl()
		{
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            
            DoubleBuffered = false;
        }
	}
}

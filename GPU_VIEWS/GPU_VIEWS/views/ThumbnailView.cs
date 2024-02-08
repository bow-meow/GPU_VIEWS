using GPU_VIEWS.Eto.Controls;
using GPU_VIEWS.renderers;

namespace GPU_VIEWS.views
{
    public class ThumbnailView : WgpuView
    {
        public ThumbnailView()
        {
            Renderer = new WgpuThumbnailRenderer();
        }
    }
}
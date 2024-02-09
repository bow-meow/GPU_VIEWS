using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FontStashSharp;
using GPU_VIEWS.Eto.Controls;
using GPU_VIEWS.renderers;

namespace GPU_VIEWS.views
{
    public class ThumbnailView : WgpuView
    {
        
        private FontSystem fontSystem;
        private float _rads = 0.0f;
        public ThumbnailView()
        {
            base.Wgpuninitialized += Wgpu_Init;

            var settings = new FontSystemSettings
			{
				FontResolutionFactor = 2,
				KernelWidth = 2,
				KernelHeight = 2
			};

			fontSystem = new FontSystem(settings);
			fontSystem.AddFont(File.ReadAllBytes(@"fonts/DroidSans.ttf"));
			fontSystem.AddFont(File.ReadAllBytes(@"fonts/DroidSansJapanese.ttf"));
			fontSystem.AddFont(File.ReadAllBytes(@"fonts/Symbola-Emoji.ttf"));
        }

        private void Wgpu_Init(object sender, EventArgs e)
        {
            Renderer = new WgpuThumbnailRenderer(this);
            Renderer.Initialize(Image);

            base.Draw += Wgpu_Draw;
        }

        private void Wgpu_Draw(object sender, EventArgs e) => DrawD();

        private void DrawD()
        {
            
            var text = "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog";
			var scale = new Vector2(2, 2);

			var font = fontSystem.GetFont(12);

			var size = font.MeasureString(text, scale);
			var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);

            font.DrawText(Renderer.TextRenderer, text, new Vector2(100, 100), FSColor.LightCoral, _rads, origin, scale);
            Renderer.Render();
            _rads += 0.01f;
        }
    }
}
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Eto.Forms;
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
            Renderer = new ImageRenderer(this);
            Renderer.Initialize(Image);

            //base.Draw += Wgpu_Draw;
            Task.Run(async () =>
            {
                while(true)
                {
                    await Task.Delay(20);
                    Application.Instance.Invoke(() =>
                    {
                        DrawD();
                    });

                }
            });
        }

        private void Wgpu_Draw(object sender, EventArgs e) => DrawD();

        private void DrawD()
        {
			var text = "matt and nathan round and round";
			var scale = new Vector2(2, 2);
				
			var font = fontSystem.GetFont(30);
			
			var size = font.MeasureString(text, scale);
			var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);		
            Renderer.Render((pass) =>
            {
                Renderer.TextRenderer.RenderPass = pass;
                font.DrawText(Renderer.TextRenderer, text, new Vector2(400, 400), FSColor.Red, _rads, origin, scale);
                Renderer.TextRenderer.Render(pass);
            });
            _rads += 1.0f;
        }
    }
}
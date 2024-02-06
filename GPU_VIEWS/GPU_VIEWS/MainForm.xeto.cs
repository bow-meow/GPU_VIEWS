using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using GPU_VIEWS.Eto.Controls;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using Color = SixLabors.ImageSharp.Color;
using SharpSize = SixLabors.ImageSharp.Size;
using EtoSize = Eto.Drawing.Size;
using System.IO;
using System.Reflection;

namespace GPU_VIEWS
{
    public class MainForm : Form
    {
        StackLayout MainStack;
        TableLayout MainTable;
        public MainForm()
        {
            XamlReader.Load(this);

            MainStack = new StackLayout();
            MainTable = new TableLayout();

            for(int i = 0; i < 40; i++)
            {
                MainStack.Spacing = 20;
                var img = SixLabors.ImageSharp.Image.Load<Rgba32>(GetImage());
                DrawStuff(img);
                var view = new WgpuView();
                view.Image = img;
                view.Size = new EtoSize(500, 500);
                view.BackgroundColor = Colors.Transparent;
                MainStack.Items.Add(view);
            }

            var v = new WgpuView();
            var im = SixLabors.ImageSharp.Image.Load<Rgba32>(GetImage());
            v.Image = im;
            v.Size = new EtoSize(1000, 1000);

            var scroll = new Scrollable();
            scroll.Content = MainStack;

            MainTable.Rows.Add(new TableRow(new TableCell(scroll, true), new TableCell(v, false)));

            Content = MainTable;
        }

        public Stream GetImage()
        {
            var resourceName = "GPU_VIEWS.assets.Kaioken_high_quality.png";

            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(resourceName);
        }

        public void DrawStuff(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            string topText = "Top Text";
            string bottomText = "Bottom Text";

            SixLabors.Fonts.Font font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 20, SixLabors.Fonts.FontStyle.Bold);

            var txtop = new TextOptions(font);

            var textColor = Color.White;

            var borderColor = Color.FromRgba(100, 0, 0, 200);
            var borderWidth = 20;
            var borderLeftWidth = 10;
            var borderRightWidth = 10;

            var topTextPosition = new SixLabors.ImageSharp.PointF(image.Width / 2, borderWidth);
            var bottomTextPosition = new SixLabors.ImageSharp.PointF(image.Width / 2, image.Height - borderWidth);

            image.Mutate(ctx => ctx
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, 0, image.Width, borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, image.Height - borderWidth, image.Width, borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, borderWidth, borderLeftWidth, image.Height - 2 * borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(image.Width - borderRightWidth, borderWidth, borderRightWidth, image.Height - 2 * borderWidth))
            );

            image.Mutate(ctx => ctx
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, 0, image.Width, borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, image.Height - borderWidth, image.Width, borderWidth))
            );

            image.Mutate(ctx => ctx
                .DrawText(topText, font, textColor, topTextPosition)
                .DrawText(bottomText, font, textColor, bottomTextPosition)
            );
        }

        protected void HandleClickMe(object sender, EventArgs e)
        {
            MessageBox.Show("I was clicked!");
        }

        protected void HandleAbout(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog(this);
        }

        protected void HandleQuit(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }
    }
}

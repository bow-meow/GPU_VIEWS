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

namespace GPU_VIEWS
{
    public class MainForm : Form
    {
        StackLayout MainStack;
        public MainForm()
        {
            XamlReader.Load(this);

            for(int i = 0; i < 40; i++)
            {
                MainStack.Spacing = 20;
                var img = SixLabors.ImageSharp.Image.Load<Rgba32>("C:\\Users\\Blunt\\Pictures\\Kaioken_high_quality.png");
                DrawStuff(img);
                var view = new WgpuView();
                view.Image = img;
                view.Size = new EtoSize(500, 500);
                view.BackgroundColor = Colors.Transparent;
                MainStack.Items.Add(view);
            }
        }

        public void DrawStuff(SixLabors.ImageSharp.Image<Rgba32> image)
        {
            string topText = "Top Text";
            string bottomText = "Bottom Text";

            // Define the font for your text
            SixLabors.Fonts.Font font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 20, SixLabors.Fonts.FontStyle.Bold);

            var txtop = new TextOptions(font);


            // Define the color for your text
            var textColor = Color.White;

            // Define the border color and width
            var borderColor = Color.FromRgba(100, 0, 0, 200); // Adjust the alpha value for transparency
            var borderWidth = 20;
            var borderLeftWidth = 10; // Width of the left border
            var borderRightWidth = 10; // Width of the right border

            // Calculate the position for top and bottom text
            var topTextPosition = new SixLabors.ImageSharp.PointF(image.Width / 2, borderWidth);
            var bottomTextPosition = new SixLabors.ImageSharp.PointF(image.Width / 2, image.Height - borderWidth);

            // Draw the borders
            image.Mutate(ctx => ctx
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, 0, image.Width, borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, image.Height - borderWidth, image.Width, borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, borderWidth, borderLeftWidth, image.Height - 2 * borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(image.Width - borderRightWidth, borderWidth, borderRightWidth, image.Height - 2 * borderWidth))
            );

            // Draw the borders
            image.Mutate(ctx => ctx
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, 0, image.Width, borderWidth))
                .Fill(borderColor, new SixLabors.ImageSharp.RectangleF(0, image.Height - borderWidth, image.Width, borderWidth))
            );

            // Draw the text at the top and bottom
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

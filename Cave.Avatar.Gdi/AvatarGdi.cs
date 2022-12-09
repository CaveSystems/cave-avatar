using System;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Cave.Media;

internal class AvatarGdi
{
    /// <summary>
    /// Retrieves an avatar using only the text on a shaded background.
    /// </summary>
    /// <param name="text">The text / name of the avatar.</param>
    /// <param name="rectSize">Size to use to create the avatar.</param>
    /// <returns>Returns a new Bitmap instance.</returns>
    public static IBitmap32 GetText(string text, int rectSize)
    {
        var str = (text.Length > 3) ? text.Substring(0, 3) : text;
        var data = Hash.FromString(Hash.Type.MD5, text);
        var colors = new ARGB[4];

        var n = 0;
        for (var i = 0; i < 4; i++)
        {
            colors[i].AsInt32 = BitConverter.ToInt32(data, n);
            colors[i].Alpha = 255;
            n += 3;
        }
        var rot1 = data[n++] * 360 / 256;
        var rot2 = data[n++] * 360 / 256;

        var rect = new Rectangle(0, 0, rectSize, rectSize);
        var result = new Bitmap(rectSize, rectSize);
        using (var g = Graphics.FromImage(result))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // background
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, colors[0], colors[1], rot1))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            // text
            {
                var format = new System.Drawing.StringFormat(StringFormatFlags.NoClip)
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center,
                };
                var fontSize = (float)(2.5 * rectSize / (text.Length * Math.Sqrt(text.Length)));
                using (var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var pen = new Pen(colors[2]))
                using (var brush = new LinearGradientBrush(rect, colors[2], colors[3], rot2))
                using (var path = new GraphicsPath())
                {
                    pen.Width = fontSize / 30f;
                    pen.LineJoin = LineJoin.Round;
                    path.AddString(text, font.FontFamily, (int)font.Style, font.Size, rect, format);
                    g.DrawPath(pen, path);
                    g.FillPath(brush, path);
                }
            }
        }
        return new GdiBitmap32(result);
    }

    /// <summary>
    /// Retrieves an avatar using only the first letter on a shaded background.
    /// </summary>
    /// <param name="text">The text / name of the avatar.</param>
    /// <param name="size">Size to use to create the avatar.</param>
    /// <param name="drawBackground">The background to use.</param>
    /// <returns>Returns a new Bitmap instance.</returns>
    public static IBitmap32 GetFirstLetter(string text, int size, bool drawBackground)
    {
        var c = text.Length > 0 ? text[0] : ' ';
        var data = Hash.FromString(Hash.Type.MD5, text);
        var colors = new ARGB[4];

        var n = 0;
        for (var i = 0; i < 4; i++)
        {
            colors[i].AsInt32 = BitConverter.ToInt32(data, n);
            colors[i].Alpha = 255;
            n += 3;
        }
        var rot1 = data[n++] * 360 / 256;
        var rot2 = data[n++] * 360 / 256;

        var rect = new Rectangle(0, 0, size, size);
        var result = new Bitmap(size, size);
        using (var g = Graphics.FromImage(result))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // background
            if (drawBackground)
            {
                using (var brush = new LinearGradientBrush(rect, colors[0], colors[1], rot1))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            // text
            {
                var format = new StringFormat(StringFormatFlags.NoClip)
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center,
                };
                using (var font = new Font(FontFamily.GenericSansSerif, size * 3f / 4f, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    using (var pen = new Pen(colors[2]))
                    using (var brush = new LinearGradientBrush(rect, colors[2], colors[3], rot2))
                    using (var path = new GraphicsPath())
                    {
                        pen.Width = size / 30f;
                        pen.LineJoin = LineJoin.Round;
                        path.AddString(c.ToString(), font.FontFamily, (int)font.Style, font.Size, rect, format);
                        g.DrawPath(pen, path);
                        g.FillPath(brush, path);
                    }
                }
            }
        }
        return new GdiBitmap32(result);
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Cave.IO;
using Cave.Net;

#if NET20 || NET35 || NET40 || NET45 || NET46 || NET471
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
#elif NETSTANDARD20
#else
#error No code defined for the current framework or NETXX version define missing!
#endif

#if SKIA && (NETSTANDARD20 || NET45 || NET46 || NET471)
using SkiaSharp;
#elif NET20 || NET35 || NET40 || !SKIA
#else
#error No code defined for the current framework or NETXX version define missing!
#endif

namespace Cave.Media.Video
{
    /// <summary>Provides a class for generating avatars using gravatar.</summary>
    public class Avatar
    {
        /// <summary>
        /// Contains an array with all types of avatars.
        /// </summary>
        public static readonly IReadOnlyList<AvatarType> Types = new ReadOnlyCollection<AvatarType>((AvatarType[])Enum.GetValues(typeof(AvatarType)));

        string name;
        AvatarType type;
        int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="Avatar"/> class.
        /// </summary>
        /// <param name="name">Name of the user.</param>
        /// <param name="type">Type.</param>
        /// <param name="size">Size in pixel.</param>
        public Avatar(string name, AvatarType type, int size)
        {
            this.name = name;
            this.type = type;
            this.size = size;
        }

        /// <summary>
        /// Gets or sets the name of the avatar.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the type of the avatar.
        /// </summary>
        public AvatarType Type
        {
            get => type;
            set
            {
                type = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the size of the avatar.
        /// </summary>
        public int Size
        {
            get => size;
            set
            {
                size = value;
                Invalidate();
            }
        }

        /// <summary>Invalidates this instance.</summary>
        public void Invalidate()
        {
#if NET20 || NET35 || NET40 || NET45 || NET46 || NET471
            bitmap = null;
#elif NETSTANDARD20
#else
#error No code defined for the current framework or NETXX version define missing!
#endif
#if SKIA && (NETSTANDARD20 || NET45 || NET46 || NET471)
			m_SKBitmap = null;
#elif NET20 || NET35 || NET40 || !SKIA
#else
#error No code defined for the current framework or NETXX version define missing!
#endif
        }

#if NET20 || NET35 || NET40 || NET45 || NET46 || NET471
        #region static class

        /// <summary>
        /// Gets the Gravatar with the specified properties.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <param name="type">Avatar type.</param>
        /// <param name="rectSize">Size of the bitmap.</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetGravatar(string text, AvatarType type, int rectSize)
        {
            string hash = StringExtensions.ToHexString(Hash.FromString(Hash.Type.MD5, text));
            byte[] data = HttpConnection.Get("http://www.gravatar.com/avatar/" + hash + "?d=" + type.ToString() + "&s=" + rectSize);
            var result = new System.Drawing.Bitmap(new MemoryStream(data));

            switch (type)
            {
                case AvatarType.IdentIcon: result.MakeTransparent(Color.White); break;
                case AvatarType.MonsterId: result.MakeTransparent(); break;
                case AvatarType.Retro: result.MakeTransparent(); break;
            }
            return result;
        }

        /// <summary>
        /// Retrieves an avatar using only the text on a shaded background.
        /// </summary>
        /// <param name="text">The text / name of the avatar.</param>
        /// <param name="type">The avatar type.</param>
        /// <param name="rectSize">Size to use to create the avatar.</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetText(string text, AvatarType type, int rectSize)
        {
            string str = (text.Length > 3) ? text.Substring(0, 3) : text;
            byte[] data = Hash.FromString(Hash.Type.MD5, text);
            var colors = new ARGB[4];

            int n = 0;
            for (int i = 0; i < 4; i++)
            {
                colors[i].AsInt32 = BitConverter.ToInt32(data, n);
                colors[i].Alpha = 255;
                n += 3;
            }
            int rot1 = data[n++] * 360 / 256;
            int rot2 = data[n++] * 360 / 256;

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
                    float fontSize = (float)(2.5 * rectSize / (text.Length * Math.Sqrt(text.Length)));
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
            return result;
        }

        /// <summary>
        /// Retrieves an avatar using only the first letter on a shaded background.
        /// </summary>
        /// <param name="text">The text / name of the avatar.</param>
        /// <param name="type">The avatar type.</param>
        /// <param name="size">Size to use to create the avatar.</param>
        /// <param name="drawBackground">The background to use.</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetFirstLetter(string text, AvatarType type, int size, bool drawBackground)
        {
            char c = text.Length > 0 ? text[0] : ' ';
            byte[] data = Hash.FromString(Hash.Type.MD5, text);
            var colors = new ARGB[4];

            int n = 0;
            for (int i = 0; i < 4; i++)
            {
                colors[i].AsInt32 = BitConverter.ToInt32(data, n);
                colors[i].Alpha = 255;
                n += 3;
            }
            int rot1 = data[n++] * 360 / 256;
            int rot2 = data[n++] * 360 / 256;

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
            return result;
        }

        /// <summary>
        /// Obtains an avatar for the specified string.
        /// </summary>
        /// <param name="name">The name of the user to obtain an avatar for.</param>
        /// <param name="type">The type of avatar to use.</param>
        /// <param name="size">Size to use to create avatar.</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetAvatar(string name, AvatarType type, int size)
        {
            switch (type)
            {
                case AvatarType.IdentIcon:
                case AvatarType.MonsterId:
                case AvatarType.Retro:
                case AvatarType.Wavatar: return GetGravatar(name, type, size);
                case AvatarType.FirstLetter: return GetFirstLetter(name, type, size, true);
                case AvatarType.FirstLetterWithoutBackground: return GetFirstLetter(name, type, size, false);
                case AvatarType.Text: return GetText(name, type, size);
                default: throw new NotImplementedException(string.Format("AvatarType {0} is not implemented!", type));
            }
        }

        /// <summary>
        /// Obtains a random avatar.
        /// </summary>
        /// <param name="size">Size to use to create avatar.</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetRandomAvatar(int size)
        {
            AvatarType type = Types[(int)(DefaultRNG.UInt32 % Types.Count)];
            byte[] data = DefaultRNG.Get(32);
            string text = StringExtensions.GetValidChars(ASCII.Strings.Printable, ASCII.GetCleanString(data));
            return GetAvatar(text, type, size);
        }

        /// <summary>
        /// Obtains a random avatar for the specified string.
        /// </summary>
        /// <param name="name">The name of the user to obtain an avatar for.</param>
        /// <param name="size">Size to use to create avatar.</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetRandomAvatar(string name, int size)
        {
            AvatarType type = Types[(int)(DefaultRNG.UInt32 % Types.Count)];
            return GetAvatar(name, type, size);
        }

        /// <summary>
        /// Obtains a random avatar cached avatar.
        /// </summary>
        /// <param name="size">Size of the avatar.</param>
        /// <returns>Returns a new Bitmap instance or null if no avatars are cached locally.</returns>
        public static Bitmap GetRandomCachedAvatar(int size)
        {
            AssemblyVersionInfo ver = AssemblyVersionInfo.Program;
            var files = new List<string>();
            foreach (string path in new string[] { FileSystem.LocalMachineAppData, FileSystem.LocalUserAppData })
            {
                foreach (AvatarType type in Enum.GetValues(typeof(AvatarType)))
                {
                    string filePath = FileSystem.Combine(path, ver.Company, "Avatar", type.ToString(), size.ToString());
                    files.AddRange(Directory.GetFiles(filePath, "*.png"));
                }
            }
            if (files.Count == 0)
            {
                return null;
            }

            return new Bitmap(files[Math.Abs(Environment.TickCount) % files.Count]);
        }

        /// <summary>
        /// Obtains an avatar and caches it under the localmachine (if writable, otherwise localuser) directory.
        /// </summary>
        /// <param name="name">The name of the user to obtain an avatar for.</param>
        /// <param name="type">The type of avatar to use.</param>
        /// <param name="size">the size (32, 48, 64, 96, 128, 192, 256 are good values).</param>
        /// <returns>Returns a new Bitmap instance.</returns>
        public static Bitmap GetCachedAvatar(string name, AvatarType type, int size)
        {
            AssemblyVersionInfo ver = AssemblyVersionInfo.Program;
            Bitmap img = null;
            string hash = StringExtensions.ToHexString(Hash.FromString(Hash.Type.MD5, name));
            foreach (string path in new string[] { FileSystem.LocalMachineAppData, FileSystem.LocalUserAppData })
            {
                bool save = true;
                string fileName = FileSystem.Combine(path, ver.Company, "Avatar", type.ToString(), size.ToString(), hash + ".png");

                // get image from file
                try
                {
                    if (File.Exists(fileName))
                    {
                        img = new Bitmap(fileName);
                        save = false;
                    }
                }
                catch
                {
                }

                // get image from gravatar
                if (img == null)
                {
                    try
                    {
                        img = GetAvatar(name, type, size);
                    }
                    catch
                    {
                    }
                }

                // save image
                if (save)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        img.Save(fileName, ImageFormat.Png);
                    }
                    catch
                    {
                    }
                }

                // set image
                if (img != null)
                {
                    return img;
                }
            }
            return null;
        }
        #endregion

        Bitmap bitmap;

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                if (bitmap == null)
                {
                    bitmap = GetCachedAvatar(name, type, size);
                }
                return bitmap;
            }
        }
#elif NETSTANDARD20
#else
#error No code defined for the current framework or NETXX version define missing!
#endif

#if SKIA && (NETSTANDARD20 || NET45 || NET46 || NET471)
        #region static class
		/// <summary>
		/// Obtains an avatar for the specified string
		/// </summary>
		public static SKBitmap GetGravatarSK(string text, AvatarType type, int rectSize)
        {
            string hash = StringTools.ToHexString(Hash.FromString(Hash.Type.MD5, text));
            byte[] data = HttpConnection.Get("http://www.gravatar.com/avatar/" + hash + "?d=" + type.ToString() + "&s=" + rectSize);
			SKBitmap bitmap = SKBitmap.Decode(data);

            switch (type)
            {
                default: return bitmap;
                    //make transparent:
                case AvatarType.identicon: 
                case AvatarType.monsterid: 
                case AvatarType.retro: break;
            }

			SKBitmap result = new SKBitmap(rectSize, rectSize);
            using (SKCanvas canvas = new SKCanvas(result))
            using (SKPaint paint = new SKPaint())
            using (var colorFilter = SKColorFilter.CreateBlendMode(bitmap.GetPixel(0, 0), SkiaSharp.SKBlendMode.DstIn))
            {
                paint.ColorFilter = colorFilter;
                canvas.DrawBitmap(bitmap, 0, 0, paint);
            }
            bitmap.Dispose();
            return result;
        }

        /// <summary>
        /// Obtains an avatar for the specified string
        /// </summary>
        /// <param name="name">The name of the user to obtain an avatar for</param>
        /// <param name="type">The type of avatar to use</param>
        /// <param name="size">Size to use to create avatar</param>
        /// <returns></returns>
        public static SKBitmap GetAvatarSK(string name, AvatarType type, int size)
        {
            switch (type)
            {
                case AvatarType.identicon:
                case AvatarType.monsterid:
                case AvatarType.retro:
                case AvatarType.wavatar: return GetGravatarSK(name, type, size);
                case AvatarType.FirstLetter: return GetFirstLetterSK(name, type, size, true);
                case AvatarType.FirstLetterWithoutBackground: return GetFirstLetterSK(name, type, size, false);
                case AvatarType.Text: return GetTextSK(name, type, size);
                default: throw new NotImplementedException(string.Format("AvatarType {0} is not implemented!", type));
            }
        }

        /// <summary>
        /// Retrieves an avatar using only the text on a shaded background
        /// </summary>
        /// <param name="text">The text / name of the avatar</param>
        /// <param name="type">The avatar type</param>
        /// <param name="size">Size to use to create the avatar</param>
        /// <returns></returns>
        public static SKBitmap GetTextSK(string text, AvatarType type, int size)
        {
            throw new NotImplementedException("TODO!");
        }

        /// <summary>
        /// Retrieves an avatar using only the first letter on a shaded background
        /// </summary>
        /// <param name="text">The text / name of the avatar</param>
        /// <param name="type">The avatar type</param>
        /// <param name="size">Size to use to create the avatar</param>
        /// <param name="drawBackground">The background to use</param>
        /// <returns></returns>
        public static SKBitmap GetFirstLetterSK(string text, AvatarType type, int size, bool drawBackground)
        {
            throw new NotImplementedException("TODO!");
        }

        /// <summary>
        /// Obtains a random avatar
        /// </summary>
        /// <param name="size">Size to use to create avatar</param>
        /// <returns></returns>
        public static SKBitmap GetRandomAvatarSK(int size)
        {
            AvatarType type = (AvatarType)(DefaultRNG.UInt32 % (uint)AvatarType._Count);
            byte[] data = DefaultRNG.Get(32);
            string text = StringTools.GetValidChars(ASCII.Strings.Printable, ASCII.GetCleanString(data));
            return GetAvatarSK(text, type, size);
        }

        /// <summary>
        /// Obtains a random avatar for the specified string
        /// </summary>
        /// <param name="name">The name of the user to obtain an avatar for</param>
        /// <param name="size">Size to use to create avatar</param>
        /// <returns></returns>
        public static SKBitmap GetRandomAvatarSK(string name, int size)
        {
            AvatarType type = (AvatarType)(DefaultRNG.UInt32 % (uint)AvatarType._Count);
            return GetAvatarSK(name, type, size);
        }

        /// <summary>
        /// Obtains a random avatar cached avatar.
        /// </summary>
        /// <param name="size">Size of the avatar</param>
        /// <returns>Returns null if no avatars are cached locally</returns>
        public static SKBitmap GetRandomCachedAvatarSK(int size)
        {
            AssemblyVersionInfo ver = AssemblyVersionInfo.Program;
            List<string> files = new List<string>();
            foreach (string path in new string[] { Paths.LocalMachineAppData, Paths.LocalUserAppData })
            {
                foreach (AvatarType type in Enum.GetValues(typeof(AvatarType)))
                {
                    string filePath = Paths.Combine(path, ver.Company, "Avatar", type.ToString(), size.ToString());
                    files.AddRange(Directory.GetFiles(filePath, "*.png"));
                }
            }
            if (files.Count == 0) return null;
            return SKBitmap.Decode(files[Math.Abs(Environment.TickCount) % files.Count]);
        }

        /// <summary>
        /// Obtains an avatar and caches it under the localmachine (if writable, otherwise localuser) directory
        /// </summary>
        /// <param name="name">The name of the user to obtain an avatar for</param>
        /// <param name="type">The type of avatar to use</param>
        /// <param name="size">the size (32, 48, 64, 96, 128, 192, 256 are good values)</param>
        /// <returns></returns>
        public static SKBitmap GetCachedAvatarSK(string name, AvatarType type, int size)
        {
            AssemblyVersionInfo ver = AssemblyVersionInfo.Program;
			SKBitmap bitmap = null;
            string hash = StringTools.ToHexString(Hash.FromString(Hash.Type.MD5, name));
            foreach (string path in new string[] { Paths.LocalMachineAppData, Paths.LocalUserAppData })
            {
                bool save = true;
                string fileName = Paths.Combine(path, ver.Company, "Avatar", type.ToString(), size.ToString(), hash + ".png");
                //get image from file
                try
                {
                    if (File.Exists(fileName))
                    {
                        bitmap = SKBitmap.Decode(fileName);
                        save = false;
                    }
                }
                catch { }
                //get image from gravatar
                if (bitmap == null)
                {
                    try
                    {
                        bitmap = GetAvatarSK(name, type, size);
                    }
                    catch { }
                }
                //save image
                if (save)
                {
                    try
                    {
                        Directory.CreateDirectory(FileSystem.Instance.GetDirectoryName(fileName));
                        bitmap.Save(fileName);
                    }
                    catch { }
                }
                //set image
                if (bitmap != null)
                {
                    return bitmap;
                }
            }
            return null;
        }
        #endregion

		SKBitmap m_SKBitmap;

        /// <summary>
        /// Obtains the bitmap
        /// </summary>
        public SKBitmap SKBitmap
        {
            get
            {
                if (m_SKBitmap == null)
                {
                    m_SKBitmap = GetCachedAvatarSK(m_Name, m_Type, m_Size);
                }
                return m_SKBitmap;
            }
        }
#elif NET20 || NET35 || NET40 || !SKIA
#else
#error No code defined for the current framework or NETXX version define missing!
#endif
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cave.IO;
using Cave.Media;

namespace Cave.Web.Avatar
{
    /// <summary>
    /// Provides an avatar image api.
    /// </summary>
    public class AvatarInterface
    {
        const int defaultSize = 600;

        class Item
        {
            public string FileName;
            public Bitmap32 Bitmap;
        }

        /// <summary>Gets or sets the size of the avatar.</summary>
        /// <value>The size of the avatar.</value>
        public int AvatarSize { get; set; } = defaultSize;

        /// <summary>Gets or sets the small space.</summary>
        /// <value>The small space.</value>
        public int SmallSpace { get; set; } = defaultSize / 12;

        /// <summary>
        /// Gets the number of available combinations.
        /// </summary>
        public long AvailableCombinations { get; }

        CRC32 crc32 = new CRC32();
        IList<Item> faces;
        IList<Item> noses;
        IList<Item> eyes;
        IList<Item> mouths;

        /// <summary>Initializes a new instance of the <see cref="AvatarInterface"/> class.</summary>
        public AvatarInterface(string imagePath)
        {
            noses = LoadImages(FileSystem.Combine(imagePath, "noses"));
            mouths = LoadImages(FileSystem.Combine(imagePath, "mouths"));
            eyes = LoadImages(FileSystem.Combine(imagePath, "eyes"));
            faces = LoadImages(FileSystem.Combine(imagePath, "faces"));

            AvailableCombinations = 256; // colors
            AvailableCombinations *= noses.Count * 2;
            AvailableCombinations *= mouths.Count * 2;
            AvailableCombinations *= eyes.Count * 2;
            AvailableCombinations *= faces.Count * 2;
        }

        IList<Item> LoadImages(string path)
        {
            var results = new List<Item>();
            foreach (string file in Directory.GetFiles(path, "*.png"))
            {
                Trace.TraceInformation("Load {0}", file);
                Bitmap32 bitmap = Bitmap32.FromFile(file);
                results.Add(new Item() { FileName = file, Bitmap = bitmap });
            }
            return results;
        }

        void DrawFace(int index, ARGB color, IList<Item> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                Item item = bmp[index];
                bitmap = item.Bitmap.Data;
            }

            for (int n = 0; n < bitmap.Data.Length; n++)
            {
                ARGB pixel = bitmap[n];
                if (pixel.Alpha == 0)
                {
                    continue;
                }

                pixel.Red = (byte)(pixel.Red * color.Red / 255);
                pixel.Green = (byte)(pixel.Green * color.Green / 255);
                pixel.Blue = (byte)(pixel.Blue * color.Blue / 255);
                bitmap[n] = pixel;
            }

            {
                int x = (AvatarSize - bitmap.Width) / 2;
                int y = (AvatarSize - bitmap.Height) / 2;
                int w = bitmap.Width;
                int h = bitmap.Height;
                Translation? translation = null;
                switch (i)
                {
                    default:
                    case 0: // draw normal;
                        break;
                    case 1: // draw flipped
                        translation = new Translation() { FlipHorizontally = true };
                        break;
                }
                target.Draw(bitmap, x, y, w, h, translation);
                Trace.TraceInformation("DrawFace {0} {1} {2} {3} {4} {5} Flip: {6}", index, x, y, w, h, color, translation.HasValue);
            }
        }

        void DrawEyes(int index, IList<Item> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                Item item = bmp[index];
                bitmap = item.Bitmap.Data;
            }

            int x = (AvatarSize - bitmap.Width) / 2;
            int y = (AvatarSize - bitmap.Height) / 2;
            int w, h;
            Translation? translation = null;
            switch (i)
            {
                default:
                case 0: // draw normal;
                    w = bitmap.Width;
                    h = bitmap.Height;
                    break;
                case 1: // draw flipped, streched vert - 1/8, streched horiz + 1/4, move up
                    w = bitmap.Width * 7 / 8;
                    h = bitmap.Height * 5 / 4;
                    x = (AvatarSize - w) / 2;
                    y = (AvatarSize - h) / 2;
                    translation = new Translation() { FlipHorizontally = true };
                    break;
            }
            target.Draw(bitmap, x, y, w, h, translation);
            Trace.TraceInformation("DrawEyes {0} {1} {2} {3} {4} Flip:{5}", index, x, y, w, h, translation.HasValue);
        }

        void DrawMouth(int index, IList<Item> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                var item = bmp[index];
                bitmap = item.Bitmap.Data;
            }

            int x = (AvatarSize - bitmap.Width) / 2;
            int y = (AvatarSize - bitmap.Height) / 2;
            int w, h;
            Translation? translation = null;
            switch (i)
            {
                default:
                case 0: // draw normal;
                    w = bitmap.Width;
                    h = bitmap.Height;
                    break;
                case 1: // draw flipped
                    y = y + SmallSpace;
                    w = bitmap.Width;
                    h = bitmap.Height - SmallSpace;
                    translation = new Translation() { FlipHorizontally = true };
                    break;
            }
            target.Draw(bitmap, x, y, w, h, translation);
            Trace.TraceInformation("DrawMouth {0} {1} {2} {3} {4} Flip: {5}", index, x, y, w, h, translation.HasValue);
        }

        void DrawNose(int index, IList<Item> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                var item = bmp[index];
                bitmap = item.Bitmap.Data;
            }

            int x = (AvatarSize - bitmap.Width) / 2;
            int y = (AvatarSize - bitmap.Height) / 2;
            int w, h;
            Translation? translation = null;
            switch (i)
            {
                default:
                case 0: // draw normal;
                    w = bitmap.Width;
                    h = bitmap.Height;
                    break;
                case 1: // draw streched in center
                    w = bitmap.Width * 3 / 4;
                    h = bitmap.Height * 3 / 4;
                    x = (AvatarSize - w) / 2;
                    y = (AvatarSize - h) / 2;
                    translation = new Translation() { FlipHorizontally = true };
                    break;
            }
            target.Draw(bitmap, x, y, w, h, translation);
            Trace.TraceInformation("DrawNose {0} {1} {2} {3} {4} Flip: {5}", index, x, y, w, h, translation.HasValue);
        }

        void Draw(WebData data, int color, int nose, int eyes, int mouth, int face, int rotate)
        {
            using (MemoryStream stream = new MemoryStream())
            using (Bitmap32 bmp = new Bitmap32(AvatarSize, AvatarSize))
            {
                ARGB faceColor = ARGB.FromHSI(color / 256.0f, 1, 1);
                DrawFace(face, faceColor, faces, bmp);
                DrawEyes(eyes, this.eyes, bmp);
                DrawMouth(mouth, mouths, bmp);
                DrawNose(nose, noses, bmp);
                using (Bitmap32 result = new Bitmap32(AvatarSize, AvatarSize))
                {
                    result.Draw(bmp, 0, 0, new Translation() { Rotation = ((rotate % 16) - 7) * 0.02f });
                    result.Save(stream);
                }
                WebMessage msg = WebMessage.Create(data.Method, "Avatar created");
                data.Answer = WebAnswer.Raw(data.Request, msg, stream.GetBuffer(), "image/png");
                data.Answer.AllowCompression = false;
                data.Answer.SetCacheTime(TimeSpan.FromDays(1));
            }
        }

        /// <summary>Retrieves an avatar with specific settings. All unset values will be randomized.</summary>
        /// <param name="data">The data.</param>
        /// <param name="color">The color (0..255).</param>
        /// <param name="nose">The nose (0..31).</param>
        /// <param name="eyes">The eyes (0..31).</param>
        /// <param name="mouth">The mouth (0..31).</param>
        /// <param name="face">The face (0..31).</param>
        /// <param name="rotate">The rotation (0..15).</param>
        /// <remarks>Returns a raw png image of resolution 600x600.</remarks>
        [WebPage(Paths = "/avatar/values")]
        public void AvatarValues(WebData data, int? color = null, int? nose = null, int? eyes = null, int? mouth = null, int? face = null, int? rotate = null)
        {
            if (!color.HasValue)
            {
                color = DefaultRNG.UInt16;
            }

            if (!nose.HasValue)
            {
                nose = DefaultRNG.UInt16;
            }

            if (!eyes.HasValue)
            {
                eyes = DefaultRNG.UInt16;
            }

            if (!mouth.HasValue)
            {
                mouth = DefaultRNG.UInt16;
            }

            if (!face.HasValue)
            {
                face = DefaultRNG.UInt16;
            }

            if (!rotate.HasValue)
            {
                rotate = DefaultRNG.UInt16 % 32;
            }

            Draw(data, color.Value, nose.Value, eyes.Value, mouth.Value, face.Value, rotate.Value);
        }

        /// <summary>Retrieves an avatar for the specified plaintext or lower 32 bits of the identifier).</summary>
        /// <param name="data">The data.</param>
        /// <param name="text">The text (using a crc32 to calculate the avatar).</param>
        /// <param name="id">The identifier (using only bits 0..31).</param>
        /// <remarks>Returns a raw png image of resolution 600x600.</remarks>
        [WebPage(Paths = "/avatar/get")]
        public void Avatar(WebData data, string text = null, long? id = null)
        {
            if (!id.HasValue)
            {
                if (text == null)
                {
                    id = DefaultRNG.UInt32;
                }
                else
                {
                    CRC32 crc = (CRC32)crc32.Clone();
                    crc.Update(Encoding.UTF8.GetBytes(text));
                    id = crc.Value;
                }
            }

            int color = (int)(id & 0xFF);
            id >>= 8;
            int nose = (int)(id & 0x1F);
            id >>= 5;
            int eyes = (int)(id & 0x1F);
            id >>= 5;
            int mouth = (int)(id & 0x1F);
            id >>= 5;
            int face = (int)(id & 0x1F);
            id >>= 5;
            int rotate = (int)(id & 0x0F);
            id >>= 4;

            // 32 bit used
            Draw(data, color, nose, eyes, mouth, face, rotate);
        }

        /// <summary>Retrieves an avatar for a random identifier, describing the selection and providing a link.</summary>
        /// <param name="data">The data.</param>
        /// <param name="color">The color (0..255).</param>
        /// <param name="nose">The nose (0..31).</param>
        /// <param name="eyes">The eyes (0..31).</param>
        /// <param name="mouth">The mouth (0..31).</param>
        /// <param name="face">The face (0..31).</param>
        /// <param name="rotate">The rotation (0..15).</param>
        /// <remarks>Returns a page with the image of resolution 600x600.</remarks>
        [WebPage(Paths = "/avatar/test")]
        public void AvatarTest(WebData data, long? color = null, long? nose = null, long? eyes = null, long? mouth = null, long? face = null, long? rotate = null)
        {
            if (!color.HasValue)
            {
                color = DefaultRNG.UInt16 & 0xFF;
            }

            if (!nose.HasValue)
            {
                nose = DefaultRNG.UInt16 & 0x1F;
            }

            if (!eyes.HasValue)
            {
                eyes = DefaultRNG.UInt16 & 0x1F;
            }

            if (!mouth.HasValue)
            {
                mouth = DefaultRNG.UInt16 & 0x1F;
            }

            if (!face.HasValue)
            {
                face = DefaultRNG.UInt16 & 0x1F;
            }

            if (!rotate.HasValue)
            {
                rotate = DefaultRNG.UInt16 & 0x0F;
            }

            long id = (color.Value & 0xFF) | (nose.Value & 0x1F) << 8 | (eyes.Value & 0x1F) << 13 | (mouth.Value & 0x1F) << 18 | (face.Value & 0x1F) << 23 | (rotate.Value & 0x0F) << 28;

            HtmlPageBuilder html = new HtmlPageBuilder(data.Request);

            html.Breadcrump.Add(new WebLink() { Link = "/avatar/get?id=" + id, Text = "Avatar " + id });
            html.Content.CardOpen(Bootstrap4.GetLink("Avatar ID " + id, "/Avatar?id=" + id));
            html.Content.Image("/avatar/get?id=" + id, "img-fluid");
            html.Content.ListGroupOpen();
            html.Content.ListGroupItemOpen();
            html.Content.AddHtml(Bootstrap4.GetLink("<<", $"/avatar/test?color={color - 1}&nose={nose}&eyes={eyes}&mouth={mouth}&face={face}&rotate={rotate}"));
            html.Content.Text($"Color: {color}");
            html.Content.AddHtml(Bootstrap4.GetLink(">>", $"/avatar/test?color={color + 1}&nose={nose}&eyes={eyes}&mouth={mouth}&face={face}&rotate={rotate}"));
            html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen();
            html.Content.AddHtml(Bootstrap4.GetLink("<<", $"/avatar/test?color={color}&nose={nose - 1}&eyes={eyes}&mouth={mouth}&face={face}&rotate={rotate}"));
            html.Content.Text("Nose: " + nose);
            html.Content.AddHtml(Bootstrap4.GetLink(">>", $"/avatar/test?color={color}&nose={nose + 1}&eyes={eyes}&mouth={mouth}&face={face}&rotate={rotate}"));
            html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen();
            html.Content.AddHtml(Bootstrap4.GetLink("<<", $"/avatar/test?color={color }&nose={nose}&eyes={eyes - 1}&mouth={mouth}&face={face}&rotate={rotate}"));
            html.Content.Text("Eyes: " + eyes);
            html.Content.AddHtml(Bootstrap4.GetLink(">>", $"/avatar/test?color={color }&nose={nose}&eyes={eyes + 1}&mouth={mouth}&face={face}&rotate={rotate}"));
            html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen();
            html.Content.AddHtml(Bootstrap4.GetLink("<<", $"/avatar/test?color={color }&nose={nose}&eyes={eyes}&mouth={mouth - 1}&face={face}&rotate={rotate}"));
            html.Content.Text("Mouth: " + mouth);
            html.Content.AddHtml(Bootstrap4.GetLink(">>", $"/avatar/test?color={color }&nose={nose}&eyes={eyes}&mouth={mouth + 1}&face={face}&rotate={rotate}"));
            html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen();
            html.Content.AddHtml(Bootstrap4.GetLink("<<", $"/avatar/test?color={color }&nose={nose}&eyes={eyes}&mouth={mouth}&face={face - 1}&rotate={rotate}"));
            html.Content.Text("Face: " + face);
            html.Content.AddHtml(Bootstrap4.GetLink(">>", $"/avatar/test?color={color }&nose={nose}&eyes={eyes}&mouth={mouth}&face={face + 1}&rotate={rotate}"));
            html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen();
            html.Content.AddHtml(Bootstrap4.GetLink("<<", $"/avatar/test?color={color }&nose={nose}&eyes={eyes}&mouth={mouth}&face={face}&rotate={rotate - 1}"));
            html.Content.Text("Rotate: " + rotate);
            html.Content.AddHtml(Bootstrap4.GetLink(">>", $"/avatar/test?color={color }&nose={nose}&eyes={eyes}&mouth={mouth}&face={face}&rotate={rotate + 1}"));
            html.Content.ListGroupItemClose();
            html.Content.ListGroupClose();
            html.Content.CardClose();

            WebMessage msg = WebMessage.Create(data.Method, "Created avatar.");
            data.Answer = html.ToAnswer(msg);
            data.Answer.SetCacheTime(TimeSpan.FromSeconds(1));
        }
    }
}

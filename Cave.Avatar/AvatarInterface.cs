using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cave.Collections.Generic;
using Cave.IO;
using Cave.Media;

namespace Cave.Web.Avatar
{
    /// <summary>
    /// Provides an avatar image api
    /// </summary>
    public class AvatarInterface
    {
        const int defaultSize = 600;

        /// <summary>Gets or sets the size of the avatar.</summary>
        /// <value>The size of the avatar.</value>
        public int AvatarSize { get; set; } = defaultSize;

        /// <summary>Gets or sets the small space.</summary>
        /// <value>The small space.</value>
        public int SmallSpace { get; set; } = defaultSize / 12;

        long AvailableCombinations;
        CRC32 crc32 = new CRC32();
        IList<C<string, Bitmap32>> m_Faces;
        IList<C<string, Bitmap32>> m_Noses;
        IList<C<string, Bitmap32>> m_Eyes;
        IList<C<string, Bitmap32>> m_Mouths;

        /// <summary>Initializes a new instance of the <see cref="AvatarInterface"/> class.</summary>
        public AvatarInterface(string imagePath)
        {
            m_Noses = LoadImages(FileSystem.Combine(imagePath, "noses"));
            m_Mouths = LoadImages(FileSystem.Combine(imagePath, "mouths"));
            m_Eyes = LoadImages(FileSystem.Combine(imagePath, "eyes"));
            m_Faces = LoadImages(FileSystem.Combine(imagePath, "faces"));

            AvailableCombinations = 256; //colors
            AvailableCombinations *= m_Noses.Count * 2;
            AvailableCombinations *= m_Mouths.Count * 2;
            AvailableCombinations *= m_Eyes.Count * 2;
            AvailableCombinations *= m_Faces.Count * 2;
        }

        IList<C<string, Bitmap32>> LoadImages(string path)
        {
            List<C<string, Bitmap32>> results = new List<C<string, Bitmap32>>();
            foreach (string file in Directory.GetFiles(path, "*.png"))
            {
                Trace.TraceInformation("Load {0}", file);
                Bitmap32 bitmap = Bitmap32.FromFile(file);
                results.Add(new C<string, Bitmap32>(file, bitmap));
            }
            return results;
        }

        void DrawFace(int index, ARGB color, IList<C<string, Bitmap32>> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                C<string, Bitmap32> item = bmp[index];
                bitmap = item.V2.Data;
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
                    case 0: //draw normal;
                        break;
                    case 1: // draw flipped
                        translation = new Translation() { FlipHorizontally = true };
                        break;
                }
                target.Draw(bitmap, x, y, w, h, translation);
                Trace.TraceInformation("DrawFace {0} {1} {2} {3} {4} {5}", index, x, y, w, h, translation);
            }
        }

        void DrawEyes(int index, IList<C<string, Bitmap32>> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                C<string, Bitmap32> item = bmp[index];
                bitmap = item.V2.Data;
            }

            int x = (AvatarSize - bitmap.Width) / 2;
            int y = (AvatarSize - bitmap.Height) / 2;
            int w, h;
            Translation? translation = null;
            switch (i)
            {
                default:
                case 0: //draw normal;
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
            Trace.TraceInformation("DrawEyes {0} {1} {2} {3} {4} {5}", index, x, y, w, h, translation);
        }

        void DrawMouth(int index, IList<C<string, Bitmap32>> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                C<string, Bitmap32> item = bmp[index];
                bitmap = item.V2.Data;
            }

            int x = (AvatarSize - bitmap.Width) / 2;
            int y = (AvatarSize - bitmap.Height) / 2;
            int w, h;
            Translation? translation = null;
            switch (i)
            {
                default:
                case 0: //draw normal;
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
            Trace.TraceInformation("DrawMouth {0} {1} {2} {3} {4} {5}", index, x, y, w, h, translation);
        }

        void DrawNose(int index, IList<C<string, Bitmap32>> bmp, Bitmap32 target)
        {
            int i = index / bmp.Count;
            index %= bmp.Count;

            ARGBImageData bitmap;
            lock (this)
            {
                C<string, Bitmap32> item = bmp[index];
                bitmap = item.V2.Data;
            }

            int x = (AvatarSize - bitmap.Width) / 2;
            int y = (AvatarSize - bitmap.Height) / 2;
            int w, h;
            Translation? translation = null;
            switch (i)
            {
                default:
                case 0: //draw normal;
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
            Trace.TraceInformation("DrawNose {0} {1} {2} {3} {4} {5}", index, x, y, w, h, translation);
        }

        void Draw(WebData data, int color, int nose, int eyes, int mouth, int face, int rotate)
        {
            using (MemoryStream stream = new MemoryStream())
            using (Bitmap32 bmp = new Bitmap32(AvatarSize, AvatarSize))
            {
                ARGB faceColor = ARGB.FromHSI(color / 256.0f, 1, 1);
                DrawFace(face, faceColor, m_Faces, bmp);
                DrawEyes(eyes, m_Eyes, bmp);
                DrawMouth(mouth, m_Mouths, bmp);
                DrawNose(nose, m_Noses, bmp);
                using (Bitmap32 result = new Bitmap32(AvatarSize, AvatarSize))
                {
                    result.Draw(bmp, 0, 0, new Translation() { Rotation = (rotate % 16 - 7) * 0.02f });
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
        /// <remarks>Returns a raw png image of resolution 600x600</remarks>
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

        /// <summary>Retrieves an avatar for the specified plaintext or lower 32 bits of the identifier)</summary>
        /// <param name="data">The data.</param>
        /// <param name="text">The text (using a crc32 to calculate the avatar).</param>
        /// <param name="id">The identifier (using only bits 0..31).</param>
        /// <remarks>Returns a raw png image of resolution 600x600</remarks>
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

            int color = (int)(id & 0xFF); id >>= 8;
            int nose = (int)(id & 0x1F); id >>= 5;
            int eyes = (int)(id & 0x1F); id >>= 5;
            int mouth = (int)(id & 0x1F); id >>= 5;
            int face = (int)(id & 0x1F); id >>= 5;
            int rotate = (int)(id & 0x0F); id >>= 4;
            //32 bit used

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
        /// <remarks>Returns a raw png image of resolution 600x600</remarks>
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

            html.Content.ListGroupItemOpen(); html.Content.Text("Color: " + color); html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen(); html.Content.Text("Nose: " + nose); html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen(); html.Content.Text("Eyes: " + eyes); html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen(); html.Content.Text("Mouth: " + mouth); html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen(); html.Content.Text("Face: " + face); html.Content.ListGroupItemClose();
            html.Content.ListGroupItemOpen(); html.Content.Text("Rotate: " + rotate); html.Content.ListGroupItemClose();

            html.Content.ListGroupClose();
            html.Content.CardClose();

            WebMessage msg = WebMessage.Create(data.Method, "Created avatar.");
            data.Answer = html.ToAnswer(msg);
            data.Answer.SetCacheTime(TimeSpan.FromDays(1));
        }
    }
}

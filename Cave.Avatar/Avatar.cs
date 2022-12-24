using System;
using System.Text;

namespace Cave.Media
{
    /// <summary>Provides a class for generating avatars</summary>
    public class Avatar
    {
        IBitmap32 bitmap;
        readonly AvatarBitmapFunction bitmapFunction;
        readonly AvatarUrlFunction? urlFunction;

        /// <summary>
        /// Provides access to to settings for the avatar
        /// </summary>
        public AvatarSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Avatar"/> class.
        /// </summary>
        /// <param name="settings">Settings used to generate the avatar.</param>
        /// <param name="bitmapFunction">Function to retrieve the bitmap for the current settings.</param>
        public Avatar(AvatarSettings settings, AvatarBitmapFunction bitmapFunction, AvatarUrlFunction urlFunction)
        {
            Settings = settings;
            this.bitmapFunction = bitmapFunction;
            this.urlFunction = urlFunction;
            settings.Changed += (v) => Invalidate();
        }

        /// <summary>
        /// Gets or sets the user name or email address.
        /// This is the text that will be hashed and sent to the avatar server at most implementations.
        /// </summary>
        public string Name
        {
            get => Settings.Name;
            set => Settings.Name = value;
        }

        /// <summary>
        /// Gets or sets the size of the avatar.
        /// </summary>
        public int Size
        {
            get => Settings.Size;
            set => Settings.Size = value;
        }

        /// <summary>Invalidates this instance.</summary>
        public void Invalidate()
        {
            if (bitmap is IDisposable disposable)
            {
                disposable.Dispose();
            }
            bitmap = null;
        }

        internal static string AddUrlSettings(string url, AvatarSettings settings, bool skipNameAndSize, bool hasAlreadyOptions)
        {
            StringBuilder sb = new();
            sb.Append(url);
            var first = !hasAlreadyOptions;
            foreach (var setting in settings)
            {
                if (skipNameAndSize)
                {
                    if (setting is AvatarName || setting is AvatarSize) continue;
                }
                if (setting.GetType().IsEnum)
                {
                    continue;
                }
                if (first)
                {
                    sb.Append('?');
                    first = false;
                }
                else
                {
                    sb.Append('&');
                }
                sb.Append(setting.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the avatar url.
        /// </summary>
        public ConnectionString? Url => urlFunction?.Invoke(Settings);

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        public IBitmap32 Bitmap
        {
            get
            {
                if (bitmap == null)
                {
                    bitmap = bitmapFunction(Settings);
                }
                return bitmap;
            }
        }
    }
}

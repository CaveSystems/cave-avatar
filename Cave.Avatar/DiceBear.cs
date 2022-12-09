using Cave.Net;

namespace Cave.Media;

/// <summary>
/// Provides dice bear avatars. See <see href="https://avatars.dicebear.com"/>.
/// </summary>
public class DiceBear
{
    const string baseUrl = "https://avatars.dicebear.com/api/{type}/{hash}.png";

    /// <summary>Creates a new gravatar.</summary>
    /// <param name="name">Name or email address of the user.</param>
    /// <param name="size">Size of the avatar in pixels.</param>
    /// <param name="type">Avatar type</param>
    /// <returns>Returns a new <see cref="Avatar"/> instance.</returns>
    public static Avatar Create(string name, int size, DiceBearType type, DiceBearBackground? background = null)
    {
        var settings = new AvatarSettings(name, size);
        settings.Set(type);
        if (background != null) settings.Set(background.Value);
        return new Avatar(settings, Get);
    }

    static IBitmap32 Get(AvatarSettings settings)
    {
        var name = settings.Name;
        var size = settings.Size;
        var type = settings.Get<DiceBearType>().ToString().SplitCamelCase().JoinSnakeCase().Replace('_', '-');
        var hash = StringExtensions.ToHexString(Hash.FromString(Hash.Type.SHA256, name));
        var url = baseUrl
            .Replace("{type}", type)
            .Replace("{hash}", hash);
        url = Avatar.AddUrlSettings(url, settings, true, false);
        var data = HttpConnection.Get(url);
        var result = Bitmap32.Create(data);
        var transparent = settings.TryGet(out DiceBearBackground background) && background.Color.Alpha == 0;
        if (transparent)
        {
            result.MakeTransparent(background.Color);
        }
        return result;
    }
}

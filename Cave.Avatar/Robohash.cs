using Cave.Net;

namespace Cave.Media;

/// <summary>
/// Provides access to the <see href="https://robohash.org/"/> api.
/// </summary>
public static class Robohash
{
    const string baseUrl = "https://robohash.org/set_set{type}/gravatar=hashed/size={size}x{size}/{hash}.png";

    /// <summary>Creates a new gravatar.</summary>
    /// <param name="name">Name or email address of the user.</param>
    /// <param name="size">Size of the avatar in pixels.</param>
    /// <param name="type">Avatar type</param>
    /// <returns>Returns a new <see cref="Avatar"/> instance.</returns>
    public static Avatar Create(string name, int size, RobohashType type)
    {
        var settings = new AvatarSettings(name, size);
        settings.Set(type);
        return new Avatar(settings, Get, GetUrl);
    }

    static ConnectionString GetUrl(AvatarSettings settings)
    {
        var type = (int)settings.Get<RobohashType>();
        var hash = StringExtensions.ToHexString(Hash.FromString(Hash.Type.MD5, settings.Name));
        var url = baseUrl
            .Replace("{type}", type.ToString())
            .Replace("{size}", settings.Size.ToString())
            .Replace("{hash}", hash);
        url = Avatar.AddUrlSettings(url, settings, true, false);
        return url;
    }

    static IBitmap32 Get(AvatarSettings settings)
    {
        var url = GetUrl(settings);
        var data = HttpConnection.Get(url);
        return Bitmap32.Create(data);
    }
}

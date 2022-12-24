using System.Drawing;
using Cave.Net;

namespace Cave.Media;

/// <summary>
/// Provides access to the <see href="https://gravatar.com/"/> api.
/// </summary>
public static class Gravatar
{
    /// <summary>Creates a new gravatar.</summary>
    /// <param name="name">Name or email address of the user.</param>
    /// <param name="size">Size of the avatar in pixels.</param>
    /// <param name="type">Avatar type</param>
    /// <returns>Returns a new <see cref="Avatar"/> instance.</returns>
    public static Avatar Create(string name, int size, GravatarType type)
    {
        var settings = new AvatarSettings(name, size);
        settings.Set(type);
        return new Avatar(settings, Get, GetUrl);
    }

    static ConnectionString GetUrl(AvatarSettings settings)
    {
        var text = settings.Name;
        var rectSize = settings.Size;
        var type = settings.Get<GravatarType>();
        var hash = StringExtensions.ToHexString(Hash.FromString(Hash.Type.MD5, text));
        var url = "http://www.gravatar.com/avatar/" + hash + "?d=" + type.ToString().ToLower() + "&s=" + rectSize;
        url = Avatar.AddUrlSettings(url, settings, true, true);
        return url;
    }

    static IBitmap32 Get(AvatarSettings settings)
    {
        var type = settings.Get<GravatarType>();
        var url = GetUrl(settings);
        var data = HttpConnection.Get(url);
        var result = Bitmap32.Create(data);
        switch (type)
        {
            case GravatarType.IdentIcon: result.MakeTransparent(Color.White); break;
            case GravatarType.MonsterId: result.MakeTransparent(); break;
            case GravatarType.Retro: result.MakeTransparent(); break;
        }
        return result;
    }
}

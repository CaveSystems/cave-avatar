using Cave.Net;

namespace Cave.Media;

/// <summary>
/// Provides access to the <see href="https://libravatar.org/"/> api.
/// </summary>
public static class Libravatar
{
    /// <summary>Creates a new gravatar.</summary>
    /// <param name="name">Name or email address of the user.</param>
    /// <param name="size">Size of the avatar in pixels.</param>
    /// <param name="type">Avatar type</param>
    /// <returns>Returns a new <see cref="Avatar"/> instance.</returns>
    public static Avatar Create(string name, int size, LibravatarType type)
    {
        var settings = new AvatarSettings(name, size);
        settings.Set(type);
        return new Avatar(settings, Get);
    }

    static IBitmap32 Get(AvatarSettings settings)
    {
        var text = settings.Name;
        var rectSize = settings.Size;
        var type = settings.Get<LibravatarType>();
        var hash = StringExtensions.ToHexString(Hash.FromString(Hash.Type.MD5, text));
        var url = "https://seccdn.libravatar.org/avatar/" + hash + "?d=" + type.ToString().ToLower() + "&s=" + rectSize;
        url = Avatar.AddUrlSettings(url, settings, true, true);
        var data = HttpConnection.Get(url);
        var result = Bitmap32.Create(data);
        switch (type)
        {
            case LibravatarType.IdentIcon: result.MakeTransparent(0xffffff); break;
            case LibravatarType.MonsterId: result.MakeTransparent(); break;
            case LibravatarType.Retro: result.MakeTransparent(); break;
        }
        return result;
    }
}


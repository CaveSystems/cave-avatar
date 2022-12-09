namespace Cave.Media;

/// <summary>
/// Available gravatar types
/// </summary>
public enum LibravatarType
{
    /// <summary>
    /// a geometric pattern based on an email hash
    /// </summary>
    IdentIcon,

    /// <summary>
    /// a generated 'monster' with different colors, faces, etc
    /// </summary>
    MonsterId,

    /// <summary>
    /// generated faces with differing features and backgrounds
    /// </summary>
    Wavatar,

    /// <summary>
    /// awesome generated, 8-bit arcade-style pixelated faces
    /// </summary>
    Retro,

    /// <summary>
    /// Uses a version of <see cref="Robohash"/>.
    /// </summary>
    Robohash,

    /// <summary>
    /// Python Avatar Generator for Absolute Nerds.
    /// </summary>
    Pagan,
}

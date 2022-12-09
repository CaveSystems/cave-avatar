namespace Cave.Media;

/// <summary>
/// Provides a dice bear background color
/// </summary>
public struct DiceBearBackground
{
    /// <summary>
    /// Override the default background color.
    /// </summary>
    public ARGB Color;

    /// <inheritdoc/>
    public override string ToString() => $"b=%23{Color.ToHtmlColor().Substring(1)}";
}

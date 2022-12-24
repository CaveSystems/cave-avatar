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
    public override string ToString() => Color.Alpha == 0 ? "style=transparent" : $"b=%23{(Color.AsUInt32 & 0xFFFFFFu):X6}";
}

namespace Cave.Media.Video
{
    /// <summary>
    /// Provides available (implemented) avatar types.
    /// </summary>
    public enum AvatarType
    {
        /// <summary>
        /// invalid avatar
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// uses the specified text
        /// </summary>
        Text = 0,

        /// <summary>
        /// uses the first letter
        /// </summary>
        FirstLetter = 1,

        /// <summary>
        /// uses the first letter
        /// </summary>
        FirstLetterWithoutBackground,

        /// <summary>
        /// a geometric pattern based on an email hash
        /// </summary>
        identicon,

        /// <summary>
        /// a generated 'monster' with different colors, faces, etc
        /// </summary>
        monsterid,

        /// <summary>
        /// generated faces with differing features and backgrounds
        /// </summary>
        wavatar,

        /// <summary>
        /// awesome generated, 8-bit arcade-style pixelated faces
        /// </summary>
        retro,

        /// <summary>
        /// Do not use this
        /// </summary>
        _Count,
    }
}

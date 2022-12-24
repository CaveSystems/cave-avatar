namespace Cave.Media;

/// <summary>
/// This function creates the avatar using the specified settings.
/// </summary>
/// <param name="settings">Avatar settings.</param>
/// <returns>Returns a new bitmap instance.</returns>
public delegate IBitmap32 AvatarBitmapFunction(AvatarSettings settings);

/// <summary>
/// This function provides the avatar url for the specified settings.
/// </summary>
/// <param name="settings">Avatar settings.</param>
/// <returns>Returns a connection string instance.</returns>
public delegate ConnectionString AvatarUrlFunction(AvatarSettings settings);

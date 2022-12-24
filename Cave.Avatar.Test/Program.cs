using Cave.Media;

namespace Cave.Media.Test;

class Program
{
    static void Main(string[] args)
    {
        Bitmap32.Loader = new GdiBitmap32Loader();

        foreach (GravatarType type in Enum.GetValues(typeof(GravatarType)))
        {
            var avatar = Gravatar.Create("Test", 100, type);
            Console.WriteLine($"Gravatar.{type}.png : {avatar.Url}");
            avatar.Bitmap.Save($"Gravatar.{type}.png");
        }
        foreach (LibravatarType type in Enum.GetValues(typeof(LibravatarType)))
        {
            var avatar = Libravatar.Create("Test", 100, type);
            Console.WriteLine($"Libravatar.{type}.png : {avatar.Url}");
            avatar.Bitmap.Save($"Libravatar.{type}.png");
        }
        foreach (DiceBearType type in Enum.GetValues(typeof(DiceBearType)))
        {
            var avatar = DiceBear.Create("Test", 100, type);
            Console.WriteLine($"DiceBear.{type}.png : {avatar.Url}");
            avatar.Bitmap.Save($"DiceBear.{type}.png");
        }
        foreach (RobohashType type in Enum.GetValues(typeof(RobohashType)))
        {
            var avatar = Robohash.Create("Test", 100, type);
            Console.WriteLine($"Robohash.{type}.png : {avatar.Url}");
            avatar.Bitmap.Save($"Robohash.{type}.png");
        }
    }
}

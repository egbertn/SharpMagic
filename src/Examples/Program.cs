using NCV.SharpMagic;
internal class Program
{
    private static void Main(string[] args)
    {
        using LibMagic dll = new();
        using var mp4 = File.OpenRead("highway.mp4");
        var mp4info = dll.GuessMimeType(mp4, 2048, true);
        Console.WriteLine($"detected mimetype = {mp4info.MimeType}");

        using var jpg = File.OpenRead("gun.jpg");
        var jpgInfo = dll.GuessMimeType(jpg, mimeOnly: true);
        Console.WriteLine($"detected mimetype = {jpgInfo.MimeType}");

        Console.WriteLine($"detected mimetype = {string.Join(',', dll.GuessMimeType(Path.GetFullPath("gun.jpg")).Properties)}");
        Console.WriteLine($"detected mimetype = {string.Join(',', dll.GuessMimeType(Path.GetFullPath("highway.mp4")).Properties)}");
    }
}
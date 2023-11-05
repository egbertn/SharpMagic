using NCV.SharpMagic;
using System;
using System.IO;

internal class Program
{
    //dotnet publish  --runtime linux-arm64 src/Examples -c Release --no-self-contained
    private static void Main()
    {   
        using LibMagic dll = new LibMagic();
        using var mp4 = File.OpenRead("tesje.mp4");
        var (mp4MimeType, _) = dll.GuessMimeType(mp4, 2048, true);
        Console.WriteLine($"detected mimetype = {mp4MimeType}");

        using var jpg = File.OpenRead("gun.jpg");
        var (mimeType, _) = dll.GuessMimeType(jpg, mimeOnly: true);
        Console.WriteLine($"detected mimetype = {mimeType}");

        Console.WriteLine($"detected properties = {string.Join(',', dll.GuessMimeType(Path.GetFullPath("gun.jpg")).Properties)}");
        Console.WriteLine($"detected properties = {string.Join(',', dll.GuessMimeType(Path.GetFullPath("tesje.mp4")).Properties)}");
    }
}
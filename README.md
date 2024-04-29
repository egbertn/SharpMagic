#  Nierop Computer Vision Sharpmagic


tiny C# client class built on top off libmagic Library which exist on must Linux distributions

## Key Features

* Highly optimized for low resource usage.
* Enables using the libmagic library to retrieve mime types or file info (as properties)
* e.g.

>>>detected mimetype = ISO Media,MP4 Base Media v1 [IS0 14496-12:2003]
>>>detected mimetype = JPEG image data,JFIF standard 1.01,aspect ratio,density 1x1,segment length 16,baseline,precision 8,1200x826
>>>components 3

* Supports only linux. Windows not tested and probably will not, since libmagic is not an out of the box component.

## How To Use

To clone and run this application, you'll need [Git](https://git-scm.com) and [Node.js](https://nodejs.org/en/download/) (which comes with [npm](http://npmjs.com)) installed on your computer. From your command line:

```bash
# Clone this repository
$ git clone https://github.com/egbertn/SharpMagic
```

```bash
# Add library to your project
dotnet add package NCV.SharpMagic
```

## Credits

This software uses the following open source packages:

- [Libmagic]
- [Hey-Red](https://github.com/hey-red/Mime/commits?author=hey-red) (inspired this code)

## Usage
Register SharpMagic as singleton
e.g.
``` csharp
// somewhere in program.cs / startup.cs
  services.AddSingleTon<SharpMagic>();
  //or directly:
  SharpMagic sharpMagic = new();

// somewhere in your code
  using var mp4 = File.OpenRead("tesje.mp4");
  var (mp4MimeType, _) = sharpMagic.GuessMimeType(mp4, 2048, true);
  Console.WriteLine($"detected mimetype = {mp4MimeType}");

  using var jpg = File.OpenRead("gun.jpg");
  var (mimeType, _) = sharpMagic.GuessMimeType(jpg, mimeOnly: true);

  Console.WriteLine($"detected properties = {string.Join(',', dll.GuessMimeType(Path.GetFullPath("gun.jpg")).Properties)}");
```

``` text
output:
detected mimetype = video/mp4
detected mimetype = image/jpeg
detected properties = JPEG image data,JFIF standard 1.01,aspect ratio,density 1x1,segment length 16,baseline,precision 8,1200x826,components 3
detected properties = ISO Media,MP4 v2 [ISO 14496-14]

```


## Support

Basically, no support. Maybe if I have time.

## License

MIT
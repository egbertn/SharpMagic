
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

namespace NCV.SharpMagic;

/// <summary>
/// The wrapper for libmagic (linux)
/// </summary>
public sealed class LibMagic :IDisposable
{
    private readonly object locker = new();
    /// <summary>
    /// Libmagic open flags for getting file type
    /// </summary>
    internal const MagicOpenFlags MagicMimeFlags =
        MagicOpenFlags.MAGIC_ERROR |
        MagicOpenFlags.MAGIC_MIME_TYPE |
        MagicOpenFlags.MAGIC_NO_CHECK_COMPRESS |
        MagicOpenFlags.MAGIC_NO_CHECK_ELF |
        MagicOpenFlags.MAGIC_NO_CHECK_APPTYPE;

    internal const MagicOpenFlags MagicTextFlags =
        MagicOpenFlags.MAGIC_ERROR |
        MagicOpenFlags.MAGIC_NO_CHECK_COMPRESS |
        MagicOpenFlags.MAGIC_NO_CHECK_ELF |
        MagicOpenFlags.MAGIC_NO_CHECK_APPTYPE;

    private readonly Lazy<nint> MimeCookie = new(() =>
    {
        nint mimeCookie = magic_open(MagicMimeFlags);

        if (mimeCookie > 0)
        {
            magic_load(mimeCookie, null);
        }
        return mimeCookie;
    });

    private readonly Lazy<nint> TextCookie = new(() =>
   {
       nint textCookie = magic_open(MagicTextFlags);

       if (textCookie > 0)
       {
           var result = magic_load(textCookie, null);
       }
       return textCookie;
   });

    /// <summary>
    /// returns eventual error
    /// </summary>
    /// <returns></returns>
    public string? Error() => Marshal.PtrToStringUTF8(magic_error(MimeCookie.Value));

    /// <summary>
    /// Get mime type from bytes buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="mimeOnly">if true, will only return mimetype</param>
    /// <returns>Mime type as string</returns>
    public (string? MimeType, ICollection<string>? Properties) GuessMimeType(ReadOnlySpan<byte> buffer, bool mimeOnly = false)
    {
        var bufferLength = buffer.Length;
        if (bufferLength == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(buffer));
        }
        if (MimeCookie.Value == 0)
        {
            throw new InvalidOperationException("Invalid cookie");
        }
        nint[] nints = { MimeCookie.Value, TextCookie.Value };
        Collection<string?> strings = new();
        lock (locker)
        {
            foreach (var n in nints)
            {
                var retPtr = magic_buffer(n, MemoryMarshal.AsRef<byte>(buffer), bufferLength);
                if (retPtr > 0)
                {
                    strings.Add(Marshal.PtrToStringAnsi(retPtr));
                }
                if (mimeOnly)
                {
                    strings.Add("");
                    break;
                }
            }
        }
        return (strings[0], strings[1]?.Split(',').Select(s => s.TrimStart()).ToArray());
    }

    /// <summary>
    /// returns mime type plus properties given the file name
    /// </summary>
    /// <param name="file">the file to be investigated</param>
    /// <param name="mimeOnly">if true, will only process mimetype</param>
    /// <returns> a tuple</returns>
    public (string? MimeType, ICollection<string>? Properties) GuessMimeType(string file, bool mimeOnly = false)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }
        if (!File.Exists(file))
        {
            throw new FileNotFoundException();
        }
        nint[] nints = { MimeCookie.Value, TextCookie.Value };
        Collection<string?> strings = new();
        lock (locker)
        {
            foreach (var n in nints)
            {
                var retPtr = magic_file(n, file);
                if (retPtr > 0)
                {
                    strings.Add(Marshal.PtrToStringAnsi(retPtr));
                }
                if (mimeOnly)
                {
                    strings.Add("");
                    break;
                }
            }
        }
        return (strings[0], strings[1]?.Split(',').Select(s => s.TrimStart()).ToArray());
    }

    /// <summary>
    /// given stream will take from the begin of it, 512 bytes by default
    /// to be processed for mimetype and properties
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="bufferSize"></param>
    /// <param name="mimeOnly"></param>
    /// <returns>a tuple</returns>
    public (string? MimeType, ICollection<string>? Properties) GuessMimeType(Stream stream, int bufferSize = 512, bool mimeOnly = false)
    {
        if (stream == null || stream.CanRead == false || stream.Length == 0)
        {
            return default;
        }
        if (stream.Length < bufferSize)
        {
            bufferSize = (int)stream.Length;
        }

        Span<byte> bytes = bufferSize < 2000 ? stackalloc byte[bufferSize] : new byte[bufferSize];
        var backup = stream.Position;
        stream.Position = 0;
        stream.Read(bytes);
        stream.Position = backup;
        return GuessMimeType(bytes, mimeOnly);
    }

    /// <summary>
    /// returns resources
    /// </summary>
    public void Dispose()
    {
        if (MimeCookie.IsValueCreated)
        {
            magic_close(MimeCookie.Value);
        }
        if (TextCookie.IsValueCreated)
        {
            magic_close(TextCookie.Value);
        }
    }

    private const string MAGIC_LIB_PATH = "libmagic.so.1";


    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern nint magic_open(MagicOpenFlags flags);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_load(nint magic_cookie, [In, MarshalAs(UnmanagedType.LPUTF8Str)] string? dbPath);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern void magic_close(nint magic_cookie);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    //[return: MarshalAs(UnmanagedType.LPUTF8Str)]
    private static extern nint magic_file(nint magic_cookie, [In, MarshalAs(UnmanagedType.LPUTF8Str)] string? dbPath);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]

    internal static extern nint magic_buffer(nint magic_cookie, in byte buffer, int length);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    //[return: MarshalAs(UnmanagedType.LPUTF8Str)]
    private static extern nint magic_error(nint magic_cookie);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern MagicOpenFlags magic_getflags(nint magic_cookie);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_setflags(nint magic_cookie, MagicOpenFlags flags);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_check(nint magic_cookie, [In, MarshalAs(UnmanagedType.LPUTF8Str)] string? dbPath);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_compile(nint magic_cookie, [In, MarshalAs(UnmanagedType.LPUTF8Str)] string? dbPath);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_getparam(nint magic_cookie, MagicParams param, out int value);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_setparam(nint magic_cookie, MagicParams param, in int value);

    [DllImport(MAGIC_LIB_PATH, ExactSpelling = true)]
    private static extern int magic_version();
}
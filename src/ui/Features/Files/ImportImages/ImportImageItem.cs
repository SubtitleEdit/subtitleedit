using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public partial class ImportImageItem : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private long _size;
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public TimeSpan Duration { get; set; }
    public byte[] Bytes { get; set; }

    private static readonly Regex TimeCodeFormat1 = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+_\d+$", RegexOptions.Compiled);
    private static readonly Regex TimeCodeFormat1WithExtension = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+_\d+\..+$", RegexOptions.Compiled);
    private static readonly Regex TimeCodeFormat2 = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+$", RegexOptions.Compiled);

    //Video sub finder
    //0_00_01_042__0_00_03_919_01.jpeg
    //0_01_52_320__0_01_53_679_1034008801249004619201080.png
    //InpaintDelogo
    //00_00_36_840__00_00_39_760.png

    public ImportImageItem()
    {
        FileName = string.Empty;
        Bytes = Array.Empty<byte>();
    }

    public ImportImageItem(string fileName)
    {
        FileName = fileName;
        Bytes = FileUtil.ReadAllBytesShared(fileName);
        Size = Bytes.Length;

        var name = Path.GetFileNameWithoutExtension(fileName);
        if (name.Contains("-to-"))
        {
            var arr = name.Replace("-to-", "_").Split('_');
            if (arr.Length == 3 && int.TryParse(arr[1], out var startTime) && int.TryParse(arr[2], out var endTime))
            {
                Start = TimeSpan.FromMilliseconds(startTime);
                End = TimeSpan.FromMilliseconds(endTime);
                Duration = End - Start;
            }
        }
        else if (TimeCodeFormat1.IsMatch(name) || TimeCodeFormat1WithExtension.IsMatch(name) || TimeCodeFormat2.IsMatch(name))
        {
            var arr = name.Replace("__", "_").Split('_');
            if (arr.Length >= 8)
            {
                try
                {
                    Start = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
                    End = new TimeSpan(0, int.Parse(arr[4]), int.Parse(arr[5]), int.Parse(arr[6]), int.Parse(arr[7]));
                    Duration = End - Start;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }

    internal SKBitmap GetBitmap()
    {
        return SKBitmap.Decode(Bytes);
    }
}


using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class PixelFormatItem
{
    public string Codec { get; set; }
    public string Name { get; set; }

    public PixelFormatItem(string codec, string name)
    {
        Codec = codec;
        Name = name;
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Codec))
        {
            return Name;
        }

        return $"{Codec}: {Name}";
    }

    public static List<PixelFormatItem> PixelFormats = new()
    {
        new PixelFormatItem(" ", " "),
        new PixelFormatItem("yuv420p", "8-bit 4:2:0"),
        new PixelFormatItem("yuv422p", "8-bit 4:2:2"),
        new PixelFormatItem("yuv444p", "8-bit 4:4:4"),
        new PixelFormatItem("yuv420p10le", "10-bit 4:2:0"),
        new PixelFormatItem("yuv422p10le", "10-bit 4:2:2"),
        new PixelFormatItem("yuv444p10le", "10-bit 4:4:4")
    };
}

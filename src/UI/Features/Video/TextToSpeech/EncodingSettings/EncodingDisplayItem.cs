using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

public class EncodingDisplayItem
{
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsStereoEnabled { get; set; }

    public EncodingDisplayItem()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    override public string ToString()
    {
        return Name;
    }

    public static List<EncodingDisplayItem> GetDefaultEncodings()
    {
        return new List<EncodingDisplayItem>
        {
            new EncodingDisplayItem { Name = "Default", Code = string.Empty, IsStereoEnabled = false },
            new EncodingDisplayItem { Name = "Copy", Code = "copy", IsStereoEnabled = true },
            new EncodingDisplayItem { Name = "AAC", Code = "aac", IsStereoEnabled = true },
            new EncodingDisplayItem { Name = "AC3", Code = "ac3", IsStereoEnabled = true },
            new EncodingDisplayItem { Name = "EAC3", Code = "eac3", IsStereoEnabled = true },
            new EncodingDisplayItem { Name = "TrueHD", Code = "truehd", IsStereoEnabled = true }
        };
    }
}

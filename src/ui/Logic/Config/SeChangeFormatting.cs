using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeChangeFormatting
{
    public string LastFromType { get; set; }
    public string LastToType { get; set; }
    public string Color { get; set; }

    public SeChangeFormatting()
    {
        LastFromType = ChangeFormattingType.Underline.ToString();
        LastToType = ChangeFormattingType.Color.ToString();
        Color = Colors.Yellow.FromColorToHex(); 
    }
}
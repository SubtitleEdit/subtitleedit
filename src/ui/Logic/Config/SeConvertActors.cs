using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeConvertActors
{
    public string LastFromType { get; set; }
    public string LastToType { get; set; }
    public bool SetColor { get; set; }
    public string Color { get; set; }
    public bool ChangeCasing { get; set; }
    public int CasingType { get; set; }
    public bool OnlyNames { get; set; }

    public SeConvertActors()
    {
        LastFromType = "InlineSquareBrackets";
        LastToType = "Actor";
        SetColor = false;
        Color = Colors.Yellow.FromColorToHex();
        ChangeCasing = false;
        CasingType = 0;
        OnlyNames = false;
    }
}

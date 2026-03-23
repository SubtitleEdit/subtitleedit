namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageConvertActors
{
    public string Title { get; set; }
    public string ConvertActorFrom { get; set; }
    public string ConvertActorTo { get; set; }
    public string InlineActorViaX { get; set; }
    public string SetColor { get; set; }
    public string OnlyNames { get; set; }
    public string NumberOfConversionsX { get; set; }

    public LanguageConvertActors()
    {
        Title = "Convert actors";
        ConvertActorFrom = "Convert actor from";
        ConvertActorTo = "Convert actor to";
        InlineActorViaX = "Inline actor via {0}";
        SetColor = "Set color";
        OnlyNames = "Only names";
        NumberOfConversionsX = "Number of conversions: {0}";
    }
}
namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// One subtitle line as Subtitle Edit holds it in memory - every field the loaded format
/// filled in, not only what survives a SubRip round trip (e.g. the EBU STL teletext row in
/// <see cref="MarginV"/>, ASSA style/actor/layer). Exchanged in
/// <see cref="PluginSubtitle.Paragraphs"/>.
/// </summary>
public class PluginParagraph
{
    public double StartMs { get; set; }
    public double EndMs { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Style { get; set; }
    public string? Actor { get; set; }
    public string? Language { get; set; }
    public string? Region { get; set; }
    public string? Effect { get; set; }
    public string? Extra { get; set; }
    public string? MarginL { get; set; }
    public string? MarginR { get; set; }

    /// <summary>For EBU STL / DVB Teletext: the teletext row (vertical position) the line starts on.</summary>
    public string? MarginV { get; set; }

    public int Layer { get; set; }
    public bool IsComment { get; set; }
    public bool Forced { get; set; }
    public bool NewSection { get; set; }
    public string? Bookmark { get; set; }
}

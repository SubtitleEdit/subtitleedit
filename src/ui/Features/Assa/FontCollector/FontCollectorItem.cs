using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.FontCollector;

public partial class FontCollectorItem : ObservableObject
{
    [ObservableProperty] private string _fontName;
    [ObservableProperty] private string _usedIn;
    [ObservableProperty] private string _status;
    [ObservableProperty] private string _fileDisplay;

    public List<string> FoundFiles { get; } = new();

    /// <summary>Set when the font is embedded in the subtitle as a [Fonts] attachment -
    /// the decoded font file bytes and the attachment's file name.</summary>
    public byte[]? EmbeddedFontBytes { get; set; }
    public string EmbeddedFileName { get; set; } = string.Empty;

    public FontCollectorItem(string fontName, string usedIn)
    {
        _fontName = fontName;
        _usedIn = usedIn;
        _status = string.Empty;
        _fileDisplay = string.Empty;
    }

    public void UpdateFileDisplay()
    {
        FileDisplay = string.Join(", ", FoundFiles.Select(Path.GetFileName));
    }
}

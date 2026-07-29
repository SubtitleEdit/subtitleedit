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

using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public partial class GetSpellCheckDictionaryDisplay : ObservableObject
{
    [ObservableProperty] private string _englishName;
    [ObservableProperty] private string _nativeName;
    [ObservableProperty] private string _description;

    /// <summary>True when the dictionary is already present in the dictionaries folder.</summary>
    [ObservableProperty] private bool _isInstalled;

    /// <summary>Green when installed, gray when not - shown as a dot in the dictionary list.</summary>
    [ObservableProperty] private IBrush _statusBrush;

    /// <summary>
    /// One or more download URLs for the dictionary. A LibreOffice entry has a direct
    /// .aff and .dic link, while a legacy entry has a single .oxt/.zip/.xpi archive link.
    /// </summary>
    public List<string> Files { get; set; }

    public bool UseShortName { get; set; }

    public string DisplayName => ToString();

    public GetSpellCheckDictionaryDisplay()
    {
        EnglishName = string.Empty;
        NativeName = string.Empty;
        Description = string.Empty;
        Files = new List<string>();
        StatusBrush = Brushes.Gray;
    }

    public override string ToString()
    {
        if (UseShortName || EnglishName == NativeName || string.IsNullOrEmpty(NativeName) || NativeName.Length > 30)
        {
            return EnglishName;
        }

        return $"{EnglishName} ({NativeName})";
    }
}

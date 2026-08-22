using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.FormatLimitWarning;

/// <summary>
/// Shown before a save when the target format has hard limits (SCC: 32 chars x 4 lines) that some
/// subtitles exceed - the writer would otherwise silently re-wrap or truncate them
/// ("my merged lines came back split" after a reopen).
/// </summary>
public partial class FormatLimitWarningViewModel : ObservableObject
{
    [ObservableProperty] private string _summaryText = string.Empty;
    [ObservableProperty] private string _maxCharactersText = string.Empty;
    [ObservableProperty] private bool _isMaxCharactersVisible;
    [ObservableProperty] private string _maxLinesText = string.Empty;
    [ObservableProperty] private bool _isMaxLinesVisible;
    [ObservableProperty] private string _linesText = string.Empty;
    [ObservableProperty] private bool _doNotShowAgain;

    public Window? Window { get; set; }
    public bool SaveAnywayPressed { get; private set; }

    private const int MaxListedLineNumbers = 30;

    public void Initialize(SubtitleFormat format, SubtitleFormatLimits limits, List<int> violatingParagraphNumbers)
    {
        SummaryText = string.Format(Se.Language.Main.FormatLimitWarningXLinesExceedLimitsOfY, violatingParagraphNumbers.Count, format.Name);

        IsMaxCharactersVisible = limits.MaxCharactersPerLine.HasValue;
        MaxCharactersText = string.Format(Se.Language.Main.FormatLimitWarningMaxXCharactersPerLine, limits.MaxCharactersPerLine ?? 0);

        IsMaxLinesVisible = limits.MaxLines.HasValue;
        MaxLinesText = string.Format(Se.Language.Main.FormatLimitWarningMaxXLines, limits.MaxLines ?? 0);

        var numbers = string.Join(", ", violatingParagraphNumbers.Take(MaxListedLineNumbers));
        if (violatingParagraphNumbers.Count > MaxListedLineNumbers)
        {
            numbers += ", ...";
        }

        LinesText = string.Format(Se.Language.Main.FormatLimitWarningLinesX, numbers);
    }

    [RelayCommand]
    private void SaveAnyway()
    {
        if (DoNotShowAgain)
        {
            Se.Settings.General.ShowFormatLimitWarning = false;
            Se.SaveSettings();
        }

        SaveAnywayPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }
}

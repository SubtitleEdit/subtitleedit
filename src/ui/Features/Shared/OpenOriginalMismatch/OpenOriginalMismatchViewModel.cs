using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.OpenOriginalMismatch;

/// <summary>
/// Asked when the original subtitle being opened does not line up 1:1 with the current subtitle:
/// how much of the original to show, and whether it may be edited (issue #13449).
/// </summary>
public partial class OpenOriginalMismatchViewModel : ObservableObject
{
    [ObservableProperty] private string _infoText;

    /// <summary>
    /// Show the original's non-matching lines as extra, display-only rows. The grid then holds the
    /// whole original, which is why editing it stays lossless - at the price of locked time codes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowMatchingLinesOnly))]
    [NotifyPropertyChangedFor(nameof(AllowEditHint))]
    private bool _showAllOriginalLines;

    public bool ShowMatchingLinesOnly
    {
        get => !ShowAllOriginalLines;
        set => ShowAllOriginalLines = !value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllowEditHint))]
    private bool _allowEditOfOriginal;

    [ObservableProperty] private string _showAllOriginalLinesText;
    [ObservableProperty] private string _showAllOriginalLinesHint;
    [ObservableProperty] private string _showAllOriginalLinesNote;
    [ObservableProperty] private string _showMatchingLinesOnlyText;
    [ObservableProperty] private string _showMatchingLinesOnlyHint;
    [ObservableProperty] private string _showMatchingLinesOnlyNote;

    private int _nonMatchingCount;

    /// <summary>
    /// What "allow edit" means for the mode that is currently selected - the consequence differs
    /// sharply between the two, and that is the whole point of the dialog.
    /// </summary>
    public string AllowEditHint
    {
        get
        {
            var language = Se.Language.Main;
            if (!AllowEditOfOriginal)
            {
                return language.AllowEditHintReadOnly;
            }

            return ShowAllOriginalLines
                ? language.AllowEditHintAllLines
                : string.Format(language.AllowEditHintMatchingOnlyX, _nonMatchingCount);
        }
    }

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public OpenOriginalMismatchViewModel()
    {
        _infoText = string.Empty;
        _showAllOriginalLinesText = string.Empty;
        _showAllOriginalLinesHint = string.Empty;
        _showAllOriginalLinesNote = string.Empty;
        _showMatchingLinesOnlyText = string.Empty;
        _showMatchingLinesOnlyHint = string.Empty;
        _showMatchingLinesOnlyNote = string.Empty;
        _showAllOriginalLines = true;
    }

    internal void Initialize(int originalLineCount, int currentLineCount, int matchingCount, int nonMatchingCount)
    {
        var language = Se.Language.Main;
        _nonMatchingCount = nonMatchingCount;

        InfoText = string.Format(language.OpenOriginalDifferentNumberOfSubtitlesXY, originalLineCount, currentLineCount);

        ShowAllOriginalLinesText = string.Format(language.ShowAllOriginalLinesX, nonMatchingCount);
        ShowAllOriginalLinesHint = language.ShowAllOriginalLinesHint;
        ShowAllOriginalLinesNote = language.ShowAllOriginalLinesNote;

        ShowMatchingLinesOnlyText = string.Format(language.ShowMatchingOriginalLinesX, matchingCount);
        ShowMatchingLinesOnlyHint = string.Format(language.ShowMatchingOriginalLinesHint, nonMatchingCount);
        ShowMatchingLinesOnlyNote = language.ShowMatchingOriginalLinesNote;

        ShowAllOriginalLines = true;
        AllowEditOfOriginal = Se.Settings.General.AllowEditOfOriginalSubtitle;
        OnPropertyChanged(nameof(AllowEditHint));
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
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
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }
}

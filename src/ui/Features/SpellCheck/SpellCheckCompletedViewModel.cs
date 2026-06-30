using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public partial class SpellCheckCompletedViewModel : ObservableObject
{
    [ObservableProperty] private string _changedWordsText;
    [ObservableProperty] private string _skippedWordsText;
    [ObservableProperty] private string _correctWordsText;
    [ObservableProperty] private string _namesText;
    [ObservableProperty] private string _addedWordsText;
    [ObservableProperty] private bool _isCorrectWordsVisible;
    [ObservableProperty] private bool _isNamesVisible;
    [ObservableProperty] private bool _isAddedWordsVisible;
    [ObservableProperty] private bool _doNotShowAgain;

    public Window? Window { get; set; }

    public SpellCheckCompletedViewModel()
    {
        _changedWordsText = string.Empty;
        _skippedWordsText = string.Empty;
        _correctWordsText = string.Empty;
        _namesText = string.Empty;
        _addedWordsText = string.Empty;
    }

    public void Initialize(int changedWords, int skippedWords, int correctWords, int names, int addedWords)
    {
        ChangedWordsText = string.Format(Se.Language.SpellCheck.ChangedWordsX, changedWords);
        SkippedWordsText = string.Format(Se.Language.SpellCheck.SkippedWordsX, skippedWords);

        // The optional rows only add value when non-zero, so keep the dialog tidy by hiding them at 0.
        CorrectWordsText = string.Format(Se.Language.SpellCheck.CorrectWordsX, correctWords);
        IsCorrectWordsVisible = correctWords > 0;

        NamesText = string.Format(Se.Language.SpellCheck.NamesX, names);
        IsNamesVisible = names > 0;

        AddedWordsText = string.Format(Se.Language.SpellCheck.AddedToDictionaryX, addedWords);
        IsAddedWordsVisible = addedWords > 0;
    }

    [RelayCommand]
    private void Ok()
    {
        if (DoNotShowAgain)
        {
            Se.Settings.SpellCheck.ShowCompletedMessage = false;
            Se.SaveSettings();
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }
}

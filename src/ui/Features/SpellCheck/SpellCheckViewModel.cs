using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck.EditWholeText;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public partial class SpellCheckViewModel : ObservableObject
{
    [ObservableProperty] private string _lineText;
    [ObservableProperty] private string _wholeText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeWordCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeWordAllCommand))]
    private string _currentWord;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeWordCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeWordAllCommand))]
    private string _wordNotFoundOriginal;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;
    [ObservableProperty] private bool _areSuggestionsAvailable;
    [ObservableProperty] private bool _isPrompting;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _paragraphs;
    [ObservableProperty] private SubtitleLineViewModel? _selectedParagraph;
    [ObservableProperty] private bool _isUndoVisible;
    [ObservableProperty] private string _undoText;

    public Window? Window { get; set; }
    public int TotalChangedWords { get; set; }
    public int TotalSkippedWords { get; set; }

    public bool OkPressed { get; private set; }
    public StackPanel PanelWholeText { get; internal set; }
    public TextBox TextBoxWordNotFound { get; internal set; }

    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IWindowService _windowService;
    private IFocusSubtitleLine? _focusSubtitleLine;

    private SpellCheckWord _currentSpellCheckWord;
    private SpellCheckResult? _lastSpellCheckResult;
    private System.Timers.Timer? _statusTimer;
    private readonly List<SpellCheckUndoItem> _undoList = new();

    public SpellCheckViewModel(ISpellCheckManager spellCheckManager, IWindowService windowService)
    {
        _spellCheckManager = spellCheckManager;
        if (Se.Settings.SpellCheck.SpellCheckProvider == SeSpellCheck.SpellCheckMsWord && WordSpellCheck.IsWordInstalled())
        {
            _spellCheckManager.WordSpellChecker = new WordSpellCheck();
            _spellCheckManager.WordSpellChecker.Initialize();
        }

        _spellCheckManager.OnWordChanged += (sender, e) =>
        {
            UpdateChangedWordInUi(e.FromWord, e.ToWord, e.WordIndex, e.Paragraph);
        };
        _windowService = windowService;

        LineText = string.Empty;
        WholeText = string.Empty;
        CurrentWord = string.Empty;
        WordNotFoundOriginal = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        SelectedDictionary = new SpellCheckDictionaryDisplay();
        Suggestions = new ObservableCollection<string>();
        SelectedSuggestion = string.Empty;
        PanelWholeText = new StackPanel();
        TextBoxWordNotFound = new TextBox();
        StatusText = string.Empty;
        UndoText = string.Empty;
        Paragraphs = new ObservableCollection<SubtitleLineViewModel>();
        _currentSpellCheckWord = new SpellCheckWord();

        LoadDictionaries();
    }

    private void UpdateChangedWordInUi(string fromWord, string toWord, int wordIndex, SubtitleLineViewModel paragraph)
    {
        WholeText = paragraph.Text;
        SelectedParagraph = Paragraphs.FirstOrDefault(p => p.Id == paragraph.Id);
        if (SelectedParagraph != null)
        {
            SelectedParagraph.Text = paragraph.Text;
        }
    }

    internal void OnDictionaryChanged()
    {
        if (SelectedDictionary == null)
        {
            return;
        }

        if (_spellCheckManager.WordSpellChecker != null)
        {
            if (Dictionaries.Count > 0)
            {
                var languages = _spellCheckManager.WordSpellChecker.GetInstalledLanguages();
                var selectedLanguage = languages.FirstOrDefault(l => l.Name == SelectedDictionary?.Name);
                if (selectedLanguage != null)
                {
                    _spellCheckManager.WordSpellChecker.CurrentLanguage = selectedLanguage;
                }
            }
        }
        else if (!string.IsNullOrEmpty(SelectedDictionary.DictionaryFileName))
        {
            // Hunspell provider: actually load the newly selected dictionary so switching
            // dialect (e.g. British -> American) mid-session takes effect immediately.
            // Previously this branch did nothing for Hunspell, so the spell checker kept
            // using the dictionary it started with until the next run (issue #11731).
            _spellCheckManager.Initialize(
                SelectedDictionary.DictionaryFileName,
                SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(SelectedDictionary));
        }

        ReCheckCurrentWordAfterDictionaryChange();
    }

    /// <summary>
    /// Re-evaluates the word currently shown after the dictionary is switched mid-session.
    /// If the word is now valid in the new dictionary the check advances to the next
    /// misspelling; otherwise the suggestion list is refreshed for the new dictionary.
    /// No-op until a check is underway and a flagged word is on screen.
    /// </summary>
    private void ReCheckCurrentWordAfterDictionaryChange()
    {
        if (_lastSpellCheckResult == null || SelectedParagraph == null || string.IsNullOrEmpty(WordNotFoundOriginal))
        {
            return;
        }

        if (_spellCheckManager.IsWordCorrect(_currentSpellCheckWord, SelectedParagraph.Text))
        {
            DoSpellCheck();
            return;
        }

        var suggestions = _spellCheckManager.GetSuggestions(WordNotFoundOriginal);
        Suggestions.Clear();
        foreach (var suggestion in suggestions)
        {
            Suggestions.Add(suggestion);
        }

        AreSuggestionsAvailable = true;
        SelectedSuggestion = suggestions.Count > 0 ? suggestions[0] : string.Empty;
    }

    private void LoadDictionaries()
    {
        if (_spellCheckManager.WordSpellChecker != null)
        {
            var languages = _spellCheckManager.WordSpellChecker.GetInstalledLanguages();
            Dictionaries.Clear();
            foreach (var language in languages)
            {
                Dictionaries.Add(new SpellCheckDictionaryDisplay
                {
                    Name = language.Name,
                });
            }

            if (Dictionaries.Count > 0)
            {
                if (!string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryName))
                {
                    SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name == Se.Settings.SpellCheck.LastLanguageDictionaryName);
                }

                if (SelectedDictionary == null)
                {
                    SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Contains("English", StringComparison.OrdinalIgnoreCase)) ?? Dictionaries[0];
                }

                _spellCheckManager.WordSpellChecker.CurrentLanguage = languages.FirstOrDefault(l => l.Name == SelectedDictionary.Name);
            }

            return;
        }

        var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        Dictionaries.Clear();
        Dictionaries.AddRange(spellCheckLanguages);
        if (Dictionaries.Count > 0)
        {
            if (!string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.SpellCheck.LastLanguageDictionaryFile);
            }

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries[0];
            }

            _spellCheckManager.Initialize(SelectedDictionary.DictionaryFileName, SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(SelectedDictionary));
        }
    }

    public void Initialize(ObservableCollection<SubtitleLineViewModel> paragraphs, int? selectedSubtitleIndex, IFocusSubtitleLine focusSubtitleLine, string? dictionaryFileName)
    {
        _focusSubtitleLine = focusSubtitleLine;
        Paragraphs.Clear();
        Paragraphs.AddRange(paragraphs);
        SetLanguage(dictionaryFileName);
        Dispatcher.UIThread.Post(DoSpellCheck, DispatcherPriority.Background);
    }

    private void SetLanguage(string? dictionaryFileName)
    {
        if (Dictionaries.Count <= 0)
        {
            return;
        }

        var subtitle = new Subtitle();
        foreach (var vm in Paragraphs)
        {
            var p = new Paragraph(vm.Text, 0, 0);
            subtitle.Paragraphs.Add(p);
        }

        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle);
        if (languageCode == null)
        {
            languageCode = "en";
        }

        if (_spellCheckManager.WordSpellChecker != null)
        {
            try
            {
                var languages = _spellCheckManager.WordSpellChecker.GetInstalledLanguages();
                var culture = new CultureInfo(languageCode);
                var text = culture.NativeName; // "Português (Portugal)"
                if (text.Contains('(') && text.Contains(')'))
                {
                    var start = text.IndexOf('(');
                    var end = text.IndexOf(')');
                    var languageName = text.Substring(start + 1, end - start - 1);
                    if (!string.IsNullOrWhiteSpace(Se.Settings.SpellCheck.LastLanguageDictionaryName) && Se.Settings.SpellCheck.LastLanguageDictionaryName.Contains(languageName, StringComparison.OrdinalIgnoreCase))
                    {
                        var selectedLan = languages.FirstOrDefault(l => l.Name.Contains(Se.Settings.SpellCheck.LastLanguageDictionaryName, StringComparison.OrdinalIgnoreCase));
                        if (selectedLan != null)
                        {
                            _spellCheckManager.WordSpellChecker.CurrentLanguage = selectedLan;
                            SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Equals(selectedLan.Name, StringComparison.OrdinalIgnoreCase));
                            return;
                        }
                    }

                    var selectedLanguage = languages.FirstOrDefault(l => l.Name.Contains(languageName, StringComparison.OrdinalIgnoreCase));
                    if (selectedLanguage != null)
                    {
                        _spellCheckManager.WordSpellChecker.CurrentLanguage = selectedLanguage;
                        SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Equals(selectedLanguage.Name, StringComparison.OrdinalIgnoreCase));
                        return;
                    }
                }
            }
            catch
            {
                // ignore
            }
            return;
        }

        var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(languageCode);
        SelectedDictionary = Dictionaries.FirstOrDefault(p => p.GetThreeLetterCode().Equals(threeLetterCode, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(dictionaryFileName))
        {
            SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName.Equals(dictionaryFileName, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedDictionary == null)
        {
            SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedDictionary == null)
        {
            if (!string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.SpellCheck.LastLanguageDictionaryFile);
            }
        }

        if (SelectedDictionary == null && Dictionaries.Count > 0)
        {
            SelectedDictionary = Dictionaries[0];
        }

        if (SelectedDictionary != null)
        {
            _spellCheckManager.Initialize(SelectedDictionary.DictionaryFileName, SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(SelectedDictionary));
        }
    }

    [RelayCommand]
    private async Task EditWholeText()
    {
        var selectedParagraph = SelectedParagraph;
        if (selectedParagraph == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditWholeTextWindow, EditWholeTextViewModel>(Window!, vm =>
        {
            vm.WholeText = selectedParagraph.Text;
            vm.LineInfo = string.Format(Se.Language.SpellCheck.LineXofY, Paragraphs.IndexOf(selectedParagraph) + 1, Paragraphs.Count);
        });

        if (!result.OkPressed)
        {
            return;
        }

        PushUndo(Se.Language.SpellCheck.EditWholeText, SpellCheckUndoAction.ChangeWholeText, string.Empty);
        selectedParagraph.Text = result.WholeText;
        DoSpellCheck();
    }

    private bool CanChangeWord() => !string.IsNullOrWhiteSpace(CurrentWord) && WordNotFoundOriginal != CurrentWord;

    [RelayCommand(CanExecute = nameof(CanChangeWord))]
    private void ChangeWord()
    {
        if (SelectedParagraph == null)
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxWordNotFound.Focus(); });
            return;
        }

        var status = string.Format(Se.Language.SpellCheck.ChangeWordFromXToY, WordNotFoundOriginal, CurrentWord);
        PushUndo(status, SpellCheckUndoAction.Change, WordNotFoundOriginal);
        _spellCheckManager.ChangeWord(WordNotFoundOriginal, CurrentWord, _currentSpellCheckWord, SelectedParagraph!);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand(CanExecute = nameof(CanChangeWord))]
    private void ChangeWordAll()
    {
        var selectedParagraph = SelectedParagraph;
        if (selectedParagraph == null)
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxWordNotFound.Focus(); });
            return;
        }

        var status = string.Format(Se.Language.SpellCheck.ChangeAllWordsFromXToY, WordNotFoundOriginal, CurrentWord);
        PushUndo(status, SpellCheckUndoAction.ChangeAll, WordNotFoundOriginal);
        _spellCheckManager.ChangeAllWord(WordNotFoundOriginal, CurrentWord, _currentSpellCheckWord, selectedParagraph);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private void SkipWord()
    {
        var status = string.Format(Se.Language.SpellCheck.IgnoreWordXOnce, WordNotFoundOriginal);
        PushUndo(status, SpellCheckUndoAction.SkipOnce, WordNotFoundOriginal);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private void SkipWordAll()
    {
        var status = string.Format(Se.Language.SpellCheck.IgnoreWordXAlways, WordNotFoundOriginal);
        PushUndo(status, SpellCheckUndoAction.SkipAll, WordNotFoundOriginal);
        _spellCheckManager.AddIgnoreWord(WordNotFoundOriginal);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private void AddToNamesList()
    {
        if (string.IsNullOrWhiteSpace(CurrentWord))
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxWordNotFound.Focus(); });
            return;
        }

        var status = string.Format(Se.Language.SpellCheck.WordXAddedToNamesList, CurrentWord);
        PushUndo(status, SpellCheckUndoAction.AddToNames, CurrentWord);
        _spellCheckManager.AddToNames(CurrentWord);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private void AddToUserDictionary()
    {
        if (string.IsNullOrWhiteSpace(CurrentWord))
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxWordNotFound.Focus(); });
            return;
        }

        var status = string.Format(Se.Language.SpellCheck.WordXAddedToUserDictionary, CurrentWord);
        PushUndo(status, SpellCheckUndoAction.AddToDictionary, CurrentWord);
        _spellCheckManager.AdToUserDictionary(CurrentWord);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private async Task GoogleIt()
    {
        if (string.IsNullOrEmpty(CurrentWord))
        {
            return;
        }

        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.google.com/search?q=" + HttpUtility.UrlEncode(CurrentWord)));
    }

    [RelayCommand]
    private async Task BrowseDictionary()
    {
        var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!, vm => { });
        if (result.OkPressed)
        {
            LoadDictionaries();
            SetLanguage(result.DictionaryFileName);
        }
    }

    [RelayCommand]
    private void SuggestionUseOnce()
    {
        if (SelectedSuggestion == null || SelectedParagraph == null)
        {
            return;
        }

        var status = string.Format(Se.Language.SpellCheck.UseSuggestionX, SelectedSuggestion);
        PushUndo(status, SpellCheckUndoAction.Change, WordNotFoundOriginal);
        _spellCheckManager.ChangeWord(WordNotFoundOriginal, SelectedSuggestion, _currentSpellCheckWord, SelectedParagraph);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private void SuggestionUseAlways()
    {
        if (SelectedSuggestion == null || SelectedParagraph == null)
        {
            return;
        }

        var status = string.Format(Se.Language.SpellCheck.UseSuggestionXAlways, SelectedSuggestion);
        PushUndo(status, SpellCheckUndoAction.ChangeAll, WordNotFoundOriginal);
        _spellCheckManager.ChangeAllWord(WordNotFoundOriginal, SelectedSuggestion, _currentSpellCheckWord, SelectedParagraph);
        ShowStatus(status);
        DoSpellCheck();
    }

    [RelayCommand]
    private void Ok()
    {
        TotalChangedWords = _spellCheckManager.NoOfChangedWords;
        TotalSkippedWords = _spellCheckManager.NoOfSkippedWords;
        OkPressed = true;
        Se.Settings.SpellCheck.LastLanguageDictionaryName = SelectedDictionary?.Name;
        Se.Settings.SpellCheck.LastLanguageDictionaryFile = SelectedDictionary?.DictionaryFileName;
        Dispatcher.UIThread.Invoke(() => { Window?.Close(); });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/spell-check");
        }
    }

    private bool CanUndo() => _undoList.Count > 0;

    private void PushUndo(string description, SpellCheckUndoAction action, string actionWord)
    {
        _undoList.Add(new SpellCheckUndoItem
        {
            Description = description,
            Action = action,
            ActionWord = actionWord,
            NoOfChangedWords = _spellCheckManager.NoOfChangedWords,
            NoOfSkippedWords = _spellCheckManager.NoOfSkippedWords,
            ParagraphTexts = Paragraphs.Select(p => (p, p.Text)).ToList(),

            // Re-scan from just before the acted-on word so it is shown again after undo.
            ResumeFrom = _lastSpellCheckResult == null
                ? null
                : new SpellCheckResult
                {
                    LineIndex = _lastSpellCheckResult.LineIndex,
                    WordIndex = _lastSpellCheckResult.WordIndex - 1,
                },
        });

        UndoText = string.Format(Se.Language.SpellCheck.UndoX, description);
        IsUndoVisible = true;
        UndoCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoList.Count == 0)
        {
            return;
        }

        var item = _undoList[^1];
        _undoList.RemoveAt(_undoList.Count - 1);

        // Restore the text of every line as it was before the action (covers Change, ChangeAll and
        // its silent cascade, and Edit-whole-text).
        foreach (var (paragraph, text) in item.ParagraphTexts)
        {
            paragraph.Text = text;
        }

        // Reverse word-list / change-all dictionary mutations.
        switch (item.Action)
        {
            case SpellCheckUndoAction.SkipAll:
                _spellCheckManager.RemoveIgnoreWord(item.ActionWord);
                break;
            case SpellCheckUndoAction.ChangeAll:
                _spellCheckManager.RemoveChangeAllWord(item.ActionWord);
                break;
            case SpellCheckUndoAction.AddToNames:
                _spellCheckManager.RemoveFromNames(item.ActionWord);
                break;
            case SpellCheckUndoAction.AddToDictionary:
                _spellCheckManager.RemoveFromUserDictionary(item.ActionWord);
                break;
        }

        // Restore counters and the scan position, then re-check so the word reappears.
        _spellCheckManager.NoOfChangedWords = item.NoOfChangedWords;
        _spellCheckManager.NoOfSkippedWords = item.NoOfSkippedWords;
        _lastSpellCheckResult = item.ResumeFrom;

        if (_undoList.Count > 0)
        {
            UndoText = string.Format(Se.Language.SpellCheck.UndoX, _undoList[^1].Description);
        }
        else
        {
            IsUndoVisible = false;
            UndoText = string.Empty;
        }

        UndoCommand.NotifyCanExecuteChanged();
        DoSpellCheck();
    }

    private void DoSpellCheck()
    {
        if (Dictionaries.Count == 0)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!, vm => { });
                if (result.OkPressed)
                {
                    LoadDictionaries();
                    SetLanguage(result.DictionaryFileName);
                    DoSpellCheck();
                }
            }, DispatcherPriority.Background);

            return;
        }

        var results = _spellCheckManager.CheckSpelling(Paragraphs, _lastSpellCheckResult);
        if (results.Count > 0)
        {
            WordNotFoundOriginal = results[0].Word.Text;
            CurrentWord = results[0].Word.Text;
            WholeText = results[0].Paragraph.Text;
            HighLightCurrentWord(results[0].Word, results[0].Paragraph);
            _currentSpellCheckWord = results[0].Word;
            _lastSpellCheckResult = results[0];
            SelectedParagraph = results[0].Paragraph;

            var suggestions = _spellCheckManager.GetSuggestions(results[0].Word.Text);
            Suggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                Suggestions.Add(suggestion);
            }

            AreSuggestionsAvailable = true;
            if (suggestions.Count > 0)
            {
                SelectedSuggestion = suggestions[0];
            }

            var lineIndex = Paragraphs.IndexOf(results[0].Paragraph) + 1;
            LineText = string.Format(Se.Language.SpellCheck.LineXofY, lineIndex, Paragraphs.Count);

            _focusSubtitleLine?.GoToAndFocusLine(SelectedParagraph);
        }
        else
        {
            Ok();
        }
    }

    private void HighLightCurrentWord(SpellCheckWord word, SubtitleLineViewModel paragraph)
    {
        var textBlock = new TextBlock();
        var fontName = Se.Settings.Appearance.SubtitleTextBoxAndGridFontName;
        if (!string.IsNullOrEmpty(fontName))
        {
            textBlock.FontFamily = new FontFamily(fontName);
        }
        var idx = word.Index;
        if (idx > 0)
        {
            var run = new Run(paragraph.Text.Substring(0, idx));
            if (!string.IsNullOrEmpty(fontName))
            {
                run.FontFamily = new FontFamily(fontName);
            }
            textBlock.Inlines!.Add(run);
        }

        var highlightRun = new Run
        {
            Text = word.Text,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Red
        };
        if (!string.IsNullOrEmpty(fontName))
        {
            highlightRun.FontFamily = new FontFamily(fontName);
        }
        textBlock.Inlines!.Add(highlightRun);

        if (idx + word.Text.Length < paragraph.Text.Length)
        {
            var run = new Run(paragraph.Text.Substring(idx + word.Text.Length));
            if (!string.IsNullOrEmpty(fontName))
            {
                run.FontFamily = new FontFamily(fontName);
            }
            textBlock.Inlines!.Add(run);
        }

        PanelWholeText.Children.Clear();
        PanelWholeText.Children.Add(textBlock);
    }

    private void ShowStatus(string statusText)
    {
        StatusText = statusText;

        _statusTimer?.Stop();
        _statusTimer?.Dispose();

        _statusTimer = new System.Timers.Timer(3000);
        _statusTimer.Elapsed += (sender, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    StatusText = string.Empty;
                    _statusTimer?.Dispose();
                    _statusTimer = null;
                }
                catch
                {
                    // ignore
                }
            });
        };
        _statusTimer.AutoReset = false;
        _statusTimer.Start();
    }

    internal void ListBoxSuggestionsDoubleTapped(object? sender, TappedEventArgs e)
    {
        SuggestionUseOnce();
    }
}
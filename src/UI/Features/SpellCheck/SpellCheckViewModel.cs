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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public partial class SpellCheckViewModel : ObservableObject
{
    [ObservableProperty] private string _lineText;
    [ObservableProperty] private string _wholeText;
    [ObservableProperty] private string _currentWord;
    [ObservableProperty] private string _wordNotFoundOriginal;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;
    [ObservableProperty] private bool _areSuggestionsAvailable;
    [ObservableProperty] private bool _isPrompting;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _paragraphs;
    [ObservableProperty] private SubtitleLineViewModel? _selectedParagraph;

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

    public SpellCheckViewModel(ISpellCheckManager spellCheckManager, IWindowService windowService)
    {
        _spellCheckManager = spellCheckManager;
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

    private void LoadDictionaries()
    {
        var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        Dictionaries.Clear();
        Dictionaries.AddRange(spellCheckLanguages);
        if (Dictionaries.Count > 0)
        {
            if (!string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.SpellCheck.LastLanguageDictionaryFile);
            }

            SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
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

        selectedParagraph.Text = result.WholeText;
        DoSpellCheck();
    }

    [RelayCommand]
    private void ChangeWord()
    {
        if (string.IsNullOrWhiteSpace(CurrentWord) || WordNotFoundOriginal == CurrentWord)
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxWordNotFound.Focus(); });
            return;
        }

        _spellCheckManager.ChangeWord(WordNotFoundOriginal, CurrentWord, _currentSpellCheckWord, SelectedParagraph!);
        ShowStatus(string.Format(Se.Language.SpellCheck.ChangeWordFromXToY, WordNotFoundOriginal, CurrentWord));
        DoSpellCheck();
    }

    [RelayCommand]
    private void ChangeWordAll()
    {
        var selectedParagraph = SelectedParagraph;
        if (string.IsNullOrWhiteSpace(CurrentWord) || WordNotFoundOriginal == CurrentWord || selectedParagraph == null)
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxWordNotFound.Focus(); });
            return;
        }

        _spellCheckManager.ChangeAllWord(WordNotFoundOriginal, CurrentWord, _currentSpellCheckWord, selectedParagraph);
        ShowStatus(string.Format(Se.Language.SpellCheck.ChangeAllWordsFromXToY, WordNotFoundOriginal, CurrentWord));
        DoSpellCheck();
    }

    [RelayCommand]
    private void SkipWord()
    {
        ShowStatus(string.Format(Se.Language.SpellCheck.IgnoreWordXOnce, WordNotFoundOriginal));
        DoSpellCheck();
    }

    [RelayCommand]
    private void SkipWordAll()
    {
        _spellCheckManager.AddIgnoreWord(WordNotFoundOriginal);
        ShowStatus(string.Format(Se.Language.SpellCheck.IgnoreWordXAlways, WordNotFoundOriginal));
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

        _spellCheckManager.AddToNames(CurrentWord);
        ShowStatus(string.Format(Se.Language.SpellCheck.WordXAddedToNamesList, CurrentWord));
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

        _spellCheckManager.AdToUserDictionary(CurrentWord);
        ShowStatus(string.Format(Se.Language.SpellCheck.WordXAddedToUserDictionary, CurrentWord));
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

        _spellCheckManager.ChangeWord(WordNotFoundOriginal, SelectedSuggestion, _currentSpellCheckWord, SelectedParagraph);
        ShowStatus(string.Format(Se.Language.SpellCheck.UseSuggestionX, SelectedSuggestion));
        DoSpellCheck();
    }

    [RelayCommand]
    private void SuggestionUseAlways()
    {
        if (SelectedSuggestion == null || SelectedParagraph == null)
        {
            return;
        }

        _spellCheckManager.ChangeAllWord(WordNotFoundOriginal, SelectedSuggestion, _currentSpellCheckWord, SelectedParagraph);
        ShowStatus(string.Format(Se.Language.SpellCheck.UseSuggestionXAlways, SelectedSuggestion));
        DoSpellCheck();
    }

    [RelayCommand]
    private void Ok()
    {
        TotalChangedWords = _spellCheckManager.NoOfChangedWords;
        TotalSkippedWords = _spellCheckManager.NoOfSkippedWords;
        OkPressed = true;
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
            Suggestions = new ObservableCollection<string>(suggestions);
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
        var idx = word.Index;
        if (idx > 0)
        {
            var text = paragraph.Text.Substring(0, idx);
            textBlock.Inlines!.Add(new Run(text));
        }

        textBlock.Inlines!.Add(new Run
        {
            Text = word.Text,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Red
        });

        if (idx + word.Text.Length < paragraph.Text.Length)
        {
            var text = paragraph.Text.Substring(idx + word.Text.Length);
            textBlock.Inlines!.Add(new Run(text));
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
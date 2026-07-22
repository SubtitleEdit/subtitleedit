using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.SpellCheck.EditWholeText;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

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
    [ObservableProperty] private Bitmap? _sourceImage;
    [ObservableProperty] private bool _hasSourceImage;
    [ObservableProperty] private bool _isUndoVisible;
    [ObservableProperty] private string _undoText;

    public Window? Window { get; set; }
    public int TotalChangedWords { get; set; }
    public int TotalSkippedWords { get; set; }
    public int TotalCorrectWords { get; set; }
    public int TotalNames { get; set; }
    public int TotalAddedWords { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>
    /// True when the scan reached the end of the subtitle, so there is no unfinished session left.
    /// </summary>
    public bool ScanCompleted { get; private set; }
    public StackPanel PanelWholeText { get; internal set; }
    public TextBox TextBoxWordNotFound { get; internal set; }

    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IBluRayHelper _bluRayHelper;
    private readonly IOcrImageSourceHolder _ocrImageSourceHolder;
    private IFocusSubtitleLine? _focusSubtitleLine;

    // Optional source image (Blu-ray .sup) loaded via the context menu so the original
    // bitmap of the current line can be compared while spell-checking OCR results (#11719).
    private IOcrSubtitle? _ocrSourceImages;

    private SpellCheckWord _currentSpellCheckWord;
    private SpellCheckResult? _lastSpellCheckResult;
    private System.Timers.Timer? _statusTimer;
    private readonly List<SpellCheckUndoItem> _undoList = new();

    // Where this run started scanning (0 = top). When the user chooses to start at the
    // current line, the scan runs to the end and then offers to wrap back to the top;
    // _stopBeforeLineIndex bounds that wrapped pass to the lines above the start so it
    // does not re-check the part already covered. _hasWrapped guards against re-prompting.
    private int _scanStartLineIndex;
    private int? _stopBeforeLineIndex;
    private bool _hasWrapped;

    // True when a spell check of this subtitle was left unfinished, which is the only case where
    // "continue spell check from current line?" has something to continue from.
    private bool _sessionInProgress;

    public SpellCheckViewModel(ISpellCheckManager spellCheckManager, IWindowService windowService, IFileHelper fileHelper, IBluRayHelper bluRayHelper, IOcrImageSourceHolder ocrImageSourceHolder)
    {
        _spellCheckManager = spellCheckManager;
        _fileHelper = fileHelper;
        _bluRayHelper = bluRayHelper;
        _ocrImageSourceHolder = ocrImageSourceHolder;
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
        Dictionaries.AddRange(LanguageFavoritesHelper.Order(spellCheckLanguages, d => SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(d)));
        if (Dictionaries.Count > 0)
        {
            SelectedDictionary = FindLastUsedDictionary(Dictionaries);

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

    // Lets the user attach the original Blu-ray .sup so the source image of the current
    // line is shown while spell-checking OCR'd text (SE4 parity, #11719).
    [RelayCommand]
    private async Task LoadSourceImage()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(
            Window, Se.Language.SpellCheck.LoadSourceImage,
            "Image based subtitles", new List<string> { "*.sup", "*.sub", "*.idx", "*.xml", "*.ts", "*.m2ts", "*.mts", "*.mkv", "*.mks" },
            string.Empty, new List<string>());
        var fileName = fileNames.FirstOrDefault();
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            var source = LoadImageSource(fileName);
            if (source == null || source.Count == 0)
            {
                return;
            }

            _ocrSourceImages = source;
            HasSourceImage = true;
            UpdateSourceImage();
        }
        catch
        {
            // Bad/unsupported file: leave the panel as-is.
        }
    }

    // Loads an image-based subtitle file into an IOcrSubtitle for the source-image preview.
    private IOcrSubtitle? LoadImageSource(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        switch (ext)
        {
            case ".sup":
                var pcsList = BluRaySupParser.ParseBluRaySup(fileName, new System.Text.StringBuilder());
                return pcsList.Count > 0 ? new OcrSubtitleBluRay(pcsList) : null;

            case ".sub":
            case ".idx":
                var vobSubParser = new VobSubParser(true);
                var subFileName = System.IO.Path.ChangeExtension(fileName, ".sub");
                var idxFileName = System.IO.Path.ChangeExtension(fileName, ".idx");
                vobSubParser.OpenSubIdx(subFileName, idxFileName);
                var packs = vobSubParser.MergeVobSubPacks();
                return packs.Count > 0 ? new OcrSubtitleVobSub(packs, vobSubParser.IdxPalette) : null;

            case ".xml":
                var bdnLines = System.IO.File.ReadAllLines(fileName).ToList();
                var bdn = new BdnXml();
                if (!bdn.IsMine(bdnLines, fileName))
                {
                    return null;
                }

                var bdnSubtitle = new Subtitle();
                bdn.LoadSubtitle(bdnSubtitle, bdnLines, fileName);
                return bdnSubtitle.Paragraphs.Count > 0
                    ? new OcrSubtitleBdn(bdnSubtitle, fileName, isSon: false)
                    : null;

            case ".ts":
            case ".m2ts":
            case ".mts":
                var tsParser = new TransportStreamParser();
                tsParser.Parse(fileName, (_, _) => { });
                if (tsParser.SubtitlePacketIds.Count == 0)
                {
                    return null;
                }

                var subtitles = tsParser.GetDvbSubtitles(tsParser.SubtitlePacketIds[0]);
                return subtitles.Count > 0 ? new OcrSubtitleTransportStream(tsParser, subtitles, fileName) : null;

            case ".mkv":
            case ".mks":
                using (var matroska = new MatroskaFile(fileName))
                {
                    if (!matroska.IsValid)
                    {
                        return null;
                    }

                    // Only Blu-ray (PGS) tracks are handled for manual load here; VobSub/DVB
                    // tracks need the async Matroska extraction in the OCR window, and those
                    // are covered automatically via the auto-attach path after OCR (#11719).
                    var track = matroska.GetTracks(true)
                        .FirstOrDefault(t => t.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase));
                    if (track == null)
                    {
                        return null;
                    }

                    var pcsData = _bluRayHelper.LoadBluRaySubFromMatroska(track, matroska, out _);
                    return pcsData.Count > 0 ? new OcrSubtitleMkvBluRay(track, pcsData) : null;
                }

            default:
                return null;
        }
    }

    partial void OnSelectedParagraphChanged(SubtitleLineViewModel? value)
    {
        UpdateSourceImage();
    }

    // Shows the source bitmap whose start time is closest to the current line (SE4 matched
    // by timecode, which survives merges/deletes better than a plain index).
    private void UpdateSourceImage()
    {
        // Dispose the previously shown bitmap before replacing it; UpdateSourceImage runs on every
        // line change during a spell-check pass, and both the Avalonia Bitmap and the SKBitmap hold
        // unmanaged memory, so without this a long pass leaks one bitmap per line (#11719).
        if (_ocrSourceImages == null || SelectedParagraph == null || _ocrSourceImages.Count == 0)
        {
            var previousEmpty = SourceImage;
            SourceImage = null;
            previousEmpty?.Dispose();
            return;
        }

        var targetMs = SelectedParagraph.StartTime.TotalMilliseconds;
        var bestIndex = 0;
        var bestDistance = double.MaxValue;
        for (var i = 0; i < _ocrSourceImages.Count; i++)
        {
            var distance = Math.Abs(_ocrSourceImages.GetStartTime(i).TotalMilliseconds - targetMs);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        var previous = SourceImage;
        try
        {
            using var sourceBitmap = _ocrSourceImages.GetBitmap(bestIndex);
            SourceImage = sourceBitmap.ToAvaloniaBitmap();
        }
        catch
        {
            SourceImage = null;
        }

        previous?.Dispose();
    }

    public void Initialize(ObservableCollection<SubtitleLineViewModel> paragraphs, int? selectedSubtitleIndex,
        IFocusSubtitleLine focusSubtitleLine, SpellCheckDictionaryDisplay? spellCheckDictionary, bool sessionInProgress = false)
    {
        _focusSubtitleLine = focusSubtitleLine;
        _sessionInProgress = sessionInProgress;
        Paragraphs.Clear();
        Paragraphs.AddRange(paragraphs);
        SetLanguage(spellCheckDictionary);

        // Auto-attach the image source from the most recent OCR session so the original
        // bitmaps show up automatically when spell-checking OCR'd text - no manual load
        // needed. Works for every format OCR can read (sup, VobSub, BDN, TS, MKV, ...) (#11719).
        if (_ocrImageSourceHolder.Source is { Count: > 0 } ocrSource)
        {
            _ocrSourceImages = ocrSource;
            HasSourceImage = true;
        }

        // Posted to run after the window exists (Window is assigned in the window ctor),
        // so the "continue from current line?" prompt and the close hook have an owner.
        Dispatcher.UIThread.Post(
            async void () =>
            {
                try
                {
                    await StartSpellCheckAsync(selectedSubtitleIndex);
                }
                catch (Exception exception)
                {
                    // Without this the task was discarded and any failure in here was lost,
                    // leaving the window up with no word, no text and no suggestions.
                    Se.LogError(exception, "Spell check could not be started");
                }
            },
            DispatcherPriority.Background);
    }

    /// <summary>
    /// Decides where the scan begins. When a line other than the first is selected, the user
    /// is asked whether to continue from that line or start from the top (SE4 parity); Cancel
    /// closes the dialog without checking. The chosen start line is remembered so the scan can
    /// later offer to wrap back to the top.
    /// </summary>
    private async Task StartSpellCheckAsync(int? selectedSubtitleIndex)
    {
        // The view model is initialized before the window service shows the dialog, and the
        // Background pass it yields for (YieldForPendingFlyoutDismissAsync) runs this posted
        // callback before window.ShowDialog(). The window therefore exists but is not on screen
        // yet, and a message box owned by a window that is not visible throws - which used to
        // leave the spell check window blank whenever the prompt below was needed, i.e. for
        // every line except the first one.
        await WaitForWindowShownAsync();

        // Make sure a summary of changes is reported even when the user closes early (Esc / X),
        // not only when the run completes or "Done" is pressed.
        if (Window != null)
        {
            Window.Closing += (_, _) => CaptureTotals();
        }

        var startLine = 0;
        if (selectedSubtitleIndex is int idx && idx > 0 && idx < Paragraphs.Count)
        {
            if (!_sessionInProgress)
            {
                // Nothing to continue from, so start where the user is standing instead of asking.
                // The scan offers to wrap back to the top when it reaches the end, so the lines above
                // are still covered.
                startLine = idx;
            }
            else if (Window != null)
            {
                // A spell check of this subtitle was left unfinished, so "continue from current line"
                // is a real choice: resume where the user is now, or start over from the top.
                var answer = await MessageBox.Show(
                    Window,
                    Se.Language.SpellCheck.SpellCheck,
                    Se.Language.SpellCheck.ContinueFromCurrentLine,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer == MessageBoxResult.Cancel || answer == MessageBoxResult.None)
                {
                    Dispatcher.UIThread.Invoke(() => Window?.Close());
                    return;
                }

                if (answer == MessageBoxResult.Yes)
                {
                    startLine = idx;
                }
            }
        }

        _scanStartLineIndex = startLine;
        _lastSpellCheckResult = startLine > 0
            ? new SpellCheckResult { LineIndex = startLine, WordIndex = -1 }
            : null;

        DoSpellCheck();
    }

    /// <summary>
    /// Completes once the window is on screen, so message boxes owned by it can be shown.
    /// </summary>
    private Task WaitForWindowShownAsync()
    {
        var window = Window;
        if (window == null || window.IsVisible)
        {
            return Task.CompletedTask;
        }

        var taskCompletionSource = new TaskCompletionSource();

        void OnOpened(object? sender, EventArgs e)
        {
            window.Opened -= OnOpened;
            taskCompletionSource.TrySetResult();
        }

        window.Opened += OnOpened;

        // The window may have been shown between the check above and the subscription
        if (window.IsVisible)
        {
            window.Opened -= OnOpened;
            taskCompletionSource.TrySetResult();
        }

        return taskCompletionSource.Task;
    }

    private void CaptureTotals()
    {
        TotalChangedWords = _spellCheckManager.NoOfChangedWords;
        TotalSkippedWords = _spellCheckManager.NoOfSkippedWords;
        TotalCorrectWords = _spellCheckManager.NoOfCorrectWords;
        TotalNames = _spellCheckManager.NoOfNames;
        TotalAddedWords = _spellCheckManager.NoOfAddedWords;
    }

    /// <summary>
    /// Resolves the last-used dictionary from settings: by dictionary file when one was stored
    /// (locale independent — hunspell display names come from CultureInfo.DisplayName and change
    /// with the OS language), falling back to the stored display name (MS Word provider entries
    /// have no file).
    /// </summary>
    private static SpellCheckDictionaryDisplay? FindLastUsedDictionary(ObservableCollection<SpellCheckDictionaryDisplay> dictionaries)
    {
        var file = Se.Settings.SpellCheck.LastLanguageDictionaryFile;
        var byFile = string.IsNullOrEmpty(file)
            ? null
            : dictionaries.FirstOrDefault(l => l.DictionaryFileName.Equals(file, StringComparison.OrdinalIgnoreCase));
        if (byFile != null)
        {
            return byFile;
        }

        var name = Se.Settings.SpellCheck.LastLanguageDictionaryName;
        return string.IsNullOrEmpty(name)
            ? null
            : dictionaries.FirstOrDefault(l => l.Name == name);
    }

    private void SetLanguage(SpellCheckDictionaryDisplay? spellCheckDictionary)
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

        if (spellCheckDictionary is not null)
        {
            // Hunspell entries are identified by dictionary file; MS Word entries have no file
            // and are identified by name only. When the passed dictionary matches nothing (file
            // deleted since last session, name stored under another OS locale), keep the
            // auto-detected dictionary instead of clearing it.
            SelectedDictionary = (!string.IsNullOrEmpty(spellCheckDictionary.DictionaryFileName)
                    ? Dictionaries.FirstOrDefault(l => l.DictionaryFileName.Equals(spellCheckDictionary.DictionaryFileName, StringComparison.OrdinalIgnoreCase))
                    : null)
                ?? Dictionaries.FirstOrDefault(l => l.Name.Equals(spellCheckDictionary.Name, StringComparison.OrdinalIgnoreCase))
                ?? SelectedDictionary;
        }

        if (SelectedDictionary == null)
        {
            // Match on the file name ("de_DE.dic" starts with "de") — the display name is
            // localized ("German"/"Deutsch") and does not reliably start with the ISO code.
            SelectedDictionary = Dictionaries.FirstOrDefault(l =>
                Path.GetFileName(l.DictionaryFileName).StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedDictionary == null)
        {
            SelectedDictionary = FindLastUsedDictionary(Dictionaries);
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
        _spellCheckManager.NoOfNames++;
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
        _spellCheckManager.NoOfAddedWords++;
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
            SetLanguage(result.SpellCheckDictionary);
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
        TotalCorrectWords = _spellCheckManager.NoOfCorrectWords;
        TotalNames = _spellCheckManager.NoOfNames;
        TotalAddedWords = _spellCheckManager.NoOfAddedWords;
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
            NoOfCorrectWords = _spellCheckManager.NoOfCorrectWords,
            NoOfNames = _spellCheckManager.NoOfNames,
            NoOfAddedWords = _spellCheckManager.NoOfAddedWords,
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
        _spellCheckManager.NoOfCorrectWords = item.NoOfCorrectWords;
        _spellCheckManager.NoOfNames = item.NoOfNames;
        _spellCheckManager.NoOfAddedWords = item.NoOfAddedWords;
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
                    SetLanguage(result.SpellCheckDictionary);
                    DoSpellCheck();
                }
            }, DispatcherPriority.Background);

            return;
        }

        var results = _spellCheckManager.CheckSpelling(Paragraphs, _lastSpellCheckResult, _stopBeforeLineIndex);
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
        else if (_scanStartLineIndex > 0 && !_hasWrapped)
        {
            // Reached the end after starting mid-list: offer to wrap back and check the top part.
            Dispatcher.UIThread.Post(async () =>
            {
                var answer = Window == null
                    ? MessageBoxResult.No
                    : await MessageBox.Show(
                        Window,
                        Se.Language.SpellCheck.SpellCheck,
                        Se.Language.SpellCheck.ContinueFromTop,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    _hasWrapped = true;
                    _stopBeforeLineIndex = _scanStartLineIndex;
                    _lastSpellCheckResult = null;
                    DoSpellCheck();
                }
                else
                {
                    // Declining to wrap back ends the run: the user chose to leave the lines above
                    // the start unchecked, so nothing is left hanging for a later "continue".
                    ScanCompleted = true;
                    Ok();
                }
            }, DispatcherPriority.Background);
        }
        else
        {
            ScanCompleted = true;
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
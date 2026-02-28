using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Core.VobSub.Ocr.Service;
using Nikse.SubtitleEdit.Features.Files.ImportImages;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.AddToNamesList;
using Nikse.SubtitleEdit.Features.Shared.AddToOcrReplaceList;
using Nikse.SubtitleEdit.Features.Shared.AddToUserDictionary;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Shared.PickFontName;
using Nikse.SubtitleEdit.Features.Shared.ShowImage;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static Nikse.SubtitleEdit.Logic.Ocr.BinaryOcrMatcher;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class OcrViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<OcrEngineItem> _ocrEngines;
    [ObservableProperty] private OcrEngineItem? _selectedOcrEngine;
    [ObservableProperty] private ObservableCollection<OcrSubtitleItem> _ocrSubtitleItems;
    [ObservableProperty] private OcrSubtitleItem? _selectedOcrSubtitleItem;
    [ObservableProperty] private ObservableCollection<string> _nOcrDatabases;
    [ObservableProperty] private string? _selectedNOcrDatabase;
    [ObservableProperty] private ObservableCollection<string> _imageCompareDatabases;
    [ObservableProperty] private string? _selectedImageCompareDatabase;
    [ObservableProperty] private ObservableCollection<int> _binaryOcrPixelsAreSpaceList;
    [ObservableProperty] private int _selectedBinaryOcrPixelsAreSpace;
    [ObservableProperty] private double _binaryOcrMaxErrorPercent;
    [ObservableProperty] private ObservableCollection<int> _nOcrMaxWrongPixelsList;
    [ObservableProperty] private int _selectedNOcrMaxWrongPixels;
    [ObservableProperty] private ObservableCollection<int> _nOcrPixelsAreSpaceList;
    [ObservableProperty] private int _selectedNOcrPixelsAreSpace;
    [ObservableProperty] private ObservableCollection<string> _ollamaLanguages;
    [ObservableProperty] private string? _selectedOllamaLanguage;
    [ObservableProperty] private ObservableCollection<TesseractDictionary> _tesseractDictionaryItems;
    [ObservableProperty] private TesseractDictionary? _selectedTesseractDictionaryItem;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private string _ollamaUrl;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private Bitmap? _currentImageSource;
    [ObservableProperty] private string _currentBitmapInfo;
    [ObservableProperty] private string _currentText;
    [ObservableProperty] private bool _isOcrRunning;
    [ObservableProperty] private bool _isNOcrVisible;
    [ObservableProperty] private bool _isOllamaVisible;
    [ObservableProperty] private bool _isTesseractVisible;
    [ObservableProperty] private bool _isBinaryImageCompareVisible;
    [ObservableProperty] private bool _isPaddleOcrVisible;
    [ObservableProperty] private bool _isGoogleVisionVisible;
    [ObservableProperty] private bool _isGoogleLensVisible;
    [ObservableProperty] private bool _isMistralOcrVisible;
    [ObservableProperty] private bool _nOcrDrawUnknownText;
    [ObservableProperty] private bool _isInspectLineVisible;
    [ObservableProperty] private bool _isInspectAdditionsVisible;
    [ObservableProperty] private string _googleVisionApiKey;
    [ObservableProperty] private string _mistralApiKey;
    [ObservableProperty] private ObservableCollection<OcrLanguage> _googleVisionLanguages;
    [ObservableProperty] private OcrLanguage? _selectedGoogleVisionLanguage;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _googleLensLanguages;
    [ObservableProperty] private OcrLanguage2 _selectedGoogleLensLanguage;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleOcrLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleOcrLanguage;
    [ObservableProperty] private bool _showContextMenu;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private bool _doFixOcrErrors;
    [ObservableProperty] private bool _doPromptForUnknownWords;
    [ObservableProperty] private bool _doTryToGuessUnknownWords;
    [ObservableProperty] private bool _doAutoBreak;
    [ObservableProperty] private bool _isDictionaryLoaded;
    [ObservableProperty] private ObservableCollection<UnknownWordItem> _unknownWords;
    [ObservableProperty] private UnknownWordItem? _selectedUnknownWord;
    [ObservableProperty] private bool _isUnknownWordSelected;
    [ObservableProperty] private ObservableCollection<ReplacementUsedItem> _allFixes;
    [ObservableProperty] private ReplacementUsedItem? _selectedAllFix;
    [ObservableProperty] private ObservableCollection<GuessUsedItem> _allGuesses;
    [ObservableProperty] private GuessUsedItem? _selectedAllGuess;
    [ObservableProperty] private bool _hasPreProcessingSettings;
    [ObservableProperty] private bool _hasCaptureTopAlign;
    [ObservableProperty] private FontFamily _textBoxFontFamily;
    [ObservableProperty] private decimal _textBoxFontSize;
    [ObservableProperty] private FontWeight _textBoxFontWeight;
    [ObservableProperty] string _unknownWordsRemoveCurrentText;

    public Window? Window { get; set; }
    public DataGrid SubtitleGrid { get; set; }
    public MatroskaTrackInfo? SelectedMatroskaTrack { get; set; }
    public bool OkPressed { get; private set; }

    public readonly List<SubtitleLineViewModel> OcredSubtitle;

    private IOcrSubtitle? _ocrSubtitle;
    private readonly INOcrCaseFixer _nOcrCaseFixer;
    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IOcrFixEngine2 _ocrFixEngine;
    private readonly IBinaryOcrMatcher _binaryOcrMatcher;
    private PreProcessingSettings? _preProcessingSettings;
    private bool _isCtrlDown;
    private CancellationTokenSource _cancellationTokenSource;
    private NOcrDb? _nOcrDb;
    private readonly List<SkipOnceChar> _runOnceChars;
    private readonly List<SkipOnceChar> _skipOnceChars;
    private readonly NOcrAddHistoryManager _nOcrAddHistoryManager;
    private readonly BinaryOcrAddHistoryManager _binaryOcrAddHistoryManager;
    private int _pendingScrollIndex = -1;
    private readonly object _scrollLock = new object();

    public OcrViewModel(
        INOcrCaseFixer nOcrCaseFixer,
        IWindowService windowService,
        IFileHelper fileHelper,
        ISpellCheckManager spellCheckManager,
        IOcrFixEngine2 ocrFixEngine,
        IBinaryOcrMatcher binaryOcrMatcher)
    {
        _nOcrCaseFixer = nOcrCaseFixer;
        _windowService = windowService;
        _fileHelper = fileHelper;
        _spellCheckManager = spellCheckManager;
        _ocrFixEngine = ocrFixEngine;
        _binaryOcrMatcher = binaryOcrMatcher;

        Title = Se.Language.Ocr.Ocr;
        OcrEngines = new ObservableCollection<OcrEngineItem>(OcrEngineItem.GetOcrEngines());
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>();
        NOcrDatabases = new ObservableCollection<string>();
        ImageCompareDatabases = new ObservableCollection<string>(BinaryOcrDb.GetDatabases());
        SelectedImageCompareDatabase = ImageCompareDatabases.FirstOrDefault();
        NOcrMaxWrongPixelsList = new ObservableCollection<int>(Enumerable.Range(0, 500));
        NOcrPixelsAreSpaceList = new ObservableCollection<int>(Enumerable.Range(1, 50));
        BinaryOcrPixelsAreSpaceList = new ObservableCollection<int>(Enumerable.Range(1, 50));
        OllamaLanguages = new ObservableCollection<string>(Iso639Dash2LanguageCode.List
            .Select(p => p.EnglishName)
            .OrderBy(p => p));
        SelectedOllamaLanguage = "English";
        SubtitleGrid = new DataGrid();
        CurrentBitmapInfo = string.Empty;
        CurrentText = string.Empty;
        ProgressText = string.Empty;
        OllamaModel = string.Empty;
        OllamaUrl = string.Empty;
        TesseractDictionaryItems = new ObservableCollection<TesseractDictionary>();
        GoogleVisionApiKey = string.Empty;
        MistralApiKey = string.Empty;
        GoogleVisionLanguages = new ObservableCollection<OcrLanguage>(GoogleVisionOcr.GetLanguages().OrderBy(p => p.ToString()));
        GoogleLensLanguages = new ObservableCollection<OcrLanguage2>(GoogleLensOcr.GetLanguages().OrderBy(p => p.ToString()));
        SelectedGoogleLensLanguage = GoogleLensLanguages.FirstOrDefault(p => p.Code == "en") ?? GoogleLensLanguages.First();
        PaddleOcrLanguages = new ObservableCollection<OcrLanguage2>(PaddleOcr.GetLanguages().OrderBy(p => p.ToString()));
        OcredSubtitle = new List<SubtitleLineViewModel>();
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        UnknownWords = new ObservableCollection<UnknownWordItem>();
        AllFixes = new ObservableCollection<ReplacementUsedItem>();
        AllGuesses = new ObservableCollection<GuessUsedItem>();
        _runOnceChars = new List<SkipOnceChar>();
        _skipOnceChars = new List<SkipOnceChar>();
        _nOcrAddHistoryManager = new NOcrAddHistoryManager();
        _binaryOcrAddHistoryManager = new BinaryOcrAddHistoryManager();
        _cancellationTokenSource = new CancellationTokenSource();
        TextBoxFontFamily = new FontFamily(FontHelper.GetSystemFonts().First());
        TextBoxFontSize = 14;
        TextBoxFontWeight = FontWeight.Regular;
        UnknownWordsRemoveCurrentText = string.Empty;
        LoadSettings();
        EngineSelectionChanged();
        LoadDictionaries();
    }

    private void LoadSettings()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var ocr = Se.Settings.Ocr;
            if (!string.IsNullOrEmpty(ocr.Engine) && OcrEngines.Any(p => p.Name == ocr.Engine))
            {
                SelectedOcrEngine = OcrEngines.First(p => p.Name == ocr.Engine);
            }

            if (!string.IsNullOrEmpty(ocr.NOcrDatabase) && NOcrDatabases.Contains(ocr.NOcrDatabase))
            {
                SelectedNOcrDatabase = ocr.NOcrDatabase;
            }

            SelectedNOcrMaxWrongPixels = ocr.NOcrMaxWrongPixels;
            NOcrDrawUnknownText = ocr.NOcrDrawUnknownText;
            SelectedNOcrPixelsAreSpace = ocr.NOcrPixelsAreSpace;
            SelectedBinaryOcrPixelsAreSpace = ocr.BinaryOcrPixelsAreSpace;
            BinaryOcrMaxErrorPercent = ocr.BinaryOcrMaxErrorPercent;
            OllamaModel = ocr.OllamaModel;
            OllamaUrl = ocr.OllamaUrl;
            SelectedOllamaLanguage = ocr.OllamaLanguage;
            GoogleVisionApiKey = ocr.GoogleVisionApiKey;
            MistralApiKey = ocr.MistralApiKey;
            SelectedGoogleVisionLanguage = GoogleVisionLanguages.FirstOrDefault(p => p.Code == ocr.GoogleVisionLanguage);
            SelectedPaddleOcrLanguage = PaddleOcrLanguages.FirstOrDefault(p => p.Code == Se.Settings.Ocr.PaddleOcrLastLanguage) ?? PaddleOcrLanguages.First();
            SelectedGoogleLensLanguage = GoogleLensLanguages.FirstOrDefault(p => p.Code == Se.Settings.Ocr.GoogleLensOcrLastLanguage) ?? GoogleLensLanguages.First();
            TextBoxFontSize = ocr.TextBoxFontSize;
            if (Se.Settings.Ocr.TextBoxFontBold)
            {
                TextBoxFontWeight = FontWeight.Bold;
            }

            try
            {
                TextBoxFontFamily = new FontFamily(ocr.TextBoxFontName);
            }
            catch
            {
                // ignored
            }

            DoFixOcrErrors = ocr.DoFixOcrErrors;
            DoPromptForUnknownWords = ocr.DoPromptForUnknownWords;
            DoTryToGuessUnknownWords = ocr.DoTryToGuessUnknownWords;
            DoAutoBreak = ocr.DoAutoBreak;
        });
    }

    private void SaveSettings()
    {
        var ocr = Se.Settings.Ocr;
        ocr.Engine = SelectedOcrEngine?.Name ?? "nOCR";
        ocr.NOcrDatabase = SelectedNOcrDatabase ?? "Latin";
        ocr.NOcrMaxWrongPixels = SelectedNOcrMaxWrongPixels;
        ocr.NOcrDrawUnknownText = NOcrDrawUnknownText;
        ocr.NOcrPixelsAreSpace = SelectedNOcrPixelsAreSpace;
        ocr.BinaryOcrPixelsAreSpace = SelectedBinaryOcrPixelsAreSpace;
        ocr.BinaryOcrMaxErrorPercent = BinaryOcrMaxErrorPercent;
        ocr.OllamaModel = OllamaModel;
        ocr.OllamaUrl = OllamaUrl;
        ocr.OllamaLanguage = SelectedOllamaLanguage ?? "English";
        ocr.GoogleVisionApiKey = GoogleVisionApiKey;
        ocr.MistralApiKey = MistralApiKey;
        ocr.GoogleVisionLanguage = SelectedGoogleVisionLanguage?.Code ?? "en";
        ocr.DoFixOcrErrors = DoFixOcrErrors;
        ocr.DoPromptForUnknownWords = DoPromptForUnknownWords;
        ocr.DoTryToGuessUnknownWords = DoTryToGuessUnknownWords;
        ocr.DoAutoBreak = DoAutoBreak;
        ocr.TextBoxFontSize = TextBoxFontSize;
        ocr.TextBoxFontBold = TextBoxFontWeight == FontWeight.Bold;
        ocr.TextBoxFontName = TextBoxFontFamily.Name;

        if (SelectedDictionary != null)
        {
            Se.Settings.Ocr.LastLanguageDictionaryFile = SelectedDictionary.DictionaryFileName;
        }

        Se.SaveSettings();
    }

    private void LoadDictionaries()
    {
        var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        Dictionaries.Clear();
        Dictionaries.Add(new SpellCheckDictionaryDisplay
        {
            Name = GetDictionaryNameNone(),
            DictionaryFileName = string.Empty,
        });
        Dictionaries.AddRange(spellCheckLanguages);
        if (Dictionaries.Count > 0)
        {
            if (!string.IsNullOrEmpty(Se.Settings.Ocr.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.Ocr.LastLanguageDictionaryFile);
            }

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries[0];
            }

            _spellCheckManager.Initialize(SelectedDictionary.DictionaryFileName, SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(SelectedDictionary));
        }
    }

    private static string GetDictionaryNameNone()
    {
        return "[" + Se.Language.General.None + "]";
    }

    private string? GetNOcrLanguageFileName()
    {
        if (SelectedNOcrDatabase == null)
        {
            return null;
        }

        return Path.Combine(Se.OcrFolder, $"{SelectedNOcrDatabase}.nocr");
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private async Task PickFont()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(Window, vm =>
        {
            vm.Initialize(true, true);
            vm.FontSize = TextBoxFontSize;
            vm.IsFontBold = TextBoxFontWeight == FontWeight.Bold;
        });

        _isCtrlDown = false;

        if (!result.OkPressed || result.SelectedFontName == null)
        {
            return;
        }

        TextBoxFontFamily = new FontFamily(result.SelectedFontName);
        TextBoxFontSize = result.FontSize;
        TextBoxFontWeight = result.IsFontBold ? FontWeight.Bold : FontWeight.Regular;
    }

    [RelayCommand]
    private async Task PickDictionary()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!);
        _isCtrlDown = false;
        if (result.OkPressed && result.SelectedDictionary != null)
        {
            LoadDictionaries();
            SelectedDictionary = Dictionaries
                .FirstOrDefault(d =>
                    d.Name.Contains(result.SelectedDictionary.EnglishName, StringComparison.OrdinalIgnoreCase) ||
                    d.Name.Contains(result.SelectedDictionary.NativeName, StringComparison.OrdinalIgnoreCase));

            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries.FirstOrDefault();
            }
        }
    }

    [RelayCommand]
    private void UnknownWordsClear()
    {
        UnknownWords.Clear();
        IsUnknownWordSelected = false;
    }

    [RelayCommand]
    private void UnknownWordsRemoveCurrent()
    {
        var word = SelectedUnknownWord?.Word.Word ?? string.Empty;
        if (string.IsNullOrEmpty(word))
        {
            return;
        }

        var itemsToRemove = UnknownWords.Where(uw => uw.Word.Word.Equals(word, StringComparison.Ordinal)).ToList();
        foreach (var item in itemsToRemove)
        {
            UnknownWords.Remove(item);
        }

        IsUnknownWordSelected = false;
    }

    [RelayCommand]
    private void PauseOcr()
    {
        IsOcrRunning = false;
        _cancellationTokenSource.Cancel();
    }

    [RelayCommand]
    private async Task AddUnknownWordToNames()
    {
        if (Window == null)
        {
            return;
        }

        var selectedWord = SelectedUnknownWord;
        if (selectedWord == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AddToNamesListWindow, AddToNamesListViewModel>(Window,
            vm => { vm.Initialize(selectedWord.Word.Word, Dictionaries.ToList(), SelectedDictionary); });
        _isCtrlDown = false;
    }

    [RelayCommand]
    private async Task AddUnknownWordToUserDictionary()
    {
        var selectedWord = SelectedUnknownWord;
        if (selectedWord == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AddToUserDictionaryWindow, AddToUserDictionaryViewModel>(Window!,
            vm => { vm.Initialize(selectedWord.Word.Word, Dictionaries.ToList(), SelectedDictionary); });
        _isCtrlDown = false;
    }

    [RelayCommand]
    private async Task AddUnknownWordToOcrPair()
    {
        var selectedWord = SelectedUnknownWord;
        if (selectedWord == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AddToOcrReplaceListWindow, AddToOcrReplaceListViewModel>(Window!,
            vm => { vm.Initialize(selectedWord.Word.Word, Dictionaries.ToList(), SelectedDictionary); });
        _isCtrlDown = false;
    }

    [RelayCommand]
    private async Task GoogleUnknowWord()
    {
        var selectedWord = SelectedUnknownWord;
        if (selectedWord == null)
        {
            return;
        }

        await Window!.Launcher.LaunchUriAsync(new Uri("https://www.google.com/search?q=" + Utilities.UrlEncode(selectedWord.Word.Word)));
    }


    [RelayCommand]
    private async Task EditExport()
    {
        if (Window == null || _ocrSubtitle == null || _ocrSubtitle.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryEditWindow, BinaryEditViewModel>(Window, vm => { vm.Initialize(string.Empty, _ocrSubtitle); });
        _isCtrlDown = false;
    }

    [RelayCommand]
    private async Task InspectLine()
    {
        var item = SelectedOcrSubtitleItem;
        var engine = SelectedOcrEngine;
        if (item == null || engine == null)
        {
            return;
        }

        if (engine.EngineType == OcrEngineType.nOcr)
        {
            await InspectLineNOcr(item);
        }

        if (engine.EngineType == OcrEngineType.BinaryImageCompare)
        {
            await InspectLineBinaryImageCompareOcr(item);
        }
    }

    private async Task InspectLineNOcr(OcrSubtitleItem item)
    {
        if (!InitNOcrDb())
        {
            return;
        }

        var bitmap = item.GetSkBitmap();
        var nBmp = new NikseBitmap2(bitmap);
        nBmp.MakeTwoColor(200);
        nBmp.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters =
            NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nBmp, SelectedNOcrPixelsAreSpace, false, true, 20, true);
        var matches = new List<NOcrChar?>();
        foreach (var splitterItem in letters)
        {
            if (splitterItem.NikseBitmap == null)
            {
                var match = new NOcrChar { Text = splitterItem.SpecialCharacter ?? string.Empty };
                matches.Add(match);
            }
            else
            {
                var match = _nOcrDb!.GetMatch(nBmp, letters, splitterItem, splitterItem.Top, true,
                    SelectedNOcrMaxWrongPixels);
                matches.Add(match);
            }
        }

        var result = await _windowService.ShowDialogAsync<NOcrInspectWindow, NOcrInspectViewModel>(Window!,
            vm =>
            {
                vm.Initialize(nBmp.GetBitmap(), SelectedOcrSubtitleItem, _nOcrDb, SelectedNOcrMaxWrongPixels, letters,
                    matches);
            });
        _isCtrlDown = false;

        if (result.AddBetterMatchPressed)
        {
            var characterAddResult =
                await _windowService.ShowDialogAsync<NOcrCharacterAddWindow, NOcrCharacterAddViewModel>(Window!,
                    vm =>
                    {
                        vm.Initialize(nBmp, item, letters, result.LetterIndex, _nOcrDb!, SelectedNOcrMaxWrongPixels,
                            _nOcrAddHistoryManager, false, false);
                    });

            _isCtrlDown = false;

            if (characterAddResult.OkPressed)
            {
                var letterBitmap = letters[result.LetterIndex].NikseBitmap;
                _nOcrAddHistoryManager.Add(characterAddResult.NOcrChar, letterBitmap, OcrSubtitleItems.IndexOf(item));
                _nOcrDb!.Add(characterAddResult.NOcrChar);
                _ = Task.Run(_nOcrDb.Save);
            }
            else if (characterAddResult.InspectHistoryPressed)
            {
                await _windowService.ShowDialogAsync<NOcrCharacterHistoryWindow, NOcrCharacterHistoryViewModel>(Window!,
                    vm => { vm.Initialize(_nOcrDb!, _nOcrAddHistoryManager); });

                _isCtrlDown = false;
            }
        }
    }

    private async Task InspectLineBinaryImageCompareOcr(OcrSubtitleItem item)
    {
        var db = InitImageComparOcrDb();
        if (db == null)
        {
            return;
        }

        var bitmap = item.GetSkBitmap();
        var nBmp = new NikseBitmap2(bitmap);
        nBmp.MakeTwoColor(200);
        nBmp.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters =
            NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nBmp, SelectedNOcrPixelsAreSpace, false, true, 20, true);
        var matches = new List<BinaryOcrMatcher.CompareMatch?>();
        foreach (var splitterItem in letters)
        {
            if (splitterItem.NikseBitmap == null)
            {
                var match = new BinaryOcrMatcher.CompareMatch(splitterItem.SpecialCharacter ?? string.Empty, false, 0, nameof(splitterItem.SpecialCharacter));
                matches.Add(match);
            }
            else
            {
                var match = _binaryOcrMatcher.GetCompareMatch(splitterItem, out _, letters, letters.IndexOf(splitterItem), db);
                matches.Add(match);
            }
        }

        var result = await _windowService.ShowDialogAsync<BinaryOcrInspectWindow, BinaryOcrInspectViewModel>(Window!,
            vm =>
            {
                vm.Initialize(nBmp.GetBitmap(), SelectedOcrSubtitleItem, db, SelectedNOcrMaxWrongPixels, letters,
                    matches);
            });

        _isCtrlDown = false;

        if (result.AddBetterMatchPressed)
        {
            var characterAddResult =
                await _windowService.ShowDialogAsync<BinaryOcrCharacterAddWindow, BinaryOcrCharacterAddViewModel>(Window!,
                    vm =>
                    {
                        vm.Initialize(nBmp, item, letters, result.LetterIndex, db, SelectedNOcrMaxWrongPixels,
                            _binaryOcrAddHistoryManager, false, false);
                    });

            _isCtrlDown = false;

            if (characterAddResult.OkPressed && characterAddResult.BinaryOcrBitmap != null)
            {
                var letterBitmap = letters[result.LetterIndex].NikseBitmap;
                _binaryOcrAddHistoryManager.Add(characterAddResult.BinaryOcrBitmap, letterBitmap, OcrSubtitleItems.IndexOf(item));
                db.Add(characterAddResult.BinaryOcrBitmap);
                _ = Task.Run(db.Save);
            }
            else if (characterAddResult.InspectHistoryPressed)
            {
                await _windowService.ShowDialogAsync<BinaryOcrCharacterHistoryWindow, BinaryOcrCharacterHistoryViewModel>(Window!,
                    vm => { vm.Initialize(db, _binaryOcrAddHistoryManager); });

                _isCtrlDown = false;
            }
        }
    }

    [RelayCommand]
    private async Task InspectAdditions()
    {
        var item = SelectedOcrSubtitleItem;
        var engine = SelectedOcrEngine;
        if (item == null || engine == null)
        {
            return;
        }

        if (engine.EngineType == OcrEngineType.nOcr)
        {
            await _windowService.ShowDialogAsync<NOcrCharacterHistoryWindow, NOcrCharacterHistoryViewModel>(Window!,
                vm => { vm.Initialize(_nOcrDb!, _nOcrAddHistoryManager); });

            _isCtrlDown = false;
        }

        if (engine.EngineType == OcrEngineType.BinaryImageCompare)
        {
            var db = InitImageComparOcrDb();
            if (db == null)
            {
                return;
            }

            await _windowService.ShowDialogAsync<BinaryOcrCharacterHistoryWindow, BinaryOcrCharacterHistoryViewModel>(Window!,
                vm => { vm.Initialize(db, _binaryOcrAddHistoryManager); });

            _isCtrlDown = false;
        }
    }

    [RelayCommand]
    private async Task ViewSelectedImage()
    {
        var item = SelectedOcrSubtitleItem;
        if (item == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ShowImageWindow, ShowImageViewModel>(Window!, vm => { vm.Initialize(Se.Language.Ocr.OcrImage, item.GetBitmapCropped(), item.Text); });
        _isCtrlDown = false;

        if (result.LeftPressed)
        {
            var idx = OcrSubtitleItems.IndexOf(item);
            var prevIdx = idx - 1;
            if (prevIdx >= 0)
            {
                SelectedOcrSubtitleItem = OcrSubtitleItems[prevIdx];
                await ViewSelectedImage();
            }
        }
        else if (result.RightPressed)
        {
            var idx = OcrSubtitleItems.IndexOf(item);
            var nextIdx = idx + 1;
            if (nextIdx < OcrSubtitleItems.Count)
            {
                SelectedOcrSubtitleItem = OcrSubtitleItems[nextIdx];
                await ViewSelectedImage();
            }
        }
    }

    [RelayCommand]
    private async Task SaveImageAs()
    {
        var item = SelectedOcrSubtitleItem;
        if (item == null)
        {
            return;
        }

        var imageIndex = OcrSubtitleItems.IndexOf(item) + 1;
        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, ".png", $"image{imageIndex}", Se.Language.General.SaveImageAs);
        _isCtrlDown = false;
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var bitmap = item.GetBitmap();
        bitmap.Save(fileName, 100);
    }

    [RelayCommand]
    private async Task CopyImageToClipboard()
    {
        var item = SelectedOcrSubtitleItem;
        if (item == null || Window == null || Window.Clipboard == null)
        {
            return;
        }

        await ClipboardHelper.CopyImageToClipboard(item.GetBitmap());
    }

    [RelayCommand]
    private async Task PickOllamaModel()
    {
        var result = await _windowService.ShowDialogAsync<PickOllamaModelWindow, PickOllamaModelViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.PickOllamaModel, OllamaModel, OllamaUrl); });

        _isCtrlDown = false;

        if (result.OkPressed && result.SelectedModel != null)
        {
            OllamaModel = result.SelectedModel;
        }
    }

    [RelayCommand]
    private async Task ShowNOcrSettings()
    {
        InitNOcrDb();
        var result =
            await _windowService.ShowDialogAsync<NOcrSettingsWindow, NOcrSettingsViewModel>(Window!,
                vm => { vm.Initialize(_nOcrDb!); });

        _isCtrlDown = false;

        if (result.EditPressed)
        {
            await _windowService.ShowDialogAsync<NOcrDbEditWindow, NOcrDbEditViewModel>(Window!,
                vm => { vm.Initialize(_nOcrDb!); });

            _isCtrlDown = false;

            return;
        }

        if (result.DeletePressed)
        {
            try
            {
                File.Delete(_nOcrDb!.FileName);
                NOcrDatabases.Remove(SelectedNOcrDatabase!);
                SelectedNOcrDatabase = NOcrDatabases.FirstOrDefault();

                if (SelectedNOcrDatabase == null)
                {
                    _nOcrDb = new NOcrDb(Path.Combine(Se.OcrFolder, "Default.nocr"));
                    _nOcrDb.Save();
                    NOcrDatabases.Add("Default");
                    SelectedNOcrDatabase = NOcrDatabases.FirstOrDefault();
                }
            }
            catch
            {
                await MessageBox.Show(
                    Window!,
                    "Error deleting file",
                    $"Could not delete the file {_nOcrDb!.FileName}.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return;
        }

        if (result.NewPressed)
        {
            var newResult = await _windowService.ShowDialogAsync<NOcrDbNewWindow, NOcrDbNewViewModel>(Window!,
                vm => { vm.Initialize(Se.Language.Ocr.NewNOcrDatabase, string.Empty); });
            _isCtrlDown = false;
            if (newResult.OkPressed)
            {
                if (!Directory.Exists(Se.OcrFolder))
                {
                    Directory.CreateDirectory(Se.OcrFolder);
                }

                var newFileName = Path.Combine(Se.OcrFolder, newResult.DatabaseName + ".nocr");
                if (File.Exists(newFileName))
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.General.FileAlreadyExists,
                        string.Format(Se.Language.General.FileXAlreadyExists, newFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                _nOcrDb = new NOcrDb(newFileName);
                _nOcrDb.Save();
                NOcrDatabases.Add(newResult.DatabaseName);
                var sortedList = NOcrDatabases.OrderBy(p => p).ToList();
                NOcrDatabases.Clear();
                NOcrDatabases.AddRange(sortedList);
                SelectedNOcrDatabase = newResult.DatabaseName;
            }

            return;
        }

        if (result.RenamePressed)
        {
            var newResult = await _windowService.ShowDialogAsync<NOcrDbNewWindow, NOcrDbNewViewModel>(Window!,
                vm =>
                {
                    vm.Initialize(Se.Language.Ocr.RenameNOcrDatabase,
                        Path.GetFileNameWithoutExtension(_nOcrDb!.FileName));
                });
            _isCtrlDown = false;
            if (newResult.OkPressed)
            {
                if (!Directory.Exists(Se.OcrFolder))
                {
                    Directory.CreateDirectory(Se.OcrFolder);
                }

                var newFileName = Path.Combine(Se.OcrFolder, newResult.DatabaseName + ".nocr");
                if (File.Exists(newFileName))
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.General.FileAlreadyExists,
                        string.Format(Se.Language.General.FileXAlreadyExists, newFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                File.Move(_nOcrDb!.FileName, newFileName);
                NOcrDatabases.Clear();
                foreach (var s in NOcrDb.GetDatabases().OrderBy(p => p))
                {
                    NOcrDatabases.Add(s);
                }

                SelectedNOcrDatabase = newResult.DatabaseName;
            }
        }
    }

    [RelayCommand]
    private async Task ShowBinaryOcrSettings()
    {
        var dbName = SelectedImageCompareDatabase;
        if (string.IsNullOrEmpty(dbName))
        {
            return;
        }

        var result =
            await _windowService.ShowDialogAsync<BinaryOcrSettingsWindow, BinaryOcrSettingsViewModel>(Window!,
                vm => { vm.Initialize(dbName); });

        _isCtrlDown = false;

        if (result.EditPressed)
        {
            await _windowService.ShowDialogAsync<BinaryOcrDbEditWindow, BinaryOcrDbEditViewModel>(Window!,
                vm => { vm.Initialize(SelectedImageCompareDatabase!); });

            _isCtrlDown = false;

            return;
        }

        if (result.DeletePressed)
        {
            try
            {
                var fileName = Path.Combine(Se.OcrFolder, dbName + BinaryOcrDb.Extension);
                File.Delete(fileName);

                ImageCompareDatabases.Clear();
                foreach (var db in BinaryOcrDb.GetDatabases())
                {
                    ImageCompareDatabases.Add(db);
                }
                SelectedImageCompareDatabase = ImageCompareDatabases.FirstOrDefault();
                if (SelectedImageCompareDatabase == null)
                {
                    var binaryOcrDb = new BinaryOcrDb(Path.Combine(Se.OcrFolder, "Latin" + BinaryOcrDb.Extension));
                    binaryOcrDb.Save();
                    ImageCompareDatabases.Add("Latin");
                    SelectedImageCompareDatabase = ImageCompareDatabases.FirstOrDefault();
                }
            }
            catch
            {
                await MessageBox.Show(
                    Window!,
                    "Error deleting file",
                    $"Could not delete the file {Path.Combine(Se.OcrFolder, dbName + BinaryOcrDb.Extension)}.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return;
        }

        if (result.NewPressed)
        {
            var newResult = await _windowService.ShowDialogAsync<BinaryOcrDbNewWindow, BinaryOcrDbNewViewModel>(Window!,
                vm => { vm.Initialize(Se.Language.Ocr.NewNOcrDatabase, string.Empty); });
            if (newResult.OkPressed)
            {
                if (!Directory.Exists(Se.OcrFolder))
                {
                    Directory.CreateDirectory(Se.OcrFolder);
                }

                var newFileName = Path.Combine(Se.OcrFolder, newResult.DatabaseName + BinaryOcrDb.Extension);
                if (File.Exists(newFileName))
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.General.FileAlreadyExists,
                        string.Format(Se.Language.General.FileXAlreadyExists, newFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                var binaryOcrDb = new BinaryOcrDb(newFileName);
                binaryOcrDb.Save();
                ImageCompareDatabases.Add(newResult.DatabaseName);
                var sortedList = ImageCompareDatabases.OrderBy(p => p).ToList();
                ImageCompareDatabases.Clear();
                ImageCompareDatabases.AddRange(sortedList);
                SelectedImageCompareDatabase = newResult.DatabaseName;
            }

            return;
        }

        if (result.RenamePressed)
        {
            var newResult = await _windowService.ShowDialogAsync<BinaryOcrDbNewWindow, BinaryOcrDbNewViewModel>(Window!,
            vm =>
            {
                vm.Initialize(Se.Language.Ocr.RenameNOcrDatabase, result.BinaryOcrDatabaseName);
            });

            _isCtrlDown = false;

            if (newResult.OkPressed)
            {
                var newFileName = Path.Combine(Se.OcrFolder, newResult.DatabaseName + BinaryOcrDb.Extension);
                if (File.Exists(newFileName))
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.General.FileAlreadyExists,
                        string.Format(Se.Language.General.FileXAlreadyExists, newFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                var oldFileName = Path.Combine(Se.OcrFolder, dbName + BinaryOcrDb.Extension);
                File.Move(oldFileName, newFileName);
                ImageCompareDatabases.Clear();
                foreach (var s in BinaryOcrDb.GetDatabases().OrderBy(p => p))
                {
                    ImageCompareDatabases.Add(s);
                }

                SelectedImageCompareDatabase = newResult.DatabaseName;
            }
        }
    }

    [RelayCommand]
    private async Task PickTesseractModel()
    {
        await TesseractModelDownload();
    }

    [RelayCommand]
    private void ToggleItalic()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeItalic = true;
        foreach (var item in selectedItems)
        {
            if (item is OcrSubtitleItem ocrItem)
            {
                if (first)
                {
                    first = false;
                    makeItalic = !ocrItem.Text.Contains("<i>");
                }

                ocrItem.Text = ocrItem.Text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
                ocrItem.Text = ocrItem.Text.Replace("<I>", string.Empty).Replace("</I>", string.Empty);
                if (makeItalic)
                {
                    if (!string.IsNullOrEmpty(ocrItem.Text))
                    {
                        ocrItem.Text = $"<i>{ocrItem.Text}</i>";
                    }
                }

                var idx = OcrSubtitleItems.IndexOf(ocrItem);
                if (_ocrFixEngine.IsLoaded())
                {
                    ocrItem.FixResult = new OcrFixLineResult
                    {
                        LineIndex = idx,

                        //TODO: spell check
                        Words = new List<OcrFixLinePartResult> { new() { Word = ocrItem.Text, IsSpellCheckedOk = null } },
                    };
                }
                else
                {
                    ocrItem.FixResult = new OcrFixLineResult(idx, ocrItem.Text);
                }
            }
        }
    }

    [RelayCommand]
    private void ToggleTopAlign()
    {
        HasCaptureTopAlign = !HasCaptureTopAlign;
    }

    [RelayCommand]
    private async Task ShowPreProcessing()
    {
        if (Window == null)
        {
            UpdateImagePreProcessingStatus();
            return;
        }

        var selectedItem = SelectedOcrSubtitleItem;
        if (selectedItem == null)
        {
            UpdateImagePreProcessingStatus();
            return;
        }

        var result = await _windowService
            .ShowDialogAsync<PreProcessingWindow, PreProcessingViewModel>(Window, vm => { vm.Initialize(_preProcessingSettings, selectedItem.GetSkBitmapClean()); });

        _isCtrlDown = false;

        if (result.OkPressed)
        {
            _preProcessingSettings = result.PreProcessingSettings;
            foreach (var item in OcrSubtitleItems)
            {
                item.PreProcessingSettings = _preProcessingSettings;
            }

            var tempIdx = OcrSubtitleItems.IndexOf(selectedItem);
            var temp = OcrSubtitleItems.ToList();
            OcrSubtitleItems.Clear();
            OcrSubtitleItems.AddRange(temp);
            SelectAndScrollToRow(tempIdx);
        }

        UpdateImagePreProcessingStatus();
    }

    private void UpdateImagePreProcessingStatus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_preProcessingSettings == null)
            {
                HasPreProcessingSettings = false;
                return;
            }

            HasPreProcessingSettings =
                _preProcessingSettings.CropTransparentColors ||
                _preProcessingSettings.InverseColors ||
                _preProcessingSettings.Binarize ||
                _preProcessingSettings.RemoveBorders;
        });
    }

    [RelayCommand]
    private void ToggleBold()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeBold = true;
        foreach (var item in selectedItems)
        {
            if (item is OcrSubtitleItem ocrItem)
            {
                if (first)
                {
                    first = false;
                    makeBold = !ocrItem.Text.Contains("<b>");
                }

                ocrItem.Text = ocrItem.Text.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                ocrItem.Text = ocrItem.Text.Replace("<B>", string.Empty).Replace("</B>", string.Empty);
                if (makeBold)
                {
                    if (!string.IsNullOrEmpty(ocrItem.Text))
                    {
                        ocrItem.Text = $"<b>{ocrItem.Text}</b>";
                    }
                }

                var idx = OcrSubtitleItems.IndexOf(ocrItem);
                if (_ocrFixEngine.IsLoaded())
                {
                    ocrItem.FixResult = new OcrFixLineResult
                    {
                        LineIndex = idx,
                        // TODO: spell check
                        Words = new List<OcrFixLinePartResult> { new() { Word = ocrItem.Text, IsSpellCheckedOk = null } },
                    };
                }
                else
                {
                    ocrItem.FixResult = new OcrFixLineResult(idx, ocrItem.Text);
                }
            }
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;

        OcredSubtitle.Clear();
        for (var i = 0; i < OcrSubtitleItems.Count; i++)
        {
            var item = OcrSubtitleItems[i];
            var subtitleLine = new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = item.Text,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
            };
            OcredSubtitle.Add(subtitleLine);
        }

        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        Close();
    }

    [RelayCommand]
    private void DeleteSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var selectedIndices = new List<int>();
        foreach (var selectedItem in selectedItems)
        {
            if (selectedItem is OcrSubtitleItem item)
            {
                var idx = OcrSubtitleItems.IndexOf(item);
                if (idx >= 0)
                {
                    selectedIndices.Add(idx);

                    var remov = UnknownWords.Where(uw => uw.Item == item).ToList();
                    foreach (var unknownWord in remov)
                    {
                        UnknownWords.Remove(unknownWord);
                    }
                }
            }
        }

        var itemsToRemove = selectedIndices
            .Select(idx => OcrSubtitleItems[idx])
            .ToList();

        foreach (var item in itemsToRemove)
        {
            OcrSubtitleItems.Remove(item);
        }

        //foreach (var index in selectedIndices.OrderByDescending(p => p))
        //{
        //    _ocrSubtitle?.Delete(index);
        //}

        Renumber();

        var toRemove = new List<UnknownWordItem>();
        foreach (var unknownWord in UnknownWords)
        {
            if (!OcrSubtitleItems.Contains(unknownWord.Item))
            {
                toRemove.Add(unknownWord);
            }
        }

        foreach (var item in toRemove)
        {
            UnknownWords.Remove(item);
        }
    }

    private void Renumber()
    {
        for (var i = 0; i < OcrSubtitleItems.Count; i++)
        {
            OcrSubtitleItems[i].Number = i + 1;
        }
    }

    [RelayCommand]
    private async Task StartOcrSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        if (selectedItems == null || selectedItems.Count == 0)
        {
            return;
        }

        var selectedIndices = new List<int>();
        foreach (var selectedItem in selectedItems)
        {
            if (selectedItem is OcrSubtitleItem item)
            {
                var index = OcrSubtitleItems.IndexOf(item);
                if (index >= 0 && !selectedIndices.Contains(index))
                {
                    selectedIndices.Add(index);
                }
            }
        }

        await StartOcr(selectedIndices);
    }

    [RelayCommand]
    private async Task StartOcr(List<int>? selectedIndices)
    {
        if (IsOcrRunning || Window == null)
        {
            return;
        }

        if (!(SelectedOcrEngine is { } ocrEngine))
        {
            return;
        }

        if (ocrEngine.EngineType == OcrEngineType.Tesseract)
        {
            var tesseractOk = await CheckAndDownloadTesseract();
            if (!tesseractOk)
            {
                return;
            }

            if (SelectedTesseractDictionaryItem == null)
            {
                var tesseractModelOk = await TesseractModelDownload();
                if (!tesseractModelOk)
                {
                    return;
                }
            }
        }

        if (SelectedDictionary != null && DoFixOcrErrors && SelectedDictionary.Name != GetDictionaryNameNone())
        {
            var threeLetterCode = SelectedDictionary.GetThreeLetterCode();
            _ocrFixEngine.Initialize(OcrSubtitleItems.ToList(), threeLetterCode, SelectedDictionary);
        }
        else
        {
            _ocrFixEngine.Unload();
        }

        SaveSettings();
        _cancellationTokenSource = new CancellationTokenSource();
        IsOcrRunning = true;
        UnknownWords.Clear();

        var startFromIndex = SelectedOcrSubtitleItem == null ? 0 : OcrSubtitleItems.IndexOf(SelectedOcrSubtitleItem);
        if (selectedIndices == null)
        {
            selectedIndices = new List<int>();
            for (var i = startFromIndex; i < OcrSubtitleItems.Count; i++)
            {
                selectedIndices.Add(i);
            }
        }

        ProgressText = Se.Language.Ocr.RunningOcrDotDotDot;
        ProgressValue = 0d;

        if (ocrEngine.EngineType == OcrEngineType.nOcr)
        {
            RunNOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.BinaryImageCompare)
        {
            RunBinaryImageCompareOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Tesseract)
        {
            RunTesseractOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.PaddleOcrStandalone)
        {
            if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "Download Paddle OCR?",
                    $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
                    MessageBoxButtons.Cancel,
                    MessageBoxIcon.Question,
                    "CPU",
                    "GPU CUDA 11",
                    "GPU CUDA 12");

                if (answer == MessageBoxResult.Cancel)
                {
                    PauseOcr();
                    return;
                }

                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm =>
                    {
                        var engine = PaddleOcrDownloadType.EngineCpu;
                        if (answer == MessageBoxResult.Custom1)
                        {
                            engine = PaddleOcrDownloadType.EngineCpu;
                        }
                        else if (answer == MessageBoxResult.Custom2)
                        {
                            engine = PaddleOcrDownloadType.EngineGpu11;
                        }
                        else if (answer == MessageBoxResult.Custom3)
                        {
                            engine = PaddleOcrDownloadType.EngineGpu12;
                        }
                        
                        vm.Initialize(engine);
                    });

                _isCtrlDown = false;

                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }
            else if (Configuration.IsRunningOnLinux && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin")))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "Download Paddle OCR?",
                    $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
                    MessageBoxButtons.Cancel,
                    MessageBoxIcon.Question,
                    "CPU",
                    "GPU CUDA");

                if (answer == MessageBoxResult.Cancel)
                {
                    PauseOcr();
                    return;
                }

                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm =>
                    {
                        vm.Initialize(answer == MessageBoxResult.Custom1
                            ? PaddleOcrDownloadType.EngineCpuLinux
                            : PaddleOcrDownloadType.EngineGpuLinux);
                    });

                _isCtrlDown = false;

                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            var modelsDirectory = Se.PaddleOcrModelsFolder;
            if (!Directory.Exists(modelsDirectory))
            {
                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm => { vm.Initialize(PaddleOcrDownloadType.Models); });

                _isCtrlDown = false;

                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunPaddleOcr(selectedIndices, ocrEngine.EngineType, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.PaddleOcrPython)
        {
            var modelsDirectory = Se.PaddleOcrModelsFolder;
            if (!Directory.Exists(modelsDirectory))
            {
                var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window!,
                    vm => { vm.Initialize(PaddleOcrDownloadType.Models); });

                _isCtrlDown = false;

                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunPaddleOcr(selectedIndices, ocrEngine.EngineType, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Ollama)
        {
            RunOllamaOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.Mistral)
        {
            if (string.IsNullOrEmpty(MistralApiKey))
            {
                await MessageBox.Show(
                    Window!,
                    "Mistral API key missing",
                    $"You must enter a valid Mistral API key.{Environment.NewLine}{Environment.NewLine}Get your API key from https://mistral.ai/",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                IsOcrRunning = false;
                return;
            }

            RunMistralOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.GoogleVision)
        {
            RunGoogleVisionOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.GoogleLens)
        {
            if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Se.GoogleLensOcrFolder, GoogleLensOcr.ExeFileName)))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "Download Google Lens OCR?",
                    $"{Environment.NewLine}\"Google Lens OCR\" requires downloading Google Lens OCR standalone.{Environment.NewLine}{Environment.NewLine}Download and use Google Lens OCR?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    PauseOcr();
                    return;
                }

                var result = await _windowService.ShowDialogAsync<DownloadGoogleLensOcrWindow, DownloadGoogleLensOcrViewModel>(Window);
                _isCtrlDown = false;
                if (!result.OkPressed)
                {
                    PauseOcr();
                    return;
                }
            }

            RunGoogleLensOcr(selectedIndices, _cancellationTokenSource.Token);
        }
        else if (ocrEngine.EngineType == OcrEngineType.GoogleLensSharp)
        {
            RunGoogleLensOcrSharp(selectedIndices, _cancellationTokenSource.Token);
        }
    }

    private Lock BatchLock = new Lock();

    private void RunPaddleOcr(List<int> selectedIndices, OcrEngineType engineType, CancellationToken cancellationToken)
    {
        var numberOfImages = selectedIndices.Count;
        var ocrEngine = new PaddleOcr();
        var language = SelectedPaddleOcrLanguage?.Code ?? "en";
        var mode = Se.Settings.Ocr.PaddleOcrMode;
        Se.Settings.Ocr.PaddleOcrLastLanguage = language;

        var batchImages = new List<PaddleOcrBatchInput>(numberOfImages);
        var count = 0;
        ProgressText = $"Initializing Paddle OCR...";
        foreach (var i in selectedIndices)
        {
            count++;
            var ocrItem = OcrSubtitleItems[i];
            batchImages.Add(new PaddleOcrBatchInput
            {
                Bitmap = ocrItem.GetSkBitmap(),
                Index = i,
                Text = $"{count} / {numberOfImages}: {ocrItem.StartTime} - {ocrItem.EndTime}"
            });

            if (cancellationToken.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }
        }

        var ocrProgress = new Progress<PaddleOcrBatchProgress>(p =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            lock (BatchLock)
            {
                var number = p.Index;
                if (!selectedIndices.Contains(number))
                {
                    return;
                }

                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                ProgressValue = number / (double)OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, number + 1, OcrSubtitleItems.Count);

                var scrollToIndex = number;
                var item = p.Item;
                if (item == null)
                {
                    item = OcrSubtitleItems[p.Index];
                }

                item.Text = p.Text;
                OcrFixLineAndSetText(number, item);
            }
        });

        _ = Task.Run(async () =>
        {
            await ocrEngine.OcrBatch(engineType, batchImages, language, mode, ocrProgress, cancellationToken);
            IsOcrRunning = false;
        });
    }

    private void RunGoogleLensOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var numberOfImages = selectedIndices.Count;
        var ocrEngine = new GoogleLensOcr();
        var language = SelectedGoogleLensLanguage?.Code ?? "en";
        Se.Settings.Ocr.PaddleOcrLastLanguage = language;

        var batchImages = new List<PaddleOcrBatchInput>(numberOfImages);
        var count = 0;
        ProgressText = $"Initializing Google Lens OCR...";
        foreach (var i in selectedIndices)
        {
            count++;
            var ocrItem = OcrSubtitleItems[i];
            batchImages.Add(new PaddleOcrBatchInput
            {
                Bitmap = ocrItem.GetSkBitmap(),
                Index = i,
                Text = $"{count} / {numberOfImages}: {ocrItem.StartTime} - {ocrItem.EndTime}"
            });

            if (cancellationToken.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }
        }

        var ocrProgress = new Progress<PaddleOcrBatchProgress>(p =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            lock (BatchLock)
            {
                var number = p.Index;
                if (!selectedIndices.Contains(number))
                {
                    return;
                }

                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                ProgressValue = number / (double)OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, number + 1, OcrSubtitleItems.Count);

                var scrollToIndex = number;
                var item = p.Item;
                if (item == null)
                {
                    item = OcrSubtitleItems[p.Index];
                }

                item.Text = p.Text;
                OcrFixLineAndSetText(number, item);
            }
        });

        _ = Task.Run(() =>
        {
            ocrEngine.OcrBatch(batchImages, language, ocrProgress, cancellationToken);
            IsOcrRunning = false;
        });
    }

    private void RunGoogleLensOcrSharp(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var numberOfImages = selectedIndices.Count;
        var ocrEngine = new GoogleLensOcrSharp(new Lens());
        var language = SelectedGoogleLensLanguage?.Code ?? "en";
        Se.Settings.Ocr.PaddleOcrLastLanguage = language;

        var batchImages = new List<PaddleOcrBatchInput>(numberOfImages);
        var count = 0;
        ProgressText = $"Initializing Google Lens OCR...";
        foreach (var i in selectedIndices)
        {
            count++;
            var ocrItem = OcrSubtitleItems[i];
            batchImages.Add(new PaddleOcrBatchInput
            {
                Bitmap = ocrItem.GetSkBitmap(),
                Index = i,
                Text = $"{count} / {numberOfImages}: {ocrItem.StartTime} - {ocrItem.EndTime}"
            });

            if (cancellationToken.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }
        }

        var ocrProgress = new Progress<PaddleOcrBatchProgress>(p =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            lock (BatchLock)
            {
                var number = p.Index;
                if (!selectedIndices.Contains(number))
                {
                    return;
                }

                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                ProgressValue = number / (double)OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, number + 1, OcrSubtitleItems.Count);

                var scrollToIndex = number;
                var item = p.Item;
                if (item == null)
                {
                    item = OcrSubtitleItems[p.Index];
                }

                item.Text = p.Text;
                OcrFixLineAndSetText(number, item);
            }
        });

        _ = Task.Run(async () =>
        {
            await ocrEngine.OcrBatch(batchImages, language, ocrProgress, cancellationToken);
            IsOcrRunning = false;
        });
    }

    private void RunNOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        if (!InitNOcrDb())
        {
            return;
        }

        _skipOnceChars.Clear();
        _ = Task.Run(() =>
        {
            using var _ = RunNOcrLoop(selectedIndices, cancellationToken);
        });
    }

    private async Task RunNOcrLoop(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        foreach (var i in selectedIndices)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }

            ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
            ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

            var item = OcrSubtitleItems[i];
            var bitmap = item.GetSkBitmap();
            var parentBitmap = new NikseBitmap2(bitmap);
            parentBitmap.MakeTwoColor(200);
            parentBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
            var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parentBitmap, SelectedNOcrPixelsAreSpace,
                false, true, 20, true);
            SelectedOcrSubtitleItem = item;
            int index = 0;
            var matches = new List<NOcrChar>();
            while (index < letters.Count)
            {
                var splitterItem = letters[index];
                if (splitterItem.NikseBitmap == null)
                {
                    if (splitterItem.SpecialCharacter != null)
                    {
                        matches.Add(new NOcrChar { Text = splitterItem.SpecialCharacter, ImageSplitterItem = splitterItem });
                    }
                }
                else
                {
                    var match = _nOcrDb!.GetMatch(parentBitmap, letters, splitterItem, splitterItem.Top, true,
                        SelectedNOcrMaxWrongPixels);

                    if (NOcrDrawUnknownText && match == null)
                    {
                        var letterIndex = letters.IndexOf(splitterItem);

                        if (_skipOnceChars.Any(p => p.LetterIndex == letterIndex && p.LineIndex == i))
                        {
                            matches.Add(new NOcrChar { Text = "*", ImageSplitterItem = splitterItem });
                            index++;
                            continue;
                        }

                        var runOnceChar =
                            _runOnceChars.FirstOrDefault(p => p.LetterIndex == letterIndex && p.LineIndex == i);
                        if (runOnceChar != null)
                        {
                            matches.Add(new NOcrChar { Text = runOnceChar.Text, ImageSplitterItem = splitterItem });
                            _runOnceChars.Clear();
                            index++;
                            continue;
                        }

                        Dispatcher.UIThread.Post(async void () =>
                        {
                            var result =
                                await _windowService.ShowDialogAsync<NOcrCharacterAddWindow, NOcrCharacterAddViewModel>(
                                    Window!,
                                    vm =>
                                    {
                                        vm.Initialize(parentBitmap, item, letters, letterIndex, _nOcrDb,
                                            SelectedNOcrMaxWrongPixels, _nOcrAddHistoryManager, true, true);
                                    });

                            _isCtrlDown = false;

                            if (result.OkPressed)
                            {
                                var letterBitmap = letters[letterIndex].NikseBitmap;
                                _nOcrAddHistoryManager.Add(result.NOcrChar, letterBitmap,
                                    OcrSubtitleItems.IndexOf(item));
                                IsInspectAdditionsVisible = true;
                                _nOcrDb.Add(result.NOcrChar);
                                _ = Task.Run(() => _nOcrDb.Save());
                                _ = Task.Run(() => RunNOcrLoop(selectedIndices.Where(p => p >= i).ToList(), cancellationToken));
                            }
                            else if (result.AbortPressed)
                            {
                                IsOcrRunning = false;
                            }
                            else if (result.UseOncePressed)
                            {
                                _runOnceChars.Add(new SkipOnceChar(i, letterIndex, result.NewText));
                                _ = Task.Run(() => RunNOcrLoop(selectedIndices.Where(p => p >= i).ToList(), cancellationToken));
                            }
                            else if (result.SkipPressed)
                            {
                                _skipOnceChars.Add(new SkipOnceChar(i, letterIndex));
                                _ = Task.Run(() => RunNOcrLoop(selectedIndices.Where(p => p >= i).ToList(), cancellationToken));
                            }
                            else if (result.InspectHistoryPressed)
                            {
                                IsOcrRunning = false;
                                await _windowService
                                    .ShowDialogAsync<NOcrCharacterHistoryWindow, NOcrCharacterHistoryViewModel>(Window!,
                                        vm => { vm.Initialize(_nOcrDb!, _nOcrAddHistoryManager); });

                                _isCtrlDown = false;
                            }
                        });

                        return;
                    }

                    if (match is { ExpandCount: > 0 })
                    {
                        index += match.ExpandCount - 1;
                    }

                    if (match == null)
                    {
                        matches.Add(new NOcrChar { Text = "*", ImageSplitterItem = splitterItem });
                    }
                    else
                    {
                        matches.Add(new NOcrChar { Text = _nOcrCaseFixer.FixUppercaseLowercaseIssues(splitterItem, match), Italic = match.Italic, ImageSplitterItem = splitterItem });
                    }
                }

                index++;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }

            matches = RemoveSpacesAfter1(matches, SelectedNOcrPixelsAreSpace);

            item.Text = ItalicTextMerger.MergeWithItalicTags(matches).Trim();
            var ocrFixResultTemp = OcrFixLine(i, item);
            if (ocrFixResultTemp.UnknownWords.Count > 0 && item.Text.Contains("<i>", StringComparison.Ordinal))
            {
                var unItalicFactor = 0.33;
                var text = GetTextWithMoreSpacesInItalic(ocrFixResultTemp.UnknownWords, matches, letters, parentBitmap, unItalicFactor, SelectedNOcrPixelsAreSpace);
                var unItalicItem = new OcrSubtitleItem(item, text);
                var unItalicResultTemp = OcrFixLine(i, unItalicItem);
                if (ocrFixResultTemp.UnknownWords.Count > unItalicResultTemp.UnknownWords.Count)
                {
                    item.Text = unItalicItem.Text;
                    item.FixResult = unItalicItem.FixResult;
                    ocrFixResultTemp = unItalicResultTemp;
                }
            }

            SetText(i, item, ocrFixResultTemp);

            _runOnceChars.Clear();
            _skipOnceChars.Clear();

            if (DoPromptForUnknownWords && ocrFixResultTemp.UnknownWords.Count > 0)
            {
                var tcs = new TaskCompletionSource<bool>();
                Dispatcher.UIThread.Post(async () =>
                {
                    foreach (var unknownWord in ocrFixResultTemp.UnknownWords)
                    {
                        var suggestions = _ocrFixEngine.GetSpellCheckSuggestions(unknownWord.Word.FixedWord);
                        var result = await _windowService.ShowDialogAsync<PromptUnknownWordWindow, PromptUnknownWordViewModel>(Window!,
                            vm => { vm.Initialize(item.GetBitmap(), item.Text, unknownWord, suggestions); });


                        if (result.ChangeWholeTextPressed)
                        {
                            item.Text = result.WholeText;
                            break;
                        }
                        else if (result.ChangeOncePressed)
                        {
                            ChangeWord(item, unknownWord, result.Word);
                        }
                        else if (result.ChangeAllPressed)
                        {
                            _ocrFixEngine.ChangeAll(unknownWord.Word.Word, result.Word);
                        }
                        else if (result.SkipOncePressed)
                        {
                            // do nothing
                        }
                        else if (result.SkipAllPressed)
                        {
                            _ocrFixEngine.SkipAll(unknownWord.Word.Word);
                        }
                        else if (result.AddToNamesListPressed)
                        {
                            _ocrFixEngine.AddName(unknownWord.Word.Word);
                        }
                        else if (result.AddToUserDictionaryPressed)
                        {
                            if (SelectedDictionary != null)
                            {
                                UserWordsHelper.AddToUserDictionary(unknownWord.Word.Word, SelectedDictionary.GetFiveLetterLanguageName() ?? "en_US");
                            }
                        }
                        else
                        {
                            _cancellationTokenSource.Cancel();
                            IsOcrRunning = false;
                            break;
                        }
                    }

                    tcs.SetResult(true);
                });
                await tcs.Task;
                if (!IsOcrRunning)
                {
                    _isCtrlDown = false;
                    return;
                }
            }
        }

        _isCtrlDown = false;
        IsOcrRunning = false;
    }

    private List<NOcrChar> RemoveSpacesAfter1(List<NOcrChar> matches, int pixelsAreSpace)
    {
        var deleteItems = new List<NOcrChar>();
        for (int i = 0; i < matches.Count - 1; i++)
        {
            var match = matches[i];
            if (match.Text.EndsWith("1", StringComparison.Ordinal) && !match.Italic)
            {
                var pixelsLess = 0;
                if (pixelsAreSpace > 7)
                {
                    pixelsLess = (int)Math.Round(pixelsAreSpace * 0.3m, MidpointRounding.AwayFromZero);
                }

                var nextMatch = matches[i + 1];
                if (nextMatch.ImageSplitterItem != null &&
                    nextMatch.ImageSplitterItem.SpecialCharacter == " " &&
                    nextMatch.ImageSplitterItem.SpacePixels - pixelsLess < pixelsAreSpace)
                {
                    deleteItems.Add(nextMatch);
                }
            }
        }

        foreach (var deleteItem in deleteItems)
        {
            matches.Remove(deleteItem);
        }

        return matches;
    }

    private static string GetTextWithMoreSpacesInItalic(
        List<UnknownWordItem> unknownWords,
        List<NOcrChar> matches,
        List<ImageSplitterItem2> letters,
        NikseBitmap2 parentBitmap,
        double unItalicFactor,
        int pixelsIsSpace)
    {
        // Clear all CouldBeSpaceBefore flags
        foreach (var letter in letters)
        {
            letter.CouldBeSpaceBefore = false;
        }

        // Check for potential spaces in italic text
        for (int i = 0; i < matches.Count - 1; i++)
        {
            var match = matches[i];
            var matchNext = matches[i + 1];
            if (!match.Italic || matchNext.Text == "," ||
                string.IsNullOrWhiteSpace(match.Text) || string.IsNullOrWhiteSpace(matchNext.Text) ||
                match.ImageSplitterItem == null || matchNext.ImageSplitterItem == null)
            {
                continue;
            }

            int blankVerticalLines = IsVerticalAngledLineTransparent(parentBitmap, match.ImageSplitterItem, matchNext.ImageSplitterItem, unItalicFactor);
            if (match.Text == "f" || match.Text == "," || matchNext.Text.StartsWith('y') || matchNext.Text.StartsWith('j'))
            {
                blankVerticalLines++;
            }

            if (blankVerticalLines >= pixelsIsSpace)
            {
                matchNext.ImageSplitterItem.CouldBeSpaceBefore = true;
            }
        }

        // Insert spaces where CouldBeSpaceBefore is true and previous match is italic
        int j = 1;
        while (j < matches.Count)
        {
            var match = matches[j];
            var prevMatch = matches[j - 1];
            if (match.ImageSplitterItem?.CouldBeSpaceBefore == true)
            {
                match.ImageSplitterItem.CouldBeSpaceBefore = false;
                if (prevMatch.Italic)
                {
                    matches.Insert(j, new NOcrChar(" "));
                    j++; // Skip the inserted space
                }
            }

            j++;
        }

        return ItalicTextMerger.MergeWithItalicTags(matches).Trim();
    }

    private static int IsVerticalAngledLineTransparent(NikseBitmap2 parentBitmap, ImageSplitterItem2 match, ImageSplitterItem2 next, double unItalicFactor)
    {
        if (match.NikseBitmap == null || next.NikseBitmap == null)
        {
            return 0;
        }

        int blanks = 0;
        var min = match.X + match.NikseBitmap.Width;
        var max = next.X + next.NikseBitmap.Width / 2;
        for (int startX = min; startX < max; startX++)
        {
            var lineBlank = true;
            for (int y = match.Y; y < match.Y + match.NikseBitmap.Height; y++)
            {
                var x = startX - (y - match.Y) * unItalicFactor;
                if (x >= 0 && x < parentBitmap.Width && y < parentBitmap.Height)
                {
                    var color = parentBitmap.GetPixel((int)Math.Round(x), y);
                    if (color.Alpha != 0)
                    {
                        lineBlank = false;
                        if (blanks > 0)
                        {
                            return blanks;
                        }
                    }
                }
            }

            if (lineBlank)
            {
                blanks++;
            }
        }

        return blanks;
    }

    private void RunBinaryImageCompareOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var db = InitImageComparOcrDb();
        if (db == null)
        {
            return;
        }

        _skipOnceChars.Clear();
        _ = Task.Run(() =>
        {
            using var _ = RunBinaryImageCompareOcrLoop(db, selectedIndices, cancellationToken);
        });
    }

    private BinaryOcrDb? InitImageComparOcrDb()
    {
        if (SelectedImageCompareDatabase == null)
        {
            return null;
        }

        var fileName = Path.Combine(Se.OcrFolder, SelectedImageCompareDatabase + BinaryOcrDb.Extension);
        if (!File.Exists(fileName))
        {
            return null;
        }

        _binaryOcrMatcher.IsLatinDb = SelectedImageCompareDatabase.Contains("Latin", StringComparison.OrdinalIgnoreCase);
        return new BinaryOcrDb(fileName, true);
    }

    private async Task RunBinaryImageCompareOcrLoop(BinaryOcrDb db, List<int> selectedIndices, CancellationToken cancellationToken)
    {
        foreach (var i in selectedIndices)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                IsOcrRunning = false;
                return;
            }

            ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
            ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

            var item = OcrSubtitleItems[i];
            var bitmap = item.GetSkBitmap();
            var parentBitmap = new NikseBitmap2(bitmap);
            parentBitmap.MakeTwoColor(200);
            parentBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
            var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parentBitmap, SelectedBinaryOcrPixelsAreSpace, false, true, 20, true);
            SelectedOcrSubtitleItem = item;
            int index = 0;
            var matches = new List<BinaryOcrMatcher.CompareMatch>();
            while (index < letters.Count)
            {
                var splitterItem = letters[index];
                if (splitterItem.NikseBitmap == null)
                {
                    if (splitterItem.SpecialCharacter != null)
                    {
                        // space or special character
                        matches.Add(new BinaryOcrMatcher.CompareMatch(splitterItem.SpecialCharacter, false, 0, nameof(splitterItem.SpecialCharacter)));
                    }
                }
                else
                {
                    var match = _binaryOcrMatcher.GetCompareMatch(splitterItem, out var secondBestMatch, letters, index, db);

                    if (match == null)
                    {
                        var letterIndex = letters.IndexOf(splitterItem);

                        if (_skipOnceChars.Any(p => p.LetterIndex == letterIndex && p.LineIndex == i))
                        {
                            matches.Add(new BinaryOcrMatcher.CompareMatch("*", false, 0, null));
                            index++;
                            continue;
                        }

                        var runOnceChar =
                            _runOnceChars.FirstOrDefault(p => p.LetterIndex == letterIndex && p.LineIndex == i);
                        if (runOnceChar != null)
                        {
                            matches.Add(new BinaryOcrMatcher.CompareMatch(runOnceChar.Text, false, 0, null));
                            _runOnceChars.Clear();
                            index++;
                            continue;
                        }

                        Dispatcher.UIThread.Post(async void () =>
                        {
                            var result =
                                await _windowService.ShowDialogAsync<BinaryOcrCharacterAddWindow, BinaryOcrCharacterAddViewModel>(
                                    Window!,
                                    vm =>
                                    {
                                        vm.Initialize(parentBitmap, item, letters, letterIndex, db,
                                            SelectedNOcrMaxWrongPixels, _binaryOcrAddHistoryManager, true, true);
                                    });

                            if (result.OkPressed)
                            {
                                if (result.BinaryOcrBitmap != null)
                                {
                                    var letterBitmap = letters[letterIndex].NikseBitmap;
                                    _binaryOcrAddHistoryManager.Add(result.BinaryOcrBitmap, letterBitmap,
                                        OcrSubtitleItems.IndexOf(item));
                                    IsInspectAdditionsVisible = true;

                                    if (result.FirstBinaryOcrBitmap != null)
                                    {
                                        result.BinaryOcrBitmap.Width = result.FirstBinaryOcrBitmap.Width;
                                        result.BinaryOcrBitmap.Height = result.FirstBinaryOcrBitmap.Height;
                                        result.BinaryOcrBitmap.NumberOfColoredPixels = result.FirstBinaryOcrBitmap.NumberOfColoredPixels;
                                        result.BinaryOcrBitmap.Hash = result.FirstBinaryOcrBitmap.Hash;
                                        result.BinaryOcrBitmap.Colors = result.FirstBinaryOcrBitmap.Colors;
                                    }

                                    db.Add(result.BinaryOcrBitmap);
                                    _ = Task.Run(() => db.Save());
                                }

                                _ = Task.Run(() => RunBinaryImageCompareOcrLoop(db, selectedIndices.Where(p => p >= i).ToList(), cancellationToken));
                            }
                            else if (result.AbortPressed)
                            {
                                IsOcrRunning = false;
                            }
                            else if (result.UseOncePressed)
                            {
                                _runOnceChars.Add(new SkipOnceChar(i, letterIndex, result.NewText));
                                _ = Task.Run(() => RunBinaryImageCompareOcrLoop(db, selectedIndices.Where(p => p >= i).ToList(), cancellationToken));
                            }
                            else if (result.SkipPressed)
                            {
                                _skipOnceChars.Add(new SkipOnceChar(i, letterIndex));
                                _ = Task.Run(() => RunBinaryImageCompareOcrLoop(db, selectedIndices.Where(p => p >= i).ToList(), cancellationToken));
                            }
                            else if (result.InspectHistoryPressed)
                            {
                                IsOcrRunning = false;
                                await _windowService
                                    .ShowDialogAsync<BinaryOcrCharacterHistoryWindow, BinaryOcrCharacterHistoryViewModel>(Window!,
                                        vm => { vm.Initialize(db, _binaryOcrAddHistoryManager); });
                            }
                        });
                        return;
                    }

                    if (match is { ExpandCount: > 0 })
                    {
                        index += match.ExpandCount - 1;
                    }

                    if (match == null)
                    {
                        matches.Add(new BinaryOcrMatcher.CompareMatch("*", false, 0, null));
                    }
                    else
                    {
                        matches.Add(new BinaryOcrMatcher.CompareMatch(match.Text, match.Italic, match.ExpandCount, match.Name));
                    }
                }

                index++;
            }

            // item.Text = ItalicTextMerger.MergeWithItalicTags(matches).Trim();
            item.Text = matches.Aggregate(string.Empty, (current, match) => current + match.Text).Trim();
            var unknownWords = OcrFixLineAndSetText(i, item);

            _runOnceChars.Clear();
            _skipOnceChars.Clear();

            if (DoPromptForUnknownWords && unknownWords.Count > 0)
            {
                var tcs = new TaskCompletionSource<bool>();
                Dispatcher.UIThread.Post(async () =>
                {
                    foreach (var unknownWord in unknownWords)
                    {
                        var suggestions = _ocrFixEngine.GetSpellCheckSuggestions(unknownWord.Word.FixedWord);
                        var result = await _windowService.ShowDialogAsync<PromptUnknownWordWindow, PromptUnknownWordViewModel>(Window!,
                            vm => { vm.Initialize(item.GetBitmap(), item.Text, unknownWord, suggestions); });

                        if (result.ChangeWholeTextPressed)
                        {
                            item.Text = result.WholeText;
                            break;
                        }
                        else if (result.ChangeOncePressed)
                        {
                            ChangeWord(item, unknownWord, result.Word);
                        }
                        else if (result.ChangeAllPressed)
                        {
                            _ocrFixEngine.ChangeAll(unknownWord.Word.Word, result.Word);
                        }
                        else if (result.SkipOncePressed)
                        {
                            // do nothing
                        }
                        else if (result.SkipAllPressed)
                        {
                            _ocrFixEngine.SkipAll(unknownWord.Word.Word);
                        }
                        else if (result.AddToNamesListPressed)
                        {
                            _ocrFixEngine.AddName(unknownWord.Word.Word);
                        }
                        else if (result.AddToUserDictionaryPressed)
                        {
                            if (SelectedDictionary != null)
                            {
                                UserWordsHelper.AddToUserDictionary(unknownWord.Word.Word, SelectedDictionary.GetFiveLetterLanguageName() ?? "en_US");
                            }
                        }
                        else
                        {
                            _cancellationTokenSource.Cancel();
                            IsOcrRunning = false;
                            break;
                        }
                    }

                    tcs.SetResult(true);
                });
                await tcs.Task;
                if (!IsOcrRunning)
                {
                    return;
                }
            }
        }

        IsOcrRunning = false;
    }

    private static void ChangeWord(OcrSubtitleItem item, UnknownWordItem unknownWord, string word)
    {
        if (unknownWord.Word.FixedWord == word)
        {
            return;
        }

        var idx = unknownWord.Word.WordIndex;
        if (item.Text.Substring(idx).StartsWith(unknownWord.Word.FixedWord))
        {
            item.Text = item.Text.Remove(idx, unknownWord.Word.FixedWord.Length).Insert(idx, word);
        }
    }


    public class OcrFixLineResultTemp
    {
        public List<UnknownWordItem> UnknownWords { get; set; } = new List<UnknownWordItem>();
        public List<ReplacementUsedItem> Fixes { get; set; } = new List<ReplacementUsedItem>();
        public List<GuessUsedItem> Guesses { get; set; } = new List<GuessUsedItem>();
        public string ResultText { get; set; } = string.Empty;
        public OcrFixLineResult OcrFixLineResult { get; set; } = new OcrFixLineResult();
    }

    private OcrFixLineResultTemp OcrFixLine(int i, OcrSubtitleItem item)
    {
        var result = new OcrFixLineResultTemp();

        if (DoAutoBreak)
        {
            item.Text = Utilities.AutoBreakLine(item.Text);
        }

        if (SelectedDictionary != null &&
            SelectedDictionary.Name != GetDictionaryNameNone() &&
            _ocrFixEngine.IsLoaded() && DoFixOcrErrors)
        {
            result.OcrFixLineResult = _ocrFixEngine.FixOcrErrors(i, item, DoTryToGuessUnknownWords);
            var alignment = GetAlignment(item);
            if (!string.IsNullOrEmpty(alignment))
            {
                result.OcrFixLineResult.Words.Insert(0, new OcrFixLinePartResult { Word = alignment, IsSpellCheckedOk = null });
            }

            result.ResultText = result.OcrFixLineResult.GetText();

            if (!string.IsNullOrEmpty(result.OcrFixLineResult.ReplacementUsed.From))
            {
                result.Fixes.Add(result.OcrFixLineResult.ReplacementUsed);
            }

            foreach (var word in result.OcrFixLineResult.Words)
            {
                if (!string.IsNullOrEmpty(word.ReplacementUsed.From))
                {
                    result.Fixes.Add(word.ReplacementUsed);
                }

                if (word.GuessUsed)
                {
                    result.Guesses.Add(new GuessUsedItem(word.Word, word.FixedWord, i));
                }

                if (word.IsSpellCheckedOk == false)
                {
                    var unknownWordItem = new UnknownWordItem(item, result.OcrFixLineResult, word);
                    result.UnknownWords.Add(unknownWordItem);
                }
            }
        }

        return result;
    }

    private void SetText(int i, OcrSubtitleItem item, OcrFixLineResultTemp resultTemp)
    {
        if (SelectedDictionary != null &&
            SelectedDictionary.Name != GetDictionaryNameNone() &&
            _ocrFixEngine.IsLoaded() && DoFixOcrErrors)
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentText = resultTemp.ResultText;
                item.Text = resultTemp.ResultText;
                item.FixResult = resultTemp.OcrFixLineResult;
            });
        }
        else
        {
            var alignment = GetAlignment(item);
            Dispatcher.UIThread.Post(() =>
            {
                item.Text = alignment + item.Text;
                CurrentText = item.Text;
                item.FixResult = new OcrFixLineResult
                {
                    LineIndex = i,
                    Words = new List<OcrFixLinePartResult> { new() { Word = item.Text, IsSpellCheckedOk = null } },
                };
            });
        }

        SelectAndScrollToRow(i);

        foreach (var unknownWord in resultTemp.UnknownWords)
        {
            UnknownWords.Add(unknownWord);
        }
        foreach (var guess in resultTemp.Guesses)
        {
            AllGuesses.Add(guess);
        }
        foreach (var fix in resultTemp.Fixes)
        {
            AllFixes.Add(fix);
        }
    }

    private List<UnknownWordItem> OcrFixLineAndSetText(int i, OcrSubtitleItem item)
    {
        if (DoAutoBreak)
        {
            item.Text = Utilities.AutoBreakLine(item.Text);
        }

        var unknownWords = new List<UnknownWordItem>();
        if (SelectedDictionary != null &&
            SelectedDictionary.Name != GetDictionaryNameNone() &&
            _ocrFixEngine.IsLoaded() && DoFixOcrErrors)
        {
            var result = _ocrFixEngine.FixOcrErrors(i, item, DoTryToGuessUnknownWords);
            var alignment = GetAlignment(item);
            if (!string.IsNullOrEmpty(alignment))
            {
                result.Words.Insert(0, new OcrFixLinePartResult { Word = alignment, IsSpellCheckedOk = null });
            }

            var resultText = result.GetText();

            Dispatcher.UIThread.Post(() =>
            {
                CurrentText = resultText;
                item.Text = resultText;
                item.FixResult = result;
            });

            if (!string.IsNullOrEmpty(result.ReplacementUsed.From))
            {
                AllFixes.Add(result.ReplacementUsed);
            }

            foreach (var word in result.Words)
            {
                if (!string.IsNullOrEmpty(word.ReplacementUsed.From))
                {
                    AllFixes.Add(word.ReplacementUsed);
                }

                if (word.GuessUsed)
                {
                    AllGuesses.Add(new GuessUsedItem(word.Word, word.FixedWord, i));
                }

                if (word.IsSpellCheckedOk == false)
                {
                    var unknownWordItem = new UnknownWordItem(item, result, word);
                    UnknownWords.Add(unknownWordItem);
                    unknownWords.Add(unknownWordItem);
                }
            }
        }
        else
        {
            var alignment = GetAlignment(item);
            Dispatcher.UIThread.Post(() =>
            {
                item.Text = alignment + item.Text;
                CurrentText = item.Text;
                item.FixResult = new OcrFixLineResult
                {
                    LineIndex = i,
                    Words = new List<OcrFixLinePartResult> { new() { Word = item.Text, IsSpellCheckedOk = null } },
                };
            });
        }

        SelectAndScrollToRow(i);
        return unknownWords;
    }

    private string GetAlignment(OcrSubtitleItem item)
    {
        if (HasCaptureTopAlign)
        {
            var bitmap = item.GetSkBitmap();
            var height = bitmap.Height;
            var top = item.GetPosition().Y + height / 2;
            var screenHeight = item.GetScreenSize().Height;
            if (top < screenHeight * 0.4)
            {
                return "{\\an8}";
            }
        }

        return string.Empty;
    }

    private bool InitNOcrDb()
    {
        var fileName = GetNOcrLanguageFileName();
        if (_nOcrDb != null && _nOcrDb.FileName == fileName)
        {
            return true;
        }

        if (fileName == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(fileName) && (_nOcrDb == null || _nOcrDb.FileName != fileName))
        {
            _nOcrDb = new NOcrDb(fileName);
        }

        return true;
    }

    private void RunTesseractOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var tesseractOcr = new TesseractOcr();
        var language = SelectedTesseractDictionaryItem?.Code ?? "eng";

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                var text = await tesseractOcr.Ocr(bitmap, language, cancellationToken);
                item.Text = text;

                OcrFixLineAndSetText(i, item);
            }

            PauseOcr();
        });
    }

    private void RunOllamaOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var ollamaOcr = new OllamaOcr();

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                SelectAndScrollToRow(i);

                var text = await ollamaOcr.Ocr(bitmap, OllamaUrl, OllamaModel, SelectedOllamaLanguage ?? "English", cancellationToken);
                item.Text = text;

                OcrFixLineAndSetText(i, item);
            }

            PauseOcr();
        });
    }

    private void RunMistralOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var mistralOcr = new MistralOcr(MistralApiKey);

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                SelectAndScrollToRow(i);

                var text = await mistralOcr.Ocr(bitmap, SelectedOllamaLanguage ?? "English", cancellationToken);
                item.Text = text;

                OcrFixLineAndSetText(i, item);
            }

            PauseOcr();
        });
    }

    private void RunGoogleVisionOcr(List<int> selectedIndices, CancellationToken cancellationToken)
    {
        var engine = new GoogleVisionOcr();

        _ = Task.Run(async () =>
        {
            foreach (var i in selectedIndices)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ProgressValue = i * 100.0 / OcrSubtitleItems.Count;
                ProgressText = string.Format(Se.Language.Ocr.RunningOcrDotDotDotXY, i + 1, OcrSubtitleItems.Count);

                var item = OcrSubtitleItems[i];
                var bitmap = item.GetSkBitmap();

                SelectAndScrollToRow(i);

                var text = await engine.Ocr(bitmap, GoogleVisionApiKey, SelectedGoogleVisionLanguage?.Code ?? "en", cancellationToken);
                item.Text = text;

                OcrFixLineAndSetText(i, item);
            }

            PauseOcr();
        });
    }

    private async Task<bool> CheckAndDownloadTesseract()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var tesseractExe = Path.Combine(Se.TesseractFolder, "tesseract.exe");
            if (File.Exists(tesseractExe))
            {
                return true;
            }

            var answer = await MessageBox.Show(
                Window!,
                "Download Tesseract OCR?",
                $"{Environment.NewLine}\"Tesseract\" requires downloading Tesseract OCR.{Environment.NewLine}{Environment.NewLine}Download and use Tesseract OCR?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await _windowService.ShowDialogAsync<DownloadTesseractWindow, DownloadTesseractViewModel>(Window!);

            return File.Exists(tesseractExe);
        }

        try
        {
            var fileName = TesseractOcr.GetExecutablePath();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit(2000); // Wait max 2 seconds

            if (process.ExitCode == 0)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await MessageBox.Show(
                Window!,
                "Please install Tesseract",
                $"{Environment.NewLine}\"Tesseract\" was not detected. Please install Tesseract." +
                Environment.NewLine + "" +
                "E.g. ´brew install tesseract´.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await MessageBox.Show(
                Window!,
                "Please install Tesseract",
                $"{Environment.NewLine}\"Tesseract\" was not detected. Please install Tesseract." +
                Environment.NewLine +
                $"E.g. ´sudo apt install tesseract-ocr´ or ´sudo pacman -S tesseract´.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        return false;
    }

    private async Task<bool> TesseractModelDownload()
    {
        var result =
            await _windowService.ShowDialogAsync<DownloadTesseractModelWindow, DownloadTesseractModelViewModel>(Window!);

        LoadActiveTesseractDictionaries();
        if (result.OkPressed)
        {
            var item = TesseractDictionaryItems.FirstOrDefault(p =>
                p.Code == result.SelectedTesseractDictionaryItem?.Code);
            SelectedTesseractDictionaryItem = item ?? TesseractDictionaryItems.FirstOrDefault();
            return true;
        }

        return false;
    }

    private void LoadActiveTesseractDictionaries()
    {
        TesseractDictionaryItems.Clear();

        var folder = Se.TesseractModelFolder;
        if (!Directory.Exists(folder))
        {
            return;
        }

        var allDictionaries = TesseractDictionary.List();
        var items = new List<TesseractDictionary>();
        foreach (var file in Directory.GetFiles(folder, "*.traineddata"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name == "osd")
            {
                continue;
            }

            var dictionary = allDictionaries.FirstOrDefault(p => p.Code == name);
            if (dictionary != null)
            {
                items.Add(dictionary);
            }
            else
            {
                items.Add(new TesseractDictionary { Code = name, Name = name, Url = string.Empty });
            }
        }

        TesseractDictionaryItems.AddRange(items.OrderBy(p => p.ToString()));
    }

    internal void SubtitleGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.I && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ToggleItalic();
            e.Handled = true; // prevent further handling if needed
        }
        else if (e.Key == Key.P && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true; // prevent further handling if needed
            Dispatcher.UIThread.Post(async void () => { await ViewSelectedImage(); });
        }
        else if (e.Key == Key.Delete)
        {
            e.Handled = true; // prevent further handling if needed
            DeleteSelectedLines();
        }
        else if (e.Key == Key.Home)
        {
            e.Handled = true; // prevent further handling if needed
            SelectAndScrollToRow(0);
        }
        else if (e.Key == Key.End)
        {
            e.Handled = true; // prevent further handling if needed
            DeleteSelectedLines();
            SelectAndScrollToRow(OcrSubtitleItems.Count - 1);
        }
    }

    internal void SubtitleGridDoubleTapped()
    {
        var engine = SelectedOcrEngine;
        if (engine == null)
        {
            return;
        }

        if (engine != null && engine.EngineType == OcrEngineType.nOcr)
        {
            Dispatcher.UIThread.Post(async void () => { await InspectLine(); });
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (e.Key == Key.Escape)
        {
            Cancel();
        }
        else if (e.Key == Key.G && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true; // prevent further handling if needed
            Dispatcher.UIThread.Post(async void () => { await ShowGoToLine(); });
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/ocr");
        }
    }

    internal void DataGridTracksSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool flowControl = TrackChanged();
        if (!flowControl)
        {
            return;
        }
    }

    private bool TrackChanged()
    {
        return true;
    }

    public static Bitmap ConvertSkBitmapToAvaloniaBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());

        return new Bitmap(stream);
    }

    internal async void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= OcrSubtitleItems.Count)
        {
            return;
        }

        lock (_scrollLock)
        {
            _pendingScrollIndex = index;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            int indexToScroll;
            lock (_scrollLock)
            {
                indexToScroll = _pendingScrollIndex;
                _pendingScrollIndex = -1;
            }

            // Only execute if this is the latest scroll request
            if (indexToScroll >= 0 && indexToScroll < OcrSubtitleItems.Count)
            {
                SelectedOcrSubtitleItem = OcrSubtitleItems[indexToScroll];
                SubtitleGrid.SelectedIndex = indexToScroll;
                SubtitleGrid.ScrollIntoView(OcrSubtitleItems[indexToScroll], null);
            }
        }, DispatcherPriority.Background);
    }

    public void Initialize(List<BluRaySupParser.PcsData> subtitles, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleBluRay(subtitles);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(List<VobSubMergedPack> vobSubMergedPackList, List<SKColor> palette, string vobSubFileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, vobSubFileName);
        _ocrSubtitle = new OcrSubtitleVobSub(vobSubMergedPackList, palette);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(Trak mp4SubtitleTrack, List<Paragraph> paragraphs, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleMp4VobSub(mp4SubtitleTrack, paragraphs);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(List<VobSubMergedPack> mergedVobSubPacks, List<SKColor> palette, MatroskaTrackInfo matroskaSubtitleInfo, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleVobSub(mergedVobSubPacks, palette);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(MatroskaTrackInfo matroskaSubtitleInfo, Subtitle subtitle, List<DvbSubPes> subtitleImages, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleMkvDvb(matroskaSubtitleInfo, subtitle, subtitleImages);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(MatroskaTrackInfo matroskaSubtitleInfo, List<BluRaySupParser.PcsData> pcsDataList, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleMkvBluRay(matroskaSubtitleInfo, pcsDataList);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void Initialize(IList<IBinaryParagraphWithPosition> list, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleIBinaryParagraph(list);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void InitializeBdn(Subtitle subtitle, string fileName, bool isSon)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleBdn(subtitle, fileName, isSon);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    public void InitializeWebVtt(Subtitle subtitle, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleWebVttImages(subtitle, fileName);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void Initialize(TransportStreamParser tsParser, List<TransportStreamSubtitle> subtitles, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, fileName);
        _ocrSubtitle = new OcrSubtitleTransportStream(tsParser, subtitles, fileName);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void Initialize(List<ImportImageItem> images)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, Se.Language.General.Images);
        _ocrSubtitle = new OcrImportImage(images);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void InitializeDivX(List<XSub> list, string fileName)
    {
        Title = string.Format(Se.Language.Ocr.OcrX, "DivX");
        _ocrSubtitle = new OcrSubtitleDivX(list, fileName);
        OcrSubtitleItems = new ObservableCollection<OcrSubtitleItem>(_ocrSubtitle.MakeOcrSubtitleItems());
    }

    internal void EngineSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        EngineSelectionChanged();
    }

    private void EngineSelectionChanged()
    {
        if (SelectedOcrEngine == null)
        {
            SelectedOcrEngine = OcrEngines.FirstOrDefault();
        }

        var et = SelectedOcrEngine?.EngineType;
        IsNOcrVisible = et == OcrEngineType.nOcr;
        IsInspectLineVisible = et == OcrEngineType.nOcr || et == OcrEngineType.BinaryImageCompare;
        IsOllamaVisible = et == OcrEngineType.Ollama;
        IsTesseractVisible = et == OcrEngineType.Tesseract;
        IsPaddleOcrVisible = et == OcrEngineType.PaddleOcrStandalone || et == OcrEngineType.PaddleOcrPython;
        IsGoogleVisionVisible = et == OcrEngineType.GoogleVision;
        IsGoogleLensVisible = et == OcrEngineType.GoogleLens || et == OcrEngineType.GoogleLensSharp;
        IsMistralOcrVisible = et == OcrEngineType.Mistral;
        IsBinaryImageCompareVisible = et == OcrEngineType.BinaryImageCompare;

        if (IsNOcrVisible && NOcrDatabases.Count == 0)
        {
            foreach (var s in NOcrDb.GetDatabases().OrderBy(p => p))
            {
                NOcrDatabases.Add(s);
            }

            if (!string.IsNullOrEmpty(Se.Settings.Ocr.NOcrDatabase) &&
                NOcrDatabases.Contains(Se.Settings.Ocr.NOcrDatabase))
            {
                SelectedNOcrDatabase = Se.Settings.Ocr.NOcrDatabase;
            }

            if (SelectedNOcrDatabase == null)
            {
                SelectedNOcrDatabase = NOcrDb.GetDatabases().FirstOrDefault();
            }
        }

        if (IsTesseractVisible)
        {
            LoadActiveTesseractDictionaries();
            if (SelectedTesseractDictionaryItem == null)
            {
                SelectedTesseractDictionaryItem = TesseractDictionaryItems.FirstOrDefault(p => p.Code == "eng") ??
                                                  TesseractDictionaryItems.FirstOrDefault();
            }
        }

        if (IsPaddleOcrVisible)
        {
            if (SelectedPaddleOcrLanguage == null)
            {
                SelectedPaddleOcrLanguage = PaddleOcrLanguages.FirstOrDefault(p => p.Code == "eng") ??
                                            PaddleOcrLanguages.FirstOrDefault();
            }
        }

        if (IsGoogleVisionVisible)
        {
            if (SelectedGoogleVisionLanguage == null)
            {
                SelectedGoogleVisionLanguage = GoogleVisionLanguages.FirstOrDefault(p => p.Code == "eng") ??
                                               GoogleVisionLanguages.FirstOrDefault();
            }
        }
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        SaveSettings();
        UiUtil.SaveWindowPosition(Window);
    }

    internal void SubtitleGridContextOpening(object? sender, EventArgs e)
    {
        ShowContextMenu = OcrSubtitleItems.Count > 0;
    }

    public void DictionaryChanged()
    {
        IsDictionaryLoaded = Dictionaries.IndexOf(SelectedDictionary ?? Dictionaries.First()) > 0;
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
        DictionaryChanged();
        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.Focus();
            if (OcrSubtitleItems.Count > 0)
            {
                SelectedOcrSubtitleItem = OcrSubtitleItems[0];
                SubtitleGrid.SelectedIndex = 0;
                SubtitleGrid.ScrollIntoView(SelectedOcrSubtitleItem, null);
                TrackChanged();
            }
        });
    }

    internal void TextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        var selected = SelectedOcrSubtitleItem;
        if (selected == null)
        {
            return;
        }

        if (selected.FixResult == null)
        {
            return;
        }

        if (selected.FixResult.GetText() == selected.Text)
        {
            return;
        }

        var idx = OcrSubtitleItems.IndexOf(selected);
        selected.FixResult = new OcrFixLineResult(idx, selected.Text);
        //TODO: spell check?
    }

    private async Task ShowGoToLine()
    {
        if (OcrSubtitleItems.Count == 0)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(Window!, vm =>
        {
            var idx = 1;
            if (SelectedOcrSubtitleItem != null)
            {
                idx = OcrSubtitleItems.IndexOf(SelectedOcrSubtitleItem) + 1;
            }

            vm.Initialize(idx, OcrSubtitleItems.Count);
        });

        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber <= OcrSubtitleItems.Count)
        {
            var no = (int)viewModel.LineNumber;
            SelectAndScrollToRow(Math.Min(OcrSubtitleItems.Count - 1, no + 1));
            SelectAndScrollToRow(no - 1);
        }
    }

    internal void UnknownWordSelectionChanged()
    {
        IsUnknownWordSelected = SelectedUnknownWord != null;
        if (IsUnknownWordSelected)
        {
            UnknownWordsRemoveCurrentText = string.Format(
                Se.Language.Ocr.RemoveXFromUnknownWordsList,
                SelectedUnknownWord?.Word.FixedWord);
        }
        else
        {
            UnknownWordsRemoveCurrentText = string.Empty;
        }
    }

    internal void UnknownWordSelectionTapped()
    {
        if (SelectedUnknownWord == null)
        {
            return;
        }

        SelectAndScrollToRow(OcrSubtitleItems.IndexOf(SelectedUnknownWord.Item));
    }

    internal void AllFixesTapped()
    {
        var selection = SelectedAllFix;
        if (selection == null)
        {
            return;
        }

        SelectAndScrollToRow(selection.LineIndex);
    }

    internal void GuessUsedTapped()
    {
        var selection = SelectedAllGuess;
        if (selection == null)
        {
            return;
        }

        SelectAndScrollToRow(selection.LineIndex);
    }

    public void TextBoxPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (OperatingSystem.IsMacOS() &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            sender is Control control)
        {
            var args = new ContextRequestedEventArgs(e);
            control.RaiseEvent(args);
            e.Handled = args.Handled;
        }
    }

    internal void UnknownWordListKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true; // prevent further handling if needed
            UnknownWordSelectionTapped();
        }
    }

    public void OnWindowKeyUp(KeyEventArgs e)
    {
        _isCtrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
    }

    internal void DataGridSubtitleMacPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (OperatingSystem.IsMacOS() &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            !e.Pointer.IsPrimary &&
            sender is Control control)
        {
            var args = new ContextRequestedEventArgs(e);
            control.RaiseEvent(args);
            e.Handled = args.Handled;
        }
    }
}
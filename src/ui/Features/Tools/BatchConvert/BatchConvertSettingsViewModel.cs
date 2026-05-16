using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceFolder;
    [ObservableProperty] private bool _useOutputFolder;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private bool _overwrite;
    [ObservableProperty] private ObservableCollection<string> _targetEncodings;
    [ObservableProperty] private string? _selectedTargetEncoding;

    [ObservableProperty] private ObservableCollection<string> _ocrEngines;
    [ObservableProperty] private string? _selectedOcrEngine;

    [ObservableProperty] private ObservableCollection<string> _languagePostFixes;
    [ObservableProperty] private string? _selectedLanguagePostFix;

    [ObservableProperty] private ObservableCollection<TesseractDictionary> _tesseractDictionaryItems;
    [ObservableProperty] private TesseractDictionary? _selectedTesseractDictionaryItem;

    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleOcrLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleOcrLanguage;

    [ObservableProperty] private ObservableCollection<string> _binaryOcrDatabases;
    [ObservableProperty] private string? _selectedBinaryOcrDatabase;

    [ObservableProperty] private ObservableCollection<string> _nOcrDatabases;
    [ObservableProperty] private string? _selectedNOcrDatabase;

    [ObservableProperty] private ObservableCollection<string> _ollamaModels;
    [ObservableProperty] private string? _selectedOllamaModel;

    [ObservableProperty] bool _isOcrLanguageVisible;
    [ObservableProperty] bool _isTesseractOcrVisible;
    [ObservableProperty] bool _isPaddleOCrVisible;
    [ObservableProperty] bool _isBinaryOcrVisible;
    [ObservableProperty] bool _isNOcrVisible;
    [ObservableProperty] bool _isOllamaVisible;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    public BatchConvertSettingsViewModel(IFolderHelper folderHelper, IWindowService windowService)
    {
        var encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();
        TargetEncodings = new ObservableCollection<string>(encodings);

        OcrEngines = new ObservableCollection<string> { "nOcr", "BinaryOcr", "Tesseract", "Ollama" };
        if (!OperatingSystem.IsMacOS())
        {
            OcrEngines.Add("PaddleOCR");
        }

        SelectedOcrEngine = Se.Settings.Ocr.Engine == "nOcr" ? OcrEngines.First() : OcrEngines.Last();

        LanguagePostFixes = new ObservableCollection<string>()
        {
            Se.Language.General.NoLanguageCode,
            Se.Language.General.TwoLetterLanguageCode,
            Se.Language.General.ThreeLetterLanguageCode,
        };
        SelectedLanguagePostFix = Se.Settings.Tools.BatchConvert.LanguagePostFix;
        if (SelectedLanguagePostFix == null)
        {
            SelectedLanguagePostFix = LanguagePostFixes[1];
        }

        PaddleOcrLanguages = new ObservableCollection<OcrLanguage2>(PaddleOcr.GetLanguages().OrderBy(p => p.ToString()));
        TesseractDictionaryItems = new ObservableCollection<TesseractDictionary>();
        BinaryOcrDatabases = new ObservableCollection<string>(BinaryOcrDb.GetDatabases(Se.OcrFolder));
        NOcrDatabases = new ObservableCollection<string>(NOcrDb.GetDatabases(Se.OcrFolder).OrderBy(p => p));
        OllamaModels = new ObservableCollection<string>(Se.Settings.Ocr.OllamaModels);

        _folderHelper = folderHelper;
        _windowService = windowService;

        OutputFolder = string.Empty;
        LoadActiveTesseractDictionaries();
        LoadSettings();
        OnOcrEngineChanged();
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

    private void LoadSettings()
    {
        UseSourceFolder = Se.Settings.Tools.BatchConvert.SaveInSourceFolder; ;
        UseOutputFolder = !UseSourceFolder;
        OutputFolder = Se.Settings.Tools.BatchConvert.OutputFolder;
        Overwrite = Se.Settings.Tools.BatchConvert.Overwrite;
        SelectedTargetEncoding = Se.Settings.Tools.BatchConvert.TargetEncoding;
        SelectedOcrEngine = OcrEngines.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.OcrEngine) ?? OcrEngines.First();    
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BatchConvert.SaveInSourceFolder = !UseOutputFolder;
        Se.Settings.Tools.BatchConvert.OutputFolder = OutputFolder;
        Se.Settings.Tools.BatchConvert.Overwrite = Overwrite;
        Se.Settings.Tools.BatchConvert.TargetEncoding = SelectedTargetEncoding ?? TargetEncodings.First();
        Se.Settings.Tools.BatchConvert.LanguagePostFix = SelectedLanguagePostFix ?? Se.Language.General.TwoLetterLanguageCode;
        Se.Settings.Tools.BatchConvert.OcrEngine = SelectedOcrEngine ?? "nOcr";

        var ocrEngine = SelectedOcrEngine;
        if (ocrEngine == "Tesseract")
        {
            Se.Settings.Tools.BatchConvert.TesseractLanguage = SelectedTesseractDictionaryItem?.Code ?? "eng";
        }

        if (ocrEngine == "PaddleOCR")
        {
            Se.Settings.Tools.BatchConvert.PaddleLanguage = SelectedPaddleOcrLanguage?.Code ?? "en";
        }

        if (ocrEngine == "BinaryOcr")
        {
            Se.Settings.Tools.BatchConvert.BinaryOcrDatabase = SelectedBinaryOcrDatabase ?? "Latin";
        }

        if (ocrEngine == "nOcr" && !string.IsNullOrWhiteSpace(SelectedNOcrDatabase))
        {
            Se.Settings.Ocr.NOcrDatabase = SelectedNOcrDatabase;
        }

        if (ocrEngine == "Ollama" && !string.IsNullOrWhiteSpace(SelectedOllamaModel))
        {
            Se.Settings.Ocr.OllamaModel = SelectedOllamaModel;
        }

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (UseOutputFolder && string.IsNullOrWhiteSpace(OutputFolder))
        {
            await MessageBox.Show(Window!, "Error",
                "Please select output folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task BrowseOutputFolder()
    {
        var folder = await _folderHelper.PickFolderAsync(Window!, "Select output folder");
        if (!string.IsNullOrEmpty(folder))
        {
            OutputFolder = folder;
            UseOutputFolder = true;
            UseSourceFolder = false;
        }
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
    }

    internal void OnOcrEngineChanged()
    {
        var ocrEngine = SelectedOcrEngine;
        if (string.IsNullOrEmpty(ocrEngine))
        {
            IsOcrLanguageVisible = false;
            IsTesseractOcrVisible = false;
            IsPaddleOCrVisible = false;
            return;
        }

        IsOcrLanguageVisible = ocrEngine != "nOcr" && ocrEngine != "BinaryOcr" && ocrEngine != "Ollama";
        IsTesseractOcrVisible = ocrEngine == "Tesseract";
        IsPaddleOCrVisible = ocrEngine == "PaddleOCR";
        IsBinaryOcrVisible = ocrEngine == "BinaryOcr";
        IsNOcrVisible = ocrEngine == "nOcr";
        IsOllamaVisible = ocrEngine == "Ollama";

        if (ocrEngine == "Tesseract")
        {
            SelectedTesseractDictionaryItem = TesseractDictionaryItems
                .FirstOrDefault(p => p.Code == Se.Settings.Tools.BatchConvert.TesseractLanguage) ?? TesseractDictionaryItems.FirstOrDefault();
        }

        if (ocrEngine == "PaddleOCR")
        {
            SelectedPaddleOcrLanguage = PaddleOcrLanguages
                .FirstOrDefault(p => p.Code == Se.Settings.Tools.BatchConvert.PaddleLanguage) ?? PaddleOcrLanguages.FirstOrDefault();
        }

        if (ocrEngine == "BinaryOcr")
        {
            SelectedBinaryOcrDatabase = BinaryOcrDatabases
                .FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.BinaryOcrDatabase) ?? BinaryOcrDatabases.FirstOrDefault();
        }

        if (ocrEngine == "nOcr")
        {
            SelectedNOcrDatabase = NOcrDatabases
                .FirstOrDefault(p => p == Se.Settings.Ocr.NOcrDatabase) ?? NOcrDatabases.FirstOrDefault();
        }

        if (ocrEngine == "Ollama")
        {
            var current = Se.Settings.Ocr.OllamaModel;
            if (!string.IsNullOrWhiteSpace(current) && !OllamaModels.Contains(current))
            {
                OllamaModels.Add(current);
            }

            SelectedOllamaModel = OllamaModels.FirstOrDefault(p => p == current) ?? OllamaModels.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task PickOllamaModel()
    {
        var result = await _windowService.ShowDialogAsync<PickOllamaModelWindow, PickOllamaModelViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.PickOllamaModel, SelectedOllamaModel, Se.Settings.Ocr.OllamaUrl); });

        if (result is { OkPressed: true, SelectedModel: not null })
        {
            if (!OllamaModels.Contains(result.SelectedModel))
            {
                OllamaModels.Add(result.SelectedModel);
            }

            SelectedOllamaModel = result.SelectedModel;
        }
    }
}
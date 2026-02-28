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

    [ObservableProperty] bool _isOcrLanguageVisible;
    [ObservableProperty] bool _isTesseractOcrVisible;
    [ObservableProperty] bool _isPaddleOCrVisible;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public BatchConvertSettingsViewModel(IFolderHelper folderHelper)
    {
        var encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();
        TargetEncodings = new ObservableCollection<string>(encodings);

        OcrEngines = new ObservableCollection<string> { "nOcr", "Tesseract" };
        if (OperatingSystem.IsWindows() && File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
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

        _folderHelper = folderHelper;

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

        IsOcrLanguageVisible = ocrEngine != "nOcr";
        IsTesseractOcrVisible = ocrEngine == "Tesseract";
        IsPaddleOCrVisible = ocrEngine == "PaddleOCR";

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
    }
}
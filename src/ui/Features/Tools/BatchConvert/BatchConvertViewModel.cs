using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BatchConvertItem> _batchItems;
    [ObservableProperty] private BatchConvertItem? _selectedBatchItem;
    [ObservableProperty] private string _batchItemsInfo;
    [ObservableProperty] private ObservableCollection<string> _targetFormats;
    [ObservableProperty] private string? _selectedTargetFormat;
    [ObservableProperty] private ObservableCollection<BatchConvertFunction> _batchFunctions;
    [ObservableProperty] private BatchConvertFunction? _selectedBatchFunction;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private bool _isConverting;
    [ObservableProperty] private bool _areControlsEnabled;
    [ObservableProperty] private string _outputFolderLabel;
    [ObservableProperty] private string _outputFolderLinkLabel;
    [ObservableProperty] private string _outputEncodingLabel;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private double _progressMaxValue;
    [ObservableProperty] private string _actionsSelected;
    [ObservableProperty] private bool _isTargetFormatSettingsVisible;
    [ObservableProperty] private ObservableCollection<string> _filterItems;
    [ObservableProperty] private string? _selectedFilterItem;
    [ObservableProperty] private string _filterText;
    [ObservableProperty] private bool _isFilterTextVisible;
    [ObservableProperty] private bool _isRemoveVisible;
    [ObservableProperty] private bool _isOpenContainingFolderVisible;

    // Add formatting
    [ObservableProperty] private bool _formattingAddItalic;
    [ObservableProperty] private bool _formattingAddBold;
    [ObservableProperty] private bool _formattingAddUnderline;
    [ObservableProperty] private bool _formattingAddAlignmentTag;
    [ObservableProperty] private bool _formattingAddColor;
    [ObservableProperty] private Color _formattingAddColorValue;
    [ObservableProperty] private ObservableCollection<DisplayAlignment> _alignmentTagOptions;
    [ObservableProperty] private DisplayAlignment? _selectedAlignmentTagOption;

    // Remove formatting
    [ObservableProperty] private bool _formattingRemoveAll;
    [ObservableProperty] private bool _formattingRemoveItalic;
    [ObservableProperty] private bool _formattingRemoveBold;
    [ObservableProperty] private bool _formattingRemoveUnderline;
    [ObservableProperty] private bool _formattingRemoveFontTags;
    [ObservableProperty] private bool _formattingRemoveAlignmentTags;
    [ObservableProperty] private bool _formattingRemoveColors;

    // Offset time codes
    [ObservableProperty] private bool _offsetTimeCodesForward;
    [ObservableProperty] private bool _offsetTimeCodesBack;
    [ObservableProperty] private TimeSpan _offsetTimeCodesTime;

    // Adjust mininum gap between subtitles
    [ObservableProperty] private int _minGapMs;

    // Adjust display duration
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private AdjustDurationDisplay _selectedAdjustType;
    [ObservableProperty] private double _adjustSeconds;
    [ObservableProperty] private int _adjustPercent;
    [ObservableProperty] private double _adjustFixed;
    [ObservableProperty] private double _adjustRecalculateMaxCharacterPerSecond;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;
    [ObservableProperty] private bool _adjustIsSecondsVisible;
    [ObservableProperty] private bool _adjustIsPercentVisible;
    [ObservableProperty] private bool _adjustIsFixedVisible;
    [ObservableProperty] private bool _adjustIsRecalculateVisible;

    // Delete lines
    [ObservableProperty] private ObservableCollection<int> _deleteLineNumbers;
    [ObservableProperty] private int _deleteXFirstLines;
    [ObservableProperty] private int _deleteXLastLines;
    [ObservableProperty] private string _deleteLinesContains;
    [ObservableProperty] private string _deleteActorsOrStyles;

    // Change frame rate
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;

    // Change speed
    [ObservableProperty] private double _changeSpeedPercent;

    // Change casing
    [ObservableProperty] private bool _normalCasing;
    [ObservableProperty] private bool _normalCasingFixNames;
    [ObservableProperty] private bool _normalCasingOnlyUpper;
    [ObservableProperty] private bool _fixNamesOnly;
    [ObservableProperty] private bool _allUppercase;
    [ObservableProperty] private bool _allLowercase;

    // Auto translate
    [ObservableProperty] public ObservableCollection<IAutoTranslator> _autoTranslators;
    [ObservableProperty] private IAutoTranslator _selectedAutoTranslator;
    [ObservableProperty] private ObservableCollection<TranslationPair> _sourceLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedSourceLanguage;
    [ObservableProperty] private ObservableCollection<TranslationPair> _targetLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedTargetLanguage;
    [ObservableProperty] private string _autoTranslateModel;
    [ObservableProperty] private string _autoTranslateUrl;
    [ObservableProperty] private string _autoTranslateApiKey;
    [ObservableProperty] private bool _autoTranslateModelIsVisible;
    [ObservableProperty] private bool _autoTranslateModelBrowseIsVisible;
    [ObservableProperty] private bool _autoTranslateUrlIsVisible;
    [ObservableProperty] private bool _autoTranslateApiKeyIsVisible;

    // Fix common errors
    [ObservableProperty] private FixCommonErrors.ProfileDisplayItem? _fixCommonErrorsProfile;

    // Merge lines with same text
    [ObservableProperty] private int _mergeSameTextMaxMillisecondsBetweenLines;
    [ObservableProperty] private bool _mergeSameTextIncludeIncrementingLines;

    // Merge lines with same time codes
    [ObservableProperty] private int _mergeSameTimeMaxMillisecondsDifference;
    [ObservableProperty] private bool _mergeSameTimeMergeDialog;
    [ObservableProperty] private bool _mergeSameTimeAutoBreak;

    // Fix right-to-left
    [ObservableProperty] private bool _rtlFixViaUniCode;
    [ObservableProperty] private bool _rtlRemoveUniCode;
    [ObservableProperty] private bool _rtlReverseStartEnd;

    // Bride gaps
    [ObservableProperty] private int _bridgeGapsSmallerThanMs;
    [ObservableProperty] private int _bridgeGapsMinGapMs;
    [ObservableProperty] private int _bridgeGapsPercentForLeft;

    // Split/break long lines
    [ObservableProperty] private bool _splitBreakSplitLongLines;
    [ObservableProperty] private int _splitBreakSingleLineMaxLength;
    [ObservableProperty] private int _splitBreakMaxNumberOfLines;
    [ObservableProperty] private bool _splitBreakRebalanceLongLines;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public ScrollViewer FunctionContainer { get; internal set; }

    private List<BatchConvertItem> _allBatchItems;
    private readonly System.Timers.Timer _filesTimer;
    private bool _isFilesDirty;
    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IBatchConverter _batchConverter;
    private readonly IBatchConvertItemSplitter _batchConvertItemSplitter;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private List<string> _encodings;
    private List<string> _targetFormatsWithSettings;

    public BatchConvertViewModel(
        IWindowService windowService,
        IFileHelper fileHelper,
        IBatchConverter batchConverter,
        IFolderHelper folderHelper,
        IBatchConvertItemSplitter batchConvertItemSplitter)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _batchConverter = batchConverter;
        _folderHelper = folderHelper;
        _batchConvertItemSplitter = batchConvertItemSplitter;

        BatchItems = new ObservableCollection<BatchConvertItem>();
        _allBatchItems = new List<BatchConvertItem>();
        BatchFunctions = new ObservableCollection<BatchConvertFunction>();

        TargetFormats = new ObservableCollection<string>(SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop().Select(p => p.Name))
        {
            BatchConverter.FormatAyato,
            BatchConverter.FormatBdnXml,
            BatchConverter.FormatBluRaySup,
            BatchConverter.FormatCavena890,
            BatchConverter.FormatCustomTextFormat,
            BatchConverter.FormatDCinemaInterop,
            BatchConverter.FormatDCinemaSmpte2014,
            BatchConverter.FormatDostImage,
            BatchConverter.FormatFcpImage,
            BatchConverter.FormatImagesWithTimeCodesInFileName,
            BatchConverter.FormatPac,
            BatchConverter.FormatPlainText,
            BatchConverter.FormatVobSub
        };

        FilterItems =
        [
            Se.Language.General.AllFiles,
            Se.Language.Tools.BatchConvert.FileNameContainsDotDotDot,
            Se.Language.Tools.BatchConvert.TrackLanguageContainsDotDotDot,
        ];
        SelectedFilterItem = FilterItems.FirstOrDefault();
        FilterText = string.Empty;

        DeleteLineNumbers = new ObservableCollection<int>();
        BatchItemsInfo = string.Empty;
        ProgressText = string.Empty;
        ActionsSelected = string.Empty;
        DeleteLinesContains = string.Empty;
        OutputFolderLabel = string.Empty;
        OutputFolderLinkLabel = string.Empty;
        OutputEncodingLabel = string.Empty;
        StatusText = string.Empty;
        DeleteActorsOrStyles = string.Empty;
        FunctionContainer = new ScrollViewer();
        FromFrameRates = new ObservableCollection<double>
        {
            23.976,
            24,
            25,
            29.97,
            30,
            48,
            59.94,
            60,
            120,
        };
        ToFrameRates = new ObservableCollection<double>
        {
            23.976,
            24,
            25,
            29.97,
            30,
            48,
            59.94,
            60,
            120,
        };
        AdjustTypes = new ObservableCollection<AdjustDurationDisplay>(AdjustDurationDisplay.ListAll());
        SelectedAdjustType = AdjustTypes.First();

        AlignmentTagOptions = new ObservableCollection<DisplayAlignment>(DisplayAlignment.GetAll());
        SelectedAlignmentTagOption = AlignmentTagOptions[1];

        BatchFunctions = new ObservableCollection<BatchConvertFunction>(BatchConvertFunction.List(this));

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();

        AutoTranslateModel = string.Empty;
        AutoTranslateUrl = string.Empty;
        AutoTranslateApiKey = string.Empty;
        AutoTranslators =
        [
            new OllamaTranslate(),
            new LibreTranslate(),
            new LmStudioTranslate(),
            new NoLanguageLeftBehindServe(),
            new NoLanguageLeftBehindApi(),
            new DeepLTranslate()
        ];
        SelectedAutoTranslator = AutoTranslators[0];
        OnAutoTranslatorChanged();

        SelectedFromFrameRate = FromFrameRates[0];
        SelectedToFrameRate = ToFrameRates[1];

        ChangeSpeedPercent = 100;

        FixCommonErrorsProfile = LoadDefaultProfile();

        _targetFormatsWithSettings = new List<string>
        {
            BatchConverter.FormatBdnXml,
            BatchConverter.FormatBluRaySup,
            BatchConverter.FormatCustomTextFormat,
            BatchConverter.FormatDostImage,
            BatchConverter.FormatFcpImage,
            BatchConverter.FormatImagesWithTimeCodesInFileName,
            BatchConverter.FormatVobSub,
            new AdvancedSubStationAlpha().Name,
        };

        LoadSettings();
        FilterComboBoxChanged();
        _filesTimer = new System.Timers.Timer(250);
        _filesTimer.Elapsed += (sender, args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _filesTimer.Stop();

                if (_isFilesDirty)
                {
                    _isFilesDirty = false;
                    UpdateFilteredFiles();
                }

                _filesTimer.Start();
            });
        };
        _filesTimer.Start();
    }

    private void UpdateFilteredFiles()
    {
        BatchItems.Clear();
        if (SelectedFilterItem == Se.Language.Tools.BatchConvert.FileNameContainsDotDotDot && !string.IsNullOrEmpty(FilterText))
        {
            foreach (var item in _allBatchItems)
            {
                if (item.FileName.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase))
                {
                    BatchItems.Add(item);
                }
            }
        }
        else if (SelectedFilterItem == Se.Language.Tools.BatchConvert.TrackLanguageContainsDotDotDot && !string.IsNullOrEmpty(FilterText))
        {
            foreach (var item in _allBatchItems)
            {
                if (item.Format.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase))
                {
                    BatchItems.Add(item);
                }
            }
        }
        else
        {
            BatchItems.AddRange(_allBatchItems);
        }
    }

    private static FixCommonErrors.ProfileDisplayItem LoadDefaultProfile()
    {
        var profiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        var displayProfiles = new List<FixCommonErrors.ProfileDisplayItem>();
        var defaultName = Se.Settings.Tools.FixCommonErrors.LastProfileName;
        var allFixRules = FixCommonErrorsViewModel.MakeDefaultRules();

        foreach (var setting in profiles)
        {
            var profile = new FixCommonErrors.ProfileDisplayItem
            {
                Name = setting.ProfileName,
                FixRules = new ObservableCollection<FixRuleDisplayItem>(allFixRules.Select(rule => new FixRuleDisplayItem(rule)
                {
                    IsSelected = setting.SelectedRules.Contains(rule.FixCommonErrorFunctionName)
                }))
            };

            if (defaultName == profile.Name)
            {
                return profile;
            }

            displayProfiles.Add(profile);
        }

        return displayProfiles.First();
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BatchConvert.TargetFormat = SelectedTargetFormat ?? TargetFormats.First();

        Se.Settings.Tools.BatchConvert.LastFilterItem = SelectedFilterItem ?? string.Empty;

        Se.Settings.Tools.BatchConvert.AdjustVia = SelectedAdjustType.Name;
        Se.Settings.Tools.BatchConvert.AdjustMaxCps = AdjustRecalculateMaxCharacterPerSecond;
        Se.Settings.Tools.BatchConvert.AdjustOptimalCps = AdjustRecalculateOptimalCharacterPerSecond;
        Se.Settings.Tools.BatchConvert.AdjustDurationFixedMilliseconds = (int)AdjustFixed;
        Se.Settings.Tools.BatchConvert.AdjustDurationSeconds = AdjustSeconds;
        Se.Settings.Tools.BatchConvert.AdjustDurationPercentage = AdjustPercent;

        Se.Settings.Tools.BatchConvert.AutoTranslateEngine = SelectedAutoTranslator.Name;
        Se.Settings.Tools.BatchConvert.AutoTranslateSourceLanguage = SelectedSourceLanguage?.TwoLetterIsoLanguageName ?? "auto";
        Se.Settings.Tools.BatchConvert.AutoTranslateTargetLanguage = SelectedTargetLanguage?.TwoLetterIsoLanguageName ?? "en";

        // Change casing
        if (NormalCasing)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "Normal";
        }
        else if (FixNamesOnly)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "FixNamesOnly";
        }
        else if (AllUppercase)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "AllUppercase";
        }
        else if (AllLowercase)
        {
            Se.Settings.Tools.BatchConvert.ChangeCasingType = "AllLowercase";
        }

        // Fix right-to-left
        if (RtlFixViaUniCode)
        {
            Se.Settings.Tools.BatchConvert.FixRtlMode = "FixViaUnicode";
        }
        else if (RtlRemoveUniCode)
        {
            Se.Settings.Tools.BatchConvert.FixRtlMode = "RemoveUnicode";
        }
        else
        {
            Se.Settings.Tools.BatchConvert.FixRtlMode = "ReverseStartEnd";
        }

        Se.SaveSettings();
    }

    private void LoadSettings()
    {
        var targetFormat = TargetFormats.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.TargetFormat);
        if (targetFormat == null)
        {
            targetFormat = TargetFormats.First();
        }

        SelectedTargetFormat = targetFormat;

        SelectedAdjustType = AdjustTypes.First();
        foreach (var adjustType in AdjustTypes)
        {
            if (adjustType.Name == Se.Settings.Tools.BatchConvert.AdjustVia)
            {
                SelectedAdjustType = adjustType;
                break;
            }
        }

        var filterItem = FilterItems.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.LastFilterItem);
        if (filterItem != null)
        {
            SelectedFilterItem = filterItem;
        }

        AdjustRecalculateMaxCharacterPerSecond = Se.Settings.Tools.BatchConvert.AdjustMaxCps;
        AdjustRecalculateOptimalCharacterPerSecond = Se.Settings.Tools.BatchConvert.AdjustOptimalCps;
        AdjustFixed = Se.Settings.Tools.BatchConvert.AdjustDurationFixedMilliseconds;
        AdjustSeconds = Se.Settings.Tools.BatchConvert.AdjustDurationSeconds;
        AdjustPercent = Se.Settings.Tools.BatchConvert.AdjustDurationPercentage;

        var translator = AutoTranslators.FirstOrDefault(p => p.Name == Se.Settings.Tools.BatchConvert.AutoTranslateEngine);
        if (translator != null)
        {
            SelectedAutoTranslator = translator;
        }

        var sourceLanguage = SourceLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == Se.Settings.Tools.BatchConvert.AutoTranslateSourceLanguage);
        if (sourceLanguage != null)
        {
            SelectedSourceLanguage = sourceLanguage;
        }

        var defaultTarget = AutoTranslateViewModel.EvaluateDefaultTargetLanguageCode(string.Empty, SelectedSourceLanguage?.Code ?? string.Empty);
        SelectedTargetLanguage = TargetLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == defaultTarget);
        var targetLanguage = TargetLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == Se.Settings.Tools.BatchConvert.AutoTranslateTargetLanguage);
        if (targetLanguage != null)
        {
            SelectedTargetLanguage = targetLanguage;
        }

        // Change casing
        if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "Normal")
        {
            NormalCasing = true;
        }
        else if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "FixNamesOnly")
        {
            FixNamesOnly = true;
        }
        else if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "AllUppercase")
        {
            AllUppercase = true;
        }
        else if (Se.Settings.Tools.BatchConvert.ChangeCasingType == "AllLowercase")
        {
            AllLowercase = true;
        }

        NormalCasingFixNames = Se.Settings.Tools.BatchConvert.NormalCasingFixNames;
        NormalCasingOnlyUpper = Se.Settings.Tools.BatchConvert.NormalCasingOnlyUpper;

        UpdateOutputProperties();

        MergeSameTextMaxMillisecondsBetweenLines = Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines;
        MergeSameTextIncludeIncrementingLines = Se.Settings.Tools.MergeSameText.IncludeIncrementingLines;

        MergeSameTimeMaxMillisecondsDifference = Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference;
        MergeSameTimeMergeDialog = Se.Settings.Tools.MergeSameTimeCode.MergeDialog;
        MergeSameTimeAutoBreak = Se.Settings.Tools.MergeSameTimeCode.AutoBreak;

        if (Se.Settings.Tools.BatchConvert.FixRtlMode == "FixViaUnicode")
        {
            RtlFixViaUniCode = true;
        }
        else if (Se.Settings.Tools.BatchConvert.FixRtlMode == "RemoveUnicode")
        {
            RtlRemoveUniCode = true;
        }
        else
        {
            RtlReverseStartEnd = true;
        }

        BridgeGapsSmallerThanMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs;
        BridgeGapsMinGapMs = Se.Settings.Tools.BridgeGaps.MinGapMs;
        BridgeGapsPercentForLeft = Se.Settings.Tools.BridgeGaps.PercentForLeft;

        SplitBreakSingleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
        SplitBreakMaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
        SplitBreakSplitLongLines = Se.Settings.Tools.SplitRebalanceLongLinesSplit;
        SplitBreakRebalanceLongLines = Se.Settings.Tools.SplitRebalanceLongLinesRebalance;
    }

    private void UpdateOutputProperties()
    {
        var targetEncoding =
            _encodings.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.TargetEncoding);
        if (targetEncoding == null)
        {
            targetEncoding = _encodings.First();
            Se.Settings.Tools.BatchConvert.TargetEncoding = targetEncoding;
        }

        if (!Se.Settings.Tools.BatchConvert.SaveInSourceFolder &&
            string.IsNullOrWhiteSpace(Se.Settings.Tools.BatchConvert.OutputFolder))
        {
            Se.Settings.Tools.BatchConvert.SaveInSourceFolder = true;
        }

        if (Se.Settings.Tools.BatchConvert.SaveInSourceFolder)
        {
            OutputFolderLinkLabel = string.Empty;
            OutputFolderLabel = Se.Language.Tools.BatchConvert.OutputFolderSource;
        }
        else
        {
            OutputFolderLinkLabel = string.Format(Se.Language.Tools.BatchConvert.OutputFolderX, Se.Settings.Tools.BatchConvert.OutputFolder);
            OutputFolderLabel = string.Empty;
        }

        OutputEncodingLabel = string.Format(Se.Language.Tools.BatchConvert.EncodingXOverwriteY,
            Se.Settings.Tools.BatchConvert.TargetEncoding,
            Se.Settings.Tools.BatchConvert.Overwrite);
    }

    [RelayCommand]
    private async Task ShowRemoveTextForHearingImpairedSettings()
    {
        _ = await _windowService
            .ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(
                Window!, vm => { vm.Initialize(new Subtitle()); });
    }

    [RelayCommand]
    private void Done()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        IsConverting = false;
        foreach (var batchItem in BatchItems)
        {
            if (batchItem.Status != "-" &&
                batchItem.Status != Se.Language.General.Converted &&
                batchItem.Status != Se.Language.General.Error)
            {
                batchItem.Status = Se.Language.General.Cancelled;
            }
        }

        ProgressText = string.Empty;
    }

    [RelayCommand]
    private async Task Convert()
    {
        if (BatchItems.Count == 0)
        {
            await ShowStatus(Se.Language.General.NoFilesToConvert);
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        foreach (var batchItem in BatchItems)
        {
            batchItem.Status = "-";
        }

        SaveSettings();

        var config = MakeBatchConvertConfig();
        _batchConverter.Initialize(config);
        var start = DateTime.UtcNow.Ticks;

        IsProgressVisible = true;
        IsConverting = true;
        AreControlsEnabled = false;
        ProgressMaxValue = BatchItems.Count;
        _ = Task.Run(async () =>
        {
            var count = 1;
            foreach (var batchItem in BatchItems)
            {
                var countDisplay = count;
                ProgressText = string.Format(Se.Language.General.ConvertingXofYDotDoDot, countDisplay, BatchItems.Count);
                ProgressValue = countDisplay / (double)BatchItems.Count;

                if (batchItem.Format!.StartsWith("Transport Stream", StringComparison.Ordinal))
                {
                    var tsResult = _batchConvertItemSplitter.LoadTransportStream(batchItem, _cancellationToken);
                    foreach (var bi in tsResult)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        await _batchConverter.Convert(bi, _cancellationToken);
                    }
                }
                else
                {
                    await _batchConverter.Convert(batchItem, _cancellationToken);
                }

                count++;

                if (_cancellationToken.IsCancellationRequested)
                {
                    ProgressText = string.Empty;
                    break;
                }
            }

            IsProgressVisible = false;
            IsConverting = false;
            AreControlsEnabled = true;
            ProgressText = string.Empty;

            var end = DateTime.UtcNow.Ticks;
            var elapsed = new TimeSpan(end - start).TotalMilliseconds;
            var message = string.Format(Se.Language.General.XFilesConvertedInY, BatchItems.Count, elapsed);
            if (_cancellationToken.IsCancellationRequested)
            {
                message += Environment.NewLine + Se.Language.General.ConversionCancelledByUser;
            }

            await ShowStatus(message);
        }, _cancellationToken);
    }

    [RelayCommand]
    private void ChangeSpeedSetFromDropFrameValue()
    {
        ChangeSpeedPercent = 100.1001;
    }

    [RelayCommand]
    private void ChangeSpeedSetToDropFrameValue()
    {
        ChangeSpeedPercent = 99.9889;
    }

    [RelayCommand]
    private async Task ShowFixCommonRules()
    {
        if (Window == null || FixCommonErrorsProfile == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BatchConvertFixCommonErrorsSettingsWindow, BatchConvertFixCommonErrorsSettingsViewModel>(Window!,
            vm => { vm.Initialize(FixCommonErrorsProfile); });

        if (result.OkPressed && result.SelectedProfile != null)
        {
            FixCommonErrorsProfile = result.SelectedProfile;
        }
    }

    [RelayCommand]
    private async Task ShowMultipleReplace()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(Window!,
            vm => { vm.Initialize(new Subtitle()); });
    }

    [RelayCommand]
    private async Task Statistics()
    {
        if (Window == null)
        {
            return;
        }

        var stats = BatchStatics.CalculateGeneralStatistics(BatchItems.ToList());
        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.File.Statistics.Title, stats, 1000, 600, false, true);
        });
    }

    [RelayCommand]
    private async Task ShowErrorList()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BatchErrorListWindow, BatchErrorListViewModel>(Window!, vm =>
        {
            vm.Initialize(BatchItems.ToList());
        });
    }


    [RelayCommand]
    private void CancelConvert()
    {
        _cancellationTokenSource.Cancel();
        IsConverting = false;
    }

    [RelayCommand]
    private async Task OpenOutputFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.Settings.Tools.BatchConvert.OutputFolder);
    }

    [RelayCommand]
    private async Task ShowTargetFormatSettings()
    {
        var targetFormat = SelectedTargetFormat;
        if (targetFormat == null)
        {
            return;
        }

        if (Window == null)
        {
            return;
        }

        if (targetFormat == BatchConverter.FormatCustomTextFormat)
        {
            var subtitles = new List<SubtitleLineViewModel>();
            var p = new Paragraph("This is a sample text", 0, 1000);
            subtitles.Add(new SubtitleLineViewModel(p, new SubRip()));

            var result = await _windowService.ShowDialogAsync<ExportCustomTextFormatWindow, ExportCustomTextFormatViewModel>(Window,
                vm => { vm.Initialize(subtitles, string.Empty, string.Empty, true); });
            return;
        }

        // check for Advadvanced SubStation Alpha
        if (targetFormat == AdvancedSubStationAlpha.NameOfFormat)
        {
            var result = await _windowService.ShowDialogAsync<BatchConvertAssaWindow, BatchConvertAssaViewModel>(Window);
            return;
        }

        IExportHandler? exportHandler = null;

        if (targetFormat == BatchConverter.FormatBdnXml)
        {
            exportHandler = new ExportHandlerBdnXml();
        }
        else if (targetFormat == BatchConverter.FormatBluRaySup)
        {
            exportHandler = new ExportHandlerBluRaySup();
        }
        else if (targetFormat == BatchConverter.FormatDostImage)
        {
            exportHandler = new ExportHandlerDost();
        }
        else if (targetFormat == BatchConverter.FormatFcpImage)
        {
            exportHandler = new ExportHandlerFcp();
        }
        else if (targetFormat == BatchConverter.FormatImagesWithTimeCodesInFileName)
        {
            exportHandler = new ExportHandlerImagesWithTimeCode();
        }
        else if (targetFormat == BatchConverter.FormatVobSub)
        {
            exportHandler = new ExportHandlerVobSub();
        }

        if (exportHandler != null)
        {
            var result = await _windowService.ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(Window, vm =>
            {
                var subtitles = new ObservableCollection<SubtitleLineViewModel>();
                var p = new Paragraph("This is a sample text", 0, 1000);
                subtitles.Add(new SubtitleLineViewModel(p, new SubRip()));
                vm.Initialize(exportHandler, subtitles, string.Empty, string.Empty, true);
            });
            return;
        }
    }

    [RelayCommand]
    private async Task AddFiles()
    {
        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window!, Se.Language.General.SelectFilesToConvert);
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            bool flowControl = AddFile(fileName);
            if (!flowControl)
            {
                continue;
            }
        }

        MakeBatchItemsInfo();
        _isFilesDirty = true;
    }

    [RelayCommand]
    private async Task ShowSubtitleFormatPicker()
    {
        if (Window == null)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<PickSubtitleFormatWindow, PickSubtitleFormatViewModel>(Window, vm =>
        {
            vm.Initialize(SubtitleFormat.AllSubtitleFormats.FirstOrDefault(p => p.Name == SelectedTargetFormat) ?? new SubRip(), new Subtitle());
        });

        if (viewModel.OkPressed)
        {
            var selectedFormat = viewModel.GetSelectedFormat();
            if (selectedFormat != null && selectedFormat.Name != SelectedTargetFormat)
            {
                SelectedTargetFormat = selectedFormat.Name;
            }
        }
    }

    private bool AddFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var fileInfo = new FileInfo(fileName);

        Subtitle? subtitle = null;
        var format = Se.Language.General.Unknown;
        if (ext == ".sup" && FileUtil.IsBluRaySup(fileName))
        {
            format = BatchConverter.FormatBluRaySup;
        }

        if (ext == ".sub" && FileUtil.IsVobSub(fileName))
        {
            format = BatchConverter.FormatVobSub;
        }

        if (ext == ".mkv" || ext == ".mks")
        {
            using (var matroska = new MatroskaFile(fileName))
            {
                if (matroska.IsValid)
                {
                    var codecToFormat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "S_VOBSUB", "VobSub" },
                            { "S_HDMV/PGS", "PGS" },
                            { "S_TEXT/UTF8", "SRT" },
                            { "S_TEXT/SSA", "SSA" },
                            { "S_TEXT/ASS", "ASS" },
                            { "S_DVBSUB", "DvdSub" },
                            { "S_HDMV/TEXTST", "TextST" }
                        };

                    var tracksByFormat = new Dictionary<string, List<string>>();

                    foreach (var track in matroska.GetTracks(true))
                    {
                        if (codecToFormat.TryGetValue(track.CodecId, out var formatName))
                        {
                            if (!tracksByFormat.ContainsKey(formatName))
                            {
                                tracksByFormat[formatName] = new List<string>();
                            }

                            tracksByFormat[formatName].Add(MakeMkvTrackInfoString(track));

                            format = $"Matroska/{formatName} - {MakeMkvTrackInfoString(track)}";
                            var matroskaBatchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
                            matroskaBatchItem.LanguageCode = track.Language;
                            matroskaBatchItem.TrackNumber = track.TrackNumber.ToString(CultureInfo.InvariantCulture);
                            BatchItems.Add(matroskaBatchItem);
                            _allBatchItems.Add(matroskaBatchItem);
                        }
                    }

                    if (tracksByFormat.Count == 0)
                    {
                        format = "No subtitle tracks";
                    }

                    return false;
                }
            }
        }
        else if (ext == ".mp4" || ext == ".m4v" || ext == ".m4s")
        {
            var mp4Files = new List<string>();
            var mp4Parser = new MP4Parser(fileName);
            var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
            if (mp4Parser.VttcSubtitle?.Paragraphs.Count > 0)
            {
                var name = "MP4/WebVTT - " + mp4Parser.VttcLanguage;
                mp4Files.Add(name);
                var mp4BatchItem = new BatchConvertItem(fileName, fileInfo.Length, name, subtitle);
                mp4BatchItem.LanguageCode = mp4Parser.VttcLanguage;
                BatchItems.Add(mp4BatchItem);
                _allBatchItems.Add(mp4BatchItem);
            }

            foreach (var track in mp4SubtitleTracks)
            {
                if (track.Mdia.IsTextSubtitle || track.Mdia.IsClosedCaption)
                {
                    var name = $"MP4/#{mp4SubtitleTracks.IndexOf(track)} {track.Mdia.HandlerType} - {track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString}";
                    mp4Files.Add(name);
                    var mp4BatchItem = new BatchConvertItem(fileName, fileInfo.Length, name, subtitle);
                    mp4BatchItem.LanguageCode = track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString;
                    BatchItems.Add(mp4BatchItem);
                    _allBatchItems.Add(mp4BatchItem);
                }
            }

            if (mp4Files.Count <= 0)
            {
                format = "No subtitle tracks";
            }

            return false;
        }
        else if ((ext == ".ts" || ext == ".m2ts" || ext == ".mts" || ext == ".mpg" || ext == ".mpeg") &&
                 (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
        {
            format = "Transport Stream";
            var tsBatchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
            BatchItems.Add(tsBatchItem);
            _allBatchItems.Add(tsBatchItem);
            return false;
        }

        if (format == Se.Language.General.Unknown && fileInfo.Length < 200_000)
        {
            subtitle = Subtitle.Parse(fileName);
            if (subtitle != null)
            {
                format = subtitle.OriginalFormat.Name;
            }
        }

        if (format == Se.Language.General.Unknown)
        {
            foreach (var f in SubtitleFormat.GetBinaryFormats(false))
            {
                if (f.IsMine(null, fileName))
                {
                    subtitle = new Subtitle();
                    f.LoadSubtitle(subtitle, null, fileName);
                    subtitle.OriginalFormat = f;
                    format = f.Name;
                    break; // format found, exit the loop
                }
            }
        }

        if (format == Se.Language.General.Unknown && fileInfo.Length < 300_000)
        {
            subtitle = Subtitle.Parse(fileName);
            if (subtitle != null)
            {
                format = subtitle.OriginalFormat.Name;
            }
        }

        var batchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
        BatchItems.Add(batchItem);
        _allBatchItems.Add(batchItem);
        return true;
    }

    private static string MakeMkvTrackInfoString(MatroskaTrackInfo track)
    {
        return (track.Language ?? "undefined") + (track.IsForced ? " (forced)" : string.Empty) + " #" + track.TrackNumber;
    }

    private void MakeBatchItemsInfo()
    {
        if (BatchItems.Count == 0)
        {
            BatchItemsInfo = string.Empty;
        }
        else if (BatchItems.Count == 1)
        {
            BatchItemsInfo = Se.Language.General.OneFile;
        }
        else
        {
            BatchItemsInfo = string.Format(Se.Language.General.XFiles, BatchItems.Count);
        }
    }

    [RelayCommand]
    private async Task AutoTranslateBrowseModel()
    {
        var result = await _windowService.ShowDialogAsync<PickOllamaModelWindow, PickOllamaModelViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.General.PickOllamaModel, Se.Settings.AutoTranslate.OllamaModel, Se.Settings.AutoTranslate.OllamaUrl); });

        if (result is { OkPressed: true, SelectedModel: not null })
        {
            Se.Settings.AutoTranslate.OllamaModel = result.SelectedModel;
            SaveSettings();
        }
    }

    [RelayCommand]
    private async Task RemoveSelectedFiles()
    {
        var selected = SelectedBatchItem;
        if (selected == null || Window == null)
        {
            return;
        }

        if (Se.Settings.General.PromptDeleteLines)
        {
            var result = await MessageBox.Show(
                Window,
                Se.Language.General.Remove,
                Se.Language.General.RemoveSelectedFile,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var idx = BatchItems.IndexOf(selected);
        BatchItems.Remove(selected);
        if (BatchItems.Count > 0)
        {
            if (idx >= BatchItems.Count)
            {
                idx = BatchItems.Count - 1;
            }

            SelectedBatchItem = BatchItems[idx];
        }

        _allBatchItems.Remove(selected);

        MakeBatchItemsInfo();
    }

    [RelayCommand]
    private void OpenContainingFolder()
    {
        var selectedItem = SelectedBatchItem;
        if (selectedItem == null || Window == null)
        {
            return;
        }

        _folderHelper.OpenFolderWithFileSelected(Window!, selectedItem.FileName);
    }

    [RelayCommand]
    private void ClearAllFiles()
    {
        BatchItems.Clear();
        _allBatchItems.Clear();
        MakeBatchItemsInfo();
    }

    [RelayCommand]
    private async Task ShowOutputProperties()
    {
        await _windowService.ShowDialogAsync<BatchConvertSettingsWindow, BatchConvertSettingsViewModel>(Window!);
        UpdateOutputProperties();
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
            UiUtil.ShowHelp("features/batch-convert");
        }
    }

    private BatchConvertConfig MakeBatchConvertConfig()
    {
        var activeFunctions = BatchFunctions.Where(p => p.IsSelected).Select(p => p.Type).ToList();

        if (activeFunctions.Contains(BatchConvertFunctionType.AutoTranslate))
        {
            UpdateAutoTranslateSettings();
        }

        return new BatchConvertConfig
        {
            SaveInSourceFolder = Se.Settings.Tools.BatchConvert.SaveInSourceFolder,
            OutputFolder = Se.Settings.Tools.BatchConvert.OutputFolder,
            Overwrite = Se.Settings.Tools.BatchConvert.Overwrite,
            TargetFormatName = SelectedTargetFormat ?? string.Empty,
            TargetEncoding = Se.Settings.Tools.BatchConvert.TargetEncoding,
            AssaUseSourceStylesIfPossible = Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible,
            AssaHeader = Se.Settings.Tools.BatchConvert.AssaHeader,
            AssaFooter = Se.Settings.Tools.BatchConvert.AssaFooter,

            AdjustDuration = new BatchConvertConfig.AdjustDurationSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AdjustDisplayDuration),
                AdjustmentType = SelectedAdjustType.Type,
                Percentage = AdjustPercent,
                FixedMilliseconds = (int)AdjustFixed,
                MaxCharsPerSecond = (double)AdjustRecalculateMaxCharacterPerSecond,
                OptimalCharsPerSecond = (double)AdjustRecalculateOptimalCharacterPerSecond,
                Seconds = (double)AdjustSeconds,
            },

            AutoTranslate = new BatchConvertConfig.AutoTranslateSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AutoTranslate),
                Translator = SelectedAutoTranslator,
                SourceLanguage = SelectedSourceLanguage ?? SourceLanguages.First(),
                TargetLanguage = SelectedTargetLanguage ?? TargetLanguages.First(),
            },

            ChangeCasing = new BatchConvertConfig.ChangeCasingSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeCasing),

                NormalCasing = NormalCasing,
                NormalCasingOnlyUpper = NormalCasingOnlyUpper,
                NormalCasingFixNames = NormalCasingFixNames,
                FixNamesOnly = FixNamesOnly,
                AllLowercase = AllLowercase,
                AllUppercase = AllUppercase,
            },

            ChangeSpeed = new BatchConvertConfig.ChangeSpeedSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeSpeed),
                SpeedPercent = ChangeSpeedPercent,
            },

            ChangeFrameRate = new BatchConvertConfig.ChangeFrameRateSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ChangeFrameRate),
                FromFrameRate = SelectedFromFrameRate,
                ToFrameRate = SelectedToFrameRate,
            },

            DeleteLines = new BatchConvertConfig.DeleteLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.DeleteLines),
                DeleteXFirst = DeleteXFirstLines,
                DeleteXLast = DeleteXLastLines,
                DeleteContains = DeleteLinesContains,
                DeleteActorsOrStyles = DeleteActorsOrStyles,
            },

            FixCommonErrors = new BatchConvertConfig.FixCommonErrorsSettings2
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.FixCommonErrors),
                Profile = FixCommonErrorsProfile,
            },

            MergeLinesWithSameTexts = new BatchConvertConfig.MergeLinesWithSameTextsSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.MergeLinesWithSameText),
                IncludeIncrementingLines = MergeSameTextIncludeIncrementingLines,
                MaxMillisecondsBetweenLines = MergeSameTextMaxMillisecondsBetweenLines,
            },

            MergeLinesWithSameTimeCodes = new BatchConvertConfig.MergeLinesWithSameTimeCodesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.MergeLinesWithSameTimeCodes),
                MaxMillisecondsDifference = MergeSameTextMaxMillisecondsBetweenLines,
                MergeDialog = MergeSameTimeMergeDialog,
                AutoBreak = MergeSameTimeAutoBreak,
            },

            OffsetTimeCodes = new BatchConvertConfig.OffsetTimeCodesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.OffsetTimeCodes),
                Forward = OffsetTimeCodesForward,
                Milliseconds = (long)OffsetTimeCodesTime.TotalMilliseconds,
            },

            AddFormatting = new BatchConvertConfig.AddFormattingSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AddFormatting),
                AddItalic = FormattingAddItalic,
                AddBold = FormattingAddBold,
                AddUnderline = FormattingAddUnderline,
                AddColor = FormattingAddColor,
                AddColorValue = FormattingAddColorValue,
                AddAlignment = FormattingAddAlignmentTag,
                AddAlignmentValue = SelectedAlignmentTagOption?.Code ?? "an2",
            },

            RemoveFormatting = new BatchConvertConfig.RemoveFormattingSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.RemoveFormatting),
                RemoveAll = FormattingRemoveAll,
                RemoveItalic = FormattingRemoveItalic,
                RemoveBold = FormattingRemoveBold,
                RemoveUnderline = FormattingRemoveUnderline,
                RemoveColor = FormattingRemoveColors,
                RemoveFontName = FormattingRemoveFontTags,
                RemoveAlignment = FormattingRemoveAlignmentTags,
            },

            RightToLeft = new BatchConvertConfig.RightToLeftSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.FixRightToLeft),
                FixViaUnicode = RtlFixViaUniCode,
                RemoveUnicode = RtlRemoveUniCode,
                ReverseStartEnd = RtlReverseStartEnd,
            },

            BridgeGaps = new BatchConvertConfig.BridgeGapsSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.BridgeGaps),
                BridgeGapsSmallerThanMs = BridgeGapsSmallerThanMs,
                MinGapMs = BridgeGapsMinGapMs,
                PercentForLeft = BridgeGapsPercentForLeft,
            },

            ApplyMinGap = new BatchConvertConfig.ApplyMinGapSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ApplyMinGap),
                MinGapMs = MinGapMs,
            },

            SplitBreakLongLines = new BatchConvertConfig.SplitBreakLongLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.SplitBreakLongLines),
                SplitLongLines = SplitBreakSplitLongLines,
                RebalanceLongLines = SplitBreakRebalanceLongLines,
                MaxNumberOfLines = SplitBreakMaxNumberOfLines,
                SingleLineMaxLength = SplitBreakSingleLineMaxLength,
            },

            MultipleReplace = new BatchConvertConfig.MultipleReplaceSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.MultipleReplace),
            },
        };
    }

    private void UpdateAutoTranslateSettings()
    {
        var translator = SelectedAutoTranslator;
        if (translator == null)
        {
            return;
        }

        var engineType = translator.GetType();

        if (engineType == typeof(LibreTranslate))
        {
            if (!string.IsNullOrEmpty(AutoTranslateUrl.Trim()))
            {
                Configuration.Settings.Tools.AutoTranslateLibreUrl = AutoTranslateUrl.Trim();
            }
            else if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.LibreTranslateUrl))
            {
                Configuration.Settings.Tools.AutoTranslateLibreUrl = Se.Settings.AutoTranslate.LibreTranslateUrl;
            }

            if (!string.IsNullOrEmpty(AutoTranslateApiKey.Trim()))
            {
                Configuration.Settings.Tools.AutoTranslateLibreApiKey = AutoTranslateApiKey.Trim();
                Se.Settings.AutoTranslate.LibreTranslateApiKey = AutoTranslateApiKey.Trim();
            }
            else if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.LibreTranslateApiKey))
            {
                Configuration.Settings.Tools.AutoTranslateLibreApiKey = Se.Settings.AutoTranslate.LibreTranslateApiKey;
            }
        }

        if (engineType == typeof(LmStudioTranslate))
        {
            if (!string.IsNullOrEmpty(AutoTranslateUrl.Trim()))
            {
                Configuration.Settings.Tools.LmStudioApiUrl = AutoTranslateUrl.Trim();
            }
            else if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.LmStudioApiUrl))
            {
                Configuration.Settings.Tools.LmStudioApiUrl = Se.Settings.AutoTranslate.LmStudioApiUrl;
            }

            Configuration.Settings.Tools.LmStudioModel = AutoTranslateModel.Trim();
        }

        if (engineType == typeof(OllamaTranslate))
        {
            if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.OllamaUrl))
            {
                Configuration.Settings.Tools.OllamaApiUrl = Se.Settings.AutoTranslate.OllamaUrl;
            }

            if (!string.IsNullOrEmpty(AutoTranslateModel))
            {
                Configuration.Settings.Tools.OllamaModel = AutoTranslateModel.Trim();
                Se.Settings.AutoTranslate.OllamaModel = AutoTranslateModel.Trim();
            }
        }

        if (engineType == typeof(NoLanguageLeftBehindServe))
        {
            if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.NllbServeUrl))
            {
                Configuration.Settings.Tools.AutoTranslateNllbServeUrl = Se.Settings.AutoTranslate.NllbServeUrl;
            }
        }

        if (engineType == typeof(NoLanguageLeftBehindApi))
        {
            if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.NllbApiUrl))
            {
                Configuration.Settings.Tools.AutoTranslateNllbApiUrl = Se.Settings.AutoTranslate.NllbApiUrl;
            }
        }

        if (engineType == typeof(DeepLTranslate))
        {
            if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.DeepLUrl))
            {
                Configuration.Settings.Tools.AutoTranslateDeepLUrl = Se.Settings.AutoTranslate.DeepLUrl;
            }

            if (!string.IsNullOrEmpty(AutoTranslateApiKey))
            {
                Configuration.Settings.Tools.AutoTranslateDeepLApiKey = AutoTranslateApiKey.Trim();
                Se.Settings.AutoTranslate.DeepLApiKey = AutoTranslateApiKey.Trim();
            }
        }
    }

    internal void SelectedFunctionChanged()
    {
        var selectedFunction = SelectedBatchFunction;
        if (selectedFunction != null)
        {
            FunctionContainer.Content = selectedFunction.View;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var totalFunctionsSelected = 0;
            foreach (var function in BatchFunctions)
            {
                if (function.IsSelected)
                {
                    totalFunctionsSelected++;
                }
            }

            if (totalFunctionsSelected == 0)
            {
                ActionsSelected = string.Empty;
            }
            else if (totalFunctionsSelected == 1)
            {
                ActionsSelected = Se.Language.Tools.BatchConvert.OneActionsSelected;
            }
            else
            {
                ActionsSelected = string.Format(Se.Language.Tools.BatchConvert.XActionsSelected, totalFunctionsSelected);
            }
        });
    }

    private async Task ShowStatus(string statusText)
    {
        StatusText = statusText;
        var _ = Task.Run(async () =>
        {
            await Task.Delay(6000, _cancellationToken).ConfigureAwait(false);
            StatusText = string.Empty;
        }); 
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private Lock _addFileLock = new Lock();
    internal void FileGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Task.Run(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            lock (_addFileLock)
                            {
                                AddFile(path);
                                MakeBatchItemsInfo();
                            }
                        });
                    }
                }

                _isFilesDirty = true;
            });
        }
    }

    internal void OnAutoTranslatorChanged()
    {
        var engine = SelectedAutoTranslator;

        AutoTranslateModelIsVisible = engine is OllamaTranslate;

        if (engine is OllamaTranslate)
        {
            AutoTranslateModel = Se.Settings.AutoTranslate.OllamaModel;
            AutoTranslateModelBrowseIsVisible = true;
            AutoTranslateModelIsVisible = true;
            AutoTranslateUrl = string.Empty;
            AutoTranslateUrlIsVisible = false;
            AutoTranslateApiKey = string.Empty;
            AutoTranslateApiKeyIsVisible = false;
        }
        else if (engine is LibreTranslate)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = Se.Settings.AutoTranslate.LibreTranslateUrl;
            AutoTranslateUrlIsVisible = true;
            AutoTranslateApiKey = Se.Settings.AutoTranslate.LibreTranslateApiKey;
            AutoTranslateApiKeyIsVisible = true;
        }
        else if (engine is LmStudioTranslate)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = Se.Settings.AutoTranslate.LmStudioApiUrl;
            AutoTranslateUrlIsVisible = true;
            AutoTranslateApiKey = string.Empty;
            AutoTranslateApiKeyIsVisible = false;
        }
        else if (engine is NoLanguageLeftBehindServe)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = Se.Settings.AutoTranslate.NnlbServeUrl;
            AutoTranslateUrlIsVisible = true;
            AutoTranslateApiKey = string.Empty;
            AutoTranslateApiKeyIsVisible = false;
        }
        else if (engine is NoLanguageLeftBehindApi)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = Se.Settings.AutoTranslate.NnlbApiUrl;
            AutoTranslateUrlIsVisible = true;
            AutoTranslateApiKey = string.Empty;
            AutoTranslateApiKeyIsVisible = false;
        }
        else if (engine is DeepLTranslate)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = Se.Settings.AutoTranslate.DeepLUrl;
            AutoTranslateUrlIsVisible = true;
            AutoTranslateApiKey = Se.Settings.AutoTranslate.DeepLApiKey;
            AutoTranslateApiKeyIsVisible = true;
        }
        else
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = string.Empty;
            AutoTranslateUrlIsVisible = false;
            AutoTranslateApiKey = string.Empty;
            AutoTranslateApiKeyIsVisible = false;
        }

        UpdateSourceLanguages(engine);
        UpdateTargetLanguages(engine);
    }

    private void UpdateSourceLanguages(IAutoTranslator autoTranslator)
    {
        UpdateSourceLanguages(autoTranslator, SourceLanguages);
    }

    private void UpdateSourceLanguages(IAutoTranslator autoTranslator, ObservableCollection<TranslationPair> sourceLanguages)
    {
        SourceLanguages.Clear();
        if (autoTranslator == null)
        {
            return;
        }

        SourceLanguages.Add(new TranslationPair(" - " + Se.Language.General.Autodetect + " - ", "auto"));

        foreach (var language in autoTranslator.GetSupportedSourceLanguages())
        {
            SourceLanguages.Add(language);
        }

        SelectedSourceLanguage = null;
        var sourceLanguageIsoCode = AutoTranslateViewModel.EvaluateDefaultSourceLanguageCode(null, new Subtitle(), sourceLanguages);
        if (!string.IsNullOrEmpty(sourceLanguageIsoCode))
        {
            var lang = SourceLanguages.FirstOrDefault(p => p.Code == sourceLanguageIsoCode);
            if (lang != null)
            {
                SelectedSourceLanguage = lang;
            }
        }

        if (SelectedSourceLanguage == null && !string.IsNullOrEmpty(Se.Settings.AutoTranslate.AutoTranslateLastSource))
        {
            var lang = SourceLanguages.FirstOrDefault(p => p.Code == Se.Settings.AutoTranslate.AutoTranslateLastSource);
            if (lang != null)
            {
                SelectedSourceLanguage = lang;
            }
        }

        if (SelectedSourceLanguage == null && SourceLanguages.Count > 0)
        {
            SelectedSourceLanguage = SourceLanguages[0];
        }
    }

    private void UpdateTargetLanguages(IAutoTranslator autoTranslator)
    {
        TargetLanguages.Clear();
        if (autoTranslator == null)
        {
            return;
        }

        foreach (var language in autoTranslator.GetSupportedTargetLanguages())
        {
            TargetLanguages.Add(language);
        }

        SelectedTargetLanguage = null;
        var targetLanguageIsoCode = AutoTranslateViewModel.EvaluateDefaultTargetLanguageCode(SelectedTargetLanguage?.Code ?? string.Empty, SelectedSourceLanguage?.Code ?? string.Empty);
        if (!string.IsNullOrEmpty(targetLanguageIsoCode))
        {
            var lang = TargetLanguages.FirstOrDefault(p => p.Code == targetLanguageIsoCode);
            if (lang != null)
            {
                SelectedTargetLanguage = lang;
            }
        }

        if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.AutoTranslateLastTarget))
        {
            var lang = TargetLanguages.FirstOrDefault(p => p.Code == Se.Settings.AutoTranslate.AutoTranslateLastTarget);
            if ((SelectedSourceLanguage == null || lang == null || SelectedSourceLanguage.Code != lang.Code) && lang != null)
            {
                SelectedTargetLanguage = lang;
            }
        }

        if (SelectedTargetLanguage == null && TargetLanguages.Count > 0)
        {
            SelectedTargetLanguage = TargetLanguages[0];
        }

        if (SelectedSourceLanguage == SelectedTargetLanguage && TargetLanguages.Count > 1)
        {
            if (SelectedSourceLanguage?.Code == "en")
            {
                SelectedTargetLanguage = TargetLanguages.FirstOrDefault(p => p.Code == "de");
            }
            else
            {
                SelectedTargetLanguage = TargetLanguages.FirstOrDefault(p => p.Code == "en");
            }
        }
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
        ComboBoxSubtitleFormatChanged();
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }

    internal void ComboBoxSubtitleFormatChanged()
    {
        var selectedFormat = SelectedTargetFormat ?? string.Empty;
        IsTargetFormatSettingsVisible = _targetFormatsWithSettings.Contains(selectedFormat);
    }

    internal void FilterComboBoxChanged()
    {
        var selection = SelectedFilterItem;
        if (selection == Se.Language.General.AllFiles)
        {
            IsFilterTextVisible = false;
        }
        else
        {
            IsFilterTextVisible = true;
        }

        _isFilesDirty = true;
    }

    internal void FilterTextChanged()
    {
        _isFilesDirty = true;
    }

    internal void FileGridContextMenuOpening()
    {
        IsRemoveVisible = SelectedBatchItem != null;
        IsOpenContainingFolderVisible = SelectedBatchItem != null;
    }

    internal void ComboBoxSubtitleFormatPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await ShowSubtitleFormatPicker();
        });
    }
}
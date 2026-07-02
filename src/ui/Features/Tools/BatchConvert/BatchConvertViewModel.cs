using Nikse.SubtitleEdit.UiLogic.Export;
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
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Files.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
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
    [ObservableProperty] private bool _isAddingFiles;
    [ObservableProperty] private string _addingFilesStatus;
    [ObservableProperty] private double _addingFilesProgressValue;
    [ObservableProperty] private double _addingFilesProgressMax;
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

    // Remove line breaks
    [ObservableProperty] private bool _removeLineBreaksOnlyShortLines;

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
    [ObservableProperty] private ObservableCollection<SpeechToTextModelDisplay> _crispAsrModels = new();
    [ObservableProperty] private SpeechToTextModelDisplay? _selectedCrispAsrModel;
    [ObservableProperty] private bool _crispAsrModelComboIsVisible;

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

    // ASSA change resolution
    [ObservableProperty] private int _assaChangeResolutionTargetWidth;
    [ObservableProperty] private int _assaChangeResolutionTargetHeight;
    [ObservableProperty] private bool _assaChangeResolutionChangeMargins;
    [ObservableProperty] private bool _assaChangeResolutionChangeFontSize;
    [ObservableProperty] private bool _assaChangeResolutionChangePosition;
    [ObservableProperty] private bool _assaChangeResolutionChangeDrawing;

    // ASSA change style
    [ObservableProperty] private string _assaChangeStyleFromStyle;
    [ObservableProperty] private string _assaChangeStyleToStyle;
    [ObservableProperty] private string _assaChangeStyleImportFileName;
    [ObservableProperty] private string _assaChangeStyleImportedStyleHeader;
    [ObservableProperty] private bool _assaChangeStyleTrimUnusedStyles;

    // Merge short lines
    [ObservableProperty] private int _mergeShortLinesMaxCharacters;
    [ObservableProperty] private int _mergeShortLinesMaxMillisecondsBetweenLines;
    [ObservableProperty] private bool _mergeShortLinesOnlyContinuationLines;

    // Apply duration limits
    [ObservableProperty] private bool _applyDurationLimitsFixMin;
    [ObservableProperty] private int _applyDurationLimitsMinDurationMs;
    [ObservableProperty] private bool _applyDurationLimitsFixMax;
    [ObservableProperty] private int _applyDurationLimitsMaxDurationMs;

    // Sort by
    [ObservableProperty] private ObservableCollection<SortByOption> _sortByOptions;
    [ObservableProperty] private SortByOption? _selectedSortByOption;
    [ObservableProperty] private bool _sortByDescending;

    public Window? Window { get; set; }
    public DataGrid FileGrid { get; set; } = new();

    public bool OkPressed { get; private set; }
    public ScrollViewer FunctionContainer { get; internal set; }

    public string EbuHeader { get; private set; } = string.Empty;
    public byte EbuJustificationCode { get; private set; } = 2;

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
    private CancellationTokenSource _addFilesCancellationTokenSource = new();
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
        AddingFilesStatus = string.Empty;
        ProgressText = string.Empty;
        ActionsSelected = string.Empty;
        DeleteLinesContains = string.Empty;
        OutputFolderLabel = string.Empty;
        OutputFolderLinkLabel = string.Empty;
        OutputEncodingLabel = string.Empty;
        StatusText = string.Empty;
        DeleteActorsOrStyles = string.Empty;
        OffsetTimeCodesForward = true;
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

        SortByOptions = new ObservableCollection<SortByOption>
        {
            new("Number", Se.Language.Tools.SortBy.SortByNumber),
            new("StartTime", Se.Language.Tools.SortBy.SortByStartTime),
            new("EndTime", Se.Language.Tools.SortBy.SortByEndTime),
        };
        SelectedSortByOption = SortByOptions[0];

        BatchFunctions = new ObservableCollection<BatchConvertFunction>(BatchConvertFunction.List(this));

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _encodings = EncodingHelper.GetEncodings().Select(p => p.DisplayName).ToList();
        _encodings.Insert(0, EncodingHelper.TryToUseSourceEncoding);

        AutoTranslateModel = string.Empty;
        AutoTranslateUrl = string.Empty;
        AutoTranslateApiKey = string.Empty;
        AutoTranslators =
        [
            new OllamaTranslate(),
            new LibreTranslate(),
            new LmStudioTranslate(),
            new LlamaCppTranslate(),
            new NoLanguageLeftBehindServe(),
            new NoLanguageLeftBehindApi(),
            new DeepLTranslate(),
            new CrispAsrMadladTranslate(),
        ];
        SelectedAutoTranslator = AutoTranslators[0];
        OnAutoTranslatorChanged();

        SelectedFromFrameRate = FromFrameRates[0];
        SelectedToFrameRate = ToFrameRates[1];

        ChangeSpeedPercent = 100;

        AssaChangeResolutionTargetWidth = 1920;
        AssaChangeResolutionTargetHeight = 1080;
        AssaChangeResolutionChangeMargins = true;
        AssaChangeResolutionChangeFontSize = true;
        AssaChangeResolutionChangePosition = true;
        AssaChangeResolutionChangeDrawing = true;

        AssaChangeStyleFromStyle = string.Empty;
        AssaChangeStyleToStyle = string.Empty;
        AssaChangeStyleImportFileName = string.Empty;
        AssaChangeStyleImportedStyleHeader = string.Empty;
        AssaChangeStyleTrimUnusedStyles = false;

        FixCommonErrorsProfile = LoadDefaultProfile();

        _targetFormatsWithSettings = new List<string>
        {
            BatchConverter.FormatBdnXml,
            BatchConverter.FormatBluRaySup,
            BatchConverter.FormatCustomTextFormat,
            BatchConverter.FormatDostImage,
            BatchConverter.FormatEbuStl,
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
        foreach (var item in _allBatchItems)
        {
            if (PassesFilter(item))
            {
                BatchItems.Add(item);
            }
        }
    }

    private bool PassesFilter(BatchConvertItem item)
    {
        if (SelectedFilterItem == Se.Language.Tools.BatchConvert.FileNameContainsDotDotDot && !string.IsNullOrEmpty(FilterText))
        {
            return item.FileName.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase);
        }

        if (SelectedFilterItem == Se.Language.Tools.BatchConvert.TrackLanguageContainsDotDotDot && !string.IsNullOrEmpty(FilterText))
        {
            return item.Format.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase);
        }

        return true;
    }

    // Appends just-parsed items to the visible grid (respecting the active filter) so files show
    // up incrementally as they load. Must run on the UI thread.
    private void AddFilteredItems(IEnumerable<BatchConvertItem> items)
    {
        foreach (var item in items)
        {
            if (PassesFilter(item))
            {
                BatchItems.Add(item);
            }
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

        Se.Settings.Tools.BatchConvert.ActiveFunctions = BatchFunctions
            .Where(p => p.IsSelected)
            .Select(p => p.Type.ToString())
            .ToArray();

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

        Se.Settings.Tools.BatchConvert.NormalCasingFixNames = NormalCasingFixNames;
        Se.Settings.Tools.BatchConvert.NormalCasingOnlyUpper = NormalCasingOnlyUpper;

        // Offset time codes
        Se.Settings.Tools.BatchConvert.OffsetTimeCodesMilliseconds = OffsetTimeCodesTime.TotalMilliseconds;
        Se.Settings.Tools.BatchConvert.OffsetTimeCodesForward = OffsetTimeCodesForward;

        // Change frame rate
        Se.Settings.Tools.BatchConvert.ChangeFrameRateFrom = SelectedFromFrameRate;
        Se.Settings.Tools.BatchConvert.ChangeFrameRateTo = SelectedToFrameRate;

        // Change speed
        Se.Settings.Tools.BatchConvert.ChangeSpeedPercent = ChangeSpeedPercent;

        // Delete lines
        Se.Settings.Tools.BatchConvert.DeleteXFirstLines = DeleteXFirstLines;
        Se.Settings.Tools.BatchConvert.DeleteXLastLines = DeleteXLastLines;
        Se.Settings.Tools.BatchConvert.DeleteLinesContains = DeleteLinesContains ?? string.Empty;
        Se.Settings.Tools.BatchConvert.DeleteActorsOrStyles = DeleteActorsOrStyles ?? string.Empty;

        // Add formatting
        Se.Settings.Tools.BatchConvert.FormattingAddItalic = FormattingAddItalic;
        Se.Settings.Tools.BatchConvert.FormattingAddBold = FormattingAddBold;
        Se.Settings.Tools.BatchConvert.FormattingAddUnderline = FormattingAddUnderline;
        Se.Settings.Tools.BatchConvert.FormattingAddAlignmentTag = FormattingAddAlignmentTag;
        Se.Settings.Tools.BatchConvert.FormattingAddAlignmentTagOption = SelectedAlignmentTagOption?.Code ?? "an2";
        Se.Settings.Tools.BatchConvert.FormattingAddColor = FormattingAddColor;
        Se.Settings.Tools.BatchConvert.FormattingAddColorValue = FormattingAddColorValue.ToString();

        // Remove line breaks
        Se.Settings.Tools.BatchConvert.RemoveLineBreaksOnlyShortLines = RemoveLineBreaksOnlyShortLines;

        // ASSA change resolution
        Se.Settings.Tools.BatchConvert.AssaChangeResolutionTargetWidth = AssaChangeResolutionTargetWidth;
        Se.Settings.Tools.BatchConvert.AssaChangeResolutionTargetHeight = AssaChangeResolutionTargetHeight;
        Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangeMargins = AssaChangeResolutionChangeMargins;
        Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangeFontSize = AssaChangeResolutionChangeFontSize;
        Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangePosition = AssaChangeResolutionChangePosition;
        Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangeDrawing = AssaChangeResolutionChangeDrawing;

        // ASSA change style
        Se.Settings.Tools.BatchConvert.AssaChangeStyleFromStyle = AssaChangeStyleFromStyle ?? string.Empty;
        Se.Settings.Tools.BatchConvert.AssaChangeStyleToStyle = AssaChangeStyleToStyle ?? string.Empty;
        Se.Settings.Tools.BatchConvert.AssaChangeStyleTrimUnusedStyles = AssaChangeStyleTrimUnusedStyles;

        // Merge short lines
        Se.Settings.Tools.BatchConvert.MergeShortLinesMaxCharacters = MergeShortLinesMaxCharacters;
        Se.Settings.Tools.BatchConvert.MergeShortLinesMaxMillisecondsBetweenLines = MergeShortLinesMaxMillisecondsBetweenLines;
        Se.Settings.Tools.BatchConvert.MergeShortLinesOnlyContinuationLines = MergeShortLinesOnlyContinuationLines;

        // Apply duration limits
        Se.Settings.Tools.BatchConvert.ApplyDurationLimitsFixMinDuration = ApplyDurationLimitsFixMin;
        Se.Settings.Tools.BatchConvert.ApplyDurationLimitsMinDurationMs = ApplyDurationLimitsMinDurationMs;
        Se.Settings.Tools.BatchConvert.ApplyDurationLimitsFixMaxDuration = ApplyDurationLimitsFixMax;
        Se.Settings.Tools.BatchConvert.ApplyDurationLimitsMaxDurationMs = ApplyDurationLimitsMaxDurationMs;

        // Sort by
        Se.Settings.Tools.BatchConvert.SortBy = SelectedSortByOption?.Key ?? "Number";
        Se.Settings.Tools.BatchConvert.SortByDescending = SortByDescending;

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

        // Offset time codes
        OffsetTimeCodesTime = TimeSpan.FromMilliseconds(Se.Settings.Tools.BatchConvert.OffsetTimeCodesMilliseconds);
        OffsetTimeCodesForward = Se.Settings.Tools.BatchConvert.OffsetTimeCodesForward;
        OffsetTimeCodesBack = !OffsetTimeCodesForward;

        // Change frame rate — match against the available list so binding picks up the value.
        var fromRate = FromFrameRates.FirstOrDefault(p => Math.Abs(p - Se.Settings.Tools.BatchConvert.ChangeFrameRateFrom) < 0.001);
        if (fromRate > 0)
        {
            SelectedFromFrameRate = fromRate;
        }
        var toRate = ToFrameRates.FirstOrDefault(p => Math.Abs(p - Se.Settings.Tools.BatchConvert.ChangeFrameRateTo) < 0.001);
        if (toRate > 0)
        {
            SelectedToFrameRate = toRate;
        }

        // Change speed
        ChangeSpeedPercent = Se.Settings.Tools.BatchConvert.ChangeSpeedPercent;

        // Delete lines
        DeleteXFirstLines = Se.Settings.Tools.BatchConvert.DeleteXFirstLines;
        DeleteXLastLines = Se.Settings.Tools.BatchConvert.DeleteXLastLines;
        DeleteLinesContains = Se.Settings.Tools.BatchConvert.DeleteLinesContains ?? string.Empty;
        DeleteActorsOrStyles = Se.Settings.Tools.BatchConvert.DeleteActorsOrStyles ?? string.Empty;

        // Add formatting
        FormattingAddItalic = Se.Settings.Tools.BatchConvert.FormattingAddItalic;
        FormattingAddBold = Se.Settings.Tools.BatchConvert.FormattingAddBold;
        FormattingAddUnderline = Se.Settings.Tools.BatchConvert.FormattingAddUnderline;
        FormattingAddAlignmentTag = Se.Settings.Tools.BatchConvert.FormattingAddAlignmentTag;
        var alignment = AlignmentTagOptions.FirstOrDefault(p => p.Code == Se.Settings.Tools.BatchConvert.FormattingAddAlignmentTagOption);
        if (alignment != null)
        {
            SelectedAlignmentTagOption = alignment;
        }
        FormattingAddColor = Se.Settings.Tools.BatchConvert.FormattingAddColor;
        if (Color.TryParse(Se.Settings.Tools.BatchConvert.FormattingAddColorValue, out var color))
        {
            FormattingAddColorValue = color;
        }

        // Remove line breaks
        RemoveLineBreaksOnlyShortLines = Se.Settings.Tools.BatchConvert.RemoveLineBreaksOnlyShortLines;

        // ASSA change resolution
        AssaChangeResolutionTargetWidth = Se.Settings.Tools.BatchConvert.AssaChangeResolutionTargetWidth;
        AssaChangeResolutionTargetHeight = Se.Settings.Tools.BatchConvert.AssaChangeResolutionTargetHeight;
        AssaChangeResolutionChangeMargins = Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangeMargins;
        AssaChangeResolutionChangeFontSize = Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangeFontSize;
        AssaChangeResolutionChangePosition = Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangePosition;
        AssaChangeResolutionChangeDrawing = Se.Settings.Tools.BatchConvert.AssaChangeResolutionChangeDrawing;

        // ASSA change style
        AssaChangeStyleFromStyle = Se.Settings.Tools.BatchConvert.AssaChangeStyleFromStyle ?? string.Empty;
        AssaChangeStyleToStyle = Se.Settings.Tools.BatchConvert.AssaChangeStyleToStyle ?? string.Empty;
        AssaChangeStyleTrimUnusedStyles = Se.Settings.Tools.BatchConvert.AssaChangeStyleTrimUnusedStyles;

        // Merge short lines
        MergeShortLinesMaxCharacters = Se.Settings.Tools.BatchConvert.MergeShortLinesMaxCharacters;
        MergeShortLinesMaxMillisecondsBetweenLines = Se.Settings.Tools.BatchConvert.MergeShortLinesMaxMillisecondsBetweenLines;
        MergeShortLinesOnlyContinuationLines = Se.Settings.Tools.BatchConvert.MergeShortLinesOnlyContinuationLines;

        // Apply duration limits
        ApplyDurationLimitsFixMin = Se.Settings.Tools.BatchConvert.ApplyDurationLimitsFixMinDuration;
        ApplyDurationLimitsMinDurationMs = Se.Settings.Tools.BatchConvert.ApplyDurationLimitsMinDurationMs;
        ApplyDurationLimitsFixMax = Se.Settings.Tools.BatchConvert.ApplyDurationLimitsFixMaxDuration;
        ApplyDurationLimitsMaxDurationMs = Se.Settings.Tools.BatchConvert.ApplyDurationLimitsMaxDurationMs;

        // Sort by
        var savedSortBy = SortByOptions.FirstOrDefault(p => p.Key == Se.Settings.Tools.BatchConvert.SortBy);
        if (savedSortBy != null)
        {
            SelectedSortByOption = savedSortBy;
        }
        SortByDescending = Se.Settings.Tools.BatchConvert.SortByDescending;
    }

    private void UpdateOutputProperties()
    {
        var targetEncoding =
            _encodings.FirstOrDefault(p => p == Se.Settings.Tools.BatchConvert.TargetEncoding);
        if (targetEncoding == null)
        {
            targetEncoding = _encodings.FirstOrDefault(p => p == TextEncoding.Utf8WithBom)
                ?? _encodings.First();
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

        if (!await EnsurePaddleOcrAvailable(config))
        {
            return;
        }

        if (!await EnsureCrispAsrAvailable(config))
        {
            return;
        }

        if (!await EnsureLlamaCppAvailable(config))
        {
            return;
        }

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

    private async Task<bool> EnsurePaddleOcrAvailable(BatchConvertConfig config)
    {
        if (Window == null || Configuration.IsRunningOnMac)
        {
            return true;
        }

        if (config.IsTargetFormatImageBased)
        {
            return true;
        }

        if (!Se.Settings.Tools.BatchConvert.OcrEngine.Equals("PaddleOCR", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!BatchItems.Any(IsImageBasedInput))
        {
            return true;
        }

        if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
        {
            var answer = await MessageBox.Show(
                Window,
                "Download Paddle OCR?",
                $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "GPU CUDA 11",
                "GPU CUDA 12");

            if (answer == MessageBoxResult.Cancel)
            {
                return false;
            }

            var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window,
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

            if (!result.OkPressed)
            {
                return false;
            }
        }
        else if (Configuration.IsRunningOnLinux && !File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin")))
        {
            var answer = await MessageBox.Show(
                Window,
                "Download Paddle OCR?",
                $"{Environment.NewLine}\"Paddle OCR\" requires downloading Paddle OCR.{Environment.NewLine}{Environment.NewLine}Download and use Paddle OCR?",
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "GPU CUDA");

            if (answer == MessageBoxResult.Cancel)
            {
                return false;
            }

            var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window,
                vm =>
                {
                    vm.Initialize(answer == MessageBoxResult.Custom1
                        ? PaddleOcrDownloadType.EngineCpuLinux
                        : PaddleOcrDownloadType.EngineGpuLinux);
                });

            if (!result.OkPressed)
            {
                return false;
            }
        }

        var modelsDirectory = Se.PaddleOcrModelsFolder;
        if (!Directory.Exists(modelsDirectory))
        {
            var result = await _windowService.ShowDialogAsync<DownloadPaddleOcrWindow, DownloadPaddleOcrViewModel>(Window,
                vm => { vm.Initialize(PaddleOcrDownloadType.Models); });

            if (!result.OkPressed)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> EnsureCrispAsrAvailable(BatchConvertConfig config)
    {
        if (Window == null)
        {
            return true;
        }

        if (!config.AutoTranslate.IsActive || config.AutoTranslate.Translator is not CrispAsrMadladTranslate)
        {
            return true;
        }

        var ready = await CrispAsrTranslateDownloadHelper.EnsureReadyAsync(Window, _windowService, SelectedCrispAsrModel?.Model.Name);
        if (ready)
        {
            SelectedCrispAsrModel = CrispAsrTranslateDownloadHelper.PopulateModels(
                CrispAsrModels,
                Path.GetFileName(Se.Settings.AutoTranslate.CrispAsrModel ?? string.Empty));
        }

        return ready;
    }

    // For llama.cpp in local (server-managed) mode: auto-detect an already-running llama-server and
    // reuse it, otherwise auto-download the engine + model (prompting) and auto-start the server, so
    // batch translation works without opening the interactive Auto-translate window first. Remote mode
    // (a user-supplied URL) is left untouched.
    private async Task<bool> EnsureLlamaCppAvailable(BatchConvertConfig config)
    {
        if (Window == null)
        {
            return true;
        }

        if (!config.AutoTranslate.IsActive || config.AutoTranslate.Translator is not LlamaCppTranslate)
        {
            return true;
        }

        // Remote mode: the user pointed llama.cpp at their own running llama-server. Detect it the same
        // way the interactive Auto-translate window does - via the LlamaCppUseRemoteServer flag - not by
        // whether AutoTranslateUrl is set, since that is pre-filled with the default localhost URL and so
        // is never empty (which previously short-circuited the whole auto-start path).
        if (Se.Settings.AutoTranslate.LlamaCppUseRemoteServer)
        {
            return true;
        }

        // Auto-detect: reuse an already-running local server.
        if (LlamaCppServerManager.IsServerRunning)
        {
            Configuration.Settings.Tools.LlamaCppApiUrl = LlamaCppServerManager.ApiUrl;
            return true;
        }

        // Pick the last-used model, else the first available translate model.
        var models = LlamaCppServerManager.GetAllTranslateModels();
        var configuredName = Path.GetFileName(Se.Settings.AutoTranslate.LlamaCppModel ?? string.Empty);
        var model = models.FirstOrDefault(m => string.Equals(m.FileName, configuredName, StringComparison.OrdinalIgnoreCase))
                    ?? models.FirstOrDefault();
        if (model == null)
        {
            return true;
        }

        // Auto-download the llama-server binary + model if missing (prompts).
        if (!await LlamaCppDownloadHelper.EnsureReadyAsync(Window, _windowService, model.FileName))
        {
            return false;
        }

        // Auto-start the local server - this points Configuration.Settings.Tools.LlamaCppApiUrl at it.
        try
        {
            await LlamaCppServerManager.EnsureServerRunningAsync(model, _cancellationToken);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return true;
    }

    private static bool IsImageBasedInput(BatchConvertItem item)
    {
        if (item.Format == null)
        {
            return false;
        }

        if (item.Format == BatchConverter.FormatBluRaySup ||
            item.Format == BatchConverter.FormatBdnXml ||
            item.Format == BatchConverter.FormatVobSub)
        {
            return true;
        }

        if (item.Format.StartsWith("Matroska", StringComparison.Ordinal) &&
            (item.Format.Contains("S_VOBSUB", StringComparison.OrdinalIgnoreCase) ||
             item.Format.Contains("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase) ||
             item.Format.Contains("S_DVBSUB", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (item.Format.StartsWith("Transport Stream", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
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
    private async Task AssaChangeStyleBrowseFromStyle()
    {
        var name = await PickStyleNameAsync();
        if (!string.IsNullOrEmpty(name))
        {
            AssaChangeStyleFromStyle = name;
        }
    }

    [RelayCommand]
    private async Task AssaChangeStyleBrowseToStyle()
    {
        var name = await PickStyleNameAsync();
        if (!string.IsNullOrEmpty(name))
        {
            AssaChangeStyleToStyle = name;
        }
    }

    [RelayCommand]
    private async Task AssaChangeStyleImportStyle()
    {
        if (Window == null)
        {
            return;
        }

        var format = new AdvancedSubStationAlpha();
        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Assa.OpenStyleImportFile, format.Name, "*" + format.Extension);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var s = Subtitle.Parse(fileName, format);
        if (s == null || string.IsNullOrEmpty(s.Header))
        {
            return;
        }

        AssaChangeStyleImportedStyleHeader = s.Header;
        AssaChangeStyleImportFileName = Path.GetFileName(fileName);

        if (string.IsNullOrEmpty(AssaChangeStyleToStyle))
        {
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(s.Header);
            if (styles.Count > 0)
            {
                AssaChangeStyleToStyle = styles[0].Name;
            }
        }
    }

    private async Task<string> PickStyleNameAsync()
    {
        if (Window == null)
        {
            return string.Empty;
        }

        var header = !string.IsNullOrEmpty(AssaChangeStyleImportedStyleHeader)
            ? AssaChangeStyleImportedStyleHeader
            : AdvancedSubStationAlpha.DefaultHeader;

        var ssaStyles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
        if (ssaStyles.Count == 0)
        {
            return string.Empty;
        }

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.Styles, ssaStyles.Select(p => new StyleDisplay(p)).ToList(), Se.Language.General.Ok, false);
        });

        if (!result.OkPressed || result.SelectedStyle == null)
        {
            return string.Empty;
        }

        return result.SelectedStyle.Name;
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

            // Remember which custom format was chosen so batch convert uses it (not just the first one).
            if (result.OkPressed && result.SelectedCustomFormat != null)
            {
                Se.Settings.Tools.BatchConvert.CustomTextFormatName = result.SelectedCustomFormat.Name;
            }

            return;
        }

        // check for Advadvanced SubStation Alpha
        if (targetFormat == AdvancedSubStationAlpha.NameOfFormat)
        {
            var result = await _windowService.ShowDialogAsync<BatchConvertAssaWindow, BatchConvertAssaViewModel>(Window);
            return;
        }

        if (targetFormat == BatchConverter.FormatEbuStl)
        {
            var result = await _windowService.ShowDialogAsync<ExportEbuStlWindow, ExportEbuStlViewModel>(Window,
                vm => { vm.Initialize(new Subtitle()); });

            if (result.OkPressed)
            {
                EbuHeader = result.Subtitle.Header ?? string.Empty;
                EbuJustificationCode = result.JustificationCode;
            }
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

        await AddFilesAsync(fileNames);
    }

    [RelayCommand]
    private void CancelAddFiles()
    {
        _addFilesCancellationTokenSource.Cancel();
    }

    private async Task AddFilesAsync(IReadOnlyList<string> fileNames)
    {
        // Parsing can be slow (probing/loading each file), so do it off the UI thread and post
        // each file's result back so rows appear incrementally. Only show the "please wait"
        // overlay (with current file name + cancel) when adding more than one file.
        _addFilesCancellationTokenSource = new CancellationTokenSource();
        var token = _addFilesCancellationTokenSource.Token;
        AddingFilesProgressMax = fileNames.Count;
        AddingFilesProgressValue = 0;
        IsAddingFiles = fileNames.Count > 1;

        await Task.Run(() =>
        {
            lock (_addFileLock)
            {
                var number = 0;
                foreach (var fileName in fileNames)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    number++;
                    var current = number;
                    Dispatcher.UIThread.Post(() => AddingFilesStatus = string.Format("{0}/{1}: {2}", current, fileNames.Count, Path.GetFileName(fileName)));
                    var added = AddFile(fileName);
                    Dispatcher.UIThread.Post(() =>
                    {
                        AddFilteredItems(added);
                        AddingFilesProgressValue = current;
                        MakeBatchItemsInfo();
                    });
                }
            }
        });

        IsAddingFiles = false;
        AddingFilesStatus = string.Empty;
    }

    [RelayCommand]
    private async Task ShowSubtitleFormatPicker()
    {
        if (Window == null)
        {
            return;
        }

        var currentRealFormat = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(p => p.Name == SelectedTargetFormat);
        var extraFormats = BuildExtraFormatPickerEntries();

        var viewModel = await _windowService.ShowDialogAsync<PickSubtitleFormatWindow, PickSubtitleFormatViewModel>(Window, vm =>
        {
            vm.Initialize(currentRealFormat ?? new SubRip(), new Subtitle(), extraFormats,
                currentRealFormat == null ? SelectedTargetFormat : null);
        });

        if (viewModel.OkPressed)
        {
            // Real formats round-trip through GetSelectedFormat().Name; the batch-only ones are only
            // identified by their (display) name.
            var selectedName = viewModel.GetSelectedFormat()?.Name ?? viewModel.SelectedSubtitleFormatName;
            if (!string.IsNullOrEmpty(selectedName) && selectedName != SelectedTargetFormat)
            {
                SelectedTargetFormat = selectedName;
            }
        }
    }

    // The batch-only output formats (image-based + plain/custom text) are not real SubtitleFormats, so
    // they must be added to the format picker explicitly. Text formats get a real preview; image-based
    // ones pass null (the picker shows a "no text preview" placeholder).
    private static IReadOnlyList<PickSubtitleFormatExtraFormat> BuildExtraFormatPickerEntries()
    {
        var sample = new List<Paragraph>
        {
            new("Hello, World!", 1000, 3000),
            new("This is a sample subtitle.", 3500, 6000),
        };

        var plainTextPreview = string.Join(Environment.NewLine + Environment.NewLine, sample.Select(p => p.Text));

        string? customTextPreview = null;
        var customFormats = Se.Settings.File.ExportCustomFormats.Select(c => new CustomFormatItem(c)).ToList();
        var selectedCustom = customFormats.FirstOrDefault(c => c.Name == Se.Settings.Tools.BatchConvert.CustomTextFormatName)
                             ?? customFormats.FirstOrDefault();
        if (selectedCustom != null)
        {
            customTextPreview = Nikse.SubtitleEdit.UiLogic.Export.CustomTextFormatter.GenerateCustomText(
                selectedCustom.ToTemplate(), sample, "Sample", string.Empty);
        }

        return new List<PickSubtitleFormatExtraFormat>
        {
            new(BatchConverter.FormatBluRaySup, null),
            new(BatchConverter.FormatVobSub, null),
            new(BatchConverter.FormatBdnXml, null),
            new(BatchConverter.FormatDostImage, null),
            new(BatchConverter.FormatFcpImage, null),
            new(BatchConverter.FormatDCinemaInterop, null),
            new(BatchConverter.FormatDCinemaSmpte2014, null),
            new(BatchConverter.FormatImagesWithTimeCodesInFileName, null),
            new(BatchConverter.FormatPlainText, plainTextPreview),
            new(BatchConverter.FormatCustomTextFormat, customTextPreview),
        };
    }

    // Parses a file and appends the resulting item(s) to _allBatchItems only (no UI-bound
    // collection touched), so it is safe to call from a background thread. Returns the items
    // added for this file; callers add them to the visible BatchItems on the UI thread.
    private List<BatchConvertItem> AddFile(string fileName)
    {
        var added = new List<BatchConvertItem>();
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
                            _allBatchItems.Add(matroskaBatchItem);
                            added.Add(matroskaBatchItem);
                        }
                    }

                    if (tracksByFormat.Count == 0)
                    {
                        format = "No subtitle tracks";
                    }

                    return added;
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
                _allBatchItems.Add(mp4BatchItem);
                added.Add(mp4BatchItem);
            }

            foreach (var track in mp4SubtitleTracks)
            {
                if (track.Mdia.IsTextSubtitle || track.Mdia.IsClosedCaption)
                {
                    var name = $"MP4/#{mp4SubtitleTracks.IndexOf(track)} {track.Mdia.HandlerType} - {track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString}";
                    mp4Files.Add(name);
                    var mp4BatchItem = new BatchConvertItem(fileName, fileInfo.Length, name, subtitle);
                    mp4BatchItem.LanguageCode = track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString;
                    _allBatchItems.Add(mp4BatchItem);
                    added.Add(mp4BatchItem);
                }
            }

            if (mp4Files.Count <= 0)
            {
                format = "No subtitle tracks";
            }

            return added;
        }
        else if ((ext == ".ts" || ext == ".m2ts" || ext == ".mts" || ext == ".mpg" || ext == ".mpeg") &&
                 (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
        {
            format = "Transport Stream";
            var tsBatchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
            _allBatchItems.Add(tsBatchItem);
            added.Add(tsBatchItem);
            return added;
        }

        if (format == Se.Language.General.Unknown && fileInfo.Length < 20_000_000)
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

        if (format == Se.Language.General.Unknown && fileInfo.Length < 20_000_000)
        {
            subtitle = Subtitle.Parse(fileName);
            if (subtitle != null)
            {
                format = subtitle.OriginalFormat.Name;
            }
        }

        var batchItem = new BatchConvertItem(fileName, fileInfo.Length, format, subtitle);
        _allBatchItems.Add(batchItem);
        added.Add(batchItem);
        return added;
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
            AutoTranslateModel = result.SelectedModel;
            Se.Settings.AutoTranslate.OllamaModel = result.SelectedModel;
            SaveSettings();
        }
    }

    [RelayCommand]
    private async Task RemoveSelectedFiles()
    {
        if (Window == null)
        {
            return;
        }

        var selectedItems = FileGrid.SelectedItems.Cast<BatchConvertItem>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (Se.Settings.General.PromptBeforeDelete)
        {
            var message = selectedItems.Count > 1
                ? string.Format(Se.Language.General.RemoveSelectedFilesX, selectedItems.Count)
                : Se.Language.General.RemoveSelectedFile;

            var result = await MessageBox.Show(
                Window,
                Se.Language.General.Remove,
                message,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var idx = selectedItems.Min(item => BatchItems.IndexOf(item));
        foreach (var item in selectedItems)
        {
            BatchItems.Remove(item);
            _allBatchItems.Remove(item);
        }

        if (BatchItems.Count > 0)
        {
            if (idx >= BatchItems.Count)
            {
                idx = BatchItems.Count - 1;
            }

            SelectedBatchItem = BatchItems[idx];
        }

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

    [RelayCommand]
    private void SelectAll()
    {
        foreach (BatchConvertFunction batchConvertFunction in BatchFunctions)
        {
            batchConvertFunction.IsSelected = true;
        }
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (BatchConvertFunction batchConvertFunction in BatchFunctions)
        {
            batchConvertFunction.IsSelected = !batchConvertFunction.IsSelected;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (BatchConvertFunction batchConvertFunction in BatchFunctions)
        {
            batchConvertFunction.IsSelected = false;
        }
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
            EbuHeader = EbuHeader,
            EbuJustificationCode = EbuJustificationCode,

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

            RemoveTextForHearingImpaired = new BatchConvertConfig.RemoveTextForHearingImpairedSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.RemoveTextForHearingImpaired),
            },

            RemoveLineBreaks = new BatchConvertConfig.RemoveLineBreaksSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.RemoveLineBreaks),
                OnlyShortLines = RemoveLineBreaksOnlyShortLines,
            },

            AssaChangeResolution = new BatchConvertConfig.AssaChangeResolutionSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AssaChangeResolution),
                TargetWidth = AssaChangeResolutionTargetWidth,
                TargetHeight = AssaChangeResolutionTargetHeight,
                ChangeMargins = AssaChangeResolutionChangeMargins,
                ChangeFontSize = AssaChangeResolutionChangeFontSize,
                ChangePosition = AssaChangeResolutionChangePosition,
                ChangeDrawing = AssaChangeResolutionChangeDrawing,
            },

            AssaChangeStyle = new BatchConvertConfig.AssaChangeStyleSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AssaChangeStyle),
                FromStyle = AssaChangeStyleFromStyle ?? string.Empty,
                ToStyle = AssaChangeStyleToStyle ?? string.Empty,
                ImportedStyleHeader = AssaChangeStyleImportedStyleHeader ?? string.Empty,
                TrimUnusedStyles = AssaChangeStyleTrimUnusedStyles,
            },

            MergeShortLines = new BatchConvertConfig.MergeShortLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.MergeShortLines),
                MaxCharacters = MergeShortLinesMaxCharacters,
                MaxMillisecondsBetweenLines = MergeShortLinesMaxMillisecondsBetweenLines,
                OnlyContinuationLines = MergeShortLinesOnlyContinuationLines,
            },

            ApplyDurationLimits = new BatchConvertConfig.ApplyDurationLimitsSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.ApplyDurationLimits),
                FixMinDurationMs = ApplyDurationLimitsFixMin,
                MinDurationMs = ApplyDurationLimitsMinDurationMs,
                FixMaxDurationMs = ApplyDurationLimitsFixMax,
                MaxDurationMs = ApplyDurationLimitsMaxDurationMs,
            },

            AutoBalanceLines = new BatchConvertConfig.AutoBalanceLinesSettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.AutoBalanceLines),
            },

            SortBy = new BatchConvertConfig.SortBySettings
            {
                IsActive = activeFunctions.Contains(BatchConvertFunctionType.SortBy),
                SortBy = SelectedSortByOption?.Key ?? "Number",
                Descending = SortByDescending,
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

        if (engineType == typeof(LlamaCppTranslate))
        {
            if (!string.IsNullOrEmpty(AutoTranslateUrl.Trim()))
            {
                Configuration.Settings.Tools.LlamaCppApiUrl = AutoTranslateUrl.Trim();
            }
            else if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.LlamaCppApiUrl))
            {
                Configuration.Settings.Tools.LlamaCppApiUrl = Se.Settings.AutoTranslate.LlamaCppApiUrl;
            }
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
        if (files == null)
        {
            return;
        }

        var paths = files
            .Select(f => f.Path?.LocalPath)
            .Where(p => p != null && File.Exists(p))
            .Select(p => p!)
            .ToList();
        if (paths.Count == 0)
        {
            return;
        }

        _ = AddFilesAsync(paths);
    }

    partial void OnSelectedCrispAsrModelChanged(SpeechToTextModelDisplay? value)
    {
        if (value == null)
        {
            return;
        }

        Se.Settings.AutoTranslate.CrispAsrModel = new CrispAsrMadlad().GetModelForCmdLine(value.Model.Name);
        Configuration.Settings.Tools.AutoTranslateCrispAsrModel = Se.Settings.AutoTranslate.CrispAsrModel;
    }

    internal void OnAutoTranslatorChanged()
    {
        var engine = SelectedAutoTranslator;

        AutoTranslateModelIsVisible = engine is OllamaTranslate;
        CrispAsrModelComboIsVisible = engine is CrispAsrMadladTranslate;

        if (engine is CrispAsrMadladTranslate)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = string.Empty;
            AutoTranslateUrlIsVisible = false;
            AutoTranslateApiKey = string.Empty;
            AutoTranslateApiKeyIsVisible = false;
            SelectedCrispAsrModel = CrispAsrTranslateDownloadHelper.PopulateModels(
                CrispAsrModels,
                Path.GetFileName(Se.Settings.AutoTranslate.CrispAsrModel ?? string.Empty));
        }
        else if (engine is OllamaTranslate)
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
        else if (engine is LlamaCppTranslate)
        {
            AutoTranslateModel = string.Empty;
            AutoTranslateModelBrowseIsVisible = false;
            AutoTranslateModelIsVisible = false;
            AutoTranslateUrl = Se.Settings.AutoTranslate.LlamaCppApiUrl;
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

        // Persist target format and function options also when the window is closed via
        // X/Escape - especially important in /batchconvertui mode, where this window is the
        // main window and nothing else flushes settings to disk on exit (#11699).
        SaveSettings();
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
        IsRemoveVisible = FileGrid.SelectedItems.Count > 0;
        IsOpenContainingFolderVisible = FileGrid.SelectedItems.Count == 1;
    }

    internal void ComboBoxSubtitleFormatPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Open the format picker on right-click (or Mac Ctrl+left-click), like the main window's
        // format combo; a plain left-click still opens the normal dropdown.
        var props = e.GetCurrentPoint(null).Properties;
        var isMacCtrlLeftClick = OperatingSystem.IsMacOS()
                                 && props.IsLeftButtonPressed
                                 && e.KeyModifiers.HasFlag(KeyModifiers.Control)
                                 && !e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (props.IsRightButtonPressed || isMacCtrlLeftClick)
        {
            Dispatcher.UIThread.Post(async () => { await ShowSubtitleFormatPicker(); });
            e.Handled = true;
        }
    }
}
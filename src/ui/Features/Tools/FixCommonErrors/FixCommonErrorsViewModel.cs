using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language.Tools;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsViewModel : ObservableObject, IFixCallbacks
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<LanguageDisplayItem> _languages;
    [ObservableProperty] private LanguageDisplayItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<FixDisplayItem> _fixes;
    [ObservableProperty] private ObservableCollection<FixDisplayItem> _visibleFixes;
    [ObservableProperty] private ObservableCollection<FixFilterChip> _fixChips;
    [ObservableProperty] private string _fixesSelectAllText;
    [ObservableProperty] private string _applySelectedFixesText;
    [ObservableProperty] private FixDisplayItem? _selectedFix;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _paragraphs;
    [ObservableProperty] private SubtitleLineViewModel? _selectedParagraph;
    [ObservableProperty] private ObservableCollection<ProfileDisplayItem> _profiles;
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;
    [ObservableProperty] private bool _step1IsVisible;
    [ObservableProperty] private bool _step2IsVisible;
    [ObservableProperty] private bool _tryToGuessUnknownWords;
    [ObservableProperty] private string _step2Title;
    [ObservableProperty] private string _fixesAppliedText = string.Empty;
    [ObservableProperty] private string _editTextTotalLength = string.Empty;
    [ObservableProperty] private IBrush _editTextTotalLengthBackground = Brushes.Transparent;

    public StackPanel? PanelSingleLineLengths { get; set; }

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public SubtitleFormat Format { get; set; } = new SubRip();
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public string Language { get; set; } = "en";
    public DataGrid GridSubtitles { get; internal set; }

    public Subtitle FixedSubtitle = new();

    private List<FixRuleDisplayItem> _allFixRules = new();
    private readonly LanguageFixCommonErrors _language;
    private bool _previewMode = true;
    public List<int> DeleteIndices = new();
    private List<FixDisplayItem> _oldFixes = new();
    private LanguageDisplayItem _oldSelectedLanguage;
    private int _totalErrors;
    private int _totalFixes;
    private SubtitleFormat _subtitleFormat;
    private readonly INamesList _namesList;
    private readonly IWindowService _windowService;
    private readonly IOcrFixEngine _ocrFixEngine;

    public FixCommonErrorsViewModel(INamesList namesList, IWindowService windowService, IOcrFixEngine ocrFixEngine)
    {
        _namesList = namesList;
        _windowService = windowService;
        _ocrFixEngine = ocrFixEngine;

        GridSubtitles = new DataGrid();
        SearchText = string.Empty;
        Languages = new ObservableCollection<LanguageDisplayItem>();
        Language = new string(' ', 0);
        Fixes = new ObservableCollection<FixDisplayItem>();
        VisibleFixes = new ObservableCollection<FixDisplayItem>();
        FixChips = new ObservableCollection<FixFilterChip>();
        FixesSelectAllText = Se.Language.General.SelectAll;
        ApplySelectedFixesText = Se.Language.Tools.FixCommonErrors.ApplySelectedFixes;
        Paragraphs = new ObservableCollection<SubtitleLineViewModel>();
        _language = Se.Language.Tools.FixCommonErrors;
        Step1IsVisible = true;
        _oldSelectedLanguage = new LanguageDisplayItem(new CultureInfo("en"), "English");
        _subtitleFormat = new SubRip();
        Profiles = new ObservableCollection<ProfileDisplayItem>();
        Step2Title = Se.Language.Tools.FixCommonErrors.FixCommonOcrErrorsStep2;
        FixCommonOcrErrors.OcrFixEngine = _ocrFixEngine;
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        _subtitleFormat = subtitleFormat;
        var languages = new List<LanguageDisplayItem>();
        foreach (var ci in Utilities.GetSubtitleLanguageCultures(true))
        {
            languages.Add(new LanguageDisplayItem(ci, ci.EnglishName));
        }

        Languages = new ObservableCollection<LanguageDisplayItem>(
            LanguageFavoritesHelper.Order(languages.OrderBy(p => p.ToString()), p => p.Code.TwoLetterISOLanguageName));

        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
        Language = languageCode;

        SelectedLanguage = Languages.FirstOrDefault(p => p.Code.TwoLetterISOLanguageName == languageCode);
        if (SelectedLanguage == null)
        {
            SelectedLanguage = Languages.First(p => p.Code.TwoLetterISOLanguageName == "en");
        }

        Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;
        Configuration.Settings.General.MaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
        Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds = Se.Settings.General.SubtitleOptimalCharactersPerSeconds;
        Configuration.Settings.General.MinimumMillisecondsBetweenLines = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();

        InitStep1(languageCode, subtitle);
        LoadProfiles();
        TryToGuessUnknownWords = Se.Settings.Tools.FixCommonErrors.TryToGuessUnknownWords;
        SelectedProfile = Profiles.FirstOrDefault(p => p.Name == Se.Settings.Tools.FixCommonErrors.LastProfileName) ?? Profiles.FirstOrDefault();

        if (Se.Settings.Tools.FixCommonErrors.SkipStep1 && SelectedProfile != null)
        {
            ShowStep2();
        }
    }

    private void SaveProfiles()
    {
        Se.Settings.Tools.FixCommonErrors.Profiles.Clear();
        foreach (var profile in Profiles)
        {
            var setting = new SeFixCommonErrorsProfile()
            {
                ProfileName = profile.Name,
                SelectedRules = new List<string>()
            };

            foreach (var rule in profile.FixRules)
            {
                if (rule.IsSelected)
                {
                    setting.SelectedRules.Add(rule.FixCommonErrorFunctionName);
                }
            }

            Se.Settings.Tools.FixCommonErrors.Profiles.Add(setting);
        }

        Se.Settings.Tools.FixCommonErrors.LastProfileName = SelectedProfile?.Name ?? Profiles.FirstOrDefault()?.Name ?? "Default";
    }

    private void LoadProfiles()
    {
        Profiles.Clear();
        var profiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        foreach (var setting in profiles)
        {
            var profile = new ProfileDisplayItem()
            {
                Name = setting.ProfileName,
                FixRules = new ObservableCollection<FixRuleDisplayItem>(_allFixRules.Select(rule => new FixRuleDisplayItem(rule)
                {
                    IsSelected = setting.SelectedRules.Contains(rule.FixCommonErrorFunctionName)
                }))
            };

            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private async Task ShowProfile()
    {
        SaveProfiles();

        var result = await _windowService.ShowDialogAsync<FixCommonErrorsProfileWindow, FixCommonErrorsProfileViewModel>(Window!,
            vm => { vm.Initialize(_allFixRules, SelectedProfile?.Name); });

        if (result.OkPressed)
        {
            LoadProfiles();
            var profile = Profiles.FirstOrDefault(p => p.Name == result.SelectedProfile?.Name);
            SelectedProfile = profile ?? Profiles.FirstOrDefault();
        }
    }

    [RelayCommand]
    public void DoRefreshFixes()
    {
        RefreshFixes();
    }

    [RelayCommand]
    private void DoApplyFixes()
    {
        _previewMode = false;
        ApplyFixes();

        RefreshFixes();
        FixesAppliedText = string.Format(_language.XFixesApplied, _totalFixes);
    }

    [RelayCommand]
    private void BackToFixList()
    {
        Step2IsVisible = false;
        Step1IsVisible = true;
    }

    private void ShowStep2()
    {
        Step1IsVisible = false;
        Step2IsVisible = true;
        _oldSelectedLanguage = SelectedLanguage!;
        _totalFixes = 0;
        FixesAppliedText = string.Empty;
        RefreshFixes();
    }

    [RelayCommand]
    private void ToApplyFixes()
    {
        SaveProfiles();
        ShowStep2();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Se.SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    public void RulesSelectAll()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        foreach (var rule in SelectedProfile.FixRules)
        {
            rule.IsSelected = true;
        }
    }

    [RelayCommand]
    public void RulesInverseSelected()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        foreach (var rule in SelectedProfile.FixRules)
        {
            rule.IsSelected = !rule.IsSelected;
        }
    }

    // Select all / none toggle. Operates on the fixes passing the active category chip, not the
    // full list - with a category chip active it must not touch fixes hidden by the filter, and
    // with the "All" chip active VisibleFixes equals Fixes, so it covers everything there. When
    // anything in the view is still unticked it selects all of it, otherwise it clears the view,
    // so one button both selects and deselects the current category (#12377).
    [RelayCommand]
    public void FixesSelectAll()
    {
        var selectAll = VisibleFixes.Any(f => !f.IsSelected);
        foreach (var fix in VisibleFixes)
        {
            fix.IsSelected = selectAll;
        }

        UpdateFixesSummary();
    }

    // ---------- action filter chips / summary (styled like the AI review window) ----------

    private static readonly Color[] ChipPalette =
    {
        Color.FromRgb(0xe8, 0xb0, 0x4c), // amber
        Color.FromRgb(0xb4, 0x8c, 0xe8), // violet
        Color.FromRgb(0x5f, 0xc6, 0xd8), // cyan
        Color.FromRgb(0xe8, 0x8c, 0xb0), // pink
        Color.FromRgb(0x6e, 0xcb, 0x87), // green
        Color.FromRgb(0x4c, 0x9c, 0xe8), // blue
        Color.FromRgb(0xe8, 0x8a, 0x5a), // orange
        Color.FromRgb(0x9a, 0xa3, 0xad), // gray
    };

    private readonly Dictionary<string, int> _actionPaletteIndex = new();

    public IBrush GetActionBrush(string actionDisplay)
    {
        return new SolidColorBrush(GetActionColor(actionDisplay));
    }

    public IBrush GetActionBackgroundBrush(string actionDisplay)
    {
        var c = GetActionColor(actionDisplay);
        return new SolidColorBrush(Color.FromArgb(0x20, c.R, c.G, c.B));
    }

    private Color GetActionColor(string actionDisplay)
    {
        if (!_actionPaletteIndex.TryGetValue(actionDisplay, out var index))
        {
            index = _actionPaletteIndex.Count;
            _actionPaletteIndex[actionDisplay] = index;
        }

        return ChipPalette[index % ChipPalette.Length];
    }

    private void AddFix(FixDisplayItem item)
    {
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FixDisplayItem.IsSelected))
            {
                UpdateFixesSummary();
            }
        };

        Fixes.Add(item);
        if (PassesFixFilter(item))
        {
            VisibleFixes.Add(item);
        }
    }

    private bool PassesFixFilter(FixDisplayItem item)
    {
        var active = FixChips.FirstOrDefault(c => c.IsActive);
        return active?.Action == null || active.Action == item.ActionDisplay;
    }

    private void RebuildFixChips()
    {
        var activeAction = FixChips.FirstOrDefault(c => c.IsActive)?.Action;

        FixChips.Clear();
        FixChips.Add(new FixFilterChip { Action = null, Label = Se.Language.General.All, Count = Fixes.Count });
        foreach (var group in Fixes.GroupBy(f => f.ActionDisplay).OrderByDescending(g => g.Count()))
        {
            FixChips.Add(new FixFilterChip { Action = group.Key, Label = group.Key, Count = group.Count() });
        }

        var toActivate = FixChips.FirstOrDefault(c => c.Action == activeAction) ?? FixChips[0];
        toActivate.IsActive = true;

        RebuildVisibleFixes();
        UpdateFixesSummary();
    }

    [RelayCommand]
    private void SetFixFilter(FixFilterChip chip)
    {
        foreach (var c in FixChips)
        {
            c.IsActive = c == chip;
        }

        RebuildVisibleFixes();
        // Refresh the "N fixes, M selected" line so it reflects the newly active category,
        // matching what Select all and Invert now act on (#12377, follow-up to #12408).
        UpdateFixesSummary();
    }

    private void RebuildVisibleFixes()
    {
        VisibleFixes.Clear();
        foreach (var item in Fixes)
        {
            if (PassesFixFilter(item))
            {
                VisibleFixes.Add(item);
            }
        }
    }

    private void UpdateFixesSummary()
    {
        // Count within the active category view so the toggle and chip figures match what
        // Select all now acts on (#12377); with "All" active VisibleFixes equals Fixes.
        var selected = VisibleFixes.Count(f => f.IsSelected);

        // Flip the toggle caption so the button says what its next click will do for the
        // current category: "Select none" once everything in view is ticked, else "Select all".
        var allVisibleSelected = VisibleFixes.Count > 0 && selected == VisibleFixes.Count;
        FixesSelectAllText = allVisibleSelected ? Se.Language.General.SelectNone : Se.Language.General.SelectAll;

        // The Apply button acts on every ticked fix across all categories, not just the visible
        // ones, so show that global count in its caption to remove the "does it apply the whole
        // list or only this tab" ambiguity raised on #12377.
        var totalSelected = Fixes.Count(f => f.IsSelected);
        ApplySelectedFixesText = $"{Se.Language.Tools.FixCommonErrors.ApplySelectedFixes} ({totalSelected})";

        UpdateChipSelectedCounts();
    }

    // Keep each category chip's "selected / total" figure current so every tab shows how many
    // of its own fixes are ticked, not just how many exist (#12377).
    private void UpdateChipSelectedCounts()
    {
        foreach (var chip in FixChips)
        {
            chip.SelectedCount = chip.Action == null
                ? Fixes.Count(f => f.IsSelected)
                : Fixes.Count(f => f.ActionDisplay == chip.Action && f.IsSelected);
        }
    }

    [RelayCommand]
    public void ApplySelectedFixes()
    {
        _previewMode = false;
        ApplyFixes();

        RefreshFixes();
    }

    partial void OnSelectedParagraphChanged(SubtitleLineViewModel? oldValue, SubtitleLineViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= SelectedParagraph_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += SelectedParagraph_PropertyChanged;
            UpdateSubtitleTextInfo(newValue);
        }
        else
        {
            EditTextTotalLength = string.Empty;
            EditTextTotalLengthBackground = Brushes.Transparent;
            PanelSingleLineLengths?.Children.Clear();
        }
    }

    partial void OnTryToGuessUnknownWordsChanged(bool value)
    {
        Se.Settings.Tools.FixCommonErrors.TryToGuessUnknownWords = value;

        // Re-scan so the fix list reflects the toggle right away; while step 1 is up there is no list yet.
        if (Step2IsVisible)
        {
            RefreshFixes();
        }
    }

    partial void OnSelectedLanguageChanged(LanguageDisplayItem? value)
    {
        // Language drives which OCR-fix list, names list, and per-language rules are loaded;
        // without this sync a user picking e.g. Croatian after auto-detect chose English would
        // still get the English replacements (GH #10940).
        if (value != null)
        {
            Language = value.Code.TwoLetterISOLanguageName;
        }
    }

    private void SelectedParagraph_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not SubtitleLineViewModel vm || vm.Paragraph == null)
        {
            return;
        }

        if (e.PropertyName == nameof(SubtitleLineViewModel.Text))
        {
            vm.Paragraph.Text = vm.Text;
            UpdateSubtitleTextInfo(vm);
        }
    }

    private void UpdateGaps()
    {
        try { SubtitleTextInfoHelper.UpdateGaps(Paragraphs); }
        catch { }
    }

    private void UpdateSubtitleTextInfo(SubtitleLineViewModel item)
    {
        if (PanelSingleLineLengths == null) return;
        var info = SubtitleTextInfoHelper.PopulateLineLengthsAndTotal(item.Text ?? string.Empty, PanelSingleLineLengths);
        EditTextTotalLength = info.TotalText;
        EditTextTotalLengthBackground = info.TotalBackground;
    }

    private void RefreshFixes()
    {
        _oldFixes = new List<FixDisplayItem>(Fixes);
        Fixes.Clear();
        VisibleFixes.Clear();
        _previewMode = true;
        ApplyFixes();
    }

    private void ApplyFixes()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        _totalErrors = 0;

        var subtitle = _previewMode ? new Subtitle(FixedSubtitle, false) : FixedSubtitle;
        foreach (var paragraph in subtitle.Paragraphs)
        {
            paragraph.Text = string.Join(Environment.NewLine, paragraph.Text.SplitToLines());
        }

        foreach (var fix in SelectedProfile.FixRules)
        {
            if (fix.IsSelected)
            {
                var fixCommonError = fix.GetFixCommonErrorFunction();
                fixCommonError.Fix(subtitle, this);
            }
        }

        Paragraphs.Clear();
        Paragraphs.AddRange(FixedSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, _subtitleFormat) { IsCpsColumnVisible = false }));
        UpdateGaps();

        Step2Title = string.Format(Se.Language.Tools.FixCommonErrors.FixCommonOcrErrorsStep2FixesFoundX, Fixes.Count);
        RebuildFixChips();
    }

    private void InitStep1(string languageCode, Subtitle subtitle)
    {
        FixedSubtitle = new Subtitle(subtitle, false);

        Se.ApplyContinuationStyleToLibSe();

        _allFixRules = MakeDefaultRules();

        if (Configuration.Settings.General.ContinuationStyle == ContinuationStyle.None)
        {
            _allFixRules.Add(
                new FixRuleDisplayItem(_language.FixEllipsesStart, _language.FixEllipsesStartExample, 1,
                    true, nameof(FixEllipsesStart)));
        }

        if (languageCode == "en")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixLowercaseIToUppercaseI,
                _language.FixLowercaseIToUppercaseIExample, 1, true, nameof(FixAloneLowercaseIToUppercaseI)));
        }

        if (languageCode == "tr")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixTurkishAnsi,
                "Ý > İ, Ð > Ğ, Þ > Ş, ý > ı, ð > ğ, þ > ş", 1, true, nameof(FixTurkishAnsiToUnicode)));
        }

        if (languageCode == "da")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixDanishLetterI,
                "Jeg synes i er søde. -> Jeg synes I er søde.", 1, true, nameof(FixDanishLetterI)));
        }

        if (languageCode == "es")
        {
            _allFixRules.Add(new FixRuleDisplayItem(_language.FixSpanishInvertedQuestionAndExclamationMarks,
                "Hablas bien castellano? -> ¿Hablas bien castellano?", 1, true,
                nameof(FixSpanishInvertedQuestionAndExclamationMarks)));
        }
    }

    public static List<FixRuleDisplayItem> MakeDefaultRules()
    {
        var language = Se.Language.Tools.FixCommonErrors;

        FixEmptyLines.Language.RemovedEmptyLine = language.RemovedEmptyLine;
        FixEmptyLines.Language.RemovedEmptyLineAtBottom = language.RemovedEmptyLineAtBottom;
        FixEmptyLines.Language.RemovedEmptyLineAtTop = language.RemovedEmptyLineAtTop;
        FixEmptyLines.Language.RemovedEmptyLineInMiddle = language.RemovedEmptyLineInMiddle;
        FixEmptyLines.Language.RemovedEmptyLinesUnusedLineBreaks = language.RemovedEmptyLinesUnusedLineBreaks;

        FixOverlappingDisplayTimes.Language.UnableToFixTextXY = language.UnableToFixTextXY;
        FixOverlappingDisplayTimes.Language.FixOverlappingDisplayTime = language.FixOverlappingDisplayTime;
        FixOverlappingDisplayTimes.Language.FixOverlappingDisplayTimes = language.FixOverlappingDisplayTimes;
        FixOverlappingDisplayTimes.Language.StartTimeLaterThanEndTime = language.StartTimeLaterThanEndTime;
        FixOverlappingDisplayTimes.Language.UnableToFixStartTimeLaterThanEndTime = language.UnableToFixStartTimeLaterThanEndTime;
        FixOverlappingDisplayTimes.Language.XFixedToYZ = language.XFixedToYZ;

        FixShortDisplayTimes.Language.FixShortDisplayTime = language.FixShortDisplayTime;
        FixShortDisplayTimes.Language.FixShortDisplayTimes = language.FixShortDisplayTimes;
        FixShortDisplayTimes.Language.UnableToFixTextXY = language.UnableToFixTextXY;

        FixLongDisplayTimes.Language.FixLongDisplayTime = language.FixLongDisplayTime;

        FixShortGaps.Language.FixShortGaps = language.FixShortGaps;
        FixShortGaps.Language.FixShortGaps = language.FixShortGaps;

        FixInvalidItalicTags.Language.FixInvalidItalicTags = language.FixInvalidItalicTags;
        FixInvalidItalicTags.Language.FixInvalidItalicTag = language.FixInvalidItalicTag;

        FixUnneededSpaces.Language.RemoveUnneededSpaces = language.RemoveUnneededSpaces;
        FixUnneededSpaces.Language.UnneededSpace = language.UnneededSpace;

        FixMissingSpaces.Language.FixMissingSpaces = language.FixMissingSpaces;
        FixMissingSpaces.Language.FixMissingSpace = language.FixMissingSpace;

        FixUnneededPeriods.Language.RemoveUnneededPeriods = language.RemoveUnneededPeriods;
        FixUnneededPeriods.Language.UnneededPeriod = language.UnneededPeriod;

        FixCommas.Language.FixCommas = language.FixCommas;

        FixLongLines.Language.UnableToFixTextXY = language.UnableToFixTextXY;
        FixLongLines.Language.BreakLongLine = language.BreakLongLine;
        FixLongLines.Language.BreakLongLines = language.BreakLongLines;

        FixShortLines.Language.MergeShortLine = language.MergeShortLine;

        FixShortLinesAll.Language.MergeShortLineAll = language.MergeShortLineAll;
        FixShortLinesAll.Language.RemoveLineBreaks = language.RemoveLineBreaks;

        FixShortLinesPixelWidth.Language.RemoveLineBreaks = language.RemoveLineBreaks;
        FixShortLinesPixelWidth.Language.UnbreakShortLine = language.UnbreakShortLine;

        FixDoubleApostrophes.Language.FixDoubleApostrophes = language.FixDoubleApostrophes;

        FixMusicNotation.Language.FixMusicNotation = language.FixMusicNotation;

        FixMissingPeriodsAtEndOfLine.Language.AddPeriods = language.AddPeriods;
        FixMissingPeriodsAtEndOfLine.Language.FixMissingPeriodAtEndOfLine = language.FixMissingPeriodAtEndOfLine;

        FixStartWithUppercaseLetterAfterParagraph.Language.FixFirstLetterToUppercaseAfterParagraph = language.FixFirstLetterToUppercaseAfterParagraph;

        FixStartWithUppercaseLetterAfterPeriodInsideParagraph.Language.StartWithUppercaseLetterAfterPeriodInsideParagraph =
            language.StartWithUppercaseLetterAfterPeriodInsideParagraph;

        FixStartWithUppercaseLetterAfterColon.Language.StartWithUppercaseLetterAfterColon = language.StartWithUppercaseLetterAfterColon;

        AddMissingQuotes.Language.AddMissingQuote = language.AddMissingQuotes;

        FixDialogsOnOneLine.Language.FixDialogsOnOneLine = language.FixDialogsOnOneLine;

        FixHyphensInDialog.Language.FixHyphensInDialogs = language.FixHyphensInDialogs;

        FixHyphensRemoveDashSingleLine.Language.RemoveHyphensSingleLine = language.RemoveHyphensSingleLine;

        Fix3PlusLines.Language.Fix3PlusLine = language.Fix3PlusLine;
        Fix3PlusLines.Language.Fix3PlusLines = language.Fix3PlusLines;

        FixDoubleDash.Language.FixDoubleDash = language.FixDoubleDash;

        FixDoubleGreaterThan.Language.FixDoubleGreaterThan = language.FixDoubleGreaterThan;

        FixContinuationStyle.Language.FixUnnecessaryLeadingDots = language.FixUnnecessaryLeadingDots;

        FixMissingOpenBracket.Language.FixMissingOpenBracket = language.FixMissingOpenBracket;

        FixCommonOcrErrors.Language.FixText = language.FixText;

        FixUppercaseIInsideWords.Language.FixUppercaseIInsideLowercaseWord = language.FixUppercaseIInsideLowercaseWord;
        FixUppercaseIInsideWords.Language.FixUppercaseIInsideLowercaseWords = language.FixUppercaseIInsideLowercaseWords;

        RemoveSpaceBetweenNumbers.Language.RemoveSpaceBetweenNumber = language.RemoveSpaceBetweenNumbers;

        RemoveDialogFirstLineInNonDialogs.Language.RemoveDialogFirstInNonDialogs = language.RemoveDialogFirstInNonDialogs;

        NormalizeStrings.Language.NormalizeStrings = language.NormalizeStrings;

        return new List<FixRuleDisplayItem>
        {
            new(language.RemovedEmptyLinesUnusedLineBreaks, language.RemovedEmptyLinesUnusedLineBreaksExample, 1, true, nameof(FixEmptyLines)),
            new(language.FixOverlappingDisplayTimes, string.Empty, 1, true, nameof(FixOverlappingDisplayTimes)),
            new(language.FixShortDisplayTimes, string.Empty, 1, true, nameof(FixShortDisplayTimes)),
            new(language.FixLongDisplayTimes, string.Empty, 1, true, nameof(FixLongDisplayTimes)),
            new(language.FixShortGaps, string.Empty, 1, true, nameof(FixShortGaps)),
            new(language.FixInvalidItalicTags, language.FixInvalidItalicTagsExample, 1, true, nameof(FixInvalidItalicTags)),
            new(language.RemoveUnneededSpaces, language.RemoveUnneededSpacesExample, 1, true, nameof(FixUnneededSpaces)),
            new(language.FixMissingSpaces, language.FixMissingSpacesExample, 1, true, nameof(FixMissingSpaces)),
            new(language.RemoveUnneededPeriods, language.RemoveUnneededPeriodsExample, 1, true, nameof(FixUnneededPeriods)),
            new(language.FixCommas, language.FixCommasExample, 1, true, nameof(FixCommas)),
            new(language.BreakLongLines, string.Empty, 1, true, nameof(FixLongLines)),
            new(language.RemoveLineBreaks, language.RemoveLineBreaksExample, 1, true, nameof(FixShortLines)),
            new(language.RemoveLineBreaksAll, string.Empty, 1, true, nameof(FixShortLinesAll)),
            new(language.RemoveLineBreaksPixelWidth, string.Empty, 1, true, nameof(FixShortLinesPixelWidth)),
            new(language.FixDoubleApostrophes, language.FixDoubleApostrophesExample, 1, true, nameof(FixDoubleApostrophes)),
            new(language.FixMusicNotation, language.FixMusicNotationExample, 1, true, nameof(FixMusicNotation)),
            new(language.AddPeriods, language.AddPeriodsExample, 1, true, nameof(FixMissingPeriodsAtEndOfLine)),
            new(language.StartWithUppercaseLetterAfterParagraph, language.StartWithUppercaseLetterAfterParagraphExample, 1, true,
                nameof(FixStartWithUppercaseLetterAfterParagraph)),
            new(language.StartWithUppercaseLetterAfterPeriodInsideParagraph, language.StartWithUppercaseLetterAfterPeriodInsideParagraphExample, 1, true,
                nameof(FixStartWithUppercaseLetterAfterPeriodInsideParagraph)),
            new(language.StartWithUppercaseLetterAfterColon, language.StartWithUppercaseLetterAfterColonExample, 1, true, nameof(FixStartWithUppercaseLetterAfterColon)),
            new(language.AddMissingQuotes, language.AddMissingQuotesExample, 1, true, nameof(AddMissingQuotes)),
            new(language.BreakDialogsOnOneLine, language.FixDialogsOneLineExample, 1, true, nameof(FixDialogsOnOneLine)),
            new(string.Format(language.FixHyphensInDialogs, GetDialogStyle(Configuration.Settings.General.DialogStyle)), string.Empty, 1, true, nameof(FixHyphensInDialog)),
            new(language.RemoveHyphensSingleLine, language.RemoveHyphensSingleLineExample, 1, true, nameof(FixHyphensRemoveDashSingleLine)),
            new(language.Fix3PlusLines, language.Fix3PlusLinesExample, 1, true, nameof(Fix3PlusLines)),
            new(language.FixDoubleDash, language.FixDoubleDashExample, 1, true, nameof(FixDoubleDash)),
            new(language.FixDoubleGreaterThan, language.FixDoubleGreaterThanExample, 1, true, nameof(FixDoubleGreaterThan)),
            new(
                string.Format(language.FixContinuationStyleX,
                    Se.Language.Options.Settings.GetContinuationStyleName(Enum.Parse<ContinuationStyle>(Se.Settings.General.ContinuationStyle))), string.Empty, 1, true,
                nameof(FixContinuationStyle)),
            new(language.FixMissingOpenBracket, language.FixMissingOpenBracketExample, 1, true, nameof(FixMissingOpenBracket)),
            new(language.FixCommonOcrErrors, language.FixOcrErrorExample, 1, true, nameof(FixCommonOcrErrors)),
            new(language.FixUppercaseIInsideLowercaseWords, language.FixUppercaseIInsideLowercaseWordsExample, 1, true, nameof(FixUppercaseIInsideWords)),
            new(language.RemoveSpaceBetweenNumber, language.FixSpaceBetweenNumbersExample, 1, true, nameof(RemoveSpaceBetweenNumbers)),
            new(language.RemoveDialogFirstInNonDialogs, language.RemoveDialogFirstInNonDialogsExample, 1, true, nameof(RemoveDialogFirstLineInNonDialogs)),
            new(language.NormalizeStrings, string.Empty, 1, true, nameof(NormalizeStrings)),
        };
    }

    private static string GetDialogStyle(DialogType dialogStyle)
    {
        if (dialogStyle == DialogType.DashSecondLineWithoutSpace)
        {
            return Se.Language.Options.Settings.DialogStyleDashSecondLineWithoutSpace;
        }

        if (dialogStyle == DialogType.DashSecondLineWithSpace)
        {
            return Se.Language.Options.Settings.DialogStyleDashSecondLineWithSpace;
        }

        if (dialogStyle == DialogType.DashBothLinesWithoutSpace)
        {
            return Se.Language.Options.Settings.DialogStyleDashBothLinesWithoutSpace;
        }

        return Se.Language.Options.Settings.DialogStyleDashBothLinesWithSpace;
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
            UiUtil.ShowHelp("features/fix-common-errors");
        }
    }

    internal void TextBoxSearch_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (SelectedProfile == null)
        {
            return;
        }

        var rules = SelectedProfile.FixRules.ToList();
        SelectedProfile.FixRules.Clear();
        foreach (var rule in rules)
        {
            if (string.IsNullOrEmpty(SearchText) || rule.Name.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()))
            {
                SelectedProfile.FixRules.Add(rule);
            }
        }
    }

    public bool AllowFix(Paragraph p, string action)
    {
        if (_previewMode)
        {
            return true;
        }

        var allowFix = Fixes.Any(f => f.Paragraph.Id.ToLowerInvariant() == p.Id.ToLowerInvariant() && f.Action == action && f.IsSelected);
        return allowFix;
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after)
    {
        if (!_previewMode)
        {
            return;
        }

        var oldFix = _oldFixes.FirstOrDefault(f => f.Paragraph.Id.ToLowerInvariant() == p.Id.ToLowerInvariant() && f.Action == action);
        var isSelected = oldFix is not { IsSelected: false };

        AddFix(new FixDisplayItem(p, p.Number, action, before, after, isSelected));
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
    {
        if (!_previewMode)
        {
            return;
        }

        var oldFix = _oldFixes.FirstOrDefault(f => f.Paragraph.Id.ToLowerInvariant() == p.Id.ToLowerInvariant() && f.Action == action);
        var isSelected = isChecked;
        if (oldFix is { IsSelected: false })
        {
            isSelected = false;
        }

        AddFix(new FixDisplayItem(p, p.Number, action, before, after, isSelected));
    }

    public void LogStatus(string sender, string message)
    {
        //TODO: Implement logging functionality
    }

    public void LogStatus(string sender, string message, bool isImportant)
    {
        //TODO: Implement logging functionality
    }

    public void UpdateFixStatus(int fixes, string message)
    {
        if (_previewMode)
        {
            return;
        }

        if (fixes > 0)
        {
            _totalFixes += fixes;
            //            LogStatus(message, string.Format(LanguageSettings.Current.FixCommonErrors.XFixesApplied, fixes));
        }
    }

    public bool IsName(string candidate)
    {
        return _namesList.IsName(candidate);
    }

    public HashSet<string> GetAbbreviations()
    {
        return _namesList.GetAbbreviations();
    }

    public void AddToTotalErrors(int count)
    {
        _totalErrors += count;
    }

    public void AddToDeleteIndices(int index)
    {
        DeleteIndices.Add(index);
    }

    internal void SelectAndScrollTo(FixDisplayItem fixDisplayItem)
    {
        var p = Paragraphs.FirstOrDefault(p => p.Number == fixDisplayItem.Paragraph.Number);
        if (p != null)
        {
            SelectedParagraph = p;
            GridSubtitles.ScrollIntoView(GridSubtitles.SelectedItem, null);
        }
    }
}

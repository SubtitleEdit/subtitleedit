using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main.FlowEditing;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Nikse.SubtitleEdit.Features.Files.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

namespace Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

public partial class CheckArteErrorsViewModel : ObservableObject
{
    internal enum ArteFixKind
    {
        None,
        TeletextLinePosition,
        UnneededSpaces,
        Rebalance,
        RemoveItalic,
        MinimumGap,
        Header,
        Split,
        TeletextColor,
        FrameAccurateTimeCode,
        DisplayDuration,
        CreateBlankSubtitle,
        ShiftStartTimeCode,
    }

    private Subtitle? _sourceSnapshot;
    private string _languageCode = "en";
    public sealed class ArteProfileItem
    {
        public string Code { get; }
        public string Name { get; }

        public ArteProfileItem(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString() => Name;
    }

    public sealed partial class ArteCheckItem : ObservableObject
    {
        public string Name { get; }
        public bool IsImplemented => Name is "ARTE blank subtitle" or "Minimum gaps" or "Maximum two lines" or "Teletext line position" or
            "Frame-accurate time codes" or "Display duration" or "Teletext line length / control codes" or "Teletext colors" or "Unneeded spaces" or "Italic formatting (not allowed)";
        public string DisplayName => IsImplemented ? Name : Name + " (not implemented)";

        [ObservableProperty]
        private bool _isSelected = true;

        public ArteCheckItem(string name)
        {
            Name = name;
            _isSelected = IsImplemented;
        }
    }

    public sealed partial class ArteFixItem : ObservableObject
    {
        public bool CanBeFixed { get; }
        public int Index { get; }
        public string IndexDisplay => Index > 0 ? Index.ToString() : string.Empty;
        public string Before { get; }
        public string After { get; }
        public string Reason { get; }
        public string GroupName { get; internal set; } = string.Empty;
        public bool ShowTiming { get; internal set; }
        public string TimingNote { get; internal set; } = string.Empty;
        public string TimeRange { get; internal set; } = string.Empty;
        public string BeforePreview => !ShowTiming || string.IsNullOrEmpty(TimeRange) ? Before : TimeRange + Environment.NewLine + Before;
        public string AfterPreview => !ShowTiming || string.IsNullOrEmpty(TimeRange) || FixKind == ArteFixKind.Split || string.IsNullOrEmpty(After)
            ? After : TimeRange + Environment.NewLine + After;
        internal ArteFixKind FixKind { get; }
        internal string? ProposedHeader { get; set; }
        internal double? ProposedStartMs { get; set; }
        internal double? ProposedEndMs { get; set; }
        internal Paragraph? ProposedParagraph { get; set; }
        internal List<Paragraph>? SplitParagraphs { get; set; }

        [ObservableProperty]
        private bool _apply;

        internal ArteFixItem(
            bool canBeFixed,
            int index,
            string before,
            string after,
            string reason,
            ArteFixKind fixKind = ArteFixKind.None,
            bool applyByDefault = true)
        {
            CanBeFixed = canBeFixed;
            Index = index;
            Before = before;
            After = after;
            Reason = reason;
            FixKind = fixKind;
            ShowTiming = fixKind is ArteFixKind.Split or ArteFixKind.Rebalance;
            _apply = canBeFixed && applyByDefault;
        }
    }

    [ObservableProperty]
    private ArteProfileItem? _selectedProfile;

    [ObservableProperty]
    private ArteFixItem? _selectedFix;

    [ObservableProperty]
    private string _fixesSummaryText = Se.Language.Tools.CheckArteErrors.NoChecksRunYet;

    public ObservableCollection<LanguageItem> Languages { get; } = LanguageItem.CreateAll();

    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private bool _isSdh;
    private bool _syncingSdhLanguage;

    public ObservableCollection<double> SourceFrameRates { get; } =
        new(ChangeFrameRateViewModel.StandardFrameRates);

    [ObservableProperty] private double _selectedSourceFrameRate = 25.0;
    [ObservableProperty] private bool _shiftWholeFileToStartTimeCode;
    [ObservableProperty] private TimeSpan _targetStartTimeCode;

    [ObservableProperty] private int _teletextMaxCells = 37;
    private bool _teletextMaxCellsCompatibilityWarningShown;
    [ObservableProperty] private int _minimumGapFrames = 5;
    [ObservableProperty] private double _readingDurationTolerancePercent = 15.0;
    [ObservableProperty] private bool _acceptShortDurations;
    [ObservableProperty] private int _shortMinimumFrames = 18;

    public Action<int, double, bool, int>? WorkingSettingsChanged { get; set; }

    public Action? RunChecksStarted { get; set; }

    public void InitializeWorkingSettings(
        int teletextMaxCells,
        double readingDurationTolerancePercent,
        bool acceptShortDurations,
        int shortMinimumFrames)
    {
        _teletextMaxCells = Math.Max(1, teletextMaxCells);
        _readingDurationTolerancePercent = Math.Max(0, readingDurationTolerancePercent);
        _acceptShortDurations = acceptShortDurations;
        _shortMinimumFrames = Math.Max(1, shortMinimumFrames);

        OnPropertyChanged(nameof(TeletextMaxCells));
        OnPropertyChanged(nameof(ReadingDurationTolerancePercent));
        OnPropertyChanged(nameof(AcceptShortDurations));
        OnPropertyChanged(nameof(ShortMinimumFrames));
        OnPropertyChanged(nameof(IsArtePresetActive));
    }

    private void PublishWorkingSettings()
    {
        WorkingSettingsChanged?.Invoke(
            TeletextMaxCells,
            ReadingDurationTolerancePercent,
            AcceptShortDurations,
            ShortMinimumFrames);
    }

    public bool IsArtePresetActive =>
        TeletextMaxCells == 37 &&
        MinimumGapFrames == 5 &&
        Math.Abs(ReadingDurationTolerancePercent - 15.0) < 0.001 &&
        !AcceptShortDurations &&
        ShortMinimumFrames == 18;

    private double MinimumGapMilliseconds => MinimumGapFrames * 40.0;

    partial void OnTeletextMaxCellsChanged(int value)
    {
        if (value < 1)
        {
            TeletextMaxCells = 1;
            return;
        }

        OnPropertyChanged(nameof(IsArtePresetActive));
        PublishWorkingSettings();

        if (value > 37 &&
            !_teletextMaxCellsCompatibilityWarningShown &&
            _sourceSnapshot != null &&
            Window != null)
        {
            _teletextMaxCellsCompatibilityWarningShown = true;
            _ = MessageBox.Show(
                Window,
                Se.Language.Tools.CheckArteErrors.CompatibilityTitle,
                string.Format(Se.Language.Tools.CheckArteErrors.CompatibilityMessageX, value),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnMinimumGapFramesChanged(int value)
    {
        if (value < 0)
        {
            MinimumGapFrames = 0;
            return;
        }

        var minimumGap = Se.Settings.General.MinimumBetweenLines;
        minimumGap.Frames = value;
        minimumGap.Milliseconds = (int)Math.Round(value * 40.0, MidpointRounding.AwayFromZero);
        Se.SaveSettings();

        OnPropertyChanged(nameof(IsArtePresetActive));
        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnReadingDurationTolerancePercentChanged(double value)
    {
        if (value < 0)
        {
            ReadingDurationTolerancePercent = 0;
            return;
        }

        OnPropertyChanged(nameof(IsArtePresetActive));
        PublishWorkingSettings();
        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnAcceptShortDurationsChanged(bool value)
    {
        OnPropertyChanged(nameof(IsArtePresetActive));
        PublishWorkingSettings();
        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnShortMinimumFramesChanged(int value)
    {
        if (value < 1)
        {
            ShortMinimumFrames = 1;
            return;
        }

        OnPropertyChanged(nameof(IsArtePresetActive));
        PublishWorkingSettings();
        if (_sourceSnapshot != null && AcceptShortDurations)
        {
            Analyze();
        }
    }

    partial void OnShiftWholeFileToStartTimeCodeChanged(bool value)
    {
        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnTargetStartTimeCodeChanged(TimeSpan value)
    {
        if (_sourceSnapshot != null && ShiftWholeFileToStartTimeCode)
        {
            Analyze();
        }
    }

    partial void OnIsSdhChanged(bool value)
    {
        if (!_syncingSdhLanguage)
        {
            var targetCode = SelectedLanguage?.Code switch
            {
                "08" when value => "2D",
                "0F" when value => "2F",
                "2D" when !value => "08",
                "2F" when !value => "0F",
                _ => null,
            };

            if (targetCode != null)
            {
                _syncingSdhLanguage = true;
                SelectedLanguage = Languages.FirstOrDefault(language => language.Code == targetCode);
                _syncingSdhLanguage = false;
            }
        }

        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if (!_syncingSdhLanguage && value?.Code is "2D" or "2F")
        {
            _syncingSdhLanguage = true;
            IsSdh = true;
            _syncingSdhLanguage = false;
        }

        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    partial void OnSelectedSourceFrameRateChanged(double value)
    {
        if (_sourceSnapshot != null)
        {
            Analyze();
        }
    }

    private void SetSourceFrameRateWithoutAnalysis(double value)
    {
        if (Math.Abs(_selectedSourceFrameRate - value) <= 0.001)
        {
            return;
        }

        _selectedSourceFrameRate = value;
        OnPropertyChanged(nameof(SelectedSourceFrameRate));
    }

    public Action<Subtitle>? ApplyToMainSubtitle { get; set; }
    public Func<IReadOnlyList<string>, int, Task<Subtitle?>>? OpenEbuOptionsDialog { get; set; }
    private readonly Stack<(Subtitle Subtitle, int AppliedCount, List<string> Notes, double SourceFrameRate)> _undoHistory = new();
    private readonly List<string> _appliedNotes = new();
    private bool CanUndo => _undoHistory.Count > 0;

    public bool OkPressed { get; private set; }
    public Subtitle? FixedSubtitle { get; private set; }
    public int AppliedFixCount { get; private set; }

    public Window? Window { get; set; }

    public ObservableCollection<ArteProfileItem> Profiles { get; } =
    [
        new("STA", "ARTE STA"),
        new("STF", "ARTE STF"),
        new("HG-DEU", "ARTE HG DEU"),
        new("HG-FRA", "ARTE HG FRA"),
    ];

    public ObservableCollection<ArteCheckItem> Checks { get; } =
    [
        new("ARTE blank subtitle"),
        new("Frame-accurate time codes"),
        new("Minimum gaps"),
        new("Display duration"),
        new("Maximum two lines"),
        new("Teletext line position"),
        new("Teletext line length / control codes"),
        new("Teletext colors"),
        new("Unneeded spaces"),
        new("Italic formatting (not allowed)"),
    ];

    public ObservableCollection<ArteFixItem> Fixes { get; } = new();

    public sealed partial class ArteFixGroup : ObservableObject
    {
        public string Name { get; }
        public IReadOnlyList<ArteFixItem> Items { get; }
        public string Header => $"{Name} ({Items.Count})";
        [ObservableProperty]
        private bool _isExpanded;

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var item in Items.Where(item => item.CanBeFixed))
            {
                item.Apply = true;
            }
        }

        [RelayCommand]
        private void InvertSelection()
        {
            foreach (var item in Items.Where(item => item.CanBeFixed))
            {
                item.Apply = !item.Apply;
            }
        }

        public ArteFixGroup(string name, IReadOnlyList<ArteFixItem> items, bool expanded)
        {
            Name = name;
            Items = items;
            _isExpanded = expanded;
        }
    }

    public ObservableCollection<ArteFixGroup> FixGroups { get; } = new();
    private readonly Dictionary<string, bool> _expandedGroups = new();


    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    public CheckArteErrorsViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        SelectedProfile = Profiles.FirstOrDefault();
        SelectedLanguage = Languages.FirstOrDefault(language => language.Code == "08");
        foreach (var check in Checks)
        {
            check.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ArteCheckItem.IsSelected) && _sourceSnapshot != null)
                {
                    Analyze();
                }
            };
        }
    }

    public void Initialize(Subtitle subtitle)
    {
        // Use Subtitle Edit's existing minimum-gap setting as the single source of truth.
        // Opening the ARTE checker must not silently replace the user's configured value.
        var minimumGap = Se.Settings.General.MinimumBetweenLines;
        _minimumGapFrames = Se.Settings.General.UseFrameMode
            ? Math.Max(0, minimumGap.Frames)
            : Math.Max(0, (int)Math.Round(minimumGap.Milliseconds / 40.0, MidpointRounding.AwayFromZero));
        OnPropertyChanged(nameof(MinimumGapFrames));
        OnPropertyChanged(nameof(IsArtePresetActive));

        // Every ARTE target session starts neutral: source 25 fps to target 25 fps.
        // A different source rate is selected explicitly by the user.
        SetSourceFrameRateWithoutAnalysis(25.0);

        // Keep an independent copy for every analysis pass. The live subtitle in
        // MainViewModel is deliberately not exposed to this tool.
        _sourceSnapshot = new Subtitle(subtitle, generateNewId: false);
        if (_sourceSnapshot.Paragraphs.Count > 0)
        {
            var first = _sourceSnapshot.Paragraphs[0];
            var hasLeadingBlank = string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(first.Text, true));
            var referenceMs = hasLeadingBlank
                ? first.StartTime.TotalMilliseconds
                : Math.Floor(first.StartTime.TotalMilliseconds / 3_600_000.0) * 3_600_000.0;
            _targetStartTimeCode = TimeSpan.FromMilliseconds(referenceMs);
            OnPropertyChanged(nameof(TargetStartTimeCode));
        }
        if (Ebu.IsStlHeader(subtitle.Header))
        {
            var header = Ebu.ReadHeader(Ebu.GetEncoding(subtitle.Header.Substring(0, 3)).GetBytes(subtitle.Header));
            IsSdh = header.LanguageCode is "2D" or "2F";
            SelectedLanguage = Languages.FirstOrDefault(language => language.Code == header.LanguageCode) ?? SelectedLanguage;
        }
        _languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? "en";
        _teletextMaxCellsCompatibilityWarningShown = false;
        _undoHistory.Clear();
        _appliedNotes.Clear();
        UndoCommand.NotifyCanExecuteChanged();
        FixedSubtitle = null;
        OkPressed = false;
        AppliedFixCount = 0;
        Fixes.Clear();
        FixesSummaryText = string.Format(Se.Language.Tools.CheckArteErrors.SubtitlesLoadedX, _sourceSnapshot.Paragraphs.Count);
    }

    private void SyncLanguageSelectionFromHeader(string headerText)
    {
        if (!Ebu.IsStlHeader(headerText))
        {
            return;
        }

        var header = Ebu.ReadHeader(Ebu.GetEncoding(headerText.Substring(0, 3)).GetBytes(headerText));
        var isSdh = header.LanguageCode is "2D" or "2F";
        var languageCode = header.LanguageCode switch
        {
            "2D" => "08",
            "2F" => "0F",
            _ => header.LanguageCode,
        };
        var language = Languages.FirstOrDefault(item => item.Code == languageCode);

        if (_isSdh != isSdh)
        {
            _isSdh = isSdh;
            OnPropertyChanged(nameof(IsSdh));
        }

        if (language != null && !ReferenceEquals(_selectedLanguage, language))
        {
            _selectedLanguage = language;
            OnPropertyChanged(nameof(SelectedLanguage));
        }
    }

    [RelayCommand]
    private async Task OpenEbuOptions()
    {
        if (OpenEbuOptionsDialog == null || _sourceSnapshot == null)
        {
            return;
        }

        var updatedSubtitle = await OpenEbuOptionsDialog(GetReportEntries(), Fixes.Count);
        if (updatedSubtitle == null)
        {
            return;
        }

        _sourceSnapshot.Header = updatedSubtitle.Header;
        SyncLanguageSelectionFromHeader(updatedSubtitle.Header);
        Analyze();
    }

    [RelayCommand]
    private void ChecksSelectAll()
    {
        foreach (var check in Checks)
        {
            check.IsSelected = check.IsImplemented;
        }
    }

    [RelayCommand]
    private void ChecksInverseSelection()
    {
        foreach (var check in Checks)
        {
            check.IsSelected = check.IsImplemented && !check.IsSelected;
        }
    }

    [RelayCommand]
    private void FixesSelectAll()
    {
        foreach (var fix in Fixes)
        {
            if (fix.CanBeFixed)
            {
                fix.Apply = true;
            }
        }
    }

    [RelayCommand]
    private void FixesInverseSelection()
    {
        foreach (var fix in Fixes)
        {
            if (fix.CanBeFixed)
            {
                fix.Apply = !fix.Apply;
            }
        }
    }

    [RelayCommand]
    private void FixesClearSelection()
    {
        foreach (var fix in Fixes)
        {
            if (fix.CanBeFixed)
            {
                fix.Apply = false;
            }
        }
    }

    [RelayCommand]
    private void ApplyArtePreset()
    {
        // ARTE working preset: 37 visible cells, five 25-fps frames,
        // 15% reading-duration tolerance, and strict short-duration checking.
        TeletextMaxCells = 37;
        MinimumGapFrames = 5;
        ReadingDurationTolerancePercent = 15.0;
        AcceptShortDurations = false;
        ShortMinimumFrames = 18;
    }

    private void Analyze() => Analyze(false);

    private void Analyze(bool showAlarmPopup)
    {
        foreach (var group in FixGroups)
        {
            _expandedGroups[group.Name] = group.IsExpanded;
        }
        FixGroups.Clear();
        Fixes.Clear();

        if (SelectedProfile == null)
        {
            FixesSummaryText = Se.Language.Tools.CheckArteErrors.SelectProfileFirst;
            return;
        }

        if (_sourceSnapshot == null)
        {
            FixesSummaryText = Se.Language.Tools.CheckArteErrors.NoSnapshotAvailable;
            return;
        }

        var selectedChecks = Checks
            .Where(c => c.IsSelected && c.IsImplemented)
            .Select(c => c.Name)
            .ToHashSet();

        // Analyze another detached copy so even future rule implementations cannot
        // accidentally change the snapshot supplied by MainViewModel.
        var subtitle = new Subtitle(_sourceSnapshot, generateNewId: false);

        // ARTE always targets 25 fps. Convert only this detached analysis copy so
        // GAP, overlap and split timing proposals are calculated on the same target
        // timeline that Start correction will apply later.
        if (Math.Abs(SelectedSourceFrameRate - 25.0) > 0.001)
        {
            subtitle.ChangeFrameRate(SelectedSourceFrameRate, 25.0);
        }

        // ARTE target header/language is mandatory setup, not an optional checklist item.
        RunCheck("Header / language data", () => AnalyzeHeader(subtitle));

        if (ShiftWholeFileToStartTimeCode)
        {
            RunCheck("Start time code", () => AnalyzeStartTimeCodeShift(subtitle));
        }

        if (selectedChecks.Contains("ARTE blank subtitle"))
        {
            RunCheck("ARTE blank subtitle", () => AnalyzeBlankSubtitle(subtitle));
        }

        if (selectedChecks.Contains("Frame-accurate time codes"))
        {
            RunCheck("Frame-accurate time codes", () => AnalyzeFrameAccurateTimeCodes(subtitle));
        }

        if (selectedChecks.Contains("Display duration"))
        {
            RunCheck("Display duration", () => AnalyzeDisplayDurations(subtitle));
        }

        // Text structure and Teletext layout are analyzed before gaps. Future
        // split/rebalance fixes can therefore run before gap normalization.
        if (selectedChecks.Contains("Maximum two lines"))
        {
            RunCheck("Split / Rebalance", () => AnalyzeMaximumTwoLines(subtitle));
        }

        if (selectedChecks.Contains("Teletext line position"))
        {
            RunCheck("Teletext line position", () => AnalyzeTeletextLinePosition(subtitle));
        }

        // Colour codes consume one Teletext character. Normalize colours first, so
        // the following layout pass evaluates the actual text that will be written.
        if (selectedChecks.Contains("Teletext colors"))
        {
            RunCheck("Teletext colors", () => AnalyzeTeletextColors(subtitle));
        }

        if (selectedChecks.Contains("Teletext line length / control codes"))
        {
            RunCheck("Split / Rebalance", () => AnalyzeTeletextLineLength(subtitle));
        }

        if (selectedChecks.Contains("Italic formatting (not allowed)"))
        {
            RunCheck("Italic formatting", () => AnalyzeItalic(subtitle));
        }

        if (selectedChecks.Contains("Unneeded spaces"))
        {
            RunCheck("Unneeded spaces", () => AnalyzeUnneededSpaces(subtitle));
        }

        if (selectedChecks.Contains("Minimum gaps"))
        {
            RunCheck("Minimum gaps", () => AnalyzeMinimumGaps(subtitle));
        }


        foreach (var fix in Fixes.Where(f => f.Index > 0 && f.Index <= subtitle.Paragraphs.Count))
        {
            fix.TimeRange = FormatTimeRange(subtitle.Paragraphs[fix.Index - 1]);
        }

        foreach (var group in Fixes.GroupBy(f => f.GroupName))
        {
            FixGroups.Add(new ArteFixGroup(group.Key, group.ToList(),
                _expandedGroups.GetValueOrDefault(group.Key)));
        }

        FixesSummaryText = Fixes.Count == 0
            ? selectedChecks.Count == 0
                ? Se.Language.Tools.CheckArteErrors.HeaderValidNoOptionalChecks
                : string.Format(Se.Language.Tools.CheckArteErrors.NoIssuesFoundX, selectedChecks.Count)
            : string.Format(Se.Language.Tools.CheckArteErrors.CorrectionsAndAlarmsX,
                Fixes.Count(f => f.CanBeFixed), Fixes.Count(f => !f.CanBeFixed));

        if (showAlarmPopup)
        {
            ShowUnresolvedAlarmPopup();
        }
    }

    [RelayCommand]
    private void RunChecks()
    {
        RunChecksStarted?.Invoke();
        Analyze(true);
    }

    private void ShowUnresolvedAlarmPopup()
    {
        if (Window == null)
        {
            return;
        }

        var alarms = Fixes
            .Where(f => !f.CanBeFixed &&
                        (f.Reason.Contains("ALARM:", StringComparison.OrdinalIgnoreCase) ||
                         f.TimingNote.Contains("ALARM:", StringComparison.OrdinalIgnoreCase)))
            .Select(f =>
            {
                var location = string.IsNullOrEmpty(f.IndexDisplay) ? string.Empty : $"UT {f.IndexDisplay}: ";
                var message = !string.IsNullOrEmpty(f.TimingNote) ? $"{f.Reason} {f.TimingNote}" : f.Reason;
                return $"{location}{message}";
            })
            .Distinct()
            .ToList();

        if (alarms.Count == 0)
        {
            return;
        }

        _ = MessageBox.Show(
            Window,
            Se.Language.Tools.CheckArteErrors.AlarmTitle,
            string.Join(Environment.NewLine + Environment.NewLine, alarms),
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void RunCheck(string groupName, Action analyze)
    {
        var first = Fixes.Count;
        analyze();
        for (var i = first; i < Fixes.Count; i++)
        {
            Fixes[i].GroupName = groupName;
        }
    }

    private void AnalyzeHeader(Subtitle subtitle)
    {
        var targetLanguageCode = SelectedLanguage?.Code switch
        {
            "2D" or "2F" => SelectedLanguage.Code,
            "08" when IsSdh => "2D",
            "0F" when IsSdh => "2F",
            _ when !IsSdh => SelectedLanguage?.Code,
            _ => null,
        };
        if (targetLanguageCode == null)
        {
            Fixes.Add(new ArteFixItem(false, 0, SelectedLanguage?.Language ?? Se.Language.Tools.CheckArteErrors.NoLanguage, string.Empty,
                Se.Language.Tools.CheckArteErrors.SdhLanguageNotConfigured));
            return;
        }
        var hasStlHeader = Ebu.IsStlHeader(subtitle.Header);
        var header = hasStlHeader
            ? Ebu.ReadHeader(Ebu.GetEncoding(subtitle.Header.Substring(0, 3)).GetBytes(subtitle.Header))
            : new Ebu.EbuGeneralSubtitleInformation();
        static string Describe(Ebu.EbuGeneralSubtitleInformation h) =>
            $"Code page: {h.CodePageNumber}\nDisk format: {h.DiskFormatCode}\nDisplay standard: {h.DisplayStandardCode}\nCharacter table: {h.CharacterCodeTableNumber}\nLanguage: {h.LanguageCode}\nCharacters per row: {h.MaximumNumberOfDisplayableCharactersInAnyTextRow}\nRows: {h.MaximumNumberOfDisplayableRows}";
        var before = hasStlHeader ? Describe(header) : Se.Language.Tools.CheckArteErrors.SourceHasNoEbuHeader;
        header.CodePageNumber = "850";
        header.DiskFormatCode = "STL25.01";
        header.DisplayStandardCode = "2";
        header.CharacterCodeTableNumber = "00";
        header.LanguageCode = targetLanguageCode;
        header.MaximumNumberOfDisplayableCharactersInAnyTextRow = "40";
        header.MaximumNumberOfDisplayableRows = "23";
        var after = Describe(header);
        if (before != after)
        {
            Fixes.Add(new ArteFixItem(true, 0, before, after,
                hasStlHeader
                    ? Se.Language.Tools.CheckArteErrors.ApplyTargetHeader
                    : Se.Language.Tools.CheckArteErrors.CreateTargetHeader,
                ArteFixKind.Header) { ProposedHeader = header.ToString() });
        }
    }

    private void AnalyzeBlankSubtitle(Subtitle subtitle)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var first = subtitle.Paragraphs[0];
        var startTimeCodeMs = GetStartTimeCodeReference(subtitle);
        var expectedEndMs = startTimeCodeMs + 200.0;
        var isBlank = string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(first.Text, true));
        var startsAtStartTimeCode =
            Math.Abs(first.StartTime.TotalMilliseconds - startTimeCodeMs) < 0.01;

        if (!isBlank)
        {
            if (startsAtStartTimeCode)
            {
                Fixes.Add(new ArteFixItem(
                    false,
                    1,
                    first.Text,
                    string.Empty,
                    Se.Language.Tools.CheckArteErrors.StartSubtitleMustBeBlank));
                return;
            }

            var blank = new Paragraph(string.Empty, startTimeCodeMs, expectedEndMs)
            {
                MarginV = "0",
            };
            Fixes.Add(new ArteFixItem(
                true,
                0,
                Se.Language.Tools.CheckArteErrors.Missing,
                FormatTimeRange(blank),
                Se.Language.Tools.CheckArteErrors.CreateStartBlank,
                ArteFixKind.CreateBlankSubtitle)
            {
                ProposedParagraph = blank,
            });
            return;
        }

        var hasCorrectDuration =
            Math.Abs(first.EndTime.TotalMilliseconds - expectedEndMs) < 0.01;
        if (!hasCorrectDuration)
        {
            var expectedBlank = new Paragraph(string.Empty, startTimeCodeMs, expectedEndMs);
            var nextStartMs = subtitle.Paragraphs.Count > 1
                ? subtitle.Paragraphs[1].StartTime.TotalMilliseconds
                : double.PositiveInfinity;
            var canFix = expectedEndMs <= nextStartMs - MinimumGapMilliseconds;

            Fixes.Add(new ArteFixItem(
                canFix,
                1,
                FormatTimeRange(first),
                canFix ? FormatTimeRange(expectedBlank) : string.Empty,
                canFix
                    ? Se.Language.Tools.CheckArteErrors.SetStartBlankDuration
                    : Se.Language.Tools.CheckArteErrors.StartBlankNoRoomAlarm,
                ArteFixKind.DisplayDuration)
            {
                ProposedEndMs = canFix ? expectedEndMs : null,
            });
        }
    }

    private static double GetStartTimeCodeReference(Subtitle subtitle)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return 0;
        }

        var first = subtitle.Paragraphs[0];
        if (string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(first.Text, true)))
        {
            return first.StartTime.TotalMilliseconds;
        }

        return Math.Floor(first.StartTime.TotalMilliseconds / 3_600_000.0) * 3_600_000.0;
    }

    private void AnalyzeStartTimeCodeShift(Subtitle subtitle)
    {
        var current = GetStartTimeCodeReference(subtitle);
        var target = RoundToArteFrame(TargetStartTimeCode.TotalMilliseconds);
        if (Math.Abs(target - current) < 0.01)
        {
            return;
        }

        var before = new Paragraph(string.Empty, current, current + 200);
        var after = new Paragraph(string.Empty, target, target + 200);
        Fixes.Add(new ArteFixItem(true, 0, FormatTimeRange(before), FormatTimeRange(after),
            "Shift all subtitle timecodes to the requested start time code.", ArteFixKind.ShiftStartTimeCode)
        {
            ProposedStartMs = target - current,
        });
    }

    private static double RoundToArteFrame(double milliseconds) =>
        Math.Round(milliseconds / 40.0, MidpointRounding.AwayFromZero) * 40.0;

    private static string FormatFrames(double milliseconds)
    {
        var frames = (long)Math.Round(milliseconds / 40.0, MidpointRounding.AwayFromZero);
        return $"{frames / 25} s {frames % 25:00} fr";
    }

    private void AnalyzeFrameAccurateTimeCodes(Subtitle subtitle)
    {
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
            var start = paragraph.StartTime.TotalMilliseconds;
            var end = paragraph.EndTime.TotalMilliseconds;
            var fixedStart = RoundToArteFrame(start);
            var fixedEnd = RoundToArteFrame(end);
            if (Math.Abs(start - fixedStart) < 0.01 && Math.Abs(end - fixedEnd) < 0.01)
            {
                continue;
            }

            if (fixedEnd <= fixedStart)
            {
                Fixes.Add(new ArteFixItem(false, i + 1, FormatTimeRange(paragraph), string.Empty,
                    Se.Language.Tools.CheckArteErrors.FrameRoundingRemovesDurationAlarm));
                continue;
            }

            var after = new Paragraph(paragraph, true)
            {
                StartTime = new TimeCode(fixedStart),
                EndTime = new TimeCode(fixedEnd),
            };
            Fixes.Add(new ArteFixItem(true, i + 1, FormatTimeRange(paragraph), FormatTimeRange(after),
                Se.Language.Tools.CheckArteErrors.RoundTimeCodesToFrames, ArteFixKind.FrameAccurateTimeCode)
            {
                ProposedStartMs = fixedStart,
                ProposedEndMs = fixedEnd,
            });
        }
    }

    private void AnalyzeDisplayDurations(Subtitle subtitle)
    {
        var minimumMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maximumMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        var maximumCps = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
            if (string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(paragraph.Text, true)))
            {
                continue;
            }
            var duration = paragraph.EndTime.TotalMilliseconds - paragraph.StartTime.TotalMilliseconds;
            var characters = HtmlUtil.RemoveHtmlTags(paragraph.Text, true).Count(character => character is not '\r' and not '\n');
            var readingMinimum = maximumCps > 0 ? characters * 1000.0 / maximumCps : 0;
            var requiredMinimum = Math.Max(minimumMs, readingMinimum);
            var toleratedMinimum = requiredMinimum * Math.Max(0, 1.0 - ReadingDurationTolerancePercent / 100.0);
            var shortMinimum = ShortMinimumFrames * 40.0;
            var acceptedMinimum = AcceptShortDurations ? shortMinimum : toleratedMinimum;
            var isTooShort = duration < acceptedMinimum;
            var isTooLong = maximumMs > 0 && duration > maximumMs;
            if (!isTooShort && !isTooLong)
            {
                continue;
            }

            var desiredEnd = isTooShort
                ? RoundToArteFrame(paragraph.StartTime.TotalMilliseconds +
                    (AcceptShortDurations ? shortMinimum : requiredMinimum))
                : RoundToArteFrame(paragraph.StartTime.TotalMilliseconds + maximumMs);
            var nextStart = i + 1 < subtitle.Paragraphs.Count
                ? subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - MinimumGapMilliseconds
                : double.PositiveInfinity;
            var canFix = desiredEnd > paragraph.StartTime.TotalMilliseconds && desiredEnd <= nextStart &&
                         (!isTooLong || desiredEnd >= requiredMinimum + paragraph.StartTime.TotalMilliseconds);
            var issue = isTooShort
                ? AcceptShortDurations
                    ? string.Format(Se.Language.Tools.CheckArteErrors.DurationBelowShortMinimumX,
                        FormatFrames(duration), ShortMinimumFrames)
                    : string.Format(Se.Language.Tools.CheckArteErrors.DurationBelowToleratedMinimumX,
                        FormatFrames(duration), FormatFrames(toleratedMinimum),
                        ReadingDurationTolerancePercent.ToString("0.#"), FormatFrames(requiredMinimum))
                : string.Format(Se.Language.Tools.CheckArteErrors.DurationAboveMaximumX,
                    FormatFrames(duration), FormatFrames(maximumMs));
            Fixes.Add(new ArteFixItem(canFix, i + 1, FormatFrames(duration),
                canFix ? FormatFrames(desiredEnd - paragraph.StartTime.TotalMilliseconds) : string.Empty,
                canFix ? issue + " " + Se.Language.Tools.CheckArteErrors.OptionalTcOutAdjustment
                    : issue + " " + Se.Language.Tools.CheckArteErrors.NoSafeTcOutAdjustmentAlarm,
                ArteFixKind.DisplayDuration, applyByDefault: false)
            {
                ProposedEndMs = canFix ? desiredEnd : null,
            });
        }
    }

    private double GetAcceptedMinimumDurationMs(string text)
    {
        if (AcceptShortDurations)
        {
            return ShortMinimumFrames * 40.0;
        }

        var characters = HtmlUtil.RemoveHtmlTags(text, true)
            .Count(character => character is not '\r' and not '\n');
        var maximumCps = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;
        var readingMinimum = maximumCps > 0 ? characters * 1000.0 / maximumCps : 0;
        var requiredMinimum = Math.Max(Se.Settings.General.SubtitleMinimumDisplayMilliseconds, readingMinimum);
        return requiredMinimum * Math.Max(0, 1.0 - ReadingDurationTolerancePercent / 100.0);
    }

    private void AnalyzeMinimumGaps(Subtitle subtitle)
    {
        const double frameMs = 40.0;
        var minimumGapFrames = MinimumGapFrames;
        var plannedStarts = subtitle.Paragraphs.Select(p => RoundToArteFrame(p.StartTime.TotalMilliseconds)).ToArray();
        var plannedEnds = subtitle.Paragraphs.Select(p => RoundToArteFrame(p.EndTime.TotalMilliseconds)).ToArray();

        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            var previous = subtitle.Paragraphs[i - 1];
            var current = subtitle.Paragraphs[i];
            var gapFrames = (int)Math.Round((plannedStarts[i] - plannedEnds[i - 1]) / frameMs, MidpointRounding.AwayFromZero);
            if (gapFrames >= minimumGapFrames)
            {
                continue;
            }

            var missingFrames = minimumGapFrames - gapFrames;
            var split = Fixes.FirstOrDefault(f => f.Index == i && f.SplitParagraphs != null);
            var previousStart = split?.SplitParagraphs![^1].StartTime.TotalMilliseconds ?? plannedStarts[i - 1];
            var previousText = split?.SplitParagraphs![^1].Text ?? previous.Text;
            var previousMinimum = GetAcceptedMinimumDurationMs(previousText);
            var currentMinimum = GetAcceptedMinimumDurationMs(current.Text);
            var previousCapacity = Math.Max(0, (int)Math.Floor((plannedEnds[i - 1] - previousStart - previousMinimum) / frameMs + 0.0001));
            var currentCapacity = Math.Max(0, (int)Math.Floor((plannedEnds[i] - plannedStarts[i] - currentMinimum) / frameMs + 0.0001));
            var canFix = previousCapacity + currentCapacity >= missingFrames;

            if (!canFix)
            {
                Fixes.Add(new ArteFixItem(false, i,
                    $"{gapFrames} frame{(Math.Abs(gapFrames) == 1 ? string.Empty : "s")}", string.Empty,
                    string.Format(Se.Language.Tools.CheckArteErrors.CannotCreateGapAlarmX, minimumGapFrames, i, i + 1),
                    ArteFixKind.MinimumGap));
                continue;
            }

            var previousShiftFrames = Math.Min(previousCapacity, missingFrames / 2);
            var currentShiftFrames = Math.Min(currentCapacity, missingFrames - previousShiftFrames);
            var remaining = missingFrames - previousShiftFrames - currentShiftFrames;
            if (remaining > 0)
            {
                var addPrevious = Math.Min(previousCapacity - previousShiftFrames, remaining);
                previousShiftFrames += addPrevious;
                remaining -= addPrevious;
            }
            if (remaining > 0)
            {
                currentShiftFrames += Math.Min(currentCapacity - currentShiftFrames, remaining);
            }

            var newEndMs = plannedEnds[i - 1] - previousShiftFrames * frameMs;
            var newStartMs = plannedStarts[i] + currentShiftFrames * frameMs;
            plannedEnds[i - 1] = newEndMs;
            plannedStarts[i] = newStartMs;

            var distribution = previousShiftFrames > 0 && currentShiftFrames > 0
                ? $"Move TC Out of UT {i} {previousShiftFrames} frame(s) earlier and TC In of UT {i + 1} {currentShiftFrames} frame(s) later."
                : previousShiftFrames > 0
                    ? $"Move TC Out of UT {i} {previousShiftFrames} frame(s) earlier."
                    : $"Move TC In of UT {i + 1} {currentShiftFrames} frame(s) later.";

            Fixes.Add(new ArteFixItem(true, i,
                $"{gapFrames} frame{(Math.Abs(gapFrames) == 1 ? string.Empty : "s")}",
                $"{minimumGapFrames} frame{(minimumGapFrames == 1 ? string.Empty : "s")}",
                $"{distribution} Result: {minimumGapFrames}-frame gap.", ArteFixKind.MinimumGap)
            {
                ProposedEndMs = newEndMs,
                ProposedStartMs = newStartMs,
            });
        }
    }

    private void AnalyzeTeletextLinePosition(Subtitle subtitle)
    {
        var correctBottomCount = 0;
        var oneRowHighBottomCount = 0;

        foreach (var paragraph in subtitle.Paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph.Text))
            {
                continue;
            }

            var lineCount = paragraph.Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Length;
            if (lineCount is < 1 or > 2 ||
                !int.TryParse(paragraph.MarginV, out var row))
            {
                continue;
            }

            var expectedBottomRow = lineCount == 1 ? 22 : 20;
            if (row == expectedBottomRow)
            {
                correctBottomCount++;
            }
            else if (row == expectedBottomRow - 1)
            {
                oneRowHighBottomCount++;
            }
        }

        var shiftWholeFileOneRow =
            oneRowHighBottomCount > 0 &&
            oneRowHighBottomCount > correctBottomCount;

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
            if (string.IsNullOrWhiteSpace(paragraph.Text))
            {
                if (!string.Equals(paragraph.MarginV, "22", StringComparison.Ordinal))
                {
                    Fixes.Add(new ArteFixItem(true, i + 1, string.IsNullOrWhiteSpace(paragraph.MarginV) ? Se.Language.Tools.CheckArteErrors.NotSet : paragraph.MarginV, "22", Se.Language.Tools.CheckArteErrors.BlankStartsOnRow22, ArteFixKind.TeletextLinePosition));
                }
                continue;
            }

            var lineCount = paragraph.Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Length;
            if (lineCount is < 1 or > 2)
            {
                continue;
            }

            var expectedBottomRow = lineCount == 1 ? 22 : 20;
            var hasRow = int.TryParse(paragraph.MarginV, out var currentRow);

            if (shiftWholeFileOneRow && hasRow)
            {
                Fixes.Add(new ArteFixItem(
                    true,
                    i + 1,
                    currentRow.ToString(),
                    (currentRow + 1).ToString(),
                    Se.Language.Tools.CheckArteErrors.FileVerticallyShifted,
                    ArteFixKind.TeletextLinePosition));
                continue;
            }

            // ARTE Teletext is double height: the stored row is the first physical row.
            // A one-line subtitle on row 23 would extend below the page; it belongs on
            // rows 22+23. Two lines must start on row 20, occupying 20+21 and 22+23.
            // Only correct these invalid bottom placements, leaving deliberate titles
            // higher on the screen untouched.
            if (hasRow &&
                ((lineCount == 1 && currentRow == TeletextRowHelper.BottomRow) ||
                 (lineCount == 2 && currentRow == TeletextRowHelper.BottomRow - 1)))
            {
                Fixes.Add(new ArteFixItem(
                    true,
                    i + 1,
                    currentRow.ToString(),
                    expectedBottomRow.ToString(),
                    string.Format(Se.Language.Tools.CheckArteErrors.DoubleHeightBottomRowsX, expectedBottomRow, lineCount),
                    ArteFixKind.TeletextLinePosition));
                continue;
            }

            // In an otherwise correctly positioned Teletext file, deliberate higher
            // positions (for example an on-screen title) remain untouched.
            if (hasRow)
            {
                continue;
            }

            Fixes.Add(new ArteFixItem(
                true,
                i + 1,
                Se.Language.Tools.CheckArteErrors.NotSet,
                expectedBottomRow.ToString(),
                string.Format(Se.Language.Tools.CheckArteErrors.NoTeletextPositionX, lineCount),
                ArteFixKind.TeletextLinePosition));
        }
    }

    private void AnalyzeTeletextLineLength(Subtitle subtitle)
    {
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];

            if (string.IsNullOrWhiteSpace(paragraph.Text))
            {
                continue;
            }

            // A proposed colour normalization can turn a 37-character visible line
            // into a 36-character coloured line. Evaluate that proposed canonical
            // text here, not the pre-correction paragraph.
            var colorNormalizedText = Fixes
                .LastOrDefault(f => f.Index == i + 1 && f.FixKind == ArteFixKind.TeletextColor)
                ?.After ?? paragraph.Text;
            var layoutParagraph = colorNormalizedText == paragraph.Text
                ? paragraph
                : new Paragraph(paragraph, true) { Text = colorNormalizedText };

            var projection =
                FlowInlineColorProjection.Parse(RemoveItalicTags(layoutParagraph.Text));

            var visibleText = projection.VisibleText
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            var lines = visibleText.Split('\n');
            var lineStart = 0;
            // In the ARTE delivery profile a coloured box occupies one teletext
            // control position for line-capacity purposes.
            var coloredBoxControlCount = ColoredBoxRegex.IsMatch(layoutParagraph.Text) ? 1 : 0;

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                var colorCodeCount = projection.ColorRuns.Count(
                    run =>
                        run.Start < lineStart + line.Length &&
                        run.End > lineStart);

                var maximum = Math.Max(1, TeletextMaxCells - colorCodeCount - coloredBoxControlCount);

                if (line.Length > maximum)
                {
                    AddTextLayoutProposal(layoutParagraph, i + 1,
                        $"Line {lineIndex + 1}: {line.Length} chars.");
                    break;
                }

                lineStart += line.Length + 1;
            }
        }
    }

    private void AnalyzeOverlaps(Subtitle subtitle)
    {
        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            var previous = subtitle.Paragraphs[i - 1];
            var current = subtitle.Paragraphs[i];

            if (current.StartTime.TotalMilliseconds < previous.EndTime.TotalMilliseconds)
            {
                Fixes.Add(new ArteFixItem(
                    false,
                    i + 1,
                    FormatTimeRange(current),
                    string.Empty,
                    $"Overlaps subtitle {i}."));
            }
        }
    }

    private void AnalyzeMaximumTwoLines(Subtitle subtitle)
    {
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
            var lineCount = paragraph.Text
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n')
                .Length;

            if (lineCount > 2)
            {
                AddTextLayoutProposal(paragraph, i + 1,
                    $"Maximum two lines exceeded ({lineCount} lines).");
            }
        }
    }

    private bool FitsTeletext(string text)
    {
        var projection = FlowInlineColorProjection.Parse(RemoveItalicTags(text).Replace("\r\n", "\n").Replace('\r', '\n'));
        var lines = projection.VisibleText.Split('\n');
        if (lines.Length > 2)
        {
            return false;
        }

        var start = 0;
        var coloredBoxControlCount = ColoredBoxRegex.IsMatch(text) ? 1 : 0;
        foreach (var line in lines)
        {
            var controls = projection.ColorRuns.Count(run => run.Start < start + line.Length && run.End > start);
            if (line.Length + controls + coloredBoxControlCount > TeletextMaxCells)
            {
                return false;
            }
            start += line.Length + 1;
        }
        return true;
    }

    private void AddTextLayoutProposal(Paragraph paragraph, int index, string reason)
    {
        // A paragraph can fail both line-count and line-width checks; offer one coherent fix.
        if (Fixes.Any(f => f.Index == index && f.Before == paragraph.Text &&
                           (f.FixKind == ArteFixKind.Rebalance || f.FixKind == ArteFixKind.Split)))
        {
            return;
        }

        string? proposed = null;
        for (var width = TeletextMaxCells; width >= 1; width--)
        {
            var candidate = Utilities.AutoBreakLine(paragraph.Text, width, width + 1, _languageCode);
            if (candidate != paragraph.Text && FitsTeletext(candidate))
            {
                proposed = candidate;
                break;
            }
        }

        if (proposed != null)
        {
            Fixes.Add(new ArteFixItem(true, index, paragraph.Text, proposed,
                reason + " Rebalance line breaks.", ArteFixKind.Rebalance));
        }
        else if (!AddSplitProposals(paragraph, index, reason))
        {
            Fixes.Add(new ArteFixItem(false, index, paragraph.Text, string.Empty,
                reason + " No valid automatic split found; edit manually."));
        }
    }

    private static string RemoveItalicTags(string text) =>
        Regex.Replace(text, @"</?i\s*>", string.Empty, RegexOptions.IgnoreCase);

    private static readonly Regex FontColorAttributeRegex = new(
        "\\bcolor\\s*=\\s*(?:\\\"(?<quoted>[^\\\"]+)\\\"|'(?<single>[^']+)'|(?<bare>[^\\s>]+))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ColoredBoxRegex = new(
        @"<box\b[^>]*\bcolor\s*=",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BoxTagRegex = new(
        @"</?box\b[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> TeletextColorNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Black", "Red", "Green", "Yellow", "Blue", "Magenta", "Cyan", "White",
    };

    private static string NormalizeTeletextColors(string text, bool isSdh, out bool hasUnsupportedColor)
    {
        var unsupportedColorFound = false;
        var normalized = FontColorAttributeRegex.Replace(text, match =>
        {
            var color = match.Groups["quoted"].Success ? match.Groups["quoted"].Value :
                match.Groups["single"].Success ? match.Groups["single"].Value : match.Groups["bare"].Value;
            if (!isSdh)
            {
                if (string.Equals(color, "Yellow", StringComparison.OrdinalIgnoreCase))
                {
                    return "color=\"Yellow\"";
                }

                var normalSubtitleColor = Ebu.GetNearestColorName(color);
                if (normalSubtitleColor == null)
                {
                    unsupportedColorFound = true;
                    return match.Value;
                }

                return "color=\"Yellow\"";
            }

            if (TeletextColorNames.Contains(color))
            {
                return "color=\"" + color + "\"";
            }
            var standardColor = Ebu.GetNearestColorName(color);
            if (standardColor == null)
            {
                unsupportedColorFound = true;
                return match.Value;
            }

            return "color=\"" + standardColor + "\"";
        });
        hasUnsupportedColor = unsupportedColorFound;
        return normalized;
    }

    private void AnalyzeTeletextColors(Subtitle subtitle)
    {
        // A normal ARTE file which deliberately uses a colour must use yellow consistently.
        // A completely uncoloured file remains uncoloured.
        var useYellowForNormalSubtitles = !IsSdh && subtitle.Paragraphs.Any(paragraph => FontColorAttributeRegex.IsMatch(paragraph.Text));

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var text = subtitle.Paragraphs[i].Text;
            // Explicit boxing belongs to SDH only. Every normal ARTE language code
            // has no boxing control: remove it before applying the normal yellow/
            // no-colour rule, otherwise Yellow text on a retained Yellow box is
            // invisible in both the grid and Flow.
            var withoutBoxing = IsSdh ? text : BoxTagRegex.Replace(text, string.Empty);
            var normalized = NormalizeTeletextColors(withoutBoxing, IsSdh, out var hasUnsupportedColor);
            if (useYellowForNormalSubtitles && !string.IsNullOrWhiteSpace(text))
            {
                // A colour can begin on a later line. Remove the individual colour spans
                // and apply one Yellow run to the complete subtitle instead.
                normalized = Regex.Replace(normalized, @"<font\b[^>]*\bcolor\s*=[^>]*>(?<text>.*?)</font\s*>", "${text}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                normalized = "<font color=\"Yellow\">" + normalized + "</font>";
            }
            if (normalized != text)
            {
                Fixes.Add(new ArteFixItem(true, i + 1, text, normalized,
                    IsSdh ? Se.Language.Tools.CheckArteErrors.ColorMapped :
                        BoxTagRegex.IsMatch(text) ? Se.Language.Tools.CheckArteErrors.NormalNoSdhBoxing :
                        Se.Language.Tools.CheckArteErrors.NormalYellowOrNoColor,
                    ArteFixKind.TeletextColor));
            }
            if (hasUnsupportedColor)
            {
                Fixes.Add(new ArteFixItem(false, i + 1, text, string.Empty,
                    Se.Language.Tools.CheckArteErrors.UnsupportedColor));
            }
        }
    }

    private void AnalyzeItalic(Subtitle subtitle)
    {
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var text = subtitle.Paragraphs[i].Text;
            var cleaned = RemoveItalicTags(text);
            if (cleaned != text)
            {
                Fixes.Add(new ArteFixItem(true, i + 1, text, cleaned,
                    Se.Language.Tools.CheckArteErrors.ItalicNotAllowed, ArteFixKind.RemoveItalic));
            }
        }
    }

    private string? RebalanceToTeletext(string text)
    {
        if (FitsTeletext(text))
        {
            return text;
        }
        for (var width = TeletextMaxCells; width >= 1; width--)
        {
            var candidate = Utilities.AutoBreakLine(text, width, width + 1, _languageCode);
            if (FitsTeletext(candidate))
            {
                return candidate;
            }
        }
        return null;
    }

    private bool AddSplitProposals(Paragraph paragraph, int index, string reason)
    {
        bool Alarm(string message)
        {
            Fixes.Add(new ArteFixItem(false, index, paragraph.Text, string.Empty,
                reason + " ALARM: " + message) { ShowTiming = true });
            return true;
        }

        if (Regex.IsMatch(paragraph.Text, @"<font\b", RegexOptions.IgnoreCase))
        {
            return Alarm("Automatic split cannot preserve color tags yet.");
        }
        var source = new Paragraph(paragraph) { Text = RemoveItalicTags(paragraph.Text) };
        // Method 1: use existing line boundaries, then rebalance each resulting subtitle.
        var parts = SplitBreakLongLinesViewModel.Split(
            new SubtitleLineViewModel(source, new Ebu()), TeletextMaxCells * 2, TeletextMaxCells,
            new SplitBreakLongLinesViewModel.SplitOptions { MinimumGapMs = (int)MinimumGapMilliseconds });
        if (parts.Count < 2)
        {
            return Alarm("No valid split found.");
        }
        var proposals = new List<Paragraph>();
        foreach (var part in parts)
        {
            var text = RebalanceToTeletext(part.Text);
            if (text == null)
            {
                return Alarm("The split text cannot be rebalanced within the line limits.");
            }
            proposals.Add(new Paragraph(paragraph, true) { Text = text });
        }
        static string Words(string value) => Regex.Replace(HtmlUtil.RemoveHtmlTags(value, true), @"\s+", " ").Trim();
        if (Words(string.Join(" ", proposals.Select(p => p.Text))) != Words(paragraph.Text))
        {
            return Alarm("The split would change the subtitle text.");
        }
        if (!ArteSplitTiming.TryFit(proposals, paragraph.StartTime.TotalMilliseconds,
                paragraph.EndTime.TotalMilliseconds, Se.Settings.General.SubtitleMinimumDisplayMilliseconds,
                Se.Settings.General.SubtitleMaximumDisplayMilliseconds,
                Se.Settings.General.SubtitleMaximumCharactersPerSeconds, MinimumGapFrames,
                ReadingDurationTolerancePercent, AcceptShortDurations, ShortMinimumFrames,
                out var timingError))
        {
            return Alarm(timingError);
        }
        var preview = string.Join(Environment.NewLine + Environment.NewLine,
            proposals.Select((p, n) => $"{n + 1}. {FormatTimeRange(p)}{Environment.NewLine}{p.Text}"));
        Fixes.Add(new ArteFixItem(true, index, paragraph.Text, preview,
            reason + " Split and rebalance (method 1); fitted inside the original time range." +
            (RemoveItalicTags(paragraph.Text) != paragraph.Text ? " Removes forbidden italic tags." : string.Empty),
            ArteFixKind.Split) { SplitParagraphs = proposals, TimingNote = timingError });
        return true;
    }

    private void AnalyzeUnneededSpaces(Subtitle subtitle)
    {
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
            if (Fixes.Any(f => f.Index == i + 1 && f.FixKind == ArteFixKind.Rebalance))
            {
                continue;
            }

            var lines = paragraph.Text
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n');

            var cleanedLines = lines
                .Select(line => line.Trim())
                .ToArray();

            var cleaned = string.Join(Environment.NewLine, cleanedLines);
            if (cleaned == paragraph.Text)
            {
                continue;
            }

            Fixes.Add(new ArteFixItem(
                true,
                i + 1,
                paragraph.Text,
                cleaned,
                "Leading or trailing spaces.",
                ArteFixKind.UnneededSpaces));
        }
    }

    internal static string FormatTimeRange(Paragraph paragraph)
    {
        static string FrameTime(double milliseconds)
        {
            var frames = (long)Math.Round(milliseconds / 40);
            return $"{frames / 90000:00}:{frames / 1500 % 60:00}:{frames / 25 % 60:00}:{frames % 25:00}";
        }
        var durationFrames = (long)Math.Round((paragraph.EndTime.TotalMilliseconds - paragraph.StartTime.TotalMilliseconds) / 40);
        return $"{FrameTime(paragraph.StartTime.TotalMilliseconds)} → {FrameTime(paragraph.EndTime.TotalMilliseconds)} | {durationFrames / 25} s {durationFrames % 25:00} fr";
    }

    [RelayCommand]
    private async Task DownloadErrorReport()
    {
        if (Window == null)
        {
            return;
        }

        var entries = GetReportEntries();
        if (entries.Count == 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                Se.Language.Tools.CheckArteErrors.NothingToReport,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var report = string.Join(
            Environment.NewLine + Environment.NewLine,
            entries);

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            ".txt",
            "arte_error_report.txt",
            Se.Language.Tools.CheckArteErrors.SaveErrorReport);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        System.IO.File.WriteAllText(fileName, report);

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(
            Window,
            vm =>
            {
                vm.Initialize(
                    Se.Language.Tools.CheckArteErrors.ErrorReportSaved,
                    string.Format(Se.Language.Tools.CheckArteErrors.ErrorReportSavedToX, fileName),
                    fileName,
                    true,
                    true);
            });
    }

    public IReadOnlyList<string> GetReportEntries()
    {
        return Fixes
            .GroupBy(f => f.GroupName)
            .Select(group =>
            {
                var entries = group.Select(f =>
                {
                    var subtitle = string.IsNullOrEmpty(f.IndexDisplay) ? string.Empty : $"UT {f.IndexDisplay}";
                    var location = string.IsNullOrEmpty(f.TimeRange)
                        ? subtitle
                        : string.IsNullOrEmpty(subtitle) ? f.TimeRange : $"{subtitle} | {f.TimeRange}";
                    var message = !string.IsNullOrEmpty(f.TimingNote) ? $"{f.Reason} {f.TimingNote}" : f.Reason;
                    return string.IsNullOrEmpty(location)
                        ? $"• {message}"
                        : $"• {location}{Environment.NewLine}  {message}";
                });

                return $"{group.Key} ({group.Count()}){Environment.NewLine}{Environment.NewLine}" +
                       string.Join(Environment.NewLine + Environment.NewLine, entries);
            })
            .ToList();
    }

    [RelayCommand]
    private void Ok()
    {
        if (_sourceSnapshot == null)
        {
            return;
        }

        var sourceFrameRate = SelectedSourceFrameRate;
        var convertFrameRate = Math.Abs(sourceFrameRate - 25.0) > 0.001;
        var fixedSubtitle = new Subtitle(_sourceSnapshot, generateNewId: false);
        var applied = 0;

        // Analyze() created all timing proposals on a detached 25 fps target copy.
        // Apply the same conversion first so those proposals match this correction copy.
        if (convertFrameRate)
        {
            fixedSubtitle.ChangeFrameRate(sourceFrameRate, 25.0);
            applied++;
        }

        foreach (var fix in Fixes.Where(f => f.CanBeFixed && f.Apply)
                     .OrderBy(f => f.FixKind == ArteFixKind.TeletextLinePosition ? 0 : f.FixKind == ArteFixKind.RemoveItalic ? 2 : 1))
        {
            if (fix.FixKind == ArteFixKind.Header)
            {
                fixedSubtitle.Header = fix.ProposedHeader!;
                applied++;
                continue;
            }
            if (fix.Index <= 0 || fix.Index > fixedSubtitle.Paragraphs.Count)
            {
                continue;
            }

            var paragraph = fixedSubtitle.Paragraphs[fix.Index - 1];

            switch (fix.FixKind)
            {
                case ArteFixKind.FrameAccurateTimeCode:
                    paragraph.StartTime = new TimeCode(fix.ProposedStartMs!.Value);
                    paragraph.EndTime = new TimeCode(fix.ProposedEndMs!.Value);
                    applied++;
                    break;

                case ArteFixKind.DisplayDuration:
                    paragraph.EndTime = new TimeCode(fix.ProposedEndMs!.Value);
                    applied++;
                    break;

                case ArteFixKind.MinimumGap:
                    paragraph.EndTime = new TimeCode(fix.ProposedEndMs!.Value);
                    if (fix.ProposedStartMs.HasValue && fix.Index < fixedSubtitle.Paragraphs.Count)
                    {
                        fixedSubtitle.Paragraphs[fix.Index].StartTime = new TimeCode(fix.ProposedStartMs.Value);
                    }
                    applied++;
                    break;

                case ArteFixKind.TeletextLinePosition:
                    paragraph.MarginV = fix.After;
                    applied++;
                    break;

                case ArteFixKind.Rebalance:
                    paragraph.MarginV = TeletextRowHelper.GetRowKeepingBottomEdge(
                        paragraph.MarginV,
                        paragraph.Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Length,
                        fix.After.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Length,
                        Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)?.ToString() ?? paragraph.MarginV;
                    paragraph.Text = fix.After;
                    applied++;
                    break;

                case ArteFixKind.RemoveItalic:
                    paragraph.Text = RemoveItalicTags(paragraph.Text);
                    applied++;
                    break;

                case ArteFixKind.UnneededSpaces:
                    paragraph.Text = fix.After;
                    applied++;
                    break;

                case ArteFixKind.TeletextColor:
                    paragraph.Text = fix.After;
                    applied++;
                    break;
            }
        }

        // Split only after all original-index edits. Work backwards so later indices stay valid.
        foreach (var fix in Fixes.Where(f => f.Apply && f.FixKind == ArteFixKind.Split)
                     .OrderByDescending(f => f.Index))
        {
            var original = fixedSubtitle.Paragraphs[fix.Index - 1];
            var removeItalic = Fixes.Any(f => f.Index == fix.Index && f.Apply && f.FixKind == ArteFixKind.RemoveItalic);
            var trimSpaces = Fixes.Any(f => f.Index == fix.Index && f.Apply && f.FixKind == ArteFixKind.UnneededSpaces);
            var replacements = fix.SplitParagraphs!.Select(part =>
            {
                var result = new Paragraph(original, true)
                {
                    Text = removeItalic ? RemoveItalicTags(part.Text) : part.Text,
                    StartTime = new TimeCode(part.StartTime.TotalMilliseconds),
                    EndTime = new TimeCode(part.EndTime.TotalMilliseconds),
                };
                if (trimSpaces)
                {
                    result.Text = string.Join(Environment.NewLine, result.Text.SplitToLines().Select(line => line.Trim()));
                }
                result.MarginV = TeletextRowHelper.GetRowKeepingBottomEdge(original.MarginV,
                    original.Text.SplitToLines().Count, result.Text.SplitToLines().Count,
                    Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)?.ToString() ?? original.MarginV;
                return result;
            }).ToList();
            // Selected GAP corrections may have moved the original start or end before splitting.
            replacements[0].StartTime = new TimeCode(original.StartTime.TotalMilliseconds);
            replacements[^1].EndTime = new TimeCode(original.EndTime.TotalMilliseconds);
            fixedSubtitle.Paragraphs.RemoveAt(fix.Index - 1);
            fixedSubtitle.Paragraphs.InsertRange(fix.Index - 1, replacements);
            applied++;
        }

        // Insert after all original-index based corrections and splits, so adding the
        // leading control subtitle cannot shift any correction target.
        foreach (var fix in Fixes.Where(f => f.Apply && f.FixKind == ArteFixKind.CreateBlankSubtitle))
        {
            fixedSubtitle.Paragraphs.Insert(0, new Paragraph(fix.ProposedParagraph!, true));
            applied++;
        }

        foreach (var fix in Fixes.Where(f => f.Apply && f.FixKind == ArteFixKind.ShiftStartTimeCode))
        {
            var offset = fix.ProposedStartMs!.Value;
            foreach (var paragraph in fixedSubtitle.Paragraphs)
            {
                paragraph.StartTime = new TimeCode(paragraph.StartTime.TotalMilliseconds + offset);
                paragraph.EndTime = new TimeCode(paragraph.EndTime.TotalMilliseconds + offset);
            }
            if (Ebu.IsStlHeader(fixedSubtitle.Header))
            {
                var header = Ebu.ReadHeader(Ebu.GetEncoding(fixedSubtitle.Header.Substring(0, 3)).GetBytes(fixedSubtitle.Header));
                var startFrames = Math.Max(0L, (long)Math.Round(GetStartTimeCodeReference(fixedSubtitle) / 40.0));
                header.TimeCodeStartOfProgramme = $"{startFrames / 90000:00}{startFrames / 1500 % 60:00}{startFrames / 25 % 60:00}{startFrames % 25:00}";
                fixedSubtitle.Header = header.ToString();
            }
            applied++;
        }

        if (applied == 0)
        {
            Analyze();
            return;
        }

        fixedSubtitle.Renumber();

        // Capture the complete pre-correction state, including the source frame-rate
        // selection, so Undo can restore both the subtitle and the ARTE setup.
        _undoHistory.Push((
            new Subtitle(_sourceSnapshot, generateNewId: false),
            AppliedFixCount,
            new List<string>(_appliedNotes),
            sourceFrameRate));

        _appliedNotes.AddRange(Fixes.Where(f => f.Apply && !string.IsNullOrEmpty(f.TimingNote))
            .Select(f => $"UT {f.IndexDisplay} | {f.TimeRange}: {f.TimingNote}"));
        if (convertFrameRate)
        {
            _appliedNotes.Add($"Frame rate converted from {sourceFrameRate:0.###} to 25 fps.");
        }

        // Publish only the complete correction pass.
        ApplyToMainSubtitle?.Invoke(new Subtitle(fixedSubtitle, generateNewId: false));
        FixedSubtitle = new Subtitle(fixedSubtitle, generateNewId: false);
        _sourceSnapshot = new Subtitle(fixedSubtitle, generateNewId: false);
        AppliedFixCount += applied;
        OkPressed = true;

        // The published subtitle is now on the 25 fps target timeline. Reset the
        // selector without triggering an intermediate analysis pass.
        if (convertFrameRate)
        {
            SetSourceFrameRateWithoutAnalysis(25.0);
        }

        UndoCommand.NotifyCanExecuteChanged();
        Analyze();
        FixesSummaryText = convertFrameRate
            ? string.Format(Se.Language.Tools.CheckArteErrors.FrameRateAndCorrectionsAppliedX,
                sourceFrameRate.ToString("0.###"), applied - 1)
            : string.Format(Se.Language.Tools.CheckArteErrors.CorrectionsAppliedX, applied);
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoHistory.Count == 0)
        {
            return;
        }

        var previous = _undoHistory.Pop();
        ApplyToMainSubtitle?.Invoke(new Subtitle(previous.Subtitle, generateNewId: false));
        _sourceSnapshot = new Subtitle(previous.Subtitle, generateNewId: false);
        FixedSubtitle = new Subtitle(previous.Subtitle, generateNewId: false);
        AppliedFixCount = previous.AppliedCount;
        _appliedNotes.Clear();
        _appliedNotes.AddRange(previous.Notes);
        SetSourceFrameRateWithoutAnalysis(previous.SourceFrameRate);
        UndoCommand.NotifyCanExecuteChanged();
        Analyze();
        FixesSummaryText = Se.Language.Tools.CheckArteErrors.LastCorrectionPassUndone;
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

    public void OnLoaded(RoutedEventArgs e)
    {
        Analyze();
    }
}

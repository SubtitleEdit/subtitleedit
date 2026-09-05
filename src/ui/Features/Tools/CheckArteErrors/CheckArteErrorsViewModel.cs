using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main.FlowEditing;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

public partial class CheckArteErrorsViewModel : ObservableObject
{
    internal enum ArteFixKind
    {
        None,
        TeletextLinePosition,
        UnneededSpaces,
    }

    private Subtitle? _sourceSnapshot;
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

        [ObservableProperty]
        private bool _isSelected = true;

        public ArteCheckItem(string name)
        {
            Name = name;
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
        internal ArteFixKind FixKind { get; }

        [ObservableProperty]
        private bool _apply;

        internal ArteFixItem(
            bool canBeFixed,
            int index,
            string before,
            string after,
            string reason,
            ArteFixKind fixKind = ArteFixKind.None)
        {
            CanBeFixed = canBeFixed;
            Index = index;
            Before = before;
            After = after;
            Reason = reason;
            FixKind = fixKind;
            _apply = canBeFixed;
        }
    }

    [ObservableProperty]
    private ArteProfileItem? _selectedProfile;

    [ObservableProperty]
    private ArteFixItem? _selectedFix;

    [ObservableProperty]
    private string _fixesSummaryText = "No ARTE checks have been run yet.";

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
        new("EBU STL / 25 fps"),
        new("Header / language data"),
        new("ARTE blank subtitle"),
        new("Frame-accurate time codes"),
        new("Overlapping display times"),
        new("Minimum gaps"),
        new("Display duration"),
        new("Maximum two lines"),
        new("Teletext line position"),
        new("Teletext line length / control codes"),
        new("Teletext colors"),
        new("Unneeded spaces"),
    ];

    public ObservableCollection<ArteFixItem> Fixes { get; } = new();

    public CheckArteErrorsViewModel()
    {
        SelectedProfile = Profiles.FirstOrDefault();
    }

    public void Initialize(Subtitle subtitle)
    {
        // ARTE UT Norm: minimum gap is five frames at 25 fps. Keep both stored
        // representations in sync because Subtitle Edit can use frame or ms mode.
        // This is intentionally persisted as the global Subtitle Edit setting.
        const int arteGapFrames = 5;

        var minimumGap = Se.Settings.General.MinimumBetweenLines;
        if (minimumGap.Frames != arteGapFrames)
        {
            minimumGap.Frames = arteGapFrames;
            Se.SaveSettings();
        }

        // Keep an independent copy for every analysis pass. The live subtitle in
        // MainViewModel is deliberately not exposed to this tool.
        _sourceSnapshot = new Subtitle(subtitle, generateNewId: false);
        FixedSubtitle = null;
        OkPressed = false;
        AppliedFixCount = 0;
        Fixes.Clear();
        FixesSummaryText = $"{_sourceSnapshot.Paragraphs.Count} subtitle(s) loaded for ARTE analysis.";
    }

    [RelayCommand]
    private void ChecksSelectAll()
    {
        foreach (var check in Checks)
        {
            check.IsSelected = true;
        }
    }

    [RelayCommand]
    private void ChecksInverseSelection()
    {
        foreach (var check in Checks)
        {
            check.IsSelected = !check.IsSelected;
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
    private void Analyze()
    {
        Fixes.Clear();

        if (SelectedProfile == null)
        {
            FixesSummaryText = "Select an ARTE profile first.";
            return;
        }

        if (_sourceSnapshot == null)
        {
            FixesSummaryText = "No subtitle snapshot is available.";
            return;
        }

        var selectedChecks = Checks
            .Where(c => c.IsSelected)
            .Select(c => c.Name)
            .ToHashSet();

        if (selectedChecks.Count == 0)
        {
            FixesSummaryText = "No ARTE checks selected.";
            return;
        }

        // Analyze another detached copy so even future rule implementations cannot
        // accidentally change the snapshot supplied by MainViewModel.
        var subtitle = new Subtitle(_sourceSnapshot, generateNewId: false);

        if (selectedChecks.Contains("ARTE blank subtitle"))
        {
            AnalyzeBlankSubtitle(subtitle);
        }

        // Text structure and Teletext layout are analyzed before gaps. Future
        // split/rebalance fixes can therefore run before gap normalization.
        if (selectedChecks.Contains("Maximum two lines"))
        {
            AnalyzeMaximumTwoLines(subtitle);
        }

        if (selectedChecks.Contains("Teletext line position"))
        {
            AnalyzeTeletextLinePosition(subtitle);
        }

        if (selectedChecks.Contains("Teletext line length / control codes"))
        {
            AnalyzeTeletextLineLength(subtitle);
        }

        if (selectedChecks.Contains("Unneeded spaces"))
        {
            AnalyzeUnneededSpaces(subtitle);
        }

        if (selectedChecks.Contains("Minimum gaps"))
        {
            AnalyzeMinimumGaps(subtitle);
        }

        if (selectedChecks.Contains("Overlapping display times"))
        {
            AnalyzeOverlaps(subtitle);
        }

        FixesSummaryText = Fixes.Count == 0
            ? $"No issues found by the {selectedChecks.Count} selected check(s) currently implemented."
            : $"{Fixes.Count} issue(s) found. Analysis did not modify the subtitle.";
    }

    private void AnalyzeBlankSubtitle(Subtitle subtitle)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var first = subtitle.Paragraphs[0];
        if (!string.IsNullOrWhiteSpace(first.Text))
        {
            Fixes.Add(new ArteFixItem(
                false,
                0,
                "Missing",
                "Create ARTE blank subtitle",
                "Required ARTE blank/control subtitle is missing before the first normal subtitle. Exact control data/time position is not guessed by this analysis step."));
        }
    }

    private void AnalyzeMinimumGaps(Subtitle subtitle)
    {
        const double arteFrameRate = 25.0;
        const int minimumGapFrames = 5;

        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            var previous = subtitle.Paragraphs[i - 1];
            var current = subtitle.Paragraphs[i];
            var gapMs = current.StartTime.TotalMilliseconds - previous.EndTime.TotalMilliseconds;
            var gapFrames = (int)Math.Round(
                gapMs * arteFrameRate / 1000.0,
                MidpointRounding.AwayFromZero);

            if (gapFrames >= minimumGapFrames)
            {
                continue;
            }

            Fixes.Add(new ArteFixItem(
                false,
                i + 1,
                $"{gapFrames} frame{(Math.Abs(gapFrames) == 1 ? string.Empty : "s")}",
                $"{minimumGapFrames} frames minimum",
                gapFrames < 0
                    ? $"Overlap with subtitle {i}; minimum gap is not met."
                    : $"Gap after subtitle {i} is below the ARTE minimum of {minimumGapFrames} frames."));
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
                    "File appears vertically shifted by one Teletext row; relative position is preserved.",
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
                "Not set",
                expectedBottomRow.ToString(),
                $"No Teletext position is set; propose bottom position for {lineCount}-line subtitle.",
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

            var projection =
                FlowInlineColorProjection.Parse(paragraph.Text);

            var visibleText = projection.VisibleText
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            var lines = visibleText.Split('\n');
            var lineStart = 0;

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                var colorCodeCount = projection.ColorRuns.Count(
                    run =>
                        run.Start < lineStart + line.Length &&
                        run.End > lineStart);

                var maximum = Math.Max(1, 37 - colorCodeCount);

                if (line.Length > maximum)
                {
                    Fixes.Add(new ArteFixItem(
                        false,
                        i + 1,
                        $"Line {lineIndex + 1}: {line.Length} chars",
                        $"Maximum {maximum} chars",
                        $"Teletext line exceeds the available width ({colorCodeCount} color control code(s))."));
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
                Fixes.Add(new ArteFixItem(
                    false,
                    i + 1,
                    paragraph.Text,
                    string.Empty,
                    $"Maximum two lines exceeded ({lineCount} lines)."));
            }
        }
    }

    private void AnalyzeUnneededSpaces(Subtitle subtitle)
    {
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
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

    private static string FormatTimeRange(Paragraph paragraph)
    {
        return $"{paragraph.StartTime} -> {paragraph.EndTime}";
    }

    [RelayCommand]
    private void GenerateReport()
    {
        FixesSummaryText = Fixes.Count == 0
            ? "Nothing to report yet."
            : $"{Fixes.Count} ARTE result(s) ready for report.";
    }

    [RelayCommand]
    private void Ok()
    {
        if (_sourceSnapshot == null)
        {
            return;
        }

        var fixedSubtitle = new Subtitle(_sourceSnapshot, generateNewId: false);
        var applied = 0;

        foreach (var fix in Fixes.Where(f => f.CanBeFixed && f.Apply))
        {
            if (fix.Index <= 0 || fix.Index > fixedSubtitle.Paragraphs.Count)
            {
                continue;
            }

            var paragraph = fixedSubtitle.Paragraphs[fix.Index - 1];

            switch (fix.FixKind)
            {
                case ArteFixKind.TeletextLinePosition:
                    paragraph.MarginV = fix.After;
                    applied++;
                    break;

                case ArteFixKind.UnneededSpaces:
                    paragraph.Text = fix.After;
                    applied++;
                    break;
            }
        }

        FixedSubtitle = fixedSubtitle;
        AppliedFixCount = applied;
        OkPressed = true;
        Window?.Close();
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

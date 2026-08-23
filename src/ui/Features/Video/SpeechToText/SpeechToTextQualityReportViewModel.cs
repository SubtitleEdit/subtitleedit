using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>One row in the quality report table.</summary>
public class QualityReportDisplayItem
{
    public SpeechToTextQualityIssueType? Type { get; init; }
    public bool IsRemoved { get; init; }
    public int Number { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Start { get; init; } = string.Empty;
    public string End { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public IBrush Brush { get; init; } = Brushes.Gray;
}

public partial class SpeechToTextQualityReportViewModel : ObservableObject
{
    [ObservableProperty] private string _summary = string.Empty;
    [ObservableProperty] private string _tip = string.Empty;
    [ObservableProperty] private bool _doNotShowAgain;
    [ObservableProperty] private bool _hasIssues;
    [ObservableProperty] private ObservableCollection<QualityReportDisplayItem> _items = new();
    [ObservableProperty] private QualityReportDisplayItem? _selectedItem;

    public ObservableCollection<SummaryCard> Cards { get; } = new();

    /// <summary>Card key for the "removed lines" filter (the issue-type keys are the enum values).</summary>
    private static readonly object RemovedKey = new();

    public int TotalLines { get; private set; }
    public int IssueCount { get; private set; }
    public int RemovedCount { get; private set; }

    public Window? Window { get; set; }

    private readonly List<QualityReportDisplayItem> _allItems = new();

    public static readonly IBrush TooShortBrush = new SolidColorBrush(Color.Parse("#E8912D"));
    public static readonly IBrush TooLongBrush = new SolidColorBrush(Color.Parse("#9B59B6"));
    public static readonly IBrush OverlapBrush = new SolidColorBrush(Color.Parse("#E74C3C"));
    public static readonly IBrush NonSpeechBrush = new SolidColorBrush(Color.Parse("#3498DB"));
    public static readonly IBrush RepeatedBrush = new SolidColorBrush(Color.Parse("#1ABC9C"));
    public static readonly IBrush RemovedBrush = new SolidColorBrush(Color.Parse("#7F8C8D"));
    public static readonly IBrush AllBrush = new SolidColorBrush(Color.Parse("#5D8AA8"));

    public static IBrush GetBrush(SpeechToTextQualityIssueType type)
    {
        return type switch
        {
            SpeechToTextQualityIssueType.TooShort => TooShortBrush,
            SpeechToTextQualityIssueType.TooLong => TooLongBrush,
            SpeechToTextQualityIssueType.Overlap => OverlapBrush,
            SpeechToTextQualityIssueType.NonSpeech => NonSpeechBrush,
            SpeechToTextQualityIssueType.Repeated => RepeatedBrush,
            _ => AllBrush,
        };
    }

    public static string GetLabel(SpeechToTextQualityIssueType type)
    {
        var l = Se.Language.Video.AudioToText;
        return type switch
        {
            SpeechToTextQualityIssueType.TooShort => l.QualityReportTooShort,
            SpeechToTextQualityIssueType.TooLong => l.QualityReportTooLong,
            SpeechToTextQualityIssueType.Overlap => l.QualityReportOverlap,
            SpeechToTextQualityIssueType.NonSpeech => l.QualityReportNonSpeech,
            SpeechToTextQualityIssueType.Repeated => l.QualityReportRepeated,
            _ => string.Empty,
        };
    }

    private static string GetHint(SpeechToTextQualityIssueType type)
    {
        var l = Se.Language.Video.AudioToText;
        return type switch
        {
            SpeechToTextQualityIssueType.TooShort => l.QualityReportTooShortHint,
            SpeechToTextQualityIssueType.TooLong => l.QualityReportTooLongHint,
            SpeechToTextQualityIssueType.Overlap => l.QualityReportOverlapHint,
            SpeechToTextQualityIssueType.NonSpeech => l.QualityReportNonSpeechHint,
            SpeechToTextQualityIssueType.Repeated => l.QualityReportRepeatedHint,
            _ => string.Empty,
        };
    }

    public void Initialize(SpeechToTextQualityReport report)
    {
        var l = Se.Language.Video.AudioToText;
        TotalLines = report.TotalLines;
        IssueCount = report.Issues.Count;
        RemovedCount = report.Removed.Count;
        HasIssues = report.HasIssues;
        Tip = l.QualityReportTip;
        Summary = report.Issues.Count == 0 && report.Removed.Count == 0
            ? l.QualityReportNoIssues
            : string.Format(l.QualityReportSummaryX, report.Issues.Count, report.TotalLines);

        _allItems.Clear();
        foreach (var issue in report.Issues.OrderBy(p => p.Number).ThenBy(p => p.Type))
        {
            _allItems.Add(MakeItem(issue, false));
        }

        foreach (var issue in report.Removed.OrderBy(p => p.Number))
        {
            _allItems.Add(MakeItem(issue, true));
        }

        Cards.Clear();
        Cards.Add(new SummaryCard { Key = null, Label = l.QualityReportAll, Count = report.Issues.Count, Brush = AllBrush, IsActive = true });
        foreach (var type in new[]
                 {
                     SpeechToTextQualityIssueType.TooShort,
                     SpeechToTextQualityIssueType.TooLong,
                     SpeechToTextQualityIssueType.Overlap,
                     SpeechToTextQualityIssueType.NonSpeech,
                     SpeechToTextQualityIssueType.Repeated,
                 })
        {
            Cards.Add(new SummaryCard { Key = type, Label = GetLabel(type), Hint = GetHint(type), Count = report.Count(type), Brush = GetBrush(type) });
        }

        if (report.Removed.Count > 0)
        {
            Cards.Add(new SummaryCard { Key = RemovedKey, Label = l.QualityReportRemoved, Count = report.Removed.Count, Brush = RemovedBrush });
        }

        ApplyFilter(Cards[0]);
    }

    private static QualityReportDisplayItem MakeItem(SpeechToTextQualityIssue issue, bool removed)
    {
        var l = Se.Language.Video.AudioToText;
        return new QualityReportDisplayItem
        {
            Type = issue.Type,
            IsRemoved = removed,
            Number = issue.Number,
            Category = removed ? $"{l.QualityReportRemoved}: {GetLabel(issue.Type)}" : GetLabel(issue.Type),
            Start = issue.StartTime.ToShortDisplayString(),
            End = issue.EndTime.ToShortDisplayString(),
            Detail = issue.Detail,
            Text = issue.Text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' '),
            Brush = removed ? RemovedBrush : GetBrush(issue.Type),
        };
    }

    [RelayCommand]
    private void SetFilter(SummaryCard? card)
    {
        if (card == null)
        {
            return;
        }

        ApplyFilter(card);
    }

    private void ApplyFilter(SummaryCard card)
    {
        foreach (var c in Cards)
        {
            c.IsActive = ReferenceEquals(c, card);
        }

        IEnumerable<QualityReportDisplayItem> filtered = _allItems;
        if (ReferenceEquals(card.Key, RemovedKey))
        {
            filtered = _allItems.Where(p => p.IsRemoved);
        }
        else if (card.Key is SpeechToTextQualityIssueType type)
        {
            filtered = _allItems.Where(p => !p.IsRemoved && p.Type == type);
        }
        else
        {
            filtered = _allItems.Where(p => !p.IsRemoved);
        }

        Items = new ObservableCollection<QualityReportDisplayItem>(filtered);
        SelectedItem = Items.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
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
}

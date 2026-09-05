using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    [ObservableProperty] private bool _canExport;
    [ObservableProperty] private string _exportStatus = string.Empty;

    public ObservableCollection<SummaryCard> Cards { get; } = new();

    /// <summary>Card key for the "removed lines" filter (the issue-type keys are the enum values).</summary>
    private static readonly object RemovedKey = new();

    public int TotalLines { get; private set; }
    public int IssueCount { get; private set; }
    public int RemovedCount { get; private set; }

    public Window? Window { get; set; }

    private readonly List<QualityReportDisplayItem> _allItems = new();
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string? _fileName;

    private const string TooShortColor = "#E8912D";
    private const string TooLongColor = "#9B59B6";
    private const string OverlapColor = "#E74C3C";
    private const string NonSpeechColor = "#3498DB";
    private const string RepeatedColor = "#1ABC9C";
    private const string RemovedColor = "#7F8C8D";
    private const string AllColor = "#5D8AA8";

    public static readonly IBrush TooShortBrush = new ImmutableSolidColorBrush(Color.Parse(TooShortColor));
    public static readonly IBrush TooLongBrush = new ImmutableSolidColorBrush(Color.Parse(TooLongColor));
    public static readonly IBrush OverlapBrush = new ImmutableSolidColorBrush(Color.Parse(OverlapColor));
    public static readonly IBrush NonSpeechBrush = new ImmutableSolidColorBrush(Color.Parse(NonSpeechColor));
    public static readonly IBrush RepeatedBrush = new ImmutableSolidColorBrush(Color.Parse(RepeatedColor));
    public static readonly IBrush RemovedBrush = new ImmutableSolidColorBrush(Color.Parse(RemovedColor));
    public static readonly IBrush AllBrush = new ImmutableSolidColorBrush(Color.Parse(AllColor));

    public SpeechToTextQualityReportViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
    }

    /// <summary>The dot colour as "#RRGGBB" - the html export paints the same palette as the window.</summary>
    private static string GetColor(SpeechToTextQualityIssueType type)
    {
        return type switch
        {
            SpeechToTextQualityIssueType.TooShort => TooShortColor,
            SpeechToTextQualityIssueType.TooLong => TooLongColor,
            SpeechToTextQualityIssueType.Overlap => OverlapColor,
            SpeechToTextQualityIssueType.NonSpeech => NonSpeechColor,
            SpeechToTextQualityIssueType.Repeated => RepeatedColor,
            _ => AllColor,
        };
    }

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

    /// <param name="fileName">The video/audio that was transcribed - named in the exports.</param>
    public void Initialize(SpeechToTextQualityReport report, string? fileName = null)
    {
        var l = Se.Language.Video.AudioToText;
        _fileName = fileName;
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
        CanExport = Items.Count > 0;
    }

    /// <summary>
    /// Puts what the window shows - the active card filter included - on the clipboard, tab
    /// separated, so it pastes as columns into Excel/Sheets and as a readable block anywhere else.
    /// </summary>
    [RelayCommand]
    private async Task CopyToClipboard()
    {
        if (Window == null || Items.Count == 0)
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, ReportExport.ToTabSeparated(MakeExportData()));

        // Not awaited: the command would stay "running" - and its menu item disabled - for as
        // long as the status is shown.
        _ = ShowExportStatus(Se.Language.General.CopiedToClipboard);
    }

    [RelayCommand]
    private Task ExportText()
    {
        return ExportToFile(".txt", () => Encoding.UTF8.GetBytes(ReportExport.ToPlainText(MakeExportData())));
    }

    [RelayCommand]
    private Task ExportExcel()
    {
        return ExportToFile(".xlsx", () => ReportExport.ToXlsx(MakeExportData()));
    }

    [RelayCommand]
    private Task ExportHtml()
    {
        return ExportToFile(".html", () => Encoding.UTF8.GetBytes(ReportExport.ToHtml(MakeExportData())));
    }

    private async Task ExportToFile(string extension, Func<byte[]> makeContent)
    {
        if (Window == null || Items.Count == 0)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, extension, GetSuggestedFileName() + extension, Se.Language.General.Export);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await File.WriteAllBytesAsync(fileName, makeContent());

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.FileSaved, string.Format(Se.Language.General.FileSavedToX, fileName), fileName, true, true);
        });
    }

    private string GetSuggestedFileName()
    {
        var name = string.IsNullOrEmpty(_fileName)
            ? string.Empty
            : Path.GetFileNameWithoutExtension(_fileName);

        return string.IsNullOrWhiteSpace(name) ? "transcription-report" : name + "-transcription-report";
    }

    /// <summary>Clipboard copies are silent otherwise - the status line says it happened, then fades.</summary>
    private async Task ShowExportStatus(string text)
    {
        ExportStatus = text;
        await Task.Delay(3000);
        if (ExportStatus == text)
        {
            ExportStatus = string.Empty;
        }
    }

    /// <summary>The rows as shown - filter included - plus one chip per card, in the window's colours.</summary>
    private ReportExportData MakeExportData()
    {
        var l = Se.Language.Video.AudioToText;
        var rows = Items.ToList();
        var chips = new List<ReportExportChip>();
        foreach (var card in Cards.Skip(1))
        {
            var id = ChipId(card.Key);
            chips.Add(new ReportExportChip
            {
                Id = id,
                Label = card.Label,
                Hint = card.Hint,
                Color = ReferenceEquals(card.Key, RemovedKey) ? RemovedColor : GetColor((SpeechToTextQualityIssueType)card.Key!),
                Count = rows.Count(p => ChipId(p) == id),
            });
        }

        return new ReportExportData
        {
            Title = l.QualityReportTitle,
            Summary = Summary,
            FileName = _fileName,
            CategoryHeader = l.QualityReportIssue,
            DetailHeader = l.QualityReportDetail,
            AllLabel = l.QualityReportAll,
            AllColor = AllColor,
            Chips = chips,
            Rows = rows.Select(item => new ReportExportRow
            {
                Number = item.Number,
                Category = item.Category,
                Show = item.Start,
                Hide = item.End,
                Detail = item.Detail,
                Text = item.Text,
                ChipId = ChipId(item),
                Color = item.IsRemoved ? RemovedColor : GetColor(item.Type ?? SpeechToTextQualityIssueType.TooShort),
            }).ToList(),
        };
    }

    private static string ChipId(object? cardKey)
    {
        return ReferenceEquals(cardKey, RemovedKey) ? "removed" : ((int)(SpeechToTextQualityIssueType)cardKey!).ToString();
    }

    private static string ChipId(QualityReportDisplayItem item)
    {
        return item.IsRemoved ? "removed" : ((int)(item.Type ?? SpeechToTextQualityIssueType.TooShort)).ToString();
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

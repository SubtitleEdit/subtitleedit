using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareViewModel : ObservableObject
{
    public ObservableCollection<CompareItem> LeftSubtitles { get; } = new();
    public ObservableCollection<CompareItem> RightSubtitles { get; } = new();
    public ObservableCollection<CompareVisual> CompareVisuals { get; } = new();

    [ObservableProperty] private CompareItem? _selectedLeft;
    [ObservableProperty] private CompareItem? _selectedRight;
    [ObservableProperty] private bool _ignoreFormatting;
    [ObservableProperty] private bool _ignoreWhiteSpace;
    [ObservableProperty] private bool _isReloadFromFileVisible;
    [ObservableProperty] private bool _isExportVisible;
    [ObservableProperty] private string _leftFileName = string.Empty;
    [ObservableProperty] private bool _leftFileNameHasChanges;
    [ObservableProperty] private string _rightFileName = string.Empty;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private CompareVisual _selectedCompareVisual;

    // Display properties for shortened file names in UI
    public string LeftFileNameDisplay => GetShortFileName(LeftFileName);
    public string RightFileNameDisplay => GetShortFileName(RightFileName);

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public DataGrid? LeftDataGrid { get; set; } = new();
    public DataGrid? RightDataGrid { get; set; } = new();

    private IFileHelper _fileHelper;
    private IFolderHelper _folderHelper;
    private List<SubtitleLineViewModel> _leftLines = new();
    private List<SubtitleLineViewModel> _rightLines = new();
    private string _language = string.Empty;

    private static readonly IBrush ListViewRed = new SolidColorBrush(Color.FromArgb(180, 255, 235, 233));
    private static readonly IBrush ListViewGreen = new SolidColorBrush(Color.FromArgb(180, 230, 255, 237));
    private static readonly IBrush ListViewOrange = new SolidColorBrush(Color.FromArgb(180, 255, 248, 220));
    private static readonly IBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);

    public CompareViewModel(IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        CompareVisuals = new ObservableCollection<CompareVisual>(CompareVisual.GetCompareVisuals());
        SelectedCompareVisual = CompareVisuals[0];
    }

    internal void Initialize(
        ObservableCollection<SubtitleLineViewModel> left,
        string leftFileName,
        ObservableCollection<SubtitleLineViewModel> right,
        string rightFileName,
        bool hasChanges)
    {
        _leftLines.Clear();
        _leftLines.AddRange(left.Select(p => new SubtitleLineViewModel(p)));
        LeftFileName = leftFileName;
        if (!string.IsNullOrEmpty(leftFileName) && hasChanges)
        {
            LeftFileNameHasChanges = true;
        }

        _rightLines.Clear();
        _rightLines.AddRange(right.Select(p => new SubtitleLineViewModel(p)));
        RightFileName = rightFileName;

        IsReloadFromFileVisible = !string.IsNullOrEmpty(LeftFileName);

        Dispatcher.UIThread.Post(Compare);
    }

    private void Compare()
    {
        LeftSubtitles.Clear();
        foreach (var l in _leftLines)
        {
            LeftSubtitles.Add(new CompareItem(l));
        }

        RightSubtitles.Clear();
        foreach (var r in _rightLines)
        {
            RightSubtitles.Add(new CompareItem(r));
        }

        StatusText = string.Empty;
        InsertMissingLines();
        AddColoringAndCountDifferences();
        SetTextStackPanels();
        IsExportVisible = LeftSubtitles.Count > 0 && RightSubtitles.Count > 0;
        SelectAndScrollToRow(LeftDataGrid, 0);
    }

    private void SetTextStackPanels()
    {
        if (LeftSubtitles.Count != RightSubtitles.Count)
        {
            return;
        }

        for (var i = 0; i < LeftSubtitles.Count; i++)
        {
            var left = LeftSubtitles[i];
            var right = RightSubtitles[i];
            var (leftBlock, rightBlock) = TextDiffHighlighter.Compare(left.Text, right.Text);
            left.TextPanel.Children.Clear();
            left.TextPanel.Children.Add(leftBlock);
            right.TextPanel.Children.Clear();
            right.TextPanel.Children.Add(rightBlock);
        }
    }

    private void AddColoringAndCountDifferences()
    {
        var differences = new List<int>();
        var index = 0;
        var left = GetLeftItemOrNull(index);
        var right = GetRightItemOrNull(index);
        var totalWords = 0;
        var wordsChanged = 0;
        var max = Math.Max(LeftSubtitles.Count, RightSubtitles.Count);
        var min = Math.Min(LeftSubtitles.Count, RightSubtitles.Count);
        var onlyShowTextDiff = SelectedCompareVisual.Type == CompareVisualType.ShowOnlyDifferencesInText;
        var onlyShowDiff = SelectedCompareVisual.Type == CompareVisualType.ShowOnlyDifferences;

        ResetAllBackgroundColors();

        if (LeftSubtitles.Count == 0 || RightSubtitles.Count == 0)
        {
            return;
        }

        if (onlyShowTextDiff)
        {
            while (index < min)
            {
                var addIndexToDifferences = false;
                Utilities.GetTotalAndChangedWords(left?.Text, right?.Text, ref totalWords, ref wordsChanged, IgnoreWhiteSpace, IgnoreFormatting, ShouldBreakToLetter());

                if (left == null || left.IsDefault)
                {
                    addIndexToDifferences = true;
                    if (right != null && !right.IsDefault)
                    {
                        SetItemBackgroundColor(index, false, ListViewRed, ItemColumn.All);
                    }
                }
                else if (right == null || right.IsDefault)
                {
                    addIndexToDifferences = true;
                    if (left != null && !left.IsDefault)
                    {
                        SetItemBackgroundColor(index, true, ListViewRed, ItemColumn.All);
                    }
                }
                else if (!AreTextsEqual(left, right))
                {
                    addIndexToDifferences = true;
                    SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.Text);
                    SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.Text);
                }

                if (addIndexToDifferences)
                {
                    differences.Add(index);
                }

                index++;
                left = GetLeftItemOrNull(index);
                right = GetRightItemOrNull(index);
            }
        }
        else
        {
            while (index < min)
            {
                Utilities.GetTotalAndChangedWords(left?.Text, right?.Text, ref totalWords, ref wordsChanged, IgnoreWhiteSpace, IgnoreFormatting, ShouldBreakToLetter());
                var addIndexToDifferences = false;

                if (left == null || left.IsDefault)
                {
                    addIndexToDifferences = true;
                    if (right != null && !right.IsDefault)
                    {
                        SetItemBackgroundColor(index, false, ListViewRed, ItemColumn.All);
                    }
                }
                else if (right == null || right.IsDefault)
                {
                    addIndexToDifferences = true;
                    if (left != null && !left.IsDefault)
                    {
                        SetItemBackgroundColor(index, true, ListViewRed, ItemColumn.All);
                    }
                }
                else
                {
                    var timingsMatch = IsTimeEqual(left.StartTime, right.StartTime) && 
                                      IsTimeEqual(left.EndTime, right.EndTime);
                    var textsMatch = AreTextsEqual(left, right);
                    var numbersMatch = left.Number == right.Number;

                    // Check if there are any differences at all
                    if (!timingsMatch || !textsMatch || !numbersMatch)
                    {
                        addIndexToDifferences = true;

                        // Start time difference
                        if (!IsTimeEqual(left.StartTime, right.StartTime))
                        {
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.StartTime);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.StartTime);
                        }

                        // End time difference
                        if (!IsTimeEqual(left.EndTime, right.EndTime))
                        {
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.EndTime);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.EndTime);
                        }

                        // Text difference
                        if (!textsMatch)
                        {
                            SetItemBackgroundColor(index, true, ListViewGreen, ItemColumn.Text);
                            SetItemBackgroundColor(index, false, ListViewGreen, ItemColumn.Text);
                        }

                        // Number difference
                        if (!numbersMatch)
                        {
                            SetItemBackgroundColor(index, true, ListViewOrange, ItemColumn.Number);
                            SetItemBackgroundColor(index, false, ListViewOrange, ItemColumn.Number);
                        }
                    }
                }

                if (addIndexToDifferences)
                {
                    differences.Add(index);
                }

                index++;
                left = GetLeftItemOrNull(index);
                right = GetRightItemOrNull(index);
            }
        }

        foreach (var idx in differences)
        {
            LeftSubtitles[idx].HasDifference = true;
            RightSubtitles[idx].HasDifference = true;
        }

        // remove items not in differences
        if (onlyShowTextDiff || onlyShowDiff)
        {
            for (var idx = LeftSubtitles.Count - 1; idx >= 0; idx--)
            {
                if (!differences.Contains(idx))
                {
                    LeftSubtitles.RemoveAt(idx);
                    RightSubtitles.RemoveAt(idx);
                }
            }
        }

        SetStatusText(differences, totalWords, wordsChanged, min);
    }

    private void SetStatusText(List<int> differences, int totalWords, int wordsChanged, int min)
    {
        if (differences.Count >= min)
        {
            StatusText = Se.Language.File.SubtitlesNotAlike;
        }
        else
        {
            if (wordsChanged != totalWords && wordsChanged > 0)
            {
                var formatString = Se.Language.File.XNumberOfDifferenceAndPercentChanged;
                if (ShouldBreakToLetter())
                {
                    formatString = Se.Language.File.XNumberOfDifferenceAndPercentLettersChanged;
                }

                StatusText = string.Format(formatString, differences.Count, wordsChanged * 100.00 / totalWords);
            }
            else
            {
                StatusText = string.Format(Se.Language.File.XNumberOfDifference, differences.Count);
            }
        }
    }

    private enum ItemColumn
    {
        All,
        Number,
        StartTime,
        EndTime,
        Text
    }

    private void SetItemBackgroundColor(int index, bool isLeft, IBrush brush, ItemColumn column)
    {
        var collection = isLeft ? LeftSubtitles : RightSubtitles;
        if (index >= collection.Count)
        {
            return;
        }

        var item = collection[index];

        switch (column)
        {
            case ItemColumn.All:
                item.NumberBackgroundBrush = brush;
                item.StartTimeBackgroundBrush = brush;
                item.EndTimeBackgroundBrush = brush;
                item.TextBackgroundBrush = brush;
                break;
            case ItemColumn.Number:
                item.NumberBackgroundBrush = brush;
                break;
            case ItemColumn.StartTime:
                item.StartTimeBackgroundBrush = brush;
                break;
            case ItemColumn.EndTime:
                item.EndTimeBackgroundBrush = brush;
                break;
            case ItemColumn.Text:
                item.TextBackgroundBrush = brush;
                break;
        }
    }

    private void ResetAllBackgroundColors()
    {
        foreach (var item in LeftSubtitles)
        {
            item.NumberBackgroundBrush = TransparentBrush;
            item.StartTimeBackgroundBrush = TransparentBrush;
            item.EndTimeBackgroundBrush = TransparentBrush;
            item.TextBackgroundBrush = TransparentBrush;
        }

        foreach (var item in RightSubtitles)
        {
            item.NumberBackgroundBrush = TransparentBrush;
            item.StartTimeBackgroundBrush = TransparentBrush;
            item.EndTimeBackgroundBrush = TransparentBrush;
            item.TextBackgroundBrush = TransparentBrush;
        }
    }

    private bool ShouldBreakToLetter() => _language != null && (_language == "ja" || _language == "zh");

    private void InsertMissingLines()
    {
        if (LeftSubtitles.Count == 0 || RightSubtitles.Count == 0)
        {
            return;
        }

        var index = 0;
        var left = GetLeftItemOrNull(index);
        var right = GetRightItemOrNull(index);
        var max = Math.Max(_leftLines.Count, _rightLines.Count);
        while (index < max)
        {
            if (left != null && right != null && GetColumnsEqualExceptNumberAndDuration(left, right) == 0)
            {
                for (var i = index + 1; i < max; i++)
                {
                    // Try to find at least two matching properties
                    if (GetColumnsEqualExceptNumber(GetLeftItemOrNull(i), right) > 1)
                    {
                        for (var j = index; j < i; j++)
                        {
                            RightSubtitles.Insert(index++, new CompareItem());
                        }
                        break;
                    }

                    if (GetColumnsEqualExceptNumber(left, GetRightItemOrNull(i)) > 1)
                    {
                        for (var j = index; j < i; j++)
                        {
                            LeftSubtitles.Insert(index++, new CompareItem());
                        }
                        break;
                    }
                }
            }

            index++;
            left = GetLeftItemOrNull(index);
            right = GetRightItemOrNull(index);
        }

        // insert rest
        var minSub = LeftSubtitles.Count < RightSubtitles.Count ? LeftSubtitles : RightSubtitles;
        for (var idx = minSub.Count; idx < max; idx++)
        {
            minSub.Insert(idx, new CompareItem());
        }
    }

    private CompareItem? GetLeftItemOrNull(int index)
    {
        if (index >= 0 && index < LeftSubtitles.Count)
        {
            return LeftSubtitles[index];
        }

        return null;
    }

    private CompareItem? GetRightItemOrNull(int index)
    {
        if (index >= 0 && index < RightSubtitles.Count)
        {
            return RightSubtitles[index];
        }

        return null;
    }

    private int GetColumnsEqualExceptNumberAndDuration(CompareItem p1, CompareItem p2)
    {
        if (p1 == null || p2 == null)
        {
            return 0;
        }

        var columnsEqual = 0;
        if (IsTimeEqual(p1.StartTime, p2.StartTime))
        {
            columnsEqual++;
        }

        if (IsTimeEqual(p1.EndTime, p2.EndTime))
        {
            columnsEqual++;
        }

        if (AreTextsEqual(p1, p2))
        {
            columnsEqual++;
        }

        return columnsEqual;
    }

    private bool AreTextsEqual(CompareItem p1, CompareItem p2)
    {
        return p1.Text.Trim() == p2.Text.Trim() ||
                    IgnoreFormatting && HtmlUtil.RemoveHtmlTags(p1.Text.Trim()) == HtmlUtil.RemoveHtmlTags(p2.Text.Trim()) ||
                    IgnoreWhiteSpace && RemoveWhiteSpace(p1.Text) == RemoveWhiteSpace(p2.Text) ||
                    IgnoreFormatting && IgnoreWhiteSpace && RemoveWhiteSpace(HtmlUtil.RemoveHtmlTags(p1.Text)) == RemoveWhiteSpace(HtmlUtil.RemoveHtmlTags(p2.Text));
    }

    public static string RemoveWhiteSpace(string text)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private int GetColumnsEqualExceptNumber(CompareItem? left, CompareItem? right)
    {
        if (left == null || right == null)
        {
            return 0;
        }

        var columnsEqual = 0;
        if (IsTimeEqual(left.StartTime, right.StartTime))
        {
            columnsEqual++;
        }

        if (IsTimeEqual(left.EndTime, right.EndTime))
        {
            columnsEqual++;
        }

        if (IsTimeEqual(left.Duration, right.Duration))
        {
            columnsEqual++;
        }

        if (AreTextsEqual(left, right))
        {
            columnsEqual++;
        }

        return columnsEqual;
    }

    private static bool IsTimeEqual(TimeSpan t1, TimeSpan t2)
    {
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            return new TimeCode(t1).ToDisplayString() == new TimeCode(t2).ToDisplayString();
        }

        const double tolerance = 0.1;
        return Math.Abs(t1.TotalMilliseconds - t2.TotalMilliseconds) < tolerance;
    }

    [RelayCommand]
    private async Task PickLeftSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        _leftLines.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            _leftLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
        }

        LeftFileNameHasChanges = false;
        LeftFileName = fileName;

        Dispatcher.UIThread.Post(Compare);
    }

    [RelayCommand]
    private async Task PickRightSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        _rightLines.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            _rightLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
        }

        RightFileName = fileName;
        IsReloadFromFileVisible = false;

        Dispatcher.UIThread.Post(Compare);
    }

    [RelayCommand]
    private void ReloadRightFromFile()
    {
        var fileName = LeftFileName;
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        _rightLines.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            _rightLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
        }

        RightFileName = fileName;
        IsReloadFromFileVisible = false;

        Dispatcher.UIThread.Post(Compare);
    }

    [RelayCommand]
    private void PreviousDifference()
    {
        var selected = SelectedLeft;
        if (selected == null)
        {
            return;
        }

        var idx = LeftSubtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        while (idx > 0)
        {
            idx--;
            if (LeftSubtitles[idx].HasDifference)
            {
                SelectAndScrollToRow(LeftDataGrid, idx);
                return;
            }
        }
    }

    [RelayCommand]
    private void NextDifference()
    {
        var selected = SelectedLeft;
        if (selected == null)
        {
            return;
        }

        var idx = LeftSubtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        while (idx < LeftSubtitles.Count - 1)
        {
            idx++;
            if (LeftSubtitles[idx].HasDifference)
            {
                SelectAndScrollToRow(LeftDataGrid, idx);
                return;
            }
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task Export()
    {
        var targetFileName = string.IsNullOrEmpty(LeftFileName) ? "compare.html" : System.IO.Path.GetFileNameWithoutExtension(LeftFileName) + "-compare.html";
        var fileName = await _fileHelper.PickSaveFile(Window!, Se.Language.File.SaveCompareHtmlTitle, targetFileName, "HTML files (*.html)|*.html");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <title>Subtitle Edit compare</title>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    td { font-family: Tahoma, Verdana, 'Noto Sans', Ubuntu; padding: 8px; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("  <body>");
        sb.AppendLine("    <h1>Subtitle Edit compare</h1>");
        sb.AppendLine("    <table>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("      <th colspan='4' style='text-align:left'>" + GetFileName(LeftFileName) + "</th>");
        sb.AppendLine("      <th>&nbsp;</th>");
        sb.AppendLine("      <th colspan='4' style='text-align:left'>" + GetFileName(RightFileName) + "</th>");
        sb.AppendLine("    </tr>");
        for (var i = 0; i < LeftSubtitles.Count; i++)
        {
            var itemLeft = LeftSubtitles[i];
            var itemRight = RightSubtitles[i];

            sb.AppendLine("    <tr>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.NumberBackgroundBrush) + ">" + GetHtmlText(itemLeft, itemLeft.Number.ToString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.StartTimeBackgroundBrush) + ">" + GetHtmlText(itemLeft, new TimeCode(itemLeft.StartTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.EndTimeBackgroundBrush) + ">" + GetHtmlText(itemLeft, new TimeCode(itemLeft.EndTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemLeft.TextBackgroundBrush) + ">" + GetHtmlText(itemLeft, itemLeft.Text) + "</td>");
            sb.AppendLine("      <td>&nbsp;</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.NumberBackgroundBrush) + ">" + GetHtmlText(itemRight, itemRight.Number.ToString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.StartTimeBackgroundBrush) + ">" + GetHtmlText(itemRight, new TimeCode(itemRight.StartTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.EndTimeBackgroundBrush) + ">" + GetHtmlText(itemRight, new TimeCode(itemRight.EndTime).ToDisplayString()) + "</td>");
            sb.AppendLine("      <td" + GetHtmlBackgroundColor(itemRight.TextBackgroundBrush) + ">" + GetHtmlText(itemRight, itemRight.Text) + "</td>");
            sb.AppendLine("    </tr>");
        }
        sb.AppendLine("    <tr>");
        sb.AppendLine("      <td colspan='9' style='text-align:left'><br />" + StatusText + "</td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("    </table>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");
        System.IO.File.WriteAllText(fileName, sb.ToString());
        await _folderHelper.OpenFolderWithFileSelected(Window!, fileName);
    }

    private static string GetFileName(string fileName)
    {
        try
        {
            return string.IsNullOrEmpty(fileName) ? string.Empty : System.IO.Path.GetFileName(fileName);
        }
        catch
        {
            return fileName;
        }
    }

    private static string GetShortFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }

        try
        {
            var name = System.IO.Path.GetFileName(fileName);
            const int maxLength = 40;
            
            if (name.Length <= maxLength)
            {
                return name;
            }

            // Show beginning and end of filename with ellipsis in the middle
            var extension = System.IO.Path.GetExtension(name);
            var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(name);
            
            if (nameWithoutExt.Length + extension.Length <= maxLength)
            {
                return name;
            }

            var charsToShow = maxLength - extension.Length - 3; // 3 for "..."
            if (charsToShow < 5)
            {
                // Filename is very long, just truncate
                return name.Substring(0, maxLength - 3) + "...";
            }

            var frontChars = charsToShow * 2 / 3; // Show more of the beginning
            var backChars = charsToShow - frontChars;
            
            return nameWithoutExt.Substring(0, frontChars) + "..." + nameWithoutExt.Substring(nameWithoutExt.Length - backChars) + extension;
        }
        catch
        {
            return fileName;
        }
    }

    partial void OnLeftFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(LeftFileNameDisplay));
    }

    partial void OnRightFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(RightFileNameDisplay));
    }

    private static string GetHtmlText(CompareItem p, string text)
    {
        return p.IsDefault ? string.Empty : HtmlUtil.EncodeNamed(text);
    }

    private static string GetHtmlBackgroundColor(IBrush brush)
    {
        if (brush == null)
        {
            return string.Empty;
        }

        if (brush is SolidColorBrush solidColorBrush)
        {
            if (solidColorBrush.Color == Colors.Transparent)
            {
                return string.Empty;
            }

            var c = solidColorBrush.Color;
            var htmlColor = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            return $" style='background-color:{htmlColor}'";
        }

        return string.Empty;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void LeftDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            return;
        }

        var selection = e.AddedItems[0] as CompareItem;
        if (selection == null)
        {
            return;
        }

        var idx = LeftSubtitles.IndexOf(selection);
        Dispatcher.UIThread.Post(() =>
        {
            SelectAndScrollToRow(RightDataGrid, idx);
        });
    }

    internal void RightDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            return;
        }

        var selection = e.AddedItems[0] as CompareItem;
        if (selection == null)
        {
            return;
        }

        var idx = RightSubtitles.IndexOf(selection);
        Dispatcher.UIThread.Post(() =>
        {
            SelectAndScrollToRow(LeftDataGrid, idx);
        });
    }

    private void SelectAndScrollToRow(DataGrid? datagrid, int index)
    {
        if (index < 0 || datagrid == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (datagrid.SelectedIndex != index)
            {
                datagrid.SelectedIndex = index;
            }

            datagrid.ScrollIntoView(datagrid.SelectedItem, null);
        });
    }

    internal void CheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        Task.Delay(100).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(Compare);
        });
    }

    internal void ComboBoxCompareVisualSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Task.Delay(100).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(Compare);
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

    internal void FileGridOnDropLeft(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    var subtitle = Subtitle.Parse(path);
                    if (subtitle == null || path == null)
                    {
                        return;
                    }

                    _leftLines.Clear();
                    foreach (var line in subtitle.Paragraphs)
                    {
                        _leftLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
                    }

                    LeftFileNameHasChanges = false;
                    LeftFileName = path;

                    Dispatcher.UIThread.Post(Compare);
                    break;
                }
            });
        }
    }

    internal void FileGridOnDropRight(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    var subtitle = Subtitle.Parse(path);
                    if (subtitle == null || path == null)
                    {
                        return;
                    }

                    _rightLines.Clear();
                    foreach (var line in subtitle.Paragraphs)
                    {
                        _rightLines.Add(new SubtitleLineViewModel(line, subtitle.OriginalFormat));
                    }

                    RightFileName = path;

                    Dispatcher.UIThread.Post(Compare);
                    break;
                }
            });
        }
    }
}

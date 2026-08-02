using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;

public partial class JoinSubtitlesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<JoinDisplayItem> _joinItems;
    [ObservableProperty] private JoinDisplayItem? _selectedJoinItem;
    [ObservableProperty] private bool _keepTimeCodes;
    [ObservableProperty] private bool _appendTimeCodes;
    [ObservableProperty] private bool _isJoinEnabled;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isMoveVisible;
    [ObservableProperty] private int _appendTimeCodesAddMilliseconds;

    public Window? Window { get; set; }
    public TableView JoinItemsGrid { get; set; }

    public bool OkPressed { get; private set; }
    public SubtitleFormat JoinedFormat { get; private set; }
    public Subtitle JoinedSubtitle { get; private set; }

    private static readonly Regex NumberRegex = new(@"\d+", RegexOptions.Compiled);

    private readonly IFileHelper _fileHelper;
    private bool _loadFailed;

    public JoinSubtitlesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        JoinItems = new ObservableCollection<JoinDisplayItem>();
        JoinItemsGrid = new TableView();
        JoinedFormat = new SubRip();
        JoinedSubtitle = new Subtitle();
        LoadSettings();
    }

    private void LoadSettings()
    {
        KeepTimeCodes = Se.Settings.Tools.JoinKeepTimeCodes;
        AppendTimeCodes = !KeepTimeCodes;
        AppendTimeCodesAddMilliseconds = Se.Settings.Tools.JoinAppendMilliseconds;
    }

    /// <summary>
    /// Switching time-code mode changes the joined result and, for "Keep time codes", the
    /// list order too (SortAndLoad sorts by start time there) - so rebuild, as SE 4 does.
    /// Posted, because the radio group sets <see cref="KeepTimeCodes"/> and
    /// <see cref="AppendTimeCodes"/> one after the other and SortAndLoad must not run
    /// between the two.
    /// </summary>
    partial void OnKeepTimeCodesChanged(bool value)
    {
        if (JoinItems.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () => await SortAndLoad());
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.JoinKeepTimeCodes = KeepTimeCodes;
        Se.Settings.Tools.JoinAppendMilliseconds = AppendTimeCodesAddMilliseconds;

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Add()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (!fileNames.Any())
        {
            return;
        }

        await AddFiles(fileNames);
    }

    /// <summary>
    /// Appends the files to the end of the list, in natural file name order (so "part2"
    /// lands before "part10"), as in SE 4. The list must not be re-sorted by start time
    /// here: in "Append time codes" mode the order is the user's to decide, and re-sorting
    /// on every add made it impossible to arrange the files at all (issue #13092).
    /// </summary>
    private async Task AddFiles(IEnumerable<string> fileNames)
    {
        var newFileNames = fileNames.ToList();
        newFileNames.Sort(NaturalCompare);

        foreach (var fileName in newFileNames)
        {
            if (JoinItems.Any(p => p.FullFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            await AddFile(fileName);
        }

        await SortAndLoad();

        IsJoinEnabled = JoinItems.Count > 1;
    }

    /// <summary>
    /// SE 4's natural file name order: digit runs are zero-padded before an ordinal
    /// compare, so "CD2" sorts before "CD10".
    /// </summary>
    private static int NaturalCompare(string x, string y)
    {
        var a = NumberRegex.Replace(x, m => m.Value.PadLeft(10, '0')).Replace(" ", string.Empty);
        var b = NumberRegex.Replace(y, m => m.Value.PadLeft(10, '0')).Replace(" ", string.Empty);
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddFile(string fileName)
    {
        if (Window == null)
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            if (fileName.EndsWith(".ismt", StringComparison.InvariantCultureIgnoreCase) ||
                               fileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) ||
                               fileName.EndsWith(".m4v", StringComparison.InvariantCultureIgnoreCase) ||
                               fileName.EndsWith(".3gp", StringComparison.InvariantCultureIgnoreCase))
            {
                var format = new IsmtDfxp();
                if (format.IsMine(null, fileName))
                {
                    subtitle = new Subtitle();
                    format.LoadSubtitle(subtitle, null, fileName);
                }
            }
        }

        if (subtitle == null || subtitle.Paragraphs.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Unable to read subtitle from file: " + fileName);
            return;
        }

        var item = new JoinDisplayItem
        {
            FileName = System.IO.Path.GetFileName(fileName),
            FullFileName = fileName,
            StartTime = subtitle.Paragraphs.Min(p => p.StartTime.TimeSpan),
            EndTime = subtitle.Paragraphs.Max(p => p.EndTime.TimeSpan),
            Lines = subtitle.Paragraphs.Count,
        };
        JoinItems.Add(item);
    }

    [RelayCommand]
    private async Task Remove()
    {
        var selected = SelectedJoinItem;
        if (selected == null)
        {
            return;
        }

        var index = JoinItems.IndexOf(selected);
        JoinItems.Remove(selected);
        if (JoinItems.Count > 0)
        {
            if (index >= JoinItems.Count)
            {
                index = JoinItems.Count - 1;
            }

            SelectedJoinItem = JoinItems[index];
        }

        IsJoinEnabled = JoinItems.Count > 1;

        await SortAndLoad();
    }

    [RelayCommand]
    private void Clear()
    {
        JoinItems.Clear();
        JoinedSubtitle = new Subtitle();
        IsJoinEnabled = false;
    }

    [RelayCommand]
    private Task MoveUp() => Move(ListMoveDirection.Up);

    [RelayCommand]
    private Task MoveDown() => Move(ListMoveDirection.Down);

    [RelayCommand]
    private Task MoveToTop() => Move(ListMoveDirection.Top);

    [RelayCommand]
    private Task MoveToBottom() => Move(ListMoveDirection.Bottom);

    /// <summary>
    /// Reorders the files to join. Only meaningful in "Append time codes" mode - "Keep time
    /// codes" sorts everything by start time anyway, which is why the menu items are hidden
    /// there, as in SE 4.
    /// </summary>
    private async Task Move(ListMoveDirection direction)
    {
        if (!AppendTimeCodes || JoinItems.Count < 2)
        {
            return;
        }

        TableViewExtras.MoveSelectedRows(JoinItemsGrid, JoinItems, direction);

        await SortAndLoad();
    }

    [RelayCommand]
    private async Task Ok()
    {
        // The joined subtitle is built by SortAndLoad, and the time-code mode and the added
        // milliseconds can both have changed since the last one ran - rebuild before handing
        // it over, so the result always matches what the dialog shows.
        await SortAndLoad();

        // A file that has become unreadable since it was added drops out of the list here,
        // so stay open with the shortened list rather than joining the remains silently.
        if (_loadFailed)
        {
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private async Task SortAndLoad()
    {
        _loadFailed = false;
        JoinedFormat = new SubRip(); // default subtitle format
        string? header = null;
        SubtitleFormat? lastFormat = null;
        var subtitles = new List<Subtitle>();
        for (var k = 0; k < JoinItems.Count; k++)
        {
            var fileName = JoinItems[k].FullFileName;
            try
            {
                var sub = new Subtitle();
                SubtitleFormat? format = null;

                if (fileName.EndsWith(".ismt", StringComparison.InvariantCultureIgnoreCase) ||
                    fileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) ||
                    fileName.EndsWith(".m4v", StringComparison.InvariantCultureIgnoreCase) ||
                    fileName.EndsWith(".3gp", StringComparison.InvariantCultureIgnoreCase))
                {
                    format = new IsmtDfxp();
                    if (format.IsMine(null, fileName))
                    {
                        var s = new Subtitle();
                        format.LoadSubtitle(s, null, fileName);
                        if (s.Paragraphs.Count > 0)
                        {
                            lastFormat = format;
                        }
                    }
                }

                var lines = FileUtil.ReadAllLinesShared(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName));
                if (lastFormat != null && lastFormat.IsMine(lines, fileName))
                {
                    format = lastFormat;
                    format.LoadSubtitle(sub, lines, fileName);
                }

                if (sub.Paragraphs.Count == 0 || format == null)
                {
                    format = sub.LoadSubtitle(fileName, out _, null);
                }

                if (format == null && lines.Count > 0 && lines.Count < 10 && lines[0].Trim() == "WEBVTT")
                {
                    format = new WebVTT(); // empty WebVTT
                }

                if (format == null)
                {
                    foreach (var binaryFormat in SubtitleFormat.GetBinaryFormats(true))
                    {
                        if (binaryFormat.IsMine(null, fileName))
                        {
                            binaryFormat.LoadSubtitle(sub, null, fileName);
                            format = binaryFormat;
                            break;
                        }
                    }
                }

                if (format == null)
                {
                    foreach (var f in SubtitleFormat.GetTextOtherFormats())
                    {
                        if (f.IsMine(lines, fileName))
                        {
                            f.LoadSubtitle(sub, lines, fileName);
                            format = f;
                            break;
                        }
                    }
                }

                if (format == null)
                { 
                    await Revert(k, "Unknown subtitle type" + Environment.NewLine + fileName);
                    break;
                }

                if (sub.Header != null)
                {
                    if (format.Name == AdvancedSubStationAlpha.NameOfFormat)
                    {
                        sub.Header = sub.Header.Replace("*Default", "Default");
                        foreach (var subParagraph in sub.Paragraphs)
                        {
                            if (subParagraph.Extra == "*Default")
                            {
                                subParagraph.Extra = "Default";
                            }
                        }
                    }

                    if (format.Name == AdvancedSubStationAlpha.NameOfFormat && header != null)
                    {
                        var oldPlayResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", header);
                        var oldPlayResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", header);
                        var newPlayResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", sub.Header);
                        var newPlayResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", sub.Header);

                        var stylesInHeader = AdvancedSubStationAlpha.GetStylesFromHeader(header);
                        var styles = new List<SsaStyle>();
                        foreach (var styleName in stylesInHeader)
                        {
                            styles.Add(AdvancedSubStationAlpha.GetSsaStyle(styleName, header));
                        }

                        foreach (var newStyle in AdvancedSubStationAlpha.GetStylesFromHeader(sub.Header))
                        {
                            if (stylesInHeader.Any(p => p == newStyle))
                            {
                                if (IsStyleDifferent(newStyle, sub, header))
                                {
                                    var styleToBeRenamed = AdvancedSubStationAlpha.GetSsaStyle(newStyle, sub.Header);
                                    var newName = styleToBeRenamed.Name + "_" + Guid.NewGuid();
                                    foreach (var p in sub.Paragraphs.Where(p => p.Extra == styleToBeRenamed.Name))
                                    {
                                        p.Extra = newName;
                                    }

                                    styleToBeRenamed.Name = newName;
                                    styles.Add(styleToBeRenamed);
                                }
                            }
                            else
                            {
                                styles.Add(AdvancedSubStationAlpha.GetSsaStyle(newStyle, sub.Header));
                            }
                        }

                        header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, styles);
                        if (!string.IsNullOrEmpty(oldPlayResX) && string.IsNullOrEmpty(newPlayResX))
                        {
                            header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", oldPlayResX, "[Script Info]", header);
                        }
                        if (!string.IsNullOrEmpty(oldPlayResY) && string.IsNullOrEmpty(newPlayResY))
                        {
                            header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", oldPlayResY, "[Script Info]", header);
                        }
                    }
                    else
                    {
                        header = sub.Header;
                    }
                }

                lastFormat = lastFormat == null || lastFormat.FriendlyName == format.FriendlyName ? format : new SubRip();

                subtitles.Add(sub);
            }
            catch (Exception exception)
            {
                await Revert(k, exception.Message);
                return;
            }
        }
        JoinedFormat = lastFormat ?? new SubRip();


        if (!AppendTimeCodes)
        {
            for (var outer = 0; outer < subtitles.Count; outer++)
            {
                for (var inner = 1; inner < subtitles.Count; inner++)
                {
                    var a = subtitles[inner - 1];
                    var b = subtitles[inner];
                    if (a.Paragraphs.Count > 0 && b.Paragraphs.Count > 0 && a.Paragraphs[0].StartTime.TotalMilliseconds > b.Paragraphs[0].StartTime.TotalMilliseconds)
                    {
                        (JoinItems[inner - 1], JoinItems[inner]) = (JoinItems[inner], JoinItems[inner - 1]);
                        (subtitles[inner - 1], subtitles[inner]) = (subtitles[inner], subtitles[inner - 1]);
                    }
                }
            }
        }

        JoinedSubtitle = new Subtitle();
        if (JoinedFormat != null && JoinedFormat.FriendlyName != SubRip.NameOfFormat)
        {
            JoinedSubtitle.Header = header;
        }

        var addTime = AppendTimeCodes;
        foreach (var sub in subtitles)
        {
            double addMs = 0;
            if (addTime && JoinedSubtitle.Paragraphs.Count > 0)
            {
                addMs = JoinedSubtitle.Paragraphs.Last().EndTime.TotalMilliseconds + Convert.ToDouble(AppendTimeCodesAddMilliseconds);
            }
            foreach (var p in sub.Paragraphs)
            {
                p.StartTime.TotalMilliseconds += addMs;
                p.EndTime.TotalMilliseconds += addMs;
                JoinedSubtitle.Paragraphs.Add(p);
            }
        }

        // "Keep time codes" (join by time) must interleave paragraphs from all files in
        // time order; otherwise files whose time codes overlap come out concatenated in
        // file order, not sorted (issue #11881). Mirrors SE4's JoinSubtitles.
        if (KeepTimeCodes)
        {
            JoinedSubtitle.Sort(SubtitleSortCriteria.StartTime);
        }

        JoinedSubtitle.Renumber();
    }

    private static bool IsStyleDifferent(string styleName, Subtitle newSubtitle, string oldHeader)
    {
        var newStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, newSubtitle.Header);
        var oldStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, oldHeader);
        if (oldStyle == null || newStyle == null)
        {
            return true;
        }

        return newStyle.ToRawAss() != oldStyle.ToRawAss();
    }

    private async Task Revert(int idx, string message)
    {
        _loadFailed = true;

        if (Window == null)
        {
            return;
        }

        for (int i = JoinItems.Count - 1; i >= idx; i--)
        {
            JoinItems.RemoveAt(i);
        }

        await MessageBox.Show(Window, "", message);
    }

    internal void GridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedJoinItem != null)
        {
            e.Handled = true;
            RemoveCommand.Execute(null);
        }
    }

    /// <summary>
    /// Ctrl+Up/Ctrl+Down reorder the selected file. Tunneled, because the ListBox underneath
    /// TableView handles Ctrl+Arrow itself (move focus without changing the selection) and a
    /// bubbling handler would never see the key.
    /// </summary>
    internal void GridMoveKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control)
        {
            return;
        }

        if (e.Key == Key.Up)
        {
            e.Handled = true;
            MoveUpCommand.Execute(null);
        }
        else if (e.Key == Key.Down)
        {
            e.Handled = true;
            MoveDownCommand.Execute(null);
        }
    }

    internal void ItemsContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteVisible = SelectedJoinItem != null;
        IsMoveVisible = AppendTimeCodes && JoinItems.Count > 1 && JoinItemsGrid.SelectedItems?.Count > 0;
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/join-subtitles");
        }
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

        var fileNames = files
            .Select(p => p.Path?.LocalPath)
            .Where(p => p != null && System.IO.File.Exists(p))
            .Select(p => p!)
            .ToList();

        if (fileNames.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () => await AddFiles(fileNames));
    }
}
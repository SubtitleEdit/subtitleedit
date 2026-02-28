using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    [ObservableProperty] private int _appendTimeCodesAddMilliseconds;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public SubtitleFormat JoinedFormat { get; private set; }
    public Subtitle JoinedSubtitle { get; private set; }

    private readonly IFileHelper _fileHelper;

    public JoinSubtitlesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        JoinItems = new ObservableCollection<JoinDisplayItem>();
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
        if (fileNames.Count() == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            bool flowControl = await AddFile(fileName);
            if (!flowControl)
            {
                continue;
            }
        }

        var items = JoinItems.ToList();
        JoinItems.Clear();
        foreach (var item in items.OrderBy(p=>p.StartTime))
        {
            JoinItems.Add(item);
        }

        await SortAndLoad();

        IsJoinEnabled = JoinItems.Count > 1;
    }

    private async Task<bool> AddFile(string fileName)
    {
        if (Window == null)
        {
            return false;
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
            return false;
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
        return true;
    }

    [RelayCommand]
    private void Remove()
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
    }

    [RelayCommand]
    private void Clear()
    {
        JoinItems.Clear();
        IsJoinEnabled = false;
    }

    [RelayCommand]
    private void Ok()
    {
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

    internal void DataGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedJoinItem != null)
        {
            e.Handled = true;
            Remove();
        }
    }

    internal void ItemsContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteVisible = SelectedJoinItem != null;
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
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
        if (files != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && System.IO.File.Exists(path))
                    {
                        bool flowControl = await AddFile(path);
                        if (!flowControl)
                        {
                            continue;
                        }
                    }
                }

                var items = JoinItems.ToList();
                JoinItems.Clear();
                foreach (var item in items.OrderBy(p => p.StartTime))
                {
                    JoinItems.Add(item);
                }

                await SortAndLoad();

                IsJoinEnabled = JoinItems.Count > 1;
            });
        }
    }
}
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

public partial class SplitSubtitleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SplitDisplayItem> _splitItems;
    [ObservableProperty] private SplitDisplayItem? _selectedSpiltItem;
    [ObservableProperty] private bool _splitByLines;
    [ObservableProperty] private bool _splitByCharacters;
    [ObservableProperty] private bool _splitByTime;
    [ObservableProperty] private int _numberOfEqualParts;
    [ObservableProperty] private string _subtitleInfo;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private ObservableCollection<SubtitleFormat> _formats;
    [ObservableProperty] private SubtitleFormat? _selectedSubtitleFormat;
    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding? _selectedEncoding;
    [ObservableProperty] private int _partsMax;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    private Subtitle _subtitle;
    private string _subtitleFileName;
    private List<Subtitle> _parts;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private bool _loading;

    public SplitSubtitleViewModel(IFolderHelper folderHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _windowService = windowService;

        SplitItems = new ObservableCollection<SplitDisplayItem>();
        Formats = new ObservableCollection<SubtitleFormat>(SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop());
        SelectedSubtitleFormat = Formats[0];
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings[0];
        OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        SubtitleInfo = string.Empty;
        SplitByLines = true;
        NumberOfEqualParts = 1;
        PartsMax = 1;

        _subtitle = new Subtitle();
        _loading = true;
        _subtitleFileName = string.Empty;
        _parts = new List<Subtitle>();

        LoadSettings();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    private void TimerUpdatePreviewElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _timerUpdatePreview.Stop();
        if (_dirty)
        {
            _dirty = false;
            UpdateSplit();
        }
        _timerUpdatePreview.Start();
    }

    public void Initialize(string fileName, Subtitle subtitle)
    {
        PartsMax = subtitle.Paragraphs.Count;
        _subtitleFileName = fileName;
        _subtitle = subtitle;

        // Default the output to the source subtitle's own folder - that's where users splitting a
        // file for translation expect the parts to land. Falls back to the loaded/Desktop folder
        // for untitled subtitles.
        if (!string.IsNullOrEmpty(fileName))
        {
            var folder = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                OutputFolder = folder;
            }
        }

        _timerUpdatePreview.Start();
        SetDirty();
    }

    private void LoadSettings()
    {
        NumberOfEqualParts = Math.Min(PartsMax, Se.Settings.Tools.SplitNumberOfEqualParts);

        OutputFolder = Se.Settings.Tools.SplitOutputFolder;
        if (string.IsNullOrEmpty(OutputFolder) || !Directory.Exists(OutputFolder))
        {
            OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        SplitByLines = Se.Settings.Tools.SplitByLines;
        SplitByCharacters = Se.Settings.Tools.SplitByCharacters;
        SplitByTime = Se.Settings.Tools.SplitByTime;

        if (SplitByLines)
        {
            SplitByLines = true;
        }
        else if (SplitByCharacters)
        {
            SplitByCharacters = true;
        }
        else if (SplitByTime)
        {
            SplitByTime = true;
        }
        else
        {
            SplitByLines = true;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.SplitNumberOfEqualParts = NumberOfEqualParts;
        Se.Settings.Tools.SplitOutputFolder = OutputFolder;
        Se.Settings.Tools.SplitByLines = SplitByLines;
        Se.Settings.Tools.SplitByCharacters = SplitByCharacters;
        Se.Settings.Tools.SplitByTime = SplitByTime;
        Se.Settings.Tools.SplitSubtitleFormat = SelectedSubtitleFormat?.Name ?? string.Empty;
        Se.Settings.Tools.SplitSubtitleEncoding = SelectedEncoding?.DisplayName ?? string.Empty;

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Browse()
    {
        if (Window == null)
        {
            return;
        }

        var folder = await _folderHelper.PickFolderAsync(Window, Se.Language.General.PickOutputFolder);
        if (!string.IsNullOrEmpty(folder))
        {
            OutputFolder = folder;
        }
    }

    [RelayCommand]
    private void OpenFolder()
    {
        if (Window == null)
        {
            return;
        }

        _folderHelper.OpenFolder(Window, OutputFolder);
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null || SplitItems.Count == 0 || SelectedSubtitleFormat == null || SelectedEncoding == null)
        {
            return;
        }

        SaveSettings();

        // Save parts. _parts and SplitItems are built together in UpdateSplit, so they stay aligned.
        for (var i = 0; i < _parts.Count; i++)
        {
            var text = _parts[i].ToText(SelectedSubtitleFormat);
            var fileName = Path.Combine(OutputFolder, SplitItems[i].FileName);
            await File.WriteAllTextAsync(fileName, text, SelectedEncoding.Encoding ?? Encoding.UTF8);
        }

        await _windowService.ShowDialogAsync<PartsSavedWindow, PartsSavedViewModel>(Window, vm =>
        {
            vm.Initialize(OutputFolder, SplitItems.Count, SelectedSubtitleFormat);
        });

        OkPressed = true;
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
        Window?.Close();
    }

    internal void UpdateSplit()
    {
        _parts = new List<Subtitle>();
        if (string.IsNullOrEmpty(OutputFolder) || !Directory.Exists(OutputFolder))
        {
            OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        var format = SelectedSubtitleFormat ?? new SubRip();
        var fileNameNoExt = Path.GetFileNameWithoutExtension(_subtitleFileName);
        if (string.IsNullOrWhiteSpace(fileNameNoExt))
        {
            fileNameNoExt = Se.Language.General.Untitled;
        }

        // calculate max parts
        if (SplitByLines)
        {
            PartsMax = Math.Max(1, _subtitle.Paragraphs.Count);
        }
        else if (SplitByCharacters)
        {
            var total = 0;
            foreach (var p in _subtitle.Paragraphs)
            {
                total += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
            }
            PartsMax = Math.Max(1, total);
        }
        else if (SplitByTime)
        {
            var totalSeconds = 0.0;
            if (_subtitle.Paragraphs.Count > 0)
            {
                totalSeconds = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds - _subtitle.Paragraphs[0].StartTime.TotalSeconds;
            }
            PartsMax = Math.Max(1, (int)(totalSeconds / 5));
        }

        if (_loading)
        {
            NumberOfEqualParts = Math.Min(PartsMax, Se.Settings.Tools.SplitNumberOfEqualParts);
            _loading = false;
        }

        if (NumberOfEqualParts <= 1)
        {
            NumberOfEqualParts = 1;
        }

        var mode = SplitByCharacters
            ? SubtitleSplitter.SplitMode.Characters
            : SplitByTime
                ? SubtitleSplitter.SplitMode.Time
                : SubtitleSplitter.SplitMode.Lines;

        // Build the parts and the preview rows from the same source, so the displayed part count
        // always matches the number of files written on OK.
        _parts = SubtitleSplitter.Split(_subtitle, NumberOfEqualParts, mode);

        Dispatcher.UIThread.Post(() =>
        {
            SplitItems.Clear();
            for (var i = 0; i < _parts.Count; i++)
            {
                var part = _parts[i];
                var size = 0;
                foreach (var p in part.Paragraphs)
                {
                    size += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
                }

                SplitItems.Add(new SplitDisplayItem()
                {
                    Lines = part.Paragraphs.Count,
                    Characters = size,
                    FileName = fileNameNoExt + ".Part" + (i + 1) + format.Extension
                });
            }
        });
    }

    public void SetDirty()
    {
        _dirty = true;
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/split-subtitle");
        }
    }
}
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
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdateSplit();
            }
            _timerUpdatePreview.Start();
        };
    }

    public void Initialize(string fileName, Subtitle subtitle)
    {
        PartsMax = subtitle.Paragraphs.Count;
        _subtitleFileName = fileName;
        _subtitle = subtitle;
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

        // Save parts
        foreach (var part in _parts)
        {
            var text = part.ToText(SelectedSubtitleFormat);
            var splitItem = SplitItems[_parts.IndexOf(part)];
            var fileName = Path.Combine(OutputFolder, splitItem.FileName);
            await File.WriteAllTextAsync(fileName, text, SelectedEncoding.Encoding ?? Encoding.UTF8);
        }

        await _windowService.ShowDialogAsync<PartsSavedWindow, PartsSavedViewModel>(Window, vm =>
        {
            vm.Initialize(OutputFolder, SplitItems.Count, SelectedSubtitleFormat);
        });

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
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

        Dispatcher.UIThread.Post(() =>
        {
            SplitItems.Clear();
            var startNumber = 0;
            if (SplitByLines)
            {
                var partSize = (int)(_subtitle.Paragraphs.Count / NumberOfEqualParts);
                for (var i = 0; i < NumberOfEqualParts; i++)
                {
                    var noOfLines = partSize;
                    if (i == NumberOfEqualParts - 1)
                    {
                        noOfLines = (int)(_subtitle.Paragraphs.Count - (NumberOfEqualParts - 1) * partSize);
                    }

                    var temp = new Subtitle { Header = _subtitle.Header };
                    var size = 0;
                    for (var number = 0; number < noOfLines; number++)
                    {
                        var p = _subtitle.Paragraphs[startNumber + number];
                        temp.Paragraphs.Add(new Paragraph(p));
                        size += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
                    }
                    startNumber += noOfLines;
                    _parts.Add(temp);

                    var item = new SplitDisplayItem()
                    {
                        Lines = noOfLines,
                        Characters = size,
                        FileName = fileNameNoExt + ".Part" + (i + 1) + format.Extension
                    };
                    SplitItems.Add(item);
                }
            }
            else if (SplitByCharacters)
            {
                var totalNumberOfCharacters = 0;
                foreach (var p in _subtitle.Paragraphs)
                {
                    totalNumberOfCharacters += HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
                }

                var partSize = (int)(totalNumberOfCharacters / NumberOfEqualParts);
                var nextLimit = partSize;
                var currentSize = 0;
                var temp = new Subtitle { Header = _subtitle.Header };
                for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    var p = _subtitle.Paragraphs[i];
                    var size = HtmlUtil.RemoveHtmlTags(p.Text, true).Length;
                    if (currentSize + size > nextLimit + 4 && _parts.Count < NumberOfEqualParts - 1)
                    {
                        _parts.Add(temp);

                        var item = new SplitDisplayItem()
                        {
                            Lines = temp.Paragraphs.Count,
                            Characters = currentSize,
                            FileName = fileNameNoExt + ".Part" + (SplitItems.Count + 1) + format.Extension
                        };
                        SplitItems.Add(item);

                        currentSize = size;
                        temp = new Subtitle { Header = _subtitle.Header };
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                    else
                    {
                        currentSize += size;
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                }

                var lastItem = new SplitDisplayItem()
                {
                    Lines = temp.Paragraphs.Count,
                    Characters = currentSize,
                    FileName = fileNameNoExt + ".Part" + (SplitItems.Count + 1) + format.Extension
                };
                SplitItems.Add(lastItem);
            }
            else if (SplitByTime)
            {
                var startMs = _subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
                var endMs = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds;
                var partSize = (endMs - startMs) / (double)NumberOfEqualParts;
                var nextLimit = startMs + partSize;
                var currentSize = 0;
                var temp = new Subtitle { Header = _subtitle.Header };
                for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    var p = _subtitle.Paragraphs[i];
                    var size = HtmlUtil.RemoveHtmlTags(p.Text, true).Replace("\r\n", "\n").Length;
                    if (p.StartTime.TotalMilliseconds > nextLimit - 10 && _parts.Count < NumberOfEqualParts - 1)
                    {
                        _parts.Add(temp);
                        var item = new SplitDisplayItem()
                        {
                            Lines = temp.Paragraphs.Count,
                            Characters = currentSize,
                            FileName = fileNameNoExt + ".Part" + (SplitItems.Count + 1) + format.Extension
                        };
                        SplitItems.Add(item);
                        temp = new Subtitle { Header = _subtitle.Header };
                        temp.Paragraphs.Add(new Paragraph(p));
                        nextLimit += partSize;
                        currentSize = 0;
                    }
                    else
                    {
                        currentSize += size;
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                }

                var lastItem = new SplitDisplayItem()
                {
                    Lines = temp.Paragraphs.Count,
                    Characters = currentSize,
                    FileName = fileNameNoExt + ".Part" + (SplitItems.Count + 1) + format.Extension
                };
                SplitItems.Add(lastItem);
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
            Window?.Close();
        }
    }
}
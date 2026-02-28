using Avalonia.Controls;
using Avalonia.Input;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public partial class ImportPlainTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<DisplayFile> _files;
    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<string> _splitAtOptions;
    [ObservableProperty] private string _selectedSplitAtOption;
    [ObservableProperty] private bool _isImportFilesVisible;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private string _plainText;
    [ObservableProperty] private string _numberOfSubtitles;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new Subtitle();
    private readonly IFileHelper _fileHelper;
    private readonly List<string> _textExtensions = new List<string>
    {
        "*.txt",
        "*.rtf" ,
    };
    private bool _dirty;
    private readonly System.Timers.Timer _timerUpdatePreview;

    public ImportPlainTextViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        Files = new ObservableCollection<DisplayFile>();
        SplitAtOptions = new ObservableCollection<string>
        {
            Se.Language.General.Auto,
            Se.Language.File.Import.BlankLines,
            Se.Language.File.Import.OneLineIsOneSubtitle,
            Se.Language.File.Import.TwoLinesAreOneSubtitle,
        };
        SelectedSplitAtOption = SplitAtOptions[0];
        PlainText = string.Empty;
        MinGapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
        NumberOfSubtitles = string.Empty;

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        _timerUpdatePreview.Start();
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            var subtitles = new List<SubtitleLineViewModel>();
            if (IsImportFilesVisible)
            {
                subtitles = UpdatePreviewFiles(Files.ToList());
            }
            else
            {
                subtitles = UpdatePreviewText(SelectedSplitAtOption, PlainText);
            }

            if (!HasTimeCodes(subtitles))
            {
                subtitles = TimeCodeCalculator.CalculateTimeCodes(
                    subtitles,
                    Se.Settings.General.SubtitleOptimalCharactersPerSeconds,
                    Se.Settings.General.SubtitleMaximumCharactersPerSeconds,
                    MinGapMs,
                    Se.Settings.General.SubtitleMinimumDisplayMilliseconds,
                    Se.Settings.General.SubtitleMaximumDisplayMilliseconds);
            }

            Dispatcher.UIThread.Post(() =>
            {
                Subtitles.Clear();
                for (int i = 0; i < subtitles.Count; i++)
                {
                    var subtitle = subtitles[i];
                    subtitle.Number = i + 1;
                    Subtitles.Add(subtitle);
                }

                if (Subtitles.Count > 0)
                {
                    NumberOfSubtitles = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, Subtitles.Count);
                }
                else
                {
                    NumberOfSubtitles = string.Empty;
                }

            });

            _dirty = false;
        }
    }

    private static bool HasTimeCodes(List<SubtitleLineViewModel> subtitles)
    {
        return subtitles.Any(s => s.StartTime != TimeSpan.Zero || s.EndTime != TimeSpan.Zero);
    }

    private static List<SubtitleLineViewModel> UpdatePreviewFiles(List<DisplayFile> list)
    {
        var subtitles = new List<SubtitleLineViewModel>();

        foreach (var file in list)
        {
            try
            {
                var text = File.ReadAllText(file.FullPath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var (start, end) = file.GetTimeCodes();
                    subtitles.Add(new SubtitleLineViewModel
                    {
                        Text = text.Trim(),
                        StartTime = start,
                        EndTime = end,
                    });
                }
            }
            catch
            {
                // ignore
            }
        }

        return subtitles;
    }

    private static List<SubtitleLineViewModel> UpdatePreviewText(string splitAtOption, string plainText)
    {
        var subtitles = new List<SubtitleLineViewModel>();

        if (splitAtOption == Se.Language.General.Auto)
        {
            return PlainTextSplitter.AutomaticSplit(plainText, Se.Settings.General.MaxNumberOfLines, Se.Settings.General.SubtitleLineMaximumLength);
        }
        else if (splitAtOption == Se.Language.File.Import.BlankLines)
        {
            var blocks = plainText.SplitToLines();
            var sb = new System.Text.StringBuilder();
            foreach (var block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block))
                {
                    if (sb.Length > 0)
                    {
                        subtitles.Add(new SubtitleLineViewModel { Text = sb.ToString().TrimEnd() });
                        sb.Clear();
                    }
                }
                else
                {
                    sb.AppendLine(block.Trim());
                }
            }

            if (sb.Length > 0)
            {
                subtitles.Add(new SubtitleLineViewModel { Text = sb.ToString().TrimEnd() });
                sb.Clear();
            }
        }
        else if (splitAtOption == Se.Language.File.Import.OneLineIsOneSubtitle)
        {
            var lines = plainText.SplitToLines();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = line.Trim() });
                }
            }
        }
        else if (splitAtOption == Se.Language.File.Import.TwoLinesAreOneSubtitle)
        {
            var lines = plainText.SplitToLines().Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            for (int i = 0; i < lines.Count; i += 2)
            {
                var text = lines[i].Trim();
                if (i + 1 < lines.Count)
                {
                    text += Environment.NewLine + lines[i + 1].Trim();
                }
                if (!string.IsNullOrWhiteSpace(text))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = text });
                }
            }
        }

        return subtitles;
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
    private async Task FileImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.ChooseImageFiles, Se.Language.General.TextFiles, ".txt", Se.Language.General.TextFiles);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var text = await File.ReadAllTextAsync(fileName);
        PlainText = text;
        _dirty = true;
    }

    [RelayCommand]
    private async Task FilesImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(Window, Se.Language.General.ChooseImageFiles, Se.Language.General.Images, _textExtensions, string.Empty, new List<string>());
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames.OrderBy(p => p))
        {
            var fileInfo = new FileInfo(fileName);
            var displayFile = new DisplayFile(fileName, fileInfo.Length);
            Files.Add(displayFile);
        }

        _dirty = true;
    }

    [RelayCommand]
    private async Task FilesClear()
    {
        Files.Clear();
        _dirty = true;
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
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLowerInvariant();
                        if (!_textExtensions.Any(x => x.EndsWith(ext)))
                        {
                            continue;
                        }

                        var fileInfo = new FileInfo(path);
                        var displayFile = new DisplayFile(path, fileInfo.Length);
                        Files.Add(displayFile);
                    }
                }
                _dirty = true;
            });
        }
    }

    internal void PlainTextChanged()
    {
        _dirty = true;
    }

    internal void SplitAtOptionChanged()
    {
        _dirty = true;
    }

    internal void CheckBoxImportFilesChanged()
    {
        _dirty = true;
    }

    internal void SetCurrentSubtitle(Subtitle subtitle)
    {
        _subtitle = new Subtitle(subtitle, false);
    }

    internal void GapChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }
}

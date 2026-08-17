using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public partial class ChaptersViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ChapterItem> _chapters = new();
    [ObservableProperty] private ChapterItem? _selectedChapter;
    [ObservableProperty] private bool _hasChapters;
    [ObservableProperty] private bool _hasSelectedChapter;
    [ObservableProperty] private bool _isVideoLoaded;
    [ObservableProperty] private bool _canWriteToVideo;
    [ObservableProperty] private string _chapterCountDisplay = "0";
    [ObservableProperty] private string _videoFileNameDisplay = string.Empty;

    [ObservableProperty] private ObservableCollection<ChapterExportFormat> _exportFormats = new();
    [ObservableProperty] private ChapterExportFormat? _selectedExportFormat;

    [ObservableProperty] private TimeSpan _shiftTime = TimeSpan.Zero;
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;

    private static readonly List<double> StandardFrameRates = FrameRateHelper.StandardRates.ToList();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _videoFileName = string.Empty;

    /// <summary>
    /// Reads the current video position in seconds. The main window owns the player, so the dialog
    /// asks for the position rather than holding on to the player itself.
    /// </summary>
    private Func<double>? _getVideoPositionSeconds;

    private Action<double>? _seekVideoToSeconds;

    public ChaptersViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        FromFrameRates = new ObservableCollection<double>(StandardFrameRates);
        ToFrameRates = new ObservableCollection<double>(StandardFrameRates);
        SelectedFromFrameRate = 25;
        SelectedToFrameRate = 23.976;

        ExportFormats = new ObservableCollection<ChapterExportFormat>
        {
            new(ChapterExportKind.MatroskaXml, new MatroskaChaptersXml().Name, ".xml"),
            new(ChapterExportKind.FfmpegMetadata, new FfmpegMetadataChapters().Name, ".ffmeta"),
            new(ChapterExportKind.Ogm, new OgmChapters().Name, ".txt"),
            new(ChapterExportKind.YouTube, new YouTubeChapters().Name, ".txt"),
        };
        SelectedExportFormat = ExportFormats[0];
    }

    public void Initialize(
        string videoFileName,
        IEnumerable<Chapter> chapters,
        Func<double>? getVideoPositionSeconds,
        Action<double>? seekVideoToSeconds,
        double frameRate)
    {
        _videoFileName = videoFileName ?? string.Empty;
        _getVideoPositionSeconds = getVideoPositionSeconds;
        _seekVideoToSeconds = seekVideoToSeconds;

        IsVideoLoaded = !string.IsNullOrEmpty(_videoFileName);
        CanWriteToVideo = IsVideoLoaded && VideoChapterReader.IsSupportedContainer(_videoFileName);
        VideoFileNameDisplay = IsVideoLoaded
            ? Path.GetFileName(_videoFileName)
            : Se.Language.Video.Chapters.NoVideoLoaded;

        if (frameRate > 1)
        {
            AddFrameRate(frameRate);
            SelectedToFrameRate = frameRate;
            SelectedFromFrameRate = Math.Abs(frameRate - 25.0) < 0.01 ? 23.976 : 25.0;
        }

        foreach (var chapter in chapters.OrderBy(p => p.StartMilliseconds))
        {
            Chapters.Add(new ChapterItem(chapter));
        }

        Renumber();

        if (Chapters.Count > 0)
        {
            SelectedChapter = Chapters[0];
        }
    }

    /// <summary>
    /// The edited chapters, always in timeline order regardless of how the list happens to be
    /// arranged in the grid.
    /// </summary>
    public List<Chapter> GetChapters()
    {
        return Chapters
            .Select(p => p.ToChapter())
            .OrderBy(p => p.StartMilliseconds)
            .ToList();
    }

    [RelayCommand]
    private void AddChapterAtVideoPosition()
    {
        var seconds = _getVideoPositionSeconds?.Invoke() ?? 0;
        AddChapterAt(seconds * TimeCode.BaseUnit);
    }

    [RelayCommand]
    private void AddChapter()
    {
        // Without a video to point at, a new chapter goes after the last one rather than on top of it.
        var ms = Chapters.Count == 0 ? 0 : Chapters.Max(p => p.StartMilliseconds) + 60000;
        AddChapterAt(ms);
    }

    private void AddChapterAt(double startMilliseconds)
    {
        var item = new ChapterItem(
            Math.Max(0, startMilliseconds),
            string.Format(Se.Language.Video.Chapters.NewChapterTitle, Chapters.Count + 1));

        Chapters.Add(item);
        SortByTime();
        SelectedChapter = item;
    }

    [RelayCommand]
    private void SetSelectedToVideoPosition()
    {
        if (SelectedChapter == null || _getVideoPositionSeconds == null)
        {
            return;
        }

        SelectedChapter.StartMilliseconds = Math.Max(0, _getVideoPositionSeconds() * TimeCode.BaseUnit);
        var selected = SelectedChapter;
        SortByTime();
        SelectedChapter = selected;
    }

    [RelayCommand]
    private void GoToSelectedChapter()
    {
        if (SelectedChapter != null)
        {
            _seekVideoToSeconds?.Invoke(SelectedChapter.StartSeconds);
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedChapter()
    {
        if (SelectedChapter == null || Window == null)
        {
            return;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.DeleteCurrentLine,
            Se.Language.Video.Chapters.DeleteSelectedChapterQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var index = Chapters.IndexOf(SelectedChapter);
        Chapters.Remove(SelectedChapter);
        Renumber();

        SelectedChapter = Chapters.Count > 0
            ? Chapters[Math.Min(index, Chapters.Count - 1)]
            : null;
    }

    [RelayCommand]
    private async Task ClearChapters()
    {
        if (Window == null || Chapters.Count == 0)
        {
            return;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.Clear,
            Se.Language.Video.Chapters.ClearChaptersQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        Chapters.Clear();
        SelectedChapter = null;
        UpdateCounts();
    }

    [RelayCommand]
    private async Task ImportFromVideo()
    {
        if (Window == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var chapters = VideoChapterReader.GetChapters(_videoFileName);
        if (chapters.Count == 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.Video.Chapters.ImportFromVideo,
                Se.Language.Video.Chapters.NoChaptersFoundInVideo,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ReplaceChapters(chapters);
    }

    [RelayCommand]
    private async Task ImportFromFile()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.Video.Chapters.ImportFromFile,
            Se.Language.Video.Chapters.ChapterFilesFilter,
            "*.xml;*.txt;*.ffmeta;*.ini",
            Se.Language.General.AllFiles,
            "*.*");

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            var chapters = ReadChapterFile(fileName);
            if (chapters.Count == 0)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.Video.Chapters.ImportFromFile,
                    Se.Language.Video.Chapters.NoChaptersFoundInVideo,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ReplaceChapters(chapters);
        }
        catch (Exception exception)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message);
        }
    }

    /// <summary>
    /// Reads any of the chapter formats, and falls back to the normal subtitle auto-detect so a
    /// plain subtitle file can be turned into chapters too.
    /// </summary>
    private static List<Chapter> ReadChapterFile(string fileName)
    {
        var subtitle = Subtitle.Parse(fileName);
        if (subtitle != null && subtitle.Paragraphs.Count > 0)
        {
            return ChapterHelper.FromSubtitle(subtitle);
        }

        return new List<Chapter>();
    }

    [RelayCommand]
    private async Task ExportToFile()
    {
        if (Window == null || Chapters.Count == 0)
        {
            return;
        }

        var format = SelectedExportFormat ?? ExportFormats[0];

        var suggestedName = string.IsNullOrEmpty(_videoFileName)
            ? "chapters"
            : Path.GetFileNameWithoutExtension(_videoFileName);

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            format.Extension,
            suggestedName + format.Extension,
            Se.Language.Video.Chapters.ExportToFile);

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            await File.WriteAllTextAsync(fileName, GetChapterFileText(format.Kind, GetChapters()));
        }
        catch (Exception exception)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message);
        }
    }

    /// <summary>
    /// The writer comes from the format the user picked, not from the file name: OGM chapters and
    /// YouTube chapters are both ".txt", so an extension cannot tell them apart.
    /// </summary>
    internal static string GetChapterFileText(ChapterExportKind kind, List<Chapter> chapters)
    {
        switch (kind)
        {
            case ChapterExportKind.MatroskaXml:
                return MatroskaChaptersXml.ToXml(chapters, "und");
            case ChapterExportKind.FfmpegMetadata:
                return FfmpegMetadataChapters.ToFfmpegMetadata(chapters);
            case ChapterExportKind.YouTube:
                return YouTubeChapters.ToDescriptionText(chapters);
            default:
                return new OgmChapters().ToText(ChapterHelper.ToSubtitle(chapters), "chapters");
        }
    }

    [RelayCommand]
    private void ApplyShift()
    {
        var deltaMs = ShiftTime.TotalMilliseconds;
        if (Math.Abs(deltaMs) < 0.001)
        {
            return;
        }

        foreach (var chapter in Chapters)
        {
            chapter.StartMilliseconds = Math.Max(0, chapter.StartMilliseconds + deltaMs);
        }

        var selected = SelectedChapter;
        SortByTime();
        SelectedChapter = selected;
    }

    [RelayCommand]
    private void ApplyFrameRateScale()
    {
        if (SelectedFromFrameRate <= 0 || SelectedToFrameRate <= 0 ||
            Math.Abs(SelectedFromFrameRate - SelectedToFrameRate) < 0.001)
        {
            return;
        }

        var factor = SelectedFromFrameRate / SelectedToFrameRate;
        foreach (var chapter in Chapters)
        {
            chapter.StartMilliseconds = Math.Max(0, chapter.StartMilliseconds * factor);
        }

        var selected = SelectedChapter;
        SortByTime();
        SelectedChapter = selected;
    }

    [RelayCommand]
    private async Task WriteToVideo()
    {
        if (Window == null || !CanWriteToVideo || Chapters.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<WriteChaptersToVideoWindow, WriteChaptersToVideoViewModel>(
            Window,
            vm => vm.Initialize(_videoFileName, GetChapters()));

        if (result.OkPressed && !string.IsNullOrEmpty(result.OutputFileName))
        {
            await MessageBox.Show(
                Window,
                Se.Language.Video.Chapters.WriteToVideoTitle,
                string.Format(Se.Language.Video.Chapters.WrittenToX, result.OutputFileName),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        OkPressed = false;
        Window?.Close();
    }

    private void ReplaceChapters(IEnumerable<Chapter> chapters)
    {
        Chapters.Clear();
        foreach (var chapter in chapters.OrderBy(p => p.StartMilliseconds))
        {
            Chapters.Add(new ChapterItem(chapter));
        }

        Renumber();
        SelectedChapter = Chapters.FirstOrDefault();
    }

    /// <summary>
    /// Sorting happens on the actions that move times deliberately (add, import, shift, scale), not
    /// while a start time is being typed - a list that reorders under the caret is unusable.
    /// </summary>
    private void SortByTime()
    {
        var ordered = Chapters.OrderBy(p => p.StartMilliseconds).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var currentIndex = Chapters.IndexOf(ordered[i]);
            if (currentIndex != i)
            {
                Chapters.Move(currentIndex, i);
            }
        }

        Renumber();
    }

    private void Renumber()
    {
        for (var i = 0; i < Chapters.Count; i++)
        {
            Chapters[i].Number = i + 1;
        }

        UpdateCounts();
    }

    private void UpdateCounts()
    {
        HasChapters = Chapters.Count > 0;
        ChapterCountDisplay = Chapters.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private void AddFrameRate(double frameRate)
    {
        if (!FromFrameRates.Any(p => Math.Abs(p - frameRate) < 0.001))
        {
            FromFrameRates = new ObservableCollection<double>(FromFrameRates.Append(frameRate).OrderBy(p => p));
            ToFrameRates = new ObservableCollection<double>(ToFrameRates.Append(frameRate).OrderBy(p => p));
        }
    }

    /// <summary>
    /// Follows the selection itself rather than the grid's SelectionChanged: the row the grid picks
    /// on its own (AlwaysSelected) never raises that event.
    /// </summary>
    partial void OnSelectedChapterChanged(ChapterItem? value)
    {
        HasSelectedChapter = value != null;
    }

    partial void OnChaptersChanged(ObservableCollection<ChapterItem> value)
    {
        UpdateCounts();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedChapter != null)
        {
            e.Handled = true;
            Dispatcher.UIThread.Invoke(async void () => await DeleteSelectedChapter());
        }
    }

    internal void OnChapterGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(GoToSelectedChapter);
    }
}

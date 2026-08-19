using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using Nikse.SubtitleEdit.Features.Video.Chapters;

namespace UITests.Features.Video;

public class ChaptersViewModelTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static List<Chapter> MakeChapters() => new()
    {
        new Chapter(0, "Intro"),
        new Chapter(10_000, "Middle"),
        new Chapter(20_000, "Outro"),
    };

    private ChaptersViewModel ShowWindow(List<Chapter>? chapters = null, double videoPositionSeconds = 0, string videoFileName = "movie.mkv")
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var vm = services.BuildServiceProvider().GetRequiredService<ChaptersViewModel>();

        var window = new ChaptersWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize(videoFileName, chapters ?? MakeChapters(), () => videoPositionSeconds, _ => { }, 25);
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    [AvaloniaFact]
    public void InitializeFillsTheListAndSelectsTheFirstChapter()
    {
        var vm = ShowWindow();

        Assert.Equal(3, vm.Chapters.Count);
        Assert.Equal("3", vm.ChapterCountDisplay);
        Assert.True(vm.HasChapters);
        Assert.Equal("Intro", vm.SelectedChapter?.Title);
        Assert.Equal(new[] { 1, 2, 3 }, vm.Chapters.Select(p => p.Number));
    }

    [AvaloniaFact]
    public void AddingAtTheVideoPositionKeepsTheListInTimeOrder()
    {
        var vm = ShowWindow(videoPositionSeconds: 15);

        vm.AddChapterAtVideoPositionCommand.Execute(null);

        Assert.Equal(4, vm.Chapters.Count);
        Assert.Equal(15_000, vm.Chapters[2].StartMilliseconds);
        Assert.Equal(new[] { 0d, 10_000, 15_000, 20_000 }, vm.Chapters.Select(p => p.StartMilliseconds));
        Assert.Equal(new[] { 1, 2, 3, 4 }, vm.Chapters.Select(p => p.Number));

        // The chapter just added stays selected even though sorting moved it.
        Assert.Equal(15_000, vm.SelectedChapter?.StartMilliseconds);
    }

    [AvaloniaFact]
    public void ChaptersComeBackInTimeOrderEvenWhenTheGridIsNot()
    {
        var vm = ShowWindow();

        // Editing a start time in place deliberately does not re-sort the grid under the caret.
        vm.Chapters[0].StartMilliseconds = 99_000;

        var chapters = vm.GetChapters();

        Assert.Equal(new[] { 10_000d, 20_000, 99_000 }, chapters.Select(p => p.StartMilliseconds));
    }

    [AvaloniaFact]
    public void ShiftMovesEveryChapterAndNeverBeforeZero()
    {
        var vm = ShowWindow();
        vm.ShiftTime = TimeSpan.FromSeconds(-5);

        vm.ApplyShiftCommand.Execute(null);

        Assert.Equal(new[] { 0d, 5_000, 15_000 }, vm.Chapters.Select(p => p.StartMilliseconds));
    }

    [AvaloniaFact]
    public void FrameRateScaleStretchesEveryChapter()
    {
        var vm = ShowWindow();
        vm.SelectedFromFrameRate = 25;
        vm.SelectedToFrameRate = 50;

        vm.ApplyFrameRateScaleCommand.Execute(null);

        Assert.Equal(new[] { 0d, 5_000, 10_000 }, vm.Chapters.Select(p => p.StartMilliseconds));
    }

    [AvaloniaFact]
    public void SettingTheSelectedChapterToTheVideoPositionResorts()
    {
        var vm = ShowWindow(videoPositionSeconds: 30);
        vm.SelectedChapter = vm.Chapters[0];

        vm.SetSelectedToVideoPositionCommand.Execute(null);

        Assert.Equal(new[] { 10_000d, 20_000, 30_000 }, vm.Chapters.Select(p => p.StartMilliseconds));
        Assert.Equal("Intro", vm.SelectedChapter?.Title);
        Assert.Equal(30_000, vm.SelectedChapter?.StartMilliseconds);
    }

    [AvaloniaFact]
    public void WriteToVideoIsOnlyOfferedForContainersThatCanHoldChapters()
    {
        Assert.True(ShowWindow(videoFileName: "movie.mkv").CanWriteToVideo);
        Assert.True(ShowWindow(videoFileName: "movie.mp4").CanWriteToVideo);
        Assert.False(ShowWindow(videoFileName: "movie.avi").CanWriteToVideo);
        Assert.False(ShowWindow(videoFileName: string.Empty).CanWriteToVideo);
    }

    [AvaloniaFact]
    public void WithoutAVideoTheChapterListStillWorks()
    {
        var vm = ShowWindow(chapters: new List<Chapter>(), videoFileName: string.Empty);

        Assert.False(vm.IsVideoLoaded);
        Assert.False(vm.HasChapters);

        vm.AddChapterCommand.Execute(null);
        vm.AddChapterCommand.Execute(null);

        Assert.Equal(2, vm.Chapters.Count);
        Assert.Equal(0, vm.Chapters[0].StartMilliseconds);
        Assert.Equal(60_000, vm.Chapters[1].StartMilliseconds);
    }

    /// <summary>
    /// OGM and YouTube chapters are both ".txt", so the export writer has to come from the picked
    /// format rather than from the file extension.
    /// </summary>
    [Theory]
    [InlineData(ChapterExportKind.MatroskaXml, "<ChapterTimeStart>")]
    [InlineData(ChapterExportKind.FfmpegMetadata, ";FFMETADATA1")]
    [InlineData(ChapterExportKind.Ogm, "CHAPTER01=")]
    [InlineData(ChapterExportKind.YouTube, "0:00 Intro")]
    public void EachExportKindWritesItsOwnFormat(ChapterExportKind kind, string expected)
    {
        var text = ChaptersViewModel.GetChapterFileText(kind, MakeChapters());

        Assert.Contains(expected, text);
    }

    [AvaloniaFact]
    public void ExportFormatsAreOfferedWithMatroskaFirst()
    {
        var vm = ShowWindow();

        Assert.Equal(4, vm.ExportFormats.Count);
        Assert.Equal(ChapterExportKind.MatroskaXml, vm.SelectedExportFormat?.Kind);
        Assert.Equal(".xml", vm.SelectedExportFormat?.Extension);
    }
}

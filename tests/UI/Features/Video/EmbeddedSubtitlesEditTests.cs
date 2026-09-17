using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// The track list in "Add/remove embedded subtitles" can be reordered, and that order is the
/// output subtitle track order - including new tracks moved above original ones.
/// </summary>
public class EmbeddedSubtitlesEditTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { /* best effort */ }
        }
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private string MakeTempSubtitle()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"embedded-subs-test-{Guid.NewGuid():N}.srt");
        File.WriteAllText(fileName, "1\n00:00:01,000 --> 00:00:02,000\nHi\n");
        _tempFiles.Add(fileName);
        return fileName;
    }

    private List<EmbeddedTrack> MakeInterleavedTracks()
    {
        return new List<EmbeddedTrack>
        {
            new() { Number = 1, Name = "Original B", LanguageOrTitle = "dan" },
            new() { New = true, FileName = MakeTempSubtitle(), Name = "New A", LanguageOrTitle = "eng", Default = true },
            new() { Number = 0, Name = "Original A", LanguageOrTitle = "swe", Deleted = true },
            new() { New = true, FileName = MakeTempSubtitle(), Name = "New B", LanguageOrTitle = "fin" },
            new() { Number = 2, Name = "Original C", LanguageOrTitle = "nor" },
        };
    }

    private static void AssertInOrder(string args, params string[] parts)
    {
        var position = -1;
        foreach (var part in parts)
        {
            var next = args.IndexOf(part, position + 1, StringComparison.Ordinal);
            Assert.True(next > position, $"'{part}' not found after position {position} in: {args}");
            position = next;
        }
    }

    [Fact]
    public void AlterEmbeddedTracksMatroska_MapsSubtitlesInListOrder()
    {
        var args = FfmpegGenerator.AlterEmbeddedTracksMatroska(MakeInterleavedTracks(), new List<EmbeddedTrack>(), "in.mkv", "out.mkv");

        AssertInOrder(args, "-map 0:s:1", "-map 1:0", "-map 2:0", "-map 0:s:2");
        Assert.DoesNotContain("-map 0:s:0", args);
        AssertInOrder(args,
            "-metadata:s:s:0 title=\"Original B\"",
            "-metadata:s:s:1 title=\"New A\"",
            "-disposition:s:1 default",
            "-metadata:s:s:2 title=\"New B\"",
            "-metadata:s:s:3 title=\"Original C\"");
    }

    [Fact]
    public void AlterEmbeddedTracksMp4_MapsSubtitlesInListOrder()
    {
        var args = FfmpegGenerator.AlterEmbeddedTracksMp4(MakeInterleavedTracks(), new List<EmbeddedTrack>(), "in.mp4", "out.mp4");

        AssertInOrder(args, "-map 0:s:1", "-map 1:0", "-map 2:0", "-map 0:s:2");
        Assert.DoesNotContain("-map 0:s:0", args);
        AssertInOrder(args,
            "-metadata:s:s:0 language=dan",
            "-metadata:s:s:1 language=eng",
            "-metadata:s:s:2 language=fin",
            "-metadata:s:s:3 language=nor");
    }

    [AvaloniaFact]
    public void MoveUpDown_ReordersTracksAndKeepsSelection()
    {
        var vm = new EmbeddedSubtitlesEditViewModel(new FolderHelper(), new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.VideoFileName = "movie.mkv";
        var a = new EmbeddedTrack { Name = "A" };
        var b = new EmbeddedTrack { Name = "B" };
        var c = new EmbeddedTrack { Name = "C" };
        vm.Tracks.Add(a);
        vm.Tracks.Add(b);
        vm.Tracks.Add(c);

        vm.SelectedTrck = a;
        Assert.False(vm.IsMoveUpEnabled);
        Assert.True(vm.IsMoveDownEnabled);

        vm.MoveDownCommand.Execute(null);
        Assert.Equal(new[] { b, a, c }, vm.Tracks);
        Assert.Same(a, vm.SelectedTrck);
        Assert.True(vm.IsMoveUpEnabled);

        vm.MoveDownCommand.Execute(null);
        Assert.Equal(new[] { b, c, a }, vm.Tracks);
        Assert.False(vm.IsMoveDownEnabled);

        vm.MoveUpCommand.Execute(null);
        Assert.Equal(new[] { b, a, c }, vm.Tracks);
    }

    [AvaloniaFact]
    public void Delete_TogglesDeleteTextForSelectedTrack()
    {
        var vm = new EmbeddedSubtitlesEditMp4ViewModel(new FolderHelper(), new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.VideoFileName = "movie.mp4";
        vm.TracksReady = true;
        var track = new EmbeddedTrack { Name = "A" };
        vm.Tracks.Add(track);
        vm.SelectedTrack = track;
        Assert.True(vm.IsTrackSelected);
        Assert.Equal(Se.Language.General.Delete, vm.DeleteText);

        vm.DeleteCommand.Execute(null);
        Assert.True(track.Deleted);
        Assert.Equal(Se.Language.General.Undelete, vm.DeleteText);

        vm.DeleteCommand.Execute(null);
        Assert.Equal(Se.Language.General.Delete, vm.DeleteText);
    }

    [Theory]
    [InlineData("movie.mkv", true)]
    [InlineData("movie.WEBM", true)]
    [InlineData("movie.mp4", false)]
    [InlineData("movie", false)]
    public void IsSupportedVideoFile_Matroska(string fileName, bool expected)
    {
        Assert.Equal(expected, EmbeddedSubtitlesEditViewModel.IsSupportedVideoFile(fileName));
    }

    [Theory]
    [InlineData("movie.mp4", true)]
    [InlineData("movie.M4V", true)]
    [InlineData("movie.mov", true)]
    [InlineData("movie.mkv", false)]
    public void IsSupportedVideoFile_Mp4(string fileName, bool expected)
    {
        Assert.Equal(expected, EmbeddedSubtitlesEditMp4ViewModel.IsSupportedVideoFile(fileName));
    }

    [AvaloniaFact]
    public void VideoFileSize_FollowsVideoFileName()
    {
        var vm = new EmbeddedSubtitlesEditViewModel(new FolderHelper(), new FileHelper(), new WindowService(new NullServiceProvider()));
        var fileName = MakeTempSubtitle();

        vm.VideoFileName = fileName;
        Assert.False(string.IsNullOrEmpty(vm.VideoFileSize));

        vm.VideoFileName = string.Empty;
        Assert.Equal(string.Empty, vm.VideoFileSize);
    }

    [AvaloniaFact]
    public void Generating_LocksTheTrackList()
    {
        var vm = new EmbeddedSubtitlesEditViewModel(new FolderHelper(), new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.VideoFileName = "movie.mkv";
        var a = new EmbeddedTrack { Name = "A" };
        var b = new EmbeddedTrack { Name = "B" };
        vm.Tracks.Add(a);
        vm.Tracks.Add(b);
        vm.SelectedTrck = a;
        Assert.True(vm.CanEditTracks);
        Assert.True(vm.IsTrackSelected);
        Assert.True(vm.IsMoveDownEnabled);

        vm.IsGenerating = true;
        Assert.False(vm.CanEditTracks);
        Assert.False(vm.IsTrackSelected);
        Assert.False(vm.IsMoveDownEnabled);

        vm.DeleteCommand.Execute(null);
        vm.MoveDownCommand.Execute(null);
        Assert.False(a.Deleted);
        Assert.Equal(new[] { a, b }, vm.Tracks);

        vm.IsGenerating = false;
        Assert.True(vm.IsTrackSelected);
        Assert.True(vm.IsMoveDownEnabled);
    }

    [AvaloniaFact]
    public void Generating_LocksTheTrackListMp4()
    {
        var vm = new EmbeddedSubtitlesEditMp4ViewModel(new FolderHelper(), new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.VideoFileName = "movie.mp4";
        vm.TracksReady = true;
        var track = new EmbeddedTrack { Name = "A" };
        vm.Tracks.Add(track);
        vm.SelectedTrack = track;
        Assert.True(vm.IsTrackSelected);

        vm.IsGenerating = true;
        Assert.False(vm.CanEditTracks);
        Assert.False(vm.IsTrackSelected);
        vm.DeleteCommand.Execute(null);
        Assert.False(track.Deleted);

        vm.IsGenerating = false;
        Assert.True(vm.CanEditTracks);
        Assert.True(vm.IsTrackSelected);
    }
}

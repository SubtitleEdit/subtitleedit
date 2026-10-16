using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Video.SyncAudioTrack;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.SyncAudioTrack;

public class SyncAudioTrackViewModelTests
{
    private static SyncAudioTrackViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<SyncAudioTrackViewModel>();
    }

    private static FfmpegMediaInfo2 MediaInfo(params string[] streams) => FfmpegMediaInfo2.ParseLog(
        "Input #0, matroska,webm, from 'in.mkv':\n  Duration: 00:01:00.00, start: 0.000000, bitrate: 128 kb/s\n" +
        string.Concat(streams.Select((s, i) => $"  Stream #0:{i}{s}\n")));

    [AvaloniaFact]
    public void TwoAudioTracks_ShowsPicker()
    {
        var vm = MakeViewModel();
        vm.SetMediaInfo(MediaInfo(": Video: h264, yuv420p, 640x360, 25 fps", "(eng): Audio: aac (LC), 48000 Hz, stereo", "(dan): Audio: ac3, 48000 Hz, 5.1"));

        Assert.True(vm.IsAudioTrackPickerVisible);
        Assert.Equal(new[] { 1, 2 }, vm.AudioTracks.Select(t => t.StreamIndex));
        Assert.StartsWith("#2: dan - ac3", vm.AudioTracks[1].Display);
        Assert.Same(vm.AudioTracks[0], vm.SelectedAudioTrack);
        Assert.Equal(string.Empty, vm.ErrorText);
    }

    [AvaloniaFact]
    public void OneAudioTrack_NoPicker()
    {
        var vm = MakeViewModel();
        vm.SetMediaInfo(MediaInfo(": Video: h264, yuv420p, 640x360, 25 fps", "(eng): Audio: aac (LC), 48000 Hz, stereo"));

        Assert.False(vm.IsAudioTrackPickerVisible);
        Assert.Equal(1, vm.SelectedAudioTrack!.StreamIndex);
    }

    [AvaloniaFact]
    public void NoAudio_SaysSo()
    {
        var vm = MakeViewModel();
        vm.SetMediaInfo(MediaInfo(": Video: h264, yuv420p, 640x360, 25 fps"));

        Assert.Null(vm.SelectedAudioTrack);
        Assert.False(string.IsNullOrEmpty(vm.ErrorText));
    }

    [AvaloniaFact]
    public void Nudge_And_Reset()
    {
        var vm = MakeViewModel();
        vm.NudgeCommand.Execute("+100");
        vm.NudgeCommand.Execute("-10");
        vm.NudgeCommand.Execute("+1000");
        Assert.Equal(1090, vm.OffsetMs);

        vm.ResetOffsetCommand.Execute(null);
        Assert.Equal(0, vm.OffsetMs);
    }
}

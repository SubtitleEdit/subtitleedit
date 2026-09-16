using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// The burn-in "Tune" list. Only the NVIDIA encoders have a tuning mode, and it is how lossless
/// and low-latency encoding are reached now that ffmpeg 9 has removed the "lossless"/"ll" preset
/// aliases (issue #14927) - so the row has to follow the chosen encoder.
/// </summary>
public class BurnInTuneTests
{
    private static BurnInViewModel BuildViewModel()
    {
        return new BurnInViewModel(
            new FolderHelper(),
            new FileHelper(),
            new WindowService(new NullServiceProvider()));
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static void SelectEncoding(BurnInViewModel vm, string codec)
    {
        vm.SelectedVideoEncoding = new VideoEncodingItem(codec, codec);
        vm.VideoEncodingChanged(null, null!);
    }

    [AvaloniaFact]
    public void Nvenc_ShowsTheTuningModes()
    {
        var vm = BuildViewModel();

        SelectEncoding(vm, "h264_nvenc");

        Assert.True(vm.IsVideoTuneVisible);
        Assert.Equal(new[] { VideoPresetOptions.BlankTune, "hq", "ll", "ull", "lossless" }, vm.VideoTunes);
        Assert.Equal(VideoPresetOptions.BlankTune, vm.SelectedVideoTune);
    }

    [AvaloniaFact]
    public void OtherEncoders_HideTheRow()
    {
        var vm = BuildViewModel();

        SelectEncoding(vm, "libx264");

        Assert.False(vm.IsVideoTuneVisible);
        Assert.Equal(VideoPresetOptions.BlankTune, vm.SelectedVideoTune);
    }

    [AvaloniaFact]
    public void SwitchingBetweenNvidiaEncoders_KeepsTheTuningMode()
    {
        var vm = BuildViewModel();

        SelectEncoding(vm, "h264_nvenc");
        vm.SelectedVideoTune = "lossless";
        SelectEncoding(vm, "hevc_nvenc");

        Assert.True(vm.IsVideoTuneVisible);
        Assert.Equal("lossless", vm.SelectedVideoTune);
    }

    [AvaloniaFact]
    public void LeavingNvidia_DropsTheTuningMode()
    {
        // "-tune lossless" means nothing to libx264 and would abort the encode, so it must not
        // survive a switch to a CPU encoder.
        var vm = BuildViewModel();

        SelectEncoding(vm, "h264_nvenc");
        vm.SelectedVideoTune = "lossless";
        SelectEncoding(vm, "libx264");

        Assert.False(vm.IsVideoTuneVisible);
        Assert.Equal(VideoPresetOptions.BlankTune, vm.SelectedVideoTune);
    }
}

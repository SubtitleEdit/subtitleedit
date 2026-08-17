using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.GetAudioClips;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// The audio-clip extraction must start exactly once per window. It used to be kicked off from the
/// window's Activated event, which fires again every time the user tabs away and back - so a second
/// loop started from line one while the first was still going, and the two overwrote each other's
/// progress, making the counter jump backwards (#13777).
/// </summary>
public class GetAudioClipsStartOnceTests
{
    [AvaloniaFact]
    public void StartAudioExtract_OnlyStartsOneRun()
    {
        // No lines and no center-channel probe, so nothing spawns ffmpeg - the guard under test is
        // synchronous and runs before any of that anyway.
        var originalCenterChannel = Se.Settings.General.FfmpegUseCenterChannelOnly;
        Se.Settings.General.FfmpegUseCenterChannelOnly = false;
        try
        {
            var vm = new GetAudioClipsViewModel();
            vm.Initialize("video.mp4", []);

            Assert.True(vm.StartAudioExtract());

            // Every later activation of the window used to land here.
            Assert.False(vm.StartAudioExtract());
            Assert.False(vm.StartAudioExtract());
        }
        finally
        {
            Se.Settings.General.FfmpegUseCenterChannelOnly = originalCenterChannel;
        }
    }
}

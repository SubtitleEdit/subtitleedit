using System.Reflection;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// AMF "-quality" used to be stored as a number ("0".."10"); the burn-in list now holds the
/// names ffmpeg accepts. A settings file written before that change must still land on the
/// quality the user picked when the window opens, instead of silently falling back to blank.
/// </summary>
public class BurnInStoredAmfQualityTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    /// <summary>
    /// Builds the window with the AMD encoders on offer (they are Windows/Linux only, so the
    /// list may not hold them here) and replays the settings load the constructor does.
    /// </summary>
    private static BurnInViewModel BuildViewModel(string codec)
    {
        var vm = new BurnInViewModel(
            new FolderHelper(),
            new FileHelper(),
            new WindowService(new NullServiceProvider()));

        if (vm.VideoEncodings.All(p => p.Codec != codec))
        {
            vm.VideoEncodings.Add(new VideoEncodingItem(codec, codec));
        }

        // FillCrf carries the current pick over to the new list, so clear it: only the stored
        // value may produce a selection here.
        vm.SelectedVideoCrf = null;
        var loadSettings = typeof(BurnInViewModel)
            .GetMethod("LoadSettings", BindingFlags.Instance | BindingFlags.NonPublic)!;
        loadSettings.Invoke(vm, null);
        return vm;
    }

    [AvaloniaTheory]
    [InlineData("hevc_amf", "0", "quality")]
    [InlineData("hevc_amf", "10", "speed")]
    [InlineData("h264_amf", "2", "quality")]
    public void StoredAmfNumber_LoadsAsTheMappedName(string codec, string stored, string expected)
    {
        using var _ = new SettingsScope("Video.BurnIn.Encoding", "Video.BurnIn.Crf");
        Se.Settings.Video.BurnIn.Encoding = codec;
        Se.Settings.Video.BurnIn.Crf = stored;

        var vm = BuildViewModel(codec);

        Assert.Equal(codec, vm.SelectedVideoEncoding.Codec);
        Assert.Equal(expected, vm.SelectedVideoCrf);
    }

    [AvaloniaFact]
    public void StoredAmfName_StillLoads()
    {
        using var _ = new SettingsScope("Video.BurnIn.Encoding", "Video.BurnIn.Crf");
        Se.Settings.Video.BurnIn.Encoding = "hevc_amf";
        Se.Settings.Video.BurnIn.Crf = "balanced";

        var vm = BuildViewModel("hevc_amf");

        Assert.Equal("balanced", vm.SelectedVideoCrf);
    }
}

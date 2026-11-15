using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// LayoutResX/LayoutResY in the properties dialog, the resampler and the video display size (#16933).
/// </summary>
public class AssaLayoutResolutionTests
{
    private static Subtitle MakeSubtitle(string extraScriptInfo = "")
    {
        var header = AdvancedSubStationAlpha.SetResolution(AdvancedSubStationAlpha.DefaultHeader, 1920, 1080);
        if (extraScriptInfo.Length > 0)
        {
            header = header.Replace("[V4+ Styles]", extraScriptInfo + Environment.NewLine + Environment.NewLine + "[V4+ Styles]");
        }

        return new Subtitle { Header = header };
    }

    private static AssaPropertiesViewModel MakeViewModel(Subtitle subtitle, int videoWidth = 0, int videoHeight = 0, int displayWidth = 0, int displayHeight = 0)
    {
        var vm = new AssaPropertiesViewModel(new StubFileHelper(), new StubWindowService());
        vm.Initialize(subtitle, new AdvancedSubStationAlpha(), "test.ass", string.Empty, videoWidth, videoHeight, displayWidth, displayHeight);
        return vm;
    }

    [AvaloniaFact]
    public void Properties_LoadsAndKeepsLayoutRes()
    {
        var vm = MakeViewModel(MakeSubtitle("LayoutResX: 1024\r\nLayoutResY: 576"));

        Assert.Equal(1024, vm.LayoutWidth);
        Assert.Equal(576, vm.LayoutHeight);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(vm.Header, out var w, out var h));
        Assert.Equal((1024, 576), (w, h));
    }

    [AvaloniaFact]
    public void Properties_WithoutLayoutRes_DoesNotAddIt()
    {
        var vm = MakeViewModel(MakeSubtitle());

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain("LayoutRes", vm.Header);
    }

    [AvaloniaFact]
    public void Properties_Clear_RemovesLayoutRes()
    {
        var vm = MakeViewModel(MakeSubtitle("LayoutResX: 1024\r\nLayoutResY: 576"));

        vm.ClearLayoutResolutionCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain("LayoutRes", vm.Header);
    }

    [AvaloniaFact]
    public void Properties_FromVideo_UsesDisplaySize()
    {
        var vm = MakeViewModel(MakeSubtitle(), 720, 576, 1024, 576);

        vm.GetLayoutResolutionFromCurrentVideoCommand.Execute(null);

        Assert.Equal((1024, 576), (vm.LayoutWidth, vm.LayoutHeight));
    }

    [AvaloniaFact]
    public void Properties_ScriptResolutionFromVideo_FillsUnsetLayoutRes_KeepsExisting()
    {
        var unset = MakeViewModel(MakeSubtitle(), 720, 576, 1024, 576);
        unset.GetResolutionFromCurrentVideoCommand.Execute(null);
        Assert.Equal((720, 576), (unset.VideoWidth, unset.VideoHeight));
        Assert.Equal((1024, 576), (unset.LayoutWidth, unset.LayoutHeight));

        var existing = MakeViewModel(MakeSubtitle("LayoutResX: 1920\r\nLayoutResY: 1080"), 1280, 720, 1280, 720);
        existing.GetResolutionFromCurrentVideoCommand.Execute(null);
        Assert.Equal((1920, 1080), (existing.LayoutWidth, existing.LayoutHeight));
    }

    [Fact]
    public void Resample_Proportional_KeepsLayoutRes()
    {
        var subtitle = MakeSubtitle("LayoutResX: 1920\r\nLayoutResY: 1080");

        AssaResamplerHelper.ApplyResampling(subtitle, 1920, 1080, 1280, 720);

        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(subtitle.Header, out var w, out var h));
        Assert.Equal((1920, 1080), (w, h));
        Assert.Equal("1280", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", subtitle.Header));
    }

    [Fact]
    public void Resample_Stretch_AdjustsLayoutResX()
    {
        var subtitle = MakeSubtitle("LayoutResX: 1920\r\nLayoutResY: 1080");

        AssaResamplerHelper.ApplyResampling(subtitle, 1920, 1080, 1440, 1080);

        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(subtitle.Header, out var w, out var h));
        Assert.Equal((1440, 1080), (w, h));
    }

    [Fact]
    public void Resample_WithoutLayoutRes_DoesNotAddIt()
    {
        var subtitle = MakeSubtitle();

        AssaResamplerHelper.ApplyResampling(subtitle, 1920, 1080, 1440, 1080);

        Assert.DoesNotContain("LayoutRes", subtitle.Header);
    }

    [Fact]
    public void MediaInfo_AnamorphicDvd_DisplayDimensionUsesDar()
    {
        var info = FfmpegMediaInfo2.ParseLog(
            "Input #0, mpeg, from 'in.vob':\n  Duration: 00:01:00.00, start: 0.000000, bitrate: 5000 kb/s\n" +
            "  Stream #0:0[0x1e0]: Video: mpeg2video (Main), yuv420p(tv, top first), 720x576 [SAR 64:45 DAR 16:9], 25 fps, 25 tbr\n");

        Assert.Equal(new Dimension(720, 576), info.Dimension);
        Assert.Equal(new Dimension(1024, 576), info.DisplayDimension);
    }

    [Fact]
    public void MediaInfo_SquarePixels_DisplayDimensionEqualsDimension()
    {
        var info = FfmpegMediaInfo2.ParseLog(
            "Input #0, matroska,webm, from 'in.mkv':\n  Duration: 00:01:00.00, start: 0.000000, bitrate: 128 kb/s\n" +
            "  Stream #0:0: Video: h264, yuv420p, 1920x1080 [SAR 1:1 DAR 16:9], 25 fps\n");

        Assert.Equal(new Dimension(1920, 1080), info.DisplayDimension);
    }
}

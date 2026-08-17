using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;

namespace UITests.Features.Sync;

/// <summary>
/// The sync dialogs draw the subtitle on their own video, the way the main window draws it on the
/// main preview - they used to show the video bare, which is the comparison they exist for
/// (discussion #13767). These cover what the dialogs hand to the player: the lines as they stand
/// right now (so a sync moves them), and the format plus header that give ASSA its styles.
/// </summary>
public class SyncPreviewSubtitleTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class FakePreviewSubtitle : IVideoPreviewSubtitle
    {
        public int InvalidateCount { get; private set; }
        public int ResetCount { get; private set; }
        public VideoPreviewSubtitleContext? Context { get; private set; }

        private Func<Subtitle>? _getSubtitle;

        public void Refresh(IVideoPlayer? videoPlayer, Func<Subtitle> getSubtitle, VideoPreviewSubtitleContext context)
        {
            _getSubtitle = getSubtitle;
            Context = context;
        }

        public void Invalidate() => InvalidateCount++;

        public void Reset() => ResetCount++;

        /// <summary>The subtitle the dialog offered, built on demand as the real push would.</summary>
        public Subtitle BuildOffered()
        {
            Assert.NotNull(_getSubtitle);
            return _getSubtitle!();
        }
    }

    private static List<SubtitleLineViewModel> ThreeLines()
        => new()
        {
            new() { StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(12), Text = "One" },
            new() { StartTime = TimeSpan.FromSeconds(30), EndTime = TimeSpan.FromSeconds(32), Text = "Two" },
            new() { StartTime = TimeSpan.FromSeconds(50), EndTime = TimeSpan.FromSeconds(52), Text = "Three" },
        };

    [AvaloniaFact]
    public void VisualSync_OffersTheWholeSubtitleToBothPlayers()
    {
        var left = new FakePreviewSubtitle();
        var right = new FakePreviewSubtitle();
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), left, right);
        vm.Initialize(ThreeLines(), videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.RefreshPreviewSubtitles();

        foreach (var preview in new[] { left, right })
        {
            var subtitle = preview.BuildOffered();
            Assert.Equal(3, subtitle.Paragraphs.Count);
            Assert.Equal("One", subtitle.Paragraphs[0].Text);
            Assert.Equal(10000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
            Assert.Equal("Three", subtitle.Paragraphs[2].Text);
        }
    }

    [AvaloniaFact]
    public void VisualSync_ManualSync_MovesTheSubtitleOnTheVideos()
    {
        // The whole point of showing it: after a sync the video has to draw the lines where they
        // now are, not where they were when the dialog opened.
        var left = new FakePreviewSubtitle();
        var right = new FakePreviewSubtitle();
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), left, right);
        vm.Initialize(ThreeLines(), videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);
        vm.RefreshPreviewSubtitles();

        vm.ApplySync(1.0, 5.0); // what "Sync" and "Manual sync" both end in

        Assert.Equal(1, left.InvalidateCount);
        Assert.Equal(1, right.InvalidateCount);
        Assert.Equal(15000, left.BuildOffered().Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(15000, right.BuildOffered().Paragraphs[0].StartTime.TotalMilliseconds, 3);
    }

    [AvaloniaFact]
    public void VisualSync_CarriesTheAssaStylesAndHeader()
    {
        // Without the header libass has no styles and no PlayRes, so \pos would land against its
        // own default resolution instead of the video's.
        const string header = "[Script Info]\r\nPlayResX: 1920\r\nPlayResY: 1080\r\n\r\n[V4+ Styles]\r\n";
        var lines = ThreeLines();
        lines[0].Style = "Narrator";

        var left = new FakePreviewSubtitle();
        var right = new FakePreviewSubtitle();
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), left, right);
        vm.Initialize(lines, videoFileName: null, subtitleFileName: null, new VideoPreviewSubtitleContext(new AdvancedSubStationAlpha(), header, false), audioVisualizer: null);

        vm.RefreshPreviewSubtitles();

        var subtitle = left.BuildOffered();
        Assert.Equal(header, subtitle.Header);
        Assert.Equal("Narrator", subtitle.Paragraphs[0].Extra);
        Assert.IsType<AdvancedSubStationAlpha>(left.Context!.Format);
    }

    [AvaloniaFact]
    public void VisualSync_Closing_ReleasesBothTempSubtitleFiles()
    {
        var left = new FakePreviewSubtitle();
        var right = new FakePreviewSubtitle();
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), left, right);
        vm.Initialize(ThreeLines(), videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.OnClosing();

        Assert.Equal(1, left.ResetCount);
        Assert.Equal(1, right.ResetCount);
    }

    [AvaloniaFact]
    public void SetSyncPoint_OffersTheWholeSubtitleWithItsHeader()
    {
        const string header = "[Script Info]\r\nPlayResX: 1280\r\n\r\n[V4+ Styles]\r\n";
        var preview = new FakePreviewSubtitle();
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), preview);
        var lines = ThreeLines();
        vm.Initialize(lines, lines[1], videoFileName: null, subtitleFileName: null, new VideoPreviewSubtitleContext(new AdvancedSubStationAlpha(), header, false), audioVisualizer: null);

        vm.RefreshPreviewSubtitle();

        var subtitle = preview.BuildOffered();
        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Two", subtitle.Paragraphs[1].Text);
        Assert.Equal(header, subtitle.Header);
    }

    [AvaloniaFact]
    public void SetSyncPoint_Closing_ReleasesTheTempSubtitleFile()
    {
        var preview = new FakePreviewSubtitle();
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), preview);
        var lines = ThreeLines();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        vm.OnClosing();

        Assert.Equal(1, preview.ResetCount);
    }
}

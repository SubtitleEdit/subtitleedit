using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

namespace UITests.Features.Sync.PointSync;

/// <summary>
/// Construction tests for the three point sync windows. Their layouts are built in code, so a bad
/// panel or a missing command only shows up when the window is actually constructed - which is
/// what these do. Both point sync windows gained a video-related button (issue #13341).
/// </summary>
public class PointSyncWindowTests : IDisposable
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

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static List<SubtitleLineViewModel> TwoLines()
        => new()
        {
            new() { StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(3) },
            new() { StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(7) },
        };

    private T Track<T>(T window) where T : Window
    {
        _windows.Add(window);
        return window;
    }

    /// <summary>
    /// Captions of every button in the window, visible or not. WithIconLeft repacks the content
    /// into an icon + text panel, so the caption is on a TextBlock inside the button rather than
    /// on Button.Content.
    /// </summary>
    private static IEnumerable<string> ButtonTexts(Window window)
        => window.GetLogicalDescendants()
            .OfType<Button>()
            .SelectMany(CaptionsOf)
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => t!);

    private static IEnumerable<string?> CaptionsOf(Button button)
        => button.GetLogicalDescendants().OfType<TextBlock>().Select(t => t.Text)
            .Append(button.Content as string);

    /// <summary>
    /// A hidden button is still in the logical tree, so presence says nothing about whether the
    /// user can see it - this is what the visibility assertions have to go through.
    /// </summary>
    private static Button ButtonWithText(Window window, string text)
        => window.GetLogicalDescendants()
            .OfType<Button>()
            .First(b => CaptionsOf(b).Contains(text));

    [AvaloniaFact]
    public void PointSyncWindow_Constructs()
    {
        var vm = new PointSyncViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        var lines = TwoLines();
        vm.Initialize(lines, 0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default, null);

        var window = Track(new PointSyncWindow(vm));

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_WithoutVideo_HidesThePlayerAndPlaybackButton()
    {
        // The videoless dialog is just the line picker and the sync point time code - the player,
        // the waveform and "Play 2 secs & back" have nothing to show or do (issue #13341).
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        var window = Track(new SetSyncPointWindow(vm));

        Assert.False(vm.IsVideoVisible);
        Assert.False(vm.VideoPlayerControl.IsVisible);
        Assert.False(vm.AudioVisualizer.IsVisible);
        Assert.False(ButtonWithText(window, Se.Language.Sync.PlayTwoSecondsAndBack).IsVisible);

        // Everything that still means something without a video stays.
        Assert.True(ButtonWithText(window, Se.Language.Sync.GoToSubPos).IsVisible);
        Assert.True(ButtonWithText(window, Se.Language.General.OpenVideoFile).IsVisible);
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_WithVideo_KeepsThePlayerAndPlaybackButton()
    {
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));
        var lines = TwoLines();

        // A path is enough for the layout decision; nothing opens it here (the open is posted to
        // the dispatcher and this test never pumps it).
        vm.Initialize(lines, lines[0], videoFileName: "/does/not/exist.mp4", subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        var window = Track(new SetSyncPointWindow(vm, WorkingVideoPlayer.MakeControl()));

        Assert.True(vm.IsVideoVisible);
        Assert.True(vm.VideoPlayerControl.IsVisible);
        Assert.True(ButtonWithText(window, Se.Language.Sync.PlayTwoSecondsAndBack).IsVisible);
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_WithoutVideo_IgnoresASavedWithVideoHeight()
    {
        // Both modes save their size under the same window name, so without the MaxHeight cap the
        // videoless dialog restores the with-video height and opens as a mostly blank window.
        var rememberBefore = Se.Settings.General.RememberPositionAndSize;
        var positionsBefore = Se.Settings.General.WindowPositions.ToList();
        try
        {
            Se.Settings.General.RememberPositionAndSize = true;
            Se.Settings.General.WindowPositions.Add(
                new SeWindowPosition(nameof(SetSyncPointWindow), false, false, 50, 50, 1100, 900));

            var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));
            var lines = TwoLines();
            vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

            var window = Track(new SetSyncPointWindow(vm));
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(window.Bounds.Height <= window.MaxHeight,
                $"height was {window.Bounds.Height}, cap {window.MaxHeight}");
            Assert.True(window.MaxHeight < 400, $"cap was {window.MaxHeight}");
        }
        finally
        {
            Se.Settings.General.RememberPositionAndSize = rememberBefore;
            Se.Settings.General.WindowPositions.Clear();
            Se.Settings.General.WindowPositions.AddRange(positionsBefore);
        }
    }

    [AvaloniaFact]
    public void PointSyncViaOtherWindow_HasBothSetSyncPointButtons()
    {
        // The other subtitle stays the usual source, and the video is the fallback for lines it
        // does not cover - so both buttons have to be there.
        var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.Initialize(TwoLines(), 0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        var window = Track(new PointSyncViaOtherWindow(vm));

        var texts = ButtonTexts(window).ToList();
        Assert.Contains(Se.Language.Sync.SetSyncPoint, texts);
        Assert.Contains(Se.Language.Sync.SetSyncPointViaVideo, texts);
    }

    private static List<SubtitleLineViewModel> ManyLines()
        => Enumerable.Range(0, 300)
            .Select(i => new SubtitleLineViewModel { StartTime = TimeSpan.FromSeconds(i * 4), EndTime = TimeSpan.FromSeconds(i * 4 + 2), Text = "Line " + i })
            .ToList();

    private static TableView LinesGrid(Window window)
        => window.GetLogicalDescendants().OfType<TableView>()
            .First(t => t.SelectedItem is SubtitleLineViewModel);

    [AvaloniaFact]
    public void PointSyncWindow_OpensAtTheMainWindowSelection()
    {
        // Retiming in several rounds reopens the dialog each time - it has to start where the
        // user is, not at line 1 (issue #15062).
        var vm = new PointSyncViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        var lines = ManyLines();
        vm.Initialize(lines, 200, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default, null);

        var window = Track(new PointSyncWindow(vm));
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Same(lines[200], vm.SelectedSubtitle);
        Assert.True(TableViewExtras.IsRowFullyVisible(LinesGrid(window), lines[200]));
    }

    [AvaloniaFact]
    public void PointSyncViaOtherWindow_OpensAtTheMainWindowSelection()
    {
        var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        var lines = ManyLines();
        vm.Initialize(lines, 200, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        var window = Track(new PointSyncViaOtherWindow(vm));
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Same(lines[200], vm.SelectedSubtitle);
        Assert.True(TableViewExtras.IsRowFullyVisible(LinesGrid(window), lines[200]));
    }

    [AvaloniaFact]
    public void PointSyncViewModel_OutOfRangeSelection_FallsBackIntoTheList()
    {
        var vm = new PointSyncViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        var lines = TwoLines();

        vm.Initialize(lines, -1, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default, null);

        Assert.Same(lines[0], vm.SelectedSubtitle);
    }

    /// <summary>
    /// A Blu-ray sup as the other subtitle: loaded with its time codes, its lines shown as images
    /// instead of text (issue #15911) - a sync point only needs the times, no OCR.
    /// </summary>
    [AvaloniaFact]
    public async Task PointSyncViaOther_BluRaySup_LoadsAsImageLines()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "point-sync-other-" + Guid.NewGuid() + ".sup");
        try
        {
            var sup = new List<byte>();
            for (var i = 0; i < 2; i++)
            {
                using var bitmap = new SKBitmap(64, 32);
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawRect(new SKRect(4, 4, 60, 28), new SKPaint { Color = SKColors.White });
                }

                var pic = new BluRaySupPicture
                {
                    StartTime = 1000 + i * 4000,
                    EndTime = 3000 + i * 4000,
                    Width = 720,
                    Height = 480,
                    CompositionNumber = i * 2,
                };
                sup.AddRange(BluRaySupPicture.CreateSupFrame(pic, bitmap, SKColors.White, 25, 10, 10, BluRayContentAlignment.BottomCenter));
            }

            File.WriteAllBytes(fileName, sup.ToArray());

            var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
            vm.Initialize(TwoLines(), 0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

            var result = await vm.TryLoadOtherFromContainer(fileName);

            Assert.NotNull(result);
            Assert.True(result.IsImageBased);
            Assert.Null(result.Error);
            Assert.Equal(2, result.Lines.Count);
            Assert.All(result.Lines, line => Assert.IsType<PointSyncOtherImageLine>(line));
            Assert.Equal(TimeSpan.FromSeconds(1), result.Lines[0].StartTime);
            Assert.Equal(TimeSpan.FromSeconds(5), result.Lines[1].StartTime);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public async Task PointSyncViaOther_SubtitleFile_IsNotAContainer()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "point-sync-other-" + Guid.NewGuid() + ".srt");
        try
        {
            File.WriteAllText(fileName, "1\n00:00:01,000 --> 00:00:02,000\nHello\n");
            var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));

            Assert.Null(await vm.TryLoadOtherFromContainer(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public void PointSyncViaOther_CtrlPlusMinus_ZoomsImagesOnlyWhenImageBased()
    {
        var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        var start = vm.ImageMaxHeight;

        vm.OnKeyDown(new KeyEventArgs { Key = Key.OemPlus, KeyModifiers = KeyModifiers.Control });
        Assert.Equal(start, vm.ImageMaxHeight);

        vm.IsOtherImageBased = true;
        vm.OnKeyDown(new KeyEventArgs { Key = Key.OemPlus, KeyModifiers = KeyModifiers.Control });
        Assert.True(vm.ImageMaxHeight > start);

        vm.OnKeyDown(new KeyEventArgs { Key = Key.OemMinus, KeyModifiers = KeyModifiers.Control });
        vm.OnKeyDown(new KeyEventArgs { Key = Key.OemMinus, KeyModifiers = KeyModifiers.Control });
        Assert.True(vm.ImageMaxHeight < start);
    }
}

using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// F9 preview: the drawing is rendered by libass (via ffmpeg) at canvas size and shown on the canvas.
/// </summary>
public class AssaDrawPreviewTests
{
    /// <summary>
    /// Points FfmpegHelper at an ffmpeg that can render ASSA (some builds, e.g. Homebrew ffmpeg 9,
    /// have no libass and so no "subtitles" filter). Returns false when none is available.
    /// </summary>
    private static bool EnsureFfmpegWithLibass()
    {
        var probe = new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader };
        probe.Paragraphs.Add(new Paragraph("{\\p1}m 0 0 l 10 0 10 10{\\p0}", 0, 1000));

        var candidates = new[] { FfmpegHelper.GetFfmpegLocation(), FfmpegHelper.GetSystemFfmpegPath(), "/opt/local/bin/ffmpeg", "/usr/local/bin/ffmpeg", "/usr/bin/ffmpeg" };
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrEmpty(candidate) || (candidate.Contains('/') && !File.Exists(candidate)))
            {
                continue;
            }

            FfmpegHelper.SetFfmpegPath(candidate);
            try
            {
                var fileName = FfmpegGenerator.GetScreenShotWithSubtitle(probe, 64, 64);
                if (fileName != null)
                {
                    File.Delete(fileName);
                    return true;
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // not installed (CI has no ffmpeg at all)
            }
        }

        return false;
    }

    private static bool WaitFor(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            System.Threading.Thread.Sleep(20);
        }

        return condition();
    }

    [AvaloniaFact]
    public void ShowPreview_RendersDrawingAtCanvasSize_AndClearsWithShapes()
    {
        var oldSePath = Se.Settings.General.FfmpegPath;
        var oldLibSePath = Configuration.Settings.General.FFmpegLocation;
        try
        {
            RunPreviewTest();
        }
        finally
        {
            Se.Settings.General.FfmpegPath = oldSePath;
            Configuration.Settings.General.FFmpegLocation = oldLibSePath;
        }
    }

    private static void RunPreviewTest()
    {
        if (!EnsureFfmpegWithLibass())
        {
            return; // no ffmpeg with libass on this machine
        }

        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var line = new SubtitleLineViewModel(new Paragraph("{\\p1}m 100 100 l 200 100 200 200 100 200{\\p0}", 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [line], 640, 360);
        var window = new AssaDrawWindow(vm);
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(vm.Canvas);

            vm.ShowPreview = true;

            Assert.True(WaitFor(() => vm.Canvas!.PreviewImage != null));
            Assert.True(vm.Canvas!.ShowPreview);
            var bitmap = Assert.IsType<Bitmap>(vm.Canvas.PreviewImage);
            Assert.Equal(640, bitmap.PixelSize.Width);
            Assert.Equal(360, bitmap.PixelSize.Height);

            // The dialog re-renders from a DispatcherTimer, which the headless platform does not tick
            vm.ClearAllCommand.Execute(null);
            vm.UpdatePreview();
            Assert.True(WaitFor(() => vm.Canvas!.PreviewImage == null));
        }
        finally
        {
            window.Close();
            DrawSettings.ShowPreview = false;
        }
    }
}

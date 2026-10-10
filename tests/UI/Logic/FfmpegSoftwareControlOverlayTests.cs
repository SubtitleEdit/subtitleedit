using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;
using System.Reflection;

namespace UITests.Logic;

/// <summary>
/// After the video was closed with a subtitle on screen, the overlay tick kept comparing the
/// (now empty) active lines with the lines last drawn - which Render never cleared on its "no
/// file" early return - so it repainted at 10 Hz forever.
/// </summary>
public class FfmpegSoftwareControlOverlayTests
{
    private static readonly FieldInfo OverlayLinesField =
        typeof(FfmpegSoftwareControl).GetField("_overlayLines", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [AvaloniaFact]
    public void Render_WithoutFile_ClearsLastDrawnLines()
    {
        var player = new FfmpegPlayer();
        var control = new FfmpegSoftwareControl(player);
        var window = new Window { Width = 320, Height = 180, Content = control };
        window.Show();
        try
        {
            OverlayLinesField.SetValue(control, new List<FfmpegPreviewSubtitle.Line>
            {
                new(0, 2, "Still on screen", false, false),
            });

            control.InvalidateVisual();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            using var frame = window.CaptureRenderedFrame();

            Assert.Empty((List<FfmpegPreviewSubtitle.Line>)OverlayLinesField.GetValue(control)!);
        }
        finally
        {
            window.Close();
        }
    }
}

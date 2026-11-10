using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// Blu-ray sup from the export window (issue #16714). The window makes every display set before
/// it opens the file, and lines can be in any order (an ASSA script is grouped by what the author
/// worked on) - the file still has to follow the timeline, with overlapping lines composed.
/// </summary>
public class ExportImageBasedBluRayOrderTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "exportbluray_order_" + Guid.NewGuid().ToString("N"));

    public ExportImageBasedBluRayOrderTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private const byte Pcs = 0x16;

    private static SubtitleLineViewModel Line(int number, string text, double start, double end) => new()
    {
        Number = number,
        Text = text,
        StartTime = TimeSpan.FromSeconds(start),
        EndTime = TimeSpan.FromSeconds(end),
    };

    private static List<(long Pts, byte Type, byte[] Payload)> ReadSegments(string fileName)
    {
        var sup = File.ReadAllBytes(fileName);
        var segments = new List<(long, byte, byte[])>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            Assert.Equal((byte)'P', sup[position]);
            Assert.Equal((byte)'G', sup[position + 1]);
            var pts = ((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5];
            var size = (sup[position + 11] << 8) + sup[position + 12];
            segments.Add((pts, sup[position + 10], sup.Skip(position + 13).Take(size).ToArray()));
            position += 13 + size;
        }

        Assert.Equal(sup.Length, position);
        return segments;
    }

    [AvaloniaFact]
    public async Task UnsortedOverlappingLines_FollowTheTimeline_AndShareTheScreen()
    {
        // A long sign written first, then the dialogue under it, then a line from earlier on.
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            Line(1, "{\\an8}Breaking news", 10, 20),
            Line(2, "First line", 11, 13),
            Line(3, "Second line", 14, 16),
            Line(4, "Before the sign", 2, 4),
        };

        var vm = new ExportImageBasedViewModel(new FileHelper(), new FolderHelper(), new WindowService(new NullServiceProvider()));
        vm.Initialize(new ExportHandlerBluRaySup(), subtitles, null, null);

        var fileName = Path.Combine(_dir, "out.sup");
        await vm.ExportRun(fileName, CancellationToken.None);

        var segments = ReadSegments(fileName);
        var pts = segments.Select(s => s.Pts).ToList();
        Assert.Equal(pts.OrderBy(p => p), pts);

        // The sign and a dialogue line on screen together: one display set with both.
        var objectCounts = segments.Where(s => s.Type == Pcs).Select(s => (int)s.Payload[10]).ToList();
        Assert.Contains(2, objectCounts);

        // The lines themselves are not reordered.
        Assert.Equal(new[] { 1, 2, 3, 4 }, subtitles.Select(s => s.Number));
    }
}

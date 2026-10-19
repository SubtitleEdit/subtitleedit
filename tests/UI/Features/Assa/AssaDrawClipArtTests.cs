using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// The colored clip art of the ASSA draw shape library.
/// </summary>
public class AssaDrawClipArtTests
{
    private static List<ShapeLibraryItem> ClipArt =>
        ShapeLibrary.BuiltIn.Where(i => i.Category == Se.Language.Assa.DrawCategoryClipArt).ToList();

    [Fact]
    public void ClipArt_KeepsItsColors_InAFewLayers()
    {
        Assert.Equal(84, ClipArt.Count);
        foreach (var item in ClipArt)
        {
            Assert.True(item.HasOwnColors, item.Name);
            Assert.NotEmpty(item.Template);

            // Each color is one ASSA line - keep it cheap
            var layers = item.Template.Select(s => s.Layer).Distinct().Count();
            Assert.InRange(layers, 1, 4);
            Assert.Contains(item.Template, s => s.ForeColor != Colors.White);
            Assert.Equal(layers, item.ToColoredGeometries().Count);
        }
    }

    [Fact]
    public void Insert_KeepsColors_OnLayersAboveTheDrawing()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var line = new SubtitleLineViewModel(new Paragraph("{\\p1}m 0 0 l 10 0 10 10{\\p0}", 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [line], 1920, 1080);
        vm.LayerColor = Colors.Green;
        vm.CurrentLibraryShape = ClipArt.First(i => i.Name == Se.Language.Assa.DrawClipArtSun);

        vm.InsertLibraryShape(new CanvasInsertEventArgs(100, 100, 400, 400, isClick: false));

        var inserted = vm.Shapes.Skip(1).ToList();
        Assert.DoesNotContain(inserted, s => s.ForeColor == Colors.Green);
        Assert.Equal([1, 2, 3], inserted.Select(s => s.Layer).Distinct().Order());
        Assert.Contains(inserted, s => s.ForeColor == Color.Parse("#FFC107"));
    }

    [Fact]
    public void TextArea_IsOnTheTopPartUnderItsCenter()
    {
        var burst = ClipArt.First(i => i.Name == Se.Language.Assa.DrawClipArtBamBurst);
        var shapes = burst.CreateShapes(0, 0, 1000, 1000, Colors.White, 0);

        // The yellow middle (painted last, top layer), so the text isn't covered by it
        var host = Assert.Single(shapes, s => s.TextFrame != null);
        Assert.Equal(Color.Parse("#FFEB3B"), host.ForeColor);
        Assert.Equal(shapes.Max(s => s.Layer), host.Layer);
    }
}

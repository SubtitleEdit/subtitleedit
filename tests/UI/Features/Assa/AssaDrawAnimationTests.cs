using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// In/out effects in ASSA draw: written as \move, \fad and \t around the layer's or group's anchor,
/// kept in a marker so the editor opens them again, and played on the canvas.
/// </summary>
public class AssaDrawAnimationTests
{
    private const string SquareA = "{\\p1}m 100 100 l 200 100 200 200 100 200{\\p0}";
    private const string SquareB = "{\\p1}m 300 100 l 400 100 400 200 300 200{\\p0}";
    private const string SquareC = "{\\p1}m 600 600 l 700 600 700 700 600 700{\\p0}";

    private static AssaDrawViewModel MakeViewModel(params string[] lines)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var rows = lines.Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, 0, 3000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, rows, 1920, 1080);
        return vm;
    }

    private static List<string> Points(AssaDrawViewModel vm) =>
        vm.Shapes.OrderBy(s => s.Layer).Select(s => s.ToAssa()).ToList();

    [AvaloniaFact]
    public void GroupAnimation_IsWrittenAroundTheGroupCenter_AndOpensAgain()
    {
        var vm = MakeViewModel(SquareA, SquareB, SquareC);
        Assert.Equal(3000, vm.LineDurationMs);
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        vm.AnimationIn = DrawAnimationEffect.Pop;
        vm.IsEditingOutEffect = true;
        vm.EditedEffect = DrawAnimationEffect.Fade;
        vm.EditedEffectMs = 500;

        Assert.Equal(DrawAnimationEffect.Fade, vm.AnimationOut);
        Assert.Equal(500, vm.AnimationOutMs);
        Assert.NotNull(vm.Shapes[0].Animation);
        Assert.NotNull(vm.Shapes[1].Animation);
        Assert.Null(vm.Shapes[2].Animation);

        var expectedPoints = Points(vm);
        var text = vm.GetAssaDrawFileText();
        vm.OkCommand.Execute(null);
        var lines = vm.ResultSubtitle.Paragraphs;

        // Both group lines anchored at the group's center (100..400 x 100..200 -> 250,150), coordinates relative to it
        Assert.StartsWith("{\\an7\\pos(250,150)\\fad(0,500)\\fscx0\\fscy0\\t(0,280,0.5,\\fscx110\\fscy110)\\t(280,400,\\fscx100\\fscy100)}{assadraw-anim c=250,150 in=Pop,400 out=Fade,500}", lines[0].Text);
        Assert.Contains("m -150 -50 l -50 -50 -50 50 -150 50", lines[0].Text);
        Assert.Contains("\\pos(250,150)", lines[1].Text);
        Assert.Contains("m 50 -50 l 150 -50 150 50 50 50", lines[1].Text);
        Assert.Equal(SquareC, lines[2].Text);
        Assert.All(lines, l => Assert.True(MainViewModel.IsAssaDrawingText(l.Text)));

        var loaded = MakeViewModel();
        loaded.LoadFromText(text);
        Assert.Equal(expectedPoints, Points(loaded));
        var animation = loaded.Shapes.First(s => s.Layer == 0).Animation;
        Assert.Equal(new DrawAnimation { In = DrawAnimationEffect.Pop, Out = DrawAnimationEffect.Fade, OutMs = 500 }, animation);
        Assert.Null(loaded.Shapes.First(s => s.Layer == 2).Animation);

        // Saving again writes the same file
        Assert.Equal(text, loaded.GetAssaDrawFileText());
    }

    [AvaloniaFact]
    public void SlideIn_UsesMove_AndASecondSlideOutFadesInstead()
    {
        var vm = MakeViewModel(SquareC);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationIn = DrawAnimationEffect.Left;
        vm.AnimationOut = DrawAnimationEffect.Bottom;

        Assert.True(vm.IsAnimationSlideConflict);
        vm.OkCommand.Execute(null);
        var line = vm.ResultSubtitle.Paragraphs[0].Text;

        // From just left of the frame (right edge 700 + 4) to the anchor 650,650, then a 400 ms fade out
        Assert.StartsWith("{\\an7\\move(-54,650,650,650,0,400)\\fad(0,400)}", line);
    }

    [AvaloniaFact]
    public void SlideOut_MovesAtTheEndOfTheLine()
    {
        var vm = MakeViewModel(SquareC);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationOut = DrawAnimationEffect.Right;
        vm.AnimationOutMs = 600;

        vm.OkCommand.Execute(null);

        Assert.StartsWith("{\\an7\\move(650,650,1974,650,2400,3000)}", vm.ResultSubtitle.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void TextInAnimatedShape_FollowsTheShape_AndOpensAgain()
    {
        var vm = MakeViewModel(SquareA);
        vm.SetSelection([vm.Shapes[0]]);
        vm.ShapeText = "Hi";
        vm.AnimationIn = DrawAnimationEffect.Spin;
        vm.AnimationOut = DrawAnimationEffect.Top;
        var text = vm.GetAssaDrawFileText();

        vm.OkCommand.Execute(null);
        var textLine = vm.ResultSubtitle.Paragraphs.Single(p => AssaDrawTextLine.IsTextLine(p.Text)).Text;
        Assert.Contains("\\move(150,150,150,-54,2600,3000)", textLine);
        Assert.DoesNotContain("\\pos(", textLine);
        Assert.Contains("{\\org(150,150)\\frz360\\fscx0\\fscy0\\t(0,400,0.5,\\frz0\\fscx100\\fscy100)}", textLine);

        // The out effect is timed for the line, so the same line length writes the same file
        var loaded = MakeViewModel();
        loaded.LineDurationMs = vm.LineDurationMs;
        loaded.LoadFromText(text);
        var shape = Assert.Single(loaded.Shapes);
        Assert.Equal("Hi", shape.Text);
        Assert.Equal("m 100 100 l 200 100 200 200 100 200", shape.ToAssa());
        Assert.Equal(DrawAnimationEffect.Spin, shape.Animation?.In);
        Assert.Equal(text, loaded.GetAssaDrawFileText());
    }

    [AvaloniaFact]
    public void ChoosingNone_RemovesTheAnimation()
    {
        var vm = MakeViewModel(SquareA);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationIn = DrawAnimationEffect.Zoom;
        vm.AnimationIn = DrawAnimationEffect.None;

        Assert.Null(vm.Shapes[0].Animation);
        vm.OkCommand.Execute(null);
        Assert.Equal(SquareA, vm.ResultSubtitle.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void Undo_RestoresThePreviousAnimation()
    {
        var vm = MakeViewModel(SquareA);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationIn = DrawAnimationEffect.Fade;

        vm.UndoCommand.Execute(null);

        Assert.Null(vm.Shapes[0].Animation);
    }

    [Fact]
    public void State_MatchesTheTags_AtTheEdges()
    {
        var animation = new DrawAnimation { In = DrawAnimationEffect.Zoom, Out = DrawAnimationEffect.Spin };
        var bounds = (100f, 100f, 200f, 200f);

        Assert.Equal(0, animation.GetState(0, 2000, bounds, 1920, 1080).Scale);
        Assert.Equal(DrawAnimationState.Rest, animation.GetState(1000, 2000, bounds, 1920, 1080));
        var end = animation.GetState(2000, 2000, bounds, 1920, 1080);
        Assert.Equal(0, end.Scale);
        Assert.Equal(360, end.Rotation);

        // A line shorter than both effects shares it between them
        Assert.Equal((150, 150), animation.GetDurations(300));
    }
}

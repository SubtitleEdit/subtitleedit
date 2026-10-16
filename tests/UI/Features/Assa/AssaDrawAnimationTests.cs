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
    public void SlideInAndOut_MovesTwice_TheSlideOutTurnsAroundAFarOrigin()
    {
        var vm = MakeViewModel(SquareC);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationIn = DrawAnimationEffect.Left;
        vm.AnimationOut = DrawAnimationEffect.Bottom;

        vm.OkCommand.Execute(null);
        var line = vm.ResultSubtitle.Paragraphs[0].Text;

        // In: from just left of the frame (right edge 700 + 4) to the anchor 650,650
        Assert.StartsWith("{\\an7\\move(-54,650,650,650,0,400)\\org(", line);

        // Out: turning around the \org moves the anchor just under the frame (top 600 -> 1084 + 4 - 600 = 484 down)
        var match = System.Text.RegularExpressions.Regex.Match(line, @"\\org\(([-\d.]+),([-\d.]+)\)\\t\(2600,3000,2,\\frz([-\d.]+)\)");
        Assert.True(match.Success, line);
        var (x, y) = TurnLikeLibass(650, 650, double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
            double.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture),
            double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(650, x, 0.5);
        Assert.Equal(1134, y, 0.5);

        // The canvas plays the same: eased like the other out effects
        var end = vm.GetCanvasAnimationTransform(vm.Shapes[0], 3000).State;
        Assert.Equal(484, end.Dy, 0.01);
    }

    /// <summary>
    /// libass \frz: counter-clockwise on screen (y down) around the \org.
    /// </summary>
    private static (double X, double Y) TurnLikeLibass(double x, double y, double originX, double originY, double frz)
    {
        var radians = frz * Math.PI / 180;
        var vx = x - originX;
        var vy = y - originY;
        return (originX + vx * Math.Cos(radians) + vy * Math.Sin(radians), originY - vx * Math.Sin(radians) + vy * Math.Cos(radians));
    }

    [AvaloniaFact]
    public void TurnedText_InASpinningShape_StillShowsWhereItWas()
    {
        var vm = MakeViewModel(SquareA, SquareB);
        vm.SetSelection([vm.Shapes[1]]);
        vm.ShapeText = "Hi";
        vm.Shapes[1].SetTextBounds(310, 110, 390, 190, 30);
        vm.SetSelection([vm.Shapes[0], vm.Shapes[1]]);
        vm.GroupShapesCommand.Execute(null);
        vm.AnimationIn = DrawAnimationEffect.Spin;

        vm.OkCommand.Execute(null);
        var textLine = vm.ResultSubtitle.Paragraphs.Single(p => AssaDrawTextLine.IsTextLine(p.Text)).Text;

        // The text turns -30 (its own angle) around the group's center 250,150, so it is written
        // where that turn brings it back to its center 350,150
        Assert.Contains("\\org(250,150)\\frz330", textLine);
        var pos = System.Text.RegularExpressions.Regex.Match(textLine, @"\\pos\(([-\d.]+),([-\d.]+)\)");
        var (x, y) = TurnLikeLibass(double.Parse(pos.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
            double.Parse(pos.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), 250, 150, -30);
        Assert.Equal(350, x, 0.05);
        Assert.Equal(150, y, 0.05);
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

    [AvaloniaFact]
    public void Loop_IsOneTPerCycle_FittedBetweenInAndOut_AndOpensAgain()
    {
        var vm = MakeViewModel(SquareA);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationIn = DrawAnimationEffect.Fade;
        vm.AnimationOut = DrawAnimationEffect.Spin;
        vm.AnimationLoop = DrawLoopEffect.Spin;
        vm.AnimationLoopMs = 1000;
        var text = vm.GetAssaDrawFileText();

        vm.OkCommand.Execute(null);
        var line = vm.ResultSubtitle.Paragraphs[0].Text;

        // 400-2600 ms between the effects: two whole turns of 1100 ms, then the spin out goes on from there
        Assert.Contains("\\t(400,1500,\\frz-360)\\t(1500,2600,\\frz-720)\\t(2600,3000,2,\\frz-1080\\fscx0\\fscy0)", line);
        Assert.Contains("loop=Spin,1000}", line);

        var loaded = MakeViewModel();
        loaded.LineDurationMs = vm.LineDurationMs;
        loaded.LoadFromText(text);
        Assert.Equal(DrawLoopEffect.Spin, loaded.Shapes[0].Animation?.Loop);
        Assert.Equal(text, loaded.GetAssaDrawFileText());
    }

    [AvaloniaFact]
    public void Loop_TurningAroundAnotherOrigin_IsLeftOut()
    {
        var vm = MakeViewModel(SquareA);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationIn = DrawAnimationEffect.Spin;
        vm.AnimationLoop = DrawLoopEffect.Swing;

        Assert.True(vm.IsLoopBlocked);
        vm.OkCommand.Execute(null);
        Assert.DoesNotContain("\\frz14", vm.ResultSubtitle.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void Swing_TurnsAroundTheTopEdge_BounceAroundAFarOrigin()
    {
        var vm = MakeViewModel(SquareA, SquareC);
        vm.SetSelection([vm.Shapes[0]]);
        vm.AnimationLoop = DrawLoopEffect.Swing;
        vm.SetSelection([vm.Shapes[1]]);
        vm.AnimationLoop = DrawLoopEffect.Bounce;

        vm.OkCommand.Execute(null);
        var lines = vm.ResultSubtitle.Paragraphs;

        Assert.StartsWith("{\\an7\\pos(150,150)\\org(150,100)\\t(0,250,0.5,\\frz14)", lines[0].Text);
        Assert.StartsWith("{\\an7\\pos(650,650)\\org(-999350,650)\\t(0,500,0.5,\\frz0.0005", lines[1].Text);

        // The canvas: the swing's middle goes out to the side, the bounce goes up a tenth of the height
        Assert.Equal(50 * Math.Sin(14 * Math.PI / 180), vm.GetCanvasAnimationTransform(vm.Shapes[0], 250).State.Dx, 0.5);
        Assert.Equal(-10, vm.GetCanvasAnimationTransform(vm.Shapes[1], 500).State.Dy, 0.01);
    }

    [AvaloniaFact]
    public void AnimatedClipArt_ComesWithItsLoop_OnlyOnItsAnimatedPart()
    {
        var vm = MakeViewModel();
        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Nikse.SubtitleEdit.Logic.Config.Se.Language.Assa.DrawClipArtRec);
        vm.InsertLibraryShape(new CanvasInsertEventArgs(100, 100, 400, 200, isClick: false));

        var dot = Assert.Single(vm.Shapes, s => s.Animation != null);
        Assert.Equal(DrawLoopEffect.Blink, dot.Animation!.Loop);
        Assert.Equal(Avalonia.Media.Color.Parse("#E53935"), dot.ForeColor);

        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Nikse.SubtitleEdit.Logic.Config.Se.Language.Assa.DrawClipArtLoadingSpinner);
        var before = vm.Shapes.Count;
        vm.InsertLibraryShape(new CanvasInsertEventArgs(600, 100, 200, 200, isClick: false));
        Assert.All(vm.Shapes.Skip(before), s => Assert.Equal(DrawLoopEffect.Spin, s.Animation?.Loop));
    }

    [Fact]
    public void Flip_TurnsHalfWayEachCycle_ThenPauses()
    {
        var animation = new DrawAnimation { Loop = DrawLoopEffect.Flip, LoopMs = 1000 };
        var bounds = (100f, 100f, 200f, 200f);

        // A half turn in the first 35% of the cycle (clockwise on screen), then it stays
        Assert.Equal("\\t(0,350,0.5,\\frz-180)\\t(1000,1350,0.5,\\frz-360)", animation.GetEffectTags(2000, 0, bounds, 1920, 1080));
        Assert.Equal(180, animation.GetState(700, 2000, bounds, 1920, 1080).Rotation, 0.01);
        Assert.Equal(360, animation.GetState(1500, 2000, bounds, 1920, 1080).Rotation, 0.01);
    }

    [Fact]
    public void Blink_IsOffInTheSecondHalfOfEachCycle()
    {
        var animation = new DrawAnimation { Loop = DrawLoopEffect.Blink, LoopMs = 1000 };
        var bounds = (100f, 100f, 200f, 200f);

        Assert.Equal(1, animation.GetState(250, 3000, bounds, 1920, 1080).Scale);
        Assert.Equal(0, animation.GetState(750, 3000, bounds, 1920, 1080).Scale);
        Assert.Equal(1, animation.GetState(1250, 3000, bounds, 1920, 1080).Scale);
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

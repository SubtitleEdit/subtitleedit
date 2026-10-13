using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// ASSA draw outlines, text boxes and text in shapes (speech bubbles): written as ASSA and read back.
/// </summary>
public class AssaDrawTextTests
{
    private static AssaDrawViewModel MakeViewModel(params Paragraph[] paragraphs)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var lines = paragraphs.Select(p => new SubtitleLineViewModel(p, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, lines, 1920, 1080);
        return vm;
    }

    private static Subtitle Ok(AssaDrawViewModel vm)
    {
        vm.OkCommand.Execute(null);
        return vm.ResultSubtitle;
    }

    /// <summary>
    /// Opens the dialog's result again, like selecting the written rows and opening ASSA draw.
    /// </summary>
    private static AssaDrawViewModel Reopen(Subtitle result)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var lines = result.Paragraphs.Select(p => new SubtitleLineViewModel(p, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(result, lines, 1920, 1080);
        return vm;
    }

    [Fact]
    public void Outline_IsWrittenOnTheLayerLine_AndReadBack()
    {
        var vm = MakeViewModel(new Paragraph("{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}", 0, 2000) { Extra = "Default" });
        vm.SelectShape(vm.Shapes[0]);
        vm.ShapeOutlineWidth = 3.5f;
        vm.ShapeOutlineColor = Color.FromRgb(0x11, 0x22, 0x33);

        var line = Assert.Single(Ok(vm).Paragraphs);
        Assert.Contains("\\bord3.5\\3c&H332211&\\p1}", line.Text);
        Assert.Contains("\\bord3.5", vm.AssaDrawingCode);

        var reopened = Reopen(vm.ResultSubtitle);
        Assert.Equal(3.5f, reopened.Shapes[0].OutlineWidth);
        Assert.Equal(Color.FromRgb(0x11, 0x22, 0x33), reopened.Shapes[0].OutlineColor);
    }

    [Fact]
    public void NoOutline_WritesNoBordTag()
    {
        var vm = MakeViewModel(new Paragraph("{\\p1}m 0 0 l 100 0 100 100{\\p0}", 0, 2000) { Extra = "Default" });
        var line = Assert.Single(Ok(vm).Paragraphs);
        Assert.DoesNotContain("\\bord", line.Text);
    }

    [Fact]
    public void OutlineOfALayer_AppliesToAllItsShapes()
    {
        var vm = MakeViewModel(new Paragraph("{\\p1}m 0 0 l 100 0 100 100 m 200 0 l 300 0 300 100{\\p0}", 0, 2000) { Extra = "Default" });
        Assert.Equal(2, vm.Shapes.Count);
        vm.SelectShape(vm.Shapes[1]);
        vm.ShapeOutlineWidth = 2;

        Assert.All(vm.Shapes, s => Assert.Equal(2, s.OutlineWidth));
    }

    [Fact]
    public void TextBox_IsWrittenAsCenteredTextLine_AndReopensAsTextBox()
    {
        var vm = MakeViewModel();
        vm.LayerColor = Colors.Yellow;
        var textBox = vm.InsertTextBox(new CanvasInsertEventArgs(100, 200, 800, 200, isClick: false));
        Assert.True(textBox.IsTextBox);
        Assert.Same(textBox, vm.SelectedTreeItem?.Shape);
        Assert.Equal(Se.Language.Assa.DrawDefaultText, vm.ShapeText);

        vm.ShapeText = "Hello\nworld";
        vm.TextBold = true;
        vm.TextShrinkToFit = false;
        vm.TextFontSize = 40;

        var result = Ok(vm);
        var line = Assert.Single(result.Paragraphs);
        Assert.Equal(AssaDrawTextLine.StyleName, line.Extra);
        Assert.StartsWith("{\\an5\\pos(500,300)\\q2\\fnArial\\fs40\\b1\\c&H00FFFF&\\bord2\\3c&H000000&\\shad0}", line.Text);
        Assert.EndsWith("}Hello\\Nworld", line.Text);
        Assert.Contains(AdvancedSubStationAlpha.GetSsaStylesFromHeader(result.Header), s => s.Name == AssaDrawTextLine.StyleName);
        Assert.True(MainViewModel.IsAssaDrawingText(line.Text));

        var reopened = Reopen(result);
        var shape = Assert.Single(reopened.Shapes);
        Assert.True(shape.IsTextBox);
        Assert.Equal("Hello\nworld", shape.Text);
        Assert.True(shape.FontBold);
        Assert.False(shape.ShrinkToFit);
        Assert.Equal(40, shape.FontSize);
        Assert.Equal(Colors.Yellow, shape.TextColor);
        Assert.Equal(2, shape.OutlineWidth);
        var (left, top, right, bottom) = shape.GetTextBounds();
        Assert.Equal(100, left, 1);
        Assert.Equal(200, top, 1);
        Assert.Equal(900, right, 1);
        Assert.Equal(400, bottom, 1);
    }

    [Fact]
    public void TextBox_DrawsNothing_SoTheResultHasOnlyTheTextLine()
    {
        var vm = MakeViewModel();
        vm.InsertTextBox(new CanvasInsertEventArgs(960, 540, 0, 0, isClick: true));

        var result = Ok(vm);
        Assert.DoesNotContain(result.Paragraphs, p => p.Text.Contains("\\p1"));
        Assert.DoesNotContain(AdvancedSubStationAlpha.GetSsaStylesFromHeader(result.Header), s => s.Name.StartsWith("AssaDraw", StringComparison.Ordinal) && s.Name != AssaDrawTextLine.StyleName);
    }

    [Fact]
    public void SpeechBubble_TextGoesInTheBody_AfterTheDrawing_OnTheSameLayer()
    {
        var vm = MakeViewModel();
        vm.LayerColor = Colors.White;
        vm.CurrentLibraryShape = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeSpeechBubble);
        vm.InsertLibraryShape(new CanvasInsertEventArgs(100, 100, 1000, 940, isClick: false));
        var bubble = Assert.Single(vm.Shapes);
        Assert.Same(bubble, vm.SelectedTreeItem?.Shape);

        vm.ShapeText = "Hi there";
        Assert.Equal(Colors.Black, bubble.TextColor);

        // The body is the top 70 of the 94 high bubble: text area 6..64 -> 160..740
        var (_, textTop, _, textBottom) = bubble.GetTextBounds();
        Assert.Equal(160, textTop, 0);
        Assert.Equal(740, textBottom, 0);

        var result = Ok(vm);
        Assert.Equal(2, result.Paragraphs.Count);
        Assert.Contains("\\p1", result.Paragraphs[0].Text);
        Assert.True(AssaDrawTextLine.IsTextLine(result.Paragraphs[1].Text));
        Assert.Equal(result.Paragraphs[0].Layer, result.Paragraphs[1].Layer);
        Assert.Contains("\\pos(600,450)", result.Paragraphs[1].Text);
        Assert.Contains("\\bord0", result.Paragraphs[1].Text);

        // Opened again, the text is the bubble's text, not a separate text box
        var reopened = Reopen(result);
        var shape = Assert.Single(reopened.Shapes);
        Assert.False(shape.IsTextBox);
        Assert.Equal("Hi there", shape.Text);
        Assert.Equal(160, shape.GetTextBounds().Top, 0);
    }

    [Fact]
    public void ThoughtBubble_TextAreaIsOnTheBigPart()
    {
        var item = ShapeLibrary.BuiltIn.First(i => i.Name == Se.Language.Assa.DrawShapeThoughtBubble);
        var shapes = item.CreateShapes(0, 0, 1000, 1000, Colors.White, 0);
        var host = shapes.OrderByDescending(s => { var b = s.GetBounds(); return (b.Right - b.Left) * (b.Bottom - b.Top); }).First();
        Assert.NotNull(host.TextFrame);
        Assert.All(shapes.Where(s => s != host), s => Assert.Null(s.TextFrame));
    }

    [Fact]
    public void ShrinkToFit_MakesLongTextSmaller_AndWraps()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 40));
        var big = AssaTextLayout.Layout(text, "Arial", 80, false, 400, 200, shrinkToFit: false);
        var fitted = AssaTextLayout.Layout(text, "Arial", 80, false, 400, 200, shrinkToFit: true);

        Assert.Equal(80, big.FontSize);
        Assert.True(big.Lines.Count * big.FontSize > 200);
        Assert.True(fitted.FontSize < 80);
        Assert.True(fitted.Lines.Count > 1);
        Assert.True(fitted.Lines.Count * fitted.FontSize <= 200.5f);
    }

    [Fact]
    public void WrappedLines_UseSoftBreaks_UserBreaksStayHard_AndReadBackAsTyped()
    {
        var shape = new DrawShape { IsTextBox = true, ShrinkToFit = false, FontSize = 40 };
        shape.AddPoint(DrawCoordinateType.Line, 0, 0, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 300, 0, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 300, 400, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 0, 400, Colors.White);
        shape.SetTextBounds(0, 0, 300, 400);
        shape.Text = "one two three four five six seven eight\nnine";

        var line = AssaDrawTextLine.Format(shape);
        Assert.Contains("\\n", line);
        Assert.EndsWith("\\Nnine", line);

        Assert.True(AssaDrawTextLine.TryParse(line, out var parsed));
        Assert.Equal(shape.Text, parsed.Text);
    }

    [Fact]
    public void BracesAndBackslashes_InText_DoNotBecomeTags()
    {
        var shape = new DrawShape { IsTextBox = true, Text = "{\\i1}x" };
        shape.AddPoint(DrawCoordinateType.Line, 0, 0, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 2000, 0, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 2000, 400, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 0, 400, Colors.White);
        shape.SetTextBounds(0, 0, 2000, 400);

        var line = AssaDrawTextLine.Format(shape);
        Assert.DoesNotContain("{\\i1}", line);
        Assert.True(AssaDrawTextLine.TryParse(line, out var parsed));
        Assert.Equal("(\\i1)x", parsed.Text);
    }

    [Fact]
    public void MovedInTheGrid_PosWinsOverTheStoredTextArea()
    {
        const string text = "{\\an5\\pos(1000,600)\\q2\\fnArial\\fs40\\c&HFFFFFF&\\bord0\\shad0}{assadraw-text box=100,200,300,400}Hi";
        Assert.True(AssaDrawTextLine.TryParse(text, out var parsed));
        Assert.Equal((900f, 500f, 1100f, 700f), parsed.Box);
    }

    private static DrawShape MakeBubbleLikeShape()
    {
        // 100 x 100, text area in the top part (the "body")
        var shape = new DrawShape { Text = "Hi" };
        shape.AddPoint(DrawCoordinateType.Line, 0, 0, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 100, 0, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 100, 100, Colors.White);
        shape.AddPoint(DrawCoordinateType.Line, 0, 100, Colors.White);
        shape.SetTextBounds(10, 0, 90, 70);
        return shape;
    }

    [Fact]
    public void Flip_MirrorsTheTextArea_ButKeepsTheTextReadable()
    {
        var shape = MakeBubbleLikeShape();

        shape.FlipVertical();
        Assert.Equal(new DrawTextFrame(50, 65, 80, 70, 0), shape.TextFrame);

        shape.FlipHorizontal();
        Assert.Equal(new DrawTextFrame(50, 65, 80, 70, 0), shape.TextFrame);
    }

    [Fact]
    public void Rotate_TurnsTheTextAreaAndTheText()
    {
        var shape = MakeBubbleLikeShape();

        shape.Rotate(30);

        var frame = shape.TextFrame!.Value;
        Assert.Equal(30, frame.Angle, 1);
        Assert.Equal(80, frame.Width, 1);
        Assert.Equal(70, frame.Height, 1);

        // The area's center turned around the shape's center (50, 50): (50, 35) -> 15 up, rotated 30°
        Assert.Equal(50 + 15 * MathF.Sin(MathF.PI / 6), frame.CenterX, 1);
        Assert.Equal(50 - 15 * MathF.Cos(MathF.PI / 6), frame.CenterY, 1);
    }

    [Fact]
    public void RotatedText_IsWrittenWithFrz_AndReadBack()
    {
        var vm = MakeViewModel();
        var textBox = vm.InsertTextBox(new CanvasInsertEventArgs(100, 200, 800, 200, isClick: false));
        textBox.Rotate(-45);

        var result = Ok(vm);
        var line = Assert.Single(result.Paragraphs);
        Assert.Contains("\\pos(500,300)", line.Text);
        Assert.Contains("\\frz45\\", line.Text);

        var shape = Assert.Single(Reopen(result).Shapes);
        var frame = shape.TextFrame!.Value;
        Assert.Equal(-45, frame.Angle, 1);
        Assert.Equal(800, frame.Width, 1);
        Assert.Equal(200, frame.Height, 1);
    }

    [Fact]
    public void ResizeAndNudge_MoveTheTextAreaToo()
    {
        var vm = MakeViewModel();
        var textBox = vm.InsertTextBox(new CanvasInsertEventArgs(100, 200, 800, 200, isClick: false));

        vm.ShapeWidth = 400;
        Assert.Equal((100f, 200f, 500f, 400f), textBox.GetTextBounds());

        vm.ShapeX = 0;
        Assert.Equal((0f, 200f, 400f, 400f), textBox.GetTextBounds());
    }

    [Fact]
    public void TextInAPlainShape_IsSupportedToo()
    {
        var vm = MakeViewModel(new Paragraph("{\\p1}m 0 0 l 400 0 400 200 0 200{\\p0}", 0, 2000) { Extra = "Default" });
        vm.SelectShape(vm.Shapes[0]);
        Assert.True(vm.CanEditText);
        vm.ShapeText = "Sign";

        var result = Ok(vm);
        Assert.Equal(2, result.Paragraphs.Count);
        Assert.Contains("\\pos(200,100)", result.Paragraphs[1].Text);
    }

    [Fact]
    public void EraserShape_CantHaveText()
    {
        var vm = MakeViewModel(new Paragraph("{\\iclip(m 0 0 l 100 0 100 100)}", 0, 2000) { Extra = "Default" });
        vm.SelectShape(vm.Shapes[0]);
        Assert.False(vm.CanEditText);
    }

    [Fact]
    public void EveryBubble_HasATextAreaInsideIt()
    {
        foreach (var item in ShapeLibrary.BuiltIn.Where(i => i.Category == Se.Language.Assa.DrawCategorySpeechBubbles))
        {
            Assert.NotNull(item.TextArea);
            var shapes = item.CreateShapes(0, 0, 1000, 1000, Colors.White, 0);
            var host = Assert.Single(shapes, s => s.TextFrame != null);
            var frame = host.TextFrame!.Value;
            Assert.True(frame.Width > 300 && frame.Height > 150, item.Name);
            Assert.True(host.HitTest(frame.CenterX, frame.CenterY, 0), item.Name);
        }
    }

    [Fact]
    public void HasChanges_IsFalseWhenOpened_TrueAfterAnEdit_FalseAfterUndo()
    {
        var vm = MakeViewModel(new Paragraph("{\\p1}m 0 0 l 100 0 100 100{\\p0}", 0, 2000) { Extra = "Default" });
        vm.Initialize();
        Assert.False(vm.HasChanges);

        vm.InsertTextBox(new CanvasInsertEventArgs(960, 540, 0, 0, isClick: true));
        Assert.True(vm.HasChanges);

        vm.UndoCommand.Execute(null);
        Assert.False(vm.HasChanges);
    }
}

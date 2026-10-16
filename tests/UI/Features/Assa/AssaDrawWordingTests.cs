using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// ASSA draw texts: tool shortcuts shown once, translated tool names and no hard-coded English.
/// </summary>
public class AssaDrawWordingTests
{
    private static AssaDrawViewModel MakeViewModel(params string[] lines)
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var rows = lines.Select((t, i) => new SubtitleLineViewModel(new Paragraph(t, 0, 2000) { Extra = "Default", Layer = i }, new AdvancedSubStationAlpha())).ToList();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, rows, 1920, 1080);
        return vm;
    }

    [AvaloniaFact]
    public void ToolButtons_ShowEachShortcutOnce()
    {
        var vm = MakeViewModel();
        var window = new AssaDrawWindow(vm) { Width = 1200, Height = 800 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var names = window.GetVisualDescendants().OfType<Button>()
                .Select(b => AutomationProperties.GetName(b) ?? string.Empty)
                .ToList();

            Assert.Contains(Se.Language.Assa.DrawLineTool + " (L, F4)", names);
            Assert.Contains(Se.Language.Assa.DrawBezierTool + " (B, F5)", names);
            Assert.Contains(Se.Language.Assa.DrawRectangleTool + " (R, F6)", names);
            Assert.Contains(Se.Language.Assa.DrawCircleTool + " (C, F7)", names);
            Assert.Contains(Se.Language.Assa.DrawSelectTool + " (V)", names);
            Assert.DoesNotContain(names, n => n.Contains(") ("));
            Assert.Contains(Se.Language.Assa.DrawOpenDrawing, names);
            Assert.Contains(Se.Language.Assa.DrawSaveDrawing, names);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void ToolName_IsTranslatedName_NotEnumName()
    {
        Assert.Equal(Se.Language.Assa.DrawColorPickerToolName, AssaDrawWindow.GetToolName(DrawingTool.ColorPicker));
        Assert.Equal(Se.Language.Assa.DrawLineTool, AssaDrawWindow.GetToolName(DrawingTool.Line));
        Assert.Equal("Select and move shapes", Se.Language.Assa.DrawSelectTool);
    }

    [Fact]
    public void ToolName_DropsShortcutOfOlderTranslations()
    {
        var old = Se.Language.Assa.DrawRectangleTool;
        try
        {
            Se.Language.Assa.DrawRectangleTool = "Rechteck (F6)";
            Assert.Equal("Rechteck", AssaDrawWindow.GetToolName(DrawingTool.Rectangle));
        }
        finally
        {
            Se.Language.Assa.DrawRectangleTool = old;
        }
    }

    [AvaloniaFact]
    public void TreeAndSelectionTexts_ComeFromLanguage()
    {
        var vm = MakeViewModel("{\\p1}m 100 100 l 200 100 200 200{\\p0}", "{\\iclip(m 0 0 l 10 0 10 10)}");

        Assert.Equal(string.Format(Se.Language.Assa.DrawShapeXDraw, 1), vm.ShapeTreeItems[0].Children[0].Name);
        Assert.Equal(string.Format(Se.Language.Assa.DrawShapeXErase, 1), vm.ShapeTreeItems[1].Children[0].Name);

        vm.SelectShape(vm.Shapes[0]);
        Assert.EndsWith(string.Format(Se.Language.Assa.DrawXPoints, 3), vm.SelectionText);

        vm.SelectAllShapesCommand.Execute(null);
        vm.DuplicateShapeCommand.Execute(null);
        Assert.Equal(string.Format(Se.Language.Assa.DrawXShapes, 2), vm.SelectionText);

        Assert.Equal(string.Format(Se.Language.Assa.DrawPositionXY, 0, 0), vm.PositionText);
    }
}

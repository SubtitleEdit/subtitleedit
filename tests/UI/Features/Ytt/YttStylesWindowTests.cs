using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ytt;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Ytt;

/// <summary>
/// Drives the YouTube timed text styles window headless, built like in production, so the
/// bindings and the header rewrite are exercised.
/// </summary>
public class YttStylesWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        YttPenLookup.Clear();
    }

    private static Subtitle MakeSubtitle()
    {
        var head = new YttHead();
        head.Pens.Add(new YttPen { Id = 0 });
        head.Pens.Add(new YttPen { Id = 1, Italic = true, ForeColor = "#FF0000", ForeOpacity = 254 });
        head.Pens.Add(new YttPen { Id = 2, Size = 200, EdgeType = 3 });
        head.WindowPositions.Add(new YttWindowPosition { Id = 1, AnchorPoint = 1, Horizontal = 50, Vertical = 0 });
        head.WindowStyles.Add(new YttWindowStyle { Id = 1, Justify = 2 });
        var subtitle = new Subtitle { Header = head.ToHeader() };
        subtitle.Paragraphs.Add(new Paragraph("Hi <s p=\"1\">there</s>", 1000, 2000) { Region = "1" });
        subtitle.Paragraphs.Add(new Paragraph("Line pen", 3000, 4000) { Style = "2", Effect = "1" });
        return subtitle;
    }

    private YttStylesViewModel ShowWindow(Subtitle subtitle)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var vm = services.BuildServiceProvider().GetRequiredService<YttStylesViewModel>();
        var window = new YttStylesWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize(subtitle, "test.ytt", null);
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    [AvaloniaFact]
    public void LoadsEntriesWithoutPlaceholderAndCountsUsages()
    {
        var vm = ShowWindow(MakeSubtitle());

        Assert.Equal(new[] { 1, 2 }, vm.Pens.Select(p => p.Id));
        Assert.Equal(1, vm.Pens[0].UsageCount);
        Assert.Equal(1, vm.Pens[1].UsageCount);
        Assert.Single(vm.Positions);
        Assert.Equal(1, vm.Positions[0].UsageCount);
        Assert.Single(vm.WindowStyles);
        Assert.Equal(1, vm.WindowStyles[0].UsageCount);
        Assert.True(vm.Pens[0].Italic);
        Assert.Equal(Color.Parse("#FF0000"), vm.Pens[0].ForeColor);
        Assert.Equal("Shown at 125% on YouTube", vm.Pens[1].RealSizeText);
    }

    [AvaloniaFact]
    public void OkWritesEditsNewEntriesAndKeepsPlaceholder()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.SelectedPen = vm.Pens[0];
        vm.Pens[0].Bold = true;
        vm.SelectedTabIndex = YttStylesViewModel.TabPens;
        vm.NewCommand.Execute(null);
        vm.SelectedTabIndex = YttStylesViewModel.TabPositions;
        vm.Positions[0].AnchorPointIndex = 8;
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        var head = YttHead.Parse(vm.Header);
        Assert.NotNull(head.GetPen(0));
        Assert.True(head.GetPen(1)!.Bold);
        Assert.True(head.GetPen(1)!.Italic);
        Assert.NotNull(head.GetPen(3)); // the new pen gets the next free id
        Assert.Equal(8, head.GetWindowPosition(1)!.AnchorPoint);
    }

    [AvaloniaFact]
    public void DeletedIdIsNotReused()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.SelectedTabIndex = YttStylesViewModel.TabPens;
        vm.SelectedPen = vm.Pens[1];
        vm.DeleteCommand.Execute(null);
        vm.NewCommand.Execute(null);

        Assert.Equal(new[] { 1, 3 }, vm.Pens.Select(p => p.Id));
    }

    [AvaloniaFact]
    public void PreviewRendersAtReferenceSize()
    {
        using var bitmap = YttStylesViewModel.RenderPreview(new YttPen { EdgeType = 3, Size = 200 }, new YttWindowPosition(), "Preview");

        Assert.Equal(1280, bitmap.Width);
        Assert.Equal(720, bitmap.Height);
    }

    [AvaloniaFact]
    public void TokenizerColorsPenIdWithPenColor()
    {
        YttPenLookup.SetHeader(MakeSubtitle().Header);
        const string text = "Hi <s p=\"1\">there</s>";

        var ranges = SubtitleSyntaxTokenizer.Tokenize(text);
        var start = text.IndexOf("1\"", StringComparison.Ordinal);
        var range = ranges.Single(r => r.Start == start);

        Assert.Equal(Color.FromRgb(255, 0, 0), range.Color);
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Layout 14 is the quality control layout: the file's errors with Fix buttons and the bookmarks
/// beside the grid, override tag chips above the text box, and buttons to play or speak the line.
/// </summary>
public class QcLayoutTests
{
    [Theory]
    [InlineData("{\\an8\\pos(640,80)}Text", new[] { "\\an8", "\\pos(640,80)" })]
    [InlineData("{\\t(0,500,\\fs40\\c&H0000FF&)\\blur2}Text", new[] { "\\t(0,500,\\fs40\\c&H0000FF&)", "\\blur2" })]
    [InlineData("{comment\\i1}Text", new[] { "\\i1" })]
    [InlineData("{just a comment}Text", new string[0])]
    [InlineData("Text {\\i1}italic", new string[0])]
    [InlineData("{\\an8 no end", new string[0])]
    [InlineData("", new string[0])]
    public void GetLeadingTags_SplitsTheLeadingBlock(string text, string[] expected)
    {
        Assert.Equal(expected, AssaOverrideTags.GetLeadingTags(text));
    }

    [Theory]
    [InlineData("{\\an8\\pos(640,80)\\fad(200,200)}Text", 1, "{\\an8\\fad(200,200)}Text")]
    [InlineData("{\\an8}Text", 0, "Text")]
    [InlineData("{note\\an8}Text", 0, "{note}Text")]
    [InlineData("{\\an8}Text", 5, "{\\an8}Text")]
    [InlineData("Text", 0, "Text")]
    public void RemoveLeadingTag_RemovesOneTag(string text, int index, string expected)
    {
        Assert.Equal(expected, AssaOverrideTags.RemoveLeadingTag(text, index));
    }

    [AvaloniaFact]
    public void Layout14_ShowsTheLeadingTagsAsChips_AndTheirCrossRemovesOne()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            MakeAssa(vm);
            ShowLayout14(vm);

            // The chips have the top row of the text area to themselves, across all its columns.
            var textEditGrid = window.GetLogicalDescendants().OfType<Grid>().Single(g => g.Name == "SubtitleTextEditGrid");
            var chips = textEditGrid.Children.OfType<WrapPanel>().Single();
            Assert.Equal(0, Grid.GetRow(chips));
            Assert.Equal(textEditGrid.ColumnDefinitions.Count, Grid.GetColumnSpan(chips));
            Assert.Equal(new[] { "\\an8", "\\pos(640,80)" }, ChipTexts(chips));

            var cross = chips.GetLogicalDescendants().OfType<TextBlock>().Where(t => t.Text == "×").ElementAt(0);
            var point = cross.TranslatePoint(new Point(cross.Bounds.Width / 2, cross.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("{\\pos(640,80)}NORTHFIELD", vm.Subtitles[0].Text);
            Assert.Equal(new[] { "\\pos(640,80)" }, ChipTexts(chips));

            // Another line, other tags.
            vm.SelectedSubtitle = vm.Subtitles[1];
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(ChipTexts(chips));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Layout14_ListsTheErrorsOfTheFile_WithFixWhereThereIsOne()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddQcLines(vm);
            ShowLayout14(vm, 0);

            var issues = IssuesList(window).ItemsSource!.Cast<QcLayoutPanel.Issue>().ToList();
            Assert.Contains(issues, i => i.Line == vm.Subtitles[0] && i.Error.Type == LineErrorType.Overlap && i.Fix == vm.ExtendSelectedToNextCommand);
            Assert.Contains(issues, i => i.Line == vm.Subtitles[2] && i.Error.Type == LineErrorType.LineTooLong && i.Fix == vm.AutoBreakCommand);
            Assert.DoesNotContain(issues, i => i.Line == vm.Subtitles[3]);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Layout14_Fix_FixesTheLine_AndTheIssueGoesAway()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddQcLines(vm);
            ShowLayout14(vm, 3);
            var list = IssuesList(window);
            var overlap = list.ItemsSource!.Cast<QcLayoutPanel.Issue>().First(i => i.Error.Type == LineErrorType.Overlap);

            QcLayoutPanel.ApplyFix(vm, overlap.Line, overlap.Fix!);
            Dispatcher.UIThread.RunJobs();

            Assert.Same(vm.Subtitles[0], vm.SelectedSubtitle);
            Assert.True(vm.Subtitles[0].EndTime < vm.Subtitles[1].StartTime);

            RunRefresh(window);
            Assert.DoesNotContain(list.ItemsSource!.Cast<QcLayoutPanel.Issue>(), i => i.Error.Type == LineErrorType.Overlap);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Layout14_ListsBookmarks_AndPickingOneGoesToItsLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddQcLines(vm);
            vm.Subtitles[3].Bookmark = "Check the name";
            ShowLayout14(vm, 0);

            var list = window.GetLogicalDescendants().OfType<ListBox>().Single(l => AutomationProperties.GetName(l) == Se.Language.Main.QcBookmarks);
            Assert.Equal(new[] { vm.Subtitles[3] }, list.ItemsSource!.Cast<SubtitleLineViewModel>());

            list.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(vm.Subtitles[3], vm.SelectedSubtitle);

            // A new bookmark shows up once the edits settle.
            vm.Subtitles[1].Bookmark = string.Empty;
            RunRefresh(window);
            Assert.Equal(2, list.ItemsSource!.Cast<SubtitleLineViewModel>().Count());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static void AddQcLines(MainViewModel vm)
    {
        void Add(double start, double end, string text) => vm.Subtitles.Add(new SubtitleLineViewModel
        {
            Number = vm.Subtitles.Count + 1, Text = text, StartTime = TimeSpan.FromSeconds(start), EndTime = TimeSpan.FromSeconds(end),
        });

        Add(1, 3.5, "Overlaps the next one.");
        Add(3, 5, "Starts too early.");
        Add(6, 10, "This line is far too long to fit on one single line of a subtitle.");
        Add(12, 14, "Fine.");
    }

    private static ListBox IssuesList(Window window)
    {
        return window.GetLogicalDescendants().OfType<ListBox>().Single(l => AutomationProperties.GetName(l) == Se.Language.Main.QcIssues);
    }

    /// <summary>
    /// The lists rebuild on a short timer after edits. The headless platform never ticks timers,
    /// so check that one was queued and run it.
    /// </summary>
    private static void RunRefresh(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        var panel = IssuesList(window).GetLogicalAncestors().OfType<Grid>().First(g => QcLayoutPanel.IsRefreshPending(g) || g.RowDefinitions.Count == 6);
        Assert.True(QcLayoutPanel.IsRefreshPending(panel));
        QcLayoutPanel.RunPendingRefresh(panel);
        Assert.False(QcLayoutPanel.IsRefreshPending(panel));
    }

    private static void MakeAssa(MainViewModel vm)
    {
        vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "Advanced Sub Station Alpha");
        var subtitle = (Subtitle)typeof(MainViewModel).GetField("_subtitle", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(vm)!;
        subtitle.Header = """
            [Script Info]
            ScriptType: v4.00+
            PlayResX: 1280
            PlayResY: 720

            [V4+ Styles]
            Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
            Style: Default,Arial,52,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,30,1
            Style: Sign,Trebuchet MS,40,&H004DD8FF,&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,3,0,8,10,10,30,1

            [Events]
            Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
            """;

        vm.Subtitles.Add(new SubtitleLineViewModel { Number = 1, Text = "{\\an8\\pos(640,80)}NORTHFIELD", Style = "Sign", Layer = 1, StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(4) });
        vm.Subtitles.Add(new SubtitleLineViewModel { Number = 2, Text = "Did you hear that?", Style = "Default", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(3) });
    }

    /// <summary>Builds layout 14 and selects the first line (a rebuilt grid starts without a selection).</summary>
    private static void ShowLayout14(MainViewModel vm, int selectedIndex = 0)
    {
        InitLayout.MakeLayout(vm.MainView!, vm, 14);
        Dispatcher.UIThread.RunJobs();
        vm.SelectAndScrollToSubtitle(vm.Subtitles[selectedIndex]);
        Dispatcher.UIThread.RunJobs();
    }

    private static List<string> ChipTexts(WrapPanel chips)
    {
        return chips.Children.OfType<Border>()
            .Select(b => b.GetLogicalDescendants().OfType<TextBlock>().First().Text!)
            .ToList();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.SuppressSaveChangesPromptOnClose(vm);
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}

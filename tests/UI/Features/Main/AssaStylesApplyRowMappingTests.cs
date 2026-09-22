using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// The ASSA/SSA styles dialog works on the subtitle built from the working rows, which has no
/// entry for the display-only original rows. Its result must come back to the grid by paragraph
/// id: matched by position, every row after the first display-only row took a neighbour's style
/// when the user only changed a font size (#15126).
/// </summary>
public class AssaStylesApplyRowMappingTests
{
    [AvaloniaFact]
    public void FontSizeChange_WithDisplayOnlyOriginalRowInTheGrid_KeepsEveryLineStyle()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "Advanced Sub Station Alpha");
            AddLine(vm, "One", "Gothic", 0, 1000);
            AddLine(vm, "Two", "Default", 1000, 2000);
            AddReferenceOnlyLine(vm, "Untranslated original", 2000, 3000);
            AddLine(vm, "Three", "B Gothic", 3000, 4000);
            AddLine(vm, "Four", "Default", 4000, 5000);
            AddLine(vm, "Five", "Gothic", 5000, 6000);
            var rows = vm.Subtitles.ToList();

            var dialogSubtitle = OpenDialogWith(vm, out var rowMap);
            Assert.Equal(5, dialogSubtitle.Paragraphs.Count); // the display-only row is not in it

            // Only Gothic's font size changes.
            var header = MakeHeader(("Default", 40), ("Gothic", 39), ("B Gothic", 35));
            ApplyStylesFromDialog(vm, header, dialogSubtitle, rowMap);

            Assert.Equal(new[] { "Gothic", "Default", "", "B Gothic", "Default", "Gothic" }, vm.Subtitles.Select(p => p.Style));
            Assert.Equal(rows, vm.Subtitles);
            Assert.Contains("Style: Gothic,Arial,39,", vm.GetUpdateSubtitle().Header);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void StyleReassignedInTheDialog_ReachesThatRowAndNoOther()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "Advanced Sub Station Alpha");
            AddReferenceOnlyLine(vm, "Untranslated original", 0, 1000);
            AddLine(vm, "One", "Default", 1000, 2000);
            AddLine(vm, "Two", "Default", 2000, 3000);

            var dialogSubtitle = OpenDialogWith(vm, out var rowMap);
            dialogSubtitle.Paragraphs[1].Extra = "Gothic"; // "Two" - the dialog re-pointed it

            ApplyStylesFromDialog(vm, MakeHeader(("Default", 40), ("Gothic", 35)), dialogSubtitle, rowMap);

            Assert.Equal(new[] { "", "Default", "Gothic" }, vm.Subtitles.Select(p => p.Style));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A style deleted in the dialog leaves its lines pointing at nothing; they fall back to the
    /// first style in the file. The display-only original rows are not lines of the working
    /// subtitle and get no style.
    /// </summary>
    [AvaloniaFact]
    public void DeletedStyle_FallsBackToTheFirstStyle_AndLeavesDisplayOnlyRowsAlone()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.Name == "Advanced Sub Station Alpha");
            AddLine(vm, "One", "Gothic", 0, 1000);
            AddReferenceOnlyLine(vm, "Untranslated original", 1000, 2000);
            AddLine(vm, "Two", "B Gothic", 2000, 3000);

            var dialogSubtitle = OpenDialogWith(vm, out var rowMap);

            ApplyStylesFromDialog(vm, MakeHeader(("Default", 40), ("Gothic", 35)), dialogSubtitle, rowMap);

            Assert.Equal(new[] { "Gothic", "", "Default" }, vm.Subtitles.Select(p => p.Style));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static string MakeHeader(params (string Name, int FontSize)[] styles)
    {
        var ssaStyles = styles.Select(s => new SsaStyle { Name = s.Name, FontName = "Arial", FontSize = s.FontSize }).ToList();
        return AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, ssaStyles);
    }

    private static Subtitle OpenDialogWith(MainViewModel vm, out IReadOnlyDictionary<Guid, SubtitleLineViewModel> rowMap)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "GetUpdateSubtitleWithRowMap", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("GetUpdateSubtitleWithRowMap not found");

        var args = new object?[] { null };
        var subtitle = (Subtitle)method.Invoke(vm, args)!;
        rowMap = (IReadOnlyDictionary<Guid, SubtitleLineViewModel>)args[0]!;

        // The dialog works on a copy that keeps the paragraph ids.
        return new Subtitle(subtitle, false);
    }

    private static void ApplyStylesFromDialog(
        MainViewModel vm, string header, Subtitle resultSubtitle, IReadOnlyDictionary<Guid, SubtitleLineViewModel> rowMap)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ApplyStylesFromDialog", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ApplyStylesFromDialog not found");

        method.Invoke(vm, new object?[] { header, resultSubtitle, rowMap });
        Dispatcher.UIThread.RunJobs();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void AddLine(MainViewModel vm, string text, string style, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Style = style,
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void AddReferenceOnlyLine(MainViewModel vm, string originalText, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel
        {
            IsReferenceOnly = true,
            Text = string.Empty,
            OriginalText = originalText,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs),
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}

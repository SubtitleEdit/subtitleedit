using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// The After column can be edited in place: a nearly-right fix is corrected in the grid instead
/// of being declined and retyped in the main window. The edit is what Apply writes, and both diff
/// cells re-render against the edited text.
/// </summary>
public class AiReviewEditAfterTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static AiReviewViewModel MakeViewModel()
    {
        return new AiReviewViewModel(new WindowService(new NullServiceProvider()));
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Their going home.", 0, 1000));
        return subtitle;
    }

    private static ReviewSuggestionItem MakeSuggestion(string before, string after)
    {
        return new ReviewSuggestionItem
        {
            Number = 1,
            ParagraphIndex = 0,
            UnitId = 0,
            Category = ReviewCategory.Spelling,
            Before = before,
            After = after,
            IsSelected = true,
        };
    }

    [AvaloniaFact]
    public void Apply_WritesTheEditedAfterText()
    {
        var applied = new List<Subtitle>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, null, null, applied.Add);
        var item = MakeSuggestion("Their going home.", "There going home.");
        vm.AddSuggestionItem(item);

        item.After = "They're going home.";
        vm.ApplyCommand.Execute(null);

        Assert.Single(applied);
        Assert.Equal("They're going home.", applied[0].Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void Grid_AfterCell_TurnsIntoTextBoxOnClickAndCommitsOnEnter()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null);
        var item = MakeSuggestion("Their going home.", "There going home.");
        vm.AddSuggestionItem(item);
        var window = new AiReviewWindow(vm) { Width = 900, Height = 600 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var tableView = window.GetVisualDescendants().OfType<TableView>().Single();
            tableView.SelectedItem = item;
            Dispatcher.UIThread.RunJobs();

            var afterCell = FindAfterCell(tableView);
            Assert.NotNull(afterCell);
            Assert.Equal("There going home.", CellText(afterCell!));

            // The row is already selected, so a click on its After cell opens the editor.
            afterCell!.RaiseEvent(new PointerReleasedEventArgs(afterCell, new Pointer(0, PointerType.Mouse, true), afterCell,
                new Avalonia.Point(2, 2), 0, PointerPointProperties.None, KeyModifiers.None, MouseButton.Left)
            { RoutedEvent = InputElement.PointerReleasedEvent });
            Dispatcher.UIThread.RunJobs();

            var textBox = afterCell.GetVisualDescendants().OfType<TextBox>().SingleOrDefault();
            Assert.NotNull(textBox);
            Assert.Equal("There going home.", textBox!.Text);

            textBox.Text = "They're going home.";
            textBox.RaiseEvent(new KeyEventArgs { Key = Key.Enter, RoutedEvent = InputElement.KeyDownEvent, Source = textBox });
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("They're going home.", item.After);
            Assert.Empty(afterCell.GetVisualDescendants().OfType<TextBox>());
            Assert.Equal("They're going home.", CellText(afterCell));
            // The Before cell's diff is recomputed against the edited text too.
            Assert.Equal("Their going home.", CellText(FindBeforeCell(tableView)!));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Grid_AfterCell_EscapeDropsTheEdit()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null);
        var item = MakeSuggestion("Their going home.", "There going home.");
        vm.AddSuggestionItem(item);
        var window = new AiReviewWindow(vm) { Width = 900, Height = 600 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var tableView = window.GetVisualDescendants().OfType<TableView>().Single();
            tableView.SelectedItem = item;
            Dispatcher.UIThread.RunJobs();
            var afterCell = FindAfterCell(tableView)!;
            afterCell.RaiseEvent(new PointerReleasedEventArgs(afterCell, new Pointer(0, PointerType.Mouse, true), afterCell,
                new Avalonia.Point(2, 2), 0, PointerPointProperties.None, KeyModifiers.None, MouseButton.Left)
            { RoutedEvent = InputElement.PointerReleasedEvent });
            Dispatcher.UIThread.RunJobs();
            var textBox = afterCell.GetVisualDescendants().OfType<TextBox>().Single();

            textBox.Text = "garbage";
            textBox.RaiseEvent(new KeyEventArgs { Key = Key.Escape, RoutedEvent = InputElement.KeyDownEvent, Source = textBox });
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("There going home.", item.After);
            Assert.Empty(afterCell.GetVisualDescendants().OfType<TextBox>());
            Assert.True(window.IsVisible); // Escape ended the edit, it did not close the window
        }
        finally
        {
            window.Close();
        }
    }

    // The diff cells are the two Borders with an Ibeam cursor (After) / without (Before) that hold
    // a TextBlock built from inlines; the Before cell is the one without the cursor.
    private static Border? FindAfterCell(TableView tableView) =>
        DiffCells(tableView).FirstOrDefault(b => b.Cursor?.ToString() == new Cursor(StandardCursorType.Ibeam).ToString());

    private static Border? FindBeforeCell(TableView tableView) =>
        DiffCells(tableView).FirstOrDefault(b => b.Cursor == null);

    private static IEnumerable<Border> DiffCells(TableView tableView) =>
        tableView.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Child is TextBlock { Inlines.Count: > 0 } or TextBox);

    private static string CellText(Border cell) =>
        cell.Child is TextBlock tb ? tb.Inlines?.Text ?? tb.Text ?? string.Empty : string.Empty;
}

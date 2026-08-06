using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Text cells in a <see cref="TableView"/> must take their flow direction from their own
/// content, the way the main subtitle grid's cells do. A cell left at the window's
/// left-to-right direction lays a right-to-left line out under a left-to-right base
/// direction, which cuts the line at every zero width non-joiner (U+200C) and places the
/// pieces left to right - Persian words are torn in half and the word order is reversed
/// (issue #13160, reported for "Point sync via other subtitle").
/// </summary>
public class TableViewTextCellFlowDirectionTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    /// <summary>"دیگه نمی‌خوام ببینمت." - one Persian sentence with a ZWNJ inside نمی‌خوام.</summary>
    private const string PersianWithZwnj = "دیگه نمی‌خوام ببینمت.";

    [AvaloniaFact]
    public void TextCellFlowsRightToLeftForRightToLeftText()
    {
        var textBlock = RealizeTextCell(PersianWithZwnj);

        Assert.Equal(FlowDirection.RightToLeft, textBlock.FlowDirection);
    }

    [AvaloniaFact]
    public void TextCellFlowsLeftToRightForLeftToRightText()
    {
        var textBlock = RealizeTextCell("Hello world.");

        Assert.Equal(FlowDirection.LeftToRight, textBlock.FlowDirection);
    }

    /// <summary>
    /// The user-visible symptom: Avalonia gives the ZWNJ the paragraph embedding level
    /// instead of the level of the letters around it, so the sentence is laid out as
    /// several runs. Under a left-to-right base direction those runs are then placed left
    /// to right, which puts the first word of the sentence on the left - reversed word
    /// order, with نمی‌خوام torn in half. With the cell following its content the runs are
    /// ordered right to left again, so the first word is the rightmost thing on the line.
    /// </summary>
    [AvaloniaFact]
    public void RightToLeftTextKeepsItsFirstWordRightmost()
    {
        var textBlock = RealizeTextCell(PersianWithZwnj);
        var line = Assert.Single(textBlock.TextLayout.TextLines);

        // TextRuns are in visual order (left to right), so the last one is the rightmost.
        var rightmost = line.TextRuns
            .OfType<ShapedTextRun>()
            .Last(r => !string.IsNullOrWhiteSpace(r.Text.Span.ToString().Trim('‌')));

        Assert.StartsWith("دیگه", rightmost.Text.Span.ToString());
    }

    /// <summary>
    /// Builds a one-column grid with the shared text-cell template, shows it, and returns
    /// the realized cell's text block.
    /// </summary>
    private TextBlock RealizeTextCell(string text)
    {
        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = "Text",
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        tableView.ItemsSource = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = text },
        };

        var window = new Window { Width = 400, Height = 200, Content = tableView };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        return Assert.Single(tableView.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == text);
    }
}

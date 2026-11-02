using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// A macOS Ctrl+click on a selected list row opens the context menu, so it must keep the
/// selection like a right-click does. The class handler is only registered on macOS, so these
/// tests add it themselves and run on every platform.
/// </summary>
public class MacContextClickSelectionTests : IDisposable
{
    private readonly IDisposable _classHandler = InputElement.PointerPressedEvent.AddClassHandler<ListBoxItem>(
        MacContextClickSelection.OnRowPointerPressed, RoutingStrategies.Tunnel);

    private Window? _window;

    public void Dispose()
    {
        _classHandler.Dispose();
        _window?.Close();
    }

    [AvaloniaFact]
    public void CtrlClick_OnSelectedRow_KeepsTheWholeSelection()
    {
        var tableView = Show();
        tableView.Selection.SelectRange(1, 3);

        Click(tableView, row: 2, MouseButton.Left, RawInputModifiers.Control);

        Assert.Equal(new[] { 1, 2, 3 }, tableView.Selection.SelectedIndexes.Order());
    }

    [AvaloniaFact]
    public void CtrlClick_OnSelectedRow_MovesFocusToTheList()
    {
        var tableView = Show();
        var other = new Button();
        ((StackPanel)_window!.Content!).Children.Add(other);
        Settle();
        other.Focus();
        tableView.Selection.SelectRange(1, 3);

        Click(tableView, row: 2, MouseButton.Left, RawInputModifiers.Control);

        Assert.True(tableView.IsKeyboardFocusWithin);
        Assert.Equal(new[] { 1, 2, 3 }, tableView.Selection.SelectedIndexes.Order());
    }

    [AvaloniaFact]
    public void CtrlClick_WithFocusInsideTheRow_LeavesFocusThere()
    {
        var tableView = Show();
        tableView.Selection.SelectRange(1, 3);
        var row = (ListBoxItem)tableView.ContainerFromIndex(2)!;
        row.Focus();
        var editor = new TextBox();
        var cell = row.GetVisualDescendants().OfType<TableViewCell>().First();
        cell.Content = editor;
        Settle();
        editor.Focus();
        Assert.True(editor.IsFocused);

        Click(tableView, row: 2, MouseButton.Left, RawInputModifiers.Control);

        Assert.True(editor.IsFocused);
    }

    [AvaloniaFact]
    public void PlainClick_OnSelectedRow_CollapsesTheSelection()
    {
        var tableView = Show();
        tableView.Selection.SelectRange(1, 3);

        Click(tableView, row: 2, MouseButton.Left, RawInputModifiers.None);

        Assert.Equal(new[] { 2 }, tableView.Selection.SelectedIndexes);
    }

    [AvaloniaFact]
    public void CtrlShiftClick_OnSelectedRow_StaysARangeSelection()
    {
        var tableView = Show();
        tableView.Selection.Select(0);

        Click(tableView, row: 0, MouseButton.Left, RawInputModifiers.Control | RawInputModifiers.Shift);
        Click(tableView, row: 2, MouseButton.Left, RawInputModifiers.Control | RawInputModifiers.Shift);

        Assert.Equal(new[] { 0, 1, 2 }, tableView.Selection.SelectedIndexes.Order());
    }

    [AvaloniaFact]
    public void CtrlClick_OnUnselectedRow_IsLeftToTheList()
    {
        var tableView = Show();
        tableView.Selection.Select(0);
        var pressWasHandledByGuard = false;
        tableView.AddHandler(InputElement.PointerPressedEvent, (_, e) => pressWasHandledByGuard = e.Handled,
            RoutingStrategies.Bubble, handledEventsToo: false);

        Click(tableView, row: 2, MouseButton.Left, RawInputModifiers.Control);

        // The list took it (selection changed), rather than the guard swallowing the press.
        Assert.False(pressWasHandledByGuard);
        Assert.Contains(2, tableView.Selection.SelectedIndexes);
    }

    private TableView Show()
    {
        var tableView = new TableView
        {
            SelectionMode = SelectionMode.Multiple,
            ItemsSource = new[] { "a", "b", "c", "d", "e" },
            Height = 200,
        };
        tableView.Columns!.Add(new TableViewColumn { Header = "Text", Width = new GridLength(200) });

        _window = new Window { Width = 400, Height = 400, Content = new StackPanel { Children = { tableView } } };
        _window.Show();
        Settle();
        return tableView;
    }

    private void Click(TableView tableView, int row, MouseButton button, RawInputModifiers modifiers)
    {
        var container = (Control)tableView.ContainerFromIndex(row)!;
        var point = container.TranslatePoint(new Point(10, container.Bounds.Height / 2), _window!)!.Value;

        _window!.MouseDown(point, button, modifiers);
        _window!.MouseUp(point, button, modifiers);
        Settle();
    }

    private void Settle()
    {
        _window!.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}

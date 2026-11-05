using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The combo box overload of WindowService.SuspendUndockedTopmostWhileOpen must take the
/// suspension before the drop-down's popup opens. On Windows, demoting a topmost window
/// re-inserts it at the top of the non-topmost band, so a suspension taken with the popup
/// already on screen (DropDownOpened) would put the undocked audio visualizer back over the
/// format, encoding, and frame rate drop-downs (#16315).
/// </summary>
public class SuspendUndockedTopmostComboBoxTests : IDisposable
{
    private readonly List<bool> _calls = [];
    private readonly List<Window> _windows = [];

    public SuspendUndockedTopmostComboBoxTests()
    {
        WindowService.ResetUndockedTopmostSuspensionsForTests();
        WindowService.RegisterUndockedTopmostSetter(_calls.Add);
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        WindowService.RegisterUndockedTopmostSetter(null);
        WindowService.ResetUndockedTopmostSuspensionsForTests();
    }

    private ComboBox ShowWindowWithComboBox()
    {
        var comboBox = new ComboBox { ItemsSource = new[] { "one", "two" } };

        // Before the window is shown, as InitToolbar does it: the template does not exist yet.
        WindowService.SuspendUndockedTopmostWhileOpen(comboBox);

        var window = new Window { Width = 400, Height = 300, Content = comboBox };
        _windows.Add(window);
        window.Show();
        return comboBox;
    }

    private static Popup GetPopup(ComboBox comboBox)
    {
        return comboBox.GetVisualDescendants().OfType<Popup>().Single();
    }

    [AvaloniaFact]
    public void SuspensionEngagesBeforeThePopupOpens_AndRestoresOnClose()
    {
        var comboBox = ShowWindowWithComboBox();
        var popup = GetPopup(comboBox);

        bool? popupOpenAtSuspension = null;
        WindowService.RegisterUndockedTopmostSetter(topmost =>
        {
            _calls.Add(topmost);
            popupOpenAtSuspension ??= popup.IsOpen;
        });

        comboBox.IsDropDownOpen = true;

        Assert.True(popup.IsOpen);
        Assert.False(popupOpenAtSuspension);
        Assert.Equal([false], _calls);

        comboBox.IsDropDownOpen = false;

        Assert.False(popup.IsOpen);
        Assert.Equal([false, true], _calls);
    }

    [AvaloniaFact]
    public void ReopeningTakesAFreshSuspension()
    {
        var comboBox = ShowWindowWithComboBox();

        comboBox.IsDropDownOpen = true;
        comboBox.IsDropDownOpen = false;
        comboBox.IsDropDownOpen = true;

        Assert.Equal([false, true, false], _calls);

        comboBox.IsDropDownOpen = false;
        Assert.Equal([false, true, false, true], _calls);
    }

    [AvaloniaFact]
    public void ClosingThePopupItselfReleasesTheSuspension()
    {
        var comboBox = ShowWindowWithComboBox();

        comboBox.IsDropDownOpen = true;

        // A click outside closes the popup, which reaches the combo box through the template's
        // two-way binding rather than through a direct IsDropDownOpen assignment.
        GetPopup(comboBox).Close();

        Assert.False(comboBox.IsDropDownOpen);
        Assert.Equal([false, true], _calls);
    }

    [AvaloniaFact]
    public void OtherPropertyChangesDoNotSuspend()
    {
        var comboBox = ShowWindowWithComboBox();

        comboBox.SelectedIndex = 1;
        comboBox.Width = 200;

        Assert.Empty(_calls);
    }
}

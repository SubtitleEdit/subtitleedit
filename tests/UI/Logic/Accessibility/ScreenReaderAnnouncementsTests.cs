using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using System.Collections;
using System.Reflection;

namespace UITests.Logic.Accessibility;

/// <summary>
/// Screen-reader value announcements (#12087). Avalonia's Win32 UIA backend drops Value-pattern
/// property changes (its private property map has no entry for them), and a non-editable ComboBox
/// never raises a Value change when its selection changes while collapsed - together that kept
/// NVDA silent while Up/Down stepped through combo box values. SE patches the map via reflection
/// at startup and raises the missing Value change itself.
/// </summary>
public class ScreenReaderAnnouncementsTests : IDisposable
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

    /// <summary>
    /// Also the upgrade canary: the startup patch is deliberately fail-soft, so this test is what
    /// actually breaks the build when an Avalonia upgrade renames AutomationNode.s_propertyMap or
    /// the UiaPropertyId members - or makes the patch obsolete by shipping the entries itself.
    /// </summary>
    [Fact]
    public void PatchWin32UiaValuePropertyMap_AddsTheValuePatternEntries()
    {
        Assert.True(ScreenReaderAnnouncements.PatchWin32UiaValuePropertyMap());

        var assembly = Assembly.Load("Avalonia.Win32.Automation");
        var nodeType = assembly.GetType("Avalonia.Win32.Automation.AutomationNode", throwOnError: true)!;
        var mapField = nodeType.GetField("s_propertyMap", BindingFlags.NonPublic | BindingFlags.Static)!;
        var map = (IDictionary)mapField.GetValue(null)!;

        Assert.True(map.Contains(ValuePatternIdentifiers.ValueProperty));
        Assert.Equal("ValueValue", map[ValuePatternIdentifiers.ValueProperty]!.ToString());
        Assert.True(map.Contains(ValuePatternIdentifiers.IsReadOnlyProperty));
        Assert.Equal("ValueIsReadOnly", map[ValuePatternIdentifiers.IsReadOnlyProperty]!.ToString());
    }

    [AvaloniaFact]
    public void ComboBoxSelectionChange_RaisesValueChange_OnTheFocusedComboBoxPeer()
    {
        ScreenReaderAnnouncements.Initialize();

        var comboBox = new ComboBox { ItemsSource = new[] { "First", "Second" }, SelectedIndex = 0 };
        var window = new Window { Content = comboBox };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(comboBox.IsKeyboardFocusWithin);

        // Creating the peer up front models a running screen reader; without a UIA client no
        // peer exists and the handler stays out of the way (verified in the test below).
        var peer = ControlAutomationPeer.CreatePeerForElement(comboBox);
        var changes = new List<AutomationPropertyChangedEventArgs>();
        peer.PropertyChanged += (_, e) => changes.Add(e);

        comboBox.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        var change = Assert.Single(changes, e => e.Property == ValuePatternIdentifiers.ValueProperty);
        Assert.Equal("Second", change.NewValue as string);
        Assert.Equal("First", change.OldValue as string);

        window.Close();
    }

    [AvaloniaFact]
    public void ComboBoxSelectionChange_IsIgnored_WhileTheComboBoxIsNotFocused()
    {
        ScreenReaderAnnouncements.Initialize();

        var comboBox = new ComboBox { ItemsSource = new[] { "First", "Second" }, SelectedIndex = 0 };
        var textBox = new TextBox();
        var window = new Window { Content = new StackPanel { Children = { comboBox, textBox } } };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        var peer = ControlAutomationPeer.CreatePeerForElement(comboBox);
        var changes = new List<AutomationPropertyChangedEventArgs>();
        peer.PropertyChanged += (_, e) => changes.Add(e);

        // A selection changed programmatically (e.g. a dialog initializing its own controls)
        // must not be spoken over whatever the user is actually doing.
        comboBox.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain(changes, e => e.Property == ValuePatternIdentifiers.ValueProperty);

        window.Close();
    }
}

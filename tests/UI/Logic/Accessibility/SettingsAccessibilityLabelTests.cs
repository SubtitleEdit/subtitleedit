using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Options.Settings;

namespace UITests.Logic.Accessibility;

/// <summary>
/// Every row in the Settings window pairs a label with an input control. Without an explicit
/// association the controls are exposed to UI Automation as unnamed/generic elements, so a screen
/// reader cannot tell the user which setting they are on (issue #11745). <see cref="SettingsItem.Build"/>
/// links the control to its label via <see cref="AutomationProperties.LabeledByProperty"/>; these tests
/// pin that wiring down.
/// </summary>
public class SettingsAccessibilityLabelTests
{
    private static (Control control, TextBlock label) BuildRow(SettingsItem item)
    {
        var panel = (StackPanel)item.Build();
        var label = panel.Children.OfType<TextBlock>().First();
        var control = panel.Children.Last();
        return (control, label);
    }

    [AvaloniaFact]
    public void Build_LinksControlToItsLabel()
    {
        var (control, label) = BuildRow(new SettingsItem("Single line max length", () => new NumericUpDown()));

        Assert.Equal("Single line max length", label.Text);
        Assert.Same(label, AutomationProperties.GetLabeledBy(control));
    }

    [AvaloniaFact]
    public void Build_LinksComboBoxControlToItsLabel()
    {
        var (control, label) = BuildRow(new SettingsItem("Default encoding", () => new ComboBox()));

        Assert.IsType<ComboBox>(control);
        Assert.Same(label, AutomationProperties.GetLabeledBy(control));
    }

    [AvaloniaFact]
    public void Build_DoesNotLinkWhenLabelIsEmpty()
    {
        // Separator/anonymous rows have no label, so there is nothing meaningful to announce.
        var (control, _) = BuildRow(new SettingsItem(string.Empty, () => new CheckBox()));

        Assert.Null(AutomationProperties.GetLabeledBy(control));
    }
}

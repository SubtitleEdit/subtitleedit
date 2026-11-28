using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace UITests.Logic.Accessibility;

/// <summary>
/// Names a screen reader read wrong in the live UIA tree (#12087): toolbar buttons with a
/// literal "{0}", dialog buttons with the access-key underscore ("C_ancel"), and the
/// unnamed text box inside spinners, autocomplete boxes and editable combo boxes.
/// </summary>
public class AccessibleNameFixesTests
{
    [Fact]
    public void AccessibleNameFromHint_DropsShortcutPlaceholder()
    {
        Assert.Equal("Start a new subtitle file", UiUtil.AccessibleNameFromHint("Start a new subtitle file {0}"));
        Assert.Equal("Save", UiUtil.AccessibleNameFromHint("Save"));
        Assert.Equal(string.Empty, UiUtil.AccessibleNameFromHint(string.Empty));
    }

    [Fact]
    public void AccessibleNameFromHint_LeavesNoPlaceholderInAnyToolbarHint()
    {
        var toolbar = new LanguageMainToolbar();
        foreach (var property in typeof(LanguageMainToolbar).GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            var hint = (string?)property.GetValue(toolbar) ?? string.Empty;
            Assert.DoesNotContain("{0}", UiUtil.AccessibleNameFromHint(hint));
        }
    }

    [AvaloniaFact]
    public void MakeButton_WithAccessKey_IsNamedWithoutUnderscore()
    {
        var button = UiUtil.MakeButton("C_ancel", null);
        var window = Show(button);
        try
        {
            Assert.Equal("Cancel", ControlAutomationPeer.CreatePeerForElement(button).GetName());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MakeButton_WithAccessKey_ReplacedContentNamesItself()
    {
        var button = UiUtil.MakeButton("_OK", null);
        button.Content = "Apply";
        var window = Show(button);
        try
        {
            Assert.Equal("Apply", ControlAutomationPeer.CreatePeerForElement(button).GetName());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MakeButton_WithIconLeft_KeepsCleanName()
    {
        var button = UiUtil.MakeButton("_Find next", null).WithIconLeft("fa-solid fa-magnifying-glass");
        var window = Show(button);
        try
        {
            Assert.Equal("Find next", ControlAutomationPeer.CreatePeerForElement(button).GetName());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void RawNumericUpDown_InnerTextBox_GetsLabel()
    {
        AssertInnerTextBoxNamed(new NumericUpDown(), "PART_TextBox");
    }

    [AvaloniaFact]
    public void AutoCompleteBox_InnerTextBox_GetsLabel()
    {
        AssertInnerTextBoxNamed(new AutoCompleteBox(), "PART_TextBox");
    }

    [AvaloniaFact]
    public void EditableComboBox_InnerTextBox_GetsLabel()
    {
        AssertInnerTextBoxNamed(new ComboBox { IsEditable = true }, "PART_EditableTextBox");
    }

    [AvaloniaFact]
    public void RawNumericUpDown_SpinnerButtons_AreNamed()
    {
        AccessibleLabels.RegisterInnerTextBoxNames();
        var control = new NumericUpDown();
        var window = Show(control);
        try
        {
            var increase = control.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_IncreaseButton");
            Assert.Equal(Se.Language.General.Increase, AutomationProperties.GetName(increase));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// The status bar and the Find/Replace result are polite live regions. Avalonia's Win32
    /// backend raises UIA LiveRegionChanged from a live peer's Name change, so the text block's
    /// peer must report its new text as a Name change - this guards that across upgrades.
    /// </summary>
    [AvaloniaFact]
    public void LiveTextBlock_RaisesNameChange_WhenTextChanges()
    {
        var status = new TextBlock
        {
            Text = string.Empty,
            [AutomationProperties.LiveSettingProperty] = AutomationLiveSetting.Polite,
        };
        var window = Show(status);
        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(status);
            Assert.Equal(AutomationLiveSetting.Polite, peer.GetLiveSetting());

            string? announced = null;
            peer.PropertyChanged += (_, e) =>
            {
                if (e.Property == AutomationElementIdentifiers.NameProperty)
                {
                    announced = e.NewValue as string;
                }
            };

            status.Text = "Saved";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Saved", announced);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertInnerTextBoxNamed(Control control, string partName)
    {
        AccessibleLabels.RegisterInnerTextBoxNames();
        var label = new TextBlock { Text = "Max chars/sec" };
        AutomationProperties.SetLabeledBy(control, label);
        var window = Show(new StackPanel { Children = { label, control } });
        try
        {
            var inner = control.GetVisualDescendants().OfType<TextBox>().First(t => t.Name == partName);
            Assert.Equal("Max chars/sec", ControlAutomationPeer.CreatePeerForElement(inner).GetName());

            // A name set on the outer control later still reaches the focused text box.
            AutomationProperties.SetName(control, "Renamed");
            Assert.Equal("Renamed", ControlAutomationPeer.CreatePeerForElement(inner).GetName());
        }
        finally
        {
            window.Close();
        }
    }

    private static Window Show(Control content)
    {
        var window = new Window { Content = content, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}

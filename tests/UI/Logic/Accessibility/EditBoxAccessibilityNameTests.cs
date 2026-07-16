using System;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using UITests.Logic.Accessibility;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace UITests.Logic.Accessibility;

public class TestApp : Application
{
    public override void Initialize()
    {
        // The custom up/down controls embed a ButtonSpinner + TextBox that need a
        // theme to resolve their templates when shown in a headless window.
        Styles.Add(new FluentTheme());

        // AvaloniaEdit brings its own theme; without it a TextEditor gets no template and
        // never materializes its TextArea, so tests over the editor see nothing (same include
        // Program.cs adds at startup).
        Styles.Add(new StyleInclude(new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml", UriKind.Absolute))
        {
            Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"),
        });

        // Windows built in code use Attached.SetIcon; without the providers Program.cs
        // registers at startup, constructing such a window throws KeyNotFoundException.
        RegisterIconProvidersOnce();
    }

    private static bool _iconProvidersRegistered;

    private static void RegisterIconProvidersOnce()
    {
        if (_iconProvidersRegistered)
        {
            return;
        }

        _iconProvidersRegistered = true;
        Optris.Icons.Avalonia.IconProvider.Current
            .Register<Optris.Icons.Avalonia.FontAwesome.FontAwesomeIconProvider>()
            .Register<Optris.Icons.Avalonia.MaterialDesign.MaterialDesignIconProvider>();
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .WithInterFont();
}

/// <summary>
/// Verifies the editor's time/duration controls expose an accessible Name to the
/// platform automation layer (what NVDA / JAWS / Narrator / VoiceOver read aloud).
/// The custom controls receive keyboard focus on an inner PART_TextBox, so the name
/// set on the outer control must be forwarded to that text box (issue #11553).
/// </summary>
public class EditBoxAccessibilityNameTests
{
    private static TextBox GetInnerTextBox(Control control)
    {
        // Force the control's template to apply so PART_TextBox exists and the
        // name-forwarding in OnApplyTemplate runs.
        var window = new Window { Content = control, Width = 320, Height = 120 };
        window.Show();
        control.ApplyTemplate();

        return control.GetVisualDescendants().OfType<TextBox>().Single();
    }

    private static string? AutomationName(Control control)
        => ControlAutomationPeer.CreatePeerForElement(control)?.GetName();

    [AvaloniaFact]
    public void StartTime_NameReachesAutomationPeer()
    {
        var startTime = new TimeCodeUpDown();
        AutomationProperties.SetName(startTime, "Start time");

        var inner = GetInnerTextBox(startTime);

        Assert.Equal("Start time", AutomationProperties.GetName(inner));
        Assert.Equal("Start time", AutomationName(inner));
    }

    [AvaloniaFact]
    public void EndTime_NameReachesAutomationPeer()
    {
        var endTime = new TimeCodeUpDown();
        AutomationProperties.SetName(endTime, "Hide time");

        var inner = GetInnerTextBox(endTime);

        Assert.Equal("Hide time", AutomationName(inner));
    }

    [AvaloniaFact]
    public void Duration_NameReachesAutomationPeer()
    {
        var duration = new SecondsUpDown();
        AutomationProperties.SetName(duration, "Duration");

        var inner = GetInnerTextBox(duration);

        Assert.Equal("Duration", AutomationName(inner));
    }

    [AvaloniaFact]
    public void PlainTextBox_NameReachesAutomationPeer()
    {
        // The non-color-tag editor path is a plain TextBox; the name is set directly.
        var textBox = new TextBox();
        AutomationProperties.SetName(textBox, "Text");

        Assert.Equal("Text", AutomationName(textBox));
    }

    [AvaloniaFact]
    public void NumericUpDown_ForwardsAccessibleNameToInnerTextBox()
    {
        // Built via the real UiUtil factory, which wires up name forwarding to the
        // inner PART_TextBox (the focused element). Covers the Layer field and every
        // other numeric field in the app.
        var nud = UiUtil.MakeNumericUpDownInt(0, 100, 0, double.NaN, new object());
        AutomationProperties.SetName(nud, "Layer");

        var inner = GetInnerTextBox(nud);

        Assert.Equal("Layer", AutomationName(inner));
    }
}

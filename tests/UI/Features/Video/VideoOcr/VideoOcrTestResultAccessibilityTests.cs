using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using System.Linq;
using Xunit;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// "Test current frame" only wrote its result to the status text under the progress bar, and the
/// button being disabled during the test dropped keyboard focus to nothing - an NVDA user could
/// not find the result at all (#12087). Focus now returns to the button when the test is done,
/// and the result is the button's description.
///
/// The focus tests use a plain window: a shown VideoOcrWindow probes its (here empty) video file
/// on load and pops an error message box that takes focus, depending on timing.
/// </summary>
public class VideoOcrTestResultAccessibilityTests
{
    [AvaloniaFact]
    public void TestButton_HasTheTestResultAsItsDescription()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<VideoOcrViewModel>();
        var window = new VideoOcrWindow(vm);
        var button = window.GetLogicalDescendants().OfType<Button>().First(b => b.Command == vm.TestOcrCommand);

        vm.TestOcrResult = "Test result: Hello world";

        Assert.Equal("Test result: Hello world", ControlAutomationPeer.CreatePeerForElement(button).GetHelpText());
    }

    [AvaloniaFact]
    public void DisabledWhileFocused_GetsFocusBackWhenEnabledAgain()
    {
        var (window, button, _) = OpenWindow();
        try
        {
            Assert.True(button.Focus(NavigationMethod.Tab));

            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            Assert.Null(window.FocusManager?.GetFocusedElement()); // what disabling the button does

            button.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();

            Assert.Same(button, window.FocusManager?.GetFocusedElement());
            Assert.Contains(":focus-visible", button.Classes);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DisabledWithoutFocus_DoesNotTakeFocusWhenEnabledAgain()
    {
        var (window, button, other) = OpenWindow();
        try
        {
            Assert.True(other.Focus(NavigationMethod.Tab));

            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            button.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();

            Assert.Same(other, window.FocusManager?.GetFocusedElement());
        }
        finally
        {
            window.Close();
        }
    }

    private static (Window Window, Button Button, TextBox Other) OpenWindow()
    {
        var button = new Button { Content = "Test" };
        VideoOcrWindow.RefocusWhenReEnabled(button);
        var other = new TextBox();
        var window = new Window { Content = new StackPanel { Children = { other, button } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, button, other);
    }
}

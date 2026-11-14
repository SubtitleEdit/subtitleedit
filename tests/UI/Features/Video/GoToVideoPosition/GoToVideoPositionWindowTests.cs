using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.GoToVideoPosition;

/// <summary>
/// Go to video position must take a typed time code right away (#16649). The window used to focus
/// the TimeCodeUpDown itself, which is not focusable, so nothing had focus until the time code
/// box was clicked.
/// </summary>
public class GoToVideoPositionWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly SettingsScope _scope = new("General.UseFrameMode");

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        _scope.Dispose();
    }

    [AvaloniaFact]
    public void TimeCodeCanBeTypedWithoutClickingFirst()
    {
        Se.Settings.General.UseFrameMode = false;

        var vm = new GoToVideoPositionViewModel { Time = TimeSpan.FromSeconds(5) };
        var window = new GoToVideoPositionWindow(vm);
        _windows.Add(window);
        window.Show();
        window.Activate();
        Dispatcher.UIThread.RunJobs();

        var textBox = vm.UpDown.GetVisualDescendants().OfType<TextBox>().Single();
        Assert.True(textBox.IsFocused);

        // Typed from the first digit (hours), not into the milliseconds.
        window.KeyTextInput("00012345");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(TimeSpan.FromMilliseconds(83_450), vm.Time);
    }
}

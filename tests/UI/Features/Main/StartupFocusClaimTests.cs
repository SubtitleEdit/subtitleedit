using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// A second after start-up the main window hands keyboard focus to the subtitle grid. That claim
/// must not pull focus out of the text box the user is already typing in: it did, and on a busy
/// CI runner (where a test can stall past that second) it also made the Shift+Back and bare-Alt
/// keyboard tests flaky - the key landed on the grid instead of the text box or menu.
/// </summary>
public class StartupFocusClaimTests
{
    [AvaloniaFact]
    public async Task StartupGridFocus_DoesNotStealFocusFromTheTextBox()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        try
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 0, 2000), null!) { Number = 1 });
            vm.SelectedSubtitleIndex = 0;
            vm.SubtitleGrid.SelectedItem = vm.Subtitles[0];
            Dispatcher.UIThread.RunJobs();

            vm.EditTextBox.Focus();
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.EditTextBox.IsFocused);

            // Outlast the one-second start-up delay, pumping so its posted focus claim runs.
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 1300)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(20);
            }

            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.EditTextBox.IsFocused, "the start-up grid focus claim stole focus from the text box");
        }
        finally
        {
            window.Close();
        }
    }
}

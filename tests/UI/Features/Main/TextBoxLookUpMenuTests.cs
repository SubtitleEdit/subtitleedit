using System.Linq;
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
/// macOS "Look up" in the subtitle text box context menu (#14277). The menu item exists on macOS
/// only - it opens Dictionary.app through the "dict:" scheme, which no other platform has.
/// </summary>
public class TextBoxLookUpMenuTests
{
    [AvaloniaFact]
    public void SelectedText_IsWhatTheMenuOffersToLookUp()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            SelectTextInEditBox(vm, "The word évanoui here", start: 9, length: 7);

            vm.TextBoxContextOpening(null, EventArgs.Empty);

            if (OperatingSystem.IsMacOS())
            {
                Assert.True(vm.IsTextBoxLookUpVisible);
                Assert.Equal("Look up \"évanoui\"", vm.TextBoxLookUpHeader);
            }
            else
            {
                Assert.False(vm.IsTextBoxLookUpVisible);
            }
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void NothingSelectedAndNoRightClickedWord_HidesTheMenuItem()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            SelectTextInEditBox(vm, "The word évanoui here", start: 0, length: 0);

            vm.TextBoxContextOpening(null, EventArgs.Empty);

            Assert.False(vm.IsTextBoxLookUpVisible);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void TheMenuItemIsOnlyBuiltOnMacOs()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var flyout = vm.EditTextBox.TextControl.ContextFlyout as MenuFlyout;
            Assert.NotNull(flyout);

            var lookUpItems = flyout!.Items
                .OfType<MenuItem>()
                .Count(m => m.Command == vm.LookUpInDictionaryCommand);

            Assert.Equal(OperatingSystem.IsMacOS() ? 1 : 0, lookUpItems);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static void SelectTextInEditBox(MainViewModel vm, string text, int start, int length)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, 0, 2000), null!) { Number = 1 });
        vm.SelectedSubtitle = vm.Subtitles[0];
        Dispatcher.UIThread.RunJobs();

        vm.EditTextBox.Text = text;
        vm.EditTextBox.Select(start, length);
        Dispatcher.UIThread.RunJobs();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}

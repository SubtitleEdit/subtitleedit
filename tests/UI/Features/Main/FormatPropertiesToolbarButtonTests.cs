using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// The toolbar's format-properties button (the gear next to the format selector) mirrors the
/// File menu's "&lt;format&gt; properties..." item: it must appear exactly for the formats with a
/// format-specific properties/options dialog and follow the format selector as it changes.
/// </summary>
public class FormatPropertiesToolbarButtonTests
{
    [AvaloniaFact]
    public void FormatPropertiesButton_FollowsSelectedFormat()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var vm = (MainViewModel)view.DataContext!;

        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 0);
            Dispatcher.UIThread.RunJobs();

            var button = window.GetVisualDescendants()
                .OfType<Button>()
                .Single(b => ReferenceEquals(b.Command, vm.FilePropertiesShowCommand));

            // SubRip has no properties dialog.
            Assert.False(button.IsVisible);

            SelectFormat(vm, typeof(Ebu));
            Assert.True(button.IsVisible);
            Assert.Contains(new Ebu().Name, vm.FilePropertiesText);

            SelectFormat(vm, typeof(SubRip));
            Assert.False(button.IsVisible);

            // A format already covered by File > properties (DCinema interop) shows it too.
            SelectFormat(vm, typeof(DCinemaInterop));
            Assert.True(button.IsVisible);
        }
        finally
        {
            window.Closing -= vm.OnClosing;
            if (window.IsVisible)
            {
                window.Close();
            }
        }
    }

    private static void SelectFormat(MainViewModel vm, System.Type formatType)
    {
        vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f.GetType() == formatType);
        Dispatcher.UIThread.RunJobs();
    }
}

using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaInteropProperties;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files;

public class DCinemaInteropPropertiesTests
{
    private static DCinemaInteropPropertiesViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<DCinemaInteropPropertiesViewModel>();
    }

    [AvaloniaFact]
    public void Window_OpensWithDefaults()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte();

        var vm = MakeViewModel();
        vm.Initialize(new Subtitle(), new DCinemaInterop());
        var window = new DCinemaInteropPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.False(string.IsNullOrEmpty(vm.SubtitleId));
            Assert.Equal(1, vm.ReelNumber);
            Assert.Equal("English", vm.SelectedLanguage);
            Assert.Equal("Font1", vm.FontId);
            Assert.Equal("Arial.ttf", vm.FontUri);
            Assert.Equal(42, vm.FontSize);
            Assert.Equal(8, vm.TopBottomMargin);
            Assert.Equal(0, vm.ZPosition);
            Assert.Equal("None", vm.SelectedFontEffect);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Ok_SavesSettingsIncludingZPosition()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte();

        var vm = MakeViewModel();
        vm.Initialize(new Subtitle(), new DCinemaInterop());
        var window = new DCinemaInteropPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.MovieTitle = "Test Movie";
            vm.ReelNumber = 3;
            vm.SelectedFontEffect = "Border";
            vm.FontSize = 48;
            vm.TopBottomMargin = 10;
            vm.ZPosition = -1.25m;
            vm.FadeUpTime = 2;
            vm.FadeDownTime = 4;

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            var ss = Se.Settings.File.DCinemaSmpte;
            Assert.Equal("Test Movie", ss.CurrentDCinemaMovieTitle);
            Assert.Equal("3", ss.CurrentDCinemaReelNumber);
            Assert.Equal("border", ss.CurrentDCinemaFontEffect);
            Assert.Equal(48, ss.CurrentDCinemaFontSize);
            Assert.Equal(10, ss.DCinemaBottomMargin);
            Assert.Equal(-1.25, ss.DCinemaZPosition);
            Assert.Equal(2, ss.DCinemaFadeUpTime);
            Assert.Equal(4, ss.DCinemaFadeDownTime);
        }
        finally
        {
            if (!vm.OkPressed)
            {
                window.Close();
            }
        }
    }

    [AvaloniaFact]
    public void Cancel_DoesNotSaveSettings()
    {
        Se.Settings.File.DCinemaSmpte = new SeDCinemaSmpte();

        var vm = MakeViewModel();
        vm.Initialize(new Subtitle(), new DCinemaInterop());
        var window = new DCinemaInteropPropertiesWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.MovieTitle = "Discarded";
        vm.CancelCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.OkPressed);
        Assert.Equal(string.Empty, Se.Settings.File.DCinemaSmpte.CurrentDCinemaMovieTitle);
    }
}

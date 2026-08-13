using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.WebVttProperties;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files;

public class WebVttPropertiesTests
{
    private static WebVttPropertiesViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<WebVttPropertiesViewModel>();
    }

    [AvaloniaFact]
    public void Window_OpensWithDefaults()
    {
        Se.Settings.Formats = new SeFormats();

        var vm = MakeViewModel();
        var window = new WebVttPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("line:20%", vm.CueAn8);
            Assert.Equal("position:20% line:20%", vm.CueAn7);
            Assert.Equal(string.Empty, vm.CueAn2);
            Assert.True(vm.UseXTimestampMap);
            Assert.True(vm.MergeStyleTags);
            Assert.False(vm.MergeLinesWithSameText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Ok_SavesCueSettings()
    {
        Se.Settings.Formats = new SeFormats();

        var vm = MakeViewModel();
        var window = new WebVttPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.CueAn8 = "line:5%";
            vm.CueAn7 = "position:20% line:5%";
            vm.MergeStyleTags = false;
            vm.MergeLinesWithSameText = true;

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Equal("line:5%", Se.Settings.Formats.WebVttCueAn8);
            Assert.Equal("position:20% line:5%", Se.Settings.Formats.WebVttCueAn7);
            Assert.True(Se.Settings.Formats.WebVttDoNoMergeTags);
            Assert.True(Se.Settings.Formats.WebVttMergeLinesWithSameText);
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
        Se.Settings.Formats = new SeFormats();

        var vm = MakeViewModel();
        var window = new WebVttPropertiesWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.CueAn8 = "line:99%";
        vm.CancelCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.OkPressed);
        Assert.Equal("line:20%", Se.Settings.Formats.WebVttCueAn8);
    }

    [AvaloniaFact]
    public void Reset_RestoresDefaultCueSettings()
    {
        Se.Settings.Formats = new SeFormats();

        var vm = MakeViewModel();
        var window = new WebVttPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.CueAn8 = "line:5%";
            vm.ResetCueSettingsCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("line:20%", vm.CueAn8);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CueSettings_ReachLibSeAndAreWrittenToFile()
    {
        Se.Settings.Formats = new SeFormats();

        var vm = MakeViewModel();
        var window = new WebVttPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.CueAn8 = "line:5%";
            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Se.UpdateLibSeSettings();
            Assert.Equal("line:5%", Configuration.Settings.SubtitleSettings.WebVttCueAn8);

            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("{\\an8}Hello", 1000, 2000));
            var text = new WebVTT().ToText(subtitle, "test");

            Assert.Contains("line:5%", text);
            Assert.DoesNotContain("line:20%", text);
        }
        finally
        {
            if (!vm.OkPressed)
            {
                window.Close();
            }
        }
    }
}

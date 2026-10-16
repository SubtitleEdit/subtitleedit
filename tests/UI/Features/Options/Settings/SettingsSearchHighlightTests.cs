using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Options.Settings;

/// <summary>
/// While searching, the category highlight in the side panel follows the results instead of
/// staying on a category that has no match.
/// </summary>
public class SettingsSearchHighlightTests
{
    [AvaloniaFact]
    public void Search_HighlightsTheFirstMatchingCategory_AndTheSelectedOneAgainWhenCleared()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();
        var vm = Locator.Services.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var rules = vm.Sections.First(s => s.Title == Se.Language.General.Rules);
            var waveform = vm.Sections.First(s => s.Title == Se.Language.General.WaveformSpectrogram);
            vm.SelectedSection = rules;
            Dispatcher.UIThread.RunJobs();

            Button NavButton(SettingsSection section) => window.GetLogicalDescendants().OfType<Button>()
                .First(b => b.Classes.Contains(SettingsChrome.NavClass) && Equals(b.CommandParameter, section.Title));
            var search = window.GetLogicalDescendants().OfType<TextBox>().First();

            search.Text = "waveform double-click";
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain(SettingsChrome.SelectedClass, NavButton(rules).Classes);
            Assert.Contains(SettingsChrome.SelectedClass, NavButton(waveform).Classes);

            search.Text = "no setting is called like this";
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain(window.GetLogicalDescendants().OfType<Button>(),
                b => b.Classes.Contains(SettingsChrome.NavClass) && b.Classes.Contains(SettingsChrome.SelectedClass));

            search.Text = string.Empty;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(SettingsChrome.SelectedClass, NavButton(rules).Classes);
            Assert.Same(rules, vm.SelectedSection);
        }
        finally
        {
            window.Close();
        }
    }
}

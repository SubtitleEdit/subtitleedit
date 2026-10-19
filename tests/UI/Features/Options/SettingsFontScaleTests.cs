using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;
using Xunit;

namespace UITests.Features.Options;

/// <summary>
/// The editor look sets explicit font sizes and field heights, which the inherited window font of
/// the "Font scale (%)" setting does not reach: the Settings window stayed at 12.5 px whatever the
/// setting said (#16242). The sizes now scale with the setting, and stay as they were at 100 %.
/// </summary>
public class SettingsFontScaleTests : IDisposable
{
    public SettingsFontScaleTests()
    {
        SettingsViewModel.AnimateScrollToSection = false;
    }

    public void Dispose()
    {
        SettingsViewModel.AnimateScrollToSection = true;
        Se.Settings.Appearance.FontScale = 1.0;
    }

    [AvaloniaTheory]
    [InlineData(1.0, 12.5, 28)]
    [InlineData(1.5, 18.75, 42)]
    public void SettingsWindow_FieldsAndLabelsFollowFontScale(double fontScale, double expectedFontSize, double expectedHeight)
    {
        Se.Settings.Appearance.FontScale = fontScale;

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
            var field = rules.Panel!.GetVisualDescendants().OfType<NumericUpDown>().First(n => n.Classes.Contains("field"));
            Assert.Equal(expectedFontSize, field.FontSize);
            Assert.Equal(expectedHeight, field.Height);

            var combo = rules.Panel.GetVisualDescendants().OfType<ComboBox>().First();
            Assert.Equal(expectedFontSize, combo.FontSize);
            Assert.Equal(expectedHeight, combo.MinHeight);

            var label = rules.Panel.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.Text == Se.Language.Options.Settings.SingleLineMaxLength);
            Assert.Equal(expectedFontSize, label.FontSize);

            var category = window.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == rules.Title);
            Assert.Equal(expectedFontSize, category.FontSize);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void EditorChromeSizes_ScaleFromTheDesignValues()
    {
        Se.Settings.Appearance.FontScale = 1.3;
        Assert.Equal(12.5 * 1.3, EditorChrome.FieldFontSize, 6);
        Assert.Equal(36, EditorChrome.FieldHeight); // 28 * 1.3 = 36.4, whole pixels
        Assert.Equal(325, SettingsChrome.LabelWidth);
    }
}

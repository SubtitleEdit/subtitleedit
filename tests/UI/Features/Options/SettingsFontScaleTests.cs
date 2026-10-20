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

namespace UITests.Features.Options;

/// <summary>
/// The editor look sets explicit font sizes and field heights, which the inherited window font of
/// the "Font scale (%)" setting does not reach: the Settings window stayed at 12.5 px whatever the
/// setting said (#16242). The sizes now scale with the setting. At 100 % the dialogs and the main
/// window share one 13 px text size (#16276): the dialogs were 12.5 and the main window 14.
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
    [InlineData(1.0, 13, 28)]
    [InlineData(1.5, 19.5, 42)]
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

    [AvaloniaTheory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    public void MainWindow_UsesTheEditorLookAndTheDialogTextSize(double fontScale)
    {
        Se.Settings.Appearance.FontScale = fontScale;

        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();
        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        UiTheme.ApplyScaleToWindow(window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            window.SuppressSaveChangesPromptOnClose((MainViewModel)view.DataContext!);

            Assert.Equal(EditorChrome.FieldFontSize, window.FontSize, 6);
            Assert.Equal(UiTheme.BaseFontSize * fontScale, window.FontSize, 6);

            var combo = view.GetVisualDescendants().OfType<ComboBox>().First();
            Assert.Equal(EditorChrome.FieldFontSize, combo.FontSize, 6);
            Assert.Equal(EditorChrome.FieldHeight, combo.MinHeight);
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
        Assert.Equal(UiTheme.BaseFontSize * 1.3, EditorChrome.FieldFontSize, 6);
        Assert.Equal(36, EditorChrome.FieldHeight); // 28 * 1.3 = 36.4, whole pixels
        Assert.Equal(325, SettingsChrome.LabelWidth);
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

/// <summary>
/// Update settings in the editor look (see <see cref="EditorChrome"/>): the startup check switch
/// and the release channel in one section, sized to its content.
/// </summary>
public class CheckForUpdatesSettingsWindow : Window
{
    private const double LabelWidth = 160;

    private readonly CheckForUpdatesSettingsViewModel _vm;

    public CheckForUpdatesSettingsWindow(CheckForUpdatesSettingsViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
        DataContext = vm;
        vm.Window = this;
        Width = MinWidth = 420;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var comboChannel = UiUtil.MakeComboBox(vm.UpdateChannels, vm, nameof(vm.SelectedUpdateChannel))
            .WithAccessibleName(Se.Language.Options.Settings.CheckForUpdatesChannel);
        comboChannel.HorizontalAlignment = HorizontalAlignment.Stretch;

        var rows = new StackPanel { Spacing = 10 };
        if (vm.IsStartupCheckVisible)
        {
            rows.Children.Add(EditorChrome.MakeSwitchRow(nameof(vm.CheckForUpdatesOnStartup), Se.Language.Options.Settings.CheckForUpdatesOnStartup));
        }

        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.Options.Settings.CheckForUpdatesChannel, comboChannel, LabelWidth, spacing: 6));

        var sections = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto"),
        };
        sections.Add(EditorChrome.MakeEditorSection(Se.Language.Options.Settings.Updates, rows, first: true), 0, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        Content = EditorChrome.MakeRoot(sections, buttonBar);
        // Sized to the settings, so the panel row must not ask for the rest of the height
        ((Grid)Content).RowDefinitions[0].Height = GridLength.Auto;

        UiUtil.FocusOnFirstActivation(this, comboChannel); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

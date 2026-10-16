using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickSpellCheckDictionary;

/// <summary>
/// Pick a spell check dictionary, in the editor look (see <see cref="SharedDialogChrome"/>).
/// </summary>
public class PickSpellCheckDictionaryWindow : Window
{
    public PickSpellCheckDictionaryWindow(PickSpellCheckDictionaryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.ChooseSpellCheckDictionary;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        SharedDialogChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var combo = new ComboBox
        {
            ItemsSource = vm.Dictionaries,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 260,
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.General.Dictionary,
            [!ComboBox.SelectedValueProperty] = new Binding(nameof(vm.SelectedDictionary)),
        };

        var buttonDownload = UiUtil
            .MakeButtonBrowse(vm.BrowseDictionaryCommand, accessibleName: Se.Language.SpellCheck.ChooseSpellCheckDictionary)
            .WithLeftAlignment();
        buttonDownload.Margin = new Avalonia.Thickness(0);

        var panelDownload = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        panelDownload.Add(combo, 0, 0);
        panelDownload.Add(buttonDownload, 0, 1);

        var section = SharedDialogChrome.MakeSection(Se.Language.General.Dictionary, panelDownload, first: true);

        var buttonOk = SharedDialogChrome.Primary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = SharedDialogChrome.MakeRoot(section, SharedDialogChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, combo); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

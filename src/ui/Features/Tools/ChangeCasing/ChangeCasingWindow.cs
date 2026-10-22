using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

/// <summary>
/// Change casing, in the editor look (see <see cref="EditorChrome"/>): the casing choices (normal
/// casing with its two options as switches) in one section, Cancel and OK at the bottom.
/// </summary>
public class ChangeCasingWindow : Window
{
    public ChangeCasingWindow(ChangeCasingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.ChangeCasing);
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 300;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var radioButtonNormalCasing = MakeRadio(Se.Language.General.NormalCasing, nameof(vm.NormalCasing));

        var rowNormalCasingFixNames = EditorChrome.MakeToggleRow(nameof(vm.NormalCasingFixNames), Se.Language.Tools.ChangeCasing.FixNames);
        rowNormalCasingFixNames.Margin = new Thickness(28, 2, 0, 4);
        rowNormalCasingFixNames.Bind(IsEnabledProperty, new Binding(nameof(vm.NormalCasing)));
        rowNormalCasingFixNames.Bind(OpacityProperty, new Binding(nameof(vm.NormalCasing)) { Converter = EnabledOpacityConverter });

        var rowNormalCasingOnlyUpper = EditorChrome.MakeToggleRow(nameof(vm.NormalCasingOnlyUpper), Se.Language.Tools.ChangeCasing.OnlyFixUppercaseLines);
        rowNormalCasingOnlyUpper.Margin = new Thickness(28, 0, 0, 8);
        rowNormalCasingOnlyUpper.Bind(IsEnabledProperty, new Binding(nameof(vm.NormalCasing)));
        rowNormalCasingOnlyUpper.Bind(OpacityProperty, new Binding(nameof(vm.NormalCasing)) { Converter = EnabledOpacityConverter });

        var panel = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                radioButtonNormalCasing,
                rowNormalCasingFixNames,
                rowNormalCasingOnlyUpper,
                MakeRadio(Se.Language.Tools.ChangeCasing.FixNamesOnly, nameof(vm.FixNamesOnly)),
                MakeRadio(Se.Language.Tools.ChangeCasing.AllUppercase, nameof(vm.AllUppercase)),
                MakeRadio(Se.Language.Tools.ChangeCasing.AllLowercase, nameof(vm.AllLowercase)),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = new StackPanel { MinWidth = 320 };
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Casing, panel, first: true));

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, radioButtonNormalCasing); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    // a disabled row's label (plain TextBlock) has no disabled look, so dim the whole row
    private static readonly FuncValueConverter<bool, double> EnabledOpacityConverter = new(on => on ? 1.0 : 0.5);

    private static RadioButton MakeRadio(string text, string property)
    {
        return new RadioButton
        {
            Content = text,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
    }
}

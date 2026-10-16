using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinarySettings;

/// <summary>
/// Binary edit settings in the editor look (see <see cref="EditorChrome"/>): the margins in one
/// section, Cancel/OK at the bottom.
/// </summary>
public class BinarySettingsWindow : Window
{
    private const double LabelWidth = 80;

    public BinarySettingsWindow(BinarySettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var numericUpDownMarginLeft = MakeMarginField(vm, nameof(vm.MarginLeft), Se.Language.General.Left);
        var numericUpDownMarginTop = MakeMarginField(vm, nameof(vm.MarginTop), Se.Language.General.Top);
        var numericUpDownMarginRight = MakeMarginField(vm, nameof(vm.MarginRight), Se.Language.General.Right);
        var numericUpDownMarginBottom = MakeMarginField(vm, nameof(vm.MarginBottom), Se.Language.General.Bottom);

        var rows = new StackPanel { Spacing = 10 };
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.General.Left, numericUpDownMarginLeft, LabelWidth));
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.General.Top, numericUpDownMarginTop, LabelWidth));
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.General.Right, numericUpDownMarginRight, LabelWidth));
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.General.Bottom, numericUpDownMarginBottom, LabelWidth));
        var section = EditorChrome.MakeEditorSection(Se.Language.General.Margin, rows, first: true);
        section.MinWidth = 280;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, numericUpDownMarginLeft); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static NumericUpDown MakeMarginField(BinarySettingsViewModel vm, string property, string name)
    {
        var numericUpDown = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDown.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = property,
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        return EditorChrome.AsField(numericUpDown, name, 100, alignLeft: true, center: true);
    }
}

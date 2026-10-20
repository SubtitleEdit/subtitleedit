using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayer;

/// <summary>
/// Pick an ASSA layer number, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class PickLayerWindow : Window
{
    public PickLayerWindow(PickLayerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.PickLayer;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 300;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var upDownLayer = new NumericUpDown
        {
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Layer))
            {
                Mode = BindingMode.TwoWay,
            },
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            Width = 140,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        upDownLayer.Classes.Add("field"); // the compact field, not the theme's big spinner
        AutomationProperties.SetName(upDownLayer, Se.Language.General.Layer);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(upDownLayer, Se.Language.General.Layer);
        }

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Layer, upDownLayer, first: true);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, upDownLayer); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

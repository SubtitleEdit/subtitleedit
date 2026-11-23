using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// Width and height fields for a custom image export resolution (#17132).
/// </summary>
public class CustomResolutionWindow : Window
{
    public CustomResolutionWindow(CustomResolutionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.CustomResolution;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var upDownWidth = MakeUpDown(nameof(vm.ResolutionWidth), Se.Language.General.Width);
        var upDownHeight = MakeUpDown(nameof(vm.ResolutionHeight), Se.Language.General.Height);
        upDownWidth.KeyDown += (_, args) => vm.OnKeyDown(args);
        upDownHeight.KeyDown += (_, args) => vm.OnKeyDown(args);

        var fields = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                MakeLabel(Se.Language.General.Width),
                upDownWidth,
                MakeLabel(Se.Language.General.Height),
                upDownHeight,
            },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Resolution, fields, first: true);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, upDownWidth);
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }

    private static TextBlock MakeLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static NumericUpDown MakeUpDown(string bindingPath, string automationName)
    {
        var upDown = new NumericUpDown
        {
            Width = 130,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Minimum = CustomResolutionViewModel.MinSize,
            Maximum = CustomResolutionViewModel.MaxSize,
            Increment = 1,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(bindingPath) { Mode = BindingMode.TwoWay },
        };
        upDown.Classes.Add("field");
        AutomationProperties.SetName(upDown, automationName);
        return upDown;
    }
}

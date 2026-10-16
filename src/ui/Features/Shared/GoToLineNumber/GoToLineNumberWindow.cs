using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

/// <summary>
/// Go to line number, in the editor look (see <see cref="EditorChrome"/>): one field, Cancel and
/// OK at the bottom.
/// </summary>
public class GoToLineNumberWindow : Window
{
    public GoToLineNumberWindow(GoToLineNumberViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.GoToLineNumber;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        vm.UpDown = new NumericUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            Width = 150,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.LineNumber))
            {
                Mode = BindingMode.TwoWay,
            },
            Minimum = 1,
            Increment = 1,          // Only step in whole numbers
            FormatString = "F0",    // Show 0 decimal places
        };
        vm.UpDown.Classes.Add("field");
        AutomationProperties.SetName(vm.UpDown, Se.Language.General.GoToLineNumber);
        vm.UpDown.KeyDown += (sender, args) => vm.OnKeyDown(args);

        var row = EditorChrome.MakeLabeledRow(Se.Language.General.GoToLineNumber, vm.UpDown, 130);
        row.Margin = new Thickness(16, 18, 16, 18);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(row, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => { vm.Activated(); });
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}

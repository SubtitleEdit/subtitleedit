using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.AddToOcrReplaceList;

/// <summary>
/// Add a "from → to" pair to an OCR replace list, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class AddToOcrReplaceListWindow : Window
{
    public AddToOcrReplaceListWindow(AddToOcrReplaceListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.AddNameToOcrReplaceList;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var labelFromTo = EditorChrome.MakeWrappingDimLabel(Se.Language.Ocr.ChangeWordFromTo);
        var textBoxFrom = UiUtil.MakeTextBox(200, vm, nameof(vm.From));
        textBoxFrom.Classes.Add("text");
        AutomationProperties.SetName(textBoxFrom, Se.Language.General.From);
        var textBoxTo = UiUtil.MakeTextBox(200, vm, nameof(vm.To));
        textBoxTo.Classes.Add("text");
        AutomationProperties.SetName(textBoxTo, Se.Language.General.To);
        var panelFromTo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                textBoxFrom,
                new TextBlock { Text = "→", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center },
                textBoxTo,
            },
        };

        var labelDictionary = EditorChrome.MakeWrappingDimLabel(Se.Language.General.Dictionary);
        labelDictionary.Margin = new Thickness(0, 8, 0, 0);
        var comboBoxDictionaries = new ComboBox
        {
            Width = 200,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDictionary)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Dictionaries)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(comboBoxDictionaries, Se.Language.General.Dictionary);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var panel = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(16, 14, 16, 16),
            Children =
            {
                labelFromTo,
                panelFromTo,
                labelDictionary,
                comboBoxDictionaries,
            },
        };

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxFrom); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }
}

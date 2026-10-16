using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.AddToUserDictionary;

/// <summary>
/// Add a word to a user dictionary, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class AddToUserDictionaryWindow : Window
{
    public AddToUserDictionaryWindow(AddToUserDictionaryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.AddNameToUserDictionary;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var labelWord = EditorChrome.MakeWrappingDimLabel(Se.Language.Ocr.WordToAdd);
        var textBoxWord = UiUtil.MakeTextBox(260, vm, nameof(vm.Word));
        textBoxWord.Classes.Add("text");
        AutomationProperties.SetName(textBoxWord, Se.Language.Ocr.WordToAdd);

        var labelDictionary = EditorChrome.MakeWrappingDimLabel(Se.Language.General.Dictionary);
        labelDictionary.Margin = new Thickness(0, 8, 0, 0);
        var comboBoxDictionaries = new ComboBox
        {
            Width = 260,
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
                labelWord,
                textBoxWord,
                labelDictionary,
                comboBoxDictionaries,
            },
        };

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxWord); // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}

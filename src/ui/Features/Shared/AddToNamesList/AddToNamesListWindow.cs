using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.AddToNamesList;

/// <summary>
/// Add a name (or several, one per line) to a names list, in the editor look (see
/// <see cref="EditorChrome"/>): the name and dictionary fields, the single/multi mode switch on
/// the left of the button bar, Cancel and OK on the right.
/// </summary>
public class AddToNamesListWindow : Window
{
    // One width for the name box, the multi-name box and the dictionary drop-down, so the
    // dialog keeps the same size in both modes and long entries like "English (United
    // States) [en_US]" are not truncated (#12886).
    private const int ContentWidth = 400;

    public AddToNamesListWindow(AddToNamesListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.AddNameToNamesList;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var labelWord = EditorChrome.MakeWrappingDimLabel(Se.Language.Ocr.NameToAdd);
        labelWord.Bind(IsVisibleProperty, new Binding(nameof(vm.IsSingleMode)));

        var textBoxWord = UiUtil.MakeTextBox(ContentWidth, vm, nameof(vm.Name), nameof(vm.IsSingleMode));
        textBoxWord.Classes.Add("text");
        AutomationProperties.SetName(textBoxWord, Se.Language.Ocr.NameToAdd);

        var labelMultiNames = EditorChrome.MakeWrappingDimLabel(Se.Language.SpellCheck.EnterOneNamePerLine);
        labelMultiNames.Bind(IsVisibleProperty, new Binding(nameof(vm.IsMultiMode)));

        var textBoxMultiNames = new TextBox
        {
            Width = ContentWidth,
            Height = 250,
            AcceptsReturn = true,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.MultiNames)) { Mode = BindingMode.TwoWay },
            [!TextBox.IsVisibleProperty] = new Binding(nameof(vm.IsMultiMode)),
        };
        textBoxMultiNames.Classes.Add("text");
        AutomationProperties.SetName(textBoxMultiNames, Se.Language.SpellCheck.EnterOneNamePerLine);

        var labelDictionary = EditorChrome.MakeWrappingDimLabel(Se.Language.General.Dictionary);
        labelDictionary.Margin = new Thickness(0, 8, 0, 0);
        var comboBoxDictionaries = new ComboBox
        {
            Width = ContentWidth,
            HorizontalAlignment = HorizontalAlignment.Left,
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
                labelMultiNames,
                textBoxMultiNames,
                labelDictionary,
                comboBoxDictionaries,
            },
        };

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeDockedButtonBar(MakeMultiModeButton(vm), buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => { (vm.IsSingleMode ? textBoxWord : textBoxMultiNames).Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    /// <summary>
    /// Flat "Multi mode" / "Single mode" toggle; the text follows the current mode.
    /// </summary>
    private static Button MakeMultiModeButton(AddToNamesListViewModel vm)
    {
        var text = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.MultiModeButtonText)),
        };
        var button = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-list", FontSize = 12, VerticalAlignment = VerticalAlignment.Center },
                    text,
                },
            },
            Command = vm.ToggleMultiModeCommand,
            Height = 28,
            Padding = new Thickness(8, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!AutomationProperties.NameProperty] = new Binding(nameof(vm.MultiModeButtonText)),
        };
        button.Classes.Add("tool");
        return button;
    }
}

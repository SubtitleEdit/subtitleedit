using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.AddToUserDictionary;

public class AddToUserDictionaryWindow : Window
{
    public AddToUserDictionaryWindow(AddToUserDictionaryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.AddNameToUserDictionary;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;

        var labelWord = UiUtil.MakeLabel(Se.Language.Ocr.WordToAdd);
        var textBoxWord = UiUtil.MakeTextBox(200, vm, nameof(vm.Word));

        var labelDictionary = UiUtil.MakeLabel(Se.Language.General.Dictionary).WithMarginTop(20);
        var comboBoxDictionaries = new ComboBox
        {
            Width = 200,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDictionary)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Dictionaries)) { Mode = BindingMode.TwoWay },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 2,
            RowSpacing = 2,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelWord, 0);
        grid.Add(textBoxWord, 1);
        grid.Add(labelDictionary, 2);
        grid.Add(comboBoxDictionaries, 3);
        grid.Add(buttonPanel, 4);

        Content = grid;

        Activated += delegate { textBoxWord.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}

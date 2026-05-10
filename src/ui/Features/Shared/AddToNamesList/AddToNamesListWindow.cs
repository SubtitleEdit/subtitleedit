using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.AddToNamesList;

public class AddToNamesListWindow : Window
{
    public AddToNamesListWindow(AddToNamesListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.AddNameToNamesList;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;

        var labelWord = UiUtil.MakeLabel(Se.Language.Ocr.NameToAdd);
        labelWord.Bind(Label.IsVisibleProperty, new Binding(nameof(vm.IsSingleMode)));

        var textBoxWord = UiUtil.MakeTextBox(200, vm, nameof(vm.Name), nameof(vm.IsSingleMode));

        var labelMultiNames = UiUtil.MakeLabel(Se.Language.SpellCheck.EnterOneNamePerLine);
        labelMultiNames.Bind(Label.IsVisibleProperty, new Binding(nameof(vm.IsMultiMode)));

        var textBoxMultiNames = new TextBox
        {
            Width = 400,
            Height = 250,
            AcceptsReturn = true,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.MultiNames)) { Mode = BindingMode.TwoWay },
            [!TextBox.IsVisibleProperty] = new Binding(nameof(vm.IsMultiMode)),
        };

        var labelDictionary = UiUtil.MakeLabel(Se.Language.General.Dictionary).WithMarginTop(20);
        var comboBoxDictionaries = new ComboBox
        {
            Width = 200,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDictionary)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Dictionaries)) { Mode = BindingMode.TwoWay },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonMultiMode = UiUtil.MakeButton(Se.Language.General.MultiMode, vm.ToggleMultiModeCommand);
        buttonMultiMode.Bind(Button.ContentProperty, new Binding(nameof(vm.MultiModeButtonText)));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonMultiMode, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
        grid.Add(labelMultiNames, 2);
        grid.Add(textBoxMultiNames, 3);
        grid.Add(labelDictionary, 4);
        grid.Add(comboBoxDictionaries, 5);
        grid.Add(buttonPanel, 6);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}

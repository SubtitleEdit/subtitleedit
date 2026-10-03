using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.PickTargetLanguages;

public class PickTargetLanguagesWindow : Window
{
    public PickTargetLanguagesWindow(PickTargetLanguagesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.AlsoTranslateTo;
        CanResize = true;
        Width = 450;
        Height = 600;
        MinWidth = 350;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = new TextBox
        {
            Margin = new Thickness(5, 0, 0, 0),
            Width = 250,
        }.WithSearchAndClearIcons();
        textBoxSearch.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        textBoxSearch.TextChanged += (_, _) => vm.SearchTextChanged();

        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

        var listBoxLanguages = new ListBox
        {
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<PickTargetLanguageItem>((item, _) =>
            {
                var checkBox = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                };
                checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(PickTargetLanguageItem.IsChecked)) { Mode = BindingMode.TwoWay });
                checkBox.Bind(ContentControl.ContentProperty, new Binding(nameof(PickTargetLanguageItem.Name)));
                return checkBox;
            }, true),
        };
        listBoxLanguages.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Languages)));

        var listBoxBorder = UiUtil.MakeBorderForControl(listBoxLanguages);

        var buttonClear = UiUtil.MakeButton(Se.Language.General.None, vm.ClearAllCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonClear, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Search
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },  // ListBox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelSearch, 0);
        grid.Add(listBoxBorder, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxSearch);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

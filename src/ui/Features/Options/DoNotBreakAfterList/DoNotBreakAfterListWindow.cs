using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.DoNotBreakAfterList;

public class DoNotBreakAfterListWindow : Window
{
    private readonly DoNotBreakAfterListViewModel _vm;

    public DoNotBreakAfterListWindow(DoNotBreakAfterListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.UseDoNotBreakAfterList;
        CanResize = true;
        Width = 500;
        Height = 600;
        MinWidth = 400;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language);
        var comboLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        comboLanguages.SelectionChanged += (_, _) => vm.SelectedLanguageChanged();
        var buttonNewLanguage = UiUtil.MakeButton(Se.Language.General.New, vm.NewLanguageCommand);
        var panelLanguage = UiUtil.MakeHorizontalPanel(labelLanguage, comboLanguages, buttonNewLanguage);

        var listBoxItems = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Items)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedItem)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxItems.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(1)),
            }
        });
        var listBoxBorder = UiUtil.MakeBorderForControl(listBoxItems);

        var textBoxItem = UiUtil.MakeTextBox(150, vm, nameof(vm.NewItemText));
        textBoxItem.KeyDown += (_, e) => vm.ItemTextBoxKeyDown(e);
        var radioText = new RadioButton
        {
            Content = Se.Language.General.Text,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsTextItem)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        var radioRegex = new RadioButton
        {
            Content = Se.Language.General.RegularExpression,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsRegexItem)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        var buttonAdd = UiUtil.MakeButton(vm.AddItemCommand, IconNames.Plus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.RemoveItemCommand, IconNames.Trash, Se.Language.General.Remove);
        var panelEdit = UiUtil.MakeButtonBar(textBoxItem, radioText, radioRegex, buttonAdd, buttonRemove).WithAlignmentLeft().WithSpacing(4);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // language
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // list
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // add/remove
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelLanguage, 0);
        grid.Add(listBoxBorder, 1);
        grid.Add(panelEdit, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { comboLanguages.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => vm.SelectedLanguageChanged();

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

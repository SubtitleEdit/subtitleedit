using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.DoNotBreakAfterList;

/// <summary>
/// The "do not break after" list of a language in the editor look (see <see cref="EditorChrome"/>):
/// language on top, the list, the add/edit row below it, and OK in the button bar.
/// </summary>
public class DoNotBreakAfterListWindow : Window
{
    private readonly DoNotBreakAfterListViewModel _vm;

    public DoNotBreakAfterListWindow(DoNotBreakAfterListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.UseDoNotBreakAfterList;
        CanResize = true;
        Width = 520;
        Height = 600;
        MinWidth = 440;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        comboLanguages.SelectionChanged += (_, _) => vm.SelectedLanguageChanged();
        comboLanguages.MinWidth = 220;
        comboLanguages.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboLanguages, Se.Language.General.Language);
        var buttonNewLanguage = EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.General.New, vm.NewLanguageCommand);
        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { EditorChrome.MakeDimLabel(Se.Language.General.Language, new Thickness(0)), comboLanguages, buttonNewLanguage },
        };

        var listBoxItems = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Items)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedItem)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        AutomationProperties.SetName(listBoxItems, Se.Language.Options.Settings.UseDoNotBreakAfterList);

        var countBadge = EditorChrome.MakeCountBadge(new Binding(nameof(vm.Items) + ".Count") { Mode = BindingMode.OneWay }, EditorChrome.StrongTextBrush, EditorChrome.AccentSoftBrush);

        var listSection = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // language
                new RowDefinition { Height = GridLength.Auto }, // header
                new RowDefinition { Height = GridLength.Star }, // list
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        listSection.Add(panelLanguage, 0);
        listSection.Add(EditorChrome.MakeHeaderRow(EditorChrome.MakeSectionHeader(Se.Language.Options.Settings.UseDoNotBreakAfterList), countBadge), 1);
        listSection.Add(EditorChrome.MakeCompactListFrame(listBoxItems), 2);

        var textBoxItem = UiUtil.MakeTextBox(150, vm, nameof(vm.NewItemText)).AsTextField(Se.Language.General.Text);
        textBoxItem.Width = double.NaN;
        textBoxItem.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxItem.KeyDown += (_, e) => vm.ItemTextBoxKeyDown(e);
        var radioText = new RadioButton
        {
            Content = Se.Language.General.Text,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsTextItem)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        var radioRegex = new RadioButton
        {
            Content = Se.Language.General.RegularExpression,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsRegexItem)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        var buttonAdd = EditorChrome.CreateToolButton(IconNames.Plus, Se.Language.General.New, vm.AddItemCommand, 28);
        var buttonRemove = EditorChrome.CreateToolButton(IconNames.Trash, Se.Language.General.Remove, vm.RemoveItemCommand, 28);

        var panelEdit = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 6,
            RowSpacing = 6,
        };
        panelEdit.Add(textBoxItem, 0, 0);
        panelEdit.Add(buttonAdd, 0, 1);
        panelEdit.Add(buttonRemove, 0, 2);
        panelEdit.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children = { radioText, radioRegex },
        }, 1, 0, 1, 3);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        sections.Add(listSection, 0, 0);
        sections.Add(EditorChrome.MakeEditorSection(Se.Language.General.Edit, panelEdit), 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        UiUtil.FocusOnFirstActivation(this, comboLanguages); // initial focus on an input, not an action button - a focused button clicks on bare Space
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

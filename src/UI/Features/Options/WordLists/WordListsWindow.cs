using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public class WordListsWindow : Window
{
    private readonly WordListsViewModel _vm;

    public WordListsWindow(WordListsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.WordLists.Title;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 700;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language);
        var comboLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        comboLanguages.SelectionChanged += (s, e) => vm.SelectedLanguageChanged();
        var panelLanguage = UiUtil.MakeHorizontalPanel(labelLanguage, comboLanguages);

        var linkLabelOpenDictionariesFolder = UiUtil.MakeLink(Se.Language.General.OpenDictionaryFolder, vm.OpenDictionariesFolderCommand);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelLanguage, 0, 0, 1, 3);
        grid.Add(MakeViewNames(vm), 1, 0);
        grid.Add(MakeViewWords(vm), 1, 1);
        grid.Add(MakeViewOcrFixes(vm), 1, 2);
        grid.Add(linkLabelOpenDictionariesFolder, 2, 0, 1, 3);
        grid.Add(panelButtons, 3, 0, 1, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        Closing += (s, e) => _vm.Closing();
        Loaded += (s, e) => vm.Loaded();
    }

    private static Border MakeViewNames(WordListsViewModel vm)
    {
        var labelTitle = UiUtil.MakeLabel(Se.Language.Options.WordLists.NameAndIgnoreList).WithBold();
        var labelTitleBadgeCount = new Border
        {
            Background = UiUtil.GetBorderBrush(),     // badge background
            CornerRadius = new CornerRadius(10),      // makes it pill-like
            Padding = new Thickness(6, 0, 6, 0),      // spacing around text
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.Names) + ".Count") { Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() },
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.WhiteSmoke,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        var panelTitle = UiUtil.MakeHorizontalPanel(labelTitle, labelTitleBadgeCount);

        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Names)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedName)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemContainerTheme = MakeListBoxTheme()
        };

        var textBox = UiUtil.MakeTextBox(110, vm, nameof(vm.NewName));
        textBox.KeyDown += (s, e) => vm.NameTextBoxKeyDown(e);
        var buttonAdd = UiUtil.MakeButton(vm.AddNameCommand, IconNames.Plus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.RemoveNameCommand, IconNames.Trash, Se.Language.General.Remove);
        var panelButtons = UiUtil.MakeButtonBar(textBox, buttonAdd, buttonRemove).WithAlignmentLeft().WithSpacing(4);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // listbox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // add textbox + buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelTitle, 0);
        grid.Add(listBox, 1);
        grid.Add(panelButtons, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static ControlTheme MakeListBoxTheme()
    {
        return new ControlTheme(typeof(ListBoxItem))
        {
            BasedOn = Application.Current?.FindResource(typeof(ListBoxItem)) as ControlTheme,
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(1))
            }
        };
    }

    private static Border MakeViewWords(WordListsViewModel vm)
    {
        var labelTitle = UiUtil.MakeLabel(Se.Language.Options.WordLists.UserWords).WithBold();
        var labelTitleBadgeCount = new Border
        {
            Background = UiUtil.GetBorderBrush(),                // badge background
            CornerRadius = new CornerRadius(10),      // makes it pill-like
            Padding = new Thickness(6, 0, 6, 0),      // spacing around text
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.UserWords) + ".Count") { Source = vm, Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() },
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.WhiteSmoke,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        var panelTitle = UiUtil.MakeHorizontalPanel(labelTitle, labelTitleBadgeCount);


        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.UserWords)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedUserWord)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemContainerTheme = MakeListBoxTheme()
        };

        var textBox = UiUtil.MakeTextBox(110, vm, nameof(vm.NewUserWord));
        textBox.KeyDown += (s, e) => vm.UserWordTextBoxKeyDown(e);
        var buttonAdd = UiUtil.MakeButton(vm.AddWordCommand, IconNames.Plus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.RemoveWordCommand, IconNames.Trash, Se.Language.General.Remove);
        var panelButtons = UiUtil.MakeButtonBar(textBox, buttonAdd, buttonRemove).WithAlignmentLeft().WithSpacing(4);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // listbox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // add textbox + buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelTitle, 0);
        grid.Add(listBox, 1);
        grid.Add(panelButtons, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeViewOcrFixes(WordListsViewModel vm)
    {
        var labelTitle = UiUtil.MakeLabel(Se.Language.Options.WordLists.OcrFixList).WithBold();
        var labelTitleBadgeCount = new Border
        {
            Background = UiUtil.GetBorderBrush(),     // badge background
            CornerRadius = new CornerRadius(10),      // makes it pill-like
            Padding = new Thickness(6, 0, 6, 0),      // spacing around text
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.OcrFixes) + ".Count") { Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() },
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.WhiteSmoke,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        var panelTitle = UiUtil.MakeHorizontalPanel(labelTitle, labelTitleBadgeCount);

        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.OcrFixes)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedOcrFix)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemContainerTheme = MakeListBoxTheme()
        };

        var textBoxFind = UiUtil.MakeTextBox(110, vm, nameof(vm.NewOcrFixFind));
        var textBoxReplace = UiUtil.MakeTextBox(110, vm, nameof(vm.NewOcrFixReplace));
        textBoxReplace.KeyDown += (s, e) => vm.OcrFixTextBoxKeyDown(e);
        var buttonAdd = UiUtil.MakeButton(vm.AddOcrFixCommand, IconNames.Plus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.RemoveOcrFixCommand, IconNames.Trash, Se.Language.General.Remove);
        var panelButtons = UiUtil.MakeButtonBar(textBoxFind, textBoxReplace, buttonAdd, buttonRemove).WithAlignmentLeft().WithSpacing(4);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // listbox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // add textbox + buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelTitle, 0);
        grid.Add(listBox, 1);
        grid.Add(panelButtons, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

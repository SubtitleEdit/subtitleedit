using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

/// <summary>
/// Names, user words and OCR fixes of a language in the editor look (see <see cref="EditorChrome"/>):
/// language on top, the three lists side by side, each with its add row, and OK in the button bar.
/// </summary>
public class WordListsWindow : Window
{
    private readonly WordListsViewModel _vm;

    public WordListsWindow(WordListsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.WordLists.Title;
        CanResize = true;
        Width = 960;
        Height = 720;
        MinWidth = 760;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        comboLanguages.SelectionChanged += (s, e) => vm.SelectedLanguageChanged();
        comboLanguages.MinWidth = 240;
        comboLanguages.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboLanguages, Se.Language.General.Language);
        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { EditorChrome.MakeDimLabel(Se.Language.General.Language, new Thickness(0)), comboLanguages },
        };

        var lists = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
            },
        };
        lists.Add(MakeViewNames(vm), 0, 0);
        lists.Add(WithLeftLine(MakeViewWords(vm)), 0, 1);
        lists.Add(WithLeftLine(MakeViewOcrFixes(vm)), 0, 2);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            },
        };
        sections.Add(new Border
        {
            Child = panelLanguage,
            Padding = new Thickness(14, 12),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        }, 0, 0);
        sections.Add(lists, 1, 0);

        var buttonOpenFolder = EditorChrome.MakeTextToolButton(IconNames.FolderOpen, Se.Language.General.OpenDictionaryFolder, vm.OpenDictionariesFolderCommand);
        var buttonUseAlwaysList = EditorChrome.MakeTextToolButton(IconNames.FormatListChecks, Se.Language.SpellCheck.EditUseAlwaysList, vm.EditUseAlwaysListCommand);
        var panelLinks = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonOpenFolder, buttonUseAlwaysList },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(panelLinks, buttonOk));

        UiUtil.FocusOnFirstActivation(this, comboLanguages); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Closing += (s, e) => _vm.Closing();
        Loaded += (s, e) => vm.Loaded();

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border WithLeftLine(Control content)
    {
        return new Border
        {
            Child = content,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
    }

    // Section header with the list's icon on a colored rounded square plus a count badge in the
    // same color - the glyph style used by the settings and shortcuts windows.
    private static DockPanel MakePanelTitle(string title, string iconName, string colorHex, Binding countBinding)
    {
        var color = Color.Parse(colorHex);

        var icon = new ContentControl
        {
            FontSize = UiUtil.ScaledFontSize(12),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Optris.Icons.Avalonia.Attached.SetIcon(icon, iconName);
        // Keep the glyph white on the colored square in the dark theme too (#12717).
        icon.Classes.Add(UiTheme.IconOnAccentClassName);
        var glyph = new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(5),
            Background = new SolidColorBrush(color),
            VerticalAlignment = VerticalAlignment.Center,
            Child = icon,
        };

        var labelTitle = EditorChrome.MakeSectionHeader(title);
        labelTitle.TextTrimming = TextTrimming.CharacterEllipsis;
        var titlePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { glyph, labelTitle },
        };

        var badge = EditorChrome.MakeCountBadge(countBinding, new SolidColorBrush(color), new SolidColorBrush(color, 0.16));
        badge.Margin = new Thickness(6, 0, 0, 0);
        return EditorChrome.MakeHeaderRow(titlePanel, badge);
    }

    private static ListBox MakeListBox(string accessibleName, string itemsPath, string selectedPath, ICommand removeCommand)
    {
        var listBox = new ListBox
        {
            [AutomationProperties.NameProperty] = accessibleName,
            [!ListBox.ItemsSourceProperty] = new Binding(itemsPath) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(selectedPath) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsTextSearchEnabled = true, // type a letter (or a few quickly) to jump to the first matching entry (#16078)
        };

        // Right-click selects the item under the pointer, so Remove acts on the clicked entry.
        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.Remove, Command = removeCommand });
        listBox.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(listBox);

        return listBox;
    }

    // Header, list and the add row of one list - the list takes the free height.
    private static Grid MakeListSection(Control header, ListBox listBox, Control addRow)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // listbox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // add textbox + buttons
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };

        grid.Add(header, 0);
        grid.Add(EditorChrome.MakeCompactListFrame(listBox), 1);
        grid.Add(addRow, 2);
        return grid;
    }

    // Text box(es) stretched, then the add and remove icon buttons.
    private static Grid MakeAddRow(Button buttonAdd, Button buttonRemove, params TextBox[] textBoxes)
    {
        var grid = new Grid { ColumnSpacing = 6 };
        var column = 0;
        foreach (var textBox in textBoxes)
        {
            textBox.Width = double.NaN;
            textBox.MinWidth = 0;
            textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            grid.Add(textBox, 0, column++);
        }

        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.Add(buttonAdd, 0, column++);
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.Add(buttonRemove, 0, column);
        return grid;
    }

    private static Grid MakeViewNames(WordListsViewModel vm)
    {
        var panelTitle = MakePanelTitle(Se.Language.Options.WordLists.NameAndIgnoreList, IconNames.Account, "#4ec98a",
            new Binding(nameof(vm.Names) + ".Count") { Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() });

        var listBox = MakeListBox(Se.Language.Options.WordLists.NameAndIgnoreList, nameof(vm.Names), nameof(vm.SelectedName), vm.RemoveNameCommand);

        var textBox = UiUtil.MakeTextBox(110, vm, nameof(vm.NewName)).AsTextField(Se.Language.Options.WordLists.AddName);
        textBox.PlaceholderText = Se.Language.Options.WordLists.AddName;
        textBox.KeyDown += (s, e) => vm.NameTextBoxKeyDown(e);
        var buttonAdd = EditorChrome.CreateToolButton(IconNames.Plus, Se.Language.General.New, vm.AddNameCommand, 28);
        var buttonRemove = EditorChrome.CreateToolButton(IconNames.Trash, Se.Language.General.Remove, vm.RemoveNameCommand, 28);

        return MakeListSection(panelTitle, listBox, MakeAddRow(buttonAdd, buttonRemove, textBox));
    }

    private static Grid MakeViewWords(WordListsViewModel vm)
    {
        var panelTitle = MakePanelTitle(Se.Language.Options.WordLists.UserWords, IconNames.Spellcheck, "#7fa8f0",
            new Binding(nameof(vm.UserWords) + ".Count") { Source = vm, Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() });

        var listBox = MakeListBox(Se.Language.Options.WordLists.UserWords, nameof(vm.UserWords), nameof(vm.SelectedUserWord), vm.RemoveWordCommand);

        var textBox = UiUtil.MakeTextBox(110, vm, nameof(vm.NewUserWord)).AsTextField(Se.Language.General.AddToUserDictionary);
        textBox.PlaceholderText = Se.Language.General.AddToUserDictionary;
        textBox.KeyDown += (s, e) => vm.UserWordTextBoxKeyDown(e);
        var buttonAdd = EditorChrome.CreateToolButton(IconNames.Plus, Se.Language.General.New, vm.AddWordCommand, 28);
        var buttonRemove = EditorChrome.CreateToolButton(IconNames.Trash, Se.Language.General.Remove, vm.RemoveWordCommand, 28);

        return MakeListSection(panelTitle, listBox, MakeAddRow(buttonAdd, buttonRemove, textBox));
    }

    private static Grid MakeViewOcrFixes(WordListsViewModel vm)
    {
        var panelTitle = MakePanelTitle(Se.Language.Options.WordLists.OcrFixList, IconNames.Ocr, "#f0885a",
            new Binding(nameof(vm.OcrFixes) + ".Count") { Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() });

        var listBox = MakeListBox(Se.Language.Options.WordLists.OcrFixList, nameof(vm.OcrFixes), nameof(vm.SelectedOcrFix), vm.RemoveOcrFixCommand);

        var textBoxFind = UiUtil.MakeTextBox(110, vm, nameof(vm.NewOcrFixFind)).AsTextField(Se.Language.General.Find);
        textBoxFind.PlaceholderText = Se.Language.General.Find;
        var textBoxReplace = UiUtil.MakeTextBox(110, vm, nameof(vm.NewOcrFixReplace)).AsTextField(Se.Language.General.ReplaceWith);
        textBoxReplace.PlaceholderText = Se.Language.General.ReplaceWith;
        textBoxReplace.KeyDown += (s, e) => vm.OcrFixTextBoxKeyDown(e);
        var buttonAdd = EditorChrome.CreateToolButton(IconNames.Plus, Se.Language.General.New, vm.AddOcrFixCommand, 28);
        var buttonRemove = EditorChrome.CreateToolButton(IconNames.Trash, Se.Language.General.Remove, vm.RemoveOcrFixCommand, 28);

        return MakeListSection(panelTitle, listBox, MakeAddRow(buttonAdd, buttonRemove, textBoxFind, textBoxReplace));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

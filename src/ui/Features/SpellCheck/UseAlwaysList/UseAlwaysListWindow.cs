using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.SpellCheck.UseAlwaysList;

/// <summary>
/// The spell check "use always" list in the editor look (see <see cref="EditorChrome"/>): the
/// pairs with language, search and counts above them, the add/edit row below, and Cancel + OK in
/// the button bar.
/// </summary>
public class UseAlwaysListWindow : Window
{
    private const string WarningColor = "#e0a030";

    private readonly UseAlwaysListViewModel _vm;

    public UseAlwaysListWindow(UseAlwaysListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.UseAlwaysListTitle;
        CanResize = true;
        Width = 760;
        Height = 640;
        MinWidth = 560;
        MinHeight = 420;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        comboLanguages.MinWidth = 220;
        comboLanguages.MaxWidth = 320;
        comboLanguages.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboLanguages, Se.Language.General.Language);

        var textBoxSearch = new TextBox
        {
            PlaceholderText = Se.Language.General.Search,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SearchText)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [AutomationProperties.NameProperty] = Se.Language.General.Search,
        }.WithSearchAndClearIcons();
        textBoxSearch.Classes.Add("text");

        var toolbar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            },
            ColumnSpacing = 10,
        };
        toolbar.Add(comboLanguages, 0, 0);
        toolbar.Add(textBoxSearch, 0, 1);

        var header = new DockPanel();
        var badges = MakeBadges(vm);
        DockPanel.SetDock(badges, Dock.Right);
        header.Children.Add(badges);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.SpellCheck.UseAlwaysListHeader));

        var description = EditorChrome.MakeWrappingDimLabel(Se.Language.SpellCheck.UseAlwaysListDescription);
        description.FontSize = 12.5;

        var listSection = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // header + badges
                new RowDefinition { Height = GridLength.Auto }, // description
                new RowDefinition { Height = GridLength.Auto }, // "remember" off warning
                new RowDefinition { Height = GridLength.Auto }, // language + search
                new RowDefinition { Height = GridLength.Star }, // pairs
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        listSection.Add(header, 0, 0);
        listSection.Add(description, 1, 0);
        listSection.Add(MakeRememberOffBanner(vm), 2, 0);
        listSection.Add(toolbar, 3, 0);
        listSection.Add(MakePairsView(vm), 4, 0);

        var editSection = EditorChrome.MakeEditorSection(Se.Language.General.Edit, MakeEditRow(vm));

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        sections.Add(listSection, 0, 0);
        sections.Add(editSection, 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonOpenFolder = EditorChrome.MakeTextToolButton(IconNames.FolderOpen, Se.Language.General.OpenDictionaryFolder, vm.OpenDictionariesFolderCommand);
        buttonOpenFolder.HorizontalAlignment = HorizontalAlignment.Left;

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(buttonOpenFolder, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxSearch); // an input, not a button - a focused button clicks on bare Space
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Control MakeIcon(string iconName, double fontSize, IBrush? foreground = null, double opacity = 1.0)
    {
        var icon = new ContentControl
        {
            FontSize = UiUtil.ScaledFontSize(fontSize),
            Opacity = opacity,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (foreground != null)
        {
            icon.Foreground = foreground;
        }

        Attached.SetIcon(icon, iconName);
        return icon;
    }

    private static void SetHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(control, hint);
        }
    }

    private static Control MakeRememberOffBanner(UseAlwaysListViewModel vm)
    {
        var warning = new SolidColorBrush(Color.Parse(WarningColor));
        var banner = new Border
        {
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.Parse(WarningColor), 0.12),
            BorderBrush = new SolidColorBrush(Color.Parse(WarningColor), 0.45),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 7),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    MakeIcon(IconNames.Alert, 15, warning),
                    new TextBlock
                    {
                        Text = Se.Language.SpellCheck.UseAlwaysRememberOff,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12.5,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            },
        };
        banner.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsRememberOff)));
        return banner;
    }

    private static Border MakeBadge(string textPropertyPath, IBrush foreground, IBrush background, string? iconName)
    {
        var text = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = foreground,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(textPropertyPath),
        };

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        if (iconName != null)
        {
            panel.Children.Add(MakeIcon(iconName, 11, foreground));
        }

        panel.Children.Add(text);

        return new Border
        {
            Background = background,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 2, 9, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Child = panel,
        };
    }

    private static Control MakeBadges(UseAlwaysListViewModel vm)
    {
        var warningColor = Color.Parse(WarningColor);
        var unusedBadge = MakeBadge(nameof(vm.UnusedText), new SolidColorBrush(warningColor), new SolidColorBrush(warningColor, 0.16), IconNames.Alert);
        unusedBadge.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.HasUnused)));
        SetHint(unusedBadge, Se.Language.SpellCheck.UseAlwaysUnusedHint);

        var buttonRemoveUnused = EditorChrome.MakeTextToolButton(IconNames.Trash, Se.Language.SpellCheck.RemoveUnused, vm.RemoveUnusedCommand)
            .WithBindIsVisible(nameof(vm.HasUnused));
        buttonRemoveUnused.Height = 24;
        buttonRemoveUnused.VerticalAlignment = VerticalAlignment.Center;
        SetHint(buttonRemoveUnused, Se.Language.SpellCheck.UseAlwaysUnusedHint);

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeBadge(nameof(vm.CountText), EditorChrome.StrongTextBrush, EditorChrome.AccentSoftBrush, null),
                unusedBadge,
                buttonRemoveUnused,
            },
        };
    }

    private static Control MakePairsView(UseAlwaysListViewModel vm)
    {
        var table = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        table.Height = double.NaN;
        table.ItemsSource = vm.Pairs;
        AutomationProperties.SetName(table, Se.Language.SpellCheck.UseAlwaysListHeader);

        var warning = new SolidColorBrush(Color.Parse(WarningColor));

        table.Columns.Add(new SeTableViewColumn
        {
            Header = string.Empty,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(34),
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((_, _) =>
            {
                var icon = MakeIcon(IconNames.Alert, 15, warning);
                icon.Bind(Visual.IsVisibleProperty, new Binding(nameof(UseAlwaysPairItem.IsUnused)));
                SetHint(icon, Se.Language.SpellCheck.UseAlwaysUnusedHint);
                AutomationProperties.SetName(icon, Se.Language.SpellCheck.UseAlwaysUnusedHint);
                return icon;
            }),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.SpellCheck.UseAlwaysWord,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                [!TextBlock.TextProperty] = new Binding(nameof(UseAlwaysPairItem.From)),
                [!TextBlock.OpacityProperty] = new Binding(nameof(UseAlwaysPairItem.IsUnused))
                {
                    Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, double>(unused => unused ? 0.6 : 1.0),
                },
            }),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = string.Empty,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(34),
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((_, _) => MakeIcon(IconNames.ArrowRightThick, 14, EditorChrome.FaintTextBrush)),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.ReplaceWith,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontWeight = FontWeight.SemiBold,
                [!TextBlock.TextProperty] = new Binding(nameof(UseAlwaysPairItem.To)),
            }),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = string.Empty,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(40),
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((item, _) =>
            {
                var button = EditorChrome.CreateToolButton(IconNames.Trash, Se.Language.General.Remove, vm.RemoveCommand, 26);
                button.CommandParameter = item;
                button.HorizontalAlignment = HorizontalAlignment.Center;
                button.VerticalAlignment = VerticalAlignment.Center;
                return button;
            }),
        });

        table.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedPair)) { Source = vm, Mode = BindingMode.TwoWay });
        table.KeyDown += (_, e) => vm.GridKeyDown(e);

        var emptyState = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(24),
            Children =
            {
                MakeIcon(IconNames.FormatListChecks, 40, EditorChrome.FaintTextBrush),
                new TextBlock
                {
                    Text = Se.Language.SpellCheck.UseAlwaysEmpty,
                    FontSize = 14,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                },
                new TextBlock
                {
                    Text = Se.Language.SpellCheck.UseAlwaysEmptyHint,
                    FontSize = 12.5,
                    Foreground = EditorChrome.DimTextBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                },
            },
        };
        emptyState.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEmpty)));

        var noMatch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeIcon(IconNames.Find, 18, EditorChrome.FaintTextBrush),
                new TextBlock { Text = Se.Language.SpellCheck.UseAlwaysNoMatch, FontSize = 12.5, Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center },
            },
        };
        noMatch.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsNoMatch)));

        var panel = new Panel { Children = { table, emptyState, noMatch } };
        return EditorChrome.MakeListFrame(panel, clearListBackground: true);
    }

    private static Control MakeEditRow(UseAlwaysListViewModel vm)
    {
        var textBoxFrom = new TextBox
        {
            PlaceholderText = Se.Language.SpellCheck.UseAlwaysWord,
            [!TextBox.TextProperty] = new Binding(nameof(vm.EditFrom)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [AutomationProperties.NameProperty] = Se.Language.SpellCheck.UseAlwaysWord,
        };
        textBoxFrom.Classes.Add("text");
        textBoxFrom.KeyDown += (_, e) => vm.EditTextBoxKeyDown(e);

        var textBoxTo = new TextBox
        {
            PlaceholderText = Se.Language.General.ReplaceWith,
            [!TextBox.TextProperty] = new Binding(nameof(vm.EditTo)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [AutomationProperties.NameProperty] = Se.Language.General.ReplaceWith,
        };
        textBoxTo.Classes.Add("text");
        textBoxTo.KeyDown += (_, e) => vm.EditTextBoxKeyDown(e);

        // Same command, but a pencil instead of a plus when the word already has a pair.
        var buttonAdd = EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.General.Add, vm.AddOrUpdateCommand);
        buttonAdd.VerticalAlignment = VerticalAlignment.Center;
        buttonAdd.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEditingExisting)) { Converter = Avalonia.Data.Converters.BoolConverters.Not });

        var buttonUpdate = EditorChrome.MakeTextToolButton(IconNames.Pencil, Se.Language.General.Update, vm.AddOrUpdateCommand)
            .WithBindIsVisible(nameof(vm.IsEditingExisting));
        buttonUpdate.VerticalAlignment = VerticalAlignment.Center;

        var panelAddOrUpdate = new Panel { Children = { buttonAdd, buttonUpdate } };

        var buttonNew = EditorChrome.CreateToolButton(IconNames.NewText, Se.Language.General.New, vm.NewCommand, 28);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 8,
        };
        grid.Add(textBoxFrom, 0, 0);
        grid.Add(MakeIcon(IconNames.ArrowRightThick, 16, EditorChrome.FaintTextBrush), 0, 1);
        grid.Add(textBoxTo, 0, 2);
        grid.Add(panelAddOrUpdate, 0, 3);
        grid.Add(buttonNew, 0, 4);
        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

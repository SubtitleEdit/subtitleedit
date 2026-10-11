using Avalonia;
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

public class UseAlwaysListWindow : Window
{
    private const string AccentColor = "#7fa8f0"; // same blue as the spell check list in Word lists
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

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        comboLanguages.MinWidth = 220;
        comboLanguages.MaxWidth = 320;
        Avalonia.Automation.AutomationProperties.SetName(comboLanguages, Se.Language.General.Language);

        var textBoxSearch = new TextBox
        {
            PlaceholderText = Se.Language.General.Search,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SearchText)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.General.Search,
        }.WithSearchAndClearIcons();

        var toolbar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 10,
        };
        toolbar.Add(comboLanguages, 0, 0);
        toolbar.Add(textBoxSearch, 0, 1);
        toolbar.Add(MakeBadges(vm), 0, 2);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        var linkOpenFolder = UiUtil.MakeLink(Se.Language.General.OpenDictionaryFolder, vm.OpenDictionariesFolderCommand);
        linkOpenFolder.VerticalAlignment = VerticalAlignment.Center;
        footer.Add(linkOpenFolder, 0, 0);
        footer.Add(UiUtil.MakeButtonBar(buttonOk, buttonCancel), 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // header
                new RowDefinition { Height = GridLength.Auto }, // "remember" off warning
                new RowDefinition { Height = GridLength.Auto }, // language + search + badges
                new RowDefinition { Height = GridLength.Star }, // pairs
                new RowDefinition { Height = GridLength.Auto }, // add / edit card
                new RowDefinition { Height = GridLength.Auto }, // footer
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 12,
        };

        grid.Add(MakeHeader(), 0, 0);
        grid.Add(MakeRememberOffBanner(vm), 1, 0);
        grid.Add(toolbar, 2, 0);
        grid.Add(MakePairsView(vm), 3, 0);
        grid.Add(MakeEditCard(vm), 4, 0);
        grid.Add(footer, 5, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxSearch); // an input, not a button - a focused button clicks on bare Space
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Control MakeGlyph(string iconName, string colorHex, double size, double iconFontSize)
    {
        var icon = new ContentControl
        {
            FontSize = UiUtil.ScaledFontSize(iconFontSize),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Attached.SetIcon(icon, iconName);
        icon.Classes.Add(UiTheme.IconOnAccentClassName);

        return new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(size / 4),
            Background = new SolidColorBrush(Color.Parse(colorHex)),
            VerticalAlignment = VerticalAlignment.Top,
            Child = icon,
        };
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

    private static Control MakeHeader()
    {
        var title = new TextBlock
        {
            Text = Se.Language.SpellCheck.UseAlwaysListHeader,
            FontSize = UiUtil.ScaledFontSize(17),
            FontWeight = FontWeight.SemiBold,
        };

        var description = new TextBlock
        {
            Text = Se.Language.SpellCheck.UseAlwaysListDescription,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.75,
        };

        var texts = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 3,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { title, description },
        };

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            },
            ColumnSpacing = 14,
        };
        header.Add(MakeGlyph(IconNames.Spellcheck, AccentColor, 40, 22), 0, 0);
        header.Add(texts, 0, 1);
        return header;
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
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            },
        };
        banner.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsRememberOff)));
        return banner;
    }

    private static Border MakeBadge(string textPropertyPath, string colorHex, string? iconName)
    {
        var color = Color.Parse(colorHex);
        var foreground = new SolidColorBrush(color);
        var text = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11),
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
            Background = new SolidColorBrush(color, 0.16),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 3, 9, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Child = panel,
        };
    }

    private static Control MakeBadges(UseAlwaysListViewModel vm)
    {
        var unusedBadge = MakeBadge(nameof(vm.UnusedText), WarningColor, IconNames.Alert);
        unusedBadge.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.HasUnused)));
        ToolTip.SetTip(unusedBadge, Se.Language.SpellCheck.UseAlwaysUnusedHint);

        var buttonRemoveUnused = new Button
        {
            Content = Se.Language.SpellCheck.RemoveUnused,
            Command = vm.RemoveUnusedCommand,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.Trash).WithBindIsVisible(nameof(vm.HasUnused));
        ToolTip.SetTip(buttonRemoveUnused, Se.Language.SpellCheck.UseAlwaysUnusedHint);

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeBadge(nameof(vm.CountText), AccentColor, null),
                unusedBadge,
                buttonRemoveUnused,
            },
        };
    }

    private static Control MakePairsView(UseAlwaysListViewModel vm)
    {
        var table = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        table.Height = double.NaN;
        table.Margin = new Thickness(2);
        table.ItemsSource = vm.Pairs;
        Avalonia.Automation.AutomationProperties.SetName(table, Se.Language.SpellCheck.UseAlwaysListHeader);

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
                ToolTip.SetTip(icon, Se.Language.SpellCheck.UseAlwaysUnusedHint);
                Avalonia.Automation.AutomationProperties.SetName(icon, Se.Language.SpellCheck.UseAlwaysUnusedHint);
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
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((_, _) => MakeIcon(IconNames.ArrowRightThick, 14, null, 0.45)),
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
            Width = new GridLength(46),
            CellTemplate = new FuncDataTemplate<UseAlwaysPairItem>((item, _) =>
            {
                var button = UiUtil.MakeButton(vm.RemoveCommand, IconNames.Trash, Se.Language.General.Remove);
                button.CommandParameter = item;
                button.Margin = new Thickness(2);
                button.Padding = new Thickness(6, 2);
                button.Opacity = 0.75;
                button.HorizontalAlignment = HorizontalAlignment.Center;
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
                MakeIcon(IconNames.FormatListChecks, 40, null, 0.35),
                new TextBlock
                {
                    Text = Se.Language.SpellCheck.UseAlwaysEmpty,
                    FontSize = UiUtil.ScaledFontSize(15),
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                },
                new TextBlock
                {
                    Text = Se.Language.SpellCheck.UseAlwaysEmptyHint,
                    Opacity = 0.65,
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
                MakeIcon(IconNames.Find, 18, null, 0.45),
                new TextBlock { Text = Se.Language.SpellCheck.UseAlwaysNoMatch, Opacity = 0.65, VerticalAlignment = VerticalAlignment.Center },
            },
        };
        noMatch.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsNoMatch)));

        var panel = new Panel { Children = { table, emptyState, noMatch } };
        return UiUtil.MakeBorderForControlNoPadding(panel);
    }

    private static Control MakeEditCard(UseAlwaysListViewModel vm)
    {
        var textBoxFrom = new TextBox
        {
            PlaceholderText = Se.Language.SpellCheck.UseAlwaysWord,
            [!TextBox.TextProperty] = new Binding(nameof(vm.EditFrom)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.SpellCheck.UseAlwaysWord,
        };
        textBoxFrom.KeyDown += (_, e) => vm.EditTextBoxKeyDown(e);

        var textBoxTo = new TextBox
        {
            PlaceholderText = Se.Language.General.ReplaceWith,
            [!TextBox.TextProperty] = new Binding(nameof(vm.EditTo)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.General.ReplaceWith,
        };
        textBoxTo.KeyDown += (_, e) => vm.EditTextBoxKeyDown(e);

        // Same command, but a pencil instead of a plus when the word already has a pair.
        var buttonAdd = new Button
        {
            Content = Se.Language.General.Add,
            Command = vm.AddOrUpdateCommand,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.Plus);
        buttonAdd.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEditingExisting)) { Converter = Avalonia.Data.Converters.BoolConverters.Not });

        var buttonUpdate = new Button
        {
            Content = Se.Language.General.Update,
            Command = vm.AddOrUpdateCommand,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.Pencil).WithBindIsVisible(nameof(vm.IsEditingExisting));

        var panelAddOrUpdate = new Panel { Children = { buttonAdd, buttonUpdate } };

        var buttonNew = UiUtil.MakeButton(vm.NewCommand, IconNames.NewText, Se.Language.General.New);

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
        grid.Add(MakeIcon(IconNames.ArrowRightThick, 16, null, 0.5), 0, 1);
        grid.Add(textBoxTo, 0, 2);
        grid.Add(panelAddOrUpdate, 0, 3);
        grid.Add(buttonNew, 0, 4);

        return new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromArgb(18, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 10),
            Child = grid,
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

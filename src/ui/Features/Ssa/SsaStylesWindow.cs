using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ssa;

/// <summary>
/// SSA styles, styled like ASSA draw and the ASSA styles window: the style lists (in the file,
/// saved) on the left, the preview on the right with the style editor under it.
/// </summary>
public class SsaStylesWindow : Window
{
    private const string Current = nameof(SsaStylesViewModel.CurrentStyle) + ".";
    private static readonly IBrush DefaultStarBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));

    // Which list the edited style comes from: file styles in the editor accent, saved styles in amber
    private static readonly IBrush SavedBrush = new SolidColorBrush(Color.FromRgb(240, 168, 48));
    private static readonly IBrush SavedSoftBrush = new SolidColorBrush(Color.FromArgb(46, 240, 168, 48));

    public SsaStylesWindow(SsaStylesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 1400;
        Height = 800;
        MinWidth = 1000;
        MinHeight = 600;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        // Lists on the left, preview + editor on the right, a splitter between them (#16251)
        var content = new Grid();

        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        right.Add(MakePreview(vm), 0, 0);
        right.Add(MakeEditingBanner(vm), 1, 0);
        right.Add(MakeEditorPanel(vm), 2, 0);

        StylesDialogHelper.AddListsSplitter(this, content, MakeListsPanel(vm), right);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        mainGrid.Add(content, 0, 0);
        mainGrid.Add(MakeButtonBar(vm), 1, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.FileStyleGrid); });
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>
    /// Toggle buttons (B/I) and the alignment cells, in the editor accent.
    /// </summary>
    private static void AddStyles(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(ToggleButton.BackgroundProperty, Brushes.Transparent),
                new Setter(ToggleButton.BorderThicknessProperty, new Thickness(0)),
                new Setter(ToggleButton.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(ToggleButton.ForegroundProperty, EditorChrome.IconBrush),
                new Setter(ToggleButton.PaddingProperty, new Thickness(0)),
            },
        });
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.AccentBrush),
                new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
            },
        });

        styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Template().OfType<Border>())
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(Border.BorderBrushProperty, EditorChrome.FieldBorderBrush),
            },
        });
        styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Class(":pointerover").Template().OfType<Border>())
        {
            Setters = { new Setter(Border.BackgroundProperty, EditorChrome.HoverBrush) },
        });
        styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Class(":checked").Template().OfType<Border>())
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, EditorChrome.AccentBrush),
                new Setter(Border.BorderBrushProperty, EditorChrome.AccentBrush),
            },
        });
        styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Template().OfType<Ellipse>())
        {
            Setters = { new Setter(Shape.FillProperty, EditorChrome.FaintTextBrush) },
        });
        styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Class(":checked").Template().OfType<Ellipse>())
        {
            Setters = { new Setter(Shape.FillProperty, Brushes.White) },
        });
    }

    private static Border MakeListsPanel(SsaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(MakeFileStyles(vm), 0, 0);
        grid.Add(MakeStorageStyles(vm), 1, 0);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    /// <summary>
    /// "⋯" button: a menu built when it opens, so the items reflect the current selection.
    /// </summary>
    private static Button MakeMoreButton(Func<List<Control>> items)
    {
        var button = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.More, null, 24);
        var menu = new ContextMenu { Placement = PlacementMode.BottomEdgeAlignedRight };
        button.Click += (_, _) =>
        {
            menu.ItemsSource = items();
            menu.Open(button);
        };
        return button;
    }

    private static MenuItem MenuItem(string header, string? icon, System.Windows.Input.ICommand command, bool isEnabled = true)
    {
        var item = EditorChrome.MakeMenuItem(header, icon, command);
        item.IsEnabled = isEnabled;
        return item;
    }

    /// <summary>
    /// A list section: title, the actions under it, then the table filling the rest.
    /// </summary>
    private static Grid MakeListSection(string title, Control actions, TableView table, bool first)
    {
        actions.HorizontalAlignment = HorizontalAlignment.Left;
        actions.Margin = new Thickness(0, 6, 0, 0);

        var header = new StackPanel { Margin = new Thickness(14, first ? 10 : 12, 10, 8) };
        var titleText = EditorChrome.MakeSectionHeader(title);
        titleText.Bind(TextBlock.ForegroundProperty, new Binding(nameof(SsaStylesViewModel.CurrentTitle))
        {
            Converter = new FuncValueConverter<string, IBrush>(current => current == title ? (title == Se.Language.Assa.StylesSaved ? SavedBrush : EditorChrome.AccentBrush) : EditorChrome.DimTextBrush),
        });
        header.Children.Add(titleText);
        header.Children.Add(actions);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(new Border
        {
            Child = header,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
        }, 0, 0);

        table.Margin = new Thickness(8, 0, 8, 8);
        grid.Add(table, 1, 0);
        return grid;
    }

    private static SeTableViewColumn Column(string header, string property, GridLength width) => new()
    {
        Header = header,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        Binding = new Binding(property),
        Width = width,
    };

    private static void AddMenuItem(MenuFlyout flyout, SsaStylesViewModel vm, string header, System.Windows.Input.ICommand command, string isVisibleProperty)
    {
        var item = new MenuItem { Header = header, DataContext = vm, Command = command };
        item.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Binding(isVisibleProperty) { Source = vm });
        flyout.Items.Add(item);
    }

    private static Grid MakeFileStyles(SsaStylesViewModel vm)
    {
        // No header sorting: SSA styles are written to the file header in list
        // order on OK, so the collection order is not presentation-only.
        var dataGrid = TableViewExtras.MakeTableView();
        AutomationProperties.SetName(dataGrid, Se.Language.Assa.StylesInFile);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.FileStyles;
        dataGrid.Columns.Add(Column(Se.Language.General.Name, nameof(StyleDisplay.Name), new GridLength(150)));
        dataGrid.Columns.Add(StylesDialogHelper.MakeFontNameColumn(110));
        dataGrid.Columns.Add(Column(Se.Language.General.FontSize, nameof(StyleDisplay.FontSize), new GridLength(70)));
        dataGrid.Columns.Add(Column(Se.Language.General.Usages, nameof(StyleDisplay.UsageCount), new GridLength(70)));
        dataGrid.Columns.Add(StylesDialogHelper.MakeColorSampleColumn());

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedFileStyle)) { Source = vm });
        dataGrid.SelectionChanged += vm.FileStylesChanged;
        dataGrid.GotFocus += vm.FileStylesGotFocus;
        dataGrid.KeyDown += vm.FileStylesKeyDown;
        dataGrid.AddHandler(InputElement.KeyDownEvent, vm.FileStylesMoveKeyDown, RoutingStrategies.Tunnel);
        TableViewExtras.AttachListNavigation(dataGrid);
        vm.FileStyleGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.FilesContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        AddMenuItem(flyout, vm, Se.Language.Assa.CopyToStorageStyles, vm.FileCopyToStorageCommand, nameof(vm.IsFileStyleSelected));
        AddMenuItem(flyout, vm, Se.Language.General.Delete, vm.FileRemoveCommand, nameof(vm.IsDeleteVisible));
        AddMenuItem(flyout, vm, Se.Language.General.Clear, vm.FileRemoveAllCommand, nameof(vm.IsDeleteAllVisible));
        AddMenuItem(flyout, vm, Se.Language.Assa.TakeUsagesFromDotDotDot, vm.FileTakeUsagesFromCommand, nameof(vm.IsTakeUsagesFromVisible));
        AddMenuItem(flyout, vm, Se.Language.Assa.ReplaceStyleWithDotDotDot, vm.FileReplaceWithCommand, nameof(vm.IsFileStyleSelected));
        StylesDialogHelper.AddMoveMenuItems(flyout, vm, nameof(vm.IsMoveVisible), vm.FileMoveUpCommand, vm.FileMoveDownCommand, vm.FileMoveToTopCommand, vm.FileMoveToBottomCommand);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.New, vm.FileNewCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-regular fa-clone", Se.Language.General.Duplicate, vm.FilesDuplicateCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.FileRemoveCommand, 24));
        actions.Children.Add(MakeMoreButton(() =>
        [
            MenuItem(Se.Language.Assa.CopyToStorageStyles, "fa-solid fa-arrow-down", vm.FileCopyToStorageCommand, vm.IsFileStyleSelected),
            new Separator(),
            MenuItem(Se.Language.General.Import, "fa-solid fa-file-import", vm.FileImportCommand),
            MenuItem(Se.Language.General.Export, "fa-solid fa-file-export", vm.FileExportCommand),
            new Separator(),
            MenuItem(Se.Language.Assa.TakeUsagesFromDotDotDot, null, vm.FileTakeUsagesFromCommand, vm.IsFileStyleSelected),
            MenuItem(Se.Language.Assa.ReplaceStyleWithDotDotDot, null, vm.FileReplaceWithCommand, vm.IsFileStyleSelected),
        ]));

        return MakeListSection(Se.Language.Assa.StylesInFile, actions, dataGrid, first: true);
    }

    private static Grid MakeStorageStyles(SsaStylesViewModel vm)
    {
        // No header sorting: the storage style order is persisted to settings in
        // list order on OK, so the collection order is not presentation-only.
        var dataGrid = TableViewExtras.MakeTableView();
        AutomationProperties.SetName(dataGrid, Se.Language.Assa.StylesSaved);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.StorageStyles;
        dataGrid.Columns.Add(Column(Se.Language.General.Name, nameof(StyleDisplay.Name), new GridLength(1, GridUnitType.Star)));
        dataGrid.Columns.Add(StylesDialogHelper.MakeFontNameColumn());
        dataGrid.Columns.Add(Column(Se.Language.General.FontSize, nameof(StyleDisplay.FontSize), new GridLength(70)));
        dataGrid.Columns.Add(StylesDialogHelper.MakeColorSampleColumn());

        // The default style gets a yellow star instead of "True"/"False"
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.IsDefault,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<StyleDisplay>((_, _) => new Optris.Icons.Avalonia.Icon
            {
                Value = "fa-solid fa-star",
                FontSize = 13,
                Foreground = DefaultStarBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(StyleDisplay.IsDefault)),
            }),
            NameBinding = new Binding(nameof(StyleDisplay.IsDefault))
            {
                Converter = new FuncValueConverter<bool, string>(isDefault => isDefault ? Se.Language.General.IsDefault : string.Empty),
            },
            Width = new GridLength(44),
        });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedStorageStyle)) { Source = vm });
        dataGrid.SelectionChanged += vm.StorageStylesChanged;
        dataGrid.GotFocus += vm.StorageStylesGotFocus;
        dataGrid.KeyDown += vm.StorageStylesKeyDown;
        dataGrid.AddHandler(InputElement.KeyDownEvent, vm.StorageStylesMoveKeyDown, RoutingStrategies.Tunnel);
        vm.StorageStyleGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.StoreContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        AddMenuItem(flyout, vm, Se.Language.Assa.CopyToFileStyles, vm.StorageCopyToFilesCommand, nameof(vm.IsCopyToFileStylesVisible));
        AddMenuItem(flyout, vm, Se.Language.General.Delete, vm.StorageRemoveCommand, nameof(vm.IsDeleteVisible));
        AddMenuItem(flyout, vm, Se.Language.General.Clear, vm.StorageRemoveAllCommand, nameof(vm.IsDeleteAllVisible));
        AddMenuItem(flyout, vm, Se.Language.Assa.SetStyleAsDefault, vm.StorageSetDefaultCommand, nameof(vm.IsSetStyleAsDefaultVisible));
        StylesDialogHelper.AddMoveMenuItems(flyout, vm, nameof(vm.IsMoveVisible), vm.StorageMoveUpCommand, vm.StorageMoveDownCommand, vm.StorageMoveToTopCommand, vm.StorageMoveToBottomCommand);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.Assa.CopyToFileStyles, vm.StorageCopyToFilesCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.New, vm.StorageNewCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.StorageRemoveCommand, 24));
        actions.Children.Add(MakeMoreButton(() =>
        [
            MenuItem(Se.Language.General.Duplicate, "fa-regular fa-clone", vm.StorageDuplicateCommand, vm.IsStorageStyleSelected),
            MenuItem(Se.Language.Assa.SetStyleAsDefault, "fa-solid fa-star", vm.StorageSetDefaultCommand, vm.IsStorageStyleSelected),
            new Separator(),
            MenuItem(Se.Language.General.Import, "fa-solid fa-file-import", vm.StorageImportCommand),
            MenuItem(Se.Language.General.Export, "fa-solid fa-file-export", vm.StorageExportCommand),
        ]));

        return MakeListSection(Se.Language.Assa.StylesSaved, actions, dataGrid, first: false);
    }

    /// <summary>
    /// The style editor for "[V4 Styles]". Underline, strikeout, scale, spacing and angle are
    /// deliberately absent: a Sub Station Alpha v4 style line has no such fields, so editing them
    /// here would only have thrown the values away on save (#13734). Use .ass for those.
    /// </summary>
    private static Border MakeEditorPanel(SsaStylesViewModel vm)
    {
        StackPanel Column(params Border[] sections)
        {
            var column = new StackPanel();
            foreach (var section in sections)
            {
                column.Children.Add(section);
            }

            return column;
        }

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        columns.Add(Column(MakeNameSection(), MakeFontSection(vm)), 0, 0);
        columns.Add(new Border
        {
            Child = Column(MakeColorsSection(vm), MakeBorderSection(vm)),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0),
        }, 0, 1);
        columns.Add(Column(MakeAlignmentSection()), 0, 2);

        return new Border
        {
            Child = columns,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            [!Border.IsEnabledProperty] = new Binding(nameof(vm.CurrentStyle)) { Converter = ObjectConverters.IsNotNull },
        };
    }

    private static NumericUpDown Field(string property, double min, double max, string prefix, string name, string format = "0.#", decimal increment = 1)
    {
        var field = EditorChrome.MakeField(Current + property, min, max, prefix: prefix, format: format);
        field.Increment = increment;
        AutomationProperties.SetName(field, name);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(field, name);
        }

        return field;
    }

    /// <summary>
    /// What is being edited, above the editor: "Editing Default" with an "In file" (blue) or
    /// "Saved" (amber) badge - changing a saved style does not change the subtitle, and the
    /// other way round, so this has to be hard to miss.
    /// </summary>
    private static Border MakeEditingBanner(SsaStylesViewModel vm)
    {
        Binding Pick(object file, object saved) => new Binding(nameof(vm.CurrentTitle))
        {
            Converter = new FuncValueConverter<string, object>(title => title == Se.Language.Assa.StylesSaved ? saved : file),
        };

        var icon = new Optris.Icons.Avalonia.Icon { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        icon.Bind(Optris.Icons.Avalonia.Icon.ValueProperty, Pick("fa-solid fa-file-lines", "fa-solid fa-bookmark"));
        icon.Bind(Optris.Icons.Avalonia.Icon.ForegroundProperty, Pick(EditorChrome.AccentBrush, SavedBrush));

        var editing = new TextBlock
        {
            Text = Se.Language.Assa.StylesEditing,
            FontSize = 12.5,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var name = new TextBlock
        {
            FontSize = 13.5,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(Current + nameof(StyleDisplay.Name)),
        };
        var badgeText = new TextBlock
        {
            FontSize = 11.5,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CurrentTitle)),
        };
        var badge = new Border
        {
            Child = badgeText,
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(9, 2),
            VerticalAlignment = VerticalAlignment.Center,
        };
        badge.Bind(Border.BackgroundProperty, Pick(EditorChrome.AccentBrush, SavedBrush));
        badgeText.Bind(TextBlock.ForegroundProperty, Pick(Brushes.White, new SolidColorBrush(Color.FromRgb(60, 38, 0))));

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(icon);
        row.Children.Add(editing);
        row.Children.Add(name);
        row.Children.Add(badge);

        var banner = new Border
        {
            Child = row,
            Padding = new Thickness(14, 8),
            BorderThickness = new Thickness(3, 1, 0, 0),
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.CurrentStyle)) { Converter = ObjectConverters.IsNotNull },
        };
        banner.Bind(Border.BackgroundProperty, Pick(EditorChrome.AccentSoftBrush, SavedSoftBrush));
        banner.Bind(Border.BorderBrushProperty, Pick(EditorChrome.AccentBrush, SavedBrush));
        AutomationProperties.SetName(banner, Se.Language.Assa.StylesEditing);
        return banner;
    }

    private static Border MakeNameSection()
    {
        var name = new TextBox
        {
            FontSize = 12.5,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding(Current + nameof(StyleDisplay.Name)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(name, Se.Language.General.Name);

        return EditorChrome.MakeEditorSection(Se.Language.General.Name, name, first: true);
    }

    private static Border MakeFontSection(SsaStylesViewModel vm)
    {
        var fontName = UiUtil.MakeComboBox(vm.Fonts, vm, Current + nameof(StyleDisplay.FontName));
        fontName.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(fontName, Se.Language.General.FontName);
        var pick = EditorChrome.CreateToolButton("fa-solid fa-magnifying-glass", Se.Language.Tools.PickFontNameTitle, vm.PickFontNameCommand, 28);
        var attachments = EditorChrome.CreateToolButton("fa-solid fa-paperclip", Se.Language.Assa.Attachments, vm.BrowseFontNameCommand, 28);
        var fontRow = new Grid { ColumnSpacing = 4, ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };
        fontRow.Add(fontName, 0, 0);
        fontRow.Add(pick, 0, 1);
        fontRow.Add(attachments, 0, 2);

        var size = Field(nameof(StyleDisplay.FontSize), 1, 1000, "Aa", Se.Language.General.FontSize);
        size.Width = 76;

        ToggleButton Toggle(string text, string property, string name, FontWeight weight = FontWeight.Normal, FontStyle style = FontStyle.Normal)
        {
            var toggle = new ToggleButton
            {
                Content = new TextBlock { Text = text, FontWeight = weight, FontStyle = style, FontSize = 13 },
                Width = 28,
                Height = 28,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                [!ToggleButton.IsCheckedProperty] = new Binding(Current + property) { Mode = BindingMode.TwoWay },
            };
            toggle.Classes.Add("tool");
            AutomationProperties.SetName(toggle, name);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(toggle, name);
            }

            return toggle;
        }

        var styleRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        styleRow.Children.Add(size);
        styleRow.Children.Add(new Border { Width = 6 });
        styleRow.Children.Add(Toggle("B", nameof(StyleDisplay.Bold), Se.Language.General.Bold, FontWeight.Bold));
        styleRow.Children.Add(Toggle("I", nameof(StyleDisplay.Italic), Se.Language.General.Italic, style: FontStyle.Italic));

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(fontRow);
        content.Children.Add(styleRow);
        return EditorChrome.MakeEditorSection(Se.Language.General.Font, content);
    }

    private static Border MakeColorsSection(SsaStylesViewModel vm)
    {
        var grid = new UniformGrid { Columns = 4 };
        foreach (var (property, name) in new[]
                 {
                     (nameof(StyleDisplay.ColorPrimary), Se.Language.Assa.Primary),
                     (nameof(StyleDisplay.ColorSecondary), Se.Language.Assa.Secondary),
                     (nameof(StyleDisplay.ColorOutline), Se.Language.General.Outline),
                     (nameof(StyleDisplay.ColorShadow), Se.Language.General.Shadow),
                 })
        {
            var cell = new StackPanel { Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center };
            var button = MakeStyleColorPickerButton(vm, property);
            button.HorizontalAlignment = HorizontalAlignment.Center;
            AutomationProperties.SetName(button, name);
            cell.Children.Add(button);
            cell.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 11,
                Foreground = EditorChrome.DimTextBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
            grid.Children.Add(cell);
        }

        return EditorChrome.MakeEditorSection(Se.Language.General.Color, grid, first: true);
    }

    private static Border MakeBorderSection(SsaStylesViewModel vm)
    {
        var borderType = UiUtil.MakeComboBox(vm.BorderTypes, vm, nameof(vm.SelectedBorderType));
        borderType.HorizontalAlignment = HorizontalAlignment.Stretch;
        borderType.SelectionChanged += vm.BorderTypeChanged;
        AutomationProperties.SetName(borderType, Se.Language.General.BorderStyle);

        var outline = Field(nameof(StyleDisplay.OutlineWidth), 0, 100, "W", Se.Language.General.OutlineWidth, increment: 0.5m);
        var shadow = Field(nameof(StyleDisplay.ShadowWidth), 0, 100, "S", Se.Language.General.ShadowWidth, increment: 0.5m);

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(borderType);
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Size, outline, shadow));
        return EditorChrome.MakeEditorSection(Se.Language.General.BorderStyle, content);
    }

    /// <summary>
    /// The 3 x 3 alignment grid (top left ... bottom right) beside the margins.
    /// </summary>
    private static Border MakeAlignmentSection()
    {
        var anchors = new UniformGrid { Columns = 3, Rows = 3, Width = 90, Height = 90, VerticalAlignment = VerticalAlignment.Top };
        foreach (var an in new[] { 7, 8, 9, 4, 5, 6, 1, 2, 3 })
        {
            var radio = new RadioButton
            {
                GroupName = "ssa-styles-alignment",
                Width = 26,
                Height = 26,
                Margin = new Thickness(0, 0, 4, 4),
                Template = new FuncControlTemplate<RadioButton>((_, _) => new Border
                {
                    CornerRadius = new CornerRadius(6),
                    BorderThickness = new Thickness(1),
                    Child = new Ellipse { Width = 6, Height = 6, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                }),
                [!RadioButton.IsCheckedProperty] = new Binding(Current + "AlignmentAn" + an) { Mode = BindingMode.TwoWay },
            };
            radio.Classes.Add("anchor");
            AutomationProperties.SetName(radio, Se.Language.General.Alignment + " " + an);
            anchors.Children.Add(radio);
        }

        var margins = new StackPanel { Spacing = 6 };
        margins.Children.Add(Field(nameof(StyleDisplay.MarginLeft), 0, 1000, "L", Se.Language.General.Left, "0"));
        margins.Children.Add(Field(nameof(StyleDisplay.MarginRight), 0, 1000, "R", Se.Language.General.Right, "0"));
        margins.Children.Add(Field(nameof(StyleDisplay.MarginVertical), 0, 1000, "V", Se.Language.General.Vertical, "0"));

        var row = new Grid { ColumnSpacing = 14, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) } };
        row.Add(anchors, 0, 0);
        row.Add(margins, 0, 1);
        return EditorChrome.MakeEditorSection(Se.Language.General.Alignment + " · " + Se.Language.General.Margin, row, first: true);
    }

    private static Control MakePreview(SsaStylesViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        UiUtil.AttachHoverTooltip(image, Se.Language.General.Preview);
        AutomationProperties.SetName(image, Se.Language.General.Preview);

        return new Border { Child = image, Margin = new Thickness(16, 12) };
    }

    private static Border MakeButtonBar(SsaStylesViewModel vm)
    {
        // "2 styles in file · 5 saved"
        var summary = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        void UpdateSummary() => summary.Text = string.Format(Se.Language.Assa.StylesSummary, vm.FileStyles.Count, vm.StorageStyles.Count);
        UpdateSummary();
        vm.FileStyles.CollectionChanged += (_, _) => UpdateSummary();
        vm.StorageStyles.CollectionChanged += (_, _) => UpdateSummary();

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand)
            .WithBindIsVisible(nameof(vm.IsApplyVisible));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(buttonApply);
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(summary, 0, 0);
        grid.Add(buttons, 0, 1);
        return EditorChrome.WrapButtonBar(grid);
    }

    /// <summary>
    /// A color swatch that opens the color picker - without alpha: SSA v4 colors have none.
    /// </summary>
    private static Button MakeStyleColorPickerButton(SsaStylesViewModel vm, string colorPropertyName)
    {
        var swatch = new Border
        {
            Width = 34,
            Height = 22,
            CornerRadius = new CornerRadius(5),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.MenuBorderBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        swatch.Bind(Border.BackgroundProperty, new Binding(Current + colorPropertyName)
        {
            Source = vm,
            Converter = new ColorToBrushConverter(),
        });

        var button = new Button
        {
            Content = swatch,
            Padding = new Thickness(3),
            VerticalAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        button.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(button) is not Window window || vm.CurrentStyle is null)
            {
                return;
            }

            var propInfo = typeof(StyleDisplay).GetProperty(colorPropertyName);
            var currentColor = propInfo?.GetValue(vm.CurrentStyle) is Color c ? c : Colors.White;

            var pickerVm = new ColorPickerViewModel
            {
                ShowAlpha = false,
            };
            pickerVm.Initialize(SsaStylesViewModel.WithoutAlpha(currentColor));
            var pickerWindow = new ColorPickerWindow(pickerVm);
            await WindowService.ShowModalAsync(window, pickerWindow);

            if (pickerVm.OkPressed)
            {
                propInfo?.SetValue(vm.CurrentStyle, SsaStylesViewModel.WithoutAlpha(pickerVm.SelectedColor));
            }
        };

        return button;
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Globalization;
using System.Linq;
using Attached = Optris.Icons.Avalonia.Attached;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private readonly ShortcutsViewModel _vm;

    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        var language = Se.Language.Options.Shortcuts;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Shortcuts;
        Width = 900;
        Height = 720;
        MinWidth = 860;
        MinHeight = 550;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        _searchBox = new TextBox
        {
            PlaceholderText = language.SearchShortcuts,
            Margin = new Thickness(10),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        _searchBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        // Give the interactive controls accessible names so screen readers announce them instead of
        // reading a generic "edit"/"combo box"/"check box" (issue #11745).
        AutomationProperties.SetName(_searchBox, language.SearchShortcuts);

        var accentColor = UiUtil.GetAccentBrush() is ISolidColorBrush accentSolid ? accentSolid.Color : Colors.DodgerBlue;
        var labelBadgeCount = new Border
        {
            Background = new SolidColorBrush(accentColor, 0.18), // badge background
            CornerRadius = new CornerRadius(10),      // makes it pill-like
            Padding = new Thickness(8, 3, 8, 2),      // spacing around text
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.FlatNodes) + ".Count") { Source = vm, Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() },
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(accentColor),
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };

        var labelFilter = UiUtil.MakeTextBlock(Se.Language.General.Filter);
        var comboBoxFilter = UiUtil.MakeComboBox(vm.Filters, vm, nameof(vm.SelectedFilter))
            .WithMinWidth(120)
            .WithMargin(5, 0, 10, 0);
        comboBoxFilter.SelectionChanged += vm.ComboBoxFilter_SelectionChanged;
        AutomationProperties.SetLabeledBy(comboBoxFilter, labelFilter);

        var topGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(10),
        };
        topGrid.Add(_searchBox, 0);
        topGrid.Add(labelBadgeCount, 0, 1);
        topGrid.Add(labelFilter, 0, 2);
        topGrid.Add(comboBoxFilter, 0, 3);

        // Group filter tiles - one per thematic group, plus "All".
        var groupTiles = new ListBox
        {
            ItemsSource = vm.GroupTiles,
            Margin = new Thickness(0, 0, 0, 8),
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal }),
            ItemContainerTheme = new ControlTheme(typeof(ListBoxItem))
            {
                BasedOn = Application.Current?.FindResource(typeof(ListBoxItem)) as ControlTheme,
                Setters =
                {
                    new Setter(ListBoxItem.PaddingProperty, new Thickness(6, 2)),
                    // Accessible name for the tile container - without it screen readers
                    // announce the item as the ShortcutGroupTile class name (issue #12087).
                    new Setter(AutomationProperties.NameProperty, new MultiBinding
                    {
                        StringFormat = "{0} ({1})",
                        Bindings =
                        {
                            new Binding(nameof(ShortcutGroupTile.Name)),
                            new Binding(nameof(ShortcutGroupTile.Count)),
                        },
                    }),
                },
            },
            ItemTemplate = new FuncDataTemplate<ShortcutGroupTile>((_, _) =>
            {
                var icon = new ContentControl
                {
                    FontSize = 17,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                icon.Bind(Attached.IconProperty, new Binding(nameof(ShortcutGroupTile.IconName)));
                // Keep the glyph white on the colored square in the dark theme too - the
                // class works no matter when the data-template binding materializes the
                // Icon (#12717).
                icon.Classes.Add(UiTheme.IconOnAccentClassName);

                var glyph = new Border
                {
                    Width = 32,
                    Height = 32,
                    CornerRadius = new CornerRadius(9),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = icon,
                };
                glyph.Bind(Border.BackgroundProperty, new Binding(nameof(ShortcutGroupTile.Brush)));

                var name = new TextBlock
                {
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                };
                name.Bind(TextBlock.TextProperty, new Binding(nameof(ShortcutGroupTile.Name)));

                var count = new TextBlock
                {
                    FontSize = 10,
                    Opacity = 0.6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                count.Bind(TextBlock.TextProperty, new Binding(nameof(ShortcutGroupTile.Count)));

                return new StackPanel
                {
                    Width = 92,
                    Spacing = 2,
                    Margin = new Thickness(0, 3, 0, 2),
                    Children = { glyph, name, count },
                };
            }),
        };
        groupTiles.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedGroupTile)) { Source = vm, Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(groupTiles, Se.Language.General.Category);

        // TableView has no content-based column sizing (Auto behaves as star), so the
        // former Auto columns get pixel widths measured from the widest strings they
        // can show (the VM is initialized before the window ctor, so the rows exist).
        const double sortArrowSlack = 18; // room for TableViewHeaderSorter's ▲/▼ header suffix
        static double MeasureWidth(string text, double fontSize, FontWeight fontWeight) =>
            new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, fontWeight), fontSize, null).Width;
        static double HeaderWidth(string header) =>
            MeasureWidth(header, 14, FontWeight.SemiBold) + 8 + sortArrowSlack;

        var activeInTexts = new[]
        {
            language.CategorySubtitleGridAndTextBox,
            language.CategorySubtitleGrid,
            Se.Language.General.Waveform,
            language.CategoryTextBox,
            language.ActiveInEverywhere,
        };
        // Pill chrome: 8+8 padding, 1+1 border, 4+4 margin, plus a little slack.
        var activeInWidth = Math.Max(HeaderWidth(language.ActiveIn),
            activeInTexts.Max(t => MeasureWidth(t, 11, FontWeight.Normal)) + 30);

        // Category text chrome: 2+14 margin plus a little slack.
        var categoryWidth = Math.Max(HeaderWidth(Se.Language.General.Category),
            vm.FlatNodes.Select(n => n.GroupName).Distinct()
                .Select(t => MeasureWidth(t, 12, FontWeight.Medium) + 24)
                .DefaultIfEmpty(90).Max());

        // Keycap chips: 6+6 padding, 1+1 border, min 24 wide, 3 spacing; panel margin 4+20.
        static double ChipRowWidth(ShortcutTreeNode node) => node.KeyParts.Count == 0
            ? MeasureWidth(Se.Language.Options.Shortcuts.Unassigned, 11, FontWeight.Normal)
            : node.KeyParts.Sum(k => Math.Max(24, MeasureWidth(k, 11, FontWeight.SemiBold) + 14)) +
              (node.KeyParts.Count - 1) * 3;
        var shortcutWidth = Math.Max(HeaderWidth(Se.Language.General.Shortcut),
            vm.FlatNodes.Select(ChipRowWidth).DefaultIfEmpty(100).Max() + 24);

        var columnActiveIn = new SeTableViewColumn
        {
            Header = language.ActiveIn,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(activeInWidth),
            CellTemplate = new FuncDataTemplate<ShortcutTreeNode>((_, _) =>
            {
                var text = new TextBlock
                {
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                text.Bind(TextBlock.TextProperty, new Binding(nameof(ShortcutTreeNode.ActiveIn)));

                var pill = new Border
                {
                    BorderBrush = UiUtil.GetBorderBrush(),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(99),
                    Padding = new Thickness(8, 1, 8, 2),
                    Margin = new Thickness(4, 0, 4, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = text,
                };
                pill.Bind(Visual.OpacityProperty, new Binding(nameof(ShortcutTreeNode.ActiveInOpacity)));
                return pill;
            }),
        };

        var columnGroupIcon = new SeTableViewColumn
        {
            Header = string.Empty,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(40),
            CanUserResize = false,
            CellTemplate = new FuncDataTemplate<ShortcutTreeNode>((_, _) =>
            {
                var icon = new ContentControl
                {
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                icon.Bind(Attached.IconProperty, new Binding(nameof(ShortcutTreeNode.GroupIconName)));
                icon.Bind(TemplatedControl.ForegroundProperty, new Binding(nameof(ShortcutTreeNode.GroupBrush)));

                var square = new Border
                {
                    Width = 24,
                    Height = 24,
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(4, 2, 4, 2),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = icon,
                };
                square.Bind(Border.BackgroundProperty, new Binding(nameof(ShortcutTreeNode.GroupSoftBrush)));
                square.Bind(ToolTip.TipProperty, new Binding(nameof(ShortcutTreeNode.GroupName)));
                return square;
            }),
        };

        var columnCategory = new SeTableViewColumn
        {
            Header = Se.Language.General.Category,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(categoryWidth),
            CellTemplate = new FuncDataTemplate<ShortcutTreeNode>((_, _) =>
            {
                // Category name in the group color so the grouping reads at a glance
                // (text variant: darkened on the light theme for contrast, #12778).
                var text = new TextBlock
                {
                    FontSize = 12,
                    FontWeight = FontWeight.Medium,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 14, 0),
                };
                text.Bind(TextBlock.TextProperty, new Binding(nameof(ShortcutTreeNode.GroupName)));
                text.Bind(TextBlock.ForegroundProperty, new Binding(nameof(ShortcutTreeNode.GroupTextBrush)));
                return text;
            }),
        };

        var columnName = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(ShortcutTreeNode.Title)),
            Width = new GridLength(1, GridUnitType.Star),
        };

        var columnShortcut = new SeTableViewColumn
        {
            Header = Se.Language.General.Shortcut,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(shortcutWidth),
            CellTemplate = new FuncDataTemplate<ShortcutTreeNode>((_, _) =>
            {
                // Keycap-style chips, one per key, right-aligned like the column header side.
                var keys = new ItemsControl
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 3,
                    }),
                    ItemTemplate = new FuncDataTemplate<string>((_, _) =>
                    {
                        var keyText = new TextBlock
                        {
                            FontSize = 11,
                            FontWeight = FontWeight.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                        keyText.Bind(TextBlock.TextProperty, new Binding("."));
                        return new Border
                        {
                            Background = new SolidColorBrush(Colors.Gray, 0.12),
                            BorderBrush = new SolidColorBrush(Colors.Gray, 0.45),
                            BorderThickness = new Thickness(1, 1, 1, 2),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(6, 1, 6, 2),
                            MinWidth = 24,
                            Child = keyText,
                        };
                    }),
                };
                keys.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(ShortcutTreeNode.KeyParts)));
                keys.Bind(Visual.IsVisibleProperty, new Binding(nameof(ShortcutTreeNode.IsAssigned)));

                var notSet = new TextBlock
                {
                    Text = Se.Language.Options.Shortcuts.Unassigned,
                    FontSize = 11,
                    FontStyle = FontStyle.Italic,
                    Opacity = 0.45,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                notSet.Bind(Visual.IsVisibleProperty, new Binding(nameof(ShortcutTreeNode.IsUnassigned)));

                return new Panel
                {
                    // Extra right margin so the TableView's overlay scrollbar
                    // does not cover the key chips in the last column.
                    Margin = new Thickness(4, 2, 20, 2),
                    Children = { keys, notSet },
                };
            }),
        };

        var shortcutsGrid = TableViewExtras.MakeTableView(multiSelect: false);
        shortcutsGrid.DataContext = vm;
        shortcutsGrid.ItemsSource = vm.FlatNodes;
        shortcutsGrid.Columns.Add(columnActiveIn);
        shortcutsGrid.Columns.Add(columnGroupIcon);
        shortcutsGrid.Columns.Add(columnCategory);
        shortcutsGrid.Columns.Add(columnName);
        shortcutsGrid.Columns.Add(columnShortcut);

        // Accessible row name "title (category): shortcut" - without it screen readers
        // announce every row as the ShortcutTreeNode class name (issue #12087). Same
        // row-binding pattern as the main subtitle grid (#13015). DisplayShortcut is
        // observable, so reassigning a key updates the announced name too.
        var shortcutOrUnassignedConverter = new FuncValueConverter<string?, string>(s =>
            string.IsNullOrEmpty(s) ? Se.Language.Options.Shortcuts.Unassigned : s);
        TableViewExtras.BindRowProperty(shortcutsGrid, AutomationProperties.NameProperty,
            new MultiBinding
            {
                StringFormat = "{0} ({1}): {2}",
                Bindings =
                {
                    new Binding(nameof(ShortcutTreeNode.Title)),
                    new Binding(nameof(ShortcutTreeNode.GroupName)),
                    new Binding(nameof(ShortcutTreeNode.DisplayShortcut)) { Converter = shortcutOrUnassignedConverter },
                },
            });

        shortcutsGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedNode)) { Source = vm });
        shortcutsGrid.SelectionChanged += vm.ShortcutsGrid_SelectionChanged;
        shortcutsGrid.DoubleTapped += (s, e) =>
        {
            // Double-clicking a sortable column header must not open the detect-key dialog.
            if (!TableViewExtras.IsInColumnHeader(e.Source as Visual))
            {
                vm.ShortcutsGridDoubleTapped(s, e);
            }
        };

        // Click-to-sort on every column the DataGrid sorted (the SortMemberPath template
        // columns and the Name text column, #12431). The list's order is presentation-
        // only - OK/save reads Se.Settings.Shortcuts, not FlatNodes - and a search or
        // filter change rebuilds the list in its default group order again.
        var sorter = new TableViewHeaderSorter(shortcutsGrid);
        sorter.AddSortable<ShortcutTreeNode, string>(columnActiveIn, x => x.ActiveIn)
            .AddSortable<ShortcutTreeNode, string>(columnGroupIcon, x => x.GroupName)
            .AddSortable<ShortcutTreeNode, string>(columnCategory, x => x.GroupName)
            .AddSortable<ShortcutTreeNode, string>(columnName, x => x.Title)
            .AddSortable<ShortcutTreeNode, string>(columnShortcut, x => x.DisplayShortcut);

        var borderShortcutsGrid = UiUtil.MakeBorderForControlNoPadding(shortcutsGrid).WithMarginBottom(5);

        var flyout = new MenuFlyout();
        shortcutsGrid.ContextFlyout = flyout;
        var menuItemImport = new MenuItem
        {
            Header = Se.Language.General.ImportDotDotDot,
            DataContext = vm,
            Command = vm.ImportCommand,
        };
        flyout.Items.Add(menuItemImport);

        var menuItemExport = new MenuItem
        {
            Header = Se.Language.General.ExportDotDotDot,
            DataContext = vm,
            Command = vm.ExportCommand,
        };
        flyout.Items.Add(menuItemExport);

        var menuItemImportSe4 = new MenuItem
        {
            Header = language.ImportFromSe4,
            DataContext = vm,
            Command = vm.ImportFromSe4Command,
        };
        flyout.Items.Add(menuItemImportSe4);


        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        var buttonResetAllShortcuts = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetAllShortcutsCommand);
        var buttonImportSe4 = UiUtil.MakeButton(language.ImportFromSe4, vm.ImportFromSe4Command);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);

        // Utility actions on the left, dialog actions on the right.
        var buttonBarLeft = UiUtil.MakeControlBarLeft(buttonImportSe4, buttonResetAllShortcuts);
        buttonBarLeft.Margin = new Thickness(10, 20, 10, 10);
        buttonBarLeft.VerticalAlignment = VerticalAlignment.Bottom;
        var buttonBarRight = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        var buttonPanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        buttonPanel.Add(buttonBarLeft, 0, 0);
        buttonPanel.Add(buttonBarRight, 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };
        grid.Add(topGrid, 0);
        grid.Add(groupTiles, 1);
        grid.Add(borderShortcutsGrid, 2);

        var editPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Get platform-specific labels
        var isMac = OperatingSystem.IsMacOS();
        var ctrlLabel = isMac ? Se.Language.Options.Shortcuts.ControlMac : Se.Language.Options.Shortcuts.Control;
        var altLabel = isMac ? Se.Language.Options.Shortcuts.AltMac : Se.Language.Options.Shortcuts.Alt;
        var shiftLabel = isMac ? Se.Language.Options.Shortcuts.ShiftMac : Se.Language.Options.Shortcuts.Shift;
        var winLabel = isMac ? Se.Language.Options.Shortcuts.WinMac : Se.Language.Options.Shortcuts.Win;

        // Shift checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(shiftLabel).WithMarginRight(3));
        var checkBoxShift = UiUtil.MakeCheckBox(vm, nameof(vm.ShiftIsSelected));
        checkBoxShift.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        AutomationProperties.SetName(checkBoxShift, shiftLabel);
        editPanel.Children.Add(checkBoxShift);

        // Control checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(ctrlLabel).WithMarginRight(3));
        var controlCheckBox = UiUtil.MakeCheckBox(vm, nameof(vm.CtrlIsSelected));
        controlCheckBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        AutomationProperties.SetName(controlCheckBox, ctrlLabel);
        editPanel.Children.Add(controlCheckBox);

        // Alt checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(altLabel).WithMarginRight(3));
        var checkBoxAlt = UiUtil.MakeCheckBox(vm, nameof(vm.AltIsSelected));
        checkBoxAlt.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        AutomationProperties.SetName(checkBoxAlt, altLabel);
        editPanel.Children.Add(checkBoxAlt);

        // Win key checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(winLabel).WithMarginRight(3));
        var checkBoxWin = UiUtil.MakeCheckBox(vm, nameof(vm.WinIsSelected));
        checkBoxWin.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        AutomationProperties.SetName(checkBoxWin, winLabel);
        editPanel.Children.Add(checkBoxWin);

        // Key combobox
        var comboBoxKeys = new ComboBox
        {
            Width = 200,
            Margin = new Thickness(10, 0, 5, 0),
        };
        comboBoxKeys.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Shortcuts)) { Source = vm });
        comboBoxKeys.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedShortcut)) { Source = vm });
        comboBoxKeys.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        AutomationProperties.SetName(comboBoxKeys, Se.Language.General.Shortcut);
        editPanel.Children.Add(comboBoxKeys);

        // browse button
        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.ShowGetKeyCommand, accessibleName: Se.Language.Options.Shortcuts.DetectKey);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonBrowse, Se.Language.Options.Shortcuts.DetectKey);
        }

        editPanel.Children.Add(buttonBrowse);
        buttonBrowse.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        buttonBrowse.Margin = new Thickness(0, 0, 9, 0);

        // configure button
        var buttonConfig = UiUtil.MakeButton(vm.ConfigureCommand, IconNames.Settings, Se.Language.General.Settings);

        editPanel.Children.Add(buttonConfig);
        buttonConfig.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        buttonConfig.Bind(IsVisibleProperty, new Binding(nameof(vm.IsConfigureVisible)) { Source = vm });
        buttonConfig.Margin = new Thickness(0, 0, 10, 0);

        // Reset button
        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetShortcutCommand);
        editPanel.Children.Add(buttonReset);
        buttonReset.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });

        // Name of the shortcut being edited on the left, key controls on the right.
        var labelEditing = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelEditing.Bind(TextBlock.TextProperty, new Binding(nameof(vm.SelectedNode) + "." + nameof(ShortcutTreeNode.Title)) { Source = vm });

        var editSplitGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        editSplitGrid.Add(labelEditing, 0, 0);
        editSplitGrid.Add(editPanel, 0, 1);

        var editGridBorder = UiUtil.MakeBorderForControl(editSplitGrid);
        grid.Add(editGridBorder, 3);
        grid.Add(buttonPanel, 4);

        Content = grid;

        _searchBox.TextChanged += (s, e) => vm.UpdateVisibleShortcuts(_searchBox.Text ?? string.Empty);
        Opened += (_, _) =>
        {
            Activate();
            Dispatcher.UIThread.Post(() => _searchBox.Focus(), DispatcherPriority.Input);
        };
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
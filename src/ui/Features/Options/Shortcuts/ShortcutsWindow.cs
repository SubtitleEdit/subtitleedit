using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private readonly ShortcutsViewModel _vm;

    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        var language = Se.Language.Options.Shortcuts;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = language.Title;
        Width = 760;
        Height = 650;
        MinWidth = 740;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        _searchBox = new TextBox
        {
            Watermark = language.SearchShortcuts,
            Margin = new Thickness(10),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        _searchBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });

        var labelBadgeCount = new Border
        {
            Background = UiUtil.GetBorderBrush(),     // badge background
            CornerRadius = new CornerRadius(10),      // makes it pill-like
            Padding = new Thickness(6, 3, 6, 2),      // spacing around text
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.FlatNodes) + ".Count") { Source = vm, Mode = BindingMode.OneWay, Converter = new NumberToStringWithThousandSeparator() },
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.WhiteSmoke,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };

        var labelFilter = UiUtil.MakeTextBlock(language.Filter);
        var comboBoxFilter = UiUtil.MakeComboBox(vm.Filters, vm, nameof(vm.SelectedFilter))
            .WithMinWidth(120)
            .WithMargin(5, 0, 10, 0);
        comboBoxFilter.SelectionChanged += vm.ComboBoxFilter_SelectionChanged;

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

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.FlatNodes,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Category,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ShortcutTreeNode.Category)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ShortcutTreeNode.Title)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Shortcut,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ShortcutTreeNode.DisplayShortcut)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedNode)) { Source = vm });
        dataGrid.SelectionChanged += vm.ShortcutsDataGrid_SelectionChanged;
        dataGrid.DoubleTapped += vm.ShortcutsDataGridDoubleTapped;
        var borderDataGrid = UiUtil.MakeBorderForControlNoPadding(dataGrid).WithMarginBottom(5);

        var flyout = new MenuFlyout();
        dataGrid.ContextFlyout = flyout;
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


        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        var buttonResetAllShortcuts = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetAllShortcutsCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonResetAllShortcuts, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };
        grid.Add(topGrid, 0);
        grid.Add(borderDataGrid, 1);

        var editPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
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
        editPanel.Children.Add(checkBoxShift);

        // Control checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(ctrlLabel).WithMarginRight(3));
        var controlCheckBox = UiUtil.MakeCheckBox(vm, nameof(vm.CtrlIsSelected));
        controlCheckBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(controlCheckBox);

        // Alt checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(altLabel).WithMarginRight(3));
        var checkBoxAlt = UiUtil.MakeCheckBox(vm, nameof(vm.AltIsSelected));
        checkBoxAlt.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        editPanel.Children.Add(checkBoxAlt);

        // Win key checkbox and label
        editPanel.Children.Add(UiUtil.MakeTextBlock(winLabel).WithMarginRight(3));
        var checkBoxWin = UiUtil.MakeCheckBox(vm, nameof(vm.WinIsSelected));
        checkBoxWin.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
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
        editPanel.Children.Add(comboBoxKeys);

        // browse button
        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.ShowGetKeyCommand);
        editPanel.Children.Add(buttonBrowse);
        buttonBrowse.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        buttonBrowse.Margin = new Thickness(0, 0, 9, 0);

        // configure button
        var buttonConfig = UiUtil.MakeButton(vm.ConfigureCommand, IconNames.Settings);
        editPanel.Children.Add(buttonConfig);
        buttonConfig.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });
        buttonConfig.Bind(IsVisibleProperty, new Binding(nameof(vm.IsConfigureVisible)) { Source = vm });
        buttonConfig.Margin = new Thickness(0, 0, 10, 0);

        // Reset button
        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetShortcutCommand);
        editPanel.Children.Add(buttonReset);
        buttonReset.Bind(IsEnabledProperty, new Binding(nameof(vm.IsControlsEnabled)) { Source = vm });

        var editGridBorder = UiUtil.MakeBorderForControl(editPanel);
        grid.Add(editGridBorder, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        _searchBox.TextChanged += (s, e) => vm.UpdateVisibleShortcuts(_searchBox.Text ?? string.Empty);
        Activated += delegate { _searchBox.Focus(); }; // hack to make OnKeyDown work
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public class FixNamesWindow : Window
{

    private FixNamesViewModel _vm;

    public FixNamesWindow(FixNamesViewModel vm)
    {
        var lang = Se.Language.Tools.ChangeCasing;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChangeCasing;
        Width = 900;
        Height = 800;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Name title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Names
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Extra names label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Extra names entry
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Hits title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Hits
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
        };

        var row = 0;
        var labelTitle = UiUtil.MakeLabel(lang.FixNames);
        grid.Add(labelTitle, row, 0);

        row++;
        var labelNamesCount = new Label();
        labelNamesCount.Bind(Label.ContentProperty, new Binding($"{nameof(vm.Names)}.{nameof(vm.Names.Count)}")
        {
            Mode = BindingMode.OneWay,
            StringFormat = lang.NamesX,
        });
        grid.Add(labelNamesCount, row, 0);

        row++;
        grid.Add(MakeNamesView(vm), row, 0);

        row++;
        var labelExtraNames = UiUtil.MakeLabel(lang.ExtraNames);
        grid.Add(labelExtraNames, row, 0);

        row++;
        var textBoxExtraNames = UiUtil.MakeTextBox(600, vm, nameof(vm.ExtraNames));
        textBoxExtraNames.PlaceholderText = lang.EnterExtraNamesHint;
        var buttonAddExtraName = UiUtil.MakeButton(Se.Language.General.Refresh, vm.AddExtraNameCommand);
        var stackExtraNames = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 5, 0, 20),
            Children =
            {
                textBoxExtraNames,
                buttonAddExtraName,
            }
        };
        grid.Add(stackExtraNames, row, 0);

        row++;
        var labelHits = UiUtil.MakeLabel(new Binding($"{nameof(vm.Hits)}.{nameof(vm.Hits.Count)}")
        {
            StringFormat = lang.HitsX,
        });
        grid.Add(labelHits, row, 0);

        row++;
        grid.Add(MakeHitsView(vm), row, 0);

        // OK and Cancel buttons
        row++;
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtonsBottom = UiUtil.MakeButtonBar(
            buttonOk,
            buttonCancel
        );

        grid.Add(panelButtonsBottom, row, 0);

        Content = grid;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeNamesView(FixNamesViewModel vm)
    {
        // The DataGrid this replaces used DataGridCheckboxMultiSelect for extended
        // selection + Space toggling; shift/ctrl multi-select is native ListBox behavior
        // on TableView, so only the Space toggle needs wiring (TableViewExtras.AddSpaceToggle).
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Height = double.NaN;
        dataGrid.ItemsSource = vm.Names;
        dataGrid.DataContext = vm;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixNameItem>(static (item, _) =>
            new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNameItem.IsChecked)),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                }
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80)
        });

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(FixNameItem.Name)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.AddSpaceToggle<FixNameItem>(dataGrid,
            item => item.IsChecked, (item, v) => item.IsChecked = v);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.SelectAll, Command = vm.NamesSelectAllCommand });
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.InvertSelection, Command = vm.NamesInvertSelectionCommand });
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var border = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        return border;
    }

    private static Border MakeHitsView(FixNamesViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Height = double.NaN;
        dataGrid.ItemsSource = vm.Hits;
        dataGrid.DataContext = vm;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Apply,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixNameHitItem>(static (_, _) => new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNameHitItem.IsEnabled)),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                }
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80)
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(FixNameHitItem.LineIndexDisplay)),
            Width = new GridLength(140),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Before,
            Binding = new Binding(nameof(FixNameHitItem.Before)),
            Width = new GridLength(220),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.After,
            Binding = new Binding(nameof(FixNameHitItem.After)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.AddSpaceToggle<FixNameHitItem>(dataGrid,
            item => item.IsEnabled, (item, v) => item.IsEnabled = v);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.SelectAll, Command = vm.HitsSelectAllCommand });
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.InvertSelection, Command = vm.HitsInvertSelectionCommand });
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var border = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        return border;
    }


    protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}

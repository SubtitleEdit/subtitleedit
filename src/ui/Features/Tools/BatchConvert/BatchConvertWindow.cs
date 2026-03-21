using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertWindow : Window
{
    public BatchConvertWindow(BatchConvertViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.Title;
        Width = 1024;
        Height = 740;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var fileView = MakeFileView(vm);
        var functionsListView = MakeFunctionsListView(vm);
        var functionView = MakeFunctionView(vm);

        var labelFunctionsSelected = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ActionsSelected))
            .WithAlignmentTop();

        var labelBatchItemsInfo = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.BatchItemsInfo))
            .WithAlignmentTop();

        var panelInfo = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelFunctionsSelected,
                labelBatchItemsInfo,
            }
        };
        panelInfo.WithBindVisible(vm, nameof(vm.IsConverting), new InverseBooleanConverter());

        var buttonConvert = new SplitButton
        {
            Content = Se.Language.General.Convert,
            Command = vm.ConvertCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.File.Statistics.Title,
                        Command = vm.StatisticsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.General.ListErrors,
                        Command = vm.ShowErrorListCommand,
                    },
                }
            }
        };
        buttonConvert.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.IsConverting)) { Converter = new InverseBooleanConverter() });

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand).WithBindIsVisible(nameof(vm.IsConverting), new InverseBooleanConverter());
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(vm, nameof(vm.IsConverting));
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonConvert,
            buttonCancel,
            buttonDone
        );

        var progressText = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.ProgressText))
            .WithAlignmentTop();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(fileView, 0, 0, 1, 2);
        grid.Add(functionsListView, 1, 0);
        grid.Add(functionView, 1, 1);
        grid.Add(panelInfo, 2, 0);
        grid.Add(progressText, 2, 0);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeFileView(BatchConvertViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 0,
        };

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
            ItemsSource = vm.BatchItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.FileName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.Size)) { Converter = new FileSizeConverter(), Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                    Width = new DataGridLength(90, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Format,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.Format)),
                    IsReadOnly = true,
                    Width = new DataGridLength(170, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BatchConvertItem.Status)),
                    IsReadOnly = true,
                    Width = new DataGridLength(120, DataGridLengthUnitType.Pixel),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchItem)) { Source = vm });

        var comboBoxSubtitleFormat = UiUtil.MakeComboBox(vm.TargetFormats, vm, nameof(vm.SelectedTargetFormat));
        comboBoxSubtitleFormat.SelectionChanged += (_, _) => vm.ComboBoxSubtitleFormatChanged();
        comboBoxSubtitleFormat.PointerPressed += vm.ComboBoxSubtitleFormatPointerPressed;
        comboBoxSubtitleFormat.Width = 240;

        var buttonTargetFormatSettings = UiUtil.MakeButton(vm.ShowTargetFormatSettingsCommand, IconNames.Settings)
            .WithMarginLeft(5)
            .WithMarginRight(5);
        buttonTargetFormatSettings.WithBindIsVisible(vm, nameof(vm.IsTargetFormatSettingsVisible));
        var buttonSettings = UiUtil.MakeButton(vm.ShowOutputPropertiesCommand, IconNames.Settings).WithMarginLeft(15).WithMarginRight(5);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonTargetFormatSettings, Se.Language.Tools.BatchConvert.TargetFormatSettings);
            ToolTip.SetTip(buttonSettings, Se.Language.General.Settings);
        }

        var panelFileControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0),
            Children =
            {
                UiUtil.MakeButton(vm.AddFilesCommand, IconNames.Plus, Se.Language.General.Add).WithMarginLeft(10),
                UiUtil.MakeButton(vm.RemoveSelectedFilesCommand, IconNames.Trash, Se.Language.General.Remove).WithMarginLeft(5),
                UiUtil.MakeButton(vm.ClearAllFilesCommand, IconNames.Close, Se.Language.General.Clear).WithMarginLeft(5),
                UiUtil.MakeLabel(Se.Language.General.TargetFormat).WithMarginLeft(15),
                comboBoxSubtitleFormat,
                buttonTargetFormatSettings,
                buttonSettings,
                MakeOutputPropertiesGrid(vm),
            }
        };

        var labelFilter = UiUtil.MakeLabel(Se.Language.General.Filter);
        var comboBoxFilter = UiUtil.MakeComboBox(vm.FilterItems, vm, nameof(vm.SelectedFilterItem))
            .WithMarginRight(3);
        comboBoxFilter.SelectionChanged += (_, _) => vm.FilterComboBoxChanged();
        var textBoxFilter = UiUtil.MakeTextBox(200, vm, nameof(vm.FilterText))
            .WithBindIsVisible(nameof(vm.IsFilterTextVisible));
        textBoxFilter.TextChanged += (_, _) => vm.FilterTextChanged();
        var panelFilter = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 10),
            Children =
            {
                labelFilter,
                comboBoxFilter,
                textBoxFilter,
            }
        };

        var flyout = new MenuFlyout();
        flyout.Opening += (_, _) => vm.FileGridContextMenuOpening();
        var menuItemRemove = new MenuItem
        {
            Header = Se.Language.General.Remove,
            DataContext = vm,
            Command = vm.RemoveSelectedFilesCommand,
        };
        menuItemRemove.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsRemoveVisible)) { Source = vm });
        menuItemRemove.Bind(MenuItem.IsEnabledProperty, new Binding(nameof(vm.IsConverting))
        {
            Converter = new InverseBooleanConverter(),
            Source = vm,
        });
        flyout.Items.Add(menuItemRemove);

        var menuItemOpenContainingFolder = new MenuItem
        {
            Header = Se.Language.General.OpenContainingFolder,
            DataContext = vm,
            Command = vm.OpenContainingFolderCommand,
        };
        menuItemOpenContainingFolder.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsOpenContainingFolderVisible)) { Source = vm });
        menuItemOpenContainingFolder.Bind(MenuItem.IsEnabledProperty, new Binding(nameof(vm.IsConverting))
        {
            Converter = new InverseBooleanConverter(),
            Source = vm,
        });
        flyout.Items.Add(menuItemOpenContainingFolder);

        var menuItemImport = new MenuItem
        {
            Header = Se.Language.General.AddDotDotDot,
            DataContext = vm,
            Command = vm.AddFilesCommand,
        };
        flyout.Items.Add(menuItemImport);

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);
        dropHost.ContextFlyout = flyout;

        grid.Add(dropHost, 0, 0);
        grid.Add(panelFileControls, 1, 0);
        grid.Add(panelFilter, 2, 0);

        var border = UiUtil.MakeBorderForControlNoPadding(grid);
        return border;
    }

    private static Grid MakeOutputPropertiesGrid(BatchConvertViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 0,
            RowSpacing = 0,
        };

        var labelOutputSourceFolder = UiUtil.MakeLabel(new Binding(nameof(vm.OutputFolderLabel)));
        var linkLabelOutputFolder = UiUtil.MakeLink(string.Empty, vm.OpenOutputFolderCommand, vm, nameof(vm.OutputFolderLinkLabel))
                            .WithAlignmentLeft();
        var labelOutputEncoding = UiUtil.MakeLabel(new Binding(nameof(vm.OutputEncodingLabel))).WithAlignmentTop();

        grid.Add(labelOutputSourceFolder, 0);
        grid.Add(linkLabelOutputFolder, 0);
        grid.Add(labelOutputEncoding, 1);

        return grid;
    }

    private static Border MakeFunctionsListView(BatchConvertViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility = DataGridHeadersVisibility.None,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = 300,
            DataContext = vm,
            ItemsSource = vm.BatchFunctions,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Enabled,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<BatchConvertFunction>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(0),
                        Child = MakeSelectedCheckBox(vm)
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    Binding = new Binding(nameof(BatchConvertFunction.Name)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchFunction)) { Source = vm });
        dataGrid.SelectionChanged += (_, _) => vm.SelectedFunctionChanged();

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    private static CheckBox MakeSelectedCheckBox(BatchConvertViewModel vm)
    {
        var checkBox = new CheckBox
        {
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(BatchConvertFunction.IsSelected)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
        };

        checkBox.IsCheckedChanged += (_, _) => vm.SelectedFunctionChanged();

        return checkBox;
    }

    private static Border MakeFunctionView(BatchConvertViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(10, 15, 10, 10),
            Width = double.NaN,
            Height = 300,
        };

        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
        };
        vm.FunctionContainer = scrollViewer;

        return UiUtil.MakeBorderForControl(scrollViewer);
    }
}

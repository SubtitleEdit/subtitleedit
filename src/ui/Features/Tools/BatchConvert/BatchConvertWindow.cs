using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

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

        var columnFileName = new SeTableViewColumn
        {
            Header = Se.Language.General.FileName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BatchConvertItem.FileName)),
            Width = new GridLength(1, GridUnitType.Star),
        };

        var columnSize = new SeTableViewColumn
        {
            Header = Se.Language.General.Size,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BatchConvertItem.Size)) { Converter = new FileSizeConverter(), Mode = BindingMode.OneWay },
            Width = new GridLength(90),
        };

        var columnFormat = new SeTableViewColumn
        {
            Header = Se.Language.General.Format,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BatchConvertItem.Format)),
            Width = new GridLength(170),
        };

        var columnStatus = new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(120),
            CellTemplate = new FuncDataTemplate<BatchConvertItem>((_, _) =>
            {
                // Status as a colored badge: green converted, red errors, gray cancelled;
                // in-progress statuses render as plain text (converter returns unset).
                var text = new TextBlock
                {
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                text.Bind(TextBlock.TextProperty, new Binding(nameof(BatchConvertItem.Status)));
                text.Bind(TextBlock.ForegroundProperty, new Binding(nameof(BatchConvertItem.Status))
                {
                    Converter = new BatchConvertStatusColorConverter(),
                });

                var pill = new Border
                {
                    CornerRadius = new CornerRadius(99),
                    Padding = new Thickness(8, 1, 8, 2),
                    Margin = new Thickness(4, 0, 4, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = text,
                };
                pill.Bind(Border.BackgroundProperty, new Binding(nameof(BatchConvertItem.Status))
                {
                    Converter = new BatchConvertStatusColorConverter(),
                    ConverterParameter = "background",
                });
                return pill;
            }),
        };

        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.BatchItems;
        dataGrid.Columns.AddRange(new[] { columnFileName, columnSize, columnFormat, columnStatus });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedBatchItem)) { Source = vm });
        dataGrid.KeyDown += vm.FileGridKeyDown;
        vm.FileGrid = dataGrid;
        TableViewExtras.AttachListNavigation(dataGrid);

        // Header-click sorting (the DataGrid sorted these columns too, incl. Status via
        // SortMemberPath, #12431). The sorter reorders BatchItems in place - safe here
        // because the job order is presentation-only and the conversion loop walks a
        // snapshot of the list, not the live collection.
        new TableViewHeaderSorter(dataGrid)
            .AddSortable<BatchConvertItem, string>(columnFileName, x => x.FileName)
            .AddSortable<BatchConvertItem, long>(columnSize, x => x.Size)
            .AddSortable<BatchConvertItem, string>(columnFormat, x => x.Format)
            .AddSortable<BatchConvertItem, string>(columnStatus, x => x.Status);

        var comboBoxSubtitleFormat = UiUtil.MakeComboBox(vm.TargetFormats, vm, nameof(vm.SelectedTargetFormat));
        comboBoxSubtitleFormat.SelectionChanged += (_, _) => vm.ComboBoxSubtitleFormatChanged();
        // Tunnel phase so right-click / Mac Ctrl+click opens the format picker before the ComboBox
        // consumes the click to open its dropdown (matches the main window's format combo).
        comboBoxSubtitleFormat.AddHandler(InputElement.PointerPressedEvent,
            vm.ComboBoxSubtitleFormatPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        comboBoxSubtitleFormat.Width = 240;

        var buttonTargetFormatSettings = UiUtil.MakeButton(vm.ShowTargetFormatSettingsCommand, IconNames.Settings, Se.Language.Tools.BatchConvert.TargetFormatSettings)
            .WithMarginLeft(5)
            .WithMarginRight(5);
        buttonTargetFormatSettings.WithBindIsVisible(vm, nameof(vm.IsTargetFormatSettingsVisible));
        var buttonSettings = UiUtil.MakeButton(vm.ShowOutputPropertiesCommand, IconNames.Settings, Se.Language.General.Settings).WithMarginLeft(15).WithMarginRight(5);

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

        // hack to make drag and drop work on the file grid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);
        dropHost.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dropHost);

        // Layer a "please wait" overlay on top of the file grid for while files are being added
        var fileGridContainer = new Panel
        {
            Children =
            {
                dropHost,
                MakeBusyOverlay(vm),
            }
        };

        grid.Add(fileGridContainer, 0, 0);
        grid.Add(panelFileControls, 1, 0);
        grid.Add(panelFilter, 2, 0);

        var border = UiUtil.MakeBorderForControlNoPadding(grid);
        return border;
    }

    private static Border MakeBusyOverlay(BatchConvertViewModel vm)
    {
        var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
        var cardBackground = new SolidColorBrush(isDark ? UiUtil.GetDarkThemeBackgroundColor() : Colors.White);

        var fileNameLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            MaxWidth = 360,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.8,
            Margin = new Thickness(0, 6, 0, 0),
        };
        fileNameLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.AddingFilesStatus)) { Source = vm });

        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Width = 220,
            Height = 6,
            Margin = new Thickness(0, 12, 0, 0),
        };
        progressBar.Bind(ProgressBar.MaximumProperty, new Binding(nameof(vm.AddingFilesProgressMax)) { Source = vm });
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.AddingFilesProgressValue)) { Source = vm });

        var cancelButton = UiUtil.MakeButtonCancel(vm.CancelAddFilesCommand);
        cancelButton.HorizontalAlignment = HorizontalAlignment.Center;
        cancelButton.Margin = new Thickness(0, 16, 0, 0);

        var card = new Border
        {
            Background = cardBackground,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(30, 22),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock
                    {
                        Text = Se.Language.General.PleaseWait,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                    fileNameLabel,
                    progressBar,
                    cancelButton,
                }
            }
        };

        var overlay = new Border
        {
            Background = new SolidColorBrush(Colors.Black, 0.35),
            IsVisible = false,
            Child = card,
        };
        overlay.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsAddingFiles)) { Source = vm });

        return overlay;
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
        // The DataGrid this replaces hid its header row (HeadersVisibility.None);
        // TableView has no such switch, so the two columns now show headers. No
        // header-click sorting: the functions are a curated checklist in a fixed order
        // (the old grid's headers were hidden, so its sorting was unreachable anyway).
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        // Fixed width: this grid sits in an Auto-sized outer column, and a TableView
        // with a star column measured without a width constraint demands more than
        // the whole window (star columns have no content-based size).
        dataGrid.Width = 360;
        dataGrid.Height = 300;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.BatchFunctions;
        dataGrid.ContextFlyout = new MenuFlyout()
        {
            Items =
            {
                new MenuItem()
                {
                    Header = Se.Language.General.SelectAll,
                    Command = vm.SelectAllCommand,
                },
                new MenuItem()
                {
                    Header = Se.Language.General.InvertSelection,
                    Command = vm.InvertSelectionCommand,
                },
                new MenuItem()
                {
                    Header = Se.Language.General.SelectNone,
                    Command = vm.SelectNoneCommand,
                }
            }
        };
        dataGrid.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Enabled,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<BatchConvertFunction>((item, _) =>
                new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(0),
                    Child = MakeSelectedCheckBox(vm)
                }),
                Width = new GridLength(80), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Name,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BatchConvertFunction.Name)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        TableViewExtras.BindSelectedItem(dataGrid, vm, nameof(vm.SelectedBatchFunction));
        // The DataGrid-era CheckboxMultiSelect helper is replaced by native selection,
        // AddSpaceToggle (Space toggles the checkbox) and a SelectionChanged hook that
        // shows the selected function's settings view (was onFocusedItemChanged).
        dataGrid.SelectionChanged += (_, _) => vm.SelectedFunctionChanged();
        TableViewExtras.AddSpaceToggle<BatchConvertFunction>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    private static CheckBox MakeSelectedCheckBox(BatchConvertViewModel vm)
    {
        var checkBox = new CheckBox
        {
            Focusable = false,
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

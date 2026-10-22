using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert, in the editor look (see <see cref="EditorChrome"/>): the file toolbar and filter
/// on top, the file list with the target format and output under it, the function checklist and the
/// selected function's settings at the bottom; progress and Convert in the button bar.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var buttonConvert = new SeSplitButton
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
        buttonConvert.Classes.Add("primary");
        buttonConvert.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.IsConverting)) { Converter = InverseBooleanConverter.Instance });
        AutomationProperties.SetName(buttonConvert, Se.Language.General.Convert);

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand).WithBindIsVisible(nameof(vm.IsConverting), InverseBooleanConverter.Instance);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(vm, nameof(vm.IsConverting));

        // "No files to convert" and the end-of-run summary
        var statusText = EditorChrome.MakeBoundHint(nameof(vm.StatusText));
        statusText.FontSize = EditorChrome.FieldFontSize;
        var buttonBar = EditorChrome.MakeButtonBar(statusText, buttonCancel, buttonDone, buttonConvert);

        var progressBar = UiUtil.MakeProgressBar(6);
        progressBar.Maximum = 1; // ProgressValue is the converted fraction
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressStrip = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsConverting));

        // The file list and the functions share the height, resizable with the splitter between them,
        // so both stay usable and the progress and the buttons reachable at the minimum height.
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // file toolbar + filter
                new RowDefinition(new GridLength(1, GridUnitType.Star)) { MinHeight = 180 }, // file list, target format, output
                new RowDefinition(GridLength.Auto), // splitter
                new RowDefinition(new GridLength(1, GridUnitType.Star)) { MinHeight = 220 }, // functions + the selected function's settings
                new RowDefinition(GridLength.Auto), // progress, while converting
                new RowDefinition(GridLength.Auto), // status + buttons
            },
        };

        // The splitter doubles as the line between the file list and the functions.
        var splitter = new GridSplitter
        {
            Height = 5,
            Background = EditorChrome.LineBrush,
            ResizeDirection = GridResizeDirection.Rows,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        root.Add(MakeToolbar(vm), 0, 0);
        root.Add(MakeFileView(vm), 1, 0);
        root.Add(splitter, 2, 0);
        root.Add(MakeFunctionsPanel(vm), 3, 0);
        root.Add(progressStrip, 4, 0);
        root.Add(buttonBar, 5, 0);

        Content = root;

        UiUtil.FocusOnFirstActivation(this, buttonDone); // hack to make OnKeyDown work
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A section that fills its height: small caps header (with optional content on the right) over
    /// <paramref name="content"/>, which gets the rest - unlike <see cref="EditorChrome.MakeEditorSection"/>,
    /// which stacks, so a list in it would get unlimited height.
    /// </summary>
    private static Grid MakeFillSection(string title, Control content, Control? headerRight = null, Control? headerFill = null)
    {
        var header = new DockPanel();
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        var sectionHeader = EditorChrome.MakeSectionHeader(title);
        header.Children.Add(sectionHeader);
        if (headerFill != null)
        {
            // Between the title and the right-hand content, taking the width left over.
            DockPanel.SetDock(sectionHeader, Dock.Left);
            header.Children.Add(headerFill);
        }

        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(content, 1, 0);
        return grid;
    }

    private static Border MakeToolbar(BatchConvertViewModel vm)
    {
        // Add/Remove/Clear are locked while converting: the run works on a snapshot of the list,
        // so removed files would still be converted and added files silently skipped (#15116).
        Button Action(Button button)
        {
            button.WithBindEnabled(nameof(vm.AreControlsEnabled));
            return button;
        }

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Action(EditorChrome.MakeActionButton("fa-solid fa-plus", Se.Language.General.Add, vm.AddFilesCommand)),
                Action(EditorChrome.MakeActionButton("fa-solid fa-folder-plus", Se.Language.Tools.BatchConvert.AddFolderDotDotDot, vm.AddFolderCommand)),
                Action(EditorChrome.MakeActionButton("fa-solid fa-folder-tree", Se.Language.Tools.BatchConvert.AddFolderRecursiveDotDotDot, vm.AddFolderRecursiveCommand)),
                EditorChrome.MakeToolbarSeparator(),
                Action(EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Remove, vm.RemoveSelectedFilesCommand)),
                Action(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearAllFilesCommand)),
            }
        };

        // Not while converting: rows mid-OCR ("OCR...", "OCR x%") would be filtered as errors.
        var comboBoxFilter = UiUtil.MakeComboBox(vm.FilterItems, vm, nameof(vm.SelectedFilterItem))
            .WithBindEnabled(nameof(vm.AreControlsEnabled));
        comboBoxFilter.SelectionChanged += (_, _) => vm.FilterComboBoxChanged();
        comboBoxFilter.MinWidth = 150;
        AutomationProperties.SetName(comboBoxFilter, Se.Language.General.Filter);
        var textBoxFilter = UiUtil.MakeTextBox(200, vm, nameof(vm.FilterText))
            .WithBindIsVisible(nameof(vm.IsFilterTextVisible))
            .WithBindEnabled(nameof(vm.AreControlsEnabled))
            .WithSearchAndClearIcons();
        textBoxFilter.Classes.Add("text");
        textBoxFilter.TextChanged += (_, _) => vm.FilterTextChanged();
        AutomationProperties.SetName(textBoxFilter, Se.Language.General.Filter);
        var filter = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-filter", FontSize = 12, Foreground = EditorChrome.FaintTextBrush, VerticalAlignment = VerticalAlignment.Center },
                comboBoxFilter,
                textBoxFilter,
            }
        };

        var bar = new DockPanel();
        DockPanel.SetDock(filter, Dock.Right);
        bar.Children.Add(filter);
        bar.Children.Add(actions);
        return EditorChrome.MakeToolbar(bar);
    }

    private static Border MakeFileView(BatchConvertViewModel vm)
    {
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
            NameBinding = new Binding(nameof(BatchConvertItem.Status)),
            CellTemplate = new FuncDataTemplate<BatchConvertItem>((_, _) =>
            {
                // Status as a colored badge: green converted, red errors, gray cancelled;
                // in-progress statuses render as plain text (converter returns unset).
                var text = new TextBlock
                {
                    FontSize = UiUtil.ScaledFontSize(11),
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
        dataGrid.WithAccessibleName(Se.Language.General.SubtitleFiles);

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
            Converter = InverseBooleanConverter.Instance,
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
            Converter = InverseBooleanConverter.Instance,
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

        var menuItemImportFolder = new MenuItem
        {
            Header = Se.Language.Tools.BatchConvert.AddFolderDotDotDot,
            DataContext = vm,
            Command = vm.AddFolderCommand,
        };
        flyout.Items.Add(menuItemImportFolder);

        var menuItemImportFolderRecursive = new MenuItem
        {
            Header = Se.Language.Tools.BatchConvert.AddFolderRecursiveDotDotDot,
            DataContext = vm,
            Command = vm.AddFolderRecursiveCommand,
        };
        flyout.Items.Add(menuItemImportFolderRecursive);

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
                EditorChrome.MakeListFrame(dropHost),
                MakeBusyOverlay(vm),
            }
        };

        var count = EditorChrome.MakeBoundHint(nameof(vm.BatchItemsInfo));
        var grid = new Grid
        {
            RowSpacing = 10,
            RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) },
        };
        grid.Add(MakeFillSection(Se.Language.General.SubtitleFiles, fileGridContainer, count), 0, 0);
        grid.Add(MakeTargetRow(vm), 1, 0);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14, 12, 14, 12),
        };
    }

    /// <summary>
    /// Target format (with its settings) on the left, where the files go and with which encoding on
    /// the right, and the output settings button.
    /// </summary>
    private static Grid MakeTargetRow(BatchConvertViewModel vm)
    {
        var comboBoxSubtitleFormat = UiUtil.MakeComboBox(vm.TargetFormats, vm, nameof(vm.SelectedTargetFormat));
        comboBoxSubtitleFormat.SelectionChanged += (_, _) => vm.ComboBoxSubtitleFormatChanged();
        // Tunnel phase so right-click / Mac Ctrl+click opens the format picker before the ComboBox
        // consumes the click to open its dropdown (matches the main window's format combo).
        comboBoxSubtitleFormat.AddHandler(InputElement.PointerPressedEvent,
            vm.ComboBoxSubtitleFormatPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        comboBoxSubtitleFormat.Width = 240;
        AutomationProperties.SetName(comboBoxSubtitleFormat, Se.Language.General.TargetFormat);

        var buttonTargetFormatSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", Se.Language.Tools.BatchConvert.TargetFormatSettings, vm.ShowTargetFormatSettingsCommand, 28);
        buttonTargetFormatSettings.WithBindIsVisible(vm, nameof(vm.IsTargetFormatSettingsVisible));
        // Only offered while a Transport Stream file is in the list - the settings apply to nothing else.
        var buttonTransportStreamSettings = EditorChrome.CreateToolButton("fa-solid fa-file-video", Se.Language.Tools.BatchConvert.TransportStreamSettingsDotDotDot, vm.ShowTransportStreamSettingsCommand, 28);
        buttonTransportStreamSettings.WithBindIsVisible(vm, nameof(vm.IsTransportStreamSettingsVisible));

        var target = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.TargetFormat, new Thickness(0, 0, 2, 0)),
                comboBoxSubtitleFormat,
                buttonTargetFormatSettings,
                buttonTransportStreamSettings,
            }
        };

        // Output folder: "Source folder" as text, or the chosen folder as a link that opens it
        var labelOutputSourceFolder = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.OutputFolderLabel)),
        };
        var linkLabelOutputFolder = UiUtil.MakeLink(string.Empty, vm.OpenOutputFolderCommand, vm, nameof(vm.OutputFolderLinkLabel))
            .WithAlignmentLeft();
        linkLabelOutputFolder.FontSize = EditorChrome.FieldFontSize;
        linkLabelOutputFolder.TextTrimming = TextTrimming.CharacterEllipsis;
        var folder = new Panel { VerticalAlignment = VerticalAlignment.Center, Children = { labelOutputSourceFolder, linkLabelOutputFolder } };
        var encoding = EditorChrome.MakeBoundHint(nameof(vm.OutputEncodingLabel));

        var output = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
            VerticalAlignment = VerticalAlignment.Center,
        };
        output.Add(new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-folder", FontSize = 12, Foreground = EditorChrome.FaintTextBrush, VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        output.Add(folder, 0, 1);
        output.Add(encoding, 1, 1);

        var buttonSettings = EditorChrome.CreateToolButton("fa-solid fa-sliders", Se.Language.General.Settings, vm.ShowOutputPropertiesCommand, 28);

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        row.Add(target, 0, 0);
        row.Add(EditorChrome.MakeToolbarSeparator(), 0, 1);
        row.Add(output, 0, 2);
        row.Add(buttonSettings, 0, 3);
        return row;
    }

    private static Border MakeBusyOverlay(BatchConvertViewModel vm)
    {
        var fileNameLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            MaxWidth = 360,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
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
        // A folder scan has no known total (the tree is being walked) - show a marquee for it.
        progressBar.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsScanningFolder)) { Source = vm });

        var cancelButton = UiUtil.MakeButtonCancel(vm.CancelAddFilesCommand);
        cancelButton.HorizontalAlignment = HorizontalAlignment.Center;
        cancelButton.Margin = new Thickness(0, 16, 0, 0);

        var card = new Border
        {
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
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
                        FontWeight = FontWeight.SemiBold,
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
            CornerRadius = new CornerRadius(8),
            IsVisible = false,
            Child = card,
        };
        overlay.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsAddingFiles)) { Source = vm });

        return overlay;
    }

    /// <summary>
    /// The function checklist (with the presets below it) and the selected function's settings
    /// beside it, on the panel color.
    /// </summary>
    private static Border MakeFunctionsPanel(BatchConvertViewModel vm)
    {
        var comboBoxPreset = UiUtil.MakeComboBox(vm.PresetNames, vm, nameof(vm.SelectedPresetName));
        comboBoxPreset.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxPreset.PlaceholderText = Se.Language.General.Preset;
        comboBoxPreset.WithAccessibleName(Se.Language.General.Preset);
        var buttonSavePreset = EditorChrome.CreateToolButton("fa-solid fa-floppy-disk", Se.Language.Tools.BatchConvert.SavePresetDotDotDot, vm.SavePresetCommand, 28);
        var buttonDeletePreset = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.Tools.BatchConvert.DeletePreset, vm.DeletePresetCommand, 28);
        var panelPresets = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        panelPresets.Add(EditorChrome.MakeDimLabel(Se.Language.General.Preset, new Thickness(0, 0, 4, 0)), 0, 0);
        panelPresets.Add(comboBoxPreset, 0, 1);
        panelPresets.Add(buttonSavePreset, 0, 2);
        panelPresets.Add(buttonDeletePreset, 0, 3);

        var functions = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) },
        };
        functions.Add(MakeFunctionsList(vm), 0, 0);
        functions.Add(panelPresets, 1, 0);

        // "N actions selected" beside the header, and the function search between the two,
        // so searching costs no height of its own.
        var actionsSelected = EditorChrome.MakeBoundHint(nameof(vm.ActionsSelected));
        actionsSelected.VerticalAlignment = VerticalAlignment.Center;
        actionsSelected.Margin = new Thickness(8, 0, 0, 0);
        var textBoxSearch = EditorChrome.MakeSearchBox(vm, nameof(vm.FunctionSearchText), Se.Language.General.Search, double.NaN);
        textBoxSearch.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxSearch.Margin = new Thickness(12, -4, 0, -4); // overlaps the header's line height a little rather than growing it
        textBoxSearch.MaxWidth = 220;
        var left = new Border
        {
            Child = MakeFillSection(Se.Language.General.Options, functions, actionsSelected, textBoxSearch),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };

        var right = new Border
        {
            Child = MakeFillSection(Se.Language.General.Settings, MakeFunctionView(vm)),
            Padding = new Thickness(14, 12, 14, 14),
        };

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        columns.Add(left, 0, 0);
        columns.Add(right, 0, 1);

        return new Border
        {
            Child = columns,
            Background = EditorChrome.PanelBrush,
        };
    }

    private static Border MakeFunctionsList(BatchConvertViewModel vm)
    {
        // The DataGrid this replaces hid its header row (HeadersVisibility.None);
        // TableView has no such switch, so the two columns now show headers. No
        // header-click sorting: the functions are a curated checklist in a fixed order
        // (the old grid's headers were hidden, so its sorting was unreachable anyway).
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        // Fixed width: this grid sits in an Auto-sized outer column, and a TableView
        // with a star column measured without a width constraint demands more than
        // the whole window (star columns have no content-based size).
        dataGrid.Width = FunctionsListMinWidth;
        dataGrid.Height = double.NaN; // fills the functions panel
        // Widen to the longest function name once the effective font is known - 360 fits English,
        // but Russian/Bulgarian/French names are cut off (#15901).
        dataGrid.Loaded += (_, _) => dataGrid.Width = GetFunctionsListWidth(dataGrid, vm.BatchFunctions.Select(f => f.Name));
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.VisibleBatchFunctions;
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
        // The "N actions selected" label above is empty until something is ticked, so a static name (#12087).
        dataGrid.WithAccessibleName(Se.Language.General.Options);
        // The DataGrid-era CheckboxMultiSelect helper is replaced by native selection,
        // AddSpaceToggle (Space toggles the checkbox) and a SelectionChanged hook that
        // shows the selected function's settings view (was onFocusedItemChanged).
        dataGrid.SelectionChanged += (_, _) => vm.SelectedFunctionChanged();
        TableViewExtras.AddSpaceToggle<BatchConvertFunction>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var frame = EditorChrome.MakeListFrame(dataGrid);
        frame.HorizontalAlignment = HorizontalAlignment.Left;
        return frame;
    }

    private const double FunctionsListMinWidth = 360;
    private const double FunctionsListMaxWidth = 560;

    internal static double GetFunctionsListWidth(Control list, IEnumerable<string> names)
    {
        var maxNameWidth = 0.0;
        foreach (var name in names)
        {
            var textBlock = new TextBlock
            {
                Text = name,
                FontFamily = list.GetValue(TextElement.FontFamilyProperty),
                FontSize = list.GetValue(TextElement.FontSizeProperty),
            };
            textBlock.Measure(Size.Infinity);
            maxNameWidth = Math.Max(maxNameWidth, textBlock.DesiredSize.Width);
        }

        // Enabled column (80) + name cell padding + vertical scroll bar + border, with some slack.
        const double chrome = 120;
        return Math.Clamp(Math.Ceiling(maxNameWidth + chrome), FunctionsListMinWidth, FunctionsListMaxWidth);
    }

    private static CheckBox MakeSelectedCheckBox(BatchConvertViewModel vm)
    {
        var checkBox = new CheckBox
        {
            Focusable = false,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(BatchConvertFunction.IsSelected)),
            [!AutomationProperties.NameProperty] = new Binding(nameof(BatchConvertFunction.Name)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
            MinHeight = 0,
            Padding = new Thickness(0),
        };

        // The Fluent template gives the box a fixed 32 high frame, which made every function row that
        // tall; 24 keeps a little air between the boxes in the default compact grid.
        checkBox.TemplateApplied += (_, e) =>
        {
            foreach (var frame in e.NameScope.Find<Grid>("RootGrid")?.Children.OfType<Grid>() ?? [])
            {
                frame.Height = 24;
                frame.VerticalAlignment = VerticalAlignment.Center;
            }
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
            Padding = new Thickness(12, 10, 12, 10),
            Width = double.NaN,
            Height = double.NaN, // fills the functions panel
        };
        vm.FunctionContainer = scrollViewer;

        // The function views are swapped in after the window has opened, so the one-time
        // label pass in InitializeWindow never sees them; label each view as it is shown (#12087).
        scrollViewer.PropertyChanged += (_, e) =>
        {
            if (e.Property == ContentControl.ContentProperty && scrollViewer.Content is Control view)
            {
                AccessibleLabels.Apply(view);
            }
        };

        return new Border
        {
            Child = scrollViewer,
            Background = EditorChrome.WindowBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
    }
}

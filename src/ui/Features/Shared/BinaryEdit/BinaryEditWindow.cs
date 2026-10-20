using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

/// <summary>
/// The image-based subtitle editor in the editor look (see <see cref="EditorChrome"/>): menu bar,
/// the image list with the selected line's timing/position below it, the video preview or position
/// map on the right, and the frame rate with Cancel/OK in the button bar.
/// </summary>
public class BinaryEditWindow : Window
{
    public BinaryEditWindow(BinaryEditViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.EditImagedBaseSubtitle;
        Width = 1200;
        Height = 700;
        MinWidth = 1000;
        MinHeight = 600;
        CanResize = true;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // Menu
                new RowDefinition(GridLength.Star), // Content
                new RowDefinition(GridLength.Auto), // Button panel
            },
        };

        // Top menu bar
        var menu = MakeTopMenu(vm);
        vm.Menu = menu;
        var menuBar = new Border
        {
            Child = menu,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
        if (OperatingSystem.IsMacOS())
        {
            menu.IsVisible = false;
            menuBar.IsVisible = false;
        }
        mainGrid.Add(menuBar, 0);

        // Content area (grid + video)
        var leftColumnDef = new ColumnDefinition(GridLength.Star);
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                leftColumnDef,
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
        };

        // Left section - grid with subtitles lines + controls
        var leftContent = MakeLayoutListViewAndEditBox(vm, out var controlsGrid);
        contentGrid.Add(leftContent, 0);

        const double splitterWidth = UiUtil.SplitterWidthOrHeight;
        var splitter = new GridSplitter
        {
            Width = splitterWidth,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = EditorChrome.LineBrush,
        };
        contentGrid.Add(splitter, 0, 1);

        // Right section - video player / position map, switched by a segmented toggle.
        // Both panels stay in the visual tree (IsVisible-switched) so the mpv video
        // surface is never detached.
        var videoPanel = MakeVideoPlayer(vm);
        videoPanel.Bind(Visual.IsVisibleProperty, new Binding("!" + nameof(vm.IsPositionMonitorActive)) { Source = vm });

        var monitorPanel = MakePositionMonitorPanel(vm);
        monitorPanel.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsPositionMonitorActive)) { Source = vm });

        var panelHost = new Panel { Margin = new Thickness(14, 12, 14, 14) };
        panelHost.Children.Add(videoPanel);
        panelHost.Children.Add(monitorPanel);

        var rightGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
            },
        };
        rightGrid.Add(MakeRightPanelViewToggle(vm), 0);
        rightGrid.Add(panelHost, 1);
        contentGrid.Add(rightGrid, 0, 2);

        mainGrid.Add(contentGrid, 1);

        // Status bar and button panel
        var statusTextBlock = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };

        // The frame rate exports are written at, and where it came from (issue #15549).
        var frameRatePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Margin = new Thickness(0, 0, 10, 0),
            [!ToolTip.TipProperty] = new Binding(nameof(vm.FrameRateHint)),
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.FrameRate, new Thickness(0)),
                new ComboBox
                {
                    Width = 110,
                    VerticalAlignment = VerticalAlignment.Center,
                    [AutomationProperties.NameProperty] = Se.Language.General.FrameRate,
                    [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.FrameRates)),
                    [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedFrameRate)),
                },
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonBar = EditorChrome.MakeButtonBar(statusTextBlock, frameRatePanel, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonOk);
        mainGrid.Add(buttonBar, 2);

        Content = mainGrid;
        AddHandler(KeyDownEvent, (_, args) => vm.OnKeyDown(args), handledEventsToo: true);
        AddHandler(KeyUpEvent, (_, args) => vm.OnKeyUp(args), handledEventsToo: true);

        // Tunnelling, and handledEventsToo since controls may mark their presses handled: needed to see
        // that Alt was held for a mouse gesture and cancel the menu-bar activation that would otherwise
        // fire on the Alt release (discussion #11744).
        AddHandler(PointerPressedEvent, vm.OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        Loaded += (_, _) =>
        {
            // Measure controls at natural (unconstrained) size so MinWidth is font-size-aware
            controlsGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            const double borderAndMarginOverhead = 28; // controls section padding: 14 left + 14 right
            var leftPanelMin = controlsGrid.DesiredSize.Width + borderAndMarginOverhead;
            leftColumnDef.MinWidth = leftPanelMin;   // prevents splitter from squeezing the controls
            var computedMinWidth = Math.Ceiling(leftPanelMin * 2 + splitterWidth) + 10;
            MinWidth = computedMinWidth;
            if (Width < MinWidth)
                Width = MinWidth;

            if (OperatingSystem.IsMacOS())
            {
                InitNativeMacMenuBinaryEdit.Setup(this, vm);
            }
            vm.Loaded();
        };
        Closing += vm.OnClosing;
        Deactivated += vm.OnWindowDeactivated;
    }

    private static Menu MakeTopMenu(BinaryEditViewModel vm)
    {
        var l = Se.Language.Main.Menu;
        var menu = new Menu
        {
            Height = 30,
            DataContext = vm,
        };

        // File menu
        menu.Items.Add(new MenuItem
        {
            Header = l.File,
            Items =
            {
                new MenuItem
                {
                    Header = l.Open,
                    Command = vm.FileOpenCommand,
                    Icon = new Icon { Value = IconNames.FolderOpen },
                },
                new MenuItem
                {
                    Header = l.Export,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = Se.Language.General.BluRaySup,
                            Command = vm.ExportBluRaySupCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.BdnXml,
                            Command = vm.ExportBdnXmlCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.BdnXml8Bit,
                            Command = vm.ExportBdnXml8BitCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportImscImage,
                            Command = vm.ExportImscImageCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDCinemaInteropPng,
                            Command = vm.ExportDCinemaInteropPngCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDCinemaSmpte2014Png,
                            Command = vm.ExportDCinemaSmpte2014PngCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDCinemaSmpte2014PngMxf,
                            Command = vm.ExportDCinemaSmpte2014PngMxfCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDostPng,
                            Command = vm.ExportDostPngCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDvdSup,
                            Command = vm.ExportDvdSupCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportFcpImage,
                            Command = vm.ExportFcpPngCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.ImagesWithHtmlIndex,
                            Command = vm.ExportHtmlIndexCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.ImagesWithTimeCode,
                            Command = vm.ExportImagesWithTimeCodeCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportVobSub,
                            Command = vm.ExportVobSubCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportWebVttThumbnails,
                            Command = vm.ExportWebVttThumbnailCommand,
                        },
                    }
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.ImportTimeCodesDotDotDot,
                    Command = vm.ImportTimeCodesCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Exit,
                    Command = vm.CancelCommand,
                },
            },
        });

        // Tools menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Tools,
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.AlignmentDotDotDot,
                    Command = vm.AlignmentCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.General.CenterHorizontally,
                    Command = vm.CenterHorizontallyCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.TopAlignLines,
                    Command = vm.TopAlignCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.BottomAlignLines,
                    Command = vm.BottomAlignCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.ResizeImagesDotDotDot,
                    Command = vm.ResizeImagesCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.CropImages,
                    Command = vm.CropCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.General.VideoResolution + "...",
                    Command = vm.ChangeResolutionCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.MoveCaptionsDotDotDot,
                    Command = vm.MoveCaptionsCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.AdjustBrightnessDotDotDot,
                    Command = vm.AdjustBrightnessCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.AdjustAlphaDotDotDot,
                    Command = vm.AdjustAlphaCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.AdjustColorDotDotDot,
                    Command = vm.AdjustColorCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.AdjustDurations,
                    Command = vm.AdjustDurationsCommand,
                },
                new MenuItem
                {
                    Header = l.ApplyDurationLimits,
                    Command = vm.ApplyDurationLimitsCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.SortByStartTime,
                    Command = vm.SortByStartTimeCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.RemoveFadeInOut,
                    Command = vm.RemoveFadesCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.AppendSubtitleDotDotDot,
                    Command = vm.AppendSubtitleCommand,
                },
            },
        });

        // Synchronization menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Synchronization,
            Items =
            {
                new MenuItem
                {
                    Header = l.AdjustAllTimes,
                    Command = vm.AdjustAllTimesCommand,
                },
                new MenuItem
                {
                    Header = l.ChangeFrameRate,
                    Command = vm.ChangeFrameRateCommand,
                },
                new MenuItem
                {
                    Header = l.ChangeSpeed,
                    Command = vm.ChangeSpeedCommand,
                },
            },
        });

        // Video menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Video,
            Items =
            {
                new MenuItem
                {
                    Header = l.OpenVideo,
                    Command = vm.OpenVideoCommand,
                    Icon = new Icon { Value = IconNames.Play },
                },
                new MenuItem
                {
                    Header = l.CloseVideoFile,
                    Command = vm.CloseVideoCommand,
                },
                new Separator(),
                new MenuItem
                {
                    // Where the main window has it too (its Video menu).
                    Header = l.GenerateBurnIn,
                    Command = vm.GenerateBurnInCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.ToggleSelectSubtitleWhilePlayingCurrentlyOn,
                    Command = vm.ToggleCurrentSubtitleWhilePlayingCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.SelectCurrentSubtitleWhilePlaying)),
                },
                new MenuItem
                {
                    Header = l.ToggleSelectSubtitleWhilePlayingCurrentlyOff,
                    Command = vm.ToggleCurrentSubtitleWhilePlayingCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.SelectCurrentSubtitleWhilePlaying))
                    {
                        Converter = InverseBooleanConverter.Instance,
                    },
                },
            },
        });

        // Options menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Options,
            Items =
            {
                new MenuItem
                {
                    Header = l.Settings,
                    Command = vm.SettingsCommand,
                    Icon = new Icon { Value = IconNames.Settings },
                },
            },
        });

        return menu;
    }

    private static Border MakeLayoutListViewAndEditBox(BinaryEditViewModel vm, out Grid controlsGrid)
    {
        // One brush for every preview cell in this window, so recolouring repaints them all.
        var previewBackground = ImagePreviewBackground.CreateBrush();

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star), // Subtitle grid
                new RowDefinition(GridLength.Auto), // Controls
            },
        };

        // Grid for subtitle lines (grid lines come from the TableView cell themes)
        var dataGrid = TableViewExtras.MakeTableView();

        // TableView itself is not focusable by default, and without rows there are no focusable
        // row containers either - the grid must be able to hold keyboard focus even when empty,
        // both as the menu-deactivation fallback and so the window never ends up without any
        // focusable content (same fix as the main subtitle grid, #13111).
        dataGrid.Focusable = true;

        dataGrid.Height = double.NaN;
        dataGrid.FontSize = Se.Settings.Appearance.SubtitleGridFontSize;
        dataGrid.WithAccessibleName(Se.Language.General.ImageBasedSubtitles);

        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Subtitles)));
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)) { Mode = BindingMode.Default });
        dataGrid.DoubleTapped += (_, e) => vm.OnSubtitleGridDoubleTapped(e);

        dataGrid.KeyDown += (_, e) => vm.OnSubtitleGridKeyDown(e);
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                if (target != null)
                {
                    dataGrid.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        // The DataGridCheckboxMultiSelect helper the DataGrid used provided extended
        // selection (native ListBox behavior on TableView) plus Space toggling the
        // Forced checkbox for the selected rows - keep the Space shortcut.
        TableViewExtras.AddSpaceToggle<BinarySubtitleItem>(dataGrid,
            item => item.IsForced,
            (item, value) => item.IsForced = value);

        var flyout = new MenuFlyout();
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        flyout.Opening += (_, _) => vm.OnContextMenuOpening();

        var menuItemInsertBefore = new MenuItem
        {
            Header = Se.Language.General.InsertBefore,
            DataContext = vm,
            Command = vm.InsertBeforeCommand,
        };
        flyout.Items.Add(menuItemInsertBefore);
        menuItemInsertBefore.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsInsertBeforeVisible)));

        var menuItemInsertAfter = new MenuItem
        {
            Header = Se.Language.General.InsertAfter,
            DataContext = vm,
            Command = vm.InsertAfterCommand,
        };
        flyout.Items.Add(menuItemInsertAfter);
        menuItemInsertAfter.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsInsertAfterVisible)));

        var separatorAlignment = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorAlignment);
        separatorAlignment.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemAlign = new MenuItem
        {
            Header = Se.Language.General.AlignmentDotDotDot,
            DataContext = vm,
            Command = vm.AlignmentSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemAlign);
        menuItemAlign.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemCenterH = new MenuItem
        {
            Header = Se.Language.General.CenterHorizontally,
            DataContext = vm,
            Command = vm.CenterHorizontallySelectedLinesCommand,
        };
        flyout.Items.Add(menuItemCenterH);
        menuItemCenterH.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemTopAlign = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.TopAlignLines,
            DataContext = vm,
            Command = vm.TopAlignSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemTopAlign);
        menuItemTopAlign.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemBottomAlign = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.BottomAlignLines,
            DataContext = vm,
            Command = vm.BottomAlignSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemBottomAlign);
        menuItemBottomAlign.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var separatorForced = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorForced);
        separatorForced.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemToggleForced = new MenuItem
        {
            Header = Se.Language.General.ToggleForced,
            DataContext = vm,
            Command = vm.ToggleForcedCommand,
        };
        flyout.Items.Add(menuItemToggleForced);
        menuItemToggleForced.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsToggleForcedVisible)));

        var menuItemSelectForcedLines = new MenuItem
        {
            Header = Se.Language.General.SelectForcedLines,
            DataContext = vm,
            Command = vm.SelectForcedLinesCommand,
        };
        flyout.Items.Add(menuItemSelectForcedLines);

        var menuItemSelectNonForcedLines = new MenuItem
        {
            Header = Se.Language.General.SelectNonForcedLines,
            DataContext = vm,
            Command = vm.SelectNonForcedLinesCommand,
        };
        flyout.Items.Add(menuItemSelectNonForcedLines);

        var separatorVisualOps = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorVisualOps);
        separatorVisualOps.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemResizeImagesSelectedLines = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.ResizeImagesDotDotDot,
            DataContext = vm,
            Command = vm.ResizeImagesSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemResizeImagesSelectedLines);
        menuItemResizeImagesSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemCropSelectedLines = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.CropImages,
            DataContext = vm,
            Command = vm.CropSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemCropSelectedLines);
        menuItemCropSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemMoveCaptionsSelectedLines = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.MoveCaptionsDotDotDot,
            DataContext = vm,
            Command = vm.MoveCaptionsSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemMoveCaptionsSelectedLines);
        menuItemMoveCaptionsSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        flyout.Items.Add(new Separator());

        var menuItemAdjustBrightnessSelectedLines = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.AdjustBrightnessDotDotDot,
            DataContext = vm,
            Command = vm.AdjustBrightnessSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemAdjustBrightnessSelectedLines);
        menuItemAdjustBrightnessSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemAdjustAlphaSelectedLines = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.AdjustAlphaDotDotDot,
            DataContext = vm,
            Command = vm.AdjustAlphaSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemAdjustAlphaSelectedLines);
        menuItemAdjustAlphaSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemAdjustColorSelectedLines = new MenuItem
        {
            Header = Se.Language.Tools.ImageBasedEdit.AdjustColorDotDotDot,
            DataContext = vm,
            Command = vm.AdjustColorSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemAdjustColorSelectedLines);
        menuItemAdjustColorSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var separatorDurations = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorDurations);
        separatorDurations.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemAdjustDurations = new MenuItem
        {
            Header = Se.Language.Main.Menu.AdjustDurations,
            DataContext = vm,
            Command = vm.AdjustDurationsSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemAdjustDurations);
        menuItemAdjustDurations.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemApplyDurationLimits = new MenuItem
        {
            Header = Se.Language.Main.Menu.ApplyDurationLimits,
            DataContext = vm,
            Command = vm.ApplyDurationLimitsSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemApplyDurationLimits);
        menuItemApplyDurationLimits.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemAdjustAllTimesSelectedLines = new MenuItem
        {
            Header = Se.Language.Sync.ShowEarlierLaterDotDotDot,
            DataContext = vm,
            Command = vm.AdjustAllTimesSelectedLinesCommand,
        };
        flyout.Items.Add(menuItemAdjustAllTimesSelectedLines);
        menuItemAdjustAllTimesSelectedLines.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var separatorDelete = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorDelete);
        separatorDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteSectedLinesCommand,
        };
        flyout.Items.Add(menuItemDelete);
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.HasSelection)));

        flyout.Items.Add(new Separator());
        flyout.Items.Add(ImagePreviewBackground.MakeMenuItem(vm.WindowService, () => vm.Window, previewBackground));

        vm.SubtitleGrid = dataGrid;
        dataGrid.SelectionChanged += vm.SubtitleGridSelectionChanged;

        // Columns: Zone, Forced, Number, Show, Duration, Image. All columns go through
        // the manager because the zone column toggles visibility with the position map.
        var columnManager = new TableViewColumnManager(dataGrid);

        // Position-monitor zone dot (green = active picture, amber = bottom bar,
        // red = top bar) so problem lines are visible directly in the grid.
        // Only shown while the right panel is in "Position map" mode.
        var zoneColumn = new SeTableViewColumn
        {
            Header = string.Empty,
            Width = new GridLength(30),
            MinWidth = 26,
            IsVisible = vm.IsPositionMonitorActive,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<BinarySubtitleItem>((_, _) =>
                new Ellipse
                {
                    Width = 9,
                    Height = 9,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Shape.FillProperty] = new Binding(nameof(BinarySubtitleItem.ZoneBrush)),
                }),
        };
        columnManager.Add(zoneColumn);
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsPositionMonitorActive))
            {
                zoneColumn.IsVisible = vm.IsPositionMonitorActive;
            }
        };

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Forced,
            Width = new GridLength(90),
            MinWidth = 80,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<BinarySubtitleItem>(nameof(BinarySubtitleItem.IsForced)),
        });

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new GridLength(50), // was Auto; TableView treats Auto as star
            MinWidth = 40,
            Binding = new Binding(nameof(BinarySubtitleItem.Number)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Width = new GridLength(120), // was Auto; TableView treats Auto as star
            MinWidth = 100,
            Binding = new Binding(nameof(BinarySubtitleItem.StartTime)) { Converter = new TimeSpanToDisplayFullConverter() },
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            Width = new GridLength(90), // was Auto; TableView treats Auto as star
            MinWidth = 60,
            Binding = new Binding(nameof(BinarySubtitleItem.Duration)) { Converter = new TimeSpanToDisplayShortConverter() },
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Image,
            Width = new GridLength(1, GridUnitType.Star),
            MinWidth = 80,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<BinarySubtitleItem>((_, _) =>
            {
                var image = new Image
                {
                    MaxHeight = 22,
                    Stretch = Stretch.Uniform,
                    [!Image.SourceProperty] = new Binding(nameof(BinarySubtitleItem.Bitmap)),
                };

                // Subtitle bitmaps are light or dark text on a transparent background, both
                // invisible on a matching flat row backdrop, so they get one of their own. This
                // was the mid-gray checkerboard (issue #12692), whose light squares left light
                // subtitles unreadable (#14328); it is now the same configurable colour the OCR
                // grid uses, changeable from the context menu in either window.
                return new Border
                {
                    Background = previewBackground,
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Child = image,
                };
            }),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        mainGrid.Add(EditorChrome.MakeListPane(Se.Language.General.ImageBasedSubtitles, dataGrid, margin: new Thickness(14, 12, 14, 12)), 0);

        controlsGrid = MakeControls(vm);
        mainGrid.Add(new Border
        {
            Child = controlsGrid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        }, 1);

        return new Border
        {
            Child = mainGrid,
            Background = EditorChrome.PanelBrush,
        };
    }

    private static Grid MakeControls(BinaryEditViewModel vm)
    {
        // Timing | Position | Screen size groups, spacer(*), action buttons
        var grid = new Grid { ColumnSpacing = 24, MinWidth = 600 }; // pre-load fallback; overridden dynamically in Loaded
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var startTimeUpDown = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.StartTime)}") { Mode = BindingMode.TwoWay },
        };

        var durationUpDown = new SecondsUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            [!SecondsUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Duration)}") { Mode = BindingMode.TwoWay },
        };

        grid.Add(MakeControlGroup(Se.Language.General.Timing,
            (Se.Language.General.Show, startTimeUpDown),
            (Se.Language.General.Duration, durationUpDown)), 0, 0);

        var xUpDown = MakePositionField(vm, $"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.X)}", int.MinValue, "X");
        var yUpDown = MakePositionField(vm, $"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Y)}", int.MinValue, "Y");

        // Position and size of the selected image - the forced flag lives only in the grid column
        var positionLabel = MakeInfoLabel(vm, nameof(vm.CurrentPosition));
        var sizeLabel = MakeInfoLabel(vm, nameof(vm.CurrentSize));
        grid.Add(MakeControlGroup(Se.Language.General.Position,
            ("X", xUpDown),
            ("Y", yUpDown),
            (string.Empty, positionLabel),
            (string.Empty, sizeLabel)), 0, 1);

        var screenWidthUpDown = MakePositionField(vm, nameof(vm.ScreenWidth), 1, Se.Language.Tools.ImageBasedEdit.ScreenWidth);
        var screenHeightUpDown = MakePositionField(vm, nameof(vm.ScreenHeight), 1, Se.Language.Tools.ImageBasedEdit.ScreenHeight);
        grid.Add(MakeControlGroup(Se.Language.General.VideoResolution,
            (Se.Language.Tools.ImageBasedEdit.ScreenWidth, screenWidthUpDown),
            (Se.Language.Tools.ImageBasedEdit.ScreenHeight, screenHeightUpDown)), 0, 2);

        // Actions on the selected image
        var exportBtn = EditorChrome.MakeActionButton("fa-solid fa-file-export", Se.Language.General.Export, vm.ExportImageCommand);
        var importBtn = EditorChrome.MakeActionButton("fa-solid fa-file-import", Se.Language.General.Import, vm.ImportImageCommand);
        var setTextBtn = EditorChrome.MakeActionButton("fa-solid fa-font", Se.Language.Tools.ImageBasedEdit.SetText, vm.SetTextCommand);

        var buttonStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Top,
        };
        foreach (var button in new[] { exportBtn, importBtn, setTextBtn })
        {
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            buttonStack.Children.Add(button);
        }

        grid.Add(buttonStack, 0, 4);

        xUpDown.ValueChanged += (_, _) => vm.UpdateOverlayPosition();
        yUpDown.ValueChanged += (_, _) => vm.UpdateOverlayPosition();

        return grid;
    }

    /// <summary>
    /// A small caps header above "label  control" rows that line up.
    /// </summary>
    private static StackPanel MakeControlGroup(string title, params (string Label, Control Control)[] rows)
    {
        var rowsGrid = new Grid { ColumnSpacing = 8, RowSpacing = 6 };
        rowsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        rowsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        for (var i = 0; i < rows.Length; i++)
        {
            rowsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var (label, control) = rows[i];
            if (label.Length > 0)
            {
                rowsGrid.Add(EditorChrome.MakeDimLabel(label, new Thickness(0)), i, 0);
                if (control is not TextBlock && string.IsNullOrEmpty(AutomationProperties.GetName(control)))
                {
                    AutomationProperties.SetName(control, label);
                }
            }

            rowsGrid.Add(control, i, 1);
        }

        var panel = new StackPanel { Spacing = 10, VerticalAlignment = VerticalAlignment.Top };
        panel.Children.Add(EditorChrome.MakeSectionHeader(title));
        panel.Children.Add(rowsGrid);
        return panel;
    }

    private static NumericUpDown MakePositionField(BinaryEditViewModel vm, string path, int minimum, string name)
    {
        var field = new NumericUpDown
        {
            Width = 100,
            Minimum = minimum,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(path) { Mode = BindingMode.TwoWay },
        };
        field.Classes.Add("field");
        AutomationProperties.SetName(field, name);
        return field;
    }

    private static TextBlock MakeInfoLabel(BinaryEditViewModel vm, string property)
    {
        return new TextBlock
        {
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            [!TextBlock.TextProperty] = new Binding(property),
        };
    }

    private static Border MakeVideoPlayer(BinaryEditViewModel vm)
    {
        var vp = InitVideoPlayer.MakeVideoPlayer();
        vp.FullScreenIsVisible = false;
        vp.Volume = Se.Settings.Video.Volume;
        vp.VolumeChanged += v => { Se.Settings.Video.Volume = v; };
        vp.SurfacePointerPressed += (_, _) => vm.VideoPlayerAreaPointerPressed();

        // Create a grid to hold the video player and overlay image
        var videoGrid = new Grid
        {
            ClipToBounds = true,
        };

        // Add video player as background
        videoGrid.Children.Add(vp);

        // Create a green rectangle border to show the actual video content area
        var videoContentBorder = new Border
        {
            BorderBrush = Brushes.Green,
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
        };
        videoGrid.Children.Add(videoContentBorder);
        vm.VideoContentBorder = videoContentBorder;

        // Create overlay image for subtitle bitmap
        var overlayImage = new Image
        {
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsVisible = false,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };

        videoGrid.Children.Add(overlayImage);

        // Store references
        vm.VideoPlayerControl = vp;
        vm.SubtitleOverlayImage = overlayImage;

        // Update position when video player size changes
        vp.SizeChanged += (_, _) => vm.UpdateOverlayPosition();
        vp.PropertyChanged += (_, e) =>
        {
            if (e.Property == VideoPlayerControl.PositionProperty)
            {
                vm.OnVideoPositionChanged(vp.Position);
            }
        };

        // Implement mouse dragging for overlay image
        Point? dragStartPoint = null;
        int originalX = 0;
        int originalY = 0;

        overlayImage.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(overlayImage).Properties.IsLeftButtonPressed && vm.SelectedSubtitle != null)
            {
                dragStartPoint = e.GetPosition(videoGrid);
                originalX = vm.SelectedSubtitle.X;
                originalY = vm.SelectedSubtitle.Y;
                e.Handled = true;
            }
        };

        overlayImage.PointerMoved += (_, e) =>
        {
            if (dragStartPoint.HasValue && vm.SelectedSubtitle != null)
            {
                var currentPoint = e.GetPosition(videoGrid);
                var delta = currentPoint - dragStartPoint.Value;

                // Convert screen delta to subtitle coordinate delta (inverse of scale)
                var imageWidth = overlayImage.Width;
                var bitmapWidth = vm.SelectedSubtitle.Bitmap?.Size.Width ?? 1;
                if (!double.IsNaN(imageWidth) && imageWidth > 0 && bitmapWidth > 0)
                {
                    var scale = imageWidth / bitmapWidth;
                    var deltaX = (int)(delta.X / scale);
                    var deltaY = (int)(delta.Y / scale);

                    vm.SelectedSubtitle.X = originalX + deltaX;
                    vm.SelectedSubtitle.Y = originalY + deltaY;

                    vm.UpdateOverlayPosition();
                }

                e.Handled = true;
            }
        };

        overlayImage.PointerReleased += (_, e) =>
        {
            if (dragStartPoint.HasValue)
            {
                dragStartPoint = null;
                e.Handled = true;
            }
        };

        // A plain frame: no corner radius, as the native video surface on macOS disappears inside a rounded clip.
        return new Border
        {
            Child = videoGrid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
        };
    }

    private static Control MakeRightPanelViewToggle(BinaryEditViewModel vm)
    {
        var buttonVideo = new ToggleButton
        {
            Content = Se.Language.Tools.ImageBasedEdit.VideoPreview,
        };
        var buttonMap = new ToggleButton
        {
            Content = Se.Language.Tools.ImageBasedEdit.PositionMap,
        };
        buttonVideo.Classes.Add("segment");
        buttonMap.Classes.Add("segment");

        // Explicit sync instead of IsChecked bindings: a click always toggles the
        // button locally, so clicking the already-active side would untick it with
        // no view-model change to re-assert the state.
        void Sync()
        {
            buttonVideo.IsChecked = !vm.IsPositionMonitorActive;
            buttonMap.IsChecked = vm.IsPositionMonitorActive;
        }

        buttonVideo.Click += (_, _) => { vm.IsPositionMonitorActive = false; Sync(); };
        buttonMap.Click += (_, _) => { vm.IsPositionMonitorActive = true; Sync(); };
        Sync();

        var segmented = new Border
        {
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2,
                Children = { buttonVideo, buttonMap },
            },
        };

        return new Border
        {
            Child = segmented,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 8),
        };
    }

    private static Grid MakePositionMonitorPanel(BinaryEditViewModel vm)
    {
        var lang = Se.Language.Tools.ImageBasedEdit;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // summary
                new RowDefinition(GridLength.Auto), // zone chips
                new RowDefinition(GridLength.Auto), // letterbox / title-safe controls
                new RowDefinition(GridLength.Star), // position map canvas
            },
        };

        // Summary header: "1920×1080 - 623 subtitles - bar height: 138 px"
        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12.5,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 0, 8),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PositionMonitorSummary)) { Source = vm },
        };
        grid.Add(summaryText, 0);

        // Zone chips on their own single line; clicking a chip jumps to the next
        // subtitle in that zone.
        var chipsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10),
        };
        chipsPanel.Children.Add(MakeZoneChip(vm, Color.FromRgb(0x22, 0xC5, 0x5E), nameof(vm.ActivePictureCountText), null, PositionMonitorZone.ActivePicture, isFirst: true));
        chipsPanel.Children.Add(MakeZoneChip(vm, Color.FromRgb(0xF5, 0x9E, 0x0B), nameof(vm.BottomBarCountText), nameof(vm.HasBottomBarSubtitles), PositionMonitorZone.BottomBar));
        chipsPanel.Children.Add(MakeZoneChip(vm, Color.FromRgb(0xEF, 0x44, 0x44), nameof(vm.TopBarCountText), nameof(vm.HasTopBarSubtitles), PositionMonitorZone.TopBar));
        grid.Add(chipsPanel, 1);

        // Controls row: letterbox aspect ratio + bar height, and the title-safe
        // margin. Grouped so label + control never wrap apart.
        var ratioCombo = UiUtil.MakeComboBox(vm.LetterboxRatios, vm, nameof(vm.SelectedLetterboxRatio));
        ratioCombo.MinWidth = 170;
        AutomationProperties.SetName(ratioCombo, lang.Letterbox);

        var barHeightUpDown = new NumericUpDown
        {
            Width = 80,
            Minimum = 0,
            Maximum = 2000,
            Increment = 1,
            FormatString = "F0",
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.LetterboxBarHeight)) { Mode = BindingMode.TwoWay },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsLetterboxBarHeightEditable)),
        };
        barHeightUpDown.Classes.Add("field");
        AutomationProperties.SetName(barHeightUpDown, lang.BarHeightPx);

        var titleSafeSwitch = EditorChrome.MakeSwitch(nameof(vm.ShowTitleSafeArea), lang.TitleSafePercent);
        titleSafeSwitch.DataContext = vm;

        var titleSafeUpDown = new NumericUpDown
        {
            Width = 70,
            Minimum = 0,
            Maximum = 20,
            Increment = 0.5m,
            FormatString = "F1",
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.TitleSafePercent)) { Mode = BindingMode.TwoWay },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.ShowTitleSafeArea)),
        };
        titleSafeUpDown.Classes.Add("field");
        AutomationProperties.SetName(titleSafeUpDown, lang.TitleSafePercent);

        var letterboxGroup = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 20, 8),
            Children =
            {
                EditorChrome.MakeDimLabel(lang.Letterbox, new Thickness(0)),
                ratioCombo,
                EditorChrome.MakeDimLabel(lang.BarHeightPx, new Thickness(6, 0, 0, 0)),
                barHeightUpDown,
            },
        };

        var titleSafeGroup = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 8),
            Children =
            {
                titleSafeSwitch,
                new TextBlock { Text = lang.TitleSafePercent, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center },
                titleSafeUpDown,
            },
        };

        var controlsPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 2),
            Children = { letterboxGroup, titleSafeGroup },
        };
        grid.Add(controlsPanel, 2);

        // The position map canvas
        var monitor = new PositionMonitorControl();
        monitor.ItemClicked += (_, item) => vm.SelectSubtitleFromPositionMonitor(item);
        vm.PositionMonitor = monitor;

        var monitorBorder = new Border
        {
            Child = monitor,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
        grid.Add(monitorBorder, 3);

        return grid;
    }

    private static Border MakeZoneChip(BinaryEditViewModel vm, Color color, string textPropertyName, string? highlightPropertyName, PositionMonitorZone zone, bool isFirst = false)
    {
        var bullet = new Ellipse
        {
            Width = 9,
            Height = 9,
            Fill = new SolidColorBrush(color),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 1, 5, 0),
        };

        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(textPropertyName) { Source = vm },
        };

        var chip = new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 2, 9, 2),
            Margin = new Thickness(isFirst ? 0 : 10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(color, 0.12),
            Cursor = new Cursor(StandardCursorType.Hand),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { bullet, text },
            },
        };

        // Chip doubles as a "go to next subtitle in this zone" button.
        chip.PointerPressed += (_, _) => vm.SelectNextInZone(zone);

        if (highlightPropertyName != null)
        {
            // Outline the chip when any subtitle falls in this letterbox bar - the
            // at-a-glance warning from discussion #12318.
            chip.Bind(Border.BorderBrushProperty, new Binding(highlightPropertyName)
            {
                Source = vm,
                Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, IBrush?>(on =>
                    on ? new SolidColorBrush(color, 0.85) : Brushes.Transparent),
            });
            chip.BorderThickness = new Thickness(1.5);
        }

        return chip;
    }
}

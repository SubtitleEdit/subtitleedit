using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Projektanker.Icons.Avalonia;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

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
        mainGrid.Add(menu, 0);

        // Content area (grid + video)
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
        };

        // Left section - grid with subtitles lines + controls
        var leftContent = MakeLayoutListViewAndEditBox(vm);
        contentGrid.Add(leftContent, 0);

        // Vertical splitter
        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        contentGrid.Add(splitter, 0, 1);

        // Right section - video player
        var rightContent = new Border
        {
            Child = MakeVideoPlayer(vm),
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
        };
        contentGrid.Add(rightContent, 0, 2);

        mainGrid.Add(contentGrid, 1);

        // Status bar and button panel
        var bottomPanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Margin = new Thickness(5),
        };

        var statusTextBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };
        bottomPanel.Add(statusTextBlock, 0);

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));
        bottomPanel.Add(buttonPanel, 1);

        mainGrid.Add(bottomPanel, 2);

        Content = mainGrid;
        KeyDown += (_, args) => vm.OnKeyDown(args);
        Loaded += (_, _) => vm.Loaded();
        Closing += (_, _) => vm.Closing();
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
                            Header = "DOST/png",
                            Command = vm.ExportDostPngCommand,
                        },
                        new MenuItem
                        {
                            Header = "FCP/png",
                            Command = vm.ExportFcpPngCommand,
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
                            Header = "WebVTT png",
                            Command = vm.ExportWebVttThumbnailCommand,
                        },
                    }
                },
                new Separator(),
                new MenuItem
                {
                    Header = "Import time codes...",
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
            Header = l.ToolsSelectedLines,
            Items =
            {
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
                new MenuItem
                {
                    Header = Se.Language.General.AlignmentDotDotDot,
                    Command = vm.AlignmentCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.ResizeImagesDotDotDot,
                    Command = vm.ResizeImagesCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.ImageBasedEdit.AdjustBrightnessDotDotDot,
                    Command = vm.AdjustBrightnessCommand,
                },
                new MenuItem
                {
                    Header =  Se.Language.Tools.ImageBasedEdit.AdjustAlphaDotDotDot,
                    Command = vm.AdjustAlphaCommand,
                },
                new MenuItem
                {
                    Header =  Se.Language.Tools.ImageBasedEdit.CenterHorizontally,
                    Command = vm.CenterHorizontallyCommand,
                },
                new MenuItem
                {
                    Header =  Se.Language.Tools.ImageBasedEdit.CropImages,
                    Command = vm.CropCommand,
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

    private static Grid MakeLayoutListViewAndEditBox(BinaryEditViewModel vm)
    {
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star), // DataGrid
                new RowDefinition(GridLength.Auto), // Controls
            },
            Margin = new Thickness(5),
        };

        // DataGrid for subtitle lines
        var dataGrid = new DataGrid
        {
            Height = double.NaN,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            CanUserResizeColumns = true,
            GridLinesVisibility = UiUtil.GetGridLinesVisibility(),
            VerticalGridLinesBrush = UiUtil.GetBorderBrush(),
            HorizontalGridLinesBrush = UiUtil.GetBorderBrush(),
            FontSize = Se.Settings.Appearance.SubtitleGridFontSize,
        };

        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Subtitles)));
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)) { Mode = BindingMode.Default });

        dataGrid.KeyDown += (_, e) => vm.OnDataGridKeyDown(e);

        var flyout = new MenuFlyout();
        dataGrid.ContextFlyout = flyout;
        flyout.Opening += (_, _) => vm.OnContextMenuOpening();

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteSectedLinesCommand,
        };
        flyout.Items.Add(menuItemDelete);
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)));

        var separatorInsert = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorInsert);
        separatorInsert.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsInsertBeforeVisible)));

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

        var menuItemToggleForced = new MenuItem
        {
            Header = Se.Language.General.ToggleForced,
            DataContext = vm,
            Command = vm.ToggleForcedCommand,
        };
        flyout.Items.Add(menuItemToggleForced);
        menuItemToggleForced.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsToggleForcedVisible)));

        var separatorInsertSubtitle = new Separator() { DataContext = vm };
        flyout.Items.Add(separatorInsertSubtitle);
        separatorInsertSubtitle.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsInsertSubtitleVisible)));

        var menuItemInsertSubtitle = new MenuItem
        {
            Header = Se.Language.General.InsertSubtitleAfterCurrentLine,
            DataContext = vm,
            Command = vm.InsertSubtitleCommand,
        };
        flyout.Items.Add(menuItemInsertSubtitle);
        menuItemInsertSubtitle.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsInsertSubtitleVisible)));

        vm.SubtitleGrid = dataGrid;

        var dataGridBorder = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        dataGridBorder.Margin = new Thickness(0, 0, 0, 5);

        // Columns: Forced, Number, Show, Duration, Text, Image
        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Forced,
            Width = new DataGridLength(60),
            MinWidth = 50,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<BinarySubtitleItem>((_, _) =>
                new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(BinarySubtitleItem.IsForced)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new DataGridLength(50),
            MinWidth = 40,
            IsReadOnly = true,
            Binding = new Binding(nameof(BinarySubtitleItem.Number)),
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Width = new DataGridLength(120),
            MinWidth = 100,
            IsReadOnly = true,
            Binding = new Binding(nameof(BinarySubtitleItem.StartTime)) { Converter = new TimeSpanToDisplayFullConverter() },
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Duration,
            Width = new DataGridLength(80),
            MinWidth = 60,
            IsReadOnly = true,
            Binding = new Binding(nameof(BinarySubtitleItem.Duration)) { Converter = new TimeSpanToDisplayShortConverter() },
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Image,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            MinWidth = 80,
            IsReadOnly = true,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<BinarySubtitleItem>((_, _) =>
            {
                var image = new Image
                {
                    MaxHeight = 22,
                    Stretch = Stretch.Uniform,
                    [!Image.SourceProperty] = new Binding(nameof(BinarySubtitleItem.Bitmap)),
                };
                return image;
            }),
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        mainGrid.Add(dataGridBorder, 0);

        var controlsPanel = MakeControls(vm);
        mainGrid.Add(UiUtil.MakeBorderForControl(controlsPanel), 1);

        return mainGrid;
    }

    private static StackPanel MakeControls(BinaryEditViewModel vm)
    {
        // Controls section - using vertical StackPanel with two rows
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 10, 0, 0),
        };

        // First row - Start Time, Duration, Forced
        var firstRowPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 10),
        };

        // Start Time
        var startTimePanel = new StackPanel { Orientation = Orientation.Vertical };
        startTimePanel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Show,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 2),
        });
        var startTimeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.StartTime)}")
            {
                Mode = BindingMode.TwoWay,
            },
        };
        startTimePanel.Children.Add(startTimeUpDown);
        firstRowPanel.Children.Add(startTimePanel);

        // Duration
        var durationPanel = new StackPanel { Orientation = Orientation.Vertical };
        durationPanel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Duration,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 2),
        });
        var durationUpDown = new SecondsUpDown
        {
            DataContext = vm,
            [!SecondsUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Duration)}")
            {
                Mode = BindingMode.TwoWay
            },
        };
        durationPanel.Children.Add(durationUpDown);
        firstRowPanel.Children.Add(durationPanel);
        firstRowPanel.Children.Add(UiUtil.MakeLabel().WithBindText(vm, nameof(vm.CurrentPositionAndSize)));

        controlsPanel.Children.Add(firstRowPanel);

        // Second row - X, Y, Buttons
        var secondRowPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        };

        // X Position
        var xPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        xPanel.Children.Add(new TextBlock
        {
            Text = "X",
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
        });
        var xUpDown = new NumericUpDown
        {
            Width = 130,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.X)}")
            {
                Mode = BindingMode.TwoWay
            },
        };
        xPanel.Children.Add(xUpDown);
        secondRowPanel.Children.Add(xPanel);

        // Y Position
        var yPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        yPanel.Children.Add(new TextBlock
        {
            Text = "Y",
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
        });
        var yUpDown = new NumericUpDown
        {
            Width = 130,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Y)}")
            {
                Mode = BindingMode.TwoWay
            },
        };
        yPanel.Children.Add(yUpDown);
        secondRowPanel.Children.Add(yPanel);

        // Buttons
        var buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var exportImageButton = UiUtil.MakeButton(vm.ExportImageCommand, IconNames.Export);
        ToolTip.SetTip(exportImageButton, Se.Language.General.Export);
        buttonsPanel.Children.Add(exportImageButton);

        var importImageButton = UiUtil.MakeButton(vm.ImportImageCommand, IconNames.Import);
        ToolTip.SetTip(importImageButton, Se.Language.General.Import);
        buttonsPanel.Children.Add(importImageButton);

        var setTextButton = UiUtil.MakeButton(vm.SetTextCommand, IconNames.NewText);
        ToolTip.SetTip(setTextButton, "Set text for subtitle");
        buttonsPanel.Children.Add(setTextButton);

        secondRowPanel.Children.Add(buttonsPanel);

        controlsPanel.Children.Add(secondRowPanel);

        // Third row - Screen Width, Screen Height
        var thirdRowPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 10, 0, 0),
        };

        // Screen Width
        var screenWidthPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        screenWidthPanel.Children.Add(new TextBlock
        {
            Text = "Screen Width",
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
        });
        var screenWidthUpDown = new NumericUpDown
        {
            Width = 130,
            Minimum = 1,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.ScreenWidth))
            {
                Mode = BindingMode.TwoWay
            },
        };
        screenWidthPanel.Children.Add(screenWidthUpDown);
        thirdRowPanel.Children.Add(screenWidthPanel);

        // Screen Height
        var screenHeightPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        screenHeightPanel.Children.Add(new TextBlock
        {
            Text = "Screen Height",
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
        });
        var screenHeightUpDown = new NumericUpDown
        {
            Width = 130,
            Minimum = 1,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.ScreenHeight))
            {
                Mode = BindingMode.TwoWay
            },
        };
        screenHeightPanel.Children.Add(screenHeightUpDown);
        thirdRowPanel.Children.Add(screenHeightPanel);

        controlsPanel.Children.Add(thirdRowPanel);

        // Subscribe to X and Y changes to update overlay position
        xUpDown.ValueChanged += (_, _) => vm.UpdateOverlayPosition();
        yUpDown.ValueChanged += (_, _) => vm.UpdateOverlayPosition();


        return controlsPanel;
    }

    private static Grid MakeVideoPlayer(BinaryEditViewModel vm)
    {
        var vp = InitVideoPlayer.MakeVideoPlayer();
        vp.FullScreenIsVisible = false;

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
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            [!Visual.IsVisibleProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Bitmap)}")
            {
                Converter = new NotNullConverter()
            },
            [!Image.SourceProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Bitmap)}"),
        };

        videoGrid.Children.Add(overlayImage);

        // Store references
        vm.VideoPlayerControl = vp;
        vm.SubtitleOverlayImage = overlayImage;

        // Update position when video player size changes
        vp.SizeChanged += (_, _) => vm.UpdateOverlayPosition();

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

        return videoGrid;
    }
}
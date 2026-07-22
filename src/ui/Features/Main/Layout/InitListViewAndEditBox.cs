using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static partial class InitListViewAndEditBox
{

    public static Grid MakeLayoutListViewAndEditBox(MainView mainPage, MainViewModel vm)
    {
        mainPage.DataContext = vm;

        // Unhook events from the old SubtitleGrid if it exists
        if (vm.SubtitleGrid != null)
        {
            vm.SubtitleGrid.SelectionChanged -= vm.SubtitleGrid_SelectionChanged;
            vm.SubtitleGrid.Tapped -= vm.OnSubtitleGridSingleTapped;
            vm.SubtitleGrid.DoubleTapped -= vm.OnSubtitleGridDoubleTapped;

            if (vm.SubtitleGridDropHost != null)
            {
                vm.SubtitleGridDropHost.PointerPressed -= vm.SubtitleGrid_PointerPressed;
                vm.SubtitleGridDropHost.RemoveHandler(InputElement.DoubleTappedEvent, vm.SubtitleGridDropHost_DoubleTapped);
                vm.SubtitleGridDropHost.RemoveHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed);
                vm.SubtitleGridDropHost.RemoveHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased);
                vm.SubtitleGridDropHost.RemoveHandler(InputElement.PointerMovedEvent, vm.SubtitleGrid_PointerMoved);
                vm.SubtitleGridDropHost.ContextFlyout = null;
                vm.SubtitleGridDropHost = null;
            }

            // Clear the grid to help with garbage collection
            vm.SubtitleGrid.ItemsSource = null;
        }

        vm.SubtitleGridAlternatingRowBrush = null;

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
        };

        vm.SubtitleGrid = new DataGrid
        {
            Height = double.NaN,
            Margin = new Thickness(Se.Settings.Appearance.GridCompactMode ? 0 : 2),
            ItemsSource = vm.Subtitles,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            DataContext = vm.Subtitles,
            CanUserResizeColumns = true,
            GridLinesVisibility = DataGridGridLinesVisibility.None, // Grid lines are rendered via cell themes
            VerticalGridLinesBrush = UiUtil.GetBorderBrush(),
            HorizontalGridLinesBrush = UiUtil.GetBorderBrush(),
            FontSize = Se.Settings.Appearance.SubtitleGridFontSize,
        };

        // Trough paging and shift+click jump now come from the app-wide DataGrid style
        // (DataGridScrollBarBehavior.EnableTroughPaging in Styles.axaml), so no per-grid call here.

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = vm.SubtitleGrid
        };
        vm.SubtitleGridDropHost = dropHost;
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.SubtitleGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.SubtitleGridOnDrop, RoutingStrategies.Bubble);

        vm.SubtitleGrid.Tapped += vm.OnSubtitleGridSingleTapped;
        dropHost.AddHandler(InputElement.DoubleTappedEvent, vm.SubtitleGridDropHost_DoubleTapped, RoutingStrategies.Bubble, handledEventsToo: true);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var doubleRoundedConverter = new DoubleToOneDecimalConverter();
        var cpsWmpConverter = new DoubleToOneDecimalHideMaxConverter();
        var notNullConverter = new NotNullConverter();
        var nullToOpacityConverter = new NullToOpacityConverter();
        var syntaxHighlightingConverter = new TextWithSubtitleSyntaxHighlightingConverter();
        var textToFlowDirectionConverter = new TextToFlowDirectionConverter();
        vm.SubtitleDataGridSyntaxHighlighting = syntaxHighlightingConverter;
        // How the Text/Original cells fit their text to the window (feature #11590). Read once here;
        // the grid is rebuilt when settings are applied, so a changed mode takes effect then.
        var gridTextDisplayMode = SubtitleGridTextDisplayModeDisplay.FromSettings();
        var gapConverter = new DoubleToDisplayShortConverter();
        var inverseBooleanConverter = new InverseBooleanConverter();
        var textOneLineShortConverter = new TextOneLineShortConverter();
        var booleanToGridLengthConverter = new BooleanToGridLengthConverter();
        var booleanAndConverter = BooleanAndConverter.Instance;

        // Optional alternating row background (Options > Settings > Appearance)
        SolidColorBrush? alternatingRowBrush = null;
        if (Se.Settings.Appearance.GridAlternatingRows)
        {
            var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
            var altColorHex = isDark
                ? Se.Settings.Appearance.GridAlternatingRowColorDark
                : Se.Settings.Appearance.GridAlternatingRowColor;
            try
            {
                if (!string.IsNullOrWhiteSpace(altColorHex))
                {
                    alternatingRowBrush = new SolidColorBrush(altColorHex.FromHexToColor());
                    vm.SubtitleGridAlternatingRowBrush = alternatingRowBrush;
                }
            }
            catch
            {
                alternatingRowBrush = null;
            }
        }

        // Convert row indexes to alternating background brushes when the option is enabled.
        var alternatingRowBrushConverter = alternatingRowBrush == null
            ? null
            : new FuncValueConverter<int, IBrush>(index => index % 2 == 1 ? alternatingRowBrush : Brushes.Transparent);

        // Set up data binding for row visibility based on IsHidden property
        vm.SubtitleGrid.LoadingRow += (sender, e) =>
        {
            e.Row.Bind(DataGridRow.IsVisibleProperty, new Binding(nameof(SubtitleLineViewModel.IsHidden))
            {
                Converter = inverseBooleanConverter
            });

            // Tint every other row. Binding to Index keeps the color in sync when rows are recycled,
            // inserted, or removed. Selection still wins because :selected overrides BackgroundRectangle.Fill.
            if (alternatingRowBrushConverter != null)
            {
                e.Row.Bind(DataGridRow.BackgroundProperty, new Binding(nameof(DataGridRow.Index))
                {
                    Source = e.Row,
                    Converter = alternatingRowBrushConverter,
                });
            }
        };

        vm.SubtitleGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Tag = SubtitleGridColumnKeys.Number,
            Width = new DataGridLength(50),
            MinWidth = 40,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, namescope) =>
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                         new Icon
                         {
                            Value = IconNames.Bookmark,
                            Foreground = new SolidColorBrush(Se.Settings.Appearance.BookmarkColor.FromHexToColor()),
                            VerticalAlignment = VerticalAlignment.Center,
                            IsHitTestVisible = false,
                            [!Visual.OpacityProperty] = new Binding(nameof(SubtitleLineViewModel.Bookmark)) { Converter = nullToOpacityConverter },
                         },
                         UiUtil.MakeLabel().WithBindText(value, new Binding(nameof(SubtitleLineViewModel.Number)))
                    }
                })
        });

        var startColumn = new DataGridTemplateColumn
        {
            Header = Se.Language.General.Show,
            Tag = SubtitleGridColumnKeys.Start,
            Width = new DataGridLength(120),
            MinWidth = 100,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.StartTimeBackgroundBrush)),
                };
                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                };
                border.Child = textBlock;
                return border;
            }),
        };
        vm.SubtitleGrid.Columns.Add(startColumn);
        startColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnStartTime))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        var hideColumn = new DataGridTemplateColumn
        {
            Header = Se.Language.General.Hide,
            Tag = SubtitleGridColumnKeys.End,
            Width = new DataGridLength(120),
            MinWidth = 100,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.EndTimeBackgroundBrush)),
                };
                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                };
                border.Child = textBlock;
                return border;
            }),
        };
        vm.SubtitleGrid.Columns.Add(hideColumn);
        hideColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnEndTime))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        var columnDuration = new DataGridTemplateColumn
        {
            Header = Se.Language.General.Duration,
            Tag = SubtitleGridColumnKeys.Duration,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
            MinWidth = 60,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.DurationBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                };

                border.Child = textBlock;
                return border;
            })
        };
        vm.SubtitleGrid.Columns.Add(columnDuration);
        columnDuration.Bind(DataGridTextColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnDuration))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        vm.SubtitleGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Text,
            Tag = SubtitleGridColumnKeys.Text,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            MinWidth = 100,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.TextBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,

                    // Lets the subtitle grid context menu find the word under the pointer (live spell check)
                    Tag = SubtitleGridColumnKeys.Text,
                    [!TextBlock.InlinesProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = syntaxHighlightingConverter, Mode = BindingMode.OneWay },
                    [!TextBlock.FlowDirectionProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = textToFlowDirectionConverter, Mode = BindingMode.OneWay },
                };
                SubtitleGridTextDisplayModeDisplay.ApplyTo(textBlock, gridTextDisplayMode);

                if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                {
                    textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                }

                border.Child = textBlock;
                return border;
            })
        });

        var originalColumn = new DataGridTemplateColumn
        {
            Header = Se.Language.General.OriginalText,
            Tag = SubtitleGridColumnKeys.OriginalText,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star), // Stretch text column
            MinWidth = 100,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,

                    // Lets the subtitle grid context menu find the word under the pointer (live spell check)
                    Tag = SubtitleGridColumnKeys.OriginalText,
                    [!TextBlock.InlinesProperty] = new Binding(nameof(SubtitleLineViewModel.OriginalText)) { Converter = syntaxHighlightingConverter, Mode = BindingMode.OneWay },
                    [!TextBlock.FlowDirectionProperty] = new Binding(nameof(SubtitleLineViewModel.OriginalText)) { Converter = textToFlowDirectionConverter, Mode = BindingMode.OneWay },
                };
                SubtitleGridTextDisplayModeDisplay.ApplyTo(textBlock, gridTextDisplayMode);

                if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                {
                    textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                }

                border.Child = textBlock;
                return border;
            })
        };
        originalColumn.Bind(DataGridTextColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });
        vm.SubtitleGrid.Columns.Add(originalColumn);

        var styleColumn = new DataGridTextColumn
        {
            Header = Se.Language.General.Style,
            Tag = SubtitleGridColumnKeys.Style,
            Binding = new Binding(nameof(SubtitleLineViewModel.Style)),
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        };

        var styleColumnMultiBinding = new MultiBinding
        {
            Converter = booleanAndConverter,
            Bindings =
            {
                new Binding(nameof(vm.HasFormatStyle)) { Source = vm, Mode = BindingMode.OneWay },
                new Binding(nameof(vm.ShowColumnStyle)) { Source = vm, Mode = BindingMode.OneWay }
            }
        };
        styleColumn.Bind(DataGridColumn.IsVisibleProperty, styleColumnMultiBinding);
        vm.SubtitleGrid.Columns.Add(styleColumn);

        var columnGap = new DataGridTemplateColumn
        {
            Header = Se.Language.General.Gap,
            Tag = SubtitleGridColumnKeys.Gap,
            Width = new DataGridLength(100),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.GapBackgroundBrush)) { Mode = BindingMode.OneWay },
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Gap)) { Converter = gapConverter, Mode = BindingMode.OneWay },
                };

                border.Child = textBlock;
                return border;
            })
        };
        columnGap.Bind(DataGridTextColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnGap))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });
        vm.SubtitleGrid.Columns.Add(columnGap);

        var actorColumn = new DataGridTextColumn
        {
            Header = Se.Language.General.Actor,
            Tag = SubtitleGridColumnKeys.Actor,
            Binding = new Binding(nameof(SubtitleLineViewModel.Actor)) { Mode = BindingMode.OneWay },
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        };
        vm.SubtitleGrid.Columns.Add(actorColumn);
        actorColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnActor))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        var cpsColumn = new DataGridTemplateColumn
        {
            Header = Se.Language.General.Cps,
            Tag = SubtitleGridColumnKeys.Cps,
            Width = new DataGridLength(100),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.CpsBackgroundBrush)) { Mode = BindingMode.OneWay }
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.CharactersPerSecond)) { Converter = cpsWmpConverter, Mode = BindingMode.OneWay },
                };

                border.Child = textBlock;
                return border;
            })
        };
        vm.SubtitleGrid.Columns.Add(cpsColumn);
        cpsColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnCps))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });
        
        var wpmColumn = new DataGridTemplateColumn
        {
            Header = Se.Language.General.Wpm,
            Tag = SubtitleGridColumnKeys.Wpm,
            Width = new DataGridLength(100),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.WpmBackgroundBrush)) { Mode = BindingMode.OneWay }
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.WordsPerMinute)) { Converter = cpsWmpConverter, Mode = BindingMode.OneWay },
                };

                border.Child = textBlock;
                return border;
            })
        };
        vm.SubtitleGrid.Columns.Add(wpmColumn);
        wpmColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnWpm))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });
        
        var pixelWidthColumn = new DataGridTemplateColumn
        {
            Header = Se.Language.General.PixelWidth,
            Tag = SubtitleGridColumnKeys.PixelWidth,
            Width = new DataGridLength(100),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.PixelWidth)) {  Mode = BindingMode.OneWay },
                };
                return textBlock;
            })
        };
        vm.SubtitleGrid.Columns.Add(pixelWidthColumn);
        pixelWidthColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnPixelWidth))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        var layerColumn = new DataGridTextColumn
        {
            Header = Se.Language.General.Layer,
            Tag = SubtitleGridColumnKeys.Layer,
            Binding = new Binding(nameof(SubtitleLineViewModel.Layer)),
            Width = new DataGridLength(23),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        };
        vm.SubtitleGrid.Columns.Add(layerColumn);
        layerColumn.Bind(DataGridColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnLayer))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        // A narrow empty trailing column that reserves space for the DataGrid's overlay
        // vertical scrollbar, so the bar covers this gutter instead of the outermost text
        // column (issue #12351). This is the same "give the scrollbar its own space"
        // approach used for the shortcut key column in the Shortcuts window; a trailing
        // column is used here (rather than a fixed cell margin) because right to left mode
        // moves the scrollbar to the other side and mirrors the grid, so the gutter follows
        // it automatically in both directions without leaving a gap between the Text and
        // Original columns in translation mode. Kept out of AutoFitColumns so it stays this
        // fixed width, and excluded from width persistence as a non-stretchy layout helper.
        vm.SubtitleGrid.Columns.Add(new DataGridTemplateColumn
        {
            Tag = SubtitleGridColumnKeys.ScrollbarGutter,
            Header = string.Empty,
            Width = new DataGridLength(20, DataGridLengthUnitType.Pixel),
            MinWidth = 0,
            CanUserResize = false,
            CanUserReorder = false,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) => new Border()),
        });

        RestoreSubtitleGridColumnWidths(vm.SubtitleGrid);

        vm.SubtitleGrid.DataContext = vm.Subtitles;
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;


        // Set up two-way binding for SelectedItem
        vm.SubtitleGrid[!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedSubtitle))
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
        };

        // Set up two-way binding for SelectedIndex
        vm.SubtitleGrid[!DataGrid.SelectedIndexProperty] = new Binding(nameof(vm.SelectedSubtitleIndex))
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
        };

        Grid.SetRow(dropHost, 0);
        mainGrid.Children.Add(dropHost);

        // Create a Flyout for the DataGrid
        var flyout = new MenuFlyout();

        flyout.Opening += vm.SubtitleContextOpening;

        var assaStylesMenuItem = new MenuItem
        {
            Header = Se.Language.General.Styles,
            DataContext = vm,
        };
        assaStylesMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AreAssaContentMenuItemsVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(assaStylesMenuItem);
        vm.MenuItemStyles = assaStylesMenuItem;

        var assaActorsMenuItem = new MenuItem
        {
            Header = Se.Language.General.Actors,
            DataContext = vm,
        };
        assaActorsMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AreAssaContentMenuItemsVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(assaActorsMenuItem);
        vm.MenuItemActors = assaActorsMenuItem;

        var sepAssa = new Separator { DataContext = vm };
        sepAssa.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AreAssaContentMenuItemsVisible)));
        flyout.Items.Add(sepAssa);

        var showStartTimeMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowStartColumn,
            Command = vm.ToggleShowColumnStartTimeCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnStartTime)),
            }
        };
        showStartTimeMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showStartTimeMenuItem);
        
        var showEndTimeMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowHideColumn,
            Command = vm.ToggleShowColumnEndTimeCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnEndTime)),
            }
        };
        showEndTimeMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showEndTimeMenuItem);

        var showDurationMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowDurationColumn,
            Command = vm.ToggleShowColumnDurationCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnDuration)),
            }
        };
        showDurationMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showDurationMenuItem);

        var showGapMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowGapColumn,
            Command = vm.ToggleShowColumnGapCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnGap)),
            }
        };
        showGapMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showGapMenuItem);

        var showStyleMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowStyleColumn,
            Command = vm.ToggleShowColumnStyleCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnStyle)),
            }
        };
        var showStyleColumnMultiBinding = new MultiBinding
        {
            Converter = booleanAndConverter,
            Bindings =
            {
                new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Source = vm, Mode = BindingMode.OneWay },
                new Binding(nameof(vm.HasFormatStyle)) { Source = vm, Mode = BindingMode.OneWay }
            }
        };
        showStyleMenuItem.Bind(Visual.IsVisibleProperty, showStyleColumnMultiBinding);

        flyout.Items.Add(showStyleMenuItem);

        var showActorMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowActorColumn,
            Command = vm.ToggleShowColumnActorCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnActor)),
            }
        };
        showActorMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showActorMenuItem);

        var showCpsMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowCpsColumn,
            Command = vm.ToggleShowColumnCpsCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnCps)),
            }
        };
        showCpsMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showCpsMenuItem);

        var showWpmMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowWpmColumn,
            Command = vm.ToggleShowColumnWpmCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnWpm)),
            }
        };
        showWpmMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showWpmMenuItem);
        
        var showPixelWidthMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowPixelWidthColumn,
            Command = vm.ToggleShowColumnPixelWidthCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnPixelWidth)),
            }
        };
        showPixelWidthMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(showPixelWidthMenuItem);

        var showLayerMenuItem = new MenuItem
        {
            Header = Se.Language.General.ShowLayerColumn,
            Command = vm.ToggleShowColumnLayerCommand,
            DataContext = vm,
            Icon = new Icon
            {
                Value = IconNames.CheckBold,
                VerticalAlignment = VerticalAlignment.Center,
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnLayer)),
            }
        };
        showLayerMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnLayerFlyoutMenuItem)) { Source = vm, Mode = BindingMode.TwoWay });
        flyout.Items.Add(showLayerMenuItem);


        var deleteMenuItem = new MenuItem { Header = Se.Language.General.Delete, DataContext = vm };
        deleteMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        deleteMenuItem.Command = vm.DeleteSelectedLinesCommand;
        flyout.Items.Add(deleteMenuItem);

        var insertBeforeMenuItem = new MenuItem { Header = Se.Language.General.InsertBefore, DataContext = vm };
        insertBeforeMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        insertBeforeMenuItem.Command = vm.InsertLineBeforeCommand;
        flyout.Items.Add(insertBeforeMenuItem);

        var insertAfterMenuItem = new MenuItem { Header = Se.Language.General.InsertAfter, DataContext = vm };
        insertAfterMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        insertAfterMenuItem.Command = vm.InsertLineAfterCommand;
        flyout.Items.Add(insertAfterMenuItem);

        var insertLineMenuItem = new MenuItem { Header = Se.Language.General.InsertLine, DataContext = vm };
        insertLineMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsInsertLineNoSelectionVisible)));
        insertLineMenuItem.Command = vm.InsertLineAtEndCommand;
        flyout.Items.Add(insertLineMenuItem);

        var insertSubtitleFileAfterLineMenuItem = new MenuItem { Header = Se.Language.General.InsertSubtitleAfterCurrentLine, DataContext = vm };
        insertSubtitleFileAfterLineMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsInsertSubtitleFileAfterLineVisible)));
        insertSubtitleFileAfterLineMenuItem.Command = vm.InsertSubtitleFileAfterThisLineCommand;
        flyout.Items.Add(insertSubtitleFileAfterLineMenuItem);

        var copyOriginal = new MenuItem { Header = Se.Language.Main.CopyTextFromOriginalToCurrent, Command = vm.ColumnCopyTextFromOriginalToCurrentCommand };
        copyOriginal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText)));

        var columnMenuItem = new MenuItem
        {
            Header = Se.Language.General.Column,
            DataContext = vm,
            Items =
            {
                new MenuItem { Header = Se.Language.Main.DeleteText, Command = vm.ColumnDeleteTextCommand },
                new MenuItem { Header = Se.Language.Main.DeleteTextAndShiftCellsUp, Command = vm.ColumnDeleteTextAndShiftCellsUpCommand},
                new MenuItem { Header = Se.Language.Main.InsertEmptyTextAndShiftCellsDown, Command = vm.ColumnInsertEmptyTextAndShiftCellsDownCommand },
                new MenuItem { Header = Se.Language.Main.InsertTextFromSubtitleDotDotDot, Command = vm.ColumnInsertTextFromSubtitleCommand },
                copyOriginal,
                new MenuItem { Header = Se.Language.Main.PasteFromClipboardDotDotDot, Command = vm.ColumnPasteFromClipboardCommand},
                new MenuItem { Header = Se.Language.Main.TextUp, Command = vm.ColumnTextUpCommand },
                new MenuItem { Header = Se.Language.Main.TextDown, Command = vm.ColumnTextDownCommand },
            }
        };
        columnMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(columnMenuItem);

        var sep1 = new Separator { DataContext = vm };
        sep1.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(sep1);

        var splitMenuItem = new MenuItem { Header = Se.Language.General.SplitLine, DataContext = vm };
        splitMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        splitMenuItem.Command = vm.SplitCommand;
        flyout.Items.Add(splitMenuItem);

        var mergePreviousMenuItem = new MenuItem { Header = Se.Language.General.MergeBefore, DataContext = vm };
        mergePreviousMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsMergeWithNextOrPreviousVisible)));
        mergePreviousMenuItem.Command = vm.MergeWithLineBeforeCommand;
        flyout.Items.Add(mergePreviousMenuItem);

        var mergeNextMenuItem = new MenuItem { Header = Se.Language.General.MergeAfter, DataContext = vm };
        mergeNextMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsMergeWithNextOrPreviousVisible)));
        mergeNextMenuItem.Command = vm.MergeWithLineAfterCommand;
        flyout.Items.Add(mergeNextMenuItem);

        var mergeSelectedMenuItem = new MenuItem { Header = Se.Language.General.MergeSelected, DataContext = vm };
        mergeSelectedMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        mergeSelectedMenuItem.Command = vm.MergeSelectedLinesCommand;
        flyout.Items.Add(mergeSelectedMenuItem);
        vm.MenuItemMerge = mergeSelectedMenuItem;

        var mergeSelectedAsDialogMenuItem = new MenuItem { Header = Se.Language.General.MergeSelectedAsDialog, DataContext = vm };
        mergeSelectedAsDialogMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        mergeSelectedAsDialogMenuItem.Command = vm.MergeSelectedLinesDialogCommand;
        flyout.Items.Add(mergeSelectedAsDialogMenuItem);
        vm.MenuItemMergeAsDialog = mergeSelectedAsDialogMenuItem;

        var extendToLineBeforeMenuItem = new MenuItem { Header = Se.Language.General.ExtendBefore, DataContext = vm };
        extendToLineBeforeMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        extendToLineBeforeMenuItem.Command = vm.ExtendSelectedToPreviousCommand;
        flyout.Items.Add(extendToLineBeforeMenuItem);
        vm.MenuItemExtendToLineBefore = extendToLineBeforeMenuItem;

        var extendToLineAfterMenuItem = new MenuItem { Header = Se.Language.General.ExtendAfter, DataContext = vm };
        extendToLineAfterMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        extendToLineAfterMenuItem.Command = vm.ExtendSelectedToNextCommand;
        flyout.Items.Add(extendToLineAfterMenuItem);
        vm.MenuItemExtendToLineAfter = extendToLineAfterMenuItem;

        var sep2 = new Separator { DataContext = vm };
        sep2.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(sep2);

        var RemoveFormattingMenuItem = new MenuItem
        {
            Header = Se.Language.General.RemoveFormatting,
            DataContext = vm,
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.RemoveAllFormatting,
                    Command = vm.RemoveFormattingAllCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.RemoveBold,
                    Command = vm.RemoveFormattingBoldCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.RemoveItalic,
                    Command = vm.RemoveFormattingItalicCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.RemoveUnderline,
                    Command = vm.RemoveFormattingUnderlineCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.RemoveColor,
                    Command = vm.RemoveFormattingColorCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.RemoveFontName,
                    Command = vm.RemoveFormattingFontNameCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.RemoveAlignment,
                    Command = vm.RemoveFormattingAligmentCommand,
                    DataContext = vm,
                },
            }
        };
        RemoveFormattingMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(RemoveFormattingMenuItem);


        var italicMenuItem = new MenuItem
        {
            Header = Se.Language.General.Italic,
            Command = vm.ToggleLinesItalicCommand,
            DataContext = vm,
        };
        italicMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(italicMenuItem);

        var boldMenuItem = new MenuItem
        {
            Header = Se.Language.General.Bold,
            Command = vm.ToggleLinesBoldCommand,
            DataContext = vm,
        };
        boldMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(boldMenuItem);

        var colorMenuItem = new MenuItem
        {
            Header = Se.Language.General.ColorDotDotDot,
            Command = vm.ShowColorPickerCommand,
            DataContext = vm,
        };
        colorMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(colorMenuItem);

        var fontNameMenuItem = new MenuItem
        {
            Header = Se.Language.General.FontNameDotDotDot,
            Command = vm.ShowFontNamePickerCommand,
            DataContext = vm,
        };
        fontNameMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(fontNameMenuItem);


        var alignmentMenuItem = new MenuItem
        {
            Header = Se.Language.General.AlignmentDotDotDot,
            Command = vm.ShowAlignmentPickerCommand,
            DataContext = vm,
        };
        alignmentMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(alignmentMenuItem);

        var bookmarkMenuItem = new MenuItem
        {
            Header = Se.Language.General.BookmarkDotDotDot,
            Command = vm.AddOrEditBookmarkCommand,
            DataContext = vm,
        };
        bookmarkMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(bookmarkMenuItem);

        var menuItemSelectedLines = new MenuItem
        {
            Header = Se.Language.General.SelectedLines,
            DataContext = vm,
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.SpeechToText,
                    Command = vm.SpeechToTextSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.TextToSpeech,
                    Command = vm.ShowVideoTextToSpeechCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.AutoTranslate,
                    Command = vm.AutoTranslateSelectedLinesCommand,
                    DataContext = vm,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowAutoTranslateSelectedLines)),
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.ChangeCasing,
                    Command = vm.ChangeCasingSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.SetLayer,
                    Command = vm.ShowPickLayerCommand,
                    DataContext = vm,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowLayer)),
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.FixCommonErrors,
                    Command = vm.FixCommonErrorsSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.MultipleReplace,
                    Command = vm.MultipleReplaceSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.BeautifyTimeCodes,
                    Command = vm.ShowBeautifyTimeCodesSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.VisualSync,
                    Command = vm.ShowVisualSyncSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.RemoveTextForHearingImpaired,
                    Command = vm.RemoveTextForHearingImpairedSelectedLinesCommand,
                    DataContext = vm,
                },
                new Separator { DataContext = vm },
                new MenuItem
                {
                    Header = Se.Language.General.Unbreak,
                    Command = vm.UnbreakCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.General.AutoBreak,
                    Command = vm.AutoBreakCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.SplitBreakLongLines,
                    Command = vm.ShowToolsSplitBreakLongLinesSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.EvenlyDistributeLines,
                    Command = vm.EvenlyDistributeSelectedLinesCommand,
                    DataContext = vm,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.HasMultipleLinesSelected)),
                },
                new Separator { DataContext = vm },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.FillSelectedLinesWithClipboard,
                    Command = vm.FillSelectedLinesWithClipboardCommand,
                    DataContext = vm,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.HasMultipleLinesSelected)),
                },
                new MenuItem
                {
                    [!MenuItem.HeaderProperty] = new Binding(nameof(vm.SurroundWith1Text)),
                    Command = vm.SurroundWith1Command,
                    DataContext = vm,
                },
                new MenuItem
                {
                    [!MenuItem.HeaderProperty] = new Binding(nameof(vm.SurroundWith2Text)),
                    Command = vm.SurroundWith2Command,
                    DataContext = vm,
                },
                new MenuItem
                {
                    [!MenuItem.HeaderProperty] = new Binding(nameof(vm.SurroundWith3Text)),
                    Command = vm.SurroundWith3Command,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Video.CutVideoDotDotDot,
                    Command = vm.CutVideoSelectedLinesCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.Statistics,
                    Command = vm.StatisticsSelectedLinesCommand,
                    DataContext = vm,
                },
                new Separator { DataContext = vm },
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.SaveAs,
                    Command = vm.SaveSelectedLinesAsCommand,
                    DataContext = vm,
                },
            }
        };
        menuItemSelectedLines.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        flyout.Items.Add(menuItemSelectedLines);


        // Set the ContextFlyout on the drop host so right-clicks on empty space also show the menu
        dropHost.ContextFlyout = flyout;
        dropHost.AddHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        dropHost.AddHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        dropHost.AddHandler(InputElement.PointerMovedEvent, vm.SubtitleGrid_PointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);

        // Edit area - restructured with time controls on left, multiline text on right
        var editGrid = new Grid
        {
            Margin = new Thickness(10),
            ColumnDefinitions = new ColumnDefinitions("Auto, *"), // Two columns: left for time controls, right for text
            RowDefinitions = new RowDefinitions("Auto")
        };

        // Left panel for time controls
        var timeControlsPanel = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
        };
        
        // Start Time controls
        var startTimePanel = new StackPanel
        {
            Spacing = 0,
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 10, vm.ShowUpDownLabels ? 0 : 2),
        }.WithBindVisible(vm, nameof(vm.ShowUpDownStartTime));
        var startTimeLabel = new TextBlock
        {
            Text = Se.Language.General.Show,
            FontWeight = FontWeight.Bold
        }.WithBindVisible(vm, nameof(vm.ShowUpDownLabels));
        startTimePanel.Children.Add(startTimeLabel);
        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            UseVideoOffset = true,
            [AutomationProperties.NameProperty] = Se.Language.General.StartTime,
        };
        // With a separate end-time editor, moving start should keep the end fixed
        // (StartTimeOnly). Without one, moving start drags the whole line keeping
        // its duration (StartTimeKeepDuration).
        var startTimeBindingName = nameof(vm.SelectedSubtitle) + "." + (Se.Settings.Appearance.ShowUpDownEndTime
            ? nameof(SubtitleLineViewModel.StartTimeOnly)
            : nameof(SubtitleLineViewModel.StartTimeKeepDuration));
        timeCodeUpDown[!TimeCodeUpDown.ValueProperty] = new Binding(startTimeBindingName)
        {
            Mode = BindingMode.TwoWay,
        };

        if (!vm.ShowUpDownLabels && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(timeCodeUpDown, Se.Language.General.Show);
        }
        timeCodeUpDown.Bind(TimeCodeUpDown.IsEnabledProperty, new Binding(nameof(vm.LockTimeCodes)) { Mode = BindingMode.TwoWay, Converter = inverseBooleanConverter });
        startTimePanel.Children.Add(timeCodeUpDown);
        timeCodeUpDown.ValueChanged += vm.StartTimeChanged;
        timeControlsPanel.Children.Add(startTimePanel);


        // End Time controls
        var endTimePanel = new StackPanel
        {
            Spacing = 0,
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 10, vm.ShowUpDownLabels ? 0 : 2),
        }.WithBindVisible(vm, nameof(vm.ShowUpDownEndTime));
        var endTimeLabel = new TextBlock
        {
            Text = Se.Language.General.Hide,
            FontWeight = FontWeight.Bold
        }.WithBindVisible(vm, nameof(vm.ShowUpDownLabels));
        endTimePanel.Children.Add(endTimeLabel);
        var endCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [AutomationProperties.NameProperty] = Se.Language.General.EndTime,
            [!TimeCodeUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(SubtitleLineViewModel.EndTime)}")
            {
                Mode = BindingMode.TwoWay,
            }
        };
        if (!vm.ShowUpDownLabels && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(endCodeUpDown, Se.Language.General.Hide);
        }
        endCodeUpDown.Bind(TimeCodeUpDown.IsEnabledProperty, new Binding(nameof(vm.LockTimeCodes)) { Mode = BindingMode.TwoWay, Converter = inverseBooleanConverter });
        endTimePanel.Children.Add(endCodeUpDown);
        endCodeUpDown.ValueChanged += vm.EndTimeChanged;
        timeControlsPanel.Children.Add(endTimePanel);

        // Duration display
        var durationPanel = new StackPanel
        {
            Spacing = 0,
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 10, vm.ShowUpDownLabels ? 0 : 2),
        }.WithBindVisible(vm, nameof(vm.ShowUpDownDuration));
        var durationLabel = new TextBlock
        {
            Text = Se.Language.General.Duration,
            FontWeight = FontWeight.Bold,
        }.WithBindVisible(vm, nameof(vm.ShowUpDownLabels));
        durationPanel.Children.Add(durationLabel);
        var durationUpDown = new SecondsUpDown
        {
            DataContext = vm,
            [AutomationProperties.NameProperty] = Se.Language.General.Duration,
            [!SecondsUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(SubtitleLineViewModel.Duration)}")
            {
                Mode = BindingMode.TwoWay,
            },
            [!SecondsUpDown.BackgroundProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(SubtitleLineViewModel.DurationBackgroundBrush)}")
        };
        if (!vm.ShowUpDownLabels && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(durationUpDown, Se.Language.General.Duration);
        }
        durationUpDown.Bind(SecondsUpDown.IsEnabledProperty, new Binding(nameof(vm.LockTimeCodes)) { Mode = BindingMode.TwoWay, Converter = inverseBooleanConverter });
        durationUpDown.ValueChanged += (_, _) => vm.DurationChanged();
        durationPanel.Children.Add(durationUpDown);
        timeControlsPanel.Children.Add(durationPanel);


        // Layer display
        var panelLayer = new StackPanel
        {
            Spacing = 0,
            Orientation = Orientation.Vertical,
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowLayer)),
            Margin = new Thickness(0, 0, 10, 0),
        };
        var labelLayer = new TextBlock
        {
            Text = Se.Language.General.Layer,
            FontWeight = FontWeight.Bold,
        }.WithBindVisible(vm, nameof(vm.ShowUpDownLabels));
        panelLayer.Children.Add(labelLayer);
        var upDownLayer = UiUtil.MakeNumericUpDownInt(int.MinValue, int.MaxValue, 0, double.NaN, vm, $"{nameof(vm.SelectedSubtitle)}.{nameof(SubtitleLineViewModel.Layer)}");
        AutomationProperties.SetName(upDownLayer, Se.Language.General.Layer);
        upDownLayer.HorizontalAlignment = HorizontalAlignment.Stretch;
        if (!vm.ShowUpDownLabels && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(upDownLayer, Se.Language.General.Layer);
        }
        panelLayer.Children.Add(upDownLayer);
        timeControlsPanel.Children.Add(panelLayer);

        if (!Se.Settings.Appearance.ShowUpDownStartTime ||
            !Se.Settings.Appearance.ShowUpDownEndTime || 
            !Se.Settings.Appearance.ShowUpDownDuration)
        {
            if (Se.Settings.Appearance.ShowUpDownLabels)
            {
                timeControlsPanel.Margin = new Thickness(0, 4, 0, 0);
            }
            else
            {
                //TODO: find better way to top-align with textbox
                timeControlsPanel.Margin = new Thickness(0, 18, 0, 0);
            }
        }
        
        Grid.SetColumn(timeControlsPanel, 0);
        editGrid.Children.Add(timeControlsPanel);

        // Right panel for text editing (show/duration is to the left)
        var textEditGrid = new Grid
        {
            // RightToLeftHelper mirrors this grid by name so the current/original
            // text boxes keep matching the mirrored subtitle grid columns.
            Name = "SubtitleTextEditGrid",
            ColumnDefinitions = new ColumnDefinitions("*,*,Auto"),
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
        };

        var textLabel = new TextBlock
        {
            Text = Se.Language.General.Text,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var bookmarkIcon = new Icon
        {
            DataContext = vm,
            Value = IconNames.Bookmark,
            Foreground = new SolidColorBrush(Se.Settings.Appearance.BookmarkColor.FromHexToColor()),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.Bookmark)) { Converter = notNullConverter },
            Margin = new Thickness(6, 0, 0, 1),
            VerticalAlignment = VerticalAlignment.Center,
        };
        bookmarkIcon.PointerPressed += (_, __) =>
        {
            if (vm.AddOrEditBookmarkCommand.CanExecute(null))
            {
                vm.AddOrEditBookmarkCommand.Execute(null);
            }
        };
        var bookmarkLabel = new Label
        {
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            Foreground = new SolidColorBrush(Se.Settings.Appearance.BookmarkColor.FromHexToColor()),
            [!Label.ContentProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.Bookmark)) { Converter = textOneLineShortConverter },
            [!Label.IsVisibleProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.Bookmark)) { Converter = notNullConverter },
        };
        bookmarkLabel.PointerPressed += (_, __) =>
        {
            if (vm.AddOrEditBookmarkCommand.CanExecute(null))
            {
                vm.AddOrEditBookmarkCommand.Execute(null);
            }
        };
        var panelBookmark = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!Label.IsVisibleProperty] = new Binding(nameof(vm.SelectedSubtitle)) { Converter = notNullConverter },
            Children =
            {
                bookmarkIcon,
                bookmarkLabel,
            }
        };


        var panelForTextLabel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                textLabel,
                panelBookmark,
            }
        };


        textEditGrid.Children.Add(panelForTextLabel);

        var textCharsSecLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            FontSize = 12,
            Padding = new Thickness(2, 2, 2, 2),
        };
        textCharsSecLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.EditTextCharactersPerSecond))
        {
            Mode = BindingMode.OneWay
        });
        textCharsSecLabel.Bind(TextBlock.BackgroundProperty, new Binding(nameof(vm.EditTextCharactersPerSecondBackground))
        {
            Mode = BindingMode.OneWay
        });
        textEditGrid.Children.Add(textCharsSecLabel);
        var textEditor = MakeTextBox(vm);

        textEditGrid.Children.Add(textEditor);
        Grid.SetRow(textEditor, 1);

        var textTotalLengthLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 12,
            Padding = new Thickness(2, 2, 2, 2),
        };
        textTotalLengthLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.EditTextTotalLength))
        {
            Mode = BindingMode.OneWay
        });
        textTotalLengthLabel.Bind(TextBlock.BackgroundProperty, new Binding(nameof(vm.EditTextTotalLengthBackground))
        {
            Mode = BindingMode.OneWay
        });
        textEditGrid.Children.Add(textTotalLengthLabel);
        Grid.SetRow(textTotalLengthLabel, 2);


        var panelSingleLineLengths = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Horizontal,
        };
        vm.PanelSingleLineLengths = panelSingleLineLengths;
        textEditGrid.Children.Add(panelSingleLineLengths);
        Grid.SetRow(panelSingleLineLengths, 2);

        // Create a Flyout for the subtitle text box.
        // The TextBox may have TextAlignment=Center; that inherited property would otherwise flow
        // into the flyout's menu items. Override it at the presenter level so items are always left-aligned.
        var flyoutTextBoxPresenterTheme = new ControlTheme(typeof(MenuFlyoutPresenter))
        {
            BasedOn = Application.Current?.FindResource(typeof(MenuFlyoutPresenter)) as ControlTheme,
            Setters =
            {
                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left),
            }
        };
        var flyoutTextBox = new MenuFlyout
        {
            Placement = PlacementMode.Pointer,
            FlyoutPresenterTheme = flyoutTextBoxPresenterTheme,
        };
        textEditor.ContextFlyout = flyoutTextBox;
        flyoutTextBox.Opening += vm.TextBoxContextOpening;

        var cutMenuItem = new MenuItem { Header = Se.Language.General.Cut };
        cutMenuItem.Command = vm.TextBoxCutCommand;
        flyoutTextBox.Items.Add(cutMenuItem);

        var copyMenuItem = new MenuItem { Header = Se.Language.General.Copy };
        copyMenuItem.Command = vm.TextBoxCopyCommand;
        flyoutTextBox.Items.Add(copyMenuItem);

        var pasteMenuItem = new MenuItem { Header = Se.Language.General.Paste };
        pasteMenuItem.Command = vm.TextBoxPasteCommand;
        flyoutTextBox.Items.Add(pasteMenuItem);

        flyoutTextBox.Items.Add(new Separator());

        var menuItemTextBoxSplitAtCursor = new MenuItem { Header = Se.Language.General.SplitLineAtTextBoxCursorPosition };
        menuItemTextBoxSplitAtCursor.Command = vm.SplitAtTextBoxCursorPositionCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxSplitAtCursor);

        var menuItemTextBoxSplitAtCursorAndVideoPosition = new MenuItem { Header = Se.Language.General.SplitLineAtVideoAndTextBoxPosition };
        menuItemTextBoxSplitAtCursorAndVideoPosition.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsTextBoxSplitAtCursorAndVideoPositionVisible)));
        menuItemTextBoxSplitAtCursorAndVideoPosition.Command = vm.SplitAtVideoPositionAndTextBoxCursorPositionCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxSplitAtCursorAndVideoPosition);

        flyoutTextBox.Items.Add(new Separator());

        var menuItemTextBoxRemoveAllFormatting = new MenuItem { Header = Se.Language.General.RemoveAllFormatting };
        menuItemTextBoxRemoveAllFormatting.Command = vm.TextBoxRemoveAllFormattingCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxRemoveAllFormatting);

        var menuItemTextBoxBold = new MenuItem { Header = Se.Language.General.Bold };
        menuItemTextBoxBold.Command = vm.TextBoxBoldCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxBold);

        var menuItemTextBoxItalic = new MenuItem { Header = Se.Language.General.Italic };
        menuItemTextBoxItalic.Command = vm.TextBoxItalicCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxItalic);

        var menuItemTextBoxUnderline = new MenuItem { Header = Se.Language.General.Underline };
        menuItemTextBoxUnderline.Command = vm.TextBoxUnderlineCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxUnderline);

        var menuItemTextBoxFontName = new MenuItem { Header = Se.Language.General.FontNameDotDotDot };
        menuItemTextBoxFontName.Command = vm.TextBoxFontNameCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxFontName);

        var menuItemTextBoxColor = new MenuItem { Header = Se.Language.General.Color };
        menuItemTextBoxColor.Command = vm.TextBoxColorCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxColor);

        flyoutTextBox.Items.Add(new Separator());

        var unicodeSymbols = Se.Settings.Tools.UnicodeSymbolsToInsert.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
        if (unicodeSymbols.Length > 0)
        {
            var unicodeMenuItem = new MenuItem { Header = Se.Language.Main.InsertUnicodeSymbol };
            foreach (var symbol in unicodeSymbols)
            {
                var symbolItem = new MenuItem { Header = symbol };
                symbolItem.Command = vm.TextBoxInsertUnicodeSymbolCommand;
                symbolItem.CommandParameter = symbol;
                unicodeMenuItem.Items.Add(symbolItem);
            }
            flyoutTextBox.Items.Add(unicodeMenuItem);
        }

        flyoutTextBox.Items.Add(new Separator());

        var menuItemTextBoxAiAssistant = new MenuItem
        {
            Header = Se.Language.Tools.AiAssistant.Title,
            Command = vm.ShowAiAssistantCommand,
            Icon = new Icon
            {
                Value = IconNames.Robot,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        flyoutTextBox.Items.Add(menuItemTextBoxAiAssistant);


        // translation mode (original text)
        var textLabelOriginal = new TextBlock
        {
            Text = Se.Language.General.OriginalText,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(3, 0, 0, 0),
        };
        textEditGrid.Add(textLabelOriginal, 0, 1);
        textLabelOriginal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        var textCharsSecLabelOriginal = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            FontSize = 12,
            Padding = new Thickness(2, 2, 2, 2),
        };
        textCharsSecLabelOriginal.Bind(TextBlock.TextProperty, new Binding(nameof(vm.EditTextCharactersPerSecondOriginal))
        {
            Mode = BindingMode.OneWay
        });
        textCharsSecLabelOriginal.Bind(TextBlock.BackgroundProperty, new Binding(nameof(vm.EditTextCharactersPerSecondBackgroundOriginal))
        {
            Mode = BindingMode.OneWay
        });
        textEditGrid.Add(textCharsSecLabelOriginal, 0, 1);
        textCharsSecLabelOriginal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        var textBoxOriginal = MakeTextBoxOriginal(vm);
        textEditGrid.Add(textBoxOriginal, 1, 1);
        textBoxOriginal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        var textTotalLengthLabelOriginal = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 12,
            Padding = new Thickness(2, 2, 2, 2),
        };
        textTotalLengthLabelOriginal.Bind(TextBlock.TextProperty, new Binding(nameof(vm.EditTextTotalLengthOriginal))
        {
            Mode = BindingMode.OneWay
        });
        textTotalLengthLabelOriginal.Bind(TextBlock.BackgroundProperty, new Binding(nameof(vm.EditTextTotalLengthBackgroundOriginal))
        {
            Mode = BindingMode.OneWay
        });
        textEditGrid.Add(textTotalLengthLabelOriginal, 2, 1);
        textTotalLengthLabelOriginal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });


        var panelSingleLineLengthsOriginal = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Horizontal,
        };
        vm.PanelSingleLineLengthsOriginal = panelSingleLineLengthsOriginal;
        textEditGrid.Add(panelSingleLineLengthsOriginal, 2, 1);
        panelSingleLineLengthsOriginal.DataContext = vm;
        panelSingleLineLengthsOriginal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });
        // add label to panelSingleLineLengthsOriginal
        var singleLineLengthLabel = new TextBlock
        {
            Text = "Line lengths: x/x",
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 5, 0)
        };
        panelSingleLineLengthsOriginal.Children.Add(singleLineLengthLabel);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 3,
            Margin = new Thickness(6,3,3,3)
        };

        if (Se.Settings.Appearance.TextBoxShowButtonAutoBreak)
        {
            var autoBreakButton = UiUtil.MakeButton(vm.AutoBreakCommand, IconNames.ScaleBalance);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(autoBreakButton, Se.Language.Main.AutoBreakHint);
            }
            buttonPanel.Children.Add(autoBreakButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonUnbreak)
        {
            var unbreakButton = UiUtil.MakeButton(vm.UnbreakCommand, IconNames.SetMerge);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(unbreakButton, Se.Language.Main.UnbreakHint);
            }
            buttonPanel.Children.Add(unbreakButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonItalic)
        {
            var italicButton = UiUtil.MakeButton(vm.ToggleLinesItalicOrSelectedTextCommand, IconNames.Italic);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(italicButton, Se.Language.Main.ItalicHint);
            }
            buttonPanel.Children.Add(italicButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonColor)
        {
            var colorButton = UiUtil.MakeButton(vm.ShowColorPickerCommand, IconNames.Palette);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(colorButton, Se.Language.Main.ColorHint);
            }
            buttonPanel.Children.Add(colorButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonRemoveFormatting)
        {
            var removeFormattingButton = UiUtil.MakeButton(vm.RemoveFormattingAllCommand, IconNames.FormatClear);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(removeFormattingButton, Se.Language.Main.RemoveFormattingHint);
            }
            buttonPanel.Children.Add(removeFormattingButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonAiAssistant)
        {
            var aiAssistantButton = UiUtil.MakeButton(vm.ShowAiAssistantCommand, IconNames.Robot);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(aiAssistantButton, Se.Language.Tools.AiAssistant.Hint);
            }
            buttonPanel.Children.Add(aiAssistantButton);
        }

        textEditGrid.Add(buttonPanel, 1, 2);

        Grid.SetColumn(textEditGrid, 1);
        editGrid.Children.Add(textEditGrid);

        Grid.SetRow(editGrid, 1);
        mainGrid.Children.Add(editGrid);


        textEditGrid.ColumnDefinitions[1].Bind(ColumnDefinition.WidthProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
            Converter = booleanToGridLengthConverter
        });

        return mainGrid;
    }

    // Stable keys (DataGridColumn.Tag) used to snapshot/restore subtitle grid column
    // widths across restarts. Headers are localized, so they can't be used as keys (#11415).
    internal static class SubtitleGridColumnKeys
    {
        public const string Number = "Number";
        public const string Start = "Start";
        public const string End = "End";
        public const string Duration = "Duration";
        public const string Text = "Text";
        public const string OriginalText = "OriginalText";
        public const string Style = "Style";
        public const string Gap = "Gap";
        public const string Actor = "Actor";
        public const string Cps = "Cps";
        public const string Wpm = "Wpm";
        public const string PixelWidth = "PixelWidth";
        public const string Layer = "Layer";
        public const string ScrollbarGutter = "ScrollbarGutter";
    }

    // The stretchy text columns keep filling the window, so their width is never stored.
    private static bool IsStretchyColumn(string key)
        => key == SubtitleGridColumnKeys.Text || key == SubtitleGridColumnKeys.OriginalText;

    private static void RestoreSubtitleGridColumnWidths(DataGrid grid)
    {
        var saved = Se.Settings.General.SubtitleGridColumnWidths;
        if (saved == null || saved.Count == 0)
        {
            return;
        }

        foreach (var column in grid.Columns)
        {
            if (column.Tag is string key
                && !IsStretchyColumn(key)
                && saved.TryGetValue(key, out var width)
                && width > 0)
            {
                column.Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
            }
        }
    }

    // Snapshot the current (actual) width of each fixed column so it can be restored on
    // the next launch. Called on exit. Hidden columns report ActualWidth 0 and are skipped,
    // keeping their previously stored width.
    public static void SaveSubtitleGridColumnWidths(DataGrid? grid)
    {
        if (grid == null)
        {
            return;
        }

        var widths = Se.Settings.General.SubtitleGridColumnWidths ??= new();
        foreach (var column in grid.Columns)
        {
            if (column.Tag is string key
                && !IsStretchyColumn(key)
                && key != SubtitleGridColumnKeys.ScrollbarGutter
                && column.ActualWidth > 0)
            {
                widths[key] = column.ActualWidth;
            }
        }
    }

    private static Avalonia.Controls.Control MakeTextBox(MainViewModel vm)
    {
        vm.EditTextBox.ContentControl.RemoveControlFromParent();

        var textBox = MakeSubtitleTextBox();
        textBox[!TextBox.TextProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.Text))
        {
            Mode = BindingMode.TwoWay
        };
        textBox[AutomationProperties.NameProperty] = Se.Language.General.Text;

        textBox.TextChanged += vm.SubtitleTextChanged;
        textBox.GotFocus += (_, _) => vm.SubtitleTextBoxGotFocus();
        textBox.AddHandler(InputElement.PointerPressedEvent, (_, e) => vm.StoreTextEditorPointerArgs(e), RoutingStrategies.Tunnel);

        SetupMacContextMenuForTextBox(textBox, vm);
        MainHelpers.RightToLeftHelper.FollowContentDirection(textBox);

        vm.EditTextBox = new TextBoxWrapper(textBox);
        return textBox;
    }

    /// <summary>
    /// Makes the subtitle edit text box - a <see cref="SyntaxHighlightingTextBox"/> when
    /// "Color tags" is on, else a normal TextBox.
    /// </summary>
    private static TextBox MakeSubtitleTextBox()
    {
        var appearance = Se.Settings.Appearance;

        var textBox = appearance.SubtitleTextBoxColorTags
            ? new SyntaxHighlightingTextBox()
            : new TextBox();

        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.MinHeight = 92;
        textBox.Height = 92;
        textBox.FontSize = appearance.SubtitleTextBoxFontSize;
        textBox.FontWeight = appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal;
        textBox.IsUndoEnabled = false;
        textBox.ClearSelectionOnLostFocus = false;

        if (appearance.SubtitleTextBoxCenterText)
        {
            textBox.TextAlignment = TextAlignment.Center;
        }

        if (!string.IsNullOrEmpty(appearance.SubtitleTextBoxAndGridFontName))
        {
            textBox.FontFamily = new FontFamily(appearance.SubtitleTextBoxAndGridFontName);
        }

        return textBox;
    }

    private static Avalonia.Controls.Control MakeTextBoxOriginal(MainViewModel vm)
    {
        var textBox = MakeSubtitleTextBox();
        textBox[!TextBox.TextProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.OriginalText))
        {
            Mode = BindingMode.TwoWay
        };

        SetupMacContextMenuForTextBox(textBox, vm);
        MainHelpers.RightToLeftHelper.FollowContentDirection(textBox);

        vm.EditTextBoxOriginal = new TextBoxWrapper(textBox);
        return textBox;
    }

    /// <summary>
    /// On macOS, Ctrl+Click is the right-click / context menu gesture.
    /// Avalonia's TextBox may treat Ctrl+Click as a text-selection modifier, preventing the
    /// ContextFlyout from opening. We intercept in the tunnel phase (before the TextBox) to
    /// mark the event as handled, and then open the context menu on pointer release.
    /// </summary>
    private static void SetupMacContextMenuForTextBox(TextBox textBox, MainViewModel vm)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        // Tunnel phase fires before TextBox's built-in pointer handling.
        textBox.AddHandler(
            InputElement.PointerPressedEvent,
            (_, e) =>
            {
                var point = e.GetCurrentPoint(textBox);
                if (point.Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    // Block TextBox from treating this as a selection modifier.
                    e.Handled = true;
                }
            },
            RoutingStrategies.Tunnel);

        // Show the context menu on release.
        textBox.AddHandler(
            InputElement.PointerReleasedEvent,
            (_, e) =>
            {
                if (e.InitialPressMouseButton == MouseButton.Left &&
                    e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                    !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    vm.ControlMacPointerReleased(textBox, e);
                }
            },
            RoutingStrategies.Tunnel);
    }

}

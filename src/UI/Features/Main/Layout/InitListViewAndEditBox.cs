using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Projektanker.Icons.Avalonia;
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
            vm.SubtitleGrid.PointerPressed -= vm.SubtitleGrid_PointerPressed;
            vm.SubtitleGrid.PointerReleased -= vm.SubtitleGrid_PointerReleased;

            // Remove handlers added with AddHandler
            vm.SubtitleGrid.RemoveHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed);
            vm.SubtitleGrid.RemoveHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased);

            // Clear the grid to help with garbage collection
            vm.SubtitleGrid.ItemsSource = null;
            vm.SubtitleGrid.ContextFlyout = null;
        }

        // Unhook events from old text editors if they exist
        if (vm.EditTextBoxBindingCoordinator != null)
        {
            if (vm.EditTextBoxBindingCoordinator is TextEditorBindingCoordinator oldCoordinator)
            {
                oldCoordinator.DeInitialize();
                if (vm.EditTextBox?.ContentControl != null)
                {
                    UiUtil.RemoveControlFromParent(vm.EditTextBox.ContentControl);
                }
            }
            vm.EditTextBoxBindingCoordinator = null;
        }

        if (vm.EditTextBoxHelper is TextEditorBindingHelper helper)
        {
            helper.DeInitialize();
            vm.EditTextBoxHelper = null;
        }

        if (vm.EditTextBoxOriginalHelper is TextEditorBindingHelper helperOriginal)
        {
            helperOriginal.DeInitialize();
            vm.EditTextBoxOriginalHelper = null;
        }

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
            GridLinesVisibility = UiUtil.GetGridLinesVisibility(),
            VerticalGridLinesBrush = UiUtil.GetBorderBrush(),
            HorizontalGridLinesBrush = UiUtil.GetBorderBrush(),
            FontSize = Se.Settings.Appearance.SubtitleGridFontSize,
        };
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.SubtitleGrid.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = vm.SubtitleGrid
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.SubtitleGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.SubtitleGridOnDrop, RoutingStrategies.Bubble);

        vm.SubtitleGrid.DoubleTapped += vm.OnSubtitleGridDoubleTapped;
        vm.SubtitleGrid.Tapped += vm.OnSubtitleGridSingleTapped;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var doubleRoundedConverter = new DoubleToOneDecimalConverter();
        var cpsWmpConverter = new DoubleToOneDecimalHideMaxConverter();
        var notNullConverter = new NotNullConverter();
        var syntaxHighlightingConverter = new TextWithSubtitleSyntaxHighlightingConverter();
        vm.SubtitleDataGridSyntaxHighlighting = syntaxHighlightingConverter;
        var gapConverter = new DoubleToNoDecimalHideMaxConverter();
        var inverseBooleanConverter = new InverseBooleanConverter();
        var textOneLineShortConverter = new TextOneLineShortConverter();
        var booleanToGridLengthConverter = new BooleanToGridLengthConverter();
        var booleanAndConverter = BooleanAndConverter.Instance;

        // Set up data binding for row visibility based on IsHidden property
        vm.SubtitleGrid.LoadingRow += (sender, e) =>
        {
            e.Row.Bind(DataGridRow.IsVisibleProperty, new Binding(nameof(SubtitleLineViewModel.IsHidden))
            {
                Converter = inverseBooleanConverter
            });
        };

        vm.SubtitleGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.NumberSymbol,
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
                            [!Visual.IsVisibleProperty] = new Binding(nameof(SubtitleLineViewModel.Bookmark)) { Converter = notNullConverter },
                         },
                         UiUtil.MakeLabel().WithBindText(value, new Binding(nameof(SubtitleLineViewModel.Number)))
                    }
                })
        });

        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
            Width = new DataGridLength(120),
            MinWidth = 100,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        var hideColumn = new DataGridTextColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
            Width = new DataGridLength(120),
            MinWidth = 100,
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
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
                    TextWrapping = TextWrapping.NoWrap,
                    [!TextBlock.InlinesProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = syntaxHighlightingConverter, Mode = BindingMode.OneWay },
                };

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
                    TextWrapping = TextWrapping.NoWrap,
                    [!TextBlock.InlinesProperty] = new Binding(nameof(SubtitleLineViewModel.OriginalText)) { Converter = syntaxHighlightingConverter, Mode = BindingMode.OneWay },
                };

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

        var layerColumn = new DataGridTextColumn
        {
            Header = Se.Language.General.Layer,
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
        vm.SubtitleGrid.PointerPressed += vm.SubtitleGrid_PointerPressed;

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
        deleteMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        deleteMenuItem.Command = vm.DeleteSelectedLinesCommand;
        flyout.Items.Add(deleteMenuItem);

        var insertBeforeMenuItem = new MenuItem { Header = Se.Language.General.InsertBefore, DataContext = vm };
        insertBeforeMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        insertBeforeMenuItem.Command = vm.InsertLineBeforeCommand;
        flyout.Items.Add(insertBeforeMenuItem);

        var insertAfterMenuItem = new MenuItem { Header = Se.Language.General.InsertAfter, DataContext = vm };
        insertAfterMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        insertAfterMenuItem.Command = vm.InsertLineAfterCommand;
        flyout.Items.Add(insertAfterMenuItem);

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
                new MenuItem { Header = Se.Language.Main.PasteFromClipboardDotDotDot, Command = vm.ColumnPasteFromClipboardCommand},
                new MenuItem { Header = Se.Language.Main.TextUp, Command = vm.ColumnTextUpCommand },
                new MenuItem { Header = Se.Language.Main.TextDown, Command = vm.ColumnTextDownCommand },
            }
        };
        columnMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(columnMenuItem);

        var sep1 = new Separator { DataContext = vm };
        sep1.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(sep1);

        var splitMenuItem = new MenuItem { Header = Se.Language.General.SplitLine, DataContext = vm };
        splitMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
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
        mergeSelectedMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        mergeSelectedMenuItem.Command = vm.MergeSelectedLinesCommand;
        flyout.Items.Add(mergeSelectedMenuItem);
        vm.MenuItemMerge = mergeSelectedMenuItem;

        var mergeSelectedAsDialogMenuItem = new MenuItem { Header = Se.Language.General.MergeSelectedAsDialog, DataContext = vm };
        mergeSelectedAsDialogMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        mergeSelectedAsDialogMenuItem.Command = vm.MergeSelectedLinesDialogCommand;
        flyout.Items.Add(mergeSelectedAsDialogMenuItem);
        vm.MenuItemMergeAsDialog = mergeSelectedAsDialogMenuItem;

        var extendToLineBeforeMenuItem = new MenuItem { Header = Se.Language.General.ExtendBefore, DataContext = vm };
        extendToLineBeforeMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        extendToLineBeforeMenuItem.Command = vm.ExtendSelectedToPreviousCommand;
        flyout.Items.Add(extendToLineBeforeMenuItem);
        vm.MenuItemExtendToLineBefore = extendToLineBeforeMenuItem;

        var extendToLineAfterMenuItem = new MenuItem { Header = Se.Language.General.ExtendAfter, DataContext = vm };
        extendToLineAfterMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        extendToLineAfterMenuItem.Command = vm.ExtendSelectedToPreviousCommand;
        flyout.Items.Add(extendToLineAfterMenuItem);
        vm.MenuItemExtendToLineAfter = extendToLineAfterMenuItem;

        var sep2 = new Separator { DataContext = vm };
        sep2.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
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
        RemoveFormattingMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(RemoveFormattingMenuItem);


        var italicMenuItem = new MenuItem
        {
            Header = Se.Language.General.Italic,
            Command = vm.ToggleLinesItalicCommand,
            DataContext = vm,
        };
        italicMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(italicMenuItem);

        var boldMenuItem = new MenuItem
        {
            Header = Se.Language.General.Bold,
            Command = vm.ToggleLinesBoldCommand,
            DataContext = vm,
        };
        boldMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(boldMenuItem);

        var colorMenuItem = new MenuItem
        {
            Header = Se.Language.General.ColorDotDotDot,
            Command = vm.ShowColorPickerCommand,
            DataContext = vm,
        };
        colorMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(colorMenuItem);

        var fontNameMenuItem = new MenuItem
        {
            Header = Se.Language.General.FontNameDotDotDot,
            Command = vm.ShowFontNamePickerCommand,
            DataContext = vm,
        };
        fontNameMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(fontNameMenuItem);


        var alignmentMenuItem = new MenuItem
        {
            Header = Se.Language.General.AlignmentDotDotDot,
            Command = vm.ShowAlignmentPickerCommand,
            DataContext = vm,
        };
        alignmentMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(alignmentMenuItem);

        var bookmarkMenuItem = new MenuItem
        {
            Header = Se.Language.General.BookmarkDotDotDot,
            Command = vm.AddOrEditBookmarkCommand,
            DataContext = vm,
        };
        bookmarkMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(bookmarkMenuItem);

        var menuItemSelectedLines = new MenuItem
        {
            Header = Se.Language.General.SelectedLines,
            DataContext = vm,
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.Main.Menu.TextToSpeech,
                    Command = vm.SpeechToTextSelectedLinesPromptForLangaugeFirstTimeCommand,
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
            }
        };
        menuItemSelectedLines.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridFlyoutHeaderVisible)) { Converter = inverseBooleanConverter });
        flyout.Items.Add(menuItemSelectedLines);


        // Set the ContextFlyout property
        vm.SubtitleGrid.ContextFlyout = flyout;
        vm.SubtitleGrid.AddHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed, RoutingStrategies.Tunnel);
        vm.SubtitleGrid.AddHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased, RoutingStrategies.Tunnel);

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
            Spacing = 0,
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
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
        };
        var startTimeBindingName = nameof(vm.SelectedSubtitle) + "." + (Se.Settings.Appearance.ShowUpDownEndTime
            ? nameof(SubtitleLineViewModel.StartTimeOnly)
            : nameof(SubtitleLineViewModel.StartTime));
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
        var upDownLayer = new NumericUpDown
        {
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(SubtitleLineViewModel.Layer)}")
            {
                Mode = BindingMode.TwoWay,
            },
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
        };
        if (!vm.ShowUpDownLabels && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(upDownLayer, Se.Language.General.Layer);
        }
        panelLayer.Children.Add(upDownLayer);
        timeControlsPanel.Children.Add(panelLayer);

        Grid.SetColumn(timeControlsPanel, 0);
        editGrid.Children.Add(timeControlsPanel);

        // Right panel for text editing (show/duration is to the left)
        var textEditGrid = new Grid
        {
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

        // Create a Flyout for the TextEditor
        var flyoutTextBox = new MenuFlyout();
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
            Margin = new Thickness(3)
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

        // Set up coordinator to handle vm.PropertyChanged once for both text editors
        var textEditorHelper = vm.EditTextBoxHelper as TextEditorBindingHelper;
        var originalTextEditorHelper = vm.EditTextBoxOriginalHelper as TextEditorBindingHelper;
        var coordinator = new TextEditorBindingCoordinator(vm, textEditorHelper, originalTextEditorHelper);
        coordinator.Initialize();
        vm.EditTextBoxBindingCoordinator = coordinator;

        return mainGrid;
    }

    private static Control MakeTextBox(MainViewModel vm)
    {
        UiUtil.RemoveControlFromParent(vm.EditTextBox.ContentControl);

        if (Se.Settings.Appearance.SubtitleTextBoxColorTags)
        {
            return MakeTextEditor(vm);
        }
        else
        {
            var textBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 92,
                Height = 92,
                [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.Text))
                {
                    Mode = BindingMode.TwoWay
                },
                FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
                FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
                IsUndoEnabled = false,
                ClearSelectionOnLostFocus = false,
            };
            if (Se.Settings.Appearance.SubtitleTextBoxCenterText)
            {
                textBox.TextAlignment = TextAlignment.Center;
            }
            if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
            {
                textBox.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
            }

            textBox.TextChanged += vm.SubtitleTextChanged;
            textBox.GotFocus += (_, _) => vm.SubtitleTextBoxGotFocus();

            vm.EditTextBox = new TextBoxWrapper(textBox);
            return textBox;
        }
    }

    private static Border MakeTextEditor(MainViewModel vm)
    {
        var textEditor = MakeTextEditor();

        var defaultBorderBrush = UiUtil.GetBorderBrush();
        var focusedBorderBrush = new SolidColorBrush(Colors.DodgerBlue);

        var textEditorBorder = new Border
        {
            Child = textEditor,
            BorderThickness = new Thickness(1),
            BorderBrush = defaultBorderBrush,
            CornerRadius = new CornerRadius(3),
            ClipToBounds = true,
        };

        var wrapper = new TextEditorWrapper(textEditor, textEditorBorder);

        if (Se.Settings.Appearance.SubtitleTextBoxCenterText)
        {
            wrapper.SetAlignment(TextAlignment.Center);
        }

        vm.EditTextBox = wrapper;

        // Set up Mac context menu support
        textEditor.PointerReleased += vm.ControlMacPointerReleased;
        textEditor.PointerPressed += (sender, e) =>
        {
            if (e.GetCurrentPoint(textEditor).Properties.IsRightButtonPressed)
            {
                vm.StoreTextEditorPointerArgs(e);
            }
        };

        var helper = new TextEditorBindingHelper(vm, textEditor, wrapper, textEditorBorder, defaultBorderBrush, focusedBorderBrush, isOriginal: false);
        helper.Initialize();
        vm.EditTextBoxHelper = helper;

        return textEditorBorder;
    }

    private static TextEditor MakeTextEditor()
    {
        var textEditor = new TextEditor
        {
            MinHeight = 92,
            Height = 92,
            FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
            FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
            WordWrap = true,
            ShowLineNumbers = false,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Focusable = true,
            Padding = new Thickness(6, 4, 4, 4),
        };

        // Add syntax highlighting transformer
        textEditor.TextArea.TextView.LineTransformers.Add(new SubtitleSyntaxHighlighting());

        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textEditor.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        return textEditor;
    }

    private static Control MakeTextBoxOriginal(MainViewModel vm)
    {
        if (Se.Settings.Appearance.SubtitleTextBoxColorTags)
        {
            return MakeTextEditorOriginal(vm);
        }
        else
        {
            var textBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 92,
                Height = 92,
                [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedSubtitle) + "." + nameof(SubtitleLineViewModel.OriginalText))
                {
                    Mode = BindingMode.TwoWay
                },
                FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
                FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
                IsUndoEnabled = false,
                ClearSelectionOnLostFocus = false,
            };
            if (Se.Settings.Appearance.SubtitleTextBoxCenterText)
            {
                textBox.TextAlignment = TextAlignment.Center;
            }
            if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
            {
                textBox.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
            }

            vm.EditTextBoxOriginal = new TextBoxWrapper(textBox);
            return textBox;
        }
    }

    private static Border MakeTextEditorOriginal(MainViewModel vm)
    {
        var textEditor = MakeTextEditor();

        var defaultBorderBrush = UiUtil.GetBorderBrush();
        var focusedBorderBrush = new SolidColorBrush(Colors.DodgerBlue);

        var textEditorBorder = new Border
        {
            Child = textEditor,
            BorderThickness = new Thickness(1),
            BorderBrush = defaultBorderBrush,
            CornerRadius = new CornerRadius(3),
            ClipToBounds = true,
        };

        var wrapper = new TextEditorWrapper(textEditor, textEditorBorder);

        if (Se.Settings.Appearance.SubtitleTextBoxCenterText)
        {
            wrapper.SetAlignment(TextAlignment.Center);
        }

        vm.EditTextBoxOriginal = wrapper;

        // Set up Mac context menu support
        textEditor.PointerReleased += vm.ControlMacPointerReleased;
        textEditor.PointerPressed += (sender, e) =>
        {
            if (e.GetCurrentPoint(textEditor).Properties.IsRightButtonPressed)
            {
                vm.StoreTextEditorPointerArgs(e);
            }
        };

        var helper = new TextEditorBindingHelper(vm, textEditor, wrapper, textEditorBorder, defaultBorderBrush, focusedBorderBrush, isOriginal: true);
        helper.Initialize();
        vm.EditTextBoxOriginalHelper = helper;

        return textEditorBorder;
    }
}
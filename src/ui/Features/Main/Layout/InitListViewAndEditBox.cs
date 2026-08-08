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
    // The text box's own floor - unchanged, so the default layout looks exactly as before.
    private const double SubtitleTextBoxMinimumHeight = 92;
    // Starting floor for the edit section, replaced by a measured value on first layout (see
    // TrackEditSectionMinimumHeight). textEditGrid is "Auto,*,Auto": the "Text" header and the
    // "Line length / Total chars" panel sit above and below the box, and the floor has to cover
    // all three - sized to the box alone, the box (which cannot shrink past its own MinHeight)
    // overflows its row and draws over the labels underneath (#10271).
    private const double EditGridMinimumHeight = SubtitleTextBoxMinimumHeight;
    private const double EditGridMargin = 10;
    // The subtitle grid row is Star, so without a floor the splitter can drag it away to
    // nothing and there is no handle left to drag back (#10271).
    private const double SubtitleGridMinimumHeight = 45;

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
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star) { MinHeight = SubtitleGridMinimumHeight },
                // GridSplitter constrains the row definition, so include editGrid's outer
                // margin to preserve the text box's 92 px minimum at the drag limit.
                new RowDefinition(GridLength.Auto) { MinHeight = EditGridMinimumHeight + EditGridMargin * 2 },
            },
        };

        // TableView (Avalonia 12.1) pilot #3, after Show history (#12704) and the OCR grid
        // (#13001): the main subtitle grid. TableView rows are ListBoxItems, so keyboard
        // focus moves to the current row and UI Automation exposes it to screen readers -
        // the DataGrid kept focus on itself, which made the grid unusable with a screen
        // reader (issue #13015). Grid lines come from the TableView cell themes; sorting
        // was already disabled on the DataGrid and TableView has none.
        var subtitleGrid = TableViewExtras.MakeTableView();

        // TableView itself is not focusable by default, and with no rows there is no focusable
        // row container either - so an empty grid left the window without any focusable content.
        // Keyboard focus then either stayed on the window root (Avalonia's AccessKeyHandler
        // ignores all keys in that state, so Alt appeared dead and no access keys were
        // underlined) or, once it reached the menu bar, could never leave it again because the
        // menu deactivation had no focus target to restore to (#13111). The grid is both the
        // startup focus target and that deactivation fallback, so it must be able to hold focus
        // even with no rows - like an empty list view on Windows.
        subtitleGrid.Focusable = true;

        subtitleGrid.Height = double.NaN;
        subtitleGrid.Margin = new Thickness(Se.Settings.Appearance.GridCompactMode ? 0 : 2);
        subtitleGrid.ItemsSource = vm.Subtitles;
        subtitleGrid.DataContext = vm.Subtitles;
        subtitleGrid.FontSize = Se.Settings.Appearance.SubtitleGridFontSize;

        // Keep the vertical scrollbar at its full width instead of the thin
        // expand-on-hover overlay: it then reserves its own layout space, so it never
        // covers the outermost text column (the DataGrid needed an empty trailing
        // gutter column for this, issue #12351) and it is an easier drag target.
        ScrollViewer.SetAllowAutoHide(subtitleGrid, false);

        vm.SubtitleGrid = subtitleGrid;
        vm.SubtitleGridDragSelect = new TableViewDragSelect(subtitleGrid, vm.ApplyDragSelectRange);

        // hack to make drag and drop work on the grid - also on empty rows
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

        // Collapse hidden rows (style bindings evaluate against the row's item).
        TableViewExtras.BindRowProperty(vm.SubtitleGrid, Visual.IsVisibleProperty,
            new Binding(nameof(SubtitleLineViewModel.IsHidden)) { Converter = inverseBooleanConverter });

        // Expose "number: text, start - end, duration" as the row's accessible name so
        // screen readers announce the full row like SE4's list view did (issues #13015,
        // #12087). Text stays right after the number so browsing by content is fast; the
        // time codes follow for review. The error summary is appended because the grid's
        // cell tints are the only other signal for rule violations, and color never
        // reaches the accessibility tree.
        TableViewExtras.BindRowProperty(vm.SubtitleGrid, AutomationProperties.NameProperty,
            new MultiBinding
            {
                StringFormat = "{0}: {1}, {2} - {3}, {4}{5}",
                Bindings =
                {
                    new Binding(nameof(SubtitleLineViewModel.Number)),
                    new Binding(nameof(SubtitleLineViewModel.Text)),
                    new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                    new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                    new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                    new Binding(nameof(SubtitleLineViewModel.AccessibleErrorText)),
                },
            });

        // Tint every other row. Selection still wins because :selected has priority.
        if (alternatingRowBrush != null)
        {
            TableViewExtras.ApplyAlternatingRows(vm.SubtitleGrid, alternatingRowBrush);
        }

        var columnManager = new TableViewColumnManager(vm.SubtitleGrid);
        vm.SubtitleGridColumnManager = columnManager;

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Tag = SubtitleGridColumnKeys.Number,
            Width = new GridLength(50),
            MinWidth = 40,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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

        var startColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Tag = SubtitleGridColumnKeys.Start,
            Width = new GridLength(120),
            MinWidth = 100,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnManager.Add(startColumn);
        startColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnStartTime))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        var hideColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            Tag = SubtitleGridColumnKeys.End,
            Width = new GridLength(120),
            MinWidth = 100,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnManager.Add(hideColumn);
        hideColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnEndTime))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });

        // The DataGrid sized this column to content (Auto); TableView's layout treats
        // Auto as star, so use a fixed width that fits the "8:88,888" duration format.
        var columnDuration = new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            Tag = SubtitleGridColumnKeys.Duration,
            Width = new GridLength(90),
            MinWidth = 60,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnManager.Add(columnDuration);
        columnDuration.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnDuration))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Tag = SubtitleGridColumnKeys.Text,
            Width = new GridLength(1, GridUnitType.Star),
            MinWidth = 100,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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

        var originalColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.OriginalText,
            Tag = SubtitleGridColumnKeys.OriginalText,
            Width = new GridLength(1, GridUnitType.Star), // Stretch text column
            MinWidth = 100,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        originalColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText))
        {
            Mode = BindingMode.OneWay,
            Source = vm
        });
        columnManager.Add(originalColumn);

        var styleColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Style,
            Tag = SubtitleGridColumnKeys.Style,
            Binding = new Binding(nameof(SubtitleLineViewModel.Style)),
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        };

        var styleColumnMultiBinding = new MultiBinding
        {
            Converter = booleanAndConverter,
            Bindings =
            {
                new Binding(nameof(vm.IsFormatAssaOrSsa)) { Source = vm, Mode = BindingMode.OneWay },
                new Binding(nameof(vm.ShowColumnStyle)) { Source = vm, Mode = BindingMode.OneWay }
            }
        };
        styleColumn.Bind(SeTableViewColumn.IsVisibleProperty, styleColumnMultiBinding);
        columnManager.Add(styleColumn);

        // WebVTT has styles too, but as cue classes inside the cue text instead of a field on
        // the line - so it needs a column of its own, shown in place of the ASSA one.
        var webVttStyleColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Style,
            Tag = SubtitleGridColumnKeys.WebVttStyle,
            Binding = new Binding(nameof(SubtitleLineViewModel.WebVttStyle)) { Mode = BindingMode.OneWay },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        };
        webVttStyleColumn.Bind(SeTableViewColumn.IsVisibleProperty, new MultiBinding
        {
            Converter = booleanAndConverter,
            Bindings =
            {
                new Binding(nameof(vm.IsFormatWebVtt)) { Source = vm, Mode = BindingMode.OneWay },
                new Binding(nameof(vm.ShowColumnStyle)) { Source = vm, Mode = BindingMode.OneWay }
            }
        });
        columnManager.Add(webVttStyleColumn);

        var columnGap = new SeTableViewColumn
        {
            Header = Se.Language.General.Gap,
            Tag = SubtitleGridColumnKeys.Gap,
            Width = new GridLength(100),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnGap.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnGap))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });
        columnManager.Add(columnGap);

        var actorColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Actor,
            Tag = SubtitleGridColumnKeys.Actor,
            Binding = new Binding(nameof(SubtitleLineViewModel.Actor)) { Mode = BindingMode.OneWay },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        };
        columnManager.Add(actorColumn);
        actorColumn.Bind(SeTableViewColumn.IsVisibleProperty, new MultiBinding
        {
            Converter = booleanAndConverter,
            Bindings =
            {
                new Binding(nameof(vm.IsFormatWebVtt)) { Source = vm, Mode = BindingMode.OneWay, Converter = inverseBooleanConverter },
                new Binding(nameof(vm.ShowColumnActor)) { Source = vm, Mode = BindingMode.OneWay },
            }
        });

        // WebVTT's counterpart of the actor is the "<v Name>" voice inside the cue text; it
        // replaces the Actor column while a WebVTT file is open, sharing its show/hide toggle.
        var webVttVoiceColumn = new SeTableViewColumn
        {
            Header = Se.Language.File.WebVtt.Voice,
            Tag = SubtitleGridColumnKeys.WebVttVoice,
            Binding = new Binding(nameof(SubtitleLineViewModel.WebVttVoice)) { Mode = BindingMode.OneWay },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        };
        columnManager.Add(webVttVoiceColumn);
        webVttVoiceColumn.Bind(SeTableViewColumn.IsVisibleProperty, new MultiBinding
        {
            Converter = booleanAndConverter,
            Bindings =
            {
                new Binding(nameof(vm.IsFormatWebVtt)) { Source = vm, Mode = BindingMode.OneWay },
                new Binding(nameof(vm.ShowColumnActor)) { Source = vm, Mode = BindingMode.OneWay },
            }
        });

        var cpsColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Cps,
            Tag = SubtitleGridColumnKeys.Cps,
            Width = new GridLength(100),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnManager.Add(cpsColumn);
        cpsColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnCps))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });
        
        var wpmColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Wpm,
            Tag = SubtitleGridColumnKeys.Wpm,
            Width = new GridLength(100),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnManager.Add(wpmColumn);
        wpmColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnWpm))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });
        
        var pixelWidthColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.PixelWidth,
            Tag = SubtitleGridColumnKeys.PixelWidth,
            Width = new GridLength(100),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
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
        columnManager.Add(pixelWidthColumn);
        pixelWidthColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnPixelWidth))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        var layerColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Layer,
            Tag = SubtitleGridColumnKeys.Layer,
            Binding = new Binding(nameof(SubtitleLineViewModel.Layer)),
            Width = new GridLength(23),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        };
        columnManager.Add(layerColumn);
        layerColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowColumnLayer))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        RestoreSubtitleGridColumnWidths(columnManager);

        vm.SubtitleGrid.DataContext = vm.Subtitles;
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;


        // Set up two-way binding for SelectedItem
        vm.SubtitleGrid[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedSubtitle))
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
        };

        // Set up two-way binding for SelectedIndex
        vm.SubtitleGrid[!TableView.SelectedIndexProperty] = new Binding(nameof(vm.SelectedSubtitleIndex))
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

        // WebVTT counterpart of the ASSA styles/actors block: cue classes and <v> voices.
        var webVttStylesMenuItem = new MenuItem
        {
            Header = Se.Language.General.Styles,
            DataContext = vm,
            Command = vm.SetWebVttStylesForSelectedLinesCommand,
        };
        webVttStylesMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AreWebVttContentMenuItemsVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(webVttStylesMenuItem);

        var webVttVoicesMenuItem = new MenuItem
        {
            Header = Se.Language.File.WebVtt.Voices,
            DataContext = vm,
        };
        webVttVoicesMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AreWebVttContentMenuItemsVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(webVttVoicesMenuItem);
        vm.MenuItemWebVttVoices = webVttVoicesMenuItem;

        var webVttBrowserPreviewMenuItem = new MenuItem
        {
            Header = Se.Language.File.WebVtt.BrowserPreview,
            DataContext = vm,
            Command = vm.ShowWebVttBrowserPreviewCommand,
        };
        webVttBrowserPreviewMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsWebVttBrowserPreviewVisible)) { Mode = BindingMode.TwoWay });
        flyout.Items.Add(webVttBrowserPreviewMenuItem);

        var sepWebVtt = new Separator { DataContext = vm };
        sepWebVtt.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AreWebVttContentMenuItemsVisible)));
        flyout.Items.Add(sepWebVtt);

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
            // "Actor" for most formats, "Voice" for WebVTT - the two columns share this toggle.
            [!MenuItem.HeaderProperty] = new Binding(nameof(vm.ShowActorColumnMenuHeader)),
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

        // SE4 had "Copy as text to clipboard" right here - without it the copy commands are only
        // reachable via shortcuts, and the text-only ones have no default shortcut at all
        var copyToClipboardMenuItem = new MenuItem { Header = Se.Language.General.CopyToClipboard, DataContext = vm };
        copyToClipboardMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        copyToClipboardMenuItem.Command = vm.SubtitleGridCopyCommand;
        flyout.Items.Add(copyToClipboardMenuItem);

        var copyTextToClipboardMenuItem = new MenuItem { Header = Se.Language.General.CopyTextToClipboard, DataContext = vm };
        copyTextToClipboardMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitleGridDataMenuVisible)));
        copyTextToClipboardMenuItem.Command = vm.CopyTextToClipboardCommand;
        flyout.Items.Add(copyTextToClipboardMenuItem);

        var copyOriginalTextToClipboardMenuItem = new MenuItem { Header = Se.Language.Options.Shortcuts.CopyTextFromOriginalToClipboard, DataContext = vm };
        copyOriginalTextToClipboardMenuItem.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ShowColumnOriginalText)));
        copyOriginalTextToClipboardMenuItem.Command = vm.CopyTextFromOriginalToClipboardCommand;
        flyout.Items.Add(copyOriginalTextToClipboardMenuItem);

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
                    Header = Se.Language.Main.Menu.AiReview,
                    Command = vm.AiReviewSelectedLinesCommand,
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
                    Header = Se.Language.Main.Menu.ShowSelectedLinesEarlierLater,
                    Command = vm.ShowSyncAdjustAllTimesSelectedLinesCommand,
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
        // In undocked mode the tool windows are topmost while SE is active (#11971), which
        // covers this context menu and its cascaded submenus (#13325).
        WindowService.SuspendUndockedTopmostWhileOpen(flyout);
        dropHost.AddHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        dropHost.AddHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        dropHost.AddHandler(InputElement.PointerMovedEvent, vm.SubtitleGrid_PointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);

        // Edit area - restructured with time controls on left, multiline text on right
        var editGrid = new Grid
        {
            Margin = new Thickness(EditGridMargin),
            MinHeight = EditGridMinimumHeight,
            ColumnDefinitions = new ColumnDefinitions("Auto, *"), // Two columns: left for time controls, right for text
            // Star so the section grows when the user drags the splitter above it (#10271):
            // with Auto, the extra pixel height from the splitter only added dead space below
            // the fixed-height text box.
            RowDefinitions = new RowDefinitions("*")
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
        // Keep the undocked tool windows from covering the text box context menu (#13325).
        WindowService.SuspendUndockedTopmostWhileOpen(flyoutTextBox);

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

        // Keep the SE4 order here: "at cursor/video position" first, then "at cursor position".
        // Swapping them broke muscle memory for people coming from SE4 (see issue #12888).
        var menuItemTextBoxSplitAtCursorAndVideoPosition = new MenuItem { Header = Se.Language.General.SplitLineAtVideoAndTextBoxPosition };
        menuItemTextBoxSplitAtCursorAndVideoPosition.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsTextBoxSplitAtCursorAndVideoPositionVisible)));
        menuItemTextBoxSplitAtCursorAndVideoPosition.Command = vm.SplitAtVideoPositionAndTextBoxCursorPositionCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxSplitAtCursorAndVideoPosition);

        var menuItemTextBoxSplitAtCursor = new MenuItem { Header = Se.Language.General.SplitLineAtTextBoxCursorPosition };
        menuItemTextBoxSplitAtCursor.Command = vm.SplitAtTextBoxCursorPositionCommand;
        flyoutTextBox.Items.Add(menuItemTextBoxSplitAtCursor);

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

        // Casing was shortcut-only, which made people think it had been removed (#13093).
        var menuItemTextBoxCasing = new MenuItem { Header = Se.Language.General.Casing };
        menuItemTextBoxCasing.Items.Add(new MenuItem
        {
            Header = Se.Language.General.ToggleCasing,
            Command = vm.ToggleCasingCommand,
        });
        menuItemTextBoxCasing.Items.Add(new MenuItem
        {
            Header = Se.Language.General.SelectionToUppercase,
            Command = vm.SelectionToUpperCommand,
        });
        menuItemTextBoxCasing.Items.Add(new MenuItem
        {
            Header = Se.Language.General.SelectionToLowercase,
            Command = vm.SelectionToLowerCommand,
        });
        menuItemTextBoxCasing.Items.Add(new MenuItem
        {
            Header = Se.Language.General.SelectionToSentenceCase,
            Command = vm.SelectionToSentenceCaseCommand,
        });
        menuItemTextBoxCasing.Items.Add(new Separator());
        menuItemTextBoxCasing.Items.Add(new MenuItem
        {
            Header = Se.Language.Main.Menu.ChangeCasing,
            Command = vm.ChangeCasingSelectedLinesCommand,
        });
        flyoutTextBox.Items.Add(menuItemTextBoxCasing);

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
        // no seed label here - SubtitleTextInfoHelper.FillLineLengthPanel writes the
        // "Single line length" label into index 0 itself (and reuses the text blocks)

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 3,
            Margin = new Thickness(6,3,3,3)
        };

        if (Se.Settings.Appearance.TextBoxShowButtonAutoBreak)
        {
            var autoBreakButton = UiUtil.MakeButton(vm.AutoBreakCommand, IconNames.ScaleBalance, Se.Language.Main.AutoBreakHint);
            buttonPanel.Children.Add(autoBreakButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonUnbreak)
        {
            var unbreakButton = UiUtil.MakeButton(vm.UnbreakCommand, IconNames.SetMerge, Se.Language.Main.UnbreakHint);
            buttonPanel.Children.Add(unbreakButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonItalic)
        {
            var italicButton = UiUtil.MakeButton(vm.ToggleLinesItalicOrSelectedTextCommand, IconNames.Italic, Se.Language.Main.ItalicHint);
            buttonPanel.Children.Add(italicButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonColor)
        {
            var colorButton = UiUtil.MakeButton(vm.ShowColorPickerCommand, IconNames.Palette, Se.Language.Main.ColorHint);
            buttonPanel.Children.Add(colorButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonRemoveFormatting)
        {
            var removeFormattingButton = UiUtil.MakeButton(vm.RemoveFormattingAllCommand, IconNames.FormatClear, Se.Language.Main.RemoveFormattingHint);
            buttonPanel.Children.Add(removeFormattingButton);
        }

        if (Se.Settings.Appearance.TextBoxShowButtonAiAssistant)
        {
            var aiAssistantButton = UiUtil.MakeButton(vm.ShowAiAssistantCommand, IconNames.Robot, Se.Language.Tools.AiAssistant.Hint);
            buttonPanel.Children.Add(aiAssistantButton);
        }

        textEditGrid.Add(buttonPanel, 1, 2);

        Grid.SetColumn(textEditGrid, 1);
        editGrid.Children.Add(textEditGrid);

        Grid.SetRow(editGrid, 1);
        mainGrid.Children.Add(editGrid);

        // GridSplitter overlaying the boundary between the subtitle grid (row 0) and the
        // edit box (row 1) so the text box section can be resized vertically, like SE4
        // (#10271). The splitter lives in the edit box's own row (no extra row - an extra
        // row would shrink the grid viewport and break the grid scroll perf tests); with
        // VerticalAlignment.Top it resizes the row above (grid, Star) and its own row
        // (edit box, Auto -> becomes Pixel once the user drags). The negative top margin
        // centers the 4 px strip on the boundary.
        var editBoxSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, -UiUtil.SplitterWidthOrHeight / 2.0, 0, 0)
        };
        Grid.SetRow(editBoxSplitter, 1);
        mainGrid.Children.Add(editBoxSplitter);

        TrackEditSectionMinimumHeight(mainGrid, textEditGrid);


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
        public const string WebVttStyle = "WebVttStyle";
        public const string Gap = "Gap";
        public const string Actor = "Actor";
        public const string WebVttVoice = "WebVttVoice";
        public const string Cps = "Cps";
        public const string Wpm = "Wpm";
        public const string PixelWidth = "PixelWidth";
        public const string Layer = "Layer";
    }

    // The stretchy text columns keep filling the window, so their width is never stored.
    private static bool IsStretchyColumn(string key)
        => key == SubtitleGridColumnKeys.Text || key == SubtitleGridColumnKeys.OriginalText;

    private static void RestoreSubtitleGridColumnWidths(TableViewColumnManager columnManager)
    {
        var saved = Se.Settings.General.SubtitleGridColumnWidths;
        if (saved == null || saved.Count == 0)
        {
            return;
        }

        foreach (var column in System.Linq.Enumerable.OfType<SeTableViewColumn>(columnManager.Columns))
        {
            if (column.Tag is string key
                && !IsStretchyColumn(key)
                && saved.TryGetValue(key, out var width)
                && width > 0)
            {
                column.Width = new GridLength(Math.Max(width, column.MinWidth));
            }
        }
    }

    // Snapshot the current (actual) width of each fixed column so it can be restored on
    // the next launch. Called on exit. Hidden columns report ActualWidth 0 and are skipped,
    // keeping their previously stored width.
    public static void SaveSubtitleGridColumnWidths(TableViewColumnManager? columnManager)
    {
        if (columnManager == null)
        {
            return;
        }

        var widths = Se.Settings.General.SubtitleGridColumnWidths ??= new();
        foreach (var column in System.Linq.Enumerable.OfType<SeTableViewColumn>(columnManager.Columns))
        {
            if (column.Tag is string key
                && !IsStretchyColumn(key)
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

    /// <summary>
    /// Keeps the edit section's drag floor equal to what the section actually needs: the text
    /// box's own minimum plus the "Text" header and the "Line length / Total chars" panel that
    /// sit above and below it. Those two rows are Auto, so their height follows the UI font -
    /// a hard-coded allowance goes stale as soon as the font size changes, and if it is too
    /// small the text box (which cannot shrink past its MinHeight) overflows its row and draws
    /// over the labels (#10271). Measured rather than stored: this is a derived layout fact,
    /// not something a user should configure.
    /// </summary>
    private static void TrackEditSectionMinimumHeight(Grid mainGrid, Grid textEditGrid)
    {
        textEditGrid.LayoutUpdated += (_, _) =>
        {
            if (textEditGrid.RowDefinitions.Count < 3)
            {
                return;
            }

            var labelRows = textEditGrid.RowDefinitions[0].ActualHeight +
                            textEditGrid.RowDefinitions[2].ActualHeight;
            if (labelRows <= 0)
            {
                return;
            }

            var needed = SubtitleTextBoxMinimumHeight + labelRows + EditGridMargin * 2;
            var row = mainGrid.RowDefinitions[1];

            // Only react to a real change - assigning MinHeight re-triggers layout, so an
            // unconditional write here would spin.
            if (Math.Abs(row.MinHeight - needed) > 0.5)
            {
                row.MinHeight = needed;
            }
        };
    }

    private static TextBox MakeSubtitleTextBox()
    {
        var appearance = Se.Settings.Appearance;

        var textBox = appearance.SubtitleTextBoxColorTags
            ? new SyntaxHighlightingTextBox()
            : new TextBox();

        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        // MinHeight keeps the default layout; no fixed Height so the text box grows with the
        // resizable edit section (#10271).
        textBox.MinHeight = SubtitleTextBoxMinimumHeight;
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

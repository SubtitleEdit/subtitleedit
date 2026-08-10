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
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public class FixNetflixErrorsWindow : Window
{
    private readonly FixNetflixErrorsViewModel _vm;
    private ComboBox _comboBoxLanguage = null!;

    public FixNetflixErrorsWindow(FixNetflixErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.NetflixCheckAndFix.Title;
        Width = 1100;
        Height = 680;
        MinWidth = 900;
        MinHeight = 640;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonGenerateReport = UiUtil.MakeButton(Se.Language.Tools.NetflixCheckAndFix.GenerateReport, vm.GenerateReportCommand)
            .WithIconLeft(IconNames.Netflix);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonGenerateReport,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            Margin = new Thickness(4, 0, 10, 0),
        };
        summaryText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FixesSummaryText)));

        var buttonSelectPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.ChecksSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.ChecksInverseSelectionCommand),
            summaryText
        ).WithAlignmentLeft().WithAlignmentTop();


        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(310, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(settingsView, 0, 0);
        grid.Add(fixesView, 0, 1);
        grid.Add(buttonSelectPanel, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { _comboBoxLanguage.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeSettingsView(FixNetflixErrorsViewModel vm)
    {
        _comboBoxLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                UiUtil.MakeTextBlock(Se.Language.General.Language).WithMarginRight(5),
                _comboBoxLanguage
            }
        };

        // Netflix allows higher reading speeds for SDH and requires lower ones for
        // children's programs, so both change which limits the checks run against.
        var panelTop = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8),
            Spacing = 4,
            Children =
            {
                panelLanguage,
                UiUtil.MakeCheckBox(Se.Language.Tools.NetflixCheckAndFix.ChildrensProgram, vm, nameof(vm.IsChildrenProgram)),
                UiUtil.MakeCheckBox(Se.Language.Tools.NetflixCheckAndFix.Sdh, vm, nameof(vm.IsSdh)),
            }
        };

        // Grid with list of checks. Sorting dropped in the DataGrid -> TableView
        // conversion: the checks run in list order (RunChecks gets them in collection
        // order), so the list must not be reordered.
        // Stretch (the MakeTableView default) instead of the DataGrid's Left/Top: the
        // star-sized Name column needs the control to fill its fixed-width grid cell.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Checks));

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<NetflixCheckDisplayItem>((item, _) =>
            {
                var cb = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(NetflixCheckDisplayItem.IsSelected)) { Mode = BindingMode.TwoWay },
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                cb.IsCheckedChanged += (_, __) => vm.SetDirty();
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(1),
                    Child = cb
                };
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(70)
        });

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(NetflixCheckDisplayItem.Name)),
            Width = new GridLength(1, GridUnitType.Star)
        });

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        TableViewExtras.AddSpaceToggle<NetflixCheckDisplayItem>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(306, GridUnitType.Pixel) },
            },
        };

        grid.Add(panelTop, 0, 0);
        grid.Add(dataGrid, 1, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }

    private Border MakeFixesView(FixNetflixErrorsViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // fixes in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = _vm;
        dataGrid.ItemsSource = _vm.Fixes;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        var cb = new CheckBox
                        {
                            Focusable = false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNetflixErrorsItem.Apply)) { Mode = BindingMode.TwoWay },
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                        cb.IsEnabled = item.CanBeFixed;

                        return new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = cb,
                        };
                    }),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(70)
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.IndexDisplay)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = beforeBlock,
                        };
                    }),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = afterBlock,
                        };
                    }),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Reason,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.Reason)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        // Non-fixable rows count as "checked" so they never block the all-checked
        // toggle, and the setter leaves them alone (the old helper's canToggle).
        TableViewExtras.AddSpaceToggle<FixNetflixErrorsItem>(dataGrid,
            item => !item.CanBeFixed || item.Apply,
            (item, v) =>
            {
                if (item.CanBeFixed)
                {
                    item.Apply = v;
                }
            });

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
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

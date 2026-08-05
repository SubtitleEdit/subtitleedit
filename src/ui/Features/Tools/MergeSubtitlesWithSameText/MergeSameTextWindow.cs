using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;

public class MergeSameTextWindow : Window
{
    public MergeSameTextWindow(MergeSameTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.MergeLinesWithSameText;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsOkEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeControlsView(vm), 0);
        grid.Add(MakeMergesView(vm), 1);
        grid.Add(MakeSubtitlesView(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static StackPanel MakeControlsView(MergeSameTextViewModel vm)
    {
        var labelGap = UiUtil.MakeLabel(Se.Language.Tools.MergeLinesWithSameText.MaxMsBetweenLines);
        var numericUpDownGap = UiUtil.MakeNumericUpDownInt(0, 10000, Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines, 130, vm, nameof(vm.MaxMillisecondsBetweenLines));
        numericUpDownGap.ValueChanged += (s, e) => { vm.SetDirty(); };
        var checkBoxIncludeIncrementText = UiUtil.MakeCheckBox(Se.Language.Tools.MergeLinesWithSameText.IncludeIncrementingLines, vm, nameof(vm.IncludeIncrementingLines));
        checkBoxIncludeIncrementText.IsCheckedChanged += (s, e) => { vm.SetDirty(); };
        var panelGap = UiUtil.MakeHorizontalPanel(labelGap, numericUpDownGap, checkBoxIncludeIncrementText);

        return panelGap;
    }

    private static Border MakeMergesView(MergeSameTextViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid shows
        // merge candidates in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.MergeItems;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<MergeDisplayItem>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(4),
                        Child = new CheckBox
                        {
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(MergeDisplayItem.Apply)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(70)
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Lines,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(MergeDisplayItem.Lines)),
                    Width = new GridLength(110),
                },
                new SeTableViewColumn
                {
                    // The merged text is the wide content, so it takes the star width
                    // (the DataGrid content-sized it and gave Group the star).
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(MergeDisplayItem.MergedText)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Group,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(MergeDisplayItem.MergedGroup)),
                    Width = new GridLength(90),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedMergeItem)) { Source = vm });
        dataGrid.SelectionChanged += vm.MergeItemChanged;
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var index = e.Key == Key.Home ? 0 : items.Count - 1;
                dataGrid.SelectedIndex = index;
                dataGrid.ScrollIntoView(index);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    private static Border MakeSubtitlesView(MergeSameTextViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();

        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // subtitle lines in timeline order.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.MergeSubtitles;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Hide,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Group,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Extra)),
                    Width = new GridLength(90),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedMergeSubtitle)) { Source = vm });
        vm.SubtitleGrid = dataGrid;
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var index = e.Key == Key.Home ? 0 : items.Count - 1;
                dataGrid.SelectedIndex = index;
                dataGrid.ScrollIntoView(index);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}

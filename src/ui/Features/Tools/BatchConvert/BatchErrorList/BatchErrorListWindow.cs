using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>
/// Batch convert's "List errors": same layout as the main window's error list
/// (verdict header, summary cards that filter, one row per error) plus a file
/// name column and CSV export.
/// </summary>
public class BatchErrorListWindow : Window
{
    public BatchErrorListWindow(BatchErrorListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.ErrorList.Title;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 760;
        MinHeight = 460;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.ErrorList;

        var labelTitle = new TextBlock
        {
            Text = l.Title,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        var labelSummary = new TextBlock
        {
            FontSize = 14,
            Opacity = 0.75,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Summary)),
        };
        var header = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 12),
            Children = { labelTitle, labelSummary },
        };

        var cards = SummaryCard.MakeCardsPanel(vm.Cards, vm.SetFilterCommand);

        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand).WithBindIsEnabled(nameof(vm.HasErrors));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };

        grid.Add(header, 0, 0);
        grid.Add(cards, 1, 0);
        grid.Add(MakeErrorsGridView(vm), 2, 0);
        grid.Add(panelButtons, 3, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work

        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeErrorsGridView(BatchErrorListViewModel vm)
    {
        var l = Se.Language.ErrorList;
        var dataGrid = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        dataGrid.Height = double.NaN; // auto size inside scroll viewer
        dataGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Subtitles));

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FileName,
            Binding = new Binding(nameof(BatchErrorListItem.FileName)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(BatchErrorListItem.Number)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(60),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Error,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<BatchErrorListItem>((_, _) =>
            {
                var dot = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Avalonia.Controls.Shapes.Shape.FillProperty] = new Binding(nameof(BatchErrorListItem.Brush)),
                };
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(BatchErrorListItem.Category)),
                };
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Margin = new Thickness(6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { dot, text },
                };
            }),
            Width = new GridLength(160),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(BatchErrorListItem.Show)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(105),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(BatchErrorListItem.Hide)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(105),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = l.Detail,
            Binding = new Binding(nameof(BatchErrorListItem.Detail)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(200),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(BatchErrorListItem.Text)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });
        AutomationProperties.SetName(dataGrid, l.Title);

        TableViewExtras.BindSelectedItem(dataGrid, vm, nameof(vm.SelectedSubtitle));
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0 &&
                (e.Key == Key.Home ? items[0] : items[^1]) is { } target)
            {
                dataGrid.SelectedItem = target;
                dataGrid.ScrollIntoView(target);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}

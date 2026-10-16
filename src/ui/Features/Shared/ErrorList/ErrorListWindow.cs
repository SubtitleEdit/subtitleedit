using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>
/// "List errors": a verdict header, one summary card per error class (click to
/// filter), and the affected lines in a table. Double-click / Enter / "Go to"
/// jumps to the line in the main grid. In the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class ErrorListWindow : Window
{
    public ErrorListWindow(ErrorListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.ErrorList.Title;
        CanResize = true;
        Width = 960;
        Height = 680;
        MinWidth = 720;
        MinHeight = 460;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.ErrorList;

        var labelTitle = new TextBlock
        {
            Text = l.Title,
            FontSize = UiUtil.ScaledFontSize(18),
            FontWeight = FontWeight.SemiBold,
        };
        var labelSummary = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(13),
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Summary)),
        };
        var header = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { labelTitle, labelSummary },
        };

        var cards = SummaryCard.MakeCardsPanel(vm.Cards, vm.SetFilterCommand);
        cards.Margin = new Thickness(0);

        var top = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children = { header, cards },
        };

        var labelTip = new TextBlock
        {
            Text = l.Tip,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
        };

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasErrors));
        var buttonExport = MakeExportButton(vm);
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        buttonCancel.Classes.Add("primary");

        var left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonExport, MakeExportStatusLabel(vm) },
        };

        var listArea = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 12),
        };
        listArea.Add(MakeErrorsGridView(vm), 0, 0);
        listArea.Add(labelTip, 1, 0);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        grid.Add(new Border
        {
            Child = top,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(16, 14, 16, 14),
        }, 0, 0);
        grid.Add(listArea, 1, 0);
        grid.Add(EditorChrome.MakeDockedButtonBar(left, buttonGoTo, buttonCancel), 2, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work

        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// "Export..." with the four targets from #14379 - the list used to be shareable only as a
    /// screenshot. Every target exports the rows as shown, so an active card filter applies.
    /// </summary>
    private static Button MakeExportButton(ErrorListViewModel vm)
    {
        var l = Se.Language.ErrorList;
        var button = EditorChrome.MakeTextToolButton("fa-solid fa-file-export", Se.Language.General.ExportDotDotDot, null);
        button.Flyout = new MenuFlyout
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.CopyToClipboard,
                    Command = vm.CopyToClipboardCommand,
                },
                new MenuItem
                {
                    Header = l.ExportAsText,
                    Command = vm.ExportTextCommand,
                },
                new MenuItem
                {
                    Header = l.ExportAsExcel,
                    Command = vm.ExportExcelCommand,
                },
                new MenuItem
                {
                    Header = l.ExportAsHtml,
                    Command = vm.ExportHtmlCommand,
                },
            },
        };

        return button.WithBindIsEnabled(nameof(vm.CanExport));
    }

    /// <summary>Says "Copied to clipboard" next to the buttons - a clipboard copy has nothing else to show.</summary>
    private static TextBlock MakeExportStatusLabel(ErrorListViewModel vm)
    {
        return new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ExportStatus)),
        };
    }

    private static Border MakeErrorsGridView(ErrorListViewModel vm)
    {
        var l = Se.Language.ErrorList;
        var dataGrid = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        dataGrid.Height = double.NaN; // auto size inside scroll viewer
        dataGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Subtitles));

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ErrorListItem.Number)),
            Width = new GridLength(60),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Error,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ErrorListItem>((_, _) =>
            {
                var dot = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Avalonia.Controls.Shapes.Shape.FillProperty] = new Binding(nameof(ErrorListItem.Brush)),
                };
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(ErrorListItem.Category)),
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
            Binding = new Binding(nameof(ErrorListItem.Show)),
            Width = new GridLength(105),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(ErrorListItem.Hide)),
            Width = new GridLength(105),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Detail,
            Binding = new Binding(nameof(ErrorListItem.Detail)),
            Width = new GridLength(200),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ErrorListItem.Text)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });
        AutomationProperties.SetName(dataGrid, l.Title);

        TableViewExtras.BindSelectedItem(dataGrid, vm, nameof(vm.SelectedSubtitle));
        dataGrid.DoubleTapped += vm.OnGridDoubleTapped;
        dataGrid.KeyDown += (s, e) => vm.GridKeyDown(e);
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

        return EditorChrome.MakeListFrame(dataGrid, clearListBackground: true);
    }
}

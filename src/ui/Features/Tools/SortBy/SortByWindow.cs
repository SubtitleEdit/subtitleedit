using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.SortBy;

/// <summary>
/// Sort subtitles, in the editor look (see <see cref="EditorChrome"/>): the sort order (criteria
/// list with icon actions) on top, the sorted preview below, Cancel and OK in the button bar.
/// </summary>
public class SortByWindow : Window
{
    private ComboBox? _comboBoxProperty;

    public SortByWindow(SortByViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.SortBy.Title);
        CanResize = true;
        Width = 1200;
        Height = 800;
        MinWidth = 1000;
        MinHeight = 500;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeCancelOkRoot(EditorChrome.MakeOptionsAndLists(MakeSortControls(vm), MakeSubtitleView(vm)), null, buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, () => _comboBoxProperty?.Focus());
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeSortControls(SortByViewModel vm)
    {
        var comboBoxProperty = new ComboBox
        {
            Width = 200,
            DataContext = vm,
            ItemsSource = vm.AvailableProperties,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithAccessibleName(Se.Language.General.Property);
        _comboBoxProperty = comboBoxProperty;
        comboBoxProperty.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(SortByViewModel.SelectedAvailableProperty)) { Mode = BindingMode.TwoWay });

        var rowAscending = EditorChrome.MakeToggleRow(nameof(SortByViewModel.NewCriterionAscending), Se.Language.General.Ascending, out _);

        var buttonAdd = EditorChrome.MakeTextToolButton("fa-solid fa-plus", Se.Language.General.Add, vm.AddSortCriterionCommand);

        var listBoxSortCriteria = new ListBox
        {
            Width = 300,
            Height = 170,
            DataContext = vm,
        }.WithAccessibleName(Se.Language.Tools.SortBy.SortOrder);
        listBoxSortCriteria.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(SortByViewModel.SortCriteria)));
        listBoxSortCriteria.Bind(ListBox.SelectedItemProperty, new Binding(nameof(SortByViewModel.SelectedSortCriterion)) { Mode = BindingMode.TwoWay });
        listBoxSortCriteria.ItemTemplate = new FuncDataTemplate<SortCriterion>((criterion, _) =>
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(5),
                FontSize = EditorChrome.FieldFontSize,
            };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(SortCriterion.DisplayText)));
            return textBlock;
        }, true);

        var stackPanelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Margin = new Thickness(6, 0, 0, 0),
            Children =
            {
                EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, vm.MoveSortCriterionUpCommand, 28),
                EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, vm.MoveSortCriterionDownCommand, 28),
                EditorChrome.CreateToolButton("fa-solid fa-arrow-down-up-across-line", Se.Language.General.ToggleDirection, vm.ToggleSortDirectionCommand, 28),
                EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Remove, vm.RemoveSortCriterionCommand, 28),
                EditorChrome.CreateToolButton("fa-solid fa-eraser", Se.Language.General.Clear, vm.ClearSortCriteriaCommand, 28),
            }
        };

        var stackPanelAdd = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.Property, new Thickness(0)),
                comboBoxProperty,
                rowAscending,
                buttonAdd
            }
        };

        var stackPanelList = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                EditorChrome.MakeListFrame(listBoxSortCriteria, clearListBackground: true),
                stackPanelButtons
            }
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                stackPanelAdd,
                stackPanelList
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.Tools.SortBy.SortOrder, mainPanel, first: true);
    }

    private Border MakeSubtitleView(SortByViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var twoDecimalConverter = new DoubleToTwoDecimalConverter();
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Subtitles;
        dataGridSubtitle.WithAccessibleName(Se.Language.General.Preview); // the sorted result; no heading (#12087)

        // The number, show, duration and CPS columns were content-sized (Auto) on the
        // DataGrid; TableView treats Auto as star, so they get fixed widths instead.
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new GridLength(60),
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120),
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(90),
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
            NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Cps,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.CharactersPerSecond)) { Converter = twoDecimalConverter },
            Width = new GridLength(80),
        });

        dataGridSubtitle.Bind(TableView.ItemsSourceProperty, new Binding(nameof(SortByViewModel.Subtitles)) { Mode = BindingMode.TwoWay });
        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(SortByViewModel.SelectedSubtitle)) { Mode = BindingMode.TwoWay });
        dataGridSubtitle.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGridSubtitle.ItemsSource is IList items && items.Count > 0 &&
                (e.Key == Key.Home ? items[0] : items[^1]) is { } target)
            {
                dataGridSubtitle.SelectedItem = target;
                dataGridSubtitle.ScrollIntoView(target);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return EditorChrome.MakeListSection(Se.Language.General.Preview, dataGridSubtitle);
    }
}

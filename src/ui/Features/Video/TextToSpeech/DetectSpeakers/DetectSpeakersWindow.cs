using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public class DetectSpeakersWindow : Window
{
    public DetectSpeakersWindow(DetectSpeakersViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Video.TextToSpeech.DetectSpeakersTitle);
        CanResize = true;
        Width = 900;
        Height = 600;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        var labelInfo = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.RowsInfo));

        var panelOptions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
        };
        panelOptions.Children.Add(UiUtil.MakeCheckBox(
            Se.Language.Video.TextToSpeech.DetectSpeakersSticky, vm, nameof(vm.StickySpeakers)));
        panelOptions.Children.Add(UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand));
        panelOptions.Children.Add(UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InverseSelectionCommand));

        grid.Add(labelInfo, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(MakeRowsView(vm)), 1);
        grid.Add(panelOptions, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Control MakeRowsView(DetectSpeakersViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Rows;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.TextToSpeech.DetectSpeakersColumnUse,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<DetectSpeakersRow>((_, _) => new Border
            {
                Background = Brushes.Transparent,
                Padding = new Avalonia.Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(DetectSpeakersRow.IsSelected))
                    {
                        Mode = BindingMode.TwoWay,
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            }),
            Width = new GridLength(80),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(DetectSpeakersRow.Number)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(60),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(DetectSpeakersRow.Show)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(120),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Actor,
            Binding = new Binding(nameof(DetectSpeakersRow.Speaker)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(160),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(DetectSpeakersRow.Text)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });

        TableViewExtras.AddSpaceToggle<DetectSpeakersRow>(dataGrid,
            item => item.IsSelected,
            (item, value) => item.IsSelected = value);

        return dataGrid;
    }
}

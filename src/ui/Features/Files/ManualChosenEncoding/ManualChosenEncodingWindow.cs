using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;

public class ManualChosenEncodingWindow : Window
{
    public ManualChosenEncodingWindow(ManualChosenEncodingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.ManualChosenEncoding.Title;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var searchBox = new TextBox
        {
            PlaceholderText = Se.Language.File.ManualChosenEncoding.SearchEncodings,
            Margin = new Thickness(10),
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        searchBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        searchBox.TextChanged += (s, e) => vm.SearchTextChanged();
        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                labelSearch,
                searchBox,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
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

        grid.Add(panelSearch, 0);
        grid.Add(MakeEncodingsView(vm), 1);
        grid.Add(MakePreviewBox(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { searchBox.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeEncodingsView(ManualChosenEncodingViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Encodings;

        var columnCodePage = new SeTableViewColumn
        {
            Header = Se.Language.File.ManualChosenEncoding.CodePage,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding("Encoding.CodePage"),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(100),
        };
        var columnName = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TextEncoding.DisplayName)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var columnGroup = new SeTableViewColumn
        {
            Header = Se.Language.General.Group,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding("Encoding.BodyName"),
            Width = new GridLength(180),
        };
        dataGrid.Columns.AddRange(new TableViewColumn[] { columnCodePage, columnName, columnGroup });

        // An encoding pick list whose order is presentation-only (the caller consumes
        // just SelectedEncoding), so header sorting is safe to wire.
        static string GetBodyName(TextEncoding encoding)
        {
            try
            {
                return encoding.Encoding.BodyName;
            }
            catch
            {
                return string.Empty; // some code pages have no body name
            }
        }

        var sorter = new TableViewHeaderSorter(dataGrid);
        sorter.AddSortable<TextEncoding, int>(columnCodePage, x => x.Encoding.CodePage)
            .AddSortable<TextEncoding, string>(columnName, x => x.DisplayName)
            .AddSortable<TextEncoding, string>(columnGroup, GetBodyName);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedEncoding)) { Source = vm });
        dataGrid.SelectionChanged += vm.EncodingChanged;

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    private static Control MakePreviewBox(ManualChosenEncodingViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.CurrentEncodingText));
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            IsReadOnly = true,
            Margin = new Thickness(0, 0, 0, 0),
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)) { Source = vm, Mode = BindingMode.OneWay });

        grid.Add(label, 0);
        grid.Add(textBox, 1);

        return UiUtil.MakeBorderForControl(grid);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public class PickMp4TrackWindow : Window
{
    private readonly PickMp4TrackViewModel _vm;

    public PickMp4TrackWindow(PickMp4TrackViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.WindowTitle;
        Width = 1024;
        Height = 600;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var tracksView = MakeTracksView(vm);
        var subtitleView = MakeSubtitleView(vm);

        var buttonExport = UiUtil.MakeButton("Export...", vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(tracksView, 0, 0);
        grid.Add(subtitleView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);


        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.SelectAndScrollToRow(0);
    }

    private Border MakeTracksView(PickMp4TrackViewModel vm)
    {
        var dataGridTracks = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Tracks,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "HandlerName",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4TrackInfoDisplay.HandlerType)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Name",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4TrackInfoDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Duration",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4TrackInfoDisplay.Duration)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "IsVobSubSubtitle",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4TrackInfoDisplay.IsVobSubSubtitle)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "StartPosition",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4TrackInfoDisplay.StartPosition)),
                    IsReadOnly = true,
                },
            },
        };
        dataGridTracks.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedTrack)));
        dataGridTracks.SelectionChanged += vm.DataGridTracksSelectionChanged;
        vm.TracksGrid = dataGridTracks; 

        return UiUtil.MakeBorderForControlNoPadding(dataGridTracks);
    }

    private Border MakeSubtitleView(PickMp4TrackViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridSubtitle = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Rows,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "#",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4SubtitleCueDisplay.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Show",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4SubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = "Duration",
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(Mp4SubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = "Text/Image",
                    IsReadOnly = true,
                    CellTemplate = new FuncDataTemplate<Mp4SubtitleCueDisplay>((item, _) =>
                    {
                        var stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 5
                        };

                        // Add text if available
                        if (!string.IsNullOrEmpty(item.Text))
                        {
                            var textBlock = new TextBlock
                            {
                                Text = item.Text,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                MaxWidth = 300 // Adjust as needed
                            };
                            stackPanel.Children.Add(textBlock);
                        }

                        // Add image if available
                        if (item.Image != null)
                        {
                            var image = new Image
                            {
                                Source = item.Image.Source,
                                MaxHeight = 100, // Adjust as needed
                                MaxWidth = 200,  // Adjust as needed
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            stackPanel.Children.Add(image);
                        }

                        return stackPanel;
                    })
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
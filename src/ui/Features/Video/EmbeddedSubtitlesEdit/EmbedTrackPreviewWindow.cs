using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public class EmbedTrackPreviewWindow : Window
{
    public EmbedTrackPreviewWindow(EmbedTrackPreviewViewModel vm)
    {
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

        var subtitleView = MakeSubtitleView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

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

        grid.Add(subtitleView, 0, 0, 1, 2);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        //KeyDown += (_, e) => vm.OnKeyDown(e);
        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);


        Loaded += (_, _) => 
        {
            vm.Loaded();
            Dispatcher.UIThread.InvokeAsync(() =>
            {
            }, DispatcherPriority.Input);
        };
    }

    private static Border MakeSubtitleView(EmbedTrackPreviewViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header sorting: this is a read-only preview of the track's cues in
        // subtitle order.
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Rows;
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Number)),
            Width = new GridLength(60), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MatroskaSubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(90), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.TextOrImage,
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<MatroskaSubtitleCueDisplay>((item, _) =>
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                // Add text if available
                if (!string.IsNullOrEmpty(item.Text))
                {
                    var textBlock = new TextBlock
                    {
                        Text = item.Text,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Left,
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
        });

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }
}
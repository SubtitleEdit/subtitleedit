using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

public class ReviewSpeechHistoryWindow : Window
{
    private TableView? _tableView;

    public ReviewSpeechHistoryWindow(ReviewSpeechHistoryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ReviewAudioSegmentsHistory;
        Width = 600;
        Height = 600;
        MinWidth = 600;
        MinHeight = 400;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeHistoryGrid(vm), 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate
        {
            // initial focus on an input, not an action button - a focused button clicks on bare Space
            if (_tableView != null)
            {
                TableViewExtras.FocusRow(_tableView);
            }
        };
        KeyDown += (s, e) => vm.OnKeyDown(e);
        Closing += (s, e) => vm.OnWindowClosing(e); 
        Loaded += (s, e) => vm.OnWindowLoaded();
    }

    private Border MakeHistoryGrid(ReviewSpeechHistoryViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        _tableView = tableView;
        tableView.Margin = new Thickness(0, 10, 0, 0);
        tableView.Width = double.NaN;
        tableView.Height = double.NaN;
        tableView[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.HistoryItems));
        tableView[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedHistoryItem)) { Mode = BindingMode.TwoWay };
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ReviewHistoryRow.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Voice,
            Binding = new Binding(nameof(ReviewHistoryRow.VoiceName)),
            Width = new GridLength(3, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.History,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ReviewHistoryRow>((item, _) =>
            {
                var buttonPlay = UiUtil.MakeButton(vm.PlayItemCommand,"fa-solid fa-play")
                    .WithBindIsVisible(nameof(item.IsPlaying), new InverseBooleanConverter())
                    .WithBindEnabled(nameof(item.IsPlayingEnabled));
                buttonPlay.CommandParameter = item;

                var buttonStop = UiUtil.MakeButton(vm.StopItemCommand,"fa-solid fa-stop")
                    .WithBindIsVisible(nameof(item.IsPlaying));
                buttonStop.CommandParameter = item;

                return UiUtil.MakeHorizontalPanel(buttonPlay, buttonStop);
            }),
            Width = new GridLength(90),
        });

        return UiUtil.MakeBorderForControlNoPadding(tableView);
    }
}
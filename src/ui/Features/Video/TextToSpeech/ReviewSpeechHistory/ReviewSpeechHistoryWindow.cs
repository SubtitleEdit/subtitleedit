using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

/// <summary>
/// The earlier generations of one line, in the editor look (see <see cref="EditorChrome"/>):
/// the list with play/stop per row, Cancel and OK at the bottom.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var header = EditorChrome.MakeSectionHeader(Se.Language.General.History);
        header.Margin = new Thickness(0, 0, 0, 10);

        var list = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        list.Add(header, 0);
        list.Add(MakeHistoryGrid(vm), 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(new Border { Background = EditorChrome.PanelBrush, Child = list }, 0);
        grid.Add(EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            // initial focus on an input, not an action button - a focused button clicks on bare Space
            if (_tableView != null)
            {
                TableViewExtras.FocusRow(_tableView);
            }
        });
        KeyDown += (s, e) => vm.OnKeyDown(e);
        Closing += (s, e) => vm.OnWindowClosing(e);
        Loaded += (s, e) => vm.OnWindowLoaded();
    }

    private Border MakeHistoryGrid(ReviewSpeechHistoryViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        _tableView = tableView;
        tableView.Width = double.NaN;
        tableView.Height = double.NaN;
        tableView[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.HistoryItems));
        tableView[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedHistoryItem)) { Mode = BindingMode.TwoWay };
        AutomationProperties.SetName(tableView, Se.Language.General.History);
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
                var buttonPlay = EditorChrome.CreateToolButton("fa-solid fa-play", Se.Language.General.Play, vm.PlayItemCommand, 26)
                    .WithBindIsVisible(nameof(item.IsPlaying), InverseBooleanConverter.Instance)
                    .WithBindEnabled(nameof(item.IsPlayingEnabled));
                buttonPlay.CommandParameter = item;

                var buttonStop = EditorChrome.CreateToolButton("fa-solid fa-stop", Se.Language.General.Stop, vm.StopItemCommand, 26)
                    .WithBindIsVisible(nameof(item.IsPlaying));
                buttonStop.CommandParameter = item;

                var panel = TtsMainEditorLook.Horizontal(buttonPlay, buttonStop);
                panel.Margin = new Thickness(4, 0);
                return panel;
            }),
            Width = new GridLength(90),
        });

        return EditorChrome.MakeListFrame(tableView);
    }
}

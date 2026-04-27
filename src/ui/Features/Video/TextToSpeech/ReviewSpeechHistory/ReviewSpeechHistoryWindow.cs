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

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (s, e) => vm.OnKeyDown(e);
        Closing += (s, e) => vm.OnWindowClosing(e); 
        Loaded += (s, e) => vm.OnWindowLoaded();
    }

    private static Border MakeHistoryGrid(ReviewSpeechHistoryViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Margin = new Thickness(0, 10, 0, 0),
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.HistoryItems)),
            [!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedHistoryItem)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(ReviewHistoryRow.Number)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Voice,
                    Binding = new Binding(nameof(ReviewHistoryRow.VoiceName)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.History,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
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
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
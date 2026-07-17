using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl.PickOnlineSubtitle;

public class PickOnlineSubtitleWindow : Window
{
    public PickOnlineSubtitleWindow(PickOnlineSubtitleViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.PickOnlineSubtitleTitle;
        Width = 1024;
        Height = 600;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var tracksView = MakeTracksView(vm);
        var previewView = MakePreviewView(vm);

        var statusBar = MakeStatusBar(vm);

        var buttonSave = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveCommand);
        buttonSave.Bind(IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Bind(IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonSave, buttonOk, buttonCancel);

        var splitGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
        };
        splitGrid.Add(tracksView, 0, 0);
        splitGrid.Add(previewView, 0, 1);

        var outer = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
        };
        outer.Add(splitGrid, 0);
        outer.Add(statusBar, 1);
        outer.Add(panelButtons, 2);

        Content = outer;

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
    }

    private static Border MakeTracksView(PickOnlineSubtitleViewModel vm)
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
            DataContext = vm,
            ItemsSource = vm.Tracks,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Language,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleTrackDisplay.Language)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1.6, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleTrackDisplay.Name)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Format,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleTrackDisplay.Format)),
                    IsReadOnly = true,
                    Width = new DataGridLength(0.6, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGridTracks.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedTrack)) { Mode = BindingMode.TwoWay });
        dataGridTracks.DoubleTapped += (_, _) =>
        {
            if (vm.IsOkEnabled)
            {
                vm.OkCommand.Execute(null);
            }
        };
        vm.TracksGrid = dataGridTracks;

        return UiUtil.MakeBorderForControlNoPadding(dataGridTracks);
    }

    private static Border MakePreviewView(PickOnlineSubtitleViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGridPreview = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.PreviewRows,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Show)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(OnlineSubtitleCueDisplay.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGridPreview);
    }

    private static Border MakeStatusBar(PickOnlineSubtitleViewModel vm)
    {
        var statusText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.85,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        return new Border
        {
            Child = statusText,
            Padding = new Thickness(4, 2),
        };
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public class EmbeddedSubtitlesEditWindow : Window
{
    public EmbeddedSubtitlesEditWindow(EmbeddedSubtitlesEditViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AddRemoveEmbeddedSubtitlesTitle;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 800;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        var labelVideoFileName = UiUtil.MakeLabel(Se.Language.General.VideoFile);
        var textBoxVideoFileName = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.VideoFileName)).WithHorizontalAlignmentStretch();
        textBoxVideoFileName.IsReadOnly = true;
        var buttonBrowseVideoFile = UiUtil.MakeButtonBrowse(vm.BrowseVideoFileCommand);
        var gridVideoFile = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // label
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // textbox
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // button
            },
            ColumnSpacing = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        gridVideoFile.Add(labelVideoFileName, 0, 0);
        gridVideoFile.Add(textBoxVideoFileName, 0, 1);
        gridVideoFile.Add(buttonBrowseVideoFile, 0, 2);


        var tracksView = MakeTracksView(vm);
        var progressView = MakeProgressView(vm);

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonConfig = UiUtil.MakeButton(vm.OkCommand, IconNames.Settings)
            .WithMarginRight(5)
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonGenerate,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // video file
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // tracks
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(UiUtil.MakeBorderForControl(gridVideoFile), 0);
        grid.Add(tracksView, 1);
        grid.Add(progressView, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        Activated += delegate { buttonGenerate.Focus(); }; // hack to make OnKeyDown work
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing();
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeTracksView(EmbeddedSubtitlesEditViewModel vm)
    {
        var booleanToCheckMarkConverter = new BooleanToCheckMarkConverter();
        var booleanToDeleteMarkConverter = new BooleanToDeleteMarkConverter();
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
                    Header = string.Empty,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.Deleted)) { Mode = BindingMode.OneWay, Converter = booleanToDeleteMarkConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.Name)) { Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Language,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.LanguageOrTitle)) { Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Default,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.Default)) { Converter = booleanToCheckMarkConverter, Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Forced,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.Forced)) { Converter = booleanToCheckMarkConverter, Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Codec,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.Format)) { Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(EmbeddedTrack.FileName)) { Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGridTracks.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Tracks)) { Source = vm });
        dataGridTracks.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedTrck)) { Source = vm });
        dataGridTracks.KeyDown += (s, e) => vm.OnTracksGridKeyDown(e);
        dataGridTracks.DoubleTapped += (s, e) => vm.EditCommand.Execute(null);
        vm.TracksGrid = dataGridTracks;

        var buttonAdd = new SplitButton
        {
            Content = Se.Language.General.Add,
            Command = vm.AddCommand,
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.AddCurrentSubtitle,
                        Command = vm.AddCurrentCommand,
                    },
                }
            }
        };
        var buttonEdit = UiUtil.MakeButton(Se.Language.General.Edit, vm.EditCommand);
        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand);
        var buttonPreview = UiUtil.MakeButton(Se.Language.General.Preview, vm.PreviewCommand);

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                buttonAdd,
                buttonEdit,
                buttonDelete,
                buttonPreview,
            },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // tracks
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(dataGridTracks, 0, 0);
        grid.Add(panelButtons, 1, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeProgressView(EmbeddedSubtitlesEditViewModel vm)
    {
        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var statusText = new TextBlock
        {
            Margin = new Thickness(5, 20, 0, 0),
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressSlider, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }
}

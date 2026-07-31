using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public class EmbeddedSubtitlesEditMp4Window : Window
{
    public EmbeddedSubtitlesEditMp4Window(EmbeddedSubtitlesEditMp4ViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AddRemoveEmbeddedSubtitlesMp4Title;
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
        var buttonBrowseVideoFile = UiUtil.MakeButtonBrowse(vm.BrowseVideoFileCommand, accessibleName: Se.Language.General.VideoFile);
        var gridVideoFile = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
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
            .WithBindEnabled(nameof(vm.CanGenerate))
            .WithBindIsVisible(nameof(vm.HasVideoFileName));
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

        Activated += delegate { textBoxVideoFileName.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing();
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeTracksView(EmbeddedSubtitlesEditMp4ViewModel vm)
    {
        var booleanToCheckMarkConverter = new BooleanToCheckMarkConverter();
        var booleanToDeleteMarkConverter = new BooleanToDeleteMarkConverter();
        // No header sorting: the track list's order is the output track order
        // (FfmpegGenerator.AlterEmbeddedTracksMp4 consumes Tracks in list order).
        var dataGridTracks = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridTracks.Width = double.NaN;
        dataGridTracks.Height = double.NaN;
        dataGridTracks.DataContext = vm;
        dataGridTracks.ItemsSource = vm.Tracks;
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = string.Empty,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.Deleted)) { Mode = BindingMode.OneWay, Converter = booleanToDeleteMarkConverter },
            Width = new GridLength(40), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            // Visual cue distinguishing newly-added tracks from streams already
            // present in the MP4. Without this users can't tell which row is
            // theirs vs the original after clicking Add.
            Header = Se.Language.General.New,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.New)) { Mode = BindingMode.OneWay, Converter = booleanToCheckMarkConverter },
            Width = new GridLength(60), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.Name)) { Mode = BindingMode.OneWay },
            Width = new GridLength(160), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Language,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.LanguageOrTitle)) { Mode = BindingMode.OneWay },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Default,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.Default)) { Converter = booleanToCheckMarkConverter, Mode = BindingMode.OneWay },
            Width = new GridLength(80), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Forced,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.Forced)) { Converter = booleanToCheckMarkConverter, Mode = BindingMode.OneWay },
            Width = new GridLength(80), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Codec,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.Format)) { Mode = BindingMode.OneWay },
            Width = new GridLength(90), // was content-sized (Auto) on the DataGrid
        });
        dataGridTracks.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FileName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.FileName)) { Mode = BindingMode.OneWay },
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGridTracks.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Tracks)) { Source = vm });
        dataGridTracks.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedTrack)) { Source = vm });
        dataGridTracks.KeyDown += (s, e) => vm.OnTracksGridKeyDown(e);
        dataGridTracks.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGridTracks.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGridTracks.SelectedItem = target;
                if (target != null)
                {
                    dataGridTracks.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
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
        }
            .WithBindIsVisible(nameof(vm.HasVideoFileName))
            .WithBindEnabled(nameof(vm.CanEditTracks));
        var buttonEdit = UiUtil.MakeButton(Se.Language.General.Edit, vm.EditCommand)
            .WithBindIsVisible(nameof(vm.HasVideoFileName))
            .WithBindEnabled(nameof(vm.CanEditTracks));
        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand)
            .WithBindIsVisible(nameof(vm.HasVideoFileName))
            .WithBindEnabled(nameof(vm.CanEditTracks));
        var buttonPreview = UiUtil.MakeButton(Se.Language.General.Preview, vm.PreviewCommand)
            .WithBindIsVisible(nameof(vm.HasVideoFileName))
            .WithBindEnabled(nameof(vm.CanEditTracks));

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

    private static Grid MakeProgressView(EmbeddedSubtitlesEditMp4ViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

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

        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }
}

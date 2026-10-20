using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System.Collections;
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
        Height = 640;
        MinWidth = 800;
        MinHeight = 420;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var textBoxVideoFileName = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.VideoFileName))
            .WithHorizontalAlignmentStretch()
            .WithAccessibleName(Se.Language.General.VideoFile); // the label beside it is icon + text, not a plain label (#12087)
        textBoxVideoFileName.IsReadOnly = true;
        textBoxVideoFileName.Classes.Add("text");
        var buttonBrowseVideoFile = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.VideoFile, vm.BrowseVideoFileCommand, 28);
        buttonBrowseVideoFile.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsGenerating)) { Converter = InverseBooleanConverter.Instance });

        var tracksView = MakeTracksView(vm);
        var progressView = EmbeddedTracksUi.MakeProgressView();

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.CanGenerate))
            .WithBindIsVisible(nameof(vm.HasVideoFileName));
        buttonGenerate.IsDefault = true; // the dialog's accept button - Enter runs it (#14586)
        buttonGenerate.Classes.Add("primary");
        var buttonPanel = EditorChrome.MakeButtonBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonGenerate);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // video file
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // tracks
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
        };

        grid.Add(EmbeddedTracksUi.MakeVideoFileBar(textBoxVideoFileName, buttonBrowseVideoFile), 0);
        grid.Add(tracksView, 1);
        grid.Add(progressView, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxVideoFileName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        EmbeddedTracksUi.AttachVideoDrop(this, vm.VideoDragOver, vm.VideoDrop);
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing();
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Grid MakeTracksView(EmbeddedSubtitlesEditViewModel vm)
    {
        var booleanToCheckMarkConverter = new BooleanToCheckMarkConverter();
        var booleanToDeleteMarkConverter = new BooleanToDeleteMarkConverter();
        // No header sorting: the track list's order is the output track order
        // (FfmpegGenerator.AlterEmbeddedTracksMatroska consumes Tracks in list order).
        var dataGridTracks = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridTracks.Width = double.NaN;
        dataGridTracks.Height = double.NaN;
        dataGridTracks.WithAccessibleName(Se.Language.Video.EmbeddedTracks);
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
            // Tells newly added tracks apart from the ones already in the video file.
            Header = Se.Language.General.New,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(EmbeddedTrack.New)) { Mode = BindingMode.OneWay, Converter = booleanToCheckMarkConverter },
            Width = new GridLength(60),
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
        dataGridTracks.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedTrck)) { Source = vm });
        dataGridTracks.KeyDown += (s, e) => vm.OnTracksGridKeyDown(e);
        dataGridTracks.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            // Ctrl+Up/Down reorders - handled on tunnel, before the grid's own row navigation.
            if (e.KeyModifiers == KeyModifiers.Control && e.Key is Key.Up or Key.Down)
            {
                vm.OnTracksGridKeyDown(e);
                return;
            }

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

        var commands = new EmbeddedTracksUi.Commands(
            vm.AddCommand,
            vm.AddCurrentCommand,
            vm.EditCommand,
            vm.DeleteCommand,
            vm.PreviewCommand,
            vm.MoveUpCommand,
            vm.MoveDownCommand);
        EmbeddedTracksUi.DimDeletedRows(dataGridTracks);
        EmbeddedTracksUi.AttachContextMenu(dataGridTracks, vm, commands, nameof(vm.CanEditTracks));

        return EmbeddedTracksUi.MakeTracksPanel(dataGridTracks, commands, nameof(vm.CanEditTracks));
    }
}

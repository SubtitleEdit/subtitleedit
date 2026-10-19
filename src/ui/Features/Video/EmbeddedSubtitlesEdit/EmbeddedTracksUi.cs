using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Icon = Optris.Icons.Avalonia.Icon;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

/// <summary>
/// Track list pieces shared by the Matroska and MP4 "Add/remove embedded subtitles" windows, in
/// the editor look (see <see cref="EditorChrome"/>): the video file bar, the track list with its
/// action icons in the header, the tracks context menu, dimming of deleted rows, the empty-state
/// hint, video file drop and the progress strip.
/// Both view models expose the same command and state names (IsTrackSelected, DeleteText, ...).
/// </summary>
internal static class EmbeddedTracksUi
{
    internal sealed record Commands(
        IRelayCommand Add,
        IRelayCommand AddCurrent,
        IRelayCommand Edit,
        IRelayCommand Delete,
        IRelayCommand Preview,
        IRelayCommand MoveUp,
        IRelayCommand MoveDown);

    private static readonly IValueConverter DeletedToOpacity = new FuncValueConverter<bool, double>(deleted => deleted ? 0.45 : 1.0);

    /// <summary>
    /// Fades rows marked for deletion so it is obvious which tracks will not be in the output
    /// (the ❌ column alone is easy to miss).
    /// </summary>
    public static void DimDeletedRows(TableView tableView)
    {
        TableViewExtras.BindRowProperty(tableView, Visual.OpacityProperty,
            new Binding(nameof(EmbeddedTrack.Deleted)) { Converter = DeletedToOpacity });
    }

    /// <summary>
    /// The track actions as icon buttons for the list header: add (a file or the current
    /// subtitle), edit, delete/undelete, preview, and move up/down. Hidden until a video is chosen.
    /// </summary>
    /// <param name="addEnabledPath">Optional view model property gating Add (off while generating; MP4 also waits for the track scan).</param>
    public static StackPanel MakeButtons(Commands commands, string? addEnabledPath)
    {
        var buttonAdd = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Add, commands.Add, 26);
        var buttonAddCurrent = EditorChrome.CreateToolButton("fa-solid fa-file-circle-plus", Se.Language.Video.AddCurrentSubtitle, commands.AddCurrent, 26);
        if (addEnabledPath != null)
        {
            buttonAdd.WithBindEnabled(addEnabledPath);
            buttonAddCurrent.WithBindEnabled(addEnabledPath);
        }

        var buttonEdit = EditorChrome.CreateToolButton("fa-solid fa-pen", Se.Language.General.Edit, commands.Edit, 26)
            .WithBindEnabled("IsTrackSelected");

        // Delete toggles the mark, so the name follows the selected track: Delete / Undelete.
        var buttonDelete = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, commands.Delete, 26)
            .WithBindEnabled("IsTrackSelected");
        buttonDelete.Bind(AutomationProperties.NameProperty, new Binding("DeleteText"));
        if (Se.Settings.Appearance.ShowHints)
        {
            buttonDelete.Bind(ToolTip.TipProperty, new Binding("DeleteText"));
        }

        var buttonPreview = EditorChrome.CreateToolButton("fa-solid fa-eye", Se.Language.General.Preview, commands.Preview, 26)
            .WithBindEnabled("IsTrackSelected");

        var buttonMoveUp = EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, commands.MoveUp, 26)
            .WithBindEnabled("IsMoveUpEnabled");
        var buttonMoveDown = EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, commands.MoveDown, 26)
            .WithBindEnabled("IsMoveDownEnabled");

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonAdd,
                buttonAddCurrent,
                EditorChrome.MakeToolbarSeparator(),
                buttonEdit,
                buttonDelete,
                buttonPreview,
                EditorChrome.MakeToolbarSeparator(),
                buttonMoveUp,
                buttonMoveDown,
            },
        };
        panel.Bind(Visual.IsVisibleProperty, new Binding("HasVideoFileName"));
        return panel;
    }

    public static void AttachContextMenu(TableView tableView, object vm, Commands commands, string? addEnabledPath)
    {
        var menuAdd = new MenuItem
        {
            Header = Se.Language.General.AddDotDotDot,
            Icon = MakeMenuIcon(IconNames.Plus),
            InputGesture = new KeyGesture(Key.Insert),
            DataContext = vm,
            Command = commands.Add,
        };
        if (addEnabledPath != null)
        {
            menuAdd.Bind(MenuItem.IsEnabledProperty, new Binding(addEnabledPath) { Source = vm });
        }

        var menuEdit = new MenuItem
        {
            Header = Se.Language.General.EditDotDotDot,
            Icon = MakeMenuIcon(IconNames.Pencil),
            DataContext = vm,
            Command = commands.Edit,
        };
        menuEdit.Bind(MenuItem.IsEnabledProperty, new Binding("IsTrackSelected") { Source = vm });

        var menuDelete = new MenuItem
        {
            Icon = MakeMenuIcon(IconNames.Trash),
            InputGesture = new KeyGesture(Key.Delete),
            DataContext = vm,
            Command = commands.Delete,
        };
        menuDelete.Bind(MenuItem.HeaderProperty, new Binding("DeleteText") { Source = vm });
        menuDelete.Bind(MenuItem.IsEnabledProperty, new Binding("IsTrackSelected") { Source = vm });

        var menuPreview = new MenuItem
        {
            Header = Se.Language.General.Preview,
            Icon = MakeMenuIcon(IconNames.Eye),
            DataContext = vm,
            Command = commands.Preview,
        };
        menuPreview.Bind(MenuItem.IsEnabledProperty, new Binding("IsTrackSelected") { Source = vm });

        var menuMoveUp = new MenuItem
        {
            Header = Se.Language.General.MoveUp,
            Icon = MakeMenuIcon(IconNames.ArrowUpThin),
            InputGesture = new KeyGesture(Key.Up, KeyModifiers.Control),
            DataContext = vm,
            Command = commands.MoveUp,
        };
        menuMoveUp.Bind(MenuItem.IsEnabledProperty, new Binding("IsMoveUpEnabled") { Source = vm });

        var menuMoveDown = new MenuItem
        {
            Header = Se.Language.General.MoveDown,
            Icon = MakeMenuIcon(IconNames.ArrowDownThin),
            InputGesture = new KeyGesture(Key.Down, KeyModifiers.Control),
            DataContext = vm,
            Command = commands.MoveDown,
        };
        menuMoveDown.Bind(MenuItem.IsEnabledProperty, new Binding("IsMoveDownEnabled") { Source = vm });

        tableView.ContextFlyout = new MenuFlyout
        {
            Items =
            {
                menuAdd,
                new Separator(),
                menuEdit,
                menuDelete,
                menuPreview,
                new Separator(),
                menuMoveUp,
                menuMoveDown,
            },
        };
        UiUtil.AttachMacContextFlyoutHandler(tableView);
    }

    /// <summary>
    /// Puts <paramref name="tracksTable"/> in a cell with a hint on top that is shown until a
    /// video file is chosen - an empty table with only column headers gives no clue what to do.
    /// </summary>
    public static Grid MakeTracksWithEmptyHint(TableView tracksTable)
    {
        var hint = new TextBlock
        {
            Text = Se.Language.Video.EmbeddedTracksPickVideoHint,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 360,
            FontSize = 12.5,
            Foreground = EditorChrome.FaintTextBrush,
            IsHitTestVisible = false,
        };
        hint.Bind(Visual.IsVisibleProperty, new Binding("HasVideoFileName") { Converter = InverseBooleanConverter.Instance });

        var grid = new Grid();
        grid.Children.Add(tracksTable);
        grid.Children.Add(hint);
        return grid;
    }

    /// <summary>
    /// Dropping a supported video file anywhere on the window loads it, like Browse.
    /// </summary>
    public static void AttachVideoDrop(Window window, System.EventHandler<DragEventArgs> dragOver, System.EventHandler<DragEventArgs> drop)
    {
        DragDrop.SetAllowDrop(window, true);
        window.AddHandler(DragDrop.DragOverEvent, dragOver, RoutingStrategies.Bubble);
        window.AddHandler(DragDrop.DropEvent, drop, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Progress bar with the status text beside it, in a strip above the button bar while
    /// generating.
    /// </summary>
    public static Border MakeProgressView()
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding("ProgressValue"));
        return EditorChrome.MakeProgressStrip(progressBar, "ProgressText", "IsGenerating");
    }

    /// <summary>
    /// The track list: a header with the actions, the table in a field frame under it.
    /// </summary>
    public static Grid MakeTracksPanel(TableView tracksTable, Commands commands, string? addEnabledPath)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8), MinHeight = 26 };
        var actions = MakeButtons(commands, addEnabledPath);
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Video.EmbeddedTracks));

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        panel.Add(header, 0);
        panel.Add(EditorChrome.MakeListFrame(MakeTracksWithEmptyHint(tracksTable)), 1);
        return panel;
    }

    /// <summary>
    /// The video file row on the panel color: icon and label, the (read-only) file name, its size
    /// and browse.
    /// </summary>
    public static Border MakeVideoFileBar(TextBox textBoxVideoFileName, Button buttonBrowse)
    {
        var icon = new Icon
        {
            Value = "fa-solid fa-file-video",
            FontSize = 13,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var label = EditorChrome.MakeDimLabel(Se.Language.General.VideoFile, new Thickness(0));
        var fileSize = EditorChrome.MakeBoundHint("VideoFileSize");

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), // icon
                new ColumnDefinition(GridLength.Auto), // label
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)), // file name
                new ColumnDefinition(GridLength.Auto), // file size
                new ColumnDefinition(GridLength.Auto), // browse
            },
            ColumnSpacing = 8,
        };
        grid.Add(icon, 0, 0);
        grid.Add(label, 0, 1);
        grid.Add(textBoxVideoFileName, 0, 2);
        grid.Add(fileSize, 0, 3);
        grid.Add(buttonBrowse, 0, 4);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 10),
        };
    }

    private static Icon MakeMenuIcon(string iconName) =>
        new() { Value = iconName, VerticalAlignment = VerticalAlignment.Center };
}

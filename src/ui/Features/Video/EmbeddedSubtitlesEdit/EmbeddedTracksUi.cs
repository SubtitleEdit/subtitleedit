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
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Attached = Optris.Icons.Avalonia.Attached;
using Icon = Optris.Icons.Avalonia.Icon;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

/// <summary>
/// Track list pieces shared by the Matroska and MP4 "Add/remove embedded subtitles" windows:
/// the button row under the tracks, the tracks context menu, dimming of deleted rows, the
/// empty-state hint, video file drop and the progress view.
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

    /// <param name="addEnabledPath">Optional view model property gating Add (off while generating; MP4 also waits for the track scan).</param>
    public static StackPanel MakeButtons(Commands commands, string? addEnabledPath)
    {
        var buttonAdd = new SplitButton
        {
            Content = MakeIconText(IconNames.Plus, Se.Language.General.Add),
            Command = commands.Add,
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.AddCurrentSubtitle,
                        Command = commands.AddCurrent,
                    },
                }
            }
        }.WithBindIsVisible("HasVideoFileName");
        AutomationProperties.SetName(buttonAdd, Se.Language.General.Add);
        if (addEnabledPath != null)
        {
            buttonAdd.WithBindEnabled(addEnabledPath);
        }

        var buttonEdit = UiUtil.MakeButton(Se.Language.General.Edit, commands.Edit)
            .WithIconLeft(IconNames.Pencil)
            .WithBindIsVisible("HasVideoFileName")
            .WithBindEnabled("IsTrackSelected");

        // Delete toggles the mark, so the caption follows the selected track: Delete / Undelete.
        var textDelete = new TextBlock { Padding = new Thickness(4, 0, 0, 0) };
        textDelete.Bind(TextBlock.TextProperty, new Binding("DeleteText"));
        var iconDelete = new ContentControl();
        Attached.SetIcon(iconDelete, IconNames.Trash);
        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, commands.Delete)
            .WithBindIsVisible("HasVideoFileName")
            .WithBindEnabled("IsTrackSelected");
        buttonDelete.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { iconDelete, textDelete },
        };
        buttonDelete.Bind(AutomationProperties.NameProperty, new Binding("DeleteText"));

        var buttonPreview = UiUtil.MakeButton(Se.Language.General.Preview, commands.Preview)
            .WithIconLeft(IconNames.Eye)
            .WithBindIsVisible("HasVideoFileName")
            .WithBindEnabled("IsTrackSelected");

        var buttonMoveUp = UiUtil.MakeButton(commands.MoveUp, IconNames.ArrowUpThin, Se.Language.General.MoveUp)
            .WithBindIsVisible("HasVideoFileName")
            .WithBindEnabled("IsMoveUpEnabled");
        var buttonMoveDown = UiUtil.MakeButton(commands.MoveDown, IconNames.ArrowDownThin, Se.Language.General.MoveDown)
            .WithBindIsVisible("HasVideoFileName")
            .WithBindEnabled("IsMoveDownEnabled");
        foreach (var button in new[] { buttonMoveUp, buttonMoveDown })
        {
            // Icon-only buttons are otherwise shorter than the text buttons beside them.
            button.VerticalAlignment = VerticalAlignment.Stretch;
            button.MinWidth = 36;
            button.Margin = new Thickness(2, 0);
        }

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                buttonAdd,
                buttonEdit,
                buttonDelete,
                buttonPreview,
                buttonMoveUp,
                buttonMoveDown,
            },
        };
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
            Opacity = 0.6,
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
    /// Progress bar with the status text below it (they used to share one grid cell, with the
    /// text pushed down by a margin so it overlapped the bar).
    /// </summary>
    public static StackPanel MakeProgressView()
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding("ProgressValue"));

        var statusText = new TextBlock { Margin = new Thickness(2, 0, 0, 0) };
        statusText.Bind(TextBlock.TextProperty, new Binding("ProgressText"));

        var panel = new StackPanel
        {
            Spacing = 4,
            Children = { progressBar, statusText },
        };
        panel.Bind(Visual.IsVisibleProperty, new Binding("IsGenerating"));
        return panel;
    }

    private static StackPanel MakeIconText(string iconName, string text)
    {
        var icon = new ContentControl();
        Attached.SetIcon(icon, iconName);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { icon, new TextBlock { Text = text, Padding = new Thickness(4, 0, 0, 0) } },
        };
    }

    private static Icon MakeMenuIcon(string iconName) =>
        new() { Value = iconName, VerticalAlignment = VerticalAlignment.Center };
}

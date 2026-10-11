using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System.Collections.ObjectModel;
using System.Linq;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

/// <summary>
/// Controls the Find and Replace windows share, so the two dialogs look and behave the same.
/// </summary>
internal static class FindWindowParts
{
    public const string ResultPanelName = "FindResultPanel";

    /// <summary>
    /// SE4-style "most recent find text" dropdown: the AutoCompleteBox only reveals history while
    /// typing a matching prefix, so recent searches were invisible until this button.
    /// Each entry has a delete icon (Delete key on a highlighted entry does the same), and a
    /// "Clear history" item ends the list (#15907).
    /// </summary>
    public static Button MakeHistoryButton(
        ObservableCollection<string> searchHistory,
        IRelayCommand<string> showHistoryCommand,
        IRelayCommand<string> removeHistoryCommand,
        IRelayCommand clearHistoryCommand)
    {
        var historyFlyout = new MenuFlyout();
        var buttonHistory = UiUtil.MakeButton(null, IconNames.History, Se.Language.General.ShowHistory);
        buttonHistory.Margin = new Thickness(3, 0, 0, 3);
        buttonHistory.Flyout = historyFlyout;

        // The items must exist BEFORE the flyout opens: items added from the Opening
        // event come too late for the popup's initial measure, so the menu displayed
        // as an empty sliver on Windows. Build eagerly and rebuild on history changes.
        void RebuildHistoryMenu()
        {
            historyFlyout.Items.Clear();
            foreach (var text in searchHistory)
            {
                historyFlyout.Items.Add(MakeHistoryMenuItem(text, showHistoryCommand, removeHistoryCommand));
            }

            if (searchHistory.Count > 0)
            {
                historyFlyout.Items.Add(new Separator());
                historyFlyout.Items.Add(new MenuItem
                {
                    Header = Se.Language.General.ClearHistory,
                    Command = clearHistoryCommand,
                    Icon = new Icon { Value = IconNames.Trash },
                });
            }
            else if (historyFlyout.IsOpen)
            {
                historyFlyout.Hide();
            }

            buttonHistory.IsVisible = searchHistory.Count > 0;
        }

        searchHistory.CollectionChanged += (_, _) => RebuildHistoryMenu();
        RebuildHistoryMenu();

        return buttonHistory;
    }

    private static MenuItem MakeHistoryMenuItem(string text, IRelayCommand<string> showHistoryCommand, IRelayCommand<string> removeHistoryCommand)
    {
        var menuItem = new MenuItem
        {
            Header = MakeHistoryRow(removeHistoryCommand, text),
            Command = showHistoryCommand,
            CommandParameter = text,
        };
        menuItem.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                removeHistoryCommand.Execute(text);
            }
        };

        return menuItem;
    }

    /// <summary>
    /// Item template for the search box's own suggestion drop-down, so history entries can be
    /// removed from there too (#15907).
    /// </summary>
    public static void SetHistoryItemTemplate(AutoCompleteBox searchBox, IRelayCommand<string> removeHistoryCommand)
    {
        searchBox.ItemTemplate = new FuncDataTemplate<string>((_, _) => MakeHistoryRow(removeHistoryCommand, null, searchBox));
    }

    /// <summary>
    /// History text with a delete icon to the right. The row binds to its DataContext (the history
    /// text), so recycled drop-down containers stay correct; menu rows get the text set directly.
    /// The delete button swallows the press, so it neither picks the entry nor moves focus to the
    /// list row, and removes on release (removing on press would move the pointer capture to the
    /// list). The search box's list still commits and closes on that release, so it is reopened.
    /// </summary>
    private static Grid MakeHistoryRow(IRelayCommand<string> removeHistoryCommand, string? text, AutoCompleteBox? searchBox = null)
    {
        var buttonRemove = new Button
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(4, 0),
            Margin = new Thickness(12, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Command = removeHistoryCommand,
            [!Button.CommandParameterProperty] = new Binding("."),
            Focusable = false,
        };
        var pressed = false;
        buttonRemove.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            if (e.GetCurrentPoint(buttonRemove).Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
                pressed = true;
            }
        }, RoutingStrategies.Tunnel);
        buttonRemove.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (!pressed)
            {
                return;
            }

            pressed = false;
            e.Handled = true;
            removeHistoryCommand.Execute(buttonRemove.CommandParameter as string);
            if (searchBox != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (searchBox.ItemsSource?.Cast<object>().Any() == true)
                    {
                        searchBox.IsDropDownOpen = true;
                    }
                });
            }
        }, RoutingStrategies.Tunnel);
        Attached.SetIcon(buttonRemove, IconNames.Close);
        AutomationProperties.SetName(buttonRemove, Se.Language.General.RemoveFromHistory);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonRemove, Se.Language.General.RemoveFromHistory);
        }

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        if (text != null)
        {
            row.DataContext = text;
        }

        // TextBlock: a plain string menu header would eat '_' as an access-key marker.
        row.Add(new TextBlock { [!TextBlock.TextProperty] = new Binding("."), VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        row.Add(buttonRemove, 0, 1);
        return row;
    }

    /// <summary>
    /// The search box with its history button to the right.
    /// </summary>
    public static Grid MakeSearchPanel(Control searchBox, Button historyButton)
    {
        var panel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Add(searchBox, 0, 0);
        panel.Add(historyButton, 0, 1);
        return panel;
    }

    /// <summary>
    /// The result line under the buttons ("Found 3 matches", "Replaced 12 occurrences"): an icon
    /// bound to <paramref name="iconPropertyPath"/> next to the text, hidden while there is no text.
    /// </summary>
    public static StackPanel MakeResultPanel(string textPropertyPath, string iconPropertyPath)
    {
        var icon = new ContentControl
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
        };
        icon.Bind(Attached.IconProperty, new Binding(iconPropertyPath));

        var text = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(textPropertyPath) { Mode = BindingMode.OneWay },
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 220,
        };

        return new StackPanel
        {
            Name = ResultPanelName,
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0),
            Children = { icon, text },
            [!Visual.IsVisibleProperty] = new Binding(textPropertyPath)
            {
                Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
            },
        };
    }
}

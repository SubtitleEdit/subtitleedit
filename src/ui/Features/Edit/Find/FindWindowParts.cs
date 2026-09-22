using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System.Collections.ObjectModel;
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
    /// </summary>
    public static Button MakeHistoryButton(ObservableCollection<string> searchHistory, IRelayCommand<string> showHistoryCommand)
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
                historyFlyout.Items.Add(new MenuItem
                {
                    // TextBlock header: a plain string header would eat '_' as an access-key marker.
                    Header = new TextBlock { Text = text },
                    Command = showHistoryCommand,
                    CommandParameter = text,
                });
            }

            buttonHistory.IsVisible = searchHistory.Count > 0;
        }

        searchHistory.CollectionChanged += (_, _) => RebuildHistoryMenu();
        RebuildHistoryMenu();

        return buttonHistory;
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

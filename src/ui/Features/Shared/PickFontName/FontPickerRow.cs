using Avalonia.Controls;
using Avalonia.Layout;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

/// <summary>
/// A font name drop-down with a "..." button that opens the searchable font picker, like the
/// ASSA styles and secondary subtitle windows have. The drop-down alone means scrolling
/// through every installed font.
/// </summary>
public static class FontPickerRow
{
    public static Grid Make(ComboBox comboBoxFontName)
    {
        comboBoxFontName.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxFontName.Width = double.NaN;

        var button = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Tools.PickFontNameTitle, null, 28);
        button.Click += async (_, _) => await PickFont(comboBoxFontName);

        var row = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        row.Add(comboBoxFontName, 0, 0);
        row.Add(button, 0, 1);
        return row;
    }

    private static async Task PickFont(ComboBox comboBoxFontName)
    {
        if (TopLevel.GetTopLevel(comboBoxFontName) is not Window owner)
        {
            return;
        }

        var currentFontName = comboBoxFontName.SelectedItem as string;
        var windowService = Locator.Services.GetRequiredService<IWindowService>();
        var result = await windowService.ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(owner, vm =>
        {
            vm.Initialize();
            if (!string.IsNullOrEmpty(currentFontName))
            {
                vm.SelectedFontName = currentFontName;
            }
        });

        if (!result.OkPressed || string.IsNullOrEmpty(result.SelectedFontName))
        {
            return;
        }

        // A "Collected fonts" pick need not be installed, so it need not be in the drop-down:
        // list it, where the list can show a new item.
        if (comboBoxFontName.ItemsSource is IList<string> items && !items.Contains(result.SelectedFontName))
        {
            if (items is not ObservableCollection<string>)
            {
                return;
            }

            items.Insert(0, result.SelectedFontName);
        }

        comboBoxFontName.SelectedItem = result.SelectedFontName;
    }
}

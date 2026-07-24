using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

using System.Collections;

namespace Nikse.SubtitleEdit.Features.Tools.Romanize;

public class RomanizeWindow : Window
{
    public RomanizeWindow(RomanizeViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.Romanize.Title;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeLanguagesView(vm), 0);
        grid.Add(MakeRomanizedView(vm), 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;

        vm.RomanizeCommand.Execute(null);
    }

    private static Grid MakeLanguagesView(RomanizeViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var toggletitle = new TextBlock
        { 
            Text = Se.Language.Tools.Romanize.ToggleTitle
        };
        var korean = new CheckBox
        {
            Content = Se.Language.Tools.Romanize.Korean
        };
        var japanese = new CheckBox
        {
            Content = Se.Language.Tools.Romanize.Japanese
        };
        var russian = new CheckBox
        {
            Content = Se.Language.Tools.Romanize.Russian,
        };

        korean.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(RomanizeViewModel.RomanizeKorean)));
        japanese.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(RomanizeViewModel.RomanizeJapanese)));
        russian.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(RomanizeViewModel.RomanizeRussian)));

        grid.Add(toggletitle, 0, 0, 1, 3);
        grid.Add(korean, 1, 0);
        grid.Add(japanese, 1, 1);
        grid.Add(russian, 1, 2);

        return grid;
    }
    private static Grid MakeRomanizedView(RomanizeViewModel vm)
    {
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
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var dataGrid = new DataGrid
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
            ItemsSource = vm.SubtitleItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(RomanizeSubtitleLineItem.LineNumber)),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.OriginalText,
                    Binding = new Binding(nameof(RomanizeSubtitleLineItem.TextOriginal)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Romanize,
                    Binding = new Binding(nameof(RomanizeSubtitleLineItem.TextRomanized)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                    IsReadOnly = false,
                },
            },
        };

        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                dataGrid.ScrollIntoView(target, null);
                e.Handled = true;
            }

        }, RoutingStrategies.Tunnel);

        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 0);

        return grid;
    }
}

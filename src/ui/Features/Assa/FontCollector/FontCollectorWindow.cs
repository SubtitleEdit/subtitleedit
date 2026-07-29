using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.FontCollector;

public class FontCollectorWindow : Window
{
    private readonly FontCollectorViewModel _vm;

    public FontCollectorWindow(FontCollectorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.FontCollectorTitle;
        CanResize = true;
        Width = 800;
        Height = 500;
        MinWidth = 600;
        MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            IsReadOnly = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.FontItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FontCollectorItem.FontName)),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Assa.FontCollectorUsedIn,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FontCollectorItem.UsedIn)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FontCollectorItem.Status)),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Assa.FontCollectorFontFiles,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FontCollectorItem.FileDisplay)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };

        var statusText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };

        var buttonCopy = UiUtil.MakeButton(Se.Language.Assa.FontCollectorCopyFontsToFolderDotDotDot, vm.CopyFontsToFolderCommand);
        var buttonBar = UiUtil.MakeButtonBar(
            buttonCopy,
            UiUtil.MakeButtonDone(vm.CloseCommand));

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
            ColumnSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0, 1, 2);
        grid.Add(statusText, 1, 0);
        grid.Add(buttonBar, 1, 1);

        Content = grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

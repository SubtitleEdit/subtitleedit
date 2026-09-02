using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.GridColumns;

public class GridColumnsWindow : Window
{
    public GridColumnsWindow(GridColumnsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Columns;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox
        {
            Width = 300,
            Height = 420,
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(GridColumnsViewModel.Columns)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(GridColumnsViewModel.SelectedColumn)) { Mode = BindingMode.TwoWay });
        listBox.ItemTemplate = new FuncDataTemplate<GridColumnDisplay>((_, _) =>
        {
            // The always-shown/content-driven columns (Number, Text, teletext, original)
            // still take part in ordering, but their visibility is not the user's to set.
            var checkBox = new CheckBox();
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(GridColumnDisplay.IsVisible)) { Mode = BindingMode.TwoWay });
            checkBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(GridColumnDisplay.CanToggle)));

            var textBlock = new TextBlock { Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(GridColumnDisplay.Name)));

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { checkBox, textBlock },
            };
        }, true);

        var buttonMoveUp = UiUtil.MakeButton(Se.Language.General.MoveUp, vm.MoveUpCommand).WithMinWidth(100);
        var buttonMoveDown = UiUtil.MakeButton(Se.Language.General.MoveDown, vm.MoveDownCommand).WithMinWidth(100);
        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand).WithMinWidth(100);

        var sidePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                buttonMoveUp,
                buttonMoveDown,
                buttonReset,
            },
        };

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        contentGrid.Add(listBox, 0, 0);
        contentGrid.Add(sidePanel, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(contentGrid, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, listBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

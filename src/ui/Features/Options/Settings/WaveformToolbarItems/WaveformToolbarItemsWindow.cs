using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

public class WaveformToolbarItemsWindow : Window
{
    public WaveformToolbarItemsWindow(WaveformToolbarItemsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.WaveformToolbarItems;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox
        {
            Width = 380,
            Height = 380,
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(WaveformToolbarItemsViewModel.ToolbarItems)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(WaveformToolbarItemsViewModel.SelectedToolbarItem)) { Mode = BindingMode.TwoWay });
        listBox.ItemTemplate = new FuncDataTemplate<ToolbarItemDisplay>((_, _) =>
        {
            var checkBox = new CheckBox();
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(ToolbarItemDisplay.IsVisible)) { Mode = BindingMode.TwoWay });

            var textBlock = new TextBlock { Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ToolbarItemDisplay.Name)));

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { checkBox, textBlock },
            };
        }, true);

        var buttonMoveUp = UiUtil.MakeButton(Se.Language.General.MoveUp, vm.MoveUpCommand).WithMinWidth(100);
        var buttonMoveDown = UiUtil.MakeButton(Se.Language.General.MoveDown, vm.MoveDownCommand).WithMinWidth(100);

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
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 2,
                    Margin = new Thickness(0, 5, 0, 0),
                    Children =
                    {
                        UiUtil.MakeLabel(Se.Language.General.FontSize),
                        UiUtil.MakeNumericUpDownInt(6, 72, 12, 120, vm, nameof(vm.SelectedFontSize)),
                    },
                },
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 2,
                    Children =
                    {
                        UiUtil.MakeLabel(Se.Language.General.LeftMargin),
                        UiUtil.MakeNumericUpDownInt(0, 100, 5, 120, vm, nameof(vm.SelectedLeftMargin)),
                    },
                },
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 2,
                    Children =
                    {
                        UiUtil.MakeLabel(Se.Language.General.RightMargin),
                        UiUtil.MakeNumericUpDownInt(0, 100, 5, 120, vm, nameof(vm.SelectedRightMargin)),
                    },
                },
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

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public class AssaTagHistoryWindow : Window
{
    public AssaTagHistoryWindow(AssaTagHistoryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.OverrideTagsHistory;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 450;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(200) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 15,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.Assa.OverrideTagsHistory);
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.OverrideTags)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedOverrideTag)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Grid.SetRow(listBox, 1);
        grid.Children.Add(listBox);

        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteItemCommand);
        var panelDelete = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { buttonDelete },
        };
        Grid.SetRow(panelDelete, 2);
        grid.Children.Add(panelDelete);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        Grid.SetRow(panelButtons, 3);
        grid.Children.Add(panelButtons);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }
}

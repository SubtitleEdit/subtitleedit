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
                new RowDefinition { Height = new GridLength(300) },
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

        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.OverrideTags)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedOverrideTag)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        listBox.DoubleTapped += (s, e) => vm.OkCommand.Execute(null);
        grid.Add(listBox, 0);

        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteItemCommand);
        var panelDelete = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { buttonDelete },
        };
        grid.Add(panelDelete, 1);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }
}

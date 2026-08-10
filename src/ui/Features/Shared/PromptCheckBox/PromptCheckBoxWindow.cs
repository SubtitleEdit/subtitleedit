using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.PromptCheckBox;

public class PromptCheckBoxWindow : Window
{
    public PromptCheckBoxWindow(PromptCheckBoxViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 300;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var infoText = new TextBlock
        {
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 460,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InfoText)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.HasInfoText)),
        };

        var checkBox = new CheckBox
        {
            Margin = new Thickness(0, 0, 10, 0),
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsChecked)) { Mode = BindingMode.TwoWay },
            [!ContentControl.ContentProperty] = new Binding(nameof(vm.CheckBoxText)),
        };
        AutomationProperties.SetName(checkBox, vm.CheckBoxText);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            RowSpacing = 10,
        };

        grid.Add(infoText, 0);
        grid.Add(checkBox, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { checkBox.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

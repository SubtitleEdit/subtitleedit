using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.SpellCheck.EditWholeText;

public class EditWholeTextWindow : Window
{
    public EditWholeTextWindow(EditWholeTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Spell check - Edit whole text";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelLineInfo = new Label
        {            
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 2),
            [!ContentProperty] = new Binding(nameof(vm.LineInfo)),
        };  

        var textBoxWholeText = new TextBox
        {
            AcceptsReturn = true,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 10, 0, 2),
            [!TextBox.TextProperty] = new Binding(nameof(vm.WholeText)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            Width = 400,
            Height = 100,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelLineInfo, 0, 0);
        grid.Add(textBoxWholeText, 1, 0);
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        Activated += delegate { textBoxWholeText.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

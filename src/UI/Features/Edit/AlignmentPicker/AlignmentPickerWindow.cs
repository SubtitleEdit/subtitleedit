using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.AlignmentPicker;

public class AlignmentPickerWindow: Window
{
    public AlignmentPickerWindow(AlignmentPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Choose alignment";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var buttonAn1 = UiUtil.MakeButton("Top-left", vm.An1Command).WithMinWidth(100);
        var buttonAn2 = UiUtil.MakeButton("Top-center", vm.An2Command).WithMinWidth(100);
        var buttonAn3 = UiUtil.MakeButton("Top-right", vm.An3Command).WithMinWidth(100);
        var buttonAn4 = UiUtil.MakeButton("Middle-left", vm.An4Command).WithMinWidth(100);
        var buttonAn5 = UiUtil.MakeButton("Middle-center", vm.An5Command).WithMinWidth(100);
        var buttonAn6 = UiUtil.MakeButton("Middle-right", vm.An6Command).WithMinWidth(100);
        var buttonAn7 = UiUtil.MakeButton("Bottom-left", vm.An7Command).WithMinWidth(100);
        var buttonAn8 = UiUtil.MakeButton("Bottom-center", vm.An8Command).WithMinWidth(100);
        var buttonAn9 = UiUtil.MakeButton("Bottom-right", vm.An9Command).WithMinWidth(100);

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
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        
        grid.Add(buttonAn1, 0, 0);
        grid.Add(buttonAn2, 0, 1);
        grid.Add(buttonAn3, 0, 2);
        grid.Add(buttonAn4, 1, 0);
        grid.Add(buttonAn5, 1, 1);
        grid.Add(buttonAn6, 1, 2);
        grid.Add(buttonAn7, 2, 0);
        grid.Add(buttonAn8, 2, 1);
        grid.Add(buttonAn9, 2, 2);
       
        Content = grid;
        KeyDown += vm.KeyDown;
    }
}
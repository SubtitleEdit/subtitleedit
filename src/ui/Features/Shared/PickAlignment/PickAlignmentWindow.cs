using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickAlignment;

public class PickAlignmentWindow : Window
{
    public PickAlignmentWindow(PickAlignmentViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.PickAlignmentTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var w = 150;
        var h = 100;
        var buttonAn7 = UiUtil.MakeButton(Se.Language.General.TopLeft, vm.SetAlignmentCommand).WithParameter("an7").WithMinWidth(w).WithMinHeight(h);
        var buttonAn8 = UiUtil.MakeButton(Se.Language.General.TopCenter, vm.SetAlignmentCommand).WithParameter("an8").WithMinWidth(w).WithMinHeight(h);
        var buttonAn9 = UiUtil.MakeButton(Se.Language.General.TopRight, vm.SetAlignmentCommand).WithParameter("an9").WithMinWidth(w).WithMinHeight(h);
        var buttonAn4 = UiUtil.MakeButton(Se.Language.General.MiddleLeft, vm.SetAlignmentCommand).WithParameter("an4").WithMinWidth(w).WithMinHeight(h);
        var buttonAn5 = UiUtil.MakeButton(Se.Language.General.MiddleCenter, vm.SetAlignmentCommand).WithParameter("an5").WithMinWidth(w).WithMinHeight(h);
        var buttonAn6 = UiUtil.MakeButton(Se.Language.General.MiddleRight, vm.SetAlignmentCommand).WithParameter("an6").WithMinWidth(w).WithMinHeight(h);
        var buttonAn1 = UiUtil.MakeButton(Se.Language.General.BottomLeft, vm.SetAlignmentCommand).WithParameter("an1").WithMinWidth(w).WithMinHeight(h);
        var buttonAn2 = UiUtil.MakeButton(Se.Language.General.BottomCenter, vm.SetAlignmentCommand).WithParameter("an2").WithMinWidth(w).WithMinHeight(h);
        var buttonAn3 = UiUtil.MakeButton(Se.Language.General.BottomRight, vm.SetAlignmentCommand).WithParameter("an3").WithMinWidth(w).WithMinHeight(h);

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

        grid.Add(buttonAn7, 0);
        grid.Add(buttonAn8, 0,1);
        grid.Add(buttonAn9, 0,2);
        grid.Add(buttonAn4, 1);
        grid.Add(buttonAn5, 1,1);
        grid.Add(buttonAn6, 1,2);
        grid.Add(buttonAn1, 2);
        grid.Add(buttonAn2, 2,1);
        grid.Add(buttonAn3, 2,2);

        Content = grid;

        Activated += delegate { buttonAn2.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

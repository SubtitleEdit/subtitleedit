using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

public class InterjectionsWindow : Window
{
    private readonly InterjectionsViewModel _vm;
    
    public InterjectionsWindow(InterjectionsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Interjections";
        CanResize = false;
        Width = 700;
        Height = 600;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelInterjections = new Label
        {
            Content = "Interjections",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var labelSkipIfStartWith = new Label
        {
            Content = "Skip if start with",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var textBoxInterjections = new TextBox
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
        }.BindText(vm, nameof(vm.InterjectionsText));

        var textBoxSkipList = new TextBox
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
        }.BindText(vm, nameof(vm.InterjectionsSkipStartText));

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        grid.Add(labelInterjections, 0, 0);
        grid.Add(labelSkipIfStartWith, 0, 1);
        grid.Add(textBoxInterjections, 1, 0);
        grid.Add(textBoxSkipList, 1, 1);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

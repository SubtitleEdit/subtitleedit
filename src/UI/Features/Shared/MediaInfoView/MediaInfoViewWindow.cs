using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

public class MediaInfoViewWindow : Window
{
    public MediaInfoViewWindow(MediaInfoViewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.MediaInformation;
        Width = 900;
        Height = 500;
        MinWidth = 500;
        MinHeight = 400;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var contentBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };
        vm.TextBoxContainer = contentBorder;

        var buttonOpenContainingFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenContainingFolderCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOpenContainingFolder, buttonOk);

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(contentBorder, 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);

        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

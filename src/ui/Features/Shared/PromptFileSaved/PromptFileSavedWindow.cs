using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;

public class PromptFileSavedWindow : Window
{
    public PromptFileSavedWindow(PromptFileSavedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelText = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.Text));
        labelText.Margin = new Avalonia.Thickness(15);

        var buttonOpenContainingFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.ShowInFolderCommand)
            .WithBindIsVisible(nameof(vm.IsShowInFolderVisible));
        var buttonOpenFile = UiUtil.MakeButton(Se.Language.General.OpenFile, vm.ShowFileCommand)
            .WithBindIsVisible(nameof(vm.IsShowFileVisible));
        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOpenContainingFolder, buttonOpenFile, buttonDone);

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
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelText, 0);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}

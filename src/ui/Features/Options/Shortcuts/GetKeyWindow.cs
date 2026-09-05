using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class GetKeyWindow : Window
{
    private readonly GetKeyViewModel _vm;
    
    public GetKeyWindow(GetKeyViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 400;
        CanResize = false;
        Title = Se.Language.Options.Shortcuts.DetectKey;

        _vm = vm;
        vm.Window = this;

        // The capture window must see every chord, including Alt+Space (#14536).
        UiUtil.SetWindowSystemMenuOverride(this, () => true);
        DataContext = vm;

        var labelShortcutName = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ShortCutName));
        labelShortcutName.HorizontalAlignment = HorizontalAlignment.Center;
        labelShortcutName.Margin = new Thickness(5, 10, 5, 10);

        var labelKey = UiUtil.MakeLabel(Se.Language.Options.Shortcuts.PressAKey)
            .WithBindText(vm, nameof(vm.InfoText))
            .WithBold();
        labelKey.HorizontalAlignment = HorizontalAlignment.Center;
        labelKey.Margin = new Thickness(5, 10, 5, 10);
        
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelShortcutName, 0);
        grid.Add(labelKey, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;
        
        Opened += (_, _) =>
        {
            Activate();
            Dispatcher.UIThread.Post(() => buttonOk.Focus(), DispatcherPriority.Input);
        };

        // Capture in the tunnel phase, before the focused OK button sees the key: Button's own
        // key handling clicks on any Return chord (no modifier check) and on Space, and marks
        // the event handled, so a bubbling window handler never saw those keys - Alt+Return,
        // Ctrl+Space and friends silently closed the dialog with nothing captured (#14401).
        AddHandler(KeyDownEvent, (_, e) => _vm.OnKeyDown(e), RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, (_, e) => _vm.KeyUp(e), RoutingStrategies.Tunnel);
    }
}

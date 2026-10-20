using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
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

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var labelShortcutName = new TextBlock
        {
            FontSize = 12.5,
            Foreground = EditorChrome.DimTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 520,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ShortCutName)),
        };

        // The captured chord in a keycap-like field.
        var labelKey = new TextBlock
        {
            Text = Se.Language.Options.Shortcuts.PressAKey,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InfoText)),
        };
        var keyField = new Border
        {
            Child = labelKey,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1, 1, 1, 2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18, 14),
            MinHeight = 52,
            MinWidth = 340,
        };

        var keyIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-keyboard",
            FontSize = 22,
            Foreground = EditorChrome.FaintTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        var panel = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(20, 18, 20, 20),
            Children = { keyIcon, labelShortcutName, keyField },
        };

        // The buttons never take the keyboard focus: every key press belongs to the capture.
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        buttonOk.Focusable = false;
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        buttonCancel.Focusable = false;
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(EditorChrome.MakePanel(panel), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Opened += (_, _) => Activate();

        // Capture in the tunnel phase, before the (default) OK button sees the key: Button's own
        // key handling clicks on any Return chord (no modifier check) and on Space, and marks
        // the event handled, so a bubbling window handler never saw those keys - Alt+Return,
        // Ctrl+Space and friends silently closed the dialog with nothing captured (#14401).
        AddHandler(KeyDownEvent, (_, e) => _vm.OnKeyDown(e), RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, (_, e) => _vm.KeyUp(e), RoutingStrategies.Tunnel);
    }
}

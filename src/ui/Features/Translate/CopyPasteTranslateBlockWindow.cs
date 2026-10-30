using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// One block of translate via copy-paste in the editor look (see <see cref="EditorChrome"/>): what to
/// do, the accent "get from clipboard" button, Re-copy and Cancel in the button bar.
/// </summary>
public class CopyPasteTranslateBlockWindow : Window
{
    public CopyPasteTranslateBlockWindow(CopyPasteTranslateBlockViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(CopyPasteTranslateBlockViewModel.WindowTitle)) { Mode = BindingMode.OneWay });
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Translate.BlockCopyInfo,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var buttonGetFromClipboard = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-paste", FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock { Text = Se.Language.Translate.BlockCopyGetFromClipboard, VerticalAlignment = VerticalAlignment.Center },
                },
            },
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(22, 12),
            Margin = new Thickness(0, 6, 0, 2),
            CornerRadius = new CornerRadius(8),
            FontWeight = FontWeight.SemiBold,
            FontSize = UiUtil.ScaledFontSize(15),
            Command = vm.CopyFromClipboardCommand,
        };
        buttonGetFromClipboard.Classes.Add("primary");
        AutomationProperties.SetName(buttonGetFromClipboard, Se.Language.Translate.BlockCopyGetFromClipboard);

        var body = new StackPanel
        {
            Spacing = 14,
            Margin = new Thickness(20, 18),
            Children = { labelInfo, buttonGetFromClipboard },
        };

        var buttonReCopy = UiUtil.MakeButton(Se.Language.Translate.ReCopyTextToClipboard, vm.CopyToClipboardCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonReCopy, buttonCancel);

        Content = EditorChrome.MakeRoot(body, buttonBar);

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
        Loaded += vm.Loaded;
    }
}

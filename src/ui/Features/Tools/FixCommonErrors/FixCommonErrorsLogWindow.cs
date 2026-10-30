using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsLogWindow : Window
{
    // Follows the app theme, also when it changes while the window is open
    private static readonly IBrush ImportantBrush = EditorChrome.Themed(Color.FromRgb(0xff, 0x8a, 0x80), Color.FromRgb(0xc4, 0x28, 0x28));

    public FixCommonErrorsLogWindow(FixCommonErrorsLogViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FixCommonErrors.Log;
        CanResize = true;
        Width = 800;
        Height = 500;
        MinWidth = 400;
        MinHeight = 250;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        // Icon-only; the view model swaps the icon to a check mark for a moment after copying.
        var buttonCopy = new Button
        {
            Command = vm.CopyCommand,
            Width = 26,
            Height = 26,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        buttonCopy.Classes.Add("tool");
        Attached.SetIcon(buttonCopy, IconNames.Copy);
        AutomationProperties.SetName(buttonCopy, Se.Language.General.CopyToClipboard);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonCopy, Se.Language.General.CopyToClipboard);
        }

        vm.CopyButton = buttonCopy;

        var labelImportant = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = ImportantBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(12, 0),
        };
        labelImportant.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ImportantMessagesText)));
        labelImportant.Bind(IsVisibleProperty, new Binding(nameof(vm.ImportantMessagesIsVisible)));

        var panelHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(buttonCopy, Dock.Right);
        panelHeader.Children.Add(buttonCopy);
        var title = EditorChrome.MakeSectionHeader(Se.Language.Tools.FixCommonErrors.Log);
        DockPanel.SetDock(title, Dock.Left);
        panelHeader.Children.Add(title);
        panelHeader.Children.Add(labelImportant);

        var textBoxLog = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            [!TextBox.TextProperty] = new Binding(nameof(vm.LogText)),
        };
        textBoxLog.Classes.Add("text");
        AutomationProperties.SetName(textBoxLog, Se.Language.Tools.FixCommonErrors.Log);

        var buttonDone = UiUtil.MakeButtonDone(vm.CloseCommand);
        buttonDone.Classes.Add("primary");
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonDone);

        var body = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Thickness(14, 12, 14, 14),
        };
        body.Add(panelHeader, 0);
        body.Add(textBoxLog, 1);

        Content = EditorChrome.MakeRoot(body, panelButtons);

        UiUtil.FocusOnFirstActivation(this, buttonDone);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}

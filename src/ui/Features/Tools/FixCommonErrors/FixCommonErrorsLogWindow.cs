using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsLogWindow : Window
{
    public FixCommonErrorsLogWindow(FixCommonErrorsLogViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FixCommonErrors.Log;
        CanResize = true;
        Width = 800;
        Height = 500;
        MinWidth = 400;
        MinHeight = 250;
        vm.Window = this;
        DataContext = vm;

        var buttonCopy = UiUtil.MakeButton(vm.CopyCommand, IconNames.Copy, Se.Language.General.CopyToClipboard);
        vm.CopyButton = buttonCopy;

        var labelImportant = UiUtil.MakeTextBlock(string.Empty);
        labelImportant.Foreground = new SolidColorBrush(UiTheme.IsDarkThemeEnabled()
            ? Color.FromRgb(0xff, 0x8a, 0x80)
            : Color.FromRgb(0xc4, 0x28, 0x28));
        labelImportant.VerticalAlignment = VerticalAlignment.Center;
        labelImportant.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ImportantMessagesText)));
        labelImportant.Bind(IsVisibleProperty, new Binding(nameof(vm.ImportantMessagesIsVisible)));

        var panelHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        };
        panelHeader.Add(labelImportant, 0, 0);
        panelHeader.Add(buttonCopy, 0, 1);

        var textBoxLog = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            [!TextBox.TextProperty] = new Binding(nameof(vm.LogText)),
        };
        AutomationProperties.SetName(textBoxLog, Se.Language.Tools.FixCommonErrors.Log);

        var buttonDone = UiUtil.MakeButtonDone(vm.CloseCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonDone);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(panelHeader, 0);
        grid.Add(textBoxLog, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonDone);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}

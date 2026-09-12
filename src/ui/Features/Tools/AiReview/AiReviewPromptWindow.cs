using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public class AiReviewPromptWindow : Window
{
    public AiReviewPromptWindow(AiReviewPromptViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.AiReview.EditPromptTitle;
        Width = 640;
        MinWidth = 480;
        Height = 520;
        MinHeight = 360;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.AiReview;

        var labelInfo = UiUtil.MakeTextBlock(l.PromptInfo);
        labelInfo.TextWrapping = TextWrapping.Wrap;
        labelInfo.Opacity = 0.75;

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 140,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.PromptText)) { Mode = BindingMode.TwoWay },
        };
        Avalonia.Automation.AutomationProperties.SetName(textBox, l.EditPromptTitle);

        var labelProtocol = UiUtil.MakeTextBlock(l.ProtocolInfo);
        labelProtocol.TextWrapping = TextWrapping.Wrap;
        labelProtocol.Opacity = 0.6;
        labelProtocol.FontSize = 12;
        var borderProtocol = new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = labelProtocol,
        };

        var buttonReset = UiUtil.MakeButton(l.ResetToDefault, vm.ResetToDefaultCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
        };
        buttonBar.Add(buttonReset, 0, 0);
        buttonBar.Add(UiUtil.MakeButtonBar(buttonOk, buttonCancel), 0, 2);

        // The prompt editor takes the star row so a long prompt scrolls inside the
        // text box instead of growing the window past the screen edge and pushing
        // the OK/Cancel buttons out of reach.
        var panel = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            RowSpacing = 10,
        };
        panel.Add(labelInfo, 0, 0);
        panel.Add(textBox, 1, 0);
        panel.Add(borderProtocol, 2, 0);
        panel.Add(buttonBar, 3, 0);

        Content = panel;

        Loaded += delegate
        {
            textBox.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

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
        SizeToContent = SizeToContent.Height;
        CanResize = false;
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

        var panel = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 10,
            Children =
            {
                labelInfo,
                textBox,
                borderProtocol,
                buttonBar,
            },
        };

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

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
        Height = 700;
        MinHeight = 520;
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

        var labelTemplate = UiUtil.MakeTextBlock(l.Template);
        labelTemplate.VerticalAlignment = VerticalAlignment.Center;
        var comboTemplate = new ComboBox
        {
            ItemsSource = vm.Templates,
            PlaceholderText = l.TemplatePlaceholder,
            MinWidth = 280,
            VerticalAlignment = VerticalAlignment.Center,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedTemplate)) { Mode = BindingMode.TwoWay },
        };
        Avalonia.Automation.AutomationProperties.SetName(comboTemplate, l.Template);
        var panelTemplate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { labelTemplate, comboTemplate },
        };

        var labelContext = UiUtil.MakeTextBlock(l.ReferenceContext);
        labelContext.FontWeight = FontWeight.SemiBold;
        labelContext.VerticalAlignment = VerticalAlignment.Center;
        var buttonGenerate = UiUtil.MakeButton(l.GenerateContext, vm.GenerateContextCommand);
        buttonGenerate.Bind(Button.ContentProperty, new Binding(nameof(vm.GenerateButtonText)));
        buttonGenerate.Bind(IsVisibleProperty, new Binding(nameof(vm.CanGenerate)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonGenerate, l.GenerateContextHint);
        }

        var labelGenerateStatus = UiUtil.MakeTextBlock(string.Empty);
        labelGenerateStatus.Opacity = 0.75;
        labelGenerateStatus.VerticalAlignment = VerticalAlignment.Center;
        labelGenerateStatus.Bind(TextBlock.TextProperty, new Binding(nameof(vm.GenerateStatus)));
        var panelContextHeader = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { labelContext, buttonGenerate, labelGenerateStatus },
        };

        var labelContextInfo = UiUtil.MakeTextBlock(l.ReferenceContextInfo);
        labelContextInfo.TextWrapping = TextWrapping.Wrap;
        labelContextInfo.Opacity = 0.75;

        var textBoxContext = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 90,
            PlaceholderText = l.ReferenceContextPlaceholder,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ContextText)) { Mode = BindingMode.TwoWay },
            [!TextBox.IsReadOnlyProperty] = new Binding(nameof(vm.IsGenerating)),
        };
        Avalonia.Automation.AutomationProperties.SetName(textBoxContext, l.ReferenceContext);

        var labelProtocol = UiUtil.MakeTextBlock(l.ProtocolInfo);
        labelProtocol.TextWrapping = TextWrapping.Wrap;
        labelProtocol.Opacity = 0.6;
        labelProtocol.FontSize = UiUtil.ScaledFontSize(12);
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
            RowDefinitions = new RowDefinitions("Auto,Auto,3*,Auto,Auto,2*,Auto,Auto"),
            RowSpacing = 10,
        };
        panel.Add(labelInfo, 0, 0);
        panel.Add(panelTemplate, 1, 0);
        panel.Add(textBox, 2, 0);
        panel.Add(panelContextHeader, 3, 0);
        panel.Add(labelContextInfo, 4, 0);
        panel.Add(textBoxContext, 5, 0);
        panel.Add(borderProtocol, 6, 0);
        panel.Add(buttonBar, 7, 0);

        Content = panel;

        Loaded += delegate
        {
            textBox.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate
        {
            vm.OnClosing();
            UiUtil.SaveWindowPosition(this);
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

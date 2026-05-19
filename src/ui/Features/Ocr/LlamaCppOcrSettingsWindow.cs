using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class LlamaCppOcrSettingsWindow : Window
{
    private const int LabelWidth = 130;

    public LlamaCppOcrSettingsWindow(LlamaCppOcrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.LlamaCppOcrSettingsTitle;
        Width = 680;
        Height = 460;
        MinWidth = 540;
        MinHeight = 400;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var textBoxUrl = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        textBoxUrl.Bind(TextBox.TextProperty, new Binding(nameof(vm.Url))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var numericTimeout = UiUtil.MakeNumericUpDownInt(1, 120, 5, 140, vm, nameof(vm.TimeoutMinutes));

        var textBoxPrompt = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = false,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 90,
            DataContext = vm,
        };
        textBoxPrompt.Bind(TextBox.TextProperty, new Binding(nameof(vm.Prompt))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var hintPrompt = UiUtil.MakeTextBlock(Se.Language.Ocr.LlamaCppOcrPromptHint);
        hintPrompt.Opacity = 0.7;
        hintPrompt.FontSize = 12;
        hintPrompt.Margin = new Thickness(0, 2, 0, 0);

        var promptLabel = MakeLabel(Se.Language.General.OpenAiCompatibleSttPrompt);
        promptLabel.VerticalAlignment = VerticalAlignment.Top;
        promptLabel.Margin = new Thickness(0, 6, 0, 0);

        var detailsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 12,
            RowSpacing = 12,
        };

        detailsGrid.Add(MakeLabel("Engine"), 0, 0);
        detailsGrid.Add(MakeStatusPanel(nameof(vm.EngineBrush), nameof(vm.EngineLabel)), 0, 1);

        detailsGrid.Add(MakeLabel(Se.Language.General.Url), 1, 0);
        detailsGrid.Add(textBoxUrl, 1, 1);

        detailsGrid.Add(MakeLabel(Se.Language.Ocr.LlamaCppOcrTimeoutMinutes), 2, 0);
        detailsGrid.Add(numericTimeout, 2, 1);

        detailsGrid.Add(promptLabel, 3, 0);
        detailsGrid.Add(textBoxPrompt, 3, 1);

        detailsGrid.Add(hintPrompt, 4, 1);

        var detailsBorder = new Border
        {
            Child = detailsGrid,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 14,
            Margin = UiUtil.MakeWindowMargin(),
        };
        rootGrid.Add(BuildHeader(), 0, 0);
        rootGrid.Add(detailsBorder, 1, 0);
        rootGrid.Add(buttonBar, 2, 0);

        Content = rootGrid;

        Activated += delegate { textBoxUrl.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel BuildHeader()
    {
        var title = new TextBlock
        {
            Text = "llama.cpp OCR",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };
        var subtitle = new TextBlock
        {
            Text = "Local llama.cpp server (multimodal model) used for OCR.",
            FontSize = 12,
            Opacity = 0.75,
            Margin = new Thickness(0, 2, 0, 0),
        };
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children = { title, subtitle },
        };
    }

    private static StackPanel MakeStatusPanel(string brushBindingPath, string labelBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 10,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            [!Ellipse.FillProperty] = new Binding(brushBindingPath),
        };
        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { dot, text },
        };
    }

    private static TextBlock MakeLabel(string text) => new()
    {
        Text = text,
        Opacity = 0.7,
        VerticalAlignment = VerticalAlignment.Center,
    };
}

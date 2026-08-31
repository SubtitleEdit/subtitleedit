using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Shared.FormatLimitWarning;

public class FormatLimitWarningWindow : Window
{
    public FormatLimitWarningWindow(FormatLimitWarningViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Main.FormatLimitWarningTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeIcon(IconNames.Alert, 36, 1.0),
                new TextBlock
                {
                    FontSize = 15,
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 420,
                    [!TextBlock.TextProperty] = new Binding(nameof(vm.SummaryText)),
                },
            },
        };

        var rows = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                MakeRow(IconNames.FormatFont, nameof(vm.MaxCharactersText), nameof(vm.IsMaxCharactersVisible)),
                MakeRow(IconNames.FormatFont, nameof(vm.MaxLinesText), nameof(vm.IsMaxLinesVisible)),
                MakeRow(IconNames.ViewList, nameof(vm.LinesText), null),
            },
        };

        var card = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromArgb(18, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 14, 24, 14),
            Child = rows,
        };

        var explanation = new TextBlock
        {
            Text = Se.Language.Main.FormatLimitWarningTextWillBeRewrapped,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 460,
            Opacity = 0.85,
        };

        var checkBoxDoNotShowAgain = new CheckBox
        {
            Content = new TextBlock { Text = Se.Language.Main.FormatLimitWarningDoNotShowAgain, FontSize = 13 },
            Opacity = 0.8,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoNotShowAgain)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxSmall = new LayoutTransformControl
        {
            LayoutTransform = new ScaleTransform(0.85, 0.85),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = checkBoxDoNotShowAgain,
        };

        var buttonSaveAnyway = UiUtil.MakeButton(Se.Language.Main.FormatLimitWarningSaveAnyway, vm.SaveAnywayCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonSaveAnyway, buttonCancel);

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Margin = UiUtil.MakeWindowMargin(),
            MinWidth = 360,
            Children = { header, card, explanation, checkBoxSmall, buttonPanel },
        };

        Content = panel;

        // Cancel is the safe default: Enter must not accidentally commit a lossy save.
        UiUtil.FocusOnFirstActivation(this, buttonCancel);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Control MakeIcon(string iconName, double size, double opacity)
    {
        var icon = new ContentControl
        {
            Width = size,
            Height = size,
            Opacity = opacity,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Attached.SetIcon(icon, iconName);
        return icon;
    }

    private static Control MakeRow(string iconName, string textPropertyPath, string? visiblePropertyPath)
    {
        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400,
        };
        text.Bind(TextBlock.TextProperty, new Binding(textPropertyPath));

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { MakeIcon(iconName, 20, 0.85), text },
        };

        if (visiblePropertyPath != null)
        {
            row.Bind(Visual.IsVisibleProperty, new Binding(visiblePropertyPath));
        }

        return row;
    }
}

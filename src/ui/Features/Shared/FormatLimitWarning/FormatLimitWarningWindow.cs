using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Shared.FormatLimitWarning;

/// <summary>
/// Warns that the format cannot hold the subtitle as it is (too many characters/lines), in the
/// editor look (see <see cref="EditorChrome"/>). Cancel and Save anyway in the button bar.
/// </summary>
public class FormatLimitWarningWindow : Window
{

    public FormatLimitWarningWindow(FormatLimitWarningViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Main.FormatLimitWarningTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeWarningIcon(),
                new TextBlock
                {
                    FontSize = UiUtil.ScaledFontSize(15),
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
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 14, 24, 14),
            Child = rows,
        };

        var explanation = new TextBlock
        {
            Text = Se.Language.Main.FormatLimitWarningTextWillBeRewrapped,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 460,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var toggleDoNotShowAgain = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.DoNotShowAgain)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggleDoNotShowAgain, Se.Language.Main.FormatLimitWarningDoNotShowAgain);
        var doNotShowAgainRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                toggleDoNotShowAgain,
                new TextBlock
                {
                    Text = Se.Language.Main.FormatLimitWarningDoNotShowAgain,
                    FontSize = EditorChrome.FieldFontSize,
                    Foreground = EditorChrome.DimTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

        var buttonSaveAnyway = UiUtil.MakeButton(Se.Language.Main.FormatLimitWarningSaveAnyway, vm.SaveAnywayCommand);
        buttonSaveAnyway.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Margin = new Thickness(18, 16, 18, 18),
            MinWidth = 360,
            Children = { header, card, explanation },
        };

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeDockedButtonBar(doNotShowAgainRow, buttonCancel, buttonSaveAnyway));

        // Cancel is the safe default: Enter must not accidentally commit a lossy save.
        UiUtil.FocusOnFirstActivation(this, buttonCancel);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// The warning sign in the warning orange, so it reads as a warning in both themes.
    /// </summary>
    private static Control MakeWarningIcon()
    {
        return new Icon
        {
            Value = IconNames.Alert,
            FontSize = 32,
            Foreground = EditorChrome.WarningTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 2, 0),
        };
    }

    private static Control MakeIcon(string iconName, double size, double opacity)
    {
        var icon = new ContentControl
        {
            Width = size,
            Height = size,
            Opacity = opacity,
            Foreground = EditorChrome.IconBrush,
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
            Children = { MakeIcon(iconName, 20, 1.0), text },
        };

        if (visiblePropertyPath != null)
        {
            row.Bind(Visual.IsVisibleProperty, new Binding(visiblePropertyPath));
        }

        return row;
    }
}

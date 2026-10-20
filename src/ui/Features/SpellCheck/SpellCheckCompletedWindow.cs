using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

/// <summary>
/// The spell check summary, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class SpellCheckCompletedWindow : Window
{
    public SpellCheckCompletedWindow(SpellCheckCompletedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.SpellCheck;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeHeaderIcon(32),
                new TextBlock
                {
                    Text = Se.Language.SpellCheck.SpellCheckCompleted,
                    FontSize = UiUtil.ScaledFontSize(16),
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

        var rows = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 9,
            Children =
            {
                MakeCountRow(IconNames.Pencil, nameof(vm.ChangedWordsText), null),
                MakeCountRow(IconNames.SkipNext, nameof(vm.SkippedWordsText), null),
                MakeCountRow(IconNames.CheckBold, nameof(vm.CorrectWordsText), nameof(vm.IsCorrectWordsVisible)),
                MakeCountRow(IconNames.Account, nameof(vm.NamesText), nameof(vm.IsNamesVisible)),
                MakeCountRow(IconNames.Plus, nameof(vm.AddedWordsText), nameof(vm.IsAddedWordsVisible)),
            },
        };

        // A card on the field color around the counts so it reads as a grouped result rather
        // than loose text.
        var card = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 14, 24, 14),
            Child = rows,
        };

        var body = new StackPanel
        {
            Spacing = 14,
            MinWidth = 320,
            Children = { header, card },
        };

        // A small, secondary option, left in the button bar
        var toggleDoNotShowAgain = EditorChrome.MakeToggleRow(nameof(vm.DoNotShowAgain), Se.Language.SpellCheck.DoNotShowThisAgain);
        toggleDoNotShowAgain.VerticalAlignment = VerticalAlignment.Center;
        toggleDoNotShowAgain.Margin = new Thickness(0, 0, 16, 0);

        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand);
        buttonDone.Classes.Add("primary");
        buttonDone.Margin = new Thickness(0);

        Content = EditorChrome.MakeRoot(new Border { Child = body, Padding = new Thickness(18, 16, 18, 18) }, EditorChrome.MakeDockedButtonBar(toggleDoNotShowAgain, buttonDone));

        UiUtil.FocusOnFirstActivation(this, buttonDone); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    // Use the same SpellCheck icon as the main toolbar; fall back to a generic info icon if the
    // themed image can't be loaded for any reason.
    private static Control MakeHeaderIcon(double size)
    {
        try
        {
            var image = InitToolbar.MakeImage("SpellCheck");
            image.Width = size;
            image.Height = size;
            image.VerticalAlignment = VerticalAlignment.Center;
            return image;
        }
        catch
        {
            return MakeIcon(IconNames.Information, size, 1.0);
        }
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

    private static Control MakeCountRow(string iconName, string textPropertyPath, string? visiblePropertyPath)
    {
        var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontSize = UiUtil.ScaledFontSize(14) };
        text.Bind(TextBlock.TextProperty, new Binding(textPropertyPath));

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { MakeIcon(iconName, 18, 0.85), text },
        };

        if (visiblePropertyPath != null)
        {
            row.Bind(Visual.IsVisibleProperty, new Binding(visiblePropertyPath));
        }

        return row;
    }
}

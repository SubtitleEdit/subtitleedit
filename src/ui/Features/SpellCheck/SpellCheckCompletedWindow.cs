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

public class SpellCheckCompletedWindow : Window
{
    public SpellCheckCompletedWindow(SpellCheckCompletedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.SpellCheck;
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
                MakeHeaderIcon(36),
                new TextBlock
                {
                    Text = Se.Language.SpellCheck.SpellCheckCompleted,
                    FontSize = 17,
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

        // A subtle card around the counts so it reads as a grouped result rather than loose text.
        var card = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromArgb(18, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(48, 128, 128, 128)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 14, 24, 14),
            Child = rows,
        };

        var checkBoxDoNotShowAgain = new CheckBox
        {
            Content = new TextBlock { Text = Se.Language.SpellCheck.DoNotShowThisAgain, FontSize = 13 },
            Opacity = 0.8,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoNotShowAgain)) { Mode = BindingMode.TwoWay },
        };

        // Shrink the whole control (box + label) so it reads as a small, secondary option.
        var checkBoxSmall = new LayoutTransformControl
        {
            LayoutTransform = new ScaleTransform(0.85, 0.85),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = checkBoxDoNotShowAgain,
        };

        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonDone);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 14,
            MinWidth = 320,
        };

        grid.Add(header, 0);
        grid.Add(card, 1);
        grid.Add(checkBoxSmall, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
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
        var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontSize = 15 };
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

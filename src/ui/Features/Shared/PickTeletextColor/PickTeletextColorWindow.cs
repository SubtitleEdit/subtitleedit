using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextColor;

public class PickTeletextColorWindow : Window
{
    private const double SwatchWidth = 48;
    private const double SwatchHeight = 30;

    public PickTeletextColorWindow(PickTeletextColorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.TeletextColor;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var tilePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
        };

        tilePanel.Children.Add(MakeTile(
            MakeNoColorSwatch(vm.CurrentColor == null),
            Se.Language.General.NoColor,
            vm.PickNoColorCommand,
            null));

        foreach (var item in vm.TeletextColors)
        {
            tilePanel.Children.Add(MakeTile(
                MakeColorSwatch(item.Color, vm.CurrentColor == item.Color),
                item.DisplayName,
                vm.PickColorCommand,
                item));
        }

        var panel = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 10,
            Children =
            {
                tilePanel,
            },
        };

        if (vm.IsLevel25)
        {
            // A full teletext stream also reaches the CLUT 1 half intensity colors, and any
            // other color through a redefinable colour map entry (Level 2.5).
            var halfPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
            };

            foreach (var item in vm.HalfIntensityColors)
            {
                halfPanel.Children.Add(MakeTile(
                    MakeColorSwatch(item.Color, vm.CurrentColor == item.Color),
                    item.DisplayName,
                    vm.PickColorCommand,
                    item));
            }

            panel.Children.Add(halfPanel);
        }

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        if (vm.IsLevel25)
        {
            var buttonCustom = UiUtil.MakeButton(Se.Language.General.ChooseColorDotDotDot, vm.PickCustomColorCommand);
            panel.Children.Add(UiUtil.MakeButtonBar(buttonCustom, buttonCancel));
        }
        else
        {
            panel.Children.Add(UiUtil.MakeButtonBar(buttonCancel));
        }

        Content = panel;

        UiUtil.FocusOnFirstActivation(this, buttonCancel);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Button MakeTile(Border swatch, string text, System.Windows.Input.ICommand command, object? commandParameter)
    {
        var label = new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        return new Button
        {
            Command = command,
            CommandParameter = commandParameter,
            Content = new StackPanel
            {
                Spacing = 4,
                Children = { swatch, label },
            },
            Padding = new Thickness(8, 6),
        };
    }

    private static Border MakeColorSwatch(Color color, bool isCurrent)
    {
        return new Border
        {
            Width = SwatchWidth,
            Height = SwatchHeight,
            Background = new SolidColorBrush(color),
            BorderBrush = isCurrent ? UiUtil.GetAccentBrush() : new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(isCurrent ? 3 : 1),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
    }

    private static Border MakeNoColorSwatch(bool isCurrent)
    {
        var swatch = MakeColorSwatch(Colors.White, isCurrent);
        swatch.Child = new Line
        {
            StartPoint = new Point(2, SwatchHeight - 6),
            EndPoint = new Point(SwatchWidth - 6, 2),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
        };
        return swatch;
    }
}

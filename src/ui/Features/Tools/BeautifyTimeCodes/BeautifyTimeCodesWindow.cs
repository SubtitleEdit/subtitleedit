using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public class BeautifyTimeCodesWindow : Window
{
    public BeautifyTimeCodesWindow(BeautifyTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BeautifyTimeCodes.Title;
        CanResize = true;
        Width = 1100;
        Height = 820;
        MinWidth = 900;
        MinHeight = 560;
        vm.Window = this;
        DataContext = vm;

        Content = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            Children =
            {
                BuildTopBar(vm),
                BuildVisualizerArea(vm),
                BuildChangeNavBar(vm),
                BuildButtonBar(vm),
            },
        };

        var c = (Grid)Content;
        Grid.SetRow(c.Children[0], 0);
        Grid.SetRow(c.Children[1], 1);
        Grid.SetRow(c.Children[2], 2);
        Grid.SetRow(c.Children[3], 3);

        Activated += delegate { /* leave focus on the visualizer for keyboard nav */ };
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, __) => vm.Dispose();
    }

    private static Control BuildTopBar(BeautifyTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.BeautifyTimeCodesProfile;

        var buttonEditProfile = UiUtil.MakeButton(l.Title, vm.EditProfileCommand);

        var stats = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Opacity = 0.8,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatsLine)) { Source = vm },
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
        };
        grid.Add(buttonEditProfile, 0, 0);
        grid.Add(stats, 0, 2);
        return grid;
    }

    private static Control BuildVisualizerArea(BeautifyTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.BeautifyTimeCodes;

        var (labelOriginal, borderOriginal, avOriginal) = BuildLabeledVisualizer(l.Original);
        var (labelBeautified, borderBeautified, avBeautified) = BuildLabeledVisualizer(l.Beautified);

        vm.AudioVisualizerOriginal = avOriginal;
        vm.AudioVisualizerBeautified = avBeautified;

        avOriginal.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == "StartPositionSeconds")
                avBeautified.StartPositionSeconds = avOriginal.StartPositionSeconds;
            else if (e.Property.Name == "ZoomFactor")
                avBeautified.ZoomFactor = avOriginal.ZoomFactor;
            else if (e.Property.Name == "VerticalZoomFactor")
                avBeautified.VerticalZoomFactor = avOriginal.VerticalZoomFactor;
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,5*,10,Auto,5*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        grid.Add(labelOriginal, 0, 0);
        grid.Add(borderOriginal, 1, 0);
        grid.Add(labelBeautified, 3, 0);
        grid.Add(borderBeautified, 4, 0);
        return grid;
    }

    private static (TextBlock label, Border border, Controls.AudioVisualizerControl.AudioVisualizer av) BuildLabeledVisualizer(string title)
    {
        var label = new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(5, 5, 5, 2),
        };

        var av = new Controls.AudioVisualizerControl.AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
            // Stronger tints so paragraphs stand out on the dark waveform
            ParagraphBackground = Color.FromArgb(140, 70, 110, 180),       // regular: blue
            ParagraphSelectedBackground = Color.FromArgb(210, 230, 160, 40), // current change: amber
        };

        var border = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Background = Brushes.Black,
            Child = av,
        };
        return (label, border, av);
    }

    private static Control BuildChangeNavBar(BeautifyTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.BeautifyTimeCodes;

        var buttonPrev = UiUtil.MakeButton(vm.PreviousChangeCommand, IconNames.ArrowUpThin, l.PreviousChange);
        buttonPrev.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanGoPrevious)) { Source = vm });

        var buttonNext = UiUtil.MakeButton(vm.NextChangeCommand, IconNames.ArrowDownThin, l.NextChange);
        buttonNext.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanGoNext)) { Source = vm });

        var labelPosition = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            MinWidth = 120,
            TextAlignment = TextAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangePositionLabel)) { Source = vm },
        };

        var detail = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontFamily = new FontFamily("Consolas, Menlo, monospace"),
            Margin = new Thickness(10, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangeDetail)) { Source = vm },
        };

        var inner = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonPrev, labelPosition, buttonNext, detail },
        };

        return new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 128, 128, 128)),
            BorderThickness = new Thickness(0, 1, 0, 1),
            Padding = new Thickness(6, 6, 6, 6),
            Child = inner,
        };
    }

    private static Control BuildButtonBar(BeautifyTimeCodesViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        return UiUtil.MakeButtonBar(buttonOk, buttonCancel);
    }
}

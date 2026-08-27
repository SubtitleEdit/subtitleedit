using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Main.AssistedMove;

public class AssistedMoveWindow : Window
{
    private readonly AssistedMoveViewModel _vm;

    public AssistedMoveWindow(AssistedMoveViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AssistedMove;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Width = 720;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelHeader = UiUtil.MakeLabel(Se.Language.General.AssistedMoveChooseMove);
        labelHeader.FontWeight = FontWeight.Bold;

        var originalText = new TextBlock
        {
            Text = vm.SubtitleInfo,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var panelCandidates = new StackPanel { Spacing = 8, Margin = new Thickness(0, 0, 0, 10) };
        foreach (var candidate in vm.Candidates)
        {
            panelCandidates.Children.Add(MakeCandidateCard(vm, candidate));
        }

        // The candidate list scrolls when there are more cards than fit on screen.
        MaxHeight = 850;
        var scrollCandidates = new ScrollViewer
        {
            Content = panelCandidates,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Focusable = true,
        };

        var grid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        grid.Add(labelHeader, 0);
        grid.Add(UiUtil.MakeBorderForControl(originalText), 1);
        grid.Add(scrollCandidates, 2);

        Content = grid;

        Activated += delegate { scrollCandidates.Focus(); }; // hack to make OnKeyDown work

        // Keep the whole card list reachable on small screens - the ScrollViewer takes over.
        Opened += (_, _) =>
        {
            var workingArea = Screens.ScreenFromWindow(this)?.WorkingArea;
            if (workingArea != null && RenderScaling > 0)
            {
                MaxHeight = Math.Min(MaxHeight, workingArea.Value.Height / RenderScaling * 0.92);
            }
        };
    }

    private static Button MakeCandidateCard(AssistedMoveViewModel vm, AssistedMoveCandidate candidate)
    {
        var labelNumber = new TextBlock
        {
            Text = candidate.Number.ToString(),
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 12, 0),
        };

        var labelTitle = new TextBlock
        {
            Text = candidate.Title,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var panelTexts = new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                labelTitle,
                MakePreview(candidate.FirstText, candidate.FirstInfo),
            },
        };

        if (!string.IsNullOrEmpty(candidate.SecondText))
        {
            panelTexts.Children.Add(MakePreview(candidate.SecondText, candidate.SecondInfo));
        }

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        grid.Add(labelNumber, 0, 0);
        grid.Add(panelTexts, 0, 1);

        return new Button
        {
            Content = grid,
            Command = vm.PickCommand,
            CommandParameter = candidate,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(8),
        }.WithGreenishActiveBackground();
    }

    private static Border MakePreview(string text, string info)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
        };

        var infoBlock = new TextBlock
        {
            Text = info,
            FontSize = 11,
            Opacity = 0.7,
            Margin = new Thickness(0, 2, 0, 0),
        };

        return new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 4),
            Child = new StackPanel { Children = { textBlock, infoBlock } },
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

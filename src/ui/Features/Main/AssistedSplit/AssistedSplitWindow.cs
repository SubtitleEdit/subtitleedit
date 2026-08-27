using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.AssistedSplit;

public class AssistedSplitWindow : Window
{
    private readonly AssistedSplitViewModel _vm;

    public AssistedSplitWindow(AssistedSplitViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AssistedSplit;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Width = 720;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelHeader = UiUtil.MakeLabel(Se.Language.General.AssistedSplitChooseSplitPoint);
        labelHeader.FontWeight = FontWeight.Bold;

        var originalText = new TextBlock
        {
            Text = vm.SubtitleInfo,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var panelCandidates = new StackPanel { Spacing = 8 };
        foreach (var candidate in vm.Candidates)
        {
            panelCandidates.Children.Add(MakeCandidateCard(vm, candidate));
        }

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonCancel);

        var panel = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 8,
            Children =
            {
                labelHeader,
                UiUtil.MakeBorderForControl(originalText),
                panelCandidates,
                panelButtons,
            },
        };

        Content = panel;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
    }

    private static Button MakeCandidateCard(AssistedSplitViewModel vm, AssistedSplitCandidate candidate)
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
                MakeHalfPreview(candidate.FirstText, candidate.FirstInfo),
                MakeHalfPreview(candidate.SecondText, candidate.SecondInfo),
            },
        };

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
        };
    }

    private static Border MakeHalfPreview(string text, string info)
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

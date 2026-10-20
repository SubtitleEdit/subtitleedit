using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Main.AssistedSplit;

/// <summary>
/// Assisted split in the editor look (see <see cref="AssistedCardChrome"/>): the current text on
/// top, one card per split point (number key, both halves), Cancel in the button bar.
/// </summary>
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
        AssistedCardChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = AssistedCardChrome.MakeHeader(Se.Language.General.AssistedSplitChooseSplitPoint, vm.SubtitleInfo);

        var panelCandidates = new StackPanel { Spacing = 8, Margin = new Thickness(14, 12, 14, 14) };
        foreach (var candidate in vm.Candidates)
        {
            panelCandidates.Children.Add(AssistedCardChrome.MakeCard(candidate.Number, candidate.Title, vm.PickCommand, candidate,
                AssistedCardChrome.MakePreview(candidate.FirstText, candidate.FirstInfo),
                AssistedCardChrome.MakePreview(candidate.SecondText, candidate.SecondInfo)));
        }

        // The candidate list scrolls when there are more cards than fit on screen.
        MaxHeight = 850;
        var scrollCandidates = new ScrollViewer
        {
            Content = panelCandidates,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Focusable = true,
        };

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel);
        buttonCancel.Click += (_, _) => Close();

        Content = AssistedCardChrome.MakeRoot(header, scrollCandidates, buttonCancel);

        UiUtil.FocusOnFirstActivation(this, scrollCandidates); // hack to make OnKeyDown work

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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

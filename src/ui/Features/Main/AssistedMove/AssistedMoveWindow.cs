using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Main.AssistedSplit;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main.AssistedMove;

/// <summary>
/// Assisted move in the editor look (see <see cref="AssistedCardChrome"/>): the current text on
/// top, one card per move (number key, resulting text(s)), Cancel in the button bar.
/// </summary>
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
        AssistedCardChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = AssistedCardChrome.MakeHeader(Se.Language.General.AssistedMoveChooseMove, vm.SubtitleInfo);

        var panelCandidates = new StackPanel { Spacing = 8, Margin = new Thickness(14, 12, 14, 14) };
        foreach (var candidate in vm.Candidates)
        {
            var previews = new List<Control> { AssistedCardChrome.MakePreview(candidate.FirstText, candidate.FirstInfo) };
            if (!string.IsNullOrEmpty(candidate.SecondText))
            {
                previews.Add(AssistedCardChrome.MakePreview(candidate.SecondText, candidate.SecondInfo));
            }

            panelCandidates.Children.Add(AssistedCardChrome.MakeCard(candidate.Number, candidate.Title, vm.PickCommand, candidate, previews.ToArray()));
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

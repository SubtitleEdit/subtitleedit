using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Tools.AiReview;

/// <summary>
/// "Play current" in the AI review window drives the main window's video player through the hooks
/// handed in by Initialize. These tests pin the contract of those hooks: the play button only shows
/// when a video is loaded, playing asks for the paragraph index the suggestion belongs to, and
/// closing only stops playback the window itself started.
/// </summary>
public class AiReviewPlaybackTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static AiReviewViewModel MakeViewModel()
    {
        return new AiReviewViewModel(new WindowService(new NullServiceProvider()));
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("How are you?", 1500, 2500));
        return subtitle;
    }

    private static ReviewSuggestionItem MakeSuggestion(int paragraphIndex)
    {
        return new ReviewSuggestionItem
        {
            Number = paragraphIndex + 1,
            ParagraphIndex = paragraphIndex,
            UnitId = paragraphIndex,
            Category = ReviewCategory.Spelling,
            Before = "before",
            After = "after",
            Reason = string.Empty,
        };
    }

    [AvaloniaFact]
    public void PlayCurrentLine_PlaysTheParagraphOfTheSelectedSuggestion()
    {
        var played = new List<int>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, played.Add);

        Assert.True(vm.IsPlayVisible);

        vm.SelectedSuggestion = MakeSuggestion(1);
        vm.PlayCurrentLineCommand.Execute(null);

        Assert.Equal(new[] { 1 }, played);
    }

    [AvaloniaFact]
    public void PlayCurrentLine_NoSelection_DoesNothing()
    {
        var played = new List<int>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, played.Add);

        vm.PlayCurrentLineCommand.Execute(null);

        Assert.Empty(played);
    }

    [AvaloniaFact]
    public void Initialize_WithoutPlayHook_HidesPlayButton()
    {
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null);

        Assert.False(vm.IsPlayVisible);

        // No hook, no crash - the command must stay a no-op rather than throw.
        vm.SelectedSuggestion = MakeSuggestion(0);
        vm.PlayCurrentLineCommand.Execute(null);
    }

    [AvaloniaFact]
    public void OnClosing_StopsPlaybackOnlyWhenThisWindowStartedIt()
    {
        var stopped = 0;
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, _ => { }, () => stopped++);

        vm.OnClosing();
        Assert.Equal(0, stopped);

        vm.SelectedSuggestion = MakeSuggestion(0);
        vm.PlayCurrentLineCommand.Execute(null);
        vm.OnClosing();
        Assert.Equal(1, stopped);
    }

    [AvaloniaFact]
    public void OnKeyDown_F5_PlaysSelectedLine()
    {
        var played = new List<int>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, played.Add);
        vm.SelectedSuggestion = MakeSuggestion(0);

        var e = new KeyEventArgs { Key = Key.F5, KeyModifiers = KeyModifiers.None, RoutedEvent = InputElement.KeyDownEvent };
        vm.OnKeyDown(e);

        Assert.True(e.Handled);
        Assert.Equal(new[] { 0 }, played);
    }

    [AvaloniaFact]
    public void Window_PlayButton_FollowsIsPlayVisible()
    {
        // With a video (a play hook) the button is in the tree and visible...
        var vmWithVideo = MakeViewModel();
        vmWithVideo.Initialize(MakeSubtitle(), null, _ => { });
        var windowWithVideo = new AiReviewWindow(vmWithVideo);
        try
        {
            var button = FindPlayButton(windowWithVideo);
            Assert.NotNull(button);
            Assert.True(button!.IsVisible);
        }
        finally
        {
            windowWithVideo.Close();
        }

        // ...and without one it is hidden rather than a dead button.
        var vmNoVideo = MakeViewModel();
        vmNoVideo.Initialize(MakeSubtitle(), null);
        var windowNoVideo = new AiReviewWindow(vmNoVideo);
        try
        {
            var button = FindPlayButton(windowNoVideo);
            Assert.NotNull(button);
            Assert.False(button!.IsVisible);
        }
        finally
        {
            windowNoVideo.Close();
        }
    }

    private static Button? FindPlayButton(Control root)
    {
        return root.GetLogicalDescendants().OfType<Button>()
            .FirstOrDefault(b => AutomationProperties.GetName(b) == Se.Language.General.PlayCurrent);
    }

    [AvaloniaFact]
    public void OnKeyDown_Space_IsLeftToTheGridCheckboxToggle()
    {
        var played = new List<int>();
        var vm = MakeViewModel();
        vm.Initialize(MakeSubtitle(), null, played.Add);
        vm.SelectedSuggestion = MakeSuggestion(0);

        var e = new KeyEventArgs { Key = Key.Space, KeyModifiers = KeyModifiers.None, RoutedEvent = InputElement.KeyDownEvent };
        vm.OnKeyDown(e);

        Assert.False(e.Handled);
        Assert.Empty(played);
    }
}

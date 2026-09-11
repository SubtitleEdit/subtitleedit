using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UITests.Features.Edit;

/// <summary>
/// The paragraph ids of the subtitle Multiple replace was handed are how the main window finds the
/// grid row each line came from, so the result must carry them back - "Apply" rounds included.
/// Copying the subtitle with new ids made every line look like a line the main window had never
/// seen, which meant rebuilding the rows and losing the original text of a translation (#14053).
/// </summary>
public class MultipleReplaceParagraphIdTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [AvaloniaFact]
    public void Apply_HandsBackTheParagraphIdsItWasGiven()
    {
        var (vm, ids) = WithThreeMatchingLines();
        Subtitle? applied = null;
        vm.OnApply = (subtitle, _) => applied = subtitle;

        vm.ApplyCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(applied);
        Assert.Equal(ids, applied!.Paragraphs.Select(p => p.Id).ToList());
        Assert.Equal("The color is red.", applied.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void SecondApplyRound_StillHasTheSameParagraphIds()
    {
        var (vm, ids) = WithThreeMatchingLines();
        var applied = new List<Subtitle>();
        vm.OnApply = (subtitle, _) => applied.Add(subtitle);

        vm.ApplyCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        vm.ApplyCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, applied.Count);
        Assert.Equal(ids, applied[1].Paragraphs.Select(p => p.Id).ToList());
    }

    [AvaloniaFact]
    public void Ok_HandsBackTheParagraphIdsItWasGiven()
    {
        var (vm, ids) = WithThreeMatchingLines();

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.Equal(ids, vm.FixedSubtitle.Paragraphs.Select(p => p.Id).ToList());
    }

    private static (MultipleReplaceViewModel Vm, List<Guid?> Ids) WithThreeMatchingLines()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();

        var category = new RuleTreeNode(true) { CategoryName = "c1", IsActive = true };
        vm.Nodes.Add(category);
        category.SubNodes!.Add(new RuleTreeNode(false)
        {
            Find = "colour",
            ReplaceWith = "color",
            IsActive = true,
            Parent = category,
            Type = MultipleReplaceType.CaseInsensitive,
        });

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("The colour is red.", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("The colour is green.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("The colour is blue.", 4000, 6000));
        vm.Initialize(subtitle);

        WaitForPreview(vm, 3);
        Assert.Equal(3, vm.Fixes.Count);

        return (vm, subtitle.Paragraphs.Select(p => p.Id).ToList());
    }

    // The preview is generated on a background timer, so waiting is the only way to observe it.
    private static void WaitForPreview(MultipleReplaceViewModel vm, int expectedFixes)
    {
        var end = Environment.TickCount64 + 3000;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
            if (vm.Fixes.Count == expectedFixes)
            {
                Dispatcher.UIThread.RunJobs();
                return;
            }

            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs();
    }
}

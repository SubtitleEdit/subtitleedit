using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleLines;
using Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleWords;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;

namespace UITests.Features.Shared;

/// <summary>
/// Windows whose list is filled before the window is built used to open with row 0 highlighted
/// but the view model's selection still null - so the buttons gated on "is anything selected"
/// stayed disabled, and detail panes stayed blank, until the user clicked another row and back.
/// </summary>
public class PrePopulatedGridSelectionTests
{
    private static List<SubtitleLineViewModel> MakeLines(params string[] texts)
    {
        var lines = new List<SubtitleLineViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            lines.Add(new SubtitleLineViewModel(new Paragraph(texts[i], i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        return lines;
    }

    private static void Show(Avalonia.Controls.Window window)
    {
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void ShotChangeListWindow_HasASelectionAndEnabledButtonsOnOpen()
    {
        var vm = new ShotChangeListViewModel();
        vm.Initialize(new List<double> { 1.0, 2.5 });
        var window = new ShotChangeListWindow(vm);

        Show(window);

        Assert.Same(vm.ShotChanges[0], vm.SelectedShotChange);
        Assert.True(vm.HasShotChanges, "Go to / Clear are bound to HasShotChanges");

        window.Close();
    }

    [AvaloniaFact]
    public void ErrorListWindow_HasASelectionAndAnEnabledGoToOnOpen()
    {
        var lines = MakeLines("First line", "Second line");
        var vm = new ErrorListViewModel();
        vm.Initialize(new List<ErrorListItem>
        {
            new(lines[0], null, lines[1]),
            new(lines[1], lines[0], null),
        });
        var window = new ErrorListWindow(vm);

        Show(window);

        Assert.Same(vm.Subtitles[0], vm.SelectedSubtitle);
        Assert.True(vm.HasErrors, "Go to is bound to HasErrors");

        window.Close();
    }

    [AvaloniaFact]
    public void FindDoubleWordsWindow_HasASelectionAndAnEnabledGoToOnOpen()
    {
        var vm = new FindDoubleWordsViewModel();
        vm.Initialize(MakeLines("This is is a test", "And and another one"));
        var window = new FindDoubleWordsWindow(vm);

        Show(window);

        Assert.NotEmpty(vm.Subtitles);
        Assert.Same(vm.Subtitles[0], vm.SelectedSubtitle);
        Assert.True(vm.HasDoubleWords, "Go to is bound to HasDoubleWords");

        window.Close();
    }

    [AvaloniaFact]
    public void FindDoubleLinesWindow_HasASelectionAndAnEnabledGoToOnOpen()
    {
        var vm = new FindDoubleLinesViewModel();
        vm.Initialize(MakeLines("Same line", "Same line"));
        var window = new FindDoubleLinesWindow(vm);

        Show(window);

        Assert.NotEmpty(vm.Subtitles);
        Assert.Same(vm.Subtitles[0], vm.SelectedSubtitle);
        Assert.True(vm.HasDoubleLines, "Go to is bound to HasDoubleLines");

        window.Close();
    }

    [AvaloniaFact]
    public void BatchConvertWindow_ShowsTheFirstFunctionsSettingsOnOpen()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<BatchConvertViewModel>();
        var window = new BatchConvertWindow(vm);

        Show(window);

        Assert.NotNull(vm.SelectedBatchFunction);
        Assert.NotNull(vm.FunctionContainer.Content);

        window.Close();
    }
}

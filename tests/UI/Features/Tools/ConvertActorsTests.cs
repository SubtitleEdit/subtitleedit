using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.ConvertActors;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Tools;

/// <summary>
/// Convert actors: the preview rows the dialog builds, and what "OK" puts back in the grid.
///
/// Converting "Inline actor via :" to "Actor" wrote nothing to the actor column whenever the name
/// was not on the first line, and a second speaker in the same paragraph was deleted with its line
/// left behind - the colon path had none of the paragraph splitting the bracket formats got (#14077).
/// </summary>
public class ConvertActorsTests
{
    [AvaloniaFact]
    public void ColonToActor_WritesTheActorColumn()
    {
        var rows = Convert(ConvertActorType.InlineColon, ConvertActorType.Actor, "Joe: How are you?");

        Assert.Equal(new[] { "How are you?" }, rows.Select(r => r.Text));
        Assert.Equal(new[] { "Joe" }, rows.Select(r => r.Actor));
    }

    /// <summary>Both speakers are kept: one paragraph per actor, as the bracket formats do.</summary>
    [AvaloniaFact]
    public void ColonDialogToActor_SplitsTheParagraph()
    {
        var rows = Convert(
            ConvertActorType.InlineColon,
            ConvertActorType.Actor,
            "Joe: How are you?" + Environment.NewLine + "Jane: I am fine.");

        Assert.Equal(new[] { "How are you?", "I am fine." }, rows.Select(r => r.Text));
        Assert.Equal(new[] { "Joe", "Jane" }, rows.Select(r => r.Actor));
        Assert.Equal(new[] { 1, 2 }, rows.Select(r => r.Number));
    }

    /// <summary>The name is on the second line - the actor column stayed empty and "Jane" was lost.</summary>
    [AvaloniaFact]
    public void ColonOnSecondLineOnly_StillWritesTheActorColumn()
    {
        var rows = Convert(
            ConvertActorType.InlineColon,
            ConvertActorType.Actor,
            "How are you?" + Environment.NewLine + "Jane: I am fine.");

        Assert.Equal(new[] { "How are you?" + Environment.NewLine + "I am fine." }, rows.Select(r => r.Text));
        Assert.Equal(new[] { "Jane" }, rows.Select(r => r.Actor));
    }

    /// <summary>Moving the actor into the text empties the column - it was written twice before.</summary>
    [AvaloniaFact]
    public void ActorToSquare_ClearsTheActorColumn()
    {
        var rows = Convert(
            ConvertActorType.Actor,
            ConvertActorType.InlineSquareBrackets,
            new SubRip(),
            (Text: "How are you?", Actor: "Joe"));

        Assert.Equal(new[] { "[Joe] How are you?" }, rows.Select(r => r.Text));
        Assert.Equal(new[] { string.Empty }, rows.Select(r => r.Actor));
    }

    /// <summary>
    /// "Only names" now reaches the colon conversions too: a colon in plain text is listed, but not
    /// checked, so "Meet me at 3:30." is not turned into "30." with an actor of "Meet me at 3".
    /// </summary>
    [AvaloniaFact]
    public void OnlyNames_LeavesAColonInPlainTextUnchecked()
    {
        var vm = Preview(ConvertActorType.InlineColon, ConvertActorType.Actor, new SubRip(), onlyNames: true,
            (Text: "Meet me at 3:30.", Actor: string.Empty));

        var item = Assert.Single(vm.Subtitles);
        Assert.False(item.IsChecked);
    }

    [AvaloniaFact]
    public void OnlyNamesOff_KeepsEveryConversionChecked()
    {
        var vm = Preview(ConvertActorType.InlineColon, ConvertActorType.Actor, new SubRip(), onlyNames: false,
            (Text: "Meet me at 3:30.", Actor: string.Empty));

        var item = Assert.Single(vm.Subtitles);
        Assert.True(item.IsChecked);
    }

    /// <summary>A colon that starts a line is not an actor, so there is nothing to convert.</summary>
    [AvaloniaFact]
    public void ColonWithoutAnActor_IsNotListed()
    {
        var vm = Preview(ConvertActorType.InlineColon, ConvertActorType.Actor, new SubRip(), onlyNames: false,
            (Text: ": how are you?", Actor: null!));

        Assert.Empty(vm.Subtitles);
    }

    /// <summary>
    /// A speaker tag the line splitter broke in half - "(Speaker\n2)" - found no complete pair on
    /// either line, so the paragraph was silently left alone and the actor column stayed empty.
    /// </summary>
    [AvaloniaFact]
    public void ParenthesesBrokenOverLineBreakToActor_StillWritesTheActorColumn()
    {
        var rows = Convert(
            ConvertActorType.InlineParentheses,
            ConvertActorType.Actor,
            "Princess Peach on (Speaker" + Environment.NewLine + "2) Super Smash Bros.");

        Assert.Equal(new[] { "Princess Peach on Super Smash Bros." }, rows.Select(r => r.Text));
        Assert.Equal(new[] { "Speaker 2" }, rows.Select(r => r.Actor));
    }

    [AvaloniaFact]
    public void SquareBracketsBrokenOverLineBreakToActor_StillWritesTheActorColumn()
    {
        var rows = Convert(
            ConvertActorType.InlineSquareBrackets,
            ConvertActorType.Actor,
            "[NAR" + Environment.NewLine + "RATOR] Once upon a time.");

        Assert.Equal(new[] { "Once upon a time." }, rows.Select(r => r.Text));
        Assert.Equal(new[] { "NAR RATOR" }, rows.Select(r => r.Actor));
    }

    /// <summary>
    /// A parenthetical remark spanning lines is prose, not a broken speaker tag - joining it would
    /// silently reflow the paragraph, so anything longer than a name is left alone.
    /// </summary>
    [AvaloniaFact]
    public void LongParentheticalOverLineBreak_IsNotJoined()
    {
        var text = "He said (which nobody in the whole room actually believed" + Environment.NewLine +
                   "for even a single second) that he would come.";
        var rows = Convert(ConvertActorType.InlineParentheses, ConvertActorType.Actor, text);

        Assert.Equal(new[] { text }, rows.Select(r => r.Text));
        Assert.Equal(new[] { string.Empty }, rows.Select(r => r.Actor));
    }

    private static List<SubtitleLineViewModel> Convert(ConvertActorType from, ConvertActorType to, params string[] texts)
        => Convert(from, to, new SubRip(), texts.Select(t => (Text: t, Actor: string.Empty)).ToArray());

    /// <summary>Runs the dialog and applies its result to the main grid, as the menu item does.</summary>
    private static List<SubtitleLineViewModel> Convert(
        ConvertActorType from,
        ConvertActorType to,
        SubtitleFormat format,
        params (string Text, string Actor)[] lines)
    {
        var (window, main) = CreateMainViewModel();
        try
        {
            var number = 1;
            foreach (var line in lines)
            {
                main.Subtitles.Add(new SubtitleLineViewModel(
                    new Paragraph(line.Text, number * 1000, number * 1000 + 900) { Actor = line.Actor },
                    format)
                {
                    Number = number,
                });
                number++;
            }

            var vm = RunPreview(from, to, format, false, main.Subtitles.ToList());
            Invoke(vm, "Ok");

            var apply = typeof(MainViewModel).GetMethod(
                            "ApplyFixedSubtitle",
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null,
                            new[] { typeof(List<SubtitleLineViewModel>), typeof(int) },
                            null)
                        ?? throw new InvalidOperationException("ApplyFixedSubtitle not found");
            apply.Invoke(main, new object?[] { vm.FixedSubtitle, 0 });
            Dispatcher.UIThread.RunJobs();

            return main.Subtitles.ToList();
        }
        finally
        {
            CloseWindow(window, main);
        }
    }

    private static ConvertActorsViewModel Preview(
        ConvertActorType from,
        ConvertActorType to,
        SubtitleFormat format,
        bool onlyNames,
        params (string Text, string Actor)[] lines)
    {
        var number = 1;
        var subtitles = new List<SubtitleLineViewModel>();
        foreach (var line in lines)
        {
            subtitles.Add(new SubtitleLineViewModel(
                new Paragraph(line.Text, number * 1000, number * 1000 + 900) { Actor = line.Actor },
                format)
            {
                Number = number,
            });
            number++;
        }

        return RunPreview(from, to, format, onlyNames, subtitles);
    }

    private static ConvertActorsViewModel RunPreview(
        ConvertActorType from,
        ConvertActorType to,
        SubtitleFormat format,
        bool onlyNames,
        List<SubtitleLineViewModel> subtitles)
    {
        var vm = new ConvertActorsViewModel
        {
            SelectedFromType = ConvertActorTypeDisplay.GetTypes().First(t => t.Type == from),
            SelectedToType = ConvertActorTypeDisplay.GetTypes().First(t => t.Type == to),
            ChangeCasing = false,
            SetColor = false,
            OnlyNames = onlyNames,
        };

        vm.Initialize(subtitles, format);

        // The preview runs on a timer in the dialog - here it is called directly.
        Invoke(vm, "UpdatePreview");
        Dispatcher.UIThread.RunJobs();
        vm.OnClosingCleanup();
        return vm;
    }

    private static void Invoke(ConvertActorsViewModel vm, string methodName)
    {
        var method = typeof(ConvertActorsViewModel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException(methodName + " not found");
        method.Invoke(vm, null);
        Dispatcher.UIThread.RunJobs();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}

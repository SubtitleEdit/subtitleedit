using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace UITests.Features.SpellCheck;

/// <summary>
/// The spell check window can play the line the flagged word is in, in the main window's video
/// player - on speech-to-text output the audio is often the only thing that says what an unknown
/// word should be, and until this you had to close the window to listen (issue #14145).
/// </summary>
public class SpellCheckPlayCurrentLineTests : IDisposable
{
    private readonly string _dictionariesFolder = Se.DictionariesFolder;

    public SpellCheckPlayCurrentLineTests()
    {
        // The CI runner has no dictionaries installed; make the local run take the same path.
        Se.DictionariesFolder = Path.Combine(Path.GetTempPath(), "se-spellcheck-test-no-dictionaries");
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _dictionariesFolder;
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class NullFocusSubtitleLine : IFocusSubtitleLine
    {
        public void GoToAndFocusLine(SubtitleLineViewModel p)
        {
        }
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeSubtitles()
    {
        return new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "I have not seen it, but that is not a problem." },
            new() { Text = "What are you doing here so late in the evening?" },
            new() { Text = "He said that they would come tomorrow." },
        };
    }

    private static SpellCheckViewModel MakeViewModel()
    {
        var vm = new SpellCheckViewModel(
            new SpellCheckManager(),
            new WindowService(new NullServiceProvider()),
            new FileHelper(),
            new BluRayHelper(),
            new OcrImageSourceHolder());

        // With no dictionary installed (the CI runner) Initialize posts the "get dictionaries"
        // dialog at Background priority; the null service provider then throws inside that
        // posted job, on whichever test happens to pump the dispatcher next - this class or a
        // later one. A dictionary entry whose file does not exist is enough to skip the dialog:
        // the spell checker fails to load it and the scan finds nothing.
        vm.Dictionaries.Add(new SpellCheckDictionaryDisplay
        {
            Name = "English",
            DictionaryFileName = Path.Combine(Path.GetTempPath(), "se-spellcheck-test-missing", "en_US.dic"),
        });
        return vm;
    }

    [AvaloniaFact]
    public void PlayCurrentLine_PlaysTheLineTheFlaggedWordIsIn()
    {
        var vm = MakeViewModel();
        var subtitles = MakeSubtitles();
        SubtitleLineViewModel? played = null;

        vm.Initialize(subtitles, 0, new NullFocusSubtitleLine(), null, false, p => played = p, () => { });

        Assert.True(vm.IsPlayVisible);

        // What the scan does when it stops on a word: the paragraph it is in becomes the selected one.
        vm.SelectedParagraph = subtitles[2];
        Assert.True(vm.PlayCurrentLineCommand.CanExecute(null));
        vm.PlayCurrentLineCommand.Execute(null);

        Assert.Same(subtitles[2], played);
    }

    [AvaloniaFact]
    public void NoVideoLoaded_HidesThePlayButton()
    {
        var vm = MakeViewModel();

        // The main window hands in no play hook when no video is open.
        vm.Initialize(MakeSubtitles(), 0, new NullFocusSubtitleLine(), null);

        Assert.False(vm.IsPlayVisible);
        Assert.False(vm.PlayCurrentLineCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void Closing_StopsOnlyPlaybackThisWindowStarted()
    {
        var vm = MakeViewModel();
        var subtitles = MakeSubtitles();
        var stopCount = 0;

        vm.Initialize(subtitles, 0, new NullFocusSubtitleLine(), null, false, _ => { }, () => stopCount++);

        // Nothing was played here, so a video the user left running keeps running.
        vm.OnClosingCleanup();
        Assert.Equal(0, stopCount);

        vm.SelectedParagraph = subtitles[0];
        vm.PlayCurrentLineCommand.Execute(null);
        vm.OnClosingCleanup();

        Assert.Equal(1, stopCount);
    }
}

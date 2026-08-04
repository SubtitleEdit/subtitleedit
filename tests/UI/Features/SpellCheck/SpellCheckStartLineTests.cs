using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.SpellCheck;

// Scanning from the line the user selected (after answering "continue from current line?") must find
// a misspelling on that very line, not only on the lines below it.
public class SpellCheckStartLineTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public SpellCheckStartLineTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeStartLineRepro_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        var repoRoot = FindRepoRoot();
        File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.dic"), Path.Combine(_tempDictionariesFolder, "en_US.dic"));
        File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.aff"), Path.Combine(_tempDictionariesFolder, "en_US.aff"));

        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Dictionaries")))
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }

    private SpellCheckManager MakeManager()
    {
        var manager = new SpellCheckManager();
        manager.Initialize(Path.Combine(_tempDictionariesFolder, "en_US.dic"), "en");
        return manager;
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeSubtitles()
    {
        return new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "This line is perfectly fine." },   // line 0: no errors
            new() { Text = "Ths line has a typo." },           // line 1: "Ths" is misspelled
            new() { Text = "This line is fine too." },         // line 2: no errors
        };
    }

    [Fact]
    public void FromTop_FindsTheTypo()
    {
        var results = MakeManager().CheckSpelling(MakeSubtitles(), null, null);

        Assert.Single(results);
        Assert.Equal("Ths", results[0].Word.Text);
    }

    [Fact]
    public void FromLineBeforeTheTypo_FindsTheTypo()
    {
        // User selected line 0 (before the error) and answered "continue from current line"
        var startFrom = new SpellCheckResult { LineIndex = 0, WordIndex = -1 };

        var results = MakeManager().CheckSpelling(MakeSubtitles(), startFrom, null);

        Assert.Single(results);
        Assert.Equal("Ths", results[0].Word.Text);
    }

    [Fact]
    public void FromTheLineWithTheTypo_FindsTheTypo()
    {
        // User selected line 1 - the line that HAS the typo - and answered "continue from current line".
        // This is what StartSpellCheckAsync builds: new SpellCheckResult { LineIndex = startLine, WordIndex = -1 }
        var startFrom = new SpellCheckResult { LineIndex = 1, WordIndex = -1 };

        var results = MakeManager().CheckSpelling(MakeSubtitles(), startFrom, null);

        Assert.Single(results);
        Assert.Equal("Ths", results[0].Word.Text);
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try
        {
            Directory.Delete(_tempDictionariesFolder, true);
        }
        catch
        {
            // ignore
        }
    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.SpellCheck;

// A persisted "use always" pair must only replace misspelled words - a stray "and" -> "&" pair in
// en_US_UseAlways.xml silently rewrote every correctly spelled "and". (#15767)
public class SpellCheckChangeAllCorrectWordTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly bool _originalRememberUseAlwaysList;
    private readonly string _tempDictionariesFolder;

    public SpellCheckChangeAllCorrectWordTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _originalRememberUseAlwaysList = Configuration.Settings.Tools.RememberUseAlwaysList;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeChangeAllCorrect_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        var repoRoot = FindRepoRoot();
        File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.dic"), Path.Combine(_tempDictionariesFolder, "en_US.dic"));
        File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.aff"), Path.Combine(_tempDictionariesFolder, "en_US.aff"));
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "en_US_UseAlways.xml"),
            "<UseAlways><Pair from=\"and\" to=\"&amp;\" /><Pair from=\"Ths\" to=\"This\" /></UseAlways>");

        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        Configuration.Settings.Tools.RememberUseAlwaysList = true;
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

    [Fact]
    public void PersistedPair_DoesNotReplaceCorrectlySpelledWord()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "Salt and pepper." },
        };

        var results = MakeManager().CheckSpelling(subtitles);

        Assert.Empty(results);
        Assert.Equal("Salt and pepper.", subtitles[0].Text);
    }

    [Fact]
    public void PersistedPair_StillReplacesMisspelledWord()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "Ths line and that line." },
        };

        var results = MakeManager().CheckSpelling(subtitles);

        Assert.Empty(results);
        Assert.Equal("This line and that line.", subtitles[0].Text);
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        Configuration.Settings.Tools.RememberUseAlwaysList = _originalRememberUseAlwaysList;
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

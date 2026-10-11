using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.SpellCheck;

// Switching dictionary mid-session must drop the previous language's change-all pairs. (#15767)
public class SpellCheckChangeAllDictionarySwitchTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly bool _originalRememberUseAlwaysList;
    private readonly string _tempDictionariesFolder;

    public SpellCheckChangeAllDictionarySwitchTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _originalRememberUseAlwaysList = Configuration.Settings.Tools.RememberUseAlwaysList;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeChangeAllSwitch_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        var repoRoot = FindRepoRoot();
        foreach (var name in new[] { "en_US", "en_GB" })
        {
            File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.dic"), Path.Combine(_tempDictionariesFolder, name + ".dic"));
            File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.aff"), Path.Combine(_tempDictionariesFolder, name + ".aff"));
        }

        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "en_US_UseAlways.xml"),
            "<UseAlways><Pair from=\"Ths\" to=\"This\" /></UseAlways>");

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

    [Fact]
    public void SwitchingDictionary_DropsPreviousLanguagePairs()
    {
        var manager = new SpellCheckManager();
        manager.Initialize(Path.Combine(_tempDictionariesFolder, "en_US.dic"), "en");
        manager.Initialize(Path.Combine(_tempDictionariesFolder, "en_GB.dic"), "en");

        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new() { Text = "Ths line is fine." },
        };

        var results = manager.CheckSpelling(subtitles);

        Assert.Single(results);
        Assert.Equal("Ths", results[0].Word.Text);
        Assert.Equal("Ths line is fine.", subtitles[0].Text);
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

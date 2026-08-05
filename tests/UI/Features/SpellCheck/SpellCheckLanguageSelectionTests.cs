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
/// The spell check window has to open on the language of the subtitle in front of the user. It used
/// to open on the dictionary of the previous spell check instead, so a Dutch subtitle was checked
/// against English when English was checked last - every word flagged (issue #13117).
/// </summary>
public class SpellCheckLanguageSelectionTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string? _originalLastLanguageDictionaryFile;
    private readonly string? _originalLastLanguageDictionaryName;
    private readonly string _tempDictionariesFolder;

    private static readonly string[] DutchLines =
    {
        "Ik heb het niet gezien, maar dat is niet erg.",
        "Wat doe je hier zo laat op de avond?",
        "Hij zei dat ze morgen zouden komen.",
        "Dat is een goed idee, maar we hebben geen tijd.",
        "Kun je mij even helpen met deze koffer?",
        "Ze wonen al jaren in dat kleine huis.",
        "Ik weet niet waar hij naartoe is gegaan.",
        "We moeten nu echt gaan, anders komen we te laat.",
    };

    private static readonly string[] EnglishLines =
    {
        "I have not seen it, but that is not a problem.",
        "What are you doing here so late in the evening?",
        "He said that they would come tomorrow.",
        "That is a good idea, but we do not have the time.",
        "Can you help me with this suitcase for a moment?",
        "They have lived in that small house for years.",
        "I do not know where he has gone.",
        "We really have to go now, or we will be late.",
    };

    public SpellCheckLanguageSelectionTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _originalLastLanguageDictionaryFile = Se.Settings.SpellCheck.LastLanguageDictionaryFile;
        _originalLastLanguageDictionaryName = Se.Settings.SpellCheck.LastLanguageDictionaryName;

        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeSpellCheckLanguage_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        // Three dictionaries: two English dialects (so "keep the dialect of the previous session"
        // is an actual choice) and the Dutch one the subtitle should land on.
        WriteDictionary("en_US", "color", "neighbor");
        WriteDictionary("en_GB", "colour", "neighbour");
        WriteDictionary("nl_NL", "kleur", "buurman");

        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    private void WriteDictionary(string name, params string[] words)
    {
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, name + ".aff"), "SET UTF-8\n");
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, name + ".dic"), words.Length + "\n" + string.Join("\n", words) + "\n");
    }

    private string DictionaryPath(string name)
    {
        return Path.Combine(_tempDictionariesFolder, name + ".dic");
    }

    private void SetLastUsedDictionary(string name)
    {
        Se.Settings.SpellCheck.LastLanguageDictionaryFile = DictionaryPath(name);
        Se.Settings.SpellCheck.LastLanguageDictionaryName = name;
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

    private static ObservableCollection<SubtitleLineViewModel> MakeSubtitles(string[] lines)
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        foreach (var line in lines)
        {
            subtitles.Add(new SubtitleLineViewModel { Text = line });
        }

        return subtitles;
    }

    private static SpellCheckViewModel MakeViewModel()
    {
        return new SpellCheckViewModel(
            new SpellCheckManager(),
            new WindowService(new NullServiceProvider()),
            new FileHelper(),
            new BluRayHelper(),
            new OcrImageSourceHolder());
    }

    private static string SelectedDictionaryName(SpellCheckViewModel vm)
    {
        return Path.GetFileNameWithoutExtension(vm.SelectedDictionary!.DictionaryFileName);
    }

    [AvaloniaFact]
    public void DutchSubtitle_AfterEnglishSpellCheck_UsesDutch()
    {
        SetLastUsedDictionary("en_US");
        var vm = MakeViewModel();

        vm.Initialize(MakeSubtitles(DutchLines), 0, new NullFocusSubtitleLine(), null);

        Assert.Equal("nl_NL", SelectedDictionaryName(vm));
    }

    [AvaloniaFact]
    public void EnglishSubtitle_KeepsTheDialectOfThePreviousSpellCheck()
    {
        // en_GB was used last; detection only knows "en", so the remembered dialect has to survive
        // instead of falling back to whichever English dictionary happens to be listed first.
        SetLastUsedDictionary("en_GB");
        var vm = MakeViewModel();

        vm.Initialize(MakeSubtitles(EnglishLines), 0, new NullFocusSubtitleLine(), null);

        Assert.Equal("en_GB", SelectedDictionaryName(vm));
    }

    [AvaloniaFact]
    public void DictionaryPickedForThisSubtitle_WinsOverDetection()
    {
        SetLastUsedDictionary("nl_NL");
        var vm = MakeViewModel();

        // What the main window passes after the user picked a dictionary for the subtitle it has
        // loaded - a deliberate choice that detection must not undo.
        var picked = new SpellCheckDictionaryDisplay { Name = "en_US", DictionaryFileName = DictionaryPath("en_US") };

        vm.Initialize(MakeSubtitles(DutchLines), 0, new NullFocusSubtitleLine(), picked);

        Assert.Equal("en_US", SelectedDictionaryName(vm));
    }

    [AvaloniaFact]
    public void SubtitleWithoutDetectableLanguage_KeepsTheDictionaryOfThePreviousSpellCheck()
    {
        SetLastUsedDictionary("en_GB");
        var vm = MakeViewModel();

        vm.Initialize(MakeSubtitles(new[] { "123", "- ...", "?!" }), 0, new NullFocusSubtitleLine(), null);

        Assert.Equal("en_GB", SelectedDictionaryName(vm));
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        Se.Settings.SpellCheck.LastLanguageDictionaryFile = _originalLastLanguageDictionaryFile;
        Se.Settings.SpellCheck.LastLanguageDictionaryName = _originalLastLanguageDictionaryName;

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

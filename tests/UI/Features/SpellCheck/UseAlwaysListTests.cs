using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.UseAlwaysList;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace UITests.Features.SpellCheck;

// The "use always" list editor (#15767): view, add, update and remove the pairs in
// <lang>_UseAlways.xml, and flag pairs that can never apply because the word is spelled correctly.
public class UseAlwaysListTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly bool _originalRememberUseAlwaysList;
    private readonly string _folder;

    public UseAlwaysListTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _originalRememberUseAlwaysList = Configuration.Settings.Tools.RememberUseAlwaysList;
        _folder = Path.Combine(Path.GetTempPath(), "SeUseAlwaysList_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);

        var repoRoot = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(repoRoot.FullName, "Dictionaries")))
        {
            repoRoot = repoRoot.Parent!;
        }

        File.Copy(Path.Combine(repoRoot.FullName, "Dictionaries", "en_US.dic"), Path.Combine(_folder, "en_US.dic"));
        File.Copy(Path.Combine(repoRoot.FullName, "Dictionaries", "en_US.aff"), Path.Combine(_folder, "en_US.aff"));

        Se.DictionariesFolder = _folder;
        SpellCheckConfig.DictionariesFolder = () => _folder;
        Configuration.Settings.Tools.RememberUseAlwaysList = true;
    }

    private string FileName => UseAlwaysListFile.GetFileName(_folder, "en_US");

    private void WritePairs(string xmlPairs) => File.WriteAllText(FileName, "<UseAlways>" + xmlPairs + "</UseAlways>");

    private UseAlwaysListViewModel MakeViewModel()
    {
        var vm = new UseAlwaysListViewModel(null!);
        vm.Initialize(Path.Combine(_folder, "en_US.dic"));
        return vm;
    }

    private static void WaitFor(Func<bool> condition)
    {
        var until = DateTime.UtcNow.AddSeconds(15);
        while (!condition() && DateTime.UtcNow < until)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(20);
        }
    }

    [Fact]
    public void File_RoundTrips()
    {
        UseAlwaysListFile.Save(FileName, new[]
        {
            new System.Collections.Generic.KeyValuePair<string, string>("and", "&"),
            new System.Collections.Generic.KeyValuePair<string, string>("teh", "the"),
        });

        var pairs = UseAlwaysListFile.Load(FileName);

        Assert.Equal(2, pairs.Count);
        Assert.Equal("&", pairs["and"]);
        Assert.Equal("the", pairs["teh"]);
    }

    [AvaloniaFact]
    public void Editor_SelectsLanguageAndFlagsCorrectlySpelledWords()
    {
        WritePairs("<Pair from=\"and\" to=\"&amp;\" /><Pair from=\"teh\" to=\"the\" />");

        var vm = MakeViewModel();
        WaitFor(() => vm.HasUnused);

        Assert.Equal("en_US", vm.SelectedLanguage?.Code);
        Assert.Equal(new[] { "and", "teh" }, vm.Pairs.Select(p => p.From));
        Assert.True(vm.Pairs.Single(p => p.From == "and").IsUnused);
        Assert.False(vm.Pairs.Single(p => p.From == "teh").IsUnused);
    }

    [AvaloniaFact]
    public void Editor_AddUpdateRemove_SavedOnOk()
    {
        WritePairs("<Pair from=\"and\" to=\"&amp;\" /><Pair from=\"teh\" to=\"the\" />");
        var vm = MakeViewModel();

        vm.RemoveCommand.Execute(vm.Pairs.Single(p => p.From == "and"));

        vm.EditFrom = "recieve";
        vm.EditTo = "receive";
        vm.AddOrUpdateCommand.Execute(null);

        vm.EditFrom = "teh";
        Assert.True(vm.IsEditingExisting);
        vm.EditTo = "The";
        vm.AddOrUpdateCommand.Execute(null);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        var saved = UseAlwaysListFile.Load(FileName);
        Assert.Equal(2, saved.Count);
        Assert.Equal("receive", saved["recieve"]);
        Assert.Equal("The", saved["teh"]);
        Assert.False(saved.ContainsKey("and"));
    }

    [AvaloniaFact]
    public void Editor_Cancel_DoesNotSave()
    {
        WritePairs("<Pair from=\"and\" to=\"&amp;\" />");
        var vm = MakeViewModel();

        vm.RemoveCommand.Execute(vm.Pairs.Single());
        vm.CancelCommand.Execute(null);

        Assert.Equal("&", UseAlwaysListFile.Load(FileName)["and"]);
    }

    [AvaloniaFact]
    public void Editor_RemoveUnused_KeepsUsefulPairs()
    {
        WritePairs("<Pair from=\"and\" to=\"&amp;\" /><Pair from=\"teh\" to=\"the\" />");
        var vm = MakeViewModel();
        WaitFor(() => vm.HasUnused);

        vm.RemoveUnusedCommand.Execute(null);

        Assert.Equal(new[] { "teh" }, vm.Pairs.Select(p => p.From));
        Assert.False(vm.HasUnused);
    }

    [Fact]
    public void SpellCheck_ReloadUseAlwaysList_PicksUpEditedFile()
    {
        var manager = new SpellCheckManager();
        manager.Initialize(Path.Combine(_folder, "en_US.dic"), "en");

        WritePairs("<Pair from=\"Ths\" to=\"This\" />");
        manager.ReloadUseAlwaysList();

        var subtitles = new ObservableCollection<SubtitleLineViewModel> { new() { Text = "Ths line is fine." } };
        var results = manager.CheckSpelling(subtitles);

        Assert.Empty(results);
        Assert.Equal("This line is fine.", subtitles[0].Text);
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        Configuration.Settings.Tools.RememberUseAlwaysList = _originalRememberUseAlwaysList;
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // ignore
        }
    }
}

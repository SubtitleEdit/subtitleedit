using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Options.Settings;

/// <summary>
/// The shortcut slots the Shortcuts window configures - colors 1-8, actors 1-10 and the
/// "surround with" pairs - are top-level values on <see cref="Se"/>, not one of its sections,
/// so export/import walked straight past them and every customization was lost on the way to a
/// new install (#14232, matter G).
/// </summary>
public class SettingsImportExportShortcutSlotsTests
{
    private sealed class FakeFileHelper : IFileHelper
    {
        private readonly string _path;

        public FakeFileHelper(string path) => _path = path;

        public Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "", string? suggestedStartFolder = null) => Task.FromResult(_path);
        public Task<string> PickSaveFile(Visual sender, string extension, string suggestedFileName, string title) => Task.FromResult(_path);

        public Task<string[]> PickOpenFiles(Visual sender, string title, string extensionTitle, List<string> extensions, string extensionTitle2, List<string> extensions2) => throw new NotSupportedException();
        public Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null, bool includeSpreadsheets = false) => throw new NotSupportedException();
        public Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null) => throw new NotSupportedException();
        public Task<string> PickSaveSubtitleFile(Visual sender, Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat currentFormat, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<FileHelperSubtitleSavePickerResult?> PickSaveSubtitleFileAs(Visual sender, Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat currentFormat, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveSubtitleFile(Visual sender, string extension, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveFile(Visual sender, string extension, string extensionTitle, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveFile(Visual sender, IReadOnlyList<(string Name, string Extension)> fileTypes, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickOpenVideoFile(Visual sender, string title) => throw new NotSupportedException();
        public Task<string[]> PickOpenVideoFiles(Visual sender, string title) => throw new NotSupportedException();
        public Task<string> PickOpenImageFile(Visual sender, string title) => throw new NotSupportedException();
    }

    private static async Task<string> ExportAll(string path, Se settings)
    {
        var previous = Se.Settings;
        Se.Settings = settings;
        try
        {
            var vm = new SettingsImportExportViewModel(new FakeFileHelper(path)) { Window = new Window() };
            vm.SetIsExport(true);
            vm.ExportImportAll = true;
            await vm.OkCommand.ExecuteAsync(null);
        }
        finally
        {
            Se.Settings = previous;
        }

        return await File.ReadAllTextAsync(path);
    }

    private static async Task ImportAllInto(string path, Se settings)
    {
        var previous = Se.Settings;
        Se.Settings = settings;
        try
        {
            var vm = new SettingsImportExportViewModel(new FakeFileHelper(path)) { Window = new Window() };
            vm.SetIsExport(false);
            Assert.True(await vm.PromptAndLoadImportFile());
            vm.ExportImportAll = true;
            await vm.OkCommand.ExecuteAsync(null);
        }
        finally
        {
            Se.Settings = previous;
        }
    }

    private static string TempFile() =>
        Path.Combine(SettingsIsolationFixture.SettingsDirectory, Guid.NewGuid().ToString("N") + ".json");

    [AvaloniaFact]
    public async Task ExportImportAllCarriesTheShortcutSlots()
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();

        var source = new Se
        {
            Color1 = "#ff112233",
            Actor1 = "Narrator",
        };
        source.SetSurround(1, "(", ")");
        source.SetSurround(5, "<b>", "</b>");

        await ExportAll(path, source);

        var target = new Se();
        await ImportAllInto(path, target);

        Assert.Equal("#ff112233", target.Color1);
        Assert.Equal("Narrator", target.Actor1);
        Assert.Equal("(", target.GetSurroundLeft(1));
        Assert.Equal(")", target.GetSurroundRight(1));
        Assert.Equal("<b>", target.GetSurroundLeft(5));
        Assert.Equal("</b>", target.GetSurroundRight(5));
    }

    // An export written before the slots travelled holds the defaults of `new Se()` for them,
    // so importing one must leave the user's own slots alone instead of resetting them.
    [AvaloniaFact]
    public async Task ImportingAFileWithoutTheSlotMarkerLeavesTheCurrentSlotsAlone()
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();

        await ExportAll(path, new Se());
        var withoutMarker = (await File.ReadAllTextAsync(path))
            .Replace("\"exportIncludesShortcutSlots\": true,", string.Empty);
        await File.WriteAllTextAsync(path, withoutMarker);

        var target = new Se { Color1 = "#ff445566", Actor1 = "Keep me" };
        target.SetSurround(1, "«", "»");

        await ImportAllInto(path, target);

        Assert.Equal("#ff445566", target.Color1);
        Assert.Equal("Keep me", target.Actor1);
        Assert.Equal("«", target.GetSurroundLeft(1));
        Assert.Equal("»", target.GetSurroundRight(1));
    }

    // The "search via" slots joined after the slot marker, so a file written by a build between
    // the two carries the marker but no CustomSearch keys at all - deserializing hands back the
    // factory defaults, and copying those would silently reset the user's own slots (and empty
    // out 4 and 5).
    [AvaloniaFact]
    public async Task ImportingAMarkedFileWithoutCustomSearchKeysLeavesTheCurrentSlotsAlone()
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();

        var source = new Se { Color1 = "#ff112233" };
        await ExportAll(path, source);
        var withoutCustomSearch = string.Join(
            Environment.NewLine,
            (await File.ReadAllLinesAsync(path)).Where(l => !l.TrimStart().StartsWith("\"CustomSearch", StringComparison.Ordinal)));
        await File.WriteAllTextAsync(path, withoutCustomSearch);

        var target = new Se();
        target.SetCustomSearch(4, "IMDB", "https://www.imdb.com/find?q={0}");

        await ImportAllInto(path, target);

        // The marked slot families still travel...
        Assert.Equal("#ff112233", target.Color1);

        // ...but the slots the file does not carry are left alone.
        Assert.Equal("IMDB", target.GetCustomSearchName(4));
        Assert.Equal("https://www.imdb.com/find?q={0}", target.GetCustomSearchUrl(4));
    }

    [AvaloniaFact]
    public async Task ExportWithoutShortcutsDoesNotClaimToCarryTheSlots()
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();

        var previous = Se.Settings;
        Se.Settings = new Se();
        try
        {
            var vm = new SettingsImportExportViewModel(new FakeFileHelper(path)) { Window = new Window() };
            vm.SetIsExport(true);
            vm.ExportImportAll = false;
            vm.ExportImportAppearance = true;
            await vm.OkCommand.ExecuteAsync(null);
        }
        finally
        {
            Se.Settings = previous;
        }

        var json = await File.ReadAllTextAsync(path);
        Assert.DoesNotContain("exportIncludesShortcutSlots", json);
    }
}

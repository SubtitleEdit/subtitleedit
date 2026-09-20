using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Options.Settings;

/// <summary>
/// The Settings import dialog only offered ".json", so a user arriving from SE 4.0.12 - who has a
/// Settings.xml and nothing else - could not even select their file (#14309). The dialog now takes
/// the classic XML too, mapping the categories its own checkboxes offer.
/// </summary>
public class SettingsImportSe4XmlTests
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
        public Task<string> PickOpenVideoFile(Visual sender, string title, string? lastOpenedFilePath = null) => throw new NotSupportedException();
        public Task<string[]> PickOpenVideoFiles(Visual sender, string title) => throw new NotSupportedException();
        public Task<string> PickOpenImageFile(Visual sender, string title) => throw new NotSupportedException();
    }

    private const string Se4SettingsXml =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
        "<Settings>" +
        "<General>" +
        "<SubtitleLineMaximumLength>37</SubtitleLineMaximumLength>" +
        "<SubtitleMaximumCharactersPerSeconds>17</SubtitleMaximumCharactersPerSeconds>" +
        "<UseDarkTheme>True</UseDarkTheme>" +
        "</General>" +
        "<Tools>" +
        "<ListViewSyntaxColorGap>True</ListViewSyntaxColorGap>" +
        "<ChatGptApiKey>from-se4</ChatGptApiKey>" +
        "</Tools>" +
        "<VideoControls><WaveformDrawGrid>True</WaveformDrawGrid></VideoControls>" +
        "<Shortcuts><MainFileSaveAll>Control+Shift+S</MainFileSaveAll></Shortcuts>" +
        "</Settings>";

    private static async Task<string> WriteXml(string xml, string extension = ".xml")
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = Path.Combine(SettingsIsolationFixture.SettingsDirectory, Guid.NewGuid().ToString("N") + extension);
        await File.WriteAllTextAsync(path, xml);
        return path;
    }

    private static async Task<(SettingsImportExportViewModel Vm, Se Target)> Load(string path)
    {
        var vm = new SettingsImportExportViewModel(new FakeFileHelper(path)) { Window = new Window() };
        vm.SetIsExport(false);
        Assert.True(await vm.PromptAndLoadImportFile());
        return (vm, new Se());
    }

    private static async Task ImportInto(SettingsImportExportViewModel vm, Se target, Action<SettingsImportExportViewModel>? configure = null)
    {
        var previous = Se.Settings;
        Se.Settings = target;
        try
        {
            configure?.Invoke(vm);
            await vm.OkCommand.ExecuteAsync(null);
        }
        finally
        {
            Se.Settings = previous;
        }
    }

    [AvaloniaFact]
    public async Task ImportAllFromSe4XmlFillsEveryCategoryTheFileCovers()
    {
        var (vm, target) = await Load(await WriteXml(Se4SettingsXml));

        await ImportInto(vm, target, v => v.ExportImportAll = true);

        Assert.Equal(37, target.General.SubtitleLineMaximumLength);
        Assert.Equal(17, target.General.SubtitleMaximumCharactersPerSeconds);
        Assert.True(target.General.ColorGapTooShort);
        Assert.True(target.Waveform.DrawGridLines);
        Assert.Equal("Dark", target.Appearance.Theme);
        Assert.Equal("from-se4", target.AutoTranslate.ChatGptApiKey);
        Assert.Contains(target.Shortcuts, s => s.ActionName == nameof(MainViewModel.CommandFileSaveCommand));
    }

    // SE 4's tree is not SE 5's: a category is copied field by field, so an SE 5 setting SE 4
    // never had must survive the import instead of being reset to a default.
    [AvaloniaFact]
    public async Task ImportFromSe4XmlKeepsSettingsSe4HasNoCounterpartFor()
    {
        var (vm, target) = await Load(await WriteXml(Se4SettingsXml));
        target.General.PromptBeforeDelete = true;
        target.Waveform.WaveformShotChangeColor = "#FF123456";
        target.AutoTranslate.NvidiaApiKey = "keep-me";

        await ImportInto(vm, target, v => v.ExportImportAll = true);

        Assert.True(target.General.PromptBeforeDelete);
        Assert.Equal("#FF123456", target.Waveform.WaveformShotChangeColor);
        Assert.Equal("keep-me", target.AutoTranslate.NvidiaApiKey);
    }

    [AvaloniaFact]
    public async Task ImportFromSe4XmlAppliesOnlyTheTickedCategories()
    {
        var (vm, target) = await Load(await WriteXml(Se4SettingsXml));

        await ImportInto(vm, target, v =>
        {
            v.ExportImportAll = false;
            v.ExportImportWaveform = true;
        });

        Assert.True(target.Waveform.DrawGridLines);
        Assert.Equal(new Se().General.SubtitleLineMaximumLength, target.General.SubtitleLineMaximumLength);
        Assert.Equal(new Se().Appearance.Theme, target.Appearance.Theme);
        Assert.Empty(target.Shortcuts);
    }

    // Same rule as a partial JSON export: a checkbox the file cannot fill is greyed out.
    [AvaloniaFact]
    public async Task LoadingAnSe4XmlDisablesTheCategoriesItDoesNotCarry()
    {
        var (vm, _) = await Load(await WriteXml(
            "<Settings><VideoControls><WaveformDrawGrid>True</WaveformDrawGrid></VideoControls></Settings>"));

        Assert.True(vm.IsWaveformEnabled);
        Assert.False(vm.IsRulesEnabled);
        Assert.False(vm.IsAppearanceEnabled);
        Assert.False(vm.IsAutoTranslateEnabled);
        Assert.False(vm.IsShortcutsEnabled);
    }

    // The picker offers both file types, so the branch is decided by what is in the file - a
    // Settings.xml renamed to .json still has to import as SE 4.
    [AvaloniaFact]
    public async Task AnSe4XmlIsRecognizedByItsContentNotItsExtension()
    {
        var (vm, target) = await Load(await WriteXml(Se4SettingsXml, ".json"));

        await ImportInto(vm, target, v => v.ExportImportAll = true);

        Assert.Equal(37, target.General.SubtitleLineMaximumLength);
    }

    [AvaloniaFact]
    public async Task LoadingAFileThatIsNeitherFormatFails()
    {
        var path = await WriteXml("<NotSubtitleEditSettings><Foo>1</Foo></NotSubtitleEditSettings>");
        var vm = new SettingsImportExportViewModel(new FakeFileHelper(path)) { Window = new Window() };
        vm.SetIsExport(false);

        Assert.False(await vm.PromptAndLoadImportFile());
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.GridColumns;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Text.Json;

namespace UITests.Features.Options.Settings;

/// <summary>
/// "All settings" export/import skipped whole sections - the Multiple Replace rules (Edit), the
/// stored ASSA styles (Assa), the OCR options and more - and the Columns dialog's Reset only
/// restored the order, not which columns are shown (#15944).
/// </summary>
public class SettingsImportExportAllSectionsTests
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

    private sealed class ImportDialogService(string path, Func<SettingsViewModel, Task> useSettings) : IWindowService
    {
        public async Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window where TViewModel : class
        {
            if (typeof(TViewModel) == typeof(SettingsViewModel))
            {
                var settings = new SettingsViewModel(this, new FolderHelper()) { Window = new Window() };
                var viewModel = Assert.IsType<TViewModel>(settings);
                configureViewModel?.Invoke(viewModel);
                await useSettings(settings);
                return viewModel;
            }

            if (typeof(TViewModel) == typeof(SettingsResetViewModel))
            {
                var reset = new SettingsResetViewModel { ResetAll = false, ResetAppearance = true };
                reset.OkCommand.Execute(null);
                return Assert.IsType<TViewModel>(reset);
            }

            var import = new SettingsImportExportViewModel(new FakeFileHelper(path)) { Window = new Window() };
            var importViewModel = Assert.IsType<TViewModel>(import);
            configureViewModel?.Invoke(importViewModel);
            Assert.True(await import.PromptAndLoadImportFile());
            import.ExportImportAppearance = true;
            await import.OkCommand.ExecuteAsync(null);
            return importViewModel;
        }

        public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window
            => throw new NotSupportedException();

        public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class
            => throw new NotSupportedException();

        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class
            => throw new NotSupportedException();

        public Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
            => throw new NotSupportedException();

        public Task<TViewModel> ShowWithOwnerHiddenAsync<TWindow, TViewModel>(
            Window owner,
            IReadOnlyList<Window?> companions,
            Action<TViewModel>? configureViewModel = null)
            where TWindow : Window where TViewModel : class
            => throw new NotSupportedException();
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

    private static Se MakeCustomizedSettings()
    {
        var settings = new Se();
        settings.Edit.MultipleReplace.Categories.Add(new SeEditMultipleReplace.MultipleReplaceCategory { Name = "My rules", IsActive = true });
        var style = SeAssaStyle.MakeStorageDefault();
        style.Name = "My style";
        settings.Assa.StoredStyles.Add(style);
        settings.Ocr.NOcrPixelsAreSpace = 7;
        settings.Ocr.DoTryToGuessUnknownWords = false;
        settings.File.ExportDvbTeletextPageNumber = 777;
        settings.File.RecentFiles.Add(new RecentFile { SubtitleFileName = "source-recent.srt" });
        return settings;
    }

    private static Se MakeTargetSettings()
    {
        var settings = new Se();
        settings.File.RecentFiles.Add(new RecentFile { SubtitleFileName = "target-recent.srt" });
        return settings;
    }

    [AvaloniaFact]
    public async Task ExportImportAllCarriesMultipleReplaceAssaStylesAndOcr()
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();

        var json = await ExportAll(path, MakeCustomizedSettings());
        Assert.DoesNotContain("source-recent.srt", json);

        var target = MakeTargetSettings();
        await ImportAllInto(path, target);

        Assert.Contains(target.Edit.MultipleReplace.Categories, c => c.Name == "My rules");
        Assert.Contains(target.Assa.StoredStyles, s => s.Name == "My style");
        Assert.Equal(7, target.Ocr.NOcrPixelsAreSpace);
        Assert.False(target.Ocr.DoTryToGuessUnknownWords);
        Assert.Equal(777, target.File.ExportDvbTeletextPageNumber);
        Assert.Equal("target-recent.srt", Assert.Single(target.File.RecentFiles).SubtitleFileName);
    }

    [AvaloniaFact]
    public async Task ImportOfPlainSettingsBackupCarriesAllSections()
    {
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(MakeCustomizedSettings()));

        var target = MakeTargetSettings();
        await ImportAllInto(path, target);

        Assert.Contains(target.Edit.MultipleReplace.Categories, c => c.Name == "My rules");
        Assert.Contains(target.Assa.StoredStyles, s => s.Name == "My style");
        Assert.Equal(7, target.Ocr.NOcrPixelsAreSpace);
        Assert.Equal("target-recent.srt", Assert.Single(target.File.RecentFiles).SubtitleFileName);
    }

    [AvaloniaFact]
    public async Task ImportOfOlderExportLeavesTheNewSectionsAlone()
    {
        // Exports from before #15944 carry the source-OS marker but serialized these sections as
        // defaults - importing them must not wipe the user's own rules and styles.
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();
        await File.WriteAllTextAsync(path, """
            {
              "exportSourceOs": "Windows",
              "Edit": { "MultipleReplace": { "Categories": [] } },
              "Ocr": { "NOcrPixelsAreSpace": 0 }
            }
            """);

        var target = MakeCustomizedSettings();
        await ImportAllInto(path, target);

        Assert.Contains(target.Edit.MultipleReplace.Categories, c => c.Name == "My rules");
        Assert.Equal(7, target.Ocr.NOcrPixelsAreSpace);
    }

    [Fact]
    public void GridColumnsResetRestoresDefaultVisibility()
    {
        var entries = new List<GridColumnDisplay>
        {
            new("Start", true, false, "start") { DefaultIsVisible = true },
            new("Text", false, true, "text"),
            new("Cps", true, true, "cps") { DefaultIsVisible = false },
        };

        var vm = new GridColumnsViewModel();
        vm.Initialize(entries, new List<string> { "cps", "text", "start" });
        vm.ResetCommand.Execute(null);

        Assert.Equal(new[] { "start", "text", "cps" }, vm.Columns.SelectMany(c => c.Keys));
        Assert.True(vm.Columns[0].IsVisible);
        Assert.True(vm.Columns[1].IsVisible);
        Assert.False(vm.Columns[2].IsVisible);
    }

    public enum LayoutChange
    {
        ConfirmImport,
        ApplyImport,
        ApplyImportThenOtherChange,
        ResetAppearance,
    }

    [AvaloniaTheory]
    [InlineData(LayoutChange.ConfirmImport)]
    [InlineData(LayoutChange.ApplyImport)]
    [InlineData(LayoutChange.ApplyImportThenOtherChange)]
    [InlineData(LayoutChange.ResetAppearance)]
    public async Task SettingsDialogRestoresImportedAndResetLayoutSizes(LayoutChange change)
    {
        const double importedHeight = 317;
        const double resizedHeight = 241;
        var resetAppearance = change == LayoutChange.ResetAppearance;
        Directory.CreateDirectory(SettingsIsolationFixture.SettingsDirectory);
        var path = TempFile();
        var export = new Se();
        export.Appearance.CurrentLayoutPositions.RowHeights.Add(importedHeight);
        export.Appearance.CurrentLayoutPositions.RowUnitTypes.Add(GridUnitType.Pixel);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(export));

        var previousSettings = Se.Settings;
        var previousServices = Locator.Services;
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        MainViewModel? main = null;
        Grid? appliedLayout = null;
        var expectedHeight = importedHeight;
        services.AddSingleton<IWindowService>(new ImportDialogService(path, async settings =>
        {
            if (resetAppearance)
            {
                await settings.ResetAllSettingsCommand.ExecuteAsync(null);
                return;
            }

            await settings.ImportSettingsCommand.ExecuteAsync(null);
            Assert.True(settings.SettingsImported);
            if (change is LayoutChange.ApplyImport or LayoutChange.ApplyImportThenOtherChange)
            {
                await settings.ApplyCommand.ExecuteAsync(null);
                Dispatcher.UIThread.RunJobs();
                appliedLayout = Assert.IsType<Grid>(Assert.Single(main!.ContentGrid.Children));
                Assert.Equal(new GridLength(importedHeight), appliedLayout.RowDefinitions[0].Height);
            }

            if (change == LayoutChange.ApplyImportThenOtherChange)
            {
                var row = appliedLayout!.RowDefinitions[0];
                row.Height = new GridLength(resizedHeight);
                Dispatcher.UIThread.RunJobs();
                // An ordinary Apply captures the measured height, which can be rounded by layout.
                expectedHeight = row.ActualHeight;
                Assert.InRange(expectedHeight, resizedHeight - 2, resizedHeight + 2);
                settings.ShowUpDownDuration = !settings.ShowUpDownDuration;
                await settings.ApplyCommand.ExecuteAsync(null);
                appliedLayout = Assert.IsType<Grid>(Assert.Single(main!.ContentGrid.Children));
                Assert.Equal(new GridLength(expectedHeight), appliedLayout.RowDefinitions[0].Height);
            }

            await settings.CommandOkCommand.ExecuteAsync(null);
        }));
        using var provider = services.BuildServiceProvider();
        Se.Settings = resetAppearance ? export : new Se();
        var window = new Window { Width = 1200, Height = 800 };
        try
        {
            Locator.Services = provider;

            MainView.NextHostWindow = window;
            var view = new MainView();
            window.Content = view;
            window.Show();
            Dispatcher.UIThread.RunJobs();
            main = Assert.IsType<MainViewModel>(view.DataContext);
            var originalLayout = Assert.IsType<Grid>(Assert.Single(main.ContentGrid.Children));
            if (resetAppearance)
            {
                originalLayout.RowDefinitions[0].Height = new GridLength(importedHeight);
                Dispatcher.UIThread.RunJobs();
            }
            else
            {
                Assert.NotEqual(new GridLength(importedHeight), originalLayout.RowDefinitions[0].Height);
            }

            await main.CommandShowSettingsCommand.ExecuteAsync(null);
            Dispatcher.UIThread.RunJobs();

            var layout = Assert.IsType<Grid>(Assert.Single(main.ContentGrid.Children));
            Assert.NotSame(originalLayout, layout);
            if (resetAppearance)
            {
                Assert.True(layout.RowDefinitions[0].Height.IsStar);
            }
            else
            {
                Assert.Equal(new GridLength(expectedHeight), layout.RowDefinitions[0].Height);
            }

            if (appliedLayout != null)
            {
                Assert.Same(appliedLayout, layout);
            }
        }
        finally
        {
            if (main != null)
            {
                window.SuppressSaveChangesPromptOnClose(main);
            }

            window.Close();
            Se.Settings = previousSettings;
            Locator.Services = previousServices;
        }
    }
}

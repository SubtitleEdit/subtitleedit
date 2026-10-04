using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Ssa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Ssa;

public class SsaStylesViewModelTests
{
    private static SsaStylesViewModel MakeInitializedVm()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<SsaStylesViewModel>();

        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            new() { new SsaStyle { Name = "Default" }, new SsaStyle { Name = "Top" } });
        var subtitle = new Subtitle { Header = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, string.Empty) };
        subtitle.Paragraphs.Add(new Paragraph("a", 0, 1000) { Extra = "Default" });
        vm.Initialize(subtitle, new SubStationAlpha(), "test.ssa", "Default", null);
        return vm;
    }

    /// <summary>
    /// "Clear" on the file styles also lets go of the current style - the style editor must not
    /// keep editing a style that is no longer in the list.
    /// </summary>
    [AvaloniaFact]
    public async Task FileRemoveAll_ClearsFileStylesAndCurrentStyle()
    {
        var vm = MakeInitializedVm();
        Assert.Equal(2, vm.FileStyles.Count);
        Assert.NotNull(vm.CurrentStyle);

        await vm.FileRemoveAllCommand.ExecuteAsync(null);

        Assert.Empty(vm.FileStyles);
        Assert.Null(vm.CurrentStyle);
        Assert.Null(vm.SelectedFileStyle);
        vm.OnClosingCleanup();
    }

    /// <summary>
    /// Answers the style picker the SSA styles window opens (import, replace with): the
    /// <c>pick</c> callback chooses in the picker, then OK is pressed.
    /// </summary>
    private sealed class PickerWindowService(Action<AssaStylePickerViewModel> pick) : IWindowService
    {
        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window where TViewModel : class
        {
            var picker = new AssaStylePickerViewModel();
            var vm = (TViewModel)(object)picker;
            configureViewModel?.Invoke(vm);
            pick(picker);
            picker.OkCommand.Execute(null);
            return Task.FromResult(vm);
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

    private sealed class OpenFileHelper(string fileName) : StubFileHelper
    {
        public override Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "", string? suggestedStartFolder = null)
            => Task.FromResult(fileName);
    }

    private static string MakeSsaHeader(params SsaStyle[] styles)
    {
        var assaHeader = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader, styles.ToList());
        return SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(assaHeader, string.Empty);
    }

    /// <summary>
    /// File styles "Default" (2 lines) and "Top" (1 line), no storage styles, shown in a window
    /// with both style grids so commands that work on the grid selection can run.
    /// </summary>
    private static (SsaStylesViewModel Vm, Window Window) ShowVm(IFileHelper? fileHelper = null, IWindowService? windowService = null)
    {
        var vm = new SsaStylesViewModel(fileHelper ?? new StubFileHelper(), windowService ?? new StubWindowService());
        vm.StorageStyles.Clear();

        var subtitle = new Subtitle
        {
            Header = MakeSsaHeader(new SsaStyle { Name = "Default", FontSize = 20 }, new SsaStyle { Name = "Top", FontSize = 20 }),
        };
        subtitle.Paragraphs.Add(new Paragraph("one", 0, 1000) { Extra = "Default" });
        subtitle.Paragraphs.Add(new Paragraph("two", 1000, 2000) { Extra = "Default" });
        subtitle.Paragraphs.Add(new Paragraph("three", 2000, 3000) { Extra = "Top" });
        vm.Initialize(subtitle, new SubStationAlpha(), "test.ssa", "Default", null);

        foreach (var grid in new[] { vm.FileStyleGrid, vm.StorageStyleGrid })
        {
            grid.Columns.Add(new TableViewColumn { Header = "Name", Binding = new Avalonia.Data.Binding(nameof(StyleDisplay.Name)) });
        }

        vm.FileStyleGrid.ItemsSource = vm.FileStyles;
        vm.StorageStyleGrid.ItemsSource = vm.StorageStyles;
        var panel = new StackPanel();
        panel.Children.Add(vm.FileStyleGrid);
        panel.Children.Add(vm.StorageStyleGrid);
        var window = new Window { Width = 400, Height = 600, Content = panel };
        window.Show();
        vm.Window = window;
        return (vm, window);
    }

    private static void CloseAll(SsaStylesViewModel vm, Window window)
    {
        vm.OnClosingCleanup();
        foreach (var owned in window.OwnedWindows.ToArray())
        {
            owned.Close();
        }

        window.Close();
    }

    /// <summary>
    /// Runs a command that stops at the overwrite / keep both prompt and answers it by clicking
    /// the button with <paramref name="buttonText"/>.
    /// </summary>
    private static async Task RunAnsweringOverwritePrompt(Window window, Func<Task> command, string buttonText)
    {
        var task = command();
        Dispatcher.UIThread.RunJobs();

        var messageBox = Assert.Single(window.OwnedWindows.OfType<MessageBox>());
        var button = messageBox.GetVisualDescendants().OfType<Button>().Single(b => Equals(b.Content, buttonText));
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        await task;
    }

    private static string WriteImportFile(params SsaStyle[] styles)
    {
        var subtitle = new Subtitle { Header = MakeSsaHeader(styles) };
        subtitle.Paragraphs.Add(new Paragraph("x", 0, 1000) { Extra = styles[0].Name });
        var fileName = Path.Combine(Path.GetTempPath(), "se-ssa-styles-import-" + Guid.NewGuid() + ".ssa");
        File.WriteAllText(fileName, subtitle.ToText(new SubStationAlpha()));
        return fileName;
    }

    /// <summary>
    /// "Replace style with..." picking another file style re-points the replaced style's lines
    /// to it, removes the replaced style and makes the target the current style.
    /// </summary>
    [AvaloniaFact]
    public async Task FileReplaceWith_FileStyle_RepointsLinesAndRemovesReplacedStyle()
    {
        var offered = new List<string>();
        var (vm, window) = ShowVm(windowService: new PickerWindowService(picker =>
        {
            offered.AddRange(picker.Styles.Select(p => p.Name));
            picker.SelectedStyle = picker.Styles.Single(p => p.Name == "Default");
        }));
        try
        {
            vm.FileStyleGrid.SelectedItem = vm.FileStyles.Single(p => p.Name == "Top");

            await vm.FileReplaceWithCommand.ExecuteAsync(null);

            Assert.Equal(new[] { "Default" }, offered);
            var defaultStyle = Assert.Single(vm.FileStyles);
            Assert.Equal("Default", defaultStyle.Name);
            Assert.All(vm.ResultSubtitle.Paragraphs, p => Assert.Equal("Default", p.Extra));
            Assert.Equal(3, defaultStyle.UsageCount);
            Assert.Same(defaultStyle, vm.SelectedFileStyle);
            Assert.Same(defaultStyle, vm.CurrentStyle);
        }
        finally
        {
            CloseAll(vm, window);
        }
    }

    /// <summary>
    /// "Replace style with..." also offers storage styles not in the file; picking one adds it
    /// to the file styles with its formatting.
    /// </summary>
    [AvaloniaFact]
    public async Task FileReplaceWith_StorageStyle_AddsItToFileStyles()
    {
        var offered = new List<string>();
        var (vm, window) = ShowVm(windowService: new PickerWindowService(picker =>
        {
            offered.AddRange(picker.Styles.Select(p => p.Name));
            picker.SelectedStyle = picker.Styles.Single(p => p.Name == "Signs");
        }));
        try
        {
            vm.StorageStyles.Add(new StyleDisplay(new SsaStyle { Name = "Signs", FontSize = 42 }));
            vm.StorageStyles.Add(new StyleDisplay(new SsaStyle { Name = "default", FontSize = 30 })); // already in file
            vm.FileStyleGrid.SelectedItem = vm.FileStyles.Single(p => p.Name == "Default");

            await vm.FileReplaceWithCommand.ExecuteAsync(null);

            Assert.Equal(new[] { "Top", "Signs" }, offered);
            Assert.Equal(new[] { "Top", "Signs" }, vm.FileStyles.Select(p => p.Name));
            var signs = vm.FileStyles.Single(p => p.Name == "Signs");
            Assert.Equal(42, signs.FontSize);
            Assert.Equal(new[] { "Signs", "Signs", "Top" }, vm.ResultSubtitle.Paragraphs.Select(p => p.Extra));
            Assert.Equal(2, signs.UsageCount);
            Assert.Same(signs, vm.CurrentStyle);
            Assert.Equal(2, vm.StorageStyles.Count);
        }
        finally
        {
            CloseAll(vm, window);
        }
    }

    /// <summary>
    /// Importing a style with the name of a file style and answering "Overwrite" updates the
    /// existing file style in place - same instance, same name, lines still pointing at it -
    /// instead of adding a "_2" copy (#15312). Styles without a clash are added.
    /// </summary>
    [AvaloniaFact]
    public async Task FileImport_Overwrite_UpdatesExistingFileStyleInPlace()
    {
        var fileName = WriteImportFile(
            new SsaStyle { Name = "top", FontSize = 55, Bold = true },
            new SsaStyle { Name = "Signs", FontSize = 33 });
        var (vm, window) = ShowVm(new OpenFileHelper(fileName), new PickerWindowService(_ => { }));
        try
        {
            var top = vm.FileStyles.Single(p => p.Name == "Top");

            await RunAnsweringOverwritePrompt(window, () => vm.FileImportCommand.ExecuteAsync(null), Se.Language.Assa.Overwrite);

            Assert.Equal(new[] { "Default", "Top", "Signs" }, vm.FileStyles.Select(p => p.Name));
            Assert.Same(top, vm.FileStyles[1]);
            Assert.Equal(55, top.FontSize);
            Assert.True(top.Bold);
            Assert.Equal(1, top.UsageCount);
            Assert.Equal("Top", vm.ResultSubtitle.Paragraphs[2].Extra);
            Assert.Equal(33, vm.FileStyles[2].FontSize);
        }
        finally
        {
            CloseAll(vm, window);
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public async Task FileImport_KeepBoth_AddsRenamedCopy()
    {
        var fileName = WriteImportFile(new SsaStyle { Name = "Top", FontSize = 55 });
        var (vm, window) = ShowVm(new OpenFileHelper(fileName), new PickerWindowService(_ => { }));
        try
        {
            var top = vm.FileStyles.Single(p => p.Name == "Top");

            await RunAnsweringOverwritePrompt(window, () => vm.FileImportCommand.ExecuteAsync(null), Se.Language.Assa.KeepBoth);

            Assert.Equal(new[] { "Default", "Top", "Top_2" }, vm.FileStyles.Select(p => p.Name));
            Assert.Equal(20, top.FontSize);
            Assert.Equal(55, vm.FileStyles[2].FontSize);
        }
        finally
        {
            CloseAll(vm, window);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// Storage import uses the same overwrite prompt, against the storage styles.
    /// </summary>
    [AvaloniaFact]
    public async Task StorageImport_Overwrite_UpdatesExistingStorageStyleInPlace()
    {
        var fileName = WriteImportFile(new SsaStyle { Name = "Signs", FontSize = 55 });
        var (vm, window) = ShowVm(new OpenFileHelper(fileName), new PickerWindowService(_ => { }));
        try
        {
            var stored = new StyleDisplay(new SsaStyle { Name = "Signs", FontSize = 20 });
            vm.StorageStyles.Add(stored);

            await RunAnsweringOverwritePrompt(window, () => vm.StorageImportCommand.ExecuteAsync(null), Se.Language.Assa.Overwrite);

            Assert.Same(stored, Assert.Single(vm.StorageStyles));
            Assert.Equal(55, stored.FontSize);
            Assert.Equal(2, vm.FileStyles.Count);
        }
        finally
        {
            CloseAll(vm, window);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// "Copy to storage" adds a copy of the selected file style - a separate instance, so editing
    /// the storage copy does not change the file style.
    /// </summary>
    [AvaloniaFact]
    public async Task FileCopyToStorage_AddsIndependentCopy()
    {
        var (vm, window) = ShowVm();
        try
        {
            var top = vm.FileStyles.Single(p => p.Name == "Top");
            top.FontSize = 42;
            vm.FileStyleGrid.SelectedItem = top;

            await vm.FileCopyToStorageCommand.ExecuteAsync(null);

            var copy = Assert.Single(vm.StorageStyles);
            Assert.NotSame(top, copy);
            Assert.Equal("Top", copy.Name);
            Assert.Equal(42, copy.FontSize);

            copy.FontSize = 10;
            Assert.Equal(42, top.FontSize);
            Assert.Equal(2, vm.FileStyles.Count);
        }
        finally
        {
            CloseAll(vm, window);
        }
    }

    /// <summary>
    /// "Copy to file styles" adds the selected storage style to the file styles, with usage
    /// counted for lines that already use that style name.
    /// </summary>
    [AvaloniaFact]
    public async Task StorageCopyToFiles_AddsStyleAndCountsUsage()
    {
        var (vm, window) = ShowVm();
        try
        {
            vm.ResultSubtitle.Paragraphs.Add(new Paragraph("four", 3000, 4000) { Extra = "Signs" });
            var signs = new StyleDisplay(new SsaStyle { Name = "Signs", FontSize = 42 });
            vm.StorageStyles.Add(signs);
            vm.StorageStyleGrid.SelectedItem = signs;

            await vm.StorageCopyToFilesCommand.ExecuteAsync(null);

            Assert.Equal(new[] { "Default", "Top", "Signs" }, vm.FileStyles.Select(p => p.Name));
            var copy = vm.FileStyles[2];
            Assert.NotSame(signs, copy);
            Assert.Equal(42, copy.FontSize);
            Assert.Equal(1, copy.UsageCount);
            Assert.Same(signs, Assert.Single(vm.StorageStyles));
        }
        finally
        {
            CloseAll(vm, window);
        }
    }

    /// <summary>
    /// "Copy to file styles" onto the current file style with "Overwrite" takes the storage
    /// style's formatting - and the border type combo (not bound to the style) follows it.
    /// </summary>
    [AvaloniaFact]
    public async Task StorageCopyToFiles_OverwriteCurrentStyle_UpdatesBorderType()
    {
        var (vm, window) = ShowVm();
        try
        {
            var current = vm.CurrentStyle!;
            Assert.Equal("Default", current.Name);
            Assert.Equal(BorderStyleType.Outline, vm.SelectedBorderType.Style);

            var stored = new StyleDisplay(new SsaStyle { Name = "Default", FontSize = 42, BorderStyle = "3" });
            vm.StorageStyles.Add(stored);
            vm.StorageStyleGrid.SelectedItem = stored;

            await RunAnsweringOverwritePrompt(window, () => vm.StorageCopyToFilesCommand.ExecuteAsync(null), Se.Language.Assa.Overwrite);

            Assert.Equal(2, vm.FileStyles.Count);
            Assert.Same(current, vm.FileStyles[0]);
            Assert.Equal(42, current.FontSize);
            Assert.Equal(BorderStyleType.BoxPerLine, current.BorderStyle.Style);
            Assert.Equal(BorderStyleType.BoxPerLine, vm.SelectedBorderType.Style);
            Assert.Equal(2, current.UsageCount);
        }
        finally
        {
            CloseAll(vm, window);
        }
    }
}

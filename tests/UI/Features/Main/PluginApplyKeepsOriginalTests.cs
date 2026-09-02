using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Plugins;

namespace UITests.Features.Main;

/// <summary>
/// Applying a plugin's result rebuilt the grid without the original, so the "Original text" column
/// went blank for every line - and with the original captured from a translation, that text lived
/// nowhere the rows were re-filled from. The original was then reported as changed, and the user
/// was asked to save an original with no text at all (issue #14445).
/// </summary>
public class PluginApplyKeepsOriginalTests
{
    private const string PluginResult =
        "1\r\n00:00:00,000 --> 00:00:02,000\r\nVertaalde regel 'een'\r\n\r\n" +
        "2\r\n00:00:02,000 --> 00:00:04,000\r\nVertaalde regel 'twee'\r\n\r\n";

    [AvaloniaFact]
    public async Task ApplyPluginSubtitle_KeepsTheOriginalOfATranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel ‘een’", "Translated line one", 0, 2000);
            AddLine(vm, "Vertaalde regel ‘twee’", "Translated line two", 2000, 4000);
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            Invoke(vm, "CaptureOriginalFromTranslatedRows");
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            var applied = await InvokeApplyPluginSubtitle(vm, PluginResult);

            Assert.True(applied);
            Assert.Equal(
                new[] { "Vertaalde regel 'een'", "Vertaalde regel 'twee'" },
                vm.Subtitles.Select(p => p.Text));
            Assert.Equal(
                new[] { "Translated line one", "Translated line two" },
                vm.Subtitles.Select(p => p.OriginalText));

            // The plugin touched the translation only, so the original is not a pending change.
            var originalHash = (int)GetField("_changeSubtitleHashOriginal").GetValue(vm)!;
            Assert.Equal(originalHash, vm.GetFastHashOriginal());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A read-only reference showing its non-matching lines: the display-only row comes back after
    /// the rebuild, and the matched rows keep the reference text they display.
    /// </summary>
    [AvaloniaFact]
    public async Task ApplyPluginSubtitle_KeepsAReferenceAndItsUnmatchedRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel ‘een’", string.Empty, 0, 2000);
            AddLine(vm, "Vertaalde regel ‘twee’", string.Empty, 2000, 4000);
            ImportReference(vm);
            Assert.Equal(3, vm.Subtitles.Count);
            Assert.True(vm.IsShowingOriginalNonMatchingLines);

            var applied = await InvokeApplyPluginSubtitle(vm, PluginResult);

            Assert.True(applied);
            Assert.True(vm.IsShowingOriginalNonMatchingLines);
            Assert.Equal(
                new[] { "Vertaalde regel 'een'", "Vertaalde regel 'twee'", string.Empty },
                vm.Subtitles.Select(p => p.Text));
            Assert.Equal(
                new[] { "Reference one", "Reference two", "Reference only - no translation" },
                vm.Subtitles.Select(p => p.OriginalText));
            Assert.Equal(new[] { false, false, true }, vm.Subtitles.Select(p => p.IsReferenceOnly));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The plugin receives the working subtitle without the display-only rows, so the selected
    /// indices it is told about must skip them too - a grid position would point one line too far
    /// for every reference row above the selection.
    /// </summary>
    [AvaloniaFact]
    public void GetSelectedSubtitleIndices_SkipsReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", string.Empty, 0, 2000);
            AddLine(vm, "Vertaalde regel twee", string.Empty, 2000, 4000);
            AddLine(vm, "Vertaalde regel drie", string.Empty, 6000, 8000);
            ImportReference(vm);
            Assert.Equal(new[] { false, false, true, false }, vm.Subtitles.Select(p => p.IsReferenceOnly));

            // The last working line sits at grid position 3, but is line 2 of the working subtitle.
            Dispatcher.UIThread.RunJobs(); // let the import's own select-first-row job land first
            vm.SubtitleGrid.SelectedItem = vm.Subtitles[3];
            Dispatcher.UIThread.RunJobs();

            var indices = (List<int>)typeof(MainViewModel)
                .GetMethod("GetSelectedSubtitleIndices", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(vm, null)!;

            Assert.Equal(new[] { 2 }, indices);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static void ImportReference(MainViewModel vm)
    {
        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 4000, 6000));

        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");

        method.Invoke(vm, new object?[] { 0, "reference.srt", reference, match, true });
    }

    private static async Task<bool> InvokeApplyPluginSubtitle(MainViewModel vm, string subRip)
    {
        var plugin = new InstalledPlugin
        {
            Manifest = new PluginManifest { Name = "Test plugin" },
            FolderPath = string.Empty,
            ManifestPath = string.Empty,
        };

        var response = new PluginResponse
        {
            Status = PluginConstants.StatusOk,
            Subtitle = new PluginSubtitle { Format = "SubRip", Native = subRip, SubRip = subRip },
        };

        var method = typeof(MainViewModel).GetMethod(
                         "ApplyPluginSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ApplyPluginSubtitle not found");

        return await (Task<bool>)method.Invoke(vm, new object[] { plugin, response })!;
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static SubtitleLineViewModel AddLine(
        MainViewModel vm, string text, string originalText, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        // The apply and the import both post a select-and-scroll job; drain it while the window
        // is still open, or it runs inside whichever test pumps the shared dispatcher next.
        Dispatcher.UIThread.RunJobs();

        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }

    private static void Invoke(MainViewModel vm, string name)
    {
        var method = typeof(MainViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException($"{name} not found");

        method.Invoke(vm, null);
    }

    private static void SetPrivateField(MainViewModel vm, string name, object value)
    {
        GetField(name).SetValue(vm, value);
    }

    private static FieldInfo GetField(string name)
    {
        return typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException($"Field not found: {name}");
    }
}

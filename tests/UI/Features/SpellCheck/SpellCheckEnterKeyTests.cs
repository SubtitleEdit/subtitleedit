using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System.Collections.ObjectModel;

namespace UITests.Features.SpellCheck;

/// <summary>
/// Enter in spell check reached Done, the window's default button, so typing a correction and
/// pressing Enter closed spell check instead of changing the word. Enter in the word box now
/// changes the word, and Enter in the suggestions uses the selected one once.
/// </summary>
public class SpellCheckEnterKeyTests : IDisposable
{
    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public SpellCheckEnterKeyTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeSpellCheckEnter_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        var repoRoot = FindRepoRoot();
        File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.dic"), Path.Combine(_tempDictionariesFolder, "en_US.dic"));
        File.Copy(Path.Combine(repoRoot, "Dictionaries", "en_US.aff"), Path.Combine(_tempDictionariesFolder, "en_US.aff"));

        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
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

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Dictionaries")))
        {
            dir = dir.Parent;
        }

        return dir!.FullName;
    }

    private sealed class NullFocusSubtitleLine : IFocusSubtitleLine
    {
        public void GoToAndFocusLine(SubtitleLineViewModel p)
        {
        }
    }

    /// <summary>
    /// Opens spell check on a line with two misspelled words and waits until it stops on the first -
    /// the second keeps the window open after the first is changed (the last change finishes the check).
    /// </summary>
    private (SpellCheckWindow Window, SpellCheckViewModel Vm, SubtitleLineViewModel Line) OpenOnMisspelledWord()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var dictionary = new SpellCheckDictionaryDisplay
        {
            Name = "English",
            DictionaryFileName = Path.Combine(_tempDictionariesFolder, "en_US.dic"),
        };
        var vm = Locator.Services.GetRequiredService<SpellCheckViewModel>();
        vm.Dictionaries.Add(dictionary);

        var line = new SubtitleLineViewModel { Text = "The hosue is bigg." };
        var window = new SpellCheckWindow(vm);
        vm.Initialize(new ObservableCollection<SubtitleLineViewModel> { line }, 0, new NullFocusSubtitleLine(), dictionary);
        window.Show();

        for (var i = 0; i < 100 && vm.WordNotFoundOriginal != "hosue"; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Thread.Sleep(20);
        }

        Assert.Equal("hosue", vm.WordNotFoundOriginal);
        return (window, vm, line);
    }

    private static void PressEnter(Window window)
    {
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void EnterInWordBox_ChangesTheWord_AndKeepsTheWindowOpen()
    {
        var (window, vm, line) = OpenOnMisspelledWord();
        var closed = false;
        window.Closed += (_, _) => closed = true;
        try
        {
            vm.CurrentWord = "house";
            vm.TextBoxWordNotFound.Focus();
            Dispatcher.UIThread.RunJobs();

            PressEnter(window);

            Assert.False(closed);
            Assert.Equal("The house is bigg.", line.Text);
        }
        finally
        {
            if (!closed)
            {
                window.Close();
            }
        }
    }

    [AvaloniaFact]
    public void EnterInSuggestions_UsesTheSelectedSuggestion_AndKeepsTheWindowOpen()
    {
        var (window, vm, line) = OpenOnMisspelledWord();
        var closed = false;
        window.Closed += (_, _) => closed = true;
        try
        {
            Assert.Contains("house", vm.Suggestions);
            var listBox = window.GetVisualDescendants().OfType<ListBox>().Single(l => l.ItemsSource == vm.Suggestions);
            vm.SelectedSuggestion = "house";
            Dispatcher.UIThread.RunJobs();
            var item = listBox.ContainerFromItem("house")!;
            item.Focus();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(item, window.FocusManager?.GetFocusedElement());

            PressEnter(window);

            Assert.False(closed);
            Assert.Equal("The house is bigg.", line.Text);
        }
        finally
        {
            if (!closed)
            {
                window.Close();
            }
        }
    }
}

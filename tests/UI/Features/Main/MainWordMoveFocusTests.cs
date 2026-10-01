using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Main;

/// <summary>
/// The word-moving shortcuts follow the focused text box, as in SE 4: with the caret in an
/// editable original text box they move words in the original text, otherwise in the working
/// (translation) text. A read-only original can hold focus but is never rewritten. SE 4's
/// "Move first word to previous subtitle" is back as well (#14515).
/// </summary>
public class MainWordMoveFocusTests
{
    [AvaloniaFact]
    public void FetchFirstWordFromNext_OriginalFocused_MovesOriginalWordOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello", "Hej");
            var second = AddLine(vm, "big world", "store verden", 2000, 4000);
            vm.SelectedSubtitle = first;
            FocusOriginal(vm, editable: true);

            vm.FetchFirstWordFromNextSubtitleCommand.Execute(null);

            Assert.Equal("Hej store", first.OriginalText);
            Assert.Equal("verden", second.OriginalText);
            Assert.Equal("Hello", first.Text);
            Assert.Equal("big world", second.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void FetchFirstWordFromNext_TranslationFocused_MovesTranslationWordOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello", "Hej");
            var second = AddLine(vm, "big world", "store verden", 2000, 4000);
            vm.SelectedSubtitle = first;
            FocusOriginal(vm, editable: true, focused: false);

            vm.FetchFirstWordFromNextSubtitleCommand.Execute(null);

            Assert.Equal("Hello big", first.Text);
            Assert.Equal("world", second.Text);
            Assert.Equal("Hej", first.OriginalText);
            Assert.Equal("store verden", second.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void FetchFirstWordFromNext_ReadOnlyOriginalFocused_FallsBackToTranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello", "Hej");
            var second = AddLine(vm, "big world", "store verden", 2000, 4000);
            vm.SelectedSubtitle = first;
            FocusOriginal(vm, editable: false);

            vm.FetchFirstWordFromNextSubtitleCommand.Execute(null);

            Assert.Equal("Hello big", first.Text);
            Assert.Equal("world", second.Text);
            Assert.Equal("Hej", first.OriginalText);
            Assert.Equal("store verden", second.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MoveLastWordToNext_OriginalFocused_MovesOriginalWordOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello big", "Hej store");
            var second = AddLine(vm, "world", "verden", 2000, 4000);
            vm.SelectedSubtitle = first;
            FocusOriginal(vm, editable: true);

            vm.MoveLastWordToNextSubtitleCommand.Execute(null);

            Assert.Equal("Hej", first.OriginalText);
            Assert.Equal("store verden", second.OriginalText);
            Assert.Equal("Hello big", first.Text);
            Assert.Equal("world", second.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MoveLastWordFromFirstLineDown_OriginalFocused_MovesOriginalWordOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Hello big" + Environment.NewLine + "world", "Hej store" + Environment.NewLine + "verden");
            vm.SelectedSubtitle = line;
            FocusOriginal(vm, editable: true);

            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);

            Assert.Equal("Hej" + Environment.NewLine + "store verden", line.OriginalText);
            Assert.Equal("Hello big" + Environment.NewLine + "world", line.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MoveFirstWordFromNextLineUp_OriginalFocused_MovesOriginalWordOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Hello" + Environment.NewLine + "big world", "Hej" + Environment.NewLine + "store verden");
            vm.SelectedSubtitle = line;
            FocusOriginal(vm, editable: true);

            vm.MoveFirstWordFromNextLineUpCurrentSubtitleCommand.Execute(null);

            Assert.Equal("Hej store" + Environment.NewLine + "verden", line.OriginalText);
            Assert.Equal("Hello" + Environment.NewLine + "big world", line.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// #15496: pressing "Move last word from first line down" repeatedly cycles like SE 4 - once
    /// the last word has moved down, the text collapses to one line and the next press starts over.
    /// </summary>
    [AvaloniaFact]
    public void MoveLastWordFromFirstLineDown_Repeated_CyclesBackToOneLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Hello there friend", "Hej");
            vm.SelectedSubtitle = line;

            var nl = Environment.NewLine;
            var expected = new[]
            {
                "Hello there" + nl + "friend",
                "Hello" + nl + "there friend",
                "Hello there friend",
                "Hello there" + nl + "friend",
            };
            foreach (var text in expected)
            {
                vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);
                Assert.Equal(text, line.Text);
            }
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// #15496: a second line longer than the max line length used to be auto-broken and the whole
    /// text rebalanced, so the word jumped back up instead of staying where the user put it.
    /// </summary>
    [AvaloniaFact]
    public void MoveLastWordFromFirstLineDown_LongLine_KeepsTheUsersBreak()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var nl = Environment.NewLine;
            var line = AddLine(vm, "This is a rather long subtitle" + nl + "line that will need two lines", "Hej");
            vm.SelectedSubtitle = line;

            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);
            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);
            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);

            Assert.Equal("This is a" + nl + "rather long subtitle line that will need two lines", line.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// #15496: with a font color the line break used to land inside a re-flowed color span and
    /// empty "&lt;font ...&gt;&lt;/font&gt;" pairs piled up.
    /// </summary>
    [AvaloniaFact]
    public void MoveLastWordFromFirstLineDown_FontColor_KeepsBreakAndTags()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var nl = Environment.NewLine;
            const string open = "<font color=\"#ff0000\">";
            var line = AddLine(vm, open + "This is a rather long subtitle line that will need two lines</font>", "Hej");
            vm.SelectedSubtitle = line;

            for (var i = 0; i < 9; i++)
            {
                vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);
            }

            Assert.Equal(open + "This is a</font>" + nl + open + "rather long subtitle line that will need two lines</font>", line.Text);

            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);
            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);
            vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand.Execute(null);

            Assert.Equal(open + "This is a rather long subtitle line that will need two lines</font>", line.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MoveFirstWordToPrevious_TranslationFocused_MovesTranslationWordUp()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello", "Hej");
            var second = AddLine(vm, "big world", "store verden", 2000, 4000);
            vm.SelectedSubtitle = second;
            FocusOriginal(vm, editable: true, focused: false);

            vm.MoveFirstWordToPreviousSubtitleCommand.Execute(null);

            Assert.Equal("Hello big", first.Text);
            Assert.Equal("world", second.Text);
            Assert.Equal("Hej", first.OriginalText);
            Assert.Equal("store verden", second.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MoveFirstWordToPrevious_OriginalFocused_MovesOriginalWordUp()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello", "Hej");
            var second = AddLine(vm, "big world", "store verden", 2000, 4000);
            vm.SelectedSubtitle = second;
            FocusOriginal(vm, editable: true);

            vm.MoveFirstWordToPreviousSubtitleCommand.Execute(null);

            Assert.Equal("Hej store", first.OriginalText);
            Assert.Equal("verden", second.OriginalText);
            Assert.Equal("Hello", first.Text);
            Assert.Equal("big world", second.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MoveFirstWordToPrevious_FirstLine_DoesNothing()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var first = AddLine(vm, "Hello world", "Hej verden");
            vm.SelectedSubtitle = first;

            vm.MoveFirstWordToPreviousSubtitleCommand.Execute(null);

            Assert.Equal("Hello world", first.Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [Fact]
    public void MoveFirstWordToPrevious_IsRegisteredAsShortcutAndMappedFromSe4()
    {
        const string name = nameof(MainViewModel.MoveFirstWordToPreviousSubtitleCommand);
        Assert.True(ShortcutsMain.CommandTranslationLookup.ContainsKey(name));
        Assert.Equal(name, Nikse.SubtitleEdit.Features.Options.Shortcuts.Se4ShortcutsImporter.Se4ToSe5CommandMap["MainTextBoxMoveFirstWordToPrev"]);
    }

    /// <summary>
    /// Swaps in an original text box wrapper that reports the requested focus state. Headless
    /// windows cannot reliably move keyboard focus, and the commands only read IsFocused.
    /// </summary>
    private static void FocusOriginal(MainViewModel vm, bool editable, bool focused = true)
    {
        vm.ShowColumnOriginalText = true;
        vm.IsOriginalReadOnly = !editable;
        vm.EditTextBoxOriginal = new FocusStubTextBoxWrapper(new TextBox(), focused);
    }

    private sealed class FocusStubTextBoxWrapper : ITextBoxWrapper
    {
        private readonly ITextBoxWrapper _inner;

        public FocusStubTextBoxWrapper(TextBox textBox, bool isFocused)
        {
            _inner = new TextBoxWrapper(textBox);
            IsFocused = isFocused;
        }

        public bool IsFocused { get; }
        public string Text { get => _inner.Text; set => _inner.Text = value; }
        public string SelectedText { get => _inner.SelectedText; set => _inner.SelectedText = value; }
        public int SelectionStart { get => _inner.SelectionStart; set => _inner.SelectionStart = value; }
        public int SelectionLength { get => _inner.SelectionLength; set => _inner.SelectionLength = value; }
        public int SelectionEnd { get => _inner.SelectionEnd; set => _inner.SelectionEnd = value; }
        public void Select(int start, int length) => _inner.Select(start, length);
        public int CaretIndex { get => _inner.CaretIndex; set => _inner.CaretIndex = value; }
        public void Focus() => _inner.Focus();
        public Control TextControl => _inner.TextControl;
        public Control ContentControl => _inner.ContentControl;
        public bool IsReadOnly => _inner.IsReadOnly;
        public void Cut() => _inner.Cut();
        public void Copy() => _inner.Copy();
        public void Paste() => _inner.Paste();
        public void SelectAll() => _inner.SelectAll();
        public void ClearSelection() => _inner.ClearSelection();
        public void DeleteForward() => _inner.DeleteForward();
        public void SetAlignment(Avalonia.Media.TextAlignment alignment) => _inner.SetAlignment(alignment);
        public void EnableSpellCheck(ISpellCheckManager spellCheckManager) => _inner.EnableSpellCheck(spellCheckManager);
        public void DisableSpellCheck() => _inner.DisableSpellCheck();
        public void RefreshSpellCheck() => _inner.RefreshSpellCheck();
        public bool IsSpellCheckEnabled => _inner.IsSpellCheckEnabled;
        public SpellCheckWord? GetWordAtPosition(PointerEventArgs e) => _inner.GetWordAtPosition(e);
        public bool IsWordMisspelledAtOffset(int offset) => _inner.IsWordMisspelledAtOffset(offset);
        public List<string>? GetSuggestionsForWordAtOffset(int offset) => _inner.GetSuggestionsForWordAtOffset(offset);
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
        MainViewModel vm, string text, string originalText, int startMs = 0, int endMs = 2000)
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
}

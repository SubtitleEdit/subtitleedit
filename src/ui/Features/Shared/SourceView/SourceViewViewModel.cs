using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public partial class SourceViewViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _lineAndColumnInfo;
    [ObservableProperty] private string _selectionInfo;
    [ObservableProperty] private string _validationInfo;
    [ObservableProperty] private bool _isValidationError;

    [ObservableProperty] private bool _isFindBarVisible;
    [ObservableProperty] private bool _isReplaceVisible;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private string _replaceText;
    [ObservableProperty] private bool _matchCase;
    [ObservableProperty] private bool _wholeWord;
    [ObservableProperty] private bool _useRegularExpression;
    [ObservableProperty] private string _findStatus;

    public Window? Window { get; set; }
    public TextBox? SearchTextBox { get; set; }
    public TextBox? ReplaceTextBox { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle Subtitle { get; private set; }

    public SubtitleFormat _subtitleFormat { get; private set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }
    public IRelayCommand CutCommand { get; }
    public IRelayCommand CopyCommand { get; }
    public IRelayCommand PasteCommand { get; }
    public IRelayCommand SelectAllCommand { get; }
    public IRelayCommand UndoCommand { get; }
    public IRelayCommand RedoCommand { get; }
    public IRelayCommand MoveLineUpCommand { get; }
    public IRelayCommand MoveLineDownCommand { get; }
    public IRelayCommand DuplicateLineCommand { get; }
    public IRelayCommand DeleteLineCommand { get; }

    // Parsing a multi-megabyte source on every idle tick would cost more than the editing does, so
    // above this the status line only reports the size.
    private const int MaxValidationTextLength = 4 * 1024 * 1024;

    private readonly IWindowService _windowService;
    private readonly DispatcherTimer _validationTimer;

    private SyntaxTextEditor? _editor;
    private int _initialCaretIndex;
    private bool _isDirty;

    /// <summary>Set once the user confirmed the discard, so the second Close() goes through.</summary>
    private bool _discardConfirmed;

    public SourceViewViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        SourceViewTextBox = new TextBoxWrapper(new TextBox());
        Title = string.Empty;
        Text = string.Empty;
        LineAndColumnInfo = string.Empty;
        SelectionInfo = string.Empty;
        ValidationInfo = string.Empty;
        SearchText = string.Empty;
        ReplaceText = string.Empty;
        FindStatus = string.Empty;
        Subtitle = new Subtitle();
        _subtitleFormat = new SubRip();
        CutCommand = new RelayCommand(() => SourceViewTextBox.Cut());
        CopyCommand = new RelayCommand(() => SourceViewTextBox.Copy());
        PasteCommand = new RelayCommand(() => SourceViewTextBox.Paste());
        SelectAllCommand = new RelayCommand(() => SourceViewTextBox.SelectAll());
        UndoCommand = new RelayCommand(() => _editor?.Undo());
        RedoCommand = new RelayCommand(() => _editor?.Redo());
        MoveLineUpCommand = new RelayCommand(() => _editor?.MoveSelectedLines(-1));
        MoveLineDownCommand = new RelayCommand(() => _editor?.MoveSelectedLines(1));
        DuplicateLineCommand = new RelayCommand(() => _editor?.DuplicateSelectedLines());
        DeleteLineCommand = new RelayCommand(() => _editor?.DeleteSelectedLines());

        _validationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _validationTimer.Tick += ValidationTimerTick;
    }

    public void OnClosingCleanup()
    {
        _validationTimer.Stop();
        _validationTimer.Tick -= ValidationTimerTick;

        if (_editor != null)
        {
            _editor.CaretChanged -= EditorCaretChanged;
            _editor.TextChanged -= EditorTextChanged;
        }
    }

    internal void Initialize(
        string title,
        string text,
        SubtitleFormat subtitleFormat,
        Subtitle subtitle,
        int selectedParagraphIndex)
    {
        Title = title;
        Text = text;
        _subtitleFormat = subtitleFormat;
        _initialCaretIndex = FindSelectedParagraphCaretIndex(text, subtitle, subtitleFormat, selectedParagraphIndex);

        // The editor only lays out the lines it shows, so even a very large source opens fast -
        // no need for the plain-text-box fallback this used to need above 2 MB.
        SourceViewTextBox = CreateAdvancedTextBoxWrapper(text, subtitleFormat);

        UpdateCaretInfo();
        Validate();
    }

    internal void FocusEditor()
    {
        SourceViewTextBox.Focus();
        SourceViewTextBox.CaretIndex = _initialCaretIndex;
    }

    private static int FindSelectedParagraphCaretIndex(
        string source,
        Subtitle subtitle,
        SubtitleFormat subtitleFormat,
        int selectedParagraphIndex)
    {
        if (selectedParagraphIndex < 0 || selectedParagraphIndex >= subtitle.Paragraphs.Count)
        {
            return 0;
        }

        var modifiedSubtitle = new Subtitle(subtitle, false);
        var paragraph = modifiedSubtitle.Paragraphs[selectedParagraphIndex];
        var markerPrefix = paragraph.Text.StartsWith('A') ? "B" : "A";
        paragraph.Text = markerPrefix + "__SOURCE_VIEW_CARET__";
        var modifiedSource = modifiedSubtitle.ToText(subtitleFormat);

        var length = Math.Min(source.Length, modifiedSource.Length);
        var index = 0;
        while (index < length && source[index] == modifiedSource[index])
        {
            index++;
        }

        return index < source.Length ? index : 0;
    }

    private SyntaxTextEditorWrapper CreateAdvancedTextBoxWrapper(string text, SubtitleFormat subtitleFormat)
    {
        var editor = new SyntaxTextEditor
        {
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowLineNumbers = true,
            SourceHighlighter = SourceSyntaxHighlighterFactory.ForFormat(text, subtitleFormat),
            Text = text,
        };

        // Subscribed after the text is set, so loading the source does not count as an edit.
        editor.CaretChanged += EditorCaretChanged;
        editor.TextChanged += EditorTextChanged;
        _editor = editor;

        // The view model's Text is not kept in sync while typing on purpose: materializing the
        // whole source on every keystroke is what made a large file crawl. Ok reads the editor.
        var textBoxBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Child = editor,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        return new SyntaxTextEditorWrapper(editor, textBoxBorder);
    }

    // ----------------------------------------------------------------------------------------
    // Status line
    // ----------------------------------------------------------------------------------------

    private void EditorCaretChanged(object? sender, EventArgs e) => UpdateCaretInfo();

    private void EditorTextChanged(object? sender, EventArgs e)
    {
        _isDirty = true;

        // Restart the idle window: validating while the user is still typing would only flicker.
        _validationTimer.Stop();
        _validationTimer.Start();
    }

    private void UpdateCaretInfo()
    {
        if (_editor == null)
        {
            return;
        }

        var document = _editor.Document;
        var position = document.GetPosition(_editor.CaretOffset);
        LineAndColumnInfo = string.Format(
            Se.Language.General.LineXColumnY,
            Grouped(position.Line + 1),
            Grouped(position.Column + 1));

        var selectionLength = _editor.SelectionLength;
        if (selectionLength == 0)
        {
            SelectionInfo = string.Empty;
            return;
        }

        var firstLine = document.GetPosition(_editor.SelectionStart).Line;
        var lastLine = document.GetPosition(_editor.SelectionStart + selectionLength).Line;
        SelectionInfo = string.Format(
            Se.Language.SourceView.SelectedXCharactersYLines,
            Grouped(selectionLength),
            Grouped(lastLine - firstLine + 1));
    }

    /// <summary>
    /// A source file runs to tens of thousands of lines and characters, so the status line counts
    /// are only readable with thousand separators.
    /// </summary>
    private static string Grouped(int value) => value.ToString("#,##0", CultureInfo.CurrentCulture);

    private void ValidationTimerTick(object? sender, EventArgs e)
    {
        _validationTimer.Stop();
        Validate();
    }

    /// <summary>
    /// Parses the source the way <see cref="Ok"/> will and reports the result, so a broken edit
    /// shows up while it is being made instead of when the dialog is closed.
    /// </summary>
    internal void Validate()
    {
        if (_editor == null)
        {
            return;
        }

        var source = _editor.Text ?? string.Empty;
        var lineCount = _editor.Document.LineCount;

        if (source.Length > MaxValidationTextLength)
        {
            IsValidationError = false;
            ValidationInfo = string.Format(Se.Language.SourceView.XLinesYSubtitles, Grouped(lineCount), "?");
            return;
        }

        var subtitle = new Subtitle();

        // A few formats pick the frame rate up from the source header. Validation runs on every
        // idle tick, so it must not leave that behind in the global settings.
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            _subtitleFormat.LoadSubtitle(subtitle, source.SplitToLines(), string.Empty);
        }
        catch
        {
            // A format that throws on malformed input is just an unparsable source here.
            subtitle.Paragraphs.Clear();
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }

        if (subtitle.Paragraphs.Count == 0)
        {
            IsValidationError = true;
            ValidationInfo = string.Format(Se.Language.SourceView.CouldNotParseAsX, _subtitleFormat.Name);
            return;
        }

        var errorCount = _subtitleFormat.ErrorCount;
        IsValidationError = errorCount > 0;
        ValidationInfo = errorCount > 0
            ? string.Format(
                Se.Language.SourceView.XLinesYSubtitlesZErrors,
                Grouped(lineCount),
                Grouped(subtitle.Paragraphs.Count),
                Grouped(errorCount))
            : string.Format(Se.Language.SourceView.XLinesYSubtitles, Grouped(lineCount), Grouped(subtitle.Paragraphs.Count));
    }

    // ----------------------------------------------------------------------------------------
    // Find and replace
    // ----------------------------------------------------------------------------------------

    [RelayCommand]
    private void ShowFind()
    {
        IsReplaceVisible = false;
        OpenFindBar();
    }

    [RelayCommand]
    private void ShowReplace()
    {
        IsReplaceVisible = true;
        OpenFindBar();
    }

    private void OpenFindBar()
    {
        // Selecting a word and pressing Ctrl+F should search for that word, like everywhere else.
        var selected = _editor?.SelectedText ?? string.Empty;
        if (selected.Length > 0 && selected.IndexOfAny(['\r', '\n']) < 0)
        {
            SearchText = selected;
        }

        FindStatus = string.Empty;
        IsFindBarVisible = true;

        Dispatcher.UIThread.Post(() =>
        {
            SearchTextBox?.Focus();
            SearchTextBox?.SelectAll();
        });
    }

    [RelayCommand]
    private void CloseFindBar()
    {
        IsFindBarVisible = false;
        FindStatus = string.Empty;
        SourceViewTextBox.Focus();
    }

    /// <summary>
    /// One regex for every mode - plain text is the escaped pattern - so whole word and wrap-around
    /// behave the same whichever mode the user is in. Null means the pattern is unusable.
    /// </summary>
    private Regex? BuildSearchRegex()
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            return null;
        }

        var pattern = UseRegularExpression ? SearchText : Regex.Escape(SearchText);
        if (WholeWord)
        {
            pattern = @"\b(?:" + pattern + @")\b";
        }

        var options = MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        try
        {
            return new Regex(pattern, options);
        }
        catch (ArgumentException)
        {
            FindStatus = Se.Language.SourceView.InvalidRegularExpression;
            return null;
        }
    }

    [RelayCommand]
    private void FindNext() => Find(forward: true, fromSelectionStart: false);

    [RelayCommand]
    private void FindPrevious() => Find(forward: false, fromSelectionStart: false);

    /// <summary>Re-runs the search from where the caret is while the search text is being typed.</summary>
    internal void SearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!IsFindBarVisible || string.IsNullOrEmpty(SearchText))
        {
            FindStatus = string.Empty;
            return;
        }

        Find(forward: true, fromSelectionStart: true);
    }

    private bool Find(bool forward, bool fromSelectionStart)
    {
        if (_editor == null)
        {
            return false;
        }

        var regex = BuildSearchRegex();
        if (regex == null)
        {
            return false;
        }

        var text = _editor.Text ?? string.Empty;

        // Step past the match that is selected right now, but do not skip one that starts exactly
        // under a bare caret.
        var searchFrom = _editor.SelectionStart;
        if (!fromSelectionStart && _editor.SelectionLength > 0)
        {
            searchFrom++;
        }

        var match = forward
            ? FindForward(regex, text, searchFrom)
            : FindBackward(regex, text, _editor.SelectionStart);

        if (match == null)
        {
            FindStatus = string.Format(Se.Language.General.XNotFound, SearchText);
            return false;
        }

        FindStatus = string.Empty;
        _editor.Select(match.Index, match.Length);
        _editor.BringCaretIntoView();
        return true;
    }

    private static Match? FindForward(Regex regex, string text, int startAt)
    {
        if (startAt <= text.Length)
        {
            var match = regex.Match(text, Math.Max(0, startAt));
            if (match.Success)
            {
                return match;
            }
        }

        // Wrap around to the top, the way find in the main window does.
        var fromTop = regex.Match(text);
        return fromTop.Success ? fromTop : null;
    }

    private static Match? FindBackward(Regex regex, string text, int before)
    {
        Match? best = null;
        Match? last = null;

        for (var match = regex.Match(text); match.Success; match = match.NextMatch())
        {
            if (match.Index < before)
            {
                best = match;
            }

            last = match;

            // A zero-length match would loop forever otherwise.
            if (match.Length == 0 && match.Index >= text.Length)
            {
                break;
            }
        }

        return best ?? last; // nothing above the caret: wrap around to the last match
    }

    [RelayCommand]
    private void Replace()
    {
        if (_editor == null)
        {
            return;
        }

        var regex = BuildSearchRegex();
        if (regex == null)
        {
            return;
        }

        // Only replace when the current selection really is the match - otherwise just go find one.
        if (_editor.SelectionLength > 0)
        {
            var selected = _editor.SelectedText;
            var match = regex.Match(selected);
            if (match.Success && match.Index == 0 && match.Length == selected.Length)
            {
                var replacement = UseRegularExpression
                    ? match.Result(ReplaceText ?? string.Empty)
                    : ReplaceText ?? string.Empty;

                _editor.InsertText(replacement);
                _editor.Select(_editor.CaretOffset, 0);
            }
        }

        Find(forward: true, fromSelectionStart: true);
    }

    [RelayCommand]
    private void ReplaceAll()
    {
        if (_editor == null)
        {
            return;
        }

        var regex = BuildSearchRegex();
        if (regex == null)
        {
            return;
        }

        var text = _editor.Text ?? string.Empty;
        var count = regex.Count(text);
        if (count == 0)
        {
            FindStatus = string.Format(Se.Language.General.XNotFound, SearchText);
            return;
        }

        var replacement = UseRegularExpression
            ? ReplaceText ?? string.Empty
            : (ReplaceText ?? string.Empty).Replace("$", "$$"); // a literal replacement stays literal

        var caretBefore = _editor.CaretOffset;

        // One edit for the whole document: one undo step, and the text never goes around the undo
        // stack the way assigning Text would.
        _editor.ReplaceAllText(regex.Replace(text, replacement));

        _editor.Select(Math.Min(caretBefore, _editor.Document.TextLength), 0);
        _editor.BringCaretIntoView();
        FindStatus = string.Format(Se.Language.SourceView.ReplacedXOccurrences, count);
    }

    // ----------------------------------------------------------------------------------------
    // Go to line
    // ----------------------------------------------------------------------------------------

    [RelayCommand]
    private async Task GoToLine()
    {
        if (_editor == null || Window == null)
        {
            return;
        }

        var document = _editor.Document;
        var currentLine = document.GetPosition(_editor.CaretOffset).Line + 1;

        var result = await _windowService.ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(
            Window,
            vm => vm.Initialize(currentLine, document.LineCount));

        if (result is not { OkPressed: true, LineNumber: > 0 })
        {
            return;
        }

        var line = Math.Min((int)result.LineNumber.Value, document.LineCount) - 1;
        var start = document.GetLineStartOffset(line);
        _editor.Select(start, document.GetLineEndOffset(line) - start);
        _editor.BringCaretIntoView();
        SourceViewTextBox.Focus();
    }

    // ----------------------------------------------------------------------------------------
    // Closing
    // ----------------------------------------------------------------------------------------

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        // Read the edited source from the editor, not from Text - Text is only the value the
        // window opened with.
        var sourceText = SourceViewTextBox.Text;
        var text = TrimJunk(sourceText);
        if (string.IsNullOrEmpty(text))
        {
            OkPressed = false;
            _discardConfirmed = true; // an empty source is not a change worth asking about
            Window?.Close();
            return;
        }

        var lines = sourceText.SplitToLines();
        var subtitle = new Subtitle();
        _subtitleFormat.LoadSubtitle(subtitle, lines, string.Empty);
        if (subtitle.Paragraphs.Count > 0)
        {
            Subtitle.Paragraphs.Clear();
            Subtitle.Paragraphs.AddRange(subtitle.Paragraphs);
            OkPressed = true;
            Window?.Close();
            return;
        }

        subtitle = Subtitle.Parse(lines, ".srt");
        if (subtitle.Paragraphs.Count > 0)
        {
            Subtitle.Paragraphs.Clear();
            Subtitle.Paragraphs.AddRange(subtitle.Paragraphs);
            OkPressed = true;
            Window?.Close();
            return;
        }

        await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.NoSubtitlesFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static string TrimJunk(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> junk =
        [
            '\uFEFF', // UTF-8 BOM / Zero Width No-Break Space
            '\u200B', // Zero Width Space
            '\u200C', // Zero Width Non-Joiner
            '\u200D', // Zero Width Joiner
            '\u200E', // Left-to-Right Mark
            '\u200F', // Right-to-Left Mark
            '\u00AD', // Soft Hyphen
            '\u2060', // Word Joiner
            '\uFFFD', // Replacement Character
            '\u0000', // Null
        ];

        int start = 0;
        int end = text.Length - 1;

        while (start <= end && (char.IsWhiteSpace(text[start]) || junk.Contains(text[start])))
        {
            start++;
        }

        while (end >= start && (char.IsWhiteSpace(text[end]) || junk.Contains(text[end])))
        {
            end--;
        }

        return text[start..(end + 1)].Trim();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    /// <summary>
    /// True while closing would throw away edits. The window turns this into a prompt - hand-editing
    /// a few hundred lines and losing them to a stray Escape is not a fair trade.
    /// </summary>
    public bool NeedsDiscardConfirmation => _isDirty && !OkPressed && !_discardConfirmed;

    public async Task<bool> ConfirmDiscardAsync()
    {
        if (Window == null)
        {
            return true;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.SourceView.DiscardChangesTitle,
            Se.Language.SourceView.DiscardChanges,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        _discardConfirmed = result == MessageBoxResult.Yes;
        return _discardConfirmed;
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var commandModifier = OperatingSystem.IsMacOS()
            ? (e.KeyModifiers & KeyModifiers.Meta) != 0
            : (e.KeyModifiers & KeyModifiers.Control) != 0;
        var shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;

        // Help is user-configurable, so it cannot be a case in the switch below.
        if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/source-view");
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
                e.Handled = true;
                if (IsFindBarVisible)
                {
                    CloseFindBar(); // first Escape leaves the search bar, the next one leaves the window
                }
                else
                {
                    Window?.Close();
                }

                break;

            // Cmd+H hides the application on macOS, so replace answers to Cmd+Alt+F there as well.
            // This case has to come first or Cmd+Alt+F would be swallowed by plain Ctrl+F below.
            case Key.H when commandModifier:
            case Key.F when commandModifier && (e.KeyModifiers & KeyModifiers.Alt) != 0:
                e.Handled = true;
                ShowReplace();
                break;

            case Key.F when commandModifier && !shift:
                e.Handled = true;
                ShowFind();
                break;

            case Key.G when commandModifier:
                e.Handled = true;
                _ = GoToLine();
                break;

            case Key.F3:
                e.Handled = true;
                if (string.IsNullOrEmpty(SearchText))
                {
                    ShowFind();
                }
                else
                {
                    Find(forward: !shift, fromSelectionStart: false);
                }

                break;

            case Key.Enter when IsFindBarVisible && IsSearchInputFocused():
                e.Handled = true;
                Find(forward: !shift, fromSelectionStart: false);
                break;
        }
    }

    private bool IsSearchInputFocused()
    {
        return SearchTextBox?.IsFocused == true || ReplaceTextBox?.IsFocused == true;
    }
}

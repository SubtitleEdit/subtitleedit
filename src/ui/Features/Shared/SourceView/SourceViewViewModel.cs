using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public partial class SourceViewViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _lineAndColumnInfo;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle Subtitle { get; private set; }

    public SubtitleFormat _subtitleFormat { get; private set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }
    public IRelayCommand CutCommand { get; }
    public IRelayCommand CopyCommand { get; }
    public IRelayCommand PasteCommand { get; }

    private readonly System.Timers.Timer _cursorTimer;
    private int _initialCaretIndex;

    public SourceViewViewModel()
    {
        SourceViewTextBox = new TextBoxWrapper(new TextBox());
        Title = string.Empty;
        Text = string.Empty;
        LineAndColumnInfo = string.Empty;
        Subtitle = new Subtitle();
        _subtitleFormat = new SubRip();
        CutCommand = new RelayCommand(() => SourceViewTextBox.Cut());
        CopyCommand = new RelayCommand(() => SourceViewTextBox.Copy());
        PasteCommand = new RelayCommand(() => SourceViewTextBox.Paste());

        _cursorTimer = new System.Timers.Timer(200);
        _cursorTimer.Elapsed += CursorTimerElapsed;
    }

    private void CursorTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (SourceViewTextBox == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var caretIndex = SourceViewTextBox.CaretIndex;
            int lineNumber;
            int columnNumber;

            if (SourceViewTextBox.TextControl is SyntaxTextView view)
            {
                // The editor already indexes the lines - asking it beats walking the whole source
                // four times a second (which on a large file costs more than the editing does).
                var position = view.Document.GetPosition(caretIndex);
                lineNumber = position.Line + 1;
                columnNumber = position.Column + 1;
            }
            else
            {
                var text = SourceViewTextBox.Text ?? string.Empty;
                lineNumber = 1;
                columnNumber = 1;

                for (var i = 0; i < Math.Min(caretIndex, text.Length); i++)
                {
                    if (text[i] == '\n')
                    {
                        lineNumber++;
                        columnNumber = 1;
                    }
                    else if (text[i] != '\r') // Skip carriage return
                    {
                        columnNumber++;
                    }
                }
            }

            LineAndColumnInfo = string.Format(Se.Language.General.LineXColumnY, lineNumber, columnNumber);
        });
    }

    public void OnClosingCleanup()
    {
        _cursorTimer.StopAndDispose(CursorTimerElapsed);
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
        _cursorTimer.Start();
        _initialCaretIndex = FindSelectedParagraphCaretIndex(text, subtitle, subtitleFormat, selectedParagraphIndex);

        // The editor only lays out the lines it shows, so even a very large source opens fast -
        // no need for the plain-text-box fallback this used to need above 2 MB.
        SourceViewTextBox = CreateAdvancedTextBoxWrapper(text, subtitleFormat);
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

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}

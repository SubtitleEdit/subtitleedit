using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.SourceView;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Diagnostics;

namespace UITests.Features;

/// <summary>
/// The source view's find/replace takes a user-entered regular expression and runs it on the UI
/// thread, so it gets the same five second match timeout as the main find/replace: without one a
/// pattern with catastrophic backtracking hung the program with no way out. The timeout is
/// reported like "not found" - the document is never left half-replaced.
/// </summary>
public class SourceViewRegexTimeoutTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly ShortRegexTimeout _shortRegexTimeout = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        _shortRegexTimeout.Dispose();
    }

    private sealed class NoWindowService : IWindowService
    {
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

        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window where TViewModel : class
            => throw new NotSupportedException();
    }

    // 30 a's and no "b": "(a+)+b" has to try every way of splitting them before giving up.
    private const string EvilPattern = "(a+)+b";
    private static readonly string EvilLine = new string('a', 30) + "c";

    // Well above the five second timeout, but far below "never returns".
    private const int MaxSeconds = 60;

    private SourceViewViewModel MakeSourceView()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(EvilLine, 1000, 3000));

        var format = new SubRip();
        var text = subtitle.ToText(format);

        var vm = new SourceViewViewModel(new NoWindowService());
        vm.Initialize("Source view", text, format, subtitle, 0);
        vm.UseRegularExpression = true;
        vm.SearchText = EvilPattern;

        var window = new Window { Content = new Border { Child = vm.SourceViewTextBox.ContentControl } };
        _windows.Add(window);
        vm.Window = window;
        window.Show();
        window.UpdateLayout();

        return vm;
    }

    private static string TooSlowMessage() => string.Format(
        Se.Language.SourceView.RegularExpressionTooSlowX,
        RegexUtils.UserPatternMatchTimeout.TotalSeconds);

    [AvaloniaFact]
    public void FindNext_CatastrophicPattern_GivesUpAndSaysSo()
    {
        var vm = MakeSourceView();

        var stopwatch = Stopwatch.StartNew();
        vm.FindNextCommand.Execute(null);
        stopwatch.Stop();

        Assert.Equal(TooSlowMessage(), vm.FindStatus);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"FindNext took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }

    [AvaloniaFact]
    public void ReplaceAll_CatastrophicPattern_GivesUpAndLeavesTheDocumentAlone()
    {
        var vm = MakeSourceView();
        var before = ((SyntaxTextView)vm.SourceViewTextBox.TextControl).Document.Text;
        vm.ReplaceText = "x";

        var stopwatch = Stopwatch.StartNew();
        vm.ReplaceAllCommand.Execute(null);
        stopwatch.Stop();

        Assert.Equal(TooSlowMessage(), vm.FindStatus);
        Assert.Equal(before, ((SyntaxTextView)vm.SourceViewTextBox.TextControl).Document.Text);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"ReplaceAll took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }

    // The timeout must not turn an ordinary search into a failure.
    [AvaloniaFact]
    public void OrdinaryPattern_StillFinds()
    {
        var vm = MakeSourceView();
        vm.SearchText = "a{5}";

        vm.FindNextCommand.Execute(null);

        Assert.Equal(string.Empty, vm.FindStatus);
    }
}

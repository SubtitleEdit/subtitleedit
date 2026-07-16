using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;
using FontStyle = TextMateSharp.Themes.FontStyle;

namespace AvaloniaEdit.TextMate
{
    public class TextMateColoringTransformer :
        GenericLineTransformer,
        IModelTokensChangedListener,
        IDisposable
    {
        private readonly object _lock = new object();
        private bool _isDisposed;
        private Theme _theme;
        private IGrammar _grammar;
        private TMModel _model;
        private TextDocument _document;
        private readonly TextView _textView;
        private readonly Action<Exception> _exceptionHandler;

        // These fields are only accessed under _lock so volatile is unnecessary
        private bool _areVisualLinesValid;
        private int _firstVisibleLineIndex = -1;
        private int _lastVisibleLineIndex = -1;

        // Copy-on-write: SetTheme builds a new dictionary and atomically swaps
        // the reference under lock. Readers capture the reference once and use it
        // safely - the captured dictionary is never mutated after publication.
        // Dictionary.Clear() is intentionally NOT called anywhere because an
        // in-flight TransformLine may hold a captured local reference to the
        // same dictionary object. Mutating it in-place via Clear() would corrupt
        // the concurrent read. The old dictionary becomes unreachable once all
        // in-flight readers complete, and is collected by GC naturally.
        private Dictionary<int, IBrush> _brushes;

        /// <summary>
        /// Initializes a new instance of the TextMateColoringTransformer class, which applies syntax highlighting to
        /// the specified text view.
        /// </summary>
        /// <remarks>The TextMateColoringTransformer subscribes to the VisualLinesChanged event of the
        /// TextView to update the syntax highlighting when the visual lines change.</remarks>
        /// <param name="textView">The TextView instance to which syntax highlighting will be applied. This parameter cannot be null.</param>
        /// <param name="exceptionHandler">An action to handle exceptions that occur during the transformation process.</param>
        /// <exception cref="ArgumentNullException">Thrown if the textView parameter is null.</exception>
        public TextMateColoringTransformer(
            TextView textView,
            Action<Exception> exceptionHandler)
            : base(exceptionHandler)
        {
            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
            _exceptionHandler = exceptionHandler;

            _brushes = new Dictionary<int, IBrush>();
            _textView.VisualLinesChanged += TextView_VisualLinesChanged;
        }

        /// <summary>
        /// Associates the specified text document and model with the transformer for processing and coloring.
        /// </summary>
        /// <remarks>This method is thread-safe and locks access during the operation. It validates the
        /// state of the model and grammar before setting them, ensuring that stale references are severed when null
        /// values are provided.</remarks>
        /// <param name="document">The text document to be associated with the transformer, or <see langword="null"/> to clear the existing document reference (e.g., during disposal).</param>
        /// <param name="model">The model to be set for the document, or <see langword="null"/> to clear the existing model reference (e.g., during disposal).
        /// When both <paramref name="document"/> and <paramref name="model"/> are null, all stale references are severed.</param>
        public void SetModel(TextDocument document, TMModel model)
        {
            ThrowIfDisposed();

            lock (_lock)
            {
                ThrowIfDisposed();

                _areVisualLinesValid = false;
                _document = document;
                _model = model;

                // Null guard: prevents NRE when model is null (e.g., during
                // Installation.Dispose teardown). This also enables Installation
                // to safely call SetModel(null, null) to sever stale references.
                if (_grammar != null && _model != null)
                {
                    _model.SetGrammar(_grammar);
                }
            }
        }

        /// <summary>
        /// Handles the event that occurs when the visual lines of the text view are changed, updating the indices of
        /// the first and last visible lines as needed.
        /// </summary>
        /// <remarks>This method performs updates only if the text view is valid and not disposed. If an
        /// exception occurs during processing, an optional exception handler is invoked if one is set.</remarks>
        /// <param name="sender">The source of the event, typically the text view control whose visual lines have changed.</param>
        /// <param name="e">The event data associated with the visual lines changed event.</param>
        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            // Fast path: event handler - silently return if disposed.
            if (Volatile.Read(ref _isDisposed))
                return;

            try
            {
                if (!_textView.VisualLinesValid || _textView.VisualLines.Count == 0)
                    return;

                lock (_lock)
                {
                    if (Volatile.Read(ref _isDisposed))
                        return;

                    _areVisualLinesValid = true;
                    _firstVisibleLineIndex = _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1;
                    _lastVisibleLineIndex = _textView.VisualLines[_textView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1;
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        /// <summary>
        /// Releases all resources used by this <see cref="TextMateColoringTransformer"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="TextMateColoringTransformer"/>
        /// instance and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Fast path: Volatile.Read avoids lock acquisition when already disposed.
            if (Volatile.Read(ref _isDisposed))
                return;

            if (!disposing)
                return;

            lock (_lock)
            {
                // Authoritative check under lock.
                if (Volatile.Read(ref _isDisposed))
                    return;

                // Volatile.Write ensures that Volatile.Read callers outside the lock
                // (ThrowIfDisposed, fast-path guards) see this write with proper
                // memory ordering. Both sides use the Volatile API to satisfy
                // analyzers that require matching synchronization primitives.
                Volatile.Write(ref _isDisposed, true);

                _theme = null;
                _grammar = null;
                _model = null;
                _document = null;
                _brushes = null;
            }

            // Unsubscribe outside lock - _textView is readonly so this is safe,
            // and avoids calling external code under lock.
            _textView.VisualLinesChanged -= TextView_VisualLinesChanged;
        }

        /// <summary>
        /// Sets the current theme for syntax highlighting by updating the internal brush dictionary with colors defined
        /// in the specified theme.
        /// </summary>
        /// <remarks>This method is thread-safe and minimizes lock contention by performing expensive
        /// brush creation operations outside the lock. If the object has been disposed, an exception is thrown. Any
        /// ongoing line transformation operations will continue to use the previous brush dictionary safely, as
        /// dictionaries are replaced atomically and never mutated.</remarks>
        /// <param name="theme">The theme to apply. This parameter provides color definitions that are used to construct the brush
        /// dictionary for syntax highlighting. Cannot be null.</param>
        public void SetTheme(Theme theme)
        {
            ThrowIfDisposed();

            // Build the new brush dictionary outside the lock. Color parsing and
            // ImmutableSolidColorBrush creation are the expensive operations - doing
            // them outside minimizes lock hold time
            var map = theme.GetColorMap();
            var newBrushes = new Dictionary<int, IBrush>();

            foreach (var color in map)
            {
                var id = theme.GetColorId(color);
                newBrushes[id] = new ImmutableSolidColorBrush(Color.Parse(NormalizeColor(color)));
            }

            lock (_lock)
            {
                ThrowIfDisposed();

                _theme = theme;

                // Atomic reference swap. Any concurrent TransformLine call that
                // already captured the old dictionary continues using it safely -
                // the old dictionary is never mutated, only replaced.
                _brushes = newBrushes;
            }
        }

        /// <summary>
        /// Sets the grammar to be used by the model, updating its internal state accordingly.
        /// </summary>
        /// <remarks>This method is thread-safe and should only be called when the object has not been
        /// disposed. If the model is already initialized, calling this method will also update the model's
        /// grammar.</remarks>
        /// <param name="grammar">The grammar to apply to the model. Determines how the model tokenizes and interprets text.
        /// If <see langword="null"/>, the current grammar reference is cleared and no grammar is applied to the model.</param>
        public void SetGrammar(IGrammar grammar)
        {
            ThrowIfDisposed();

            lock (_lock)
            {
                ThrowIfDisposed();

                _grammar = grammar;

                if (_model != null)
                {
                    _model.SetGrammar(grammar);
                }
            }
        }

        /// <summary>
        /// Transforms the specified document line by applying syntax highlighting and theme-based color transformations
        /// according to the current model and theme settings.
        /// </summary>
        /// <remarks>This method is thread-safe and ensures that transformations are only applied when the
        /// object has not been disposed. It captures necessary state under a lock to prevent race conditions and uses
        /// only local copies of mutable fields during transformation.</remarks>
        /// <param name="line">The document line to be transformed. Contains the text and formatting information for a single line in the
        /// document.</param>
        /// <param name="context">The context for text run construction, providing additional information required for rendering the line.</param>
        protected override void TransformLine(DocumentLine line, ITextRunConstructionContext context)
        {
            // Rendering callback - silently return if disposed.
            if (Volatile.Read(ref _isDisposed))
                return;

            try
            {
                // Capture field snapshots under lock. The lock acquisition is brief -
                // just 4 reference copies - and the lock is almost never contended
                // (only the background tokenizer thread competes via ModelTokensChanged).
                TMModel model;
                TextDocument document;
                Theme theme;
                Dictionary<int, IBrush> brushes;

                lock (_lock)
                {
                    if (Volatile.Read(ref _isDisposed))
                        return;

                    model = _model;
                    document = _document;
                    theme = _theme;
                    brushes = _brushes;
                }

                // All work below uses only captured locals - no mutable field reads
                if (model == null || document == null || theme == null || brushes == null)
                    return;

                int lineNumber = line.LineNumber;

                var tokens = model.GetLineTokens(lineNumber - 1);

                // If there are no tokens to process, avoid the overhead of GetLineTransformations
                // (including its internal GetLineByNumber call)
                if (tokens == null || tokens.Count == 0)
                    return;

                var transformsInLine = ArrayPool<ForegroundTextTransformation>.Shared.Rent(tokens.Count);

                try
                {
                    GetLineTransformations(lineNumber, tokens, transformsInLine, model, document, theme, brushes);

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (transformsInLine[i] == null)
                            continue;

                        transformsInLine[i].Transform(this, line);
                    }
                }
                finally
                {
                    ArrayPool<ForegroundTextTransformation>.Shared.Return(transformsInLine);
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        /// <summary>
        /// Generates visual transformation data for each token in the specified line, applying theme-based styling to
        /// enable syntax highlighting and formatting.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All dependencies are passed as parameters - this method performs zero
        /// mutable field reads, making it inherently thread-safe.
        /// </para>
        /// <para>
        /// This method is typically used in syntax highlighting scenarios to apply consistent
        /// visual styles to code elements. The transformations array must be pre-allocated and have the same length as
        /// the tokens list. If a token cannot be styled, its corresponding transformation entry will be set to
        /// null.
        /// </para>
        /// </remarks>
        /// <param name="lineNumber">The index of the line in the document for which transformations are to be generated.</param>
        /// <param name="tokens">A list of tokens representing the syntax elements in the specified line. Each token will be styled according
        /// to the theme.</param>
        /// <param name="transformations">An array that will be populated with transformation data for each token, defining how the tokens should be
        /// rendered visually.</param>
        /// <param name="model">The model representing the overall structure of the document, used to retrieve line and token information.</param>
        /// <param name="document">The text document containing the lines and tokens, providing access to line offsets and other document
        /// properties.</param>
        /// <param name="theme">The theme containing styling rules that dictate how tokens should be visually represented based on their
        /// scopes.</param>
        /// <param name="brushes">A dictionary mapping color identifiers to brush objects, used to apply foreground and background colors to
        /// tokens.</param>
        private void GetLineTransformations(
            int lineNumber,
            List<TMToken> tokens,
            ForegroundTextTransformation[] transformations,
            TMModel model,
            TextDocument document,
            Theme theme,
            Dictionary<int, IBrush> brushes)
        {
            // Hoisted outside the loop: lineNumber is invariant across all tokens
            // in this line, so GetLineByNumber only needs to be called once
            var lineOffset = document.GetLineByNumber(lineNumber).Offset;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var nextToken = (i + 1) < tokens.Count ? tokens[i + 1] : null;

                var startIndex = token.StartIndex;
                var endIndex = nextToken?.StartIndex ?? model.GetLines().GetLineLength(lineNumber - 1);

                if (startIndex >= endIndex || token.Scopes == null || token.Scopes.Count == 0)
                {
                    transformations[i] = null;
                    continue;
                }

                int foreground = 0;
                int background = 0;
                FontStyle fontStyle = 0;

                foreach (var themeRule in theme.Match(token.Scopes))
                {
                    if (foreground == 0 && themeRule.foreground > 0)
                        foreground = themeRule.foreground;

                    if (background == 0 && themeRule.background > 0)
                        background = themeRule.background;

                    if (fontStyle == 0 && themeRule.fontStyle > 0)
                        fontStyle = themeRule.fontStyle;
                }

                if (transformations[i] == null)
                    transformations[i] = new ForegroundTextTransformation();

                transformations[i].ColorMap = brushes;
                transformations[i].ExceptionHandler = _exceptionHandler;
                transformations[i].StartOffset = lineOffset + startIndex;
                transformations[i].EndOffset = lineOffset + endIndex;
                transformations[i].ForegroundColor = foreground;
                transformations[i].BackgroundColor = background;
                transformations[i].FontStyle = fontStyle;
            }
        }

        /// <summary>
        /// Handles changes to the model's token ranges and updates the visible lines in the text view as needed.
        /// </summary>
        /// <remarks>This method is invoked from a background thread and synchronizes access to shared
        /// state with the UI thread. If the model or document is disposed, or if the changed lines are not currently
        /// visible, the method returns without updating the UI.</remarks>
        /// <param name="e">An event object containing the ranges of lines in the model that have changed.</param>
        public void ModelTokensChanged(ModelTokensChangedEvent e)
        {
            if (e.Ranges == null)
                return;

            // Background callback - silently return if disposed
            if (Volatile.Read(ref _isDisposed))
                return;

            // Capture a consistent snapshot under lock. ModelTokensChanged is called
            // from the tokenizer's background thread, so every field read must be
            // synchronized against UI-thread writes (SetModel, Dispose, etc.)
            TMModel model;
            TextDocument document;
            bool areVisualLinesValid;
            int firstVisibleLineIndex;
            int lastVisibleLineIndex;

            lock (_lock)
            {
                if (Volatile.Read(ref _isDisposed))
                    return;

                model = _model;
                document = _document;
                areVisualLinesValid = _areVisualLinesValid;
                firstVisibleLineIndex = _firstVisibleLineIndex;
                lastVisibleLineIndex = _lastVisibleLineIndex;
            }

            if (model == null || model.IsStopped)
                return;

            if (document == null)
                return;

            int firstChangedLineIndex = int.MaxValue;
            int lastChangedLineIndex = -1;

            foreach (var range in e.Ranges)
            {
                firstChangedLineIndex = Math.Min(range.FromLineNumber - 1, firstChangedLineIndex);
                lastChangedLineIndex = Math.Max(range.ToLineNumber - 1, lastChangedLineIndex);
            }

            if (areVisualLinesValid)
            {
                bool changedLinesAreNotVisible =
                    ((firstChangedLineIndex < firstVisibleLineIndex && lastChangedLineIndex < firstVisibleLineIndex) ||
                    (firstChangedLineIndex > lastVisibleLineIndex && lastChangedLineIndex > lastVisibleLineIndex));

                if (changedLinesAreNotVisible)
                    return;
            }

            // The lambda captures only locals - no mutable field reads at dispatch time.
            // This eliminates the race between the Post() call (background thread) and
            // the lambda execution (UI thread), where SetModel or Dispose could have
            // nulled _document or changed the visible line range.
            Dispatcher.UIThread.Post(() =>
            {
                // Guard against disposal that occurred between Post() and execution
                if (Volatile.Read(ref _isDisposed))
                    return;

                int firstLineIndexToRedraw = Math.Max(firstChangedLineIndex, firstVisibleLineIndex);
                int lastLineIndexToRedrawLine = Math.Min(lastChangedLineIndex, lastVisibleLineIndex);

                int totalLines = document.Lines.Count - 1;

                firstLineIndexToRedraw = Clamp(firstLineIndexToRedraw, 0, totalLines);
                lastLineIndexToRedrawLine = Clamp(lastLineIndexToRedrawLine, 0, totalLines);

                if (!areVisualLinesValid || lastLineIndexToRedrawLine < firstLineIndexToRedraw)
                {
                    _textView.Redraw();
                    return;
                }

                DocumentLine firstLineToRedraw = document.Lines[firstLineIndexToRedraw];
                DocumentLine lastLineToRedraw = document.Lines[lastLineIndexToRedrawLine];

                _textView.Redraw(
                    firstLineToRedraw.Offset,
                    (lastLineToRedraw.Offset + lastLineToRedraw.TotalLength) - firstLineToRedraw.Offset);
            });
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this instance has been disposed.
        /// Uses <see cref="Volatile.Read(ref bool)"/> for a lock-free memory-barrier-safe
        /// read paired with <see cref="Volatile.Write(ref bool, bool)"/> in
        /// <see cref="Dispose(bool)"/>, suitable as a fast-path guard before acquiring
        /// <see cref="_lock"/>.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _isDisposed))
                throw new ObjectDisposedException(nameof(TextMateColoringTransformer));
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        static string NormalizeColor(string color)
        {
            if (color.Length == 9)
            {
                Span<char> normalizedColor = stackalloc char[] { '#', color[7], color[8], color[1], color[2], color[3], color[4], color[5], color[6] };

                return normalizedColor.ToString();
            }

            return color;
        }
    }
}
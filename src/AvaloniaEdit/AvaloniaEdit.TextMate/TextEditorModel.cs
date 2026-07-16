using System;
using System.Threading;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;

namespace AvaloniaEdit.TextMate
{
    public class TextEditorModel : AbstractLineList, IDisposable
    {
        private readonly TextDocument _document;
        private readonly TextView _textView;
        private readonly DocumentSnapshot _documentSnapshot;
        private readonly Action<Exception> _exceptionHandler;
        private InvalidLineRange? _invalidRange;
        private bool _isDisposed;

        public DocumentSnapshot DocumentSnapshot => _documentSnapshot;
        internal InvalidLineRange? InvalidRange => _invalidRange;

        public TextEditorModel(TextView textView, TextDocument document, Action<Exception> exceptionHandler)
        {
            _textView = textView;
            _document = document;
            _exceptionHandler = exceptionHandler;

            _documentSnapshot = new DocumentSnapshot(_document);

            for (int i = 0; i < _document.LineCount; i++)
                AddLine(i);

            _document.Changing += DocumentOnChanging;
            _document.Changed += DocumentOnChanged;
            _document.UpdateFinished += DocumentOnUpdateFinished;
            _textView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
        }

        public override void Dispose()
        {
            // Fast path: Volatile.Read avoids redundant unsubscription when
            // already disposed.
            if (Volatile.Read(ref _isDisposed))
                return;

            // Volatile.Write ensures that Volatile.Read callers (event handlers,
            // public method guards) see this write with proper memory ordering.
            Volatile.Write(ref _isDisposed, true);

            _document.Changing -= DocumentOnChanging;
            _document.Changed -= DocumentOnChanged;
            _document.UpdateFinished -= DocumentOnUpdateFinished;
            _textView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
        }

        public override void UpdateLine(int lineIndex) { }

        /// <summary>
        /// Invalidates the visual lines currently displayed in the viewport, causing them to be refreshed or redrawn.
        /// </summary>
        /// <remarks>Call this method when changes to the underlying document or view state require the
        /// visible lines to be updated. This method has no effect if the visual lines are already invalid or if there
        /// are no visual lines present.</remarks>
        /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
        public void InvalidateViewPortLines()
        {
            ThrowIfDisposed();

            if (!_textView.VisualLinesValid ||
                _textView.VisualLines.Count == 0)
                return;

            InvalidateLineRange(
                _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                _textView.VisualLines[^1].LastDocumentLine.LineNumber - 1);
        }

        public override int GetNumberOfLines()
        {
            return _documentSnapshot.LineCount;
        }

        public override LineText GetLineTextIncludingTerminators(int lineIndex)
        {
            return _documentSnapshot.GetLineTextIncludingTerminatorAsMemory(lineIndex);
        }

        public override int GetLineLength(int lineIndex)
        {
            return _documentSnapshot.GetLineLength(lineIndex);
        }

        private void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            TokenizeViewPort();
        }

        private void DocumentOnChanging(object sender, DocumentChangeEventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            try
            {
                if (e.RemovalLength > 0)
                {
                    var startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
                    var endLine = _document.GetLineByOffset(e.Offset + e.RemovalLength).LineNumber - 1;
                    for (int i = endLine; i > startLine; i--)
                    {
                        RemoveLine(i);
                    }

                    _documentSnapshot.RemoveLines(startLine, endLine);
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void DocumentOnChanged(object sender, DocumentChangeEventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            try
            {
                int startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
                int endLine = startLine;
                if (e.InsertionLength > 0)
                {
                    endLine = _document.GetLineByOffset(e.Offset + e.InsertionLength).LineNumber - 1;

                    for (int i = startLine; i < endLine; i++)
                    {
                        AddLine(i);
                    }
                }

                _documentSnapshot.Update(e);

                if (startLine == 0)
                {
                    SetInvalidRange(startLine, endLine);
                    return;
                }

                // some grammars (JSON, csharp, ...)
                // need to invalidate the previous line too

                SetInvalidRange(startLine - 1, endLine);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void SetInvalidRange(int startLine, int endLine)
        {
            if (!_document.IsInUpdate)
            {
                InvalidateLineRange(startLine, endLine);
                return;
            }

            // we're in a document change, store the max invalid range
            _invalidRange = _invalidRange?.Merge(startLine, endLine) ?? new InvalidLineRange(startLine, endLine);
        }

        private void DocumentOnUpdateFinished(object sender, EventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            if (_invalidRange == null)
                return;

            try
            {
                var range = _invalidRange.Value;
                int startLine = Math.Clamp(range.StartLine, 0, _documentSnapshot.LineCount - 1);
                int endLine = Math.Clamp(range.EndLine, 0, _documentSnapshot.LineCount - 1);
                InvalidateLineRange(startLine, endLine);
            }
            finally
            {
                _invalidRange = null;
            }
        }

        internal void TokenizeViewPort()
        {
            // Post is fire-and-forget - avoids the Task allocation that
            // InvokeAsync incurs. DispatcherPriority.Default is the default
            // for Post, matching the original InvokeAsync behavior
            Dispatcher.UIThread.Post(TokenizeViewPortCore, DispatcherPriority.Default);
        }

        /// <summary>
        /// Performs tokenization on the currently visible lines in the text view.
        /// </summary>
        /// <remarks>
        /// Refactored this method to facilitate testing without relying on the Dispatcher which is complex to manage in unit tests.
        /// This method performs the actual tokenization logic for the viewport, and can be called directly in tests to verify behavior without needing to simulate UI thread dispatching.
        /// This method checks whether the visual lines are valid and present before initiating
        /// tokenization. If the view is disposed or no visual lines are available, the method returns without
        /// performing any action. Any exceptions encountered during tokenization are passed to the configured exception
        /// handler, if one is set.</remarks>
        internal void TokenizeViewPortCore()
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            try
            {
                if (!_textView.VisualLinesValid ||
                    _textView.VisualLines.Count == 0)
                    return;

                ForceTokenization(
                    _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                    _textView.VisualLines[^1].LastDocumentLine.LineNumber - 1);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this instance has been disposed.
        /// Uses <see cref="Volatile.Read(ref readonly bool)"/> for a lock-free memory-barrier-safe
        /// read paired with <see cref="Volatile.Write(ref bool, bool)"/> in
        /// <see cref="Dispose"/>.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _isDisposed))
                throw new ObjectDisposedException(nameof(TextEditorModel));
        }

        /// <summary>
        /// Represents a range of line numbers that is considered invalid within a given context.
        /// NOTE: this is a struct rather than a class to avoid heap allocations when tracking
        /// invalid ranges during document updates, which can occur frequently and may involve multiple ranges.
        /// </summary>
        /// <remarks>This struct is intended for internal use to track and manipulate invalid line ranges,
        /// such as those encountered during parsing or validation operations. The range is inclusive of both the start
        /// and end lines.</remarks>
        internal readonly struct InvalidLineRange
        {
            internal int StartLine { get; }
            internal int EndLine { get; }

            internal InvalidLineRange(int startLine, int endLine)
            {
                StartLine = startLine;
                EndLine = endLine;
            }

            internal InvalidLineRange Merge(int startLine, int endLine)
            {
                return new InvalidLineRange(
                    Math.Min(startLine, StartLine),
                    Math.Max(endLine, EndLine));
            }
        }
    }
}

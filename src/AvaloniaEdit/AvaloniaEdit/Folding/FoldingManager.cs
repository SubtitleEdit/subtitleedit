// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using Avalonia.Threading;

namespace AvaloniaEdit.Folding
{
    /// <summary>
    /// Stores a list of foldings for a specific TextView and TextDocument.
    /// </summary>
    public class FoldingManager
    {
        internal TextDocument Document { get; }
        internal List<TextView> TextViews { get; } = new List<TextView>();

        private readonly TextSegmentCollection<FoldingSection> _foldings;
        private bool _isFirstUpdate = true;

        #region Constructor
        /// <summary>
        /// Creates a new FoldingManager instance.
        /// </summary>
        public FoldingManager(TextDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            _foldings = new TextSegmentCollection<FoldingSection>();
            Dispatcher.UIThread.VerifyAccess();
            TextDocumentWeakEventManager.Changed.AddHandler(document, OnDocumentChanged);
        }
        #endregion

        #region ReceiveWeakEvent

        private void OnDocumentChanged(object sender, DocumentChangeEventArgs e)
        {
            _foldings.UpdateOffsets(e);
            var newEndOffset = e.Offset + e.InsertionLength;
            // extend end offset to the end of the line (including delimiter)
            var endLine = Document.GetLineByOffset(newEndOffset);
            newEndOffset = endLine.Offset + endLine.TotalLength;
            foreach (var affectedFolding in _foldings.FindOverlappingSegments(e.Offset, newEndOffset - e.Offset))
            {
                if (affectedFolding.Length == 0)
                {
                    RemoveFolding(affectedFolding);
                }
                else
                {
                    affectedFolding.ValidateCollapsedLineSections();
                }
            }
        }

        #endregion

        #region Manage TextViews
        internal void AddToTextView(TextView textView)
        {
            if (textView == null || TextViews.Contains(textView))
                throw new ArgumentException();
            TextViews.Add(textView);
            foreach (var fs in _foldings)
            {
                if (fs.CollapsedSections != null)
                {
                    Array.Resize(ref fs.CollapsedSections, TextViews.Count);
                    fs.ValidateCollapsedLineSections();
                }
            }
        }

        internal void RemoveFromTextView(TextView textView)
        {
            var pos = TextViews.IndexOf(textView);
            if (pos < 0)
                throw new ArgumentException();
            TextViews.RemoveAt(pos);
            foreach (var fs in _foldings)
            {
                if (fs.CollapsedSections != null)
                {
                    var c = new CollapsedLineSection[TextViews.Count];
                    Array.Copy(fs.CollapsedSections, 0, c, 0, pos);
                    fs.CollapsedSections[pos].Uncollapse();
                    Array.Copy(fs.CollapsedSections, pos + 1, c, pos, c.Length - pos);
                    fs.CollapsedSections = c;
                }
            }
        }

        internal void Redraw()
        {
            foreach (var textView in TextViews)
                textView.Redraw();
        }

        internal void Redraw(FoldingSection fs)
        {
            foreach (var textView in TextViews)
                textView.Redraw(fs);
        }
        #endregion

        #region Create / Remove / Clear
        /// <summary>
        /// Creates a folding for the specified text section.
        /// </summary>
        public FoldingSection CreateFolding(int startOffset, int endOffset)
        {
            if (startOffset >= endOffset)
                throw new ArgumentException("startOffset must be less than endOffset");
            if (startOffset < 0 || endOffset > Document.TextLength)
                throw new ArgumentException("Folding must be within document boundary");
            var fs = new FoldingSection(this, startOffset, endOffset);
            _foldings.Add(fs);
            Redraw(fs);
            return fs;
        }

        /// <summary>
        /// Removes a folding section from this manager.
        /// </summary>
        public void RemoveFolding(FoldingSection fs)
        {
            if (fs == null)
                throw new ArgumentNullException(nameof(fs));
            fs.IsFolded = false;
            _foldings.Remove(fs);
            Redraw(fs);
        }

        /// <summary>
        /// Removes all folding sections.
        /// </summary>
        public void Clear()
        {
            Dispatcher.UIThread.VerifyAccess();
            foreach (var s in _foldings)
                s.IsFolded = false;
            _foldings.Clear();
            Redraw();
        }
        #endregion

        #region Get...Folding
        /// <summary>
        /// Gets all foldings in this manager.
        /// The foldings are returned sorted by start offset;
        /// for multiple foldings at the same offset the order is undefined.
        /// </summary>
        public IEnumerable<FoldingSection> AllFoldings => _foldings;

        /// <summary>
        /// Gets the first offset greater or equal to <paramref name="startOffset"/> where a folded folding starts.
        /// Returns -1 if there are no foldings after <paramref name="startOffset"/>.
        /// </summary>
        public int GetNextFoldedFoldingStart(int startOffset)
        {
            var fs = _foldings.FindFirstSegmentWithStartAfter(startOffset);
            while (fs != null && !fs.IsFolded)
                fs = _foldings.GetNextSegment(fs);
            return fs?.StartOffset ?? -1;
        }

        /// <summary>
        /// Gets the first folding with a <see cref="TextSegment.StartOffset"/> greater or equal to
        /// <paramref name="startOffset"/>.
        /// Returns null if there are no foldings after <paramref name="startOffset"/>.
        /// </summary>
        public FoldingSection GetNextFolding(int startOffset)
        {
            // TODO: returns the longest folding instead of any folding at the first position after startOffset
            return _foldings.FindFirstSegmentWithStartAfter(startOffset);
        }

        /// <summary>
        /// Gets all foldings that start exactly at <paramref name="startOffset"/>.
        /// </summary>
        public ReadOnlyCollection<FoldingSection> GetFoldingsAt(int startOffset)
        {
            var result = new List<FoldingSection>();
            var fs = _foldings.FindFirstSegmentWithStartAfter(startOffset);
            while (fs != null && fs.StartOffset == startOffset)
            {
                result.Add(fs);
                fs = _foldings.GetNextSegment(fs);
            }
            return new ReadOnlyCollection<FoldingSection>(result);
        }

        /// <summary>
        /// Gets all foldings that contain <paramref name="offset" />.
        /// </summary>
        public ReadOnlyCollection<FoldingSection> GetFoldingsContaining(int offset)
        {
            return _foldings.FindSegmentsContaining(offset);
        }
        #endregion

        #region UpdateFoldings
        /// <summary>
        /// Updates the foldings in this <see cref="FoldingManager"/> using the given new foldings.
        /// This method will try to detect which new foldings correspond to which existing foldings; and will keep the state
        /// (<see cref="FoldingSection.IsFolded"/>) for existing foldings.
        /// </summary>
        /// <param name="newFoldings">The new set of foldings. These must be sorted by starting offset.</param>
        /// <param name="firstErrorOffset">The first position of a parse error. Existing foldings starting after
        /// this offset will be kept even if they don't appear in <paramref name="newFoldings"/>.
        /// Use -1 for this parameter if there were no parse errors.</param>
        public void UpdateFoldings(IEnumerable<NewFolding> newFoldings, int firstErrorOffset)
        {
            if (newFoldings == null)
                throw new ArgumentNullException(nameof(newFoldings));

            if (firstErrorOffset < 0)
                firstErrorOffset = int.MaxValue;

            var oldFoldings = AllFoldings.ToArray();
            var oldFoldingIndex = 0;
            var previousStartOffset = 0;
            // merge new foldings into old foldings so that sections keep being collapsed
            // both oldFoldings and newFoldings are sorted by start offset
            foreach (var newFolding in newFoldings)
            {
                // ensure newFoldings are sorted correctly
                if (newFolding.StartOffset < previousStartOffset)
                    throw new ArgumentException("newFoldings must be sorted by start offset");
                previousStartOffset = newFolding.StartOffset;

                if (newFolding.StartOffset == newFolding.EndOffset)
                    continue; // ignore zero-length foldings

                // remove old foldings that were skipped
                while (oldFoldingIndex < oldFoldings.Length && newFolding.StartOffset > oldFoldings[oldFoldingIndex].StartOffset)
                {
                    RemoveFolding(oldFoldings[oldFoldingIndex++]);
                }
                FoldingSection section;
                // reuse current folding if its matching:
                if (oldFoldingIndex < oldFoldings.Length && newFolding.StartOffset == oldFoldings[oldFoldingIndex].StartOffset)
                {
                    section = oldFoldings[oldFoldingIndex++];
                    section.Length = newFolding.EndOffset - newFolding.StartOffset;
                }
                else
                {
                    // no matching current folding; create a new one:
                    section = CreateFolding(newFolding.StartOffset, newFolding.EndOffset);
                    // auto-close #regions only when opening the document
                    if (_isFirstUpdate)
                    {
                        section.IsFolded = newFolding.DefaultClosed;
                    }
                    section.Tag = newFolding;
                }
                section.Title = newFolding.Name;
            }
            _isFirstUpdate = false;
            // remove all outstanding old foldings:
            while (oldFoldingIndex < oldFoldings.Length)
            {
                var oldSection = oldFoldings[oldFoldingIndex++];
                if (oldSection.StartOffset >= firstErrorOffset)
                    break;
                RemoveFolding(oldSection);
            }
        }
        #endregion

        #region Install
        /// <summary>
        /// Adds Folding support to the specified text area.
        /// Warning: The folding manager is only valid for the text area's current document. The folding manager
        /// must be uninstalled before the text area is bound to a different document.
        /// </summary>
        /// <returns>The <see cref="FoldingManager"/> that manages the list of foldings inside the text area.</returns>
        public static FoldingManager Install(TextArea textArea)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));
            return new FoldingManagerInstallation(textArea);
        }

        /// <summary>
        /// Uninstalls the folding manager.
        /// </summary>
        /// <exception cref="ArgumentException">The specified manager was not created using <see cref="Install"/>.</exception>
        public static void Uninstall(FoldingManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));
            if (manager is FoldingManagerInstallation installation)
            {
                installation.Uninstall();
            }
            else
            {
                throw new ArgumentException("FoldingManager was not created using FoldingManager.Install");
            }
        }

        private sealed class FoldingManagerInstallation : FoldingManager
        {
            private TextArea _textArea;
            private FoldingMargin _margin;
            private FoldingElementGenerator _generator;

            public FoldingManagerInstallation(TextArea textArea) : base(textArea.Document)
            {
                _textArea = textArea;
                _margin = new FoldingMargin { FoldingManager = this };
                _generator = new FoldingElementGenerator { FoldingManager = this };
                textArea.LeftMargins.Add(_margin);
                textArea.TextView.Services.AddService(typeof(FoldingManager), this);
                // HACK: folding only works correctly when it has highest priority
                textArea.TextView.ElementGenerators.Insert(0, _generator);
                textArea.Caret.PositionChanged += TextArea_Caret_PositionChanged;
            }

            /*
			void DemoMode()
			{
				foldingGenerator = new FoldingElementGenerator() { FoldingManager = fm };
				foldingMargin = new FoldingMargin { FoldingManager = fm };
				foldingMarginBorder = new Border {
					Child = foldingMargin,
					Background = new LinearGradientBrush(Colors.White, Colors.Transparent, 0)
				};
				foldingMarginBorder.SizeChanged += UpdateTextViewClip;
				textEditor.TextArea.TextView.ElementGenerators.Add(foldingGenerator);
				textEditor.TextArea.LeftMargins.Add(foldingMarginBorder);
			}
			
			void UpdateTextViewClip(object sender, SizeChangedEventArgs e)
			{
				textEditor.TextArea.TextView.Clip = new RectangleGeometry(
					new Rect(-foldingMarginBorder.ActualWidth,
					         0,
					         textEditor.TextArea.TextView.ActualWidth + foldingMarginBorder.ActualWidth,
					         textEditor.TextArea.TextView.ActualHeight));
			}
			 */

            public void Uninstall()
            {
                Clear();
                if (_textArea != null)
                {
                    _textArea.Caret.PositionChanged -= TextArea_Caret_PositionChanged;
                    _textArea.LeftMargins.Remove(_margin);
                    _textArea.TextView.ElementGenerators.Remove(_generator);
                    _textArea.TextView.Services.RemoveService(typeof(FoldingManager));
                    _margin = null;
                    _generator = null;
                    _textArea = null;
                }
            }

            private void TextArea_Caret_PositionChanged(object sender, EventArgs e)
            {
                // Expand Foldings when Caret is moved into them.
                var caretOffset = _textArea.Caret.Offset;
                foreach (var s in GetFoldingsContaining(caretOffset))
                {
                    if (s.IsFolded && s.StartOffset < caretOffset && caretOffset < s.EndOffset)
                    {
                        s.IsFolded = false;
                    }
                }
            }
        }
        #endregion
    }
}

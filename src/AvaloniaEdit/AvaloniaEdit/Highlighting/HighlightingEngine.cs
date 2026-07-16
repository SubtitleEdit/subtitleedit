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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AvaloniaEdit.Document;
using SpanStack = System.Collections.Immutable.ImmutableStack<AvaloniaEdit.Highlighting.HighlightingSpan>;

namespace AvaloniaEdit.Highlighting
{
    /// <summary>
    /// Regex-based highlighting engine.
    /// </summary>
    public class HighlightingEngine
    {
        private SpanStack _spanStack = SpanStack.Empty;

        /// <summary>
        /// Creates a new HighlightingEngine instance.
        /// </summary>
        public HighlightingEngine(HighlightingRuleSet mainRuleSet)
        {
            CurrentRuleSet = mainRuleSet ?? throw new ArgumentNullException(nameof(mainRuleSet));
        }

        /// <summary>
        /// Gets/sets the current span stack.
        /// </summary>
        public SpanStack CurrentSpanStack
        {
            get => _spanStack;
            set => _spanStack = value ?? SpanStack.Empty;
        }

        #region Highlighting Engine

        // local variables from HighlightLineInternal (are member because they are accessed by HighlighLine helper methods)
        private string _lineText;

        private int _lineStartOffset;
        private int _position;

        /// <summary>
        /// the HighlightedLine where highlighting output is being written to.
        /// if this variable is null, nothing is highlighted and only the span state is updated
        /// </summary>
        private HighlightedLine _highlightedLine;

        /// <summary>
        /// Highlights the specified line in the specified document.
        /// 
        /// Before calling this method, <see cref="CurrentSpanStack"/> must be set to the proper
        /// state for the beginning of this line. After highlighting has completed,
        /// <see cref="CurrentSpanStack"/> will be updated to represent the state after the line.
        /// </summary>
        public HighlightedLine HighlightLine(IDocument document, IDocumentLine line)
        {
            _lineStartOffset = line.Offset;
            _lineText = document.GetText(line);
            try
            {
                _highlightedLine = new HighlightedLine(document, line);
                HighlightLineInternal();
                return _highlightedLine;
            }
            finally
            {
                _highlightedLine = null;
                _lineText = null;
                _lineStartOffset = 0;
            }
        }

        /// <summary>
        /// Updates <see cref="CurrentSpanStack"/> for the specified line in the specified document.
        /// 
        /// Before calling this method, <see cref="CurrentSpanStack"/> must be set to the proper
        /// state for the beginning of this line. After highlighting has completed,
        /// <see cref="CurrentSpanStack"/> will be updated to represent the state after the line.
        /// </summary>
        public void ScanLine(IDocument document, IDocumentLine line)
        {
            //this.lineStartOffset = line.Offset; not necessary for scanning
            _lineText = document.GetText(line);
            try
            {
                Debug.Assert(_highlightedLine == null);
                HighlightLineInternal();
            }
            finally
            {
                _lineText = null;
            }
        }

        private void HighlightLineInternal()
        {
            _position = 0;
            ResetColorStack();
            var currentRuleSet = CurrentRuleSet;
            var storedMatchArrays = new Stack<Match[]>();
            var matches = AllocateMatchArray(currentRuleSet.Spans.Count);
            Match endSpanMatch = null;

            while (true)
            {
                for (var i = 0; i < matches.Length; i++)
                {
                    if (matches[i] == null || (matches[i].Success && matches[i].Index < _position))
                        matches[i] = currentRuleSet.Spans[i].StartExpression.Match(_lineText, _position);
                }
                if (!_spanStack.IsEmpty)
                    endSpanMatch = _spanStack.Peek().EndExpression.Match(_lineText, _position);

                var firstMatch = Minimum(matches, endSpanMatch);
                if (firstMatch == null)
                    break;

                HighlightNonSpans(firstMatch.Index);

                Debug.Assert(_position == firstMatch.Index);

                if (firstMatch == endSpanMatch)
                {
                    var poppedSpan = _spanStack.Peek();
                    if (!poppedSpan.SpanColorIncludesEnd)
                        PopColor(); // pop SpanColor
                    PushColor(poppedSpan.EndColor);
                    _position = firstMatch.Index + firstMatch.Length;
                    PopColor(); // pop EndColor
                    if (poppedSpan.SpanColorIncludesEnd)
                        PopColor(); // pop SpanColor
                    _spanStack = _spanStack.Pop();
                    currentRuleSet = CurrentRuleSet;
                    //FreeMatchArray(matches);
                    if (storedMatchArrays.Count > 0)
                    {
                        matches = storedMatchArrays.Pop();
                        var index = currentRuleSet.Spans.IndexOf(poppedSpan);
                        Debug.Assert(index >= 0 && index < matches.Length);
                        if (matches[index].Index == _position)
                        {
                            throw new InvalidOperationException(
                                "A highlighting span matched 0 characters, which would cause an endless loop.\n" +
                                "Change the highlighting definition so that either the start or the end regex matches at least one character.\n" +
                                "Start regex: " + poppedSpan.StartExpression + "\n" +
                                "End regex: " + poppedSpan.EndExpression);
                        }
                    }
                    else
                    {
                        matches = AllocateMatchArray(currentRuleSet.Spans.Count);
                    }
                }
                else
                {
                    var index = Array.IndexOf(matches, firstMatch);
                    Debug.Assert(index >= 0);
                    var newSpan = currentRuleSet.Spans[index];
                    _spanStack = _spanStack.Push(newSpan);
                    currentRuleSet = CurrentRuleSet;
                    storedMatchArrays.Push(matches);
                    matches = AllocateMatchArray(currentRuleSet.Spans.Count);
                    if (newSpan.SpanColorIncludesStart)
                        PushColor(newSpan.SpanColor);
                    PushColor(newSpan.StartColor);
                    _position = firstMatch.Index + firstMatch.Length;
                    PopColor();
                    if (!newSpan.SpanColorIncludesStart)
                        PushColor(newSpan.SpanColor);
                }
                endSpanMatch = null;
            }
            HighlightNonSpans(_lineText.Length);

            PopAllColors();
        }

        private void HighlightNonSpans(int until)
        {
            Debug.Assert(_position <= until);
            if (_position == until)
                return;
            if (_highlightedLine != null)
            {
                var rules = CurrentRuleSet.Rules;
                var matches = AllocateMatchArray(rules.Count);
                while (true)
                {
                    for (var i = 0; i < matches.Length; i++)
                    {
                        if (matches[i] == null || (matches[i].Success && matches[i].Index < _position))
                            matches[i] = rules[i].Regex.Match(_lineText, _position, until - _position);
                    }
                    var firstMatch = Minimum(matches, null);
                    if (firstMatch == null)
                        break;

                    _position = firstMatch.Index;
                    var ruleIndex = Array.IndexOf(matches, firstMatch);
                    if (firstMatch.Length == 0)
                    {
                        throw new InvalidOperationException(
                            "A highlighting rule matched 0 characters, which would cause an endless loop.\n" +
                            "Change the highlighting definition so that the rule matches at least one character.\n" +
                            "Regex: " + rules[ruleIndex].Regex);
                    }
                    PushColor(rules[ruleIndex].Color);
                    _position = firstMatch.Index + firstMatch.Length;
                    PopColor();
                }
                //FreeMatchArray(matches);
            }
            _position = until;
        }

        private static readonly HighlightingRuleSet EmptyRuleSet = new HighlightingRuleSet { Name = "EmptyRuleSet" };

        private HighlightingRuleSet CurrentRuleSet
        {
            get
            {
                if (_spanStack.IsEmpty)
                    return field;
                return _spanStack.Peek().RuleSet ?? EmptyRuleSet;
            }
        }
        #endregion

        #region Color Stack Management

        private Stack<HighlightedSection> _highlightedSectionStack;
        private HighlightedSection _lastPoppedSection;

        private void ResetColorStack()
        {
            Debug.Assert(_position == 0);
            _lastPoppedSection = null;
            if (_highlightedLine == null)
            {
                _highlightedSectionStack = null;
            }
            else
            {
                _highlightedSectionStack = new Stack<HighlightedSection>();
                foreach (var span in _spanStack.Reverse())
                {
                    PushColor(span.SpanColor);
                }
            }
        }

        private void PushColor(HighlightingColor color)
        {
            if (_highlightedLine == null)
                return;
            if (color == null)
            {
                _highlightedSectionStack.Push(null);
            }
            else if (_lastPoppedSection != null && Equals(_lastPoppedSection.Color, color)
                     && _lastPoppedSection.Offset + _lastPoppedSection.Length == _position + _lineStartOffset)
            {
                _highlightedSectionStack.Push(_lastPoppedSection);
                _lastPoppedSection = null;
            }
            else
            {
                var hs = new HighlightedSection
                {
                    Offset = _position + _lineStartOffset,
                    Color = color
                };
                _highlightedLine.Sections.Add(hs);
                _highlightedSectionStack.Push(hs);
                _lastPoppedSection = null;
            }
        }

        private void PopColor()
        {
            if (_highlightedLine == null)
                return;
            var s = _highlightedSectionStack.Pop();
            if (s != null)
            {
                s.Length = (_position + _lineStartOffset) - s.Offset;
                if (s.Length == 0)
                    _highlightedLine.Sections.Remove(s);
                else
                    _lastPoppedSection = s;
            }
        }

        private void PopAllColors()
        {
            if (_highlightedSectionStack != null)
            {
                while (_highlightedSectionStack.Count > 0)
                    PopColor();
            }
        }
        #endregion

        #region Match helpers
        /// <summary>
        /// Returns the first match from the array or endSpanMatch.
        /// </summary>
        private static Match Minimum(Match[] arr, Match endSpanMatch)
        {
            Match min = null;
            foreach (var v in arr)
            {
                if (v.Success && (min == null || v.Index < min.Index))
                    min = v;
            }
            if (endSpanMatch != null && endSpanMatch.Success && (min == null || endSpanMatch.Index < min.Index))
                return endSpanMatch;
            return min;
        }

        private static Match[] AllocateMatchArray(int count)
        {
            if (count == 0)
                return Array.Empty<Match>();
            return new Match[count];
        }
        #endregion
    }
}

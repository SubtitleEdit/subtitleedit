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
using System.Diagnostics;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

using LogicalDirection = AvaloniaEdit.Document.LogicalDirection;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// Represents a visual line in the document.
    /// A visual line usually corresponds to one DocumentLine, but it can span multiple lines if
    /// all but the first are collapsed.
    /// </summary>
    public sealed class VisualLine
    {
        public const int LENGTH_LIMIT = 3000;

        private enum LifetimePhase : byte
        {
            Generating,
            Transforming,
            Live,
            Disposed
        }

        private List<VisualLineElement> _elements;
        internal bool HasInlineObjects;
        private LifetimePhase _phase;

        /// <summary>
        /// Gets the <see cref="TextView"/> to which this <see cref="VisualLine"/> belongs.
        /// </summary>
        internal TextView TextView { get; }

        /// <summary>
        /// Gets the document to which this VisualLine belongs.
        /// </summary>
        public TextDocument Document => TextView.Document;

        /// <summary>
        /// Gets the first document line displayed by this visual line.
        /// </summary>
        public DocumentLine FirstDocumentLine { get; }

        /// <summary>
        /// Gets the last document line displayed by this visual line.
        /// </summary>
        public DocumentLine LastDocumentLine { get; private set; }

        /// <summary>
        /// Gets a read-only collection of line elements.
        /// </summary>
        public ReadOnlyCollection<VisualLineElement> Elements { get; private set; }

        private ReadOnlyCollection<TextLine> _textLines;

        /// <summary>
        /// Gets a read-only collection of text lines.
        /// </summary>
        public ReadOnlyCollection<TextLine> TextLines
        {
            get
            {
                if (_phase < LifetimePhase.Live)
                    throw new InvalidOperationException();
                return _textLines;
            }
        }

        /// <summary>
        /// Gets the start offset of the VisualLine inside the document.
        /// This is equivalent to <c>FirstDocumentLine.Offset</c>.
        /// </summary>
        public int StartOffset => FirstDocumentLine.Offset;

        /// <summary>
        /// Length in visual line coordinates.
        /// </summary>
        public int VisualLength { get; private set; }

        /// <summary>
        /// Length in visual line coordinates including the end of line marker, if TextEditorOptions.ShowEndOfLine is enabled.
        /// </summary>
        public int VisualLengthWithEndOfLineMarker
        {
            get
            {
                var length = VisualLength;
                if (TextView.Options.ShowEndOfLine && LastDocumentLine.NextLine != null) length++;
                return length;
            }
        }

        /// <summary>
        /// Gets the height of the visual line in device-independent pixels.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Gets the Y position of the line. This is measured in device-independent pixels relative to the start of the document.
        /// </summary>
        public double VisualTop { get; internal set; }

        internal VisualLine(TextView textView, DocumentLine firstDocumentLine)
        {
            Debug.Assert(textView != null);
            Debug.Assert(firstDocumentLine != null);
            TextView = textView;
            FirstDocumentLine = firstDocumentLine;
        }

        internal void ConstructVisualElements(ITextRunConstructionContext context, IReadOnlyList<VisualLineElementGenerator> generators)
        {
            Debug.Assert(_phase == LifetimePhase.Generating);
            foreach (var g in generators)
            {
                g.StartGeneration(context);
            }
            _elements = new List<VisualLineElement>();
            PerformVisualElementConstruction(generators);
            foreach (var g in generators)
            {
                g.FinishGeneration();
            }

            var globalTextRunProperties = context.GlobalTextRunProperties;
            foreach (var element in _elements)
            {
                element.SetTextRunProperties(new VisualLineElementTextRunProperties(globalTextRunProperties));
            }
            this.Elements = new ReadOnlyCollection<VisualLineElement>(_elements);
            CalculateOffsets();
            _phase = LifetimePhase.Transforming;
        }

        void PerformVisualElementConstruction(IReadOnlyList<VisualLineElementGenerator> generators)
        {
            var lineLength = FirstDocumentLine.Length;
            var offset = FirstDocumentLine.Offset;
            var currentLineEnd = offset + lineLength;
            LastDocumentLine = FirstDocumentLine;
            var askInterestOffset = 0; // 0 or 1

            // for long lines, do not run the element generators for performance reasons
            if (lineLength > LENGTH_LIMIT)
            {
                _elements.Add(new VisualLineText(this, lineLength));
                return;
            }

            while (offset + askInterestOffset <= currentLineEnd)
            {
                var textPieceEndOffset = currentLineEnd;
                foreach (var g in generators)
                {
                    g.CachedInterest = g.GetFirstInterestedOffset(offset + askInterestOffset);
                    if (g.CachedInterest != -1)
                    {
                        if (g.CachedInterest < offset)
                            throw new ArgumentOutOfRangeException(g.GetType().Name + ".GetFirstInterestedOffset",
                                                                  g.CachedInterest,
                                                                  "GetFirstInterestedOffset must not return an offset less than startOffset. Return -1 to signal no interest.");
                        if (g.CachedInterest < textPieceEndOffset)
                            textPieceEndOffset = g.CachedInterest;
                    }
                }
                Debug.Assert(textPieceEndOffset >= offset);
                if (textPieceEndOffset > offset)
                {
                    var textPieceLength = textPieceEndOffset - offset;

                    _elements.Add(new VisualLineText(this, textPieceLength));

                    offset = textPieceEndOffset;
                }
                // If no elements constructed / only zero-length elements constructed:
                // do not asking the generators again for the same location (would cause endless loop)
                askInterestOffset = 1;
                foreach (var g in generators)
                {
                    if (g.CachedInterest == offset)
                    {
                        var element = g.ConstructElement(offset);
                        if (element != null)
                        {
                            _elements.Add(element);
                            if (element.DocumentLength > 0)
                            {
                                // a non-zero-length element was constructed
                                askInterestOffset = 0;
                                offset += element.DocumentLength;
                                if (offset > currentLineEnd)
                                {
                                    var newEndLine = Document.GetLineByOffset(offset);
                                    currentLineEnd = newEndLine.Offset + newEndLine.Length;
                                    this.LastDocumentLine = newEndLine;
                                    if (currentLineEnd < offset)
                                    {
                                        throw new InvalidOperationException(
                                            "The VisualLineElementGenerator " + g.GetType().Name +
                                            " produced an element which ends within the line delimiter");
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void CalculateOffsets()
        {
            var visualOffset = 0;
            var textOffset = 0;
            foreach (var element in _elements)
            {
                element.VisualColumn = visualOffset;
                element.RelativeTextOffset = textOffset;
                visualOffset += element.VisualLength;
                textOffset += element.DocumentLength;
            }
            VisualLength = visualOffset;
            Debug.Assert(textOffset == LastDocumentLine.EndOffset - FirstDocumentLine.Offset);
        }

        internal void RunTransformers(ITextRunConstructionContext context, IReadOnlyList<IVisualLineTransformer> transformers)
        {
            Debug.Assert(_phase == LifetimePhase.Transforming);
            foreach (var transformer in transformers)
            {
                transformer.Transform(context, _elements);
            }
            _phase = LifetimePhase.Live;
        }

        /// <summary>
        /// Replaces the single element at <paramref name="elementIndex"/> with the specified elements.
        /// The replacement operation must preserve the document length, but may change the visual length.
        /// </summary>
        /// <remarks>
        /// This method may only be called by line transformers.
        /// </remarks>
        public void ReplaceElement(int elementIndex, params VisualLineElement[] newElements)
        {
            ReplaceElement(elementIndex, 1, newElements);
        }

        /// <summary>
        /// Replaces <paramref name="count"/> elements starting at <paramref name="elementIndex"/> with the specified elements.
        /// The replacement operation must preserve the document length, but may change the visual length.
        /// </summary>
        /// <remarks>
        /// This method may only be called by line transformers.
        /// </remarks>
        public void ReplaceElement(int elementIndex, int count, params VisualLineElement[] newElements)
        {
            if (_phase != LifetimePhase.Transforming)
                throw new InvalidOperationException("This method may only be called by line transformers.");
            var oldDocumentLength = 0;
            for (var i = elementIndex; i < elementIndex + count; i++)
            {
                oldDocumentLength += _elements[i].DocumentLength;
            }
            var newDocumentLength = 0;
            foreach (var newElement in newElements)
            {
                newDocumentLength += newElement.DocumentLength;
            }
            if (oldDocumentLength != newDocumentLength)
                throw new InvalidOperationException("Old elements have document length " + oldDocumentLength + ", but new elements have length " + newDocumentLength);
            _elements.RemoveRange(elementIndex, count);
            _elements.InsertRange(elementIndex, newElements);
            CalculateOffsets();
        }

        internal void SetTextLines(List<TextLine> textLines)
        {
            double defaultLineHeight = TextView.DefaultLineHeight;
            _textLines = new ReadOnlyCollection<TextLine>(textLines);

            Height = 0;
            foreach (var line in textLines)
                Height += Math.Max(line.Height, defaultLineHeight);
        }

        /// <summary>
        /// Gets the visual column from a document offset relative to the first line start.
        /// </summary>
        public int GetVisualColumn(int relativeTextOffset)
        {
            ThrowUtil.CheckNotNegative(relativeTextOffset, "relativeTextOffset");
            foreach (var element in _elements)
            {
                if (element.RelativeTextOffset <= relativeTextOffset
                    && element.RelativeTextOffset + element.DocumentLength >= relativeTextOffset)
                {
                    return element.GetVisualColumn(relativeTextOffset);
                }
            }
            return VisualLength;
        }

        /// <summary>
        /// Gets the document offset (relative to the first line start) from a visual column.
        /// </summary>
        public int GetRelativeOffset(int visualColumn)
        {
            ThrowUtil.CheckNotNegative(visualColumn, "visualColumn");
            var documentLength = 0;
            foreach (var element in _elements)
            {
                if (element.VisualColumn <= visualColumn
                    && element.VisualColumn + element.VisualLength > visualColumn)
                {
                    return element.GetRelativeOffset(visualColumn);
                }
                documentLength += element.DocumentLength;
            }
            return documentLength;
        }

        /// <summary>
        /// Gets the text line containing the specified visual column.
        /// </summary>
        public TextLine GetTextLine(int visualColumn)
        {
            return GetTextLine(visualColumn, false);
        }

        /// <summary>
        /// Gets the text line containing the specified visual column.
        /// </summary>
        public TextLine GetTextLine(int visualColumn, bool isAtEndOfLine)
        {
            if (visualColumn < 0)
                throw new ArgumentOutOfRangeException(nameof(visualColumn));
            if (visualColumn >= VisualLengthWithEndOfLineMarker)
                return TextLines[TextLines.Count - 1];
            foreach (var line in TextLines)
            {
                if (isAtEndOfLine ? visualColumn <= line.Length : visualColumn < line.Length)
                    return line;
                visualColumn -= line.Length;
            }
            throw new InvalidOperationException("Shouldn't happen (VisualLength incorrect?)");
        }

        /// <summary>
        /// Gets the visual Y position from the specified text line.
        /// </summary>
        /// <param name="textLine">
        /// The line of text. (Note: This may also include inline UI elements, such as buttons.)
        /// </param>
        /// <param name="yPositionMode">The reference position within the line.</param>
        /// <returns>
        /// The distance in device-independent pixels from the top of the document to reference
        /// position within the specified text line.
        /// </returns>
        public double GetTextLineVisualYPosition(TextLine textLine, VisualYPosition yPositionMode)
        {
            if (textLine == null)
                throw new ArgumentNullException(nameof(textLine));

            var defaultLineHeight = TextView.DefaultLineHeight;
            var pos = VisualTop;
            foreach (var tl in TextLines)
            {
                double textHeight = tl.Height;
                double lineHeight = Math.Max(textHeight, defaultLineHeight);

                if (tl == textLine)
                {
                    double textOffset = (lineHeight - textHeight) / 2;

                    switch (yPositionMode)
                    {
                        case VisualYPosition.LineTop:
                            return pos;
                        case VisualYPosition.LineMiddle:
                            return pos + lineHeight / 2;
                        case VisualYPosition.LineBottom:
                            return pos + lineHeight;
                        case VisualYPosition.TextTop:
                            return pos + textOffset + tl.Baseline - TextView.DefaultBaseline;
                        case VisualYPosition.TextBottom:
                            return pos + textOffset + tl.Baseline - TextView.DefaultBaseline + TextView.DefaultTextHeight;
                        case VisualYPosition.TextMiddle:
                            return pos + textOffset + tl.Baseline - TextView.DefaultBaseline + TextView.DefaultTextHeight / 2;
                        case VisualYPosition.Baseline:
                            return pos + textOffset + tl.Baseline;
                        default:
                            throw new ArgumentException("Invalid yPositionMode:" + yPositionMode);
                    }
                }

                pos += lineHeight;
            }
            throw new ArgumentException("textLine is not a line in this VisualLine");
        }

        /// <summary>
        /// Gets the start visual column from the specified text line.
        /// </summary>
        public int GetTextLineVisualStartColumn(TextLine textLine)
        {
            if (!TextLines.Contains(textLine))
                throw new ArgumentException("textLine is not a line in this VisualLine");

            return TextLines.TakeWhile(tl => tl != textLine).Sum(tl => tl.Length);
        }

        /// <summary>
        /// Gets a TextLine by the visual position.
        /// </summary>
        public TextLine GetTextLineByVisualYPosition(double visualTop)
        {
            const double epsilon = 0.0001;
            double pos = VisualTop;
            double defaultLineHeight = TextView.DefaultLineHeight;
            foreach (var tl in TextLines)
            {
                pos += Math.Max(tl.Height, defaultLineHeight);
                if (visualTop + epsilon < pos)
                    return tl;
            }

            return TextLines[TextLines.Count - 1];
        }

        /// <summary>
        /// Gets the visual position from the specified visualColumn.
        /// </summary>
        /// <returns>Position in device-independent pixels
        /// relative to the top left of the document.</returns>
        public Point GetVisualPosition(int visualColumn, VisualYPosition yPositionMode)
        {
            var textLine = GetTextLine(visualColumn);
            var xPos = GetTextLineVisualXPosition(textLine, visualColumn);
            var yPos = GetTextLineVisualYPosition(textLine, yPositionMode);
            return new Point(xPos, yPos);
        }

        internal Point GetVisualPosition(int visualColumn, bool isAtEndOfLine, VisualYPosition yPositionMode)
        {
            var textLine = GetTextLine(visualColumn, isAtEndOfLine);
            var xPos = GetTextLineVisualXPosition(textLine, visualColumn);
            var yPos = GetTextLineVisualYPosition(textLine, yPositionMode);
            return new Point(xPos, yPos);
        }

        /// <summary>
        /// Gets the distance to the left border of the text area of the specified visual column.
        /// The visual column must belong to the specified text line.
        /// </summary>
        public double GetTextLineVisualXPosition(TextLine textLine, int visualColumn)
        {
            if (textLine == null)
                throw new ArgumentNullException(nameof(textLine));

            var xPos = textLine.GetDistanceFromCharacterHit(new CharacterHit(Math.Min(visualColumn,
                VisualLengthWithEndOfLineMarker)));

            if (visualColumn > VisualLengthWithEndOfLineMarker)
            {
                xPos += (visualColumn - VisualLengthWithEndOfLineMarker) * TextView.WideSpaceWidth;
            }

            return xPos;
        }

        /// <summary>
        /// Gets the visual column from a document position (relative to top left of the document).
        /// If the user clicks between two visual columns, rounds to the nearest column.
        /// </summary>
        public int GetVisualColumn(Point point)
        {
            return GetVisualColumn(point, TextView.Options.EnableVirtualSpace);
        }

        /// <summary>
        /// Gets the visual column from a document position (relative to top left of the document).
        /// If the user clicks between two visual columns, rounds to the nearest column.
        /// </summary>
        public int GetVisualColumn(Point point, bool allowVirtualSpace)
        {
            return GetVisualColumn(GetTextLineByVisualYPosition(point.Y), point.X, allowVirtualSpace);
        }

        internal int GetVisualColumn(Point point, bool allowVirtualSpace, out bool isAtEndOfLine)
        {
            var textLine = GetTextLineByVisualYPosition(point.Y);
            var vc = GetVisualColumn(textLine, point.X, allowVirtualSpace);
            isAtEndOfLine = (vc >= GetTextLineVisualStartColumn(textLine) + textLine.Length);
            return vc;
        }

        /// <summary>
        /// Gets the visual column from a document position (relative to top left of the document).
        /// If the user clicks between two visual columns, rounds to the nearest column.
        /// </summary>
        public int GetVisualColumn(TextLine textLine, double xPos, bool allowVirtualSpace)
        {
            if (xPos > textLine.WidthIncludingTrailingWhitespace)
            {
                if (allowVirtualSpace && textLine == TextLines[TextLines.Count - 1])
                {
                    var virtualX = (int)Math.Round((xPos - textLine.WidthIncludingTrailingWhitespace) / TextView.WideSpaceWidth, MidpointRounding.AwayFromZero);
                    return VisualLengthWithEndOfLineMarker + virtualX;
                }
            }

            var ch = textLine.GetCharacterHitFromDistance(xPos);

            return ch.FirstCharacterIndex + ch.TrailingLength;
        }

        /// <summary>
        /// Validates the visual column and returns the correct one.
        /// </summary>
        public int ValidateVisualColumn(TextViewPosition position, bool allowVirtualSpace)
        {
            return ValidateVisualColumn(Document.GetOffset(position.Location), position.VisualColumn, allowVirtualSpace);
        }

        /// <summary>
        /// Validates the visual column and returns the correct one.
        /// </summary>
        public int ValidateVisualColumn(int offset, int visualColumn, bool allowVirtualSpace)
        {
            var firstDocumentLineOffset = FirstDocumentLine.Offset;
            if (visualColumn < 0)
            {
                return GetVisualColumn(offset - firstDocumentLineOffset);
            }
            var offsetFromVisualColumn = GetRelativeOffset(visualColumn);
            offsetFromVisualColumn += firstDocumentLineOffset;
            if (offsetFromVisualColumn != offset)
            {
                return GetVisualColumn(offset - firstDocumentLineOffset);
            }
            if (visualColumn > VisualLength && !allowVirtualSpace)
            {
                return VisualLength;
            }
            return visualColumn;
        }

        /// <summary>
        /// Gets the visual column from a document position (relative to top left of the document).
        /// If the user clicks between two visual columns, returns the first of those columns.
        /// </summary>
        public int GetVisualColumnFloor(Point point)
        {
            return GetVisualColumnFloor(point, TextView.Options.EnableVirtualSpace);
        }

        /// <summary>
        /// Gets the visual column from a document position (relative to top left of the document).
        /// If the user clicks between two visual columns, returns the first of those columns.
        /// </summary>
        public int GetVisualColumnFloor(Point point, bool allowVirtualSpace)
        {
            return GetVisualColumnFloor(point, allowVirtualSpace, out _);
        }

        internal int GetVisualColumnFloor(Point point, bool allowVirtualSpace, out bool isAtEndOfLine)
        {
            var textLine = GetTextLineByVisualYPosition(point.Y);
            if (point.X > textLine.WidthIncludingTrailingWhitespace)
            {
                isAtEndOfLine = true;
                if (allowVirtualSpace && textLine == TextLines[TextLines.Count - 1])
                {
                    // clicking virtual space in the last line
                    var virtualX = (int)((point.X - textLine.WidthIncludingTrailingWhitespace) / TextView.WideSpaceWidth);
                    return VisualLengthWithEndOfLineMarker + virtualX;
                }

                // GetCharacterHitFromDistance returns a hit with FirstCharacterIndex=last character in line
                // and TrailingLength=1 when clicking behind the line, so the floor function needs to handle this case
                // specially and return the line's end column instead.
                return GetTextLineVisualStartColumn(textLine) + textLine.Length;
            }

            isAtEndOfLine = false;

            var ch = textLine.GetCharacterHitFromDistance(point.X);

            return ch.FirstCharacterIndex;
        }

        /// <summary>
        /// Gets the text view position from the specified visual column.
        /// </summary>
        public TextViewPosition GetTextViewPosition(int visualColumn)
        {
            var documentOffset = GetRelativeOffset(visualColumn) + FirstDocumentLine.Offset;
            return new TextViewPosition(Document.GetLocation(documentOffset), visualColumn);
        }

        /// <summary>
        /// Gets the text view position from the specified visual position.
        /// If the position is within a character, it is rounded to the next character boundary.
        /// </summary>
        /// <param name="visualPosition">The position in device-independent pixels relative
        /// to the top left corner of the document.</param>
        /// <param name="allowVirtualSpace">Controls whether positions in virtual space may be returned.</param>
        public TextViewPosition GetTextViewPosition(Point visualPosition, bool allowVirtualSpace)
        {
            var visualColumn = GetVisualColumn(visualPosition, allowVirtualSpace, out var isAtEndOfLine);
            var documentOffset = GetRelativeOffset(visualColumn) + FirstDocumentLine.Offset;
            var pos = new TextViewPosition(Document.GetLocation(documentOffset), visualColumn)
            {
                IsAtEndOfLine = isAtEndOfLine
            };
            return pos;
        }

        /// <summary>
        /// Gets the text view position from the specified visual position.
        /// If the position is inside a character, the position in front of the character is returned.
        /// </summary>
        /// <param name="visualPosition">The position in device-independent pixels relative
        /// to the top left corner of the document.</param>
        /// <param name="allowVirtualSpace">Controls whether positions in virtual space may be returned.</param>
        public TextViewPosition GetTextViewPositionFloor(Point visualPosition, bool allowVirtualSpace)
        {
            var visualColumn = GetVisualColumnFloor(visualPosition, allowVirtualSpace, out var isAtEndOfLine);
            var documentOffset = GetRelativeOffset(visualColumn) + FirstDocumentLine.Offset;
            var pos = new TextViewPosition(Document.GetLocation(documentOffset), visualColumn)
            {
                IsAtEndOfLine = isAtEndOfLine
            };
            return pos;
        }

        /// <summary>
        /// Gets whether the visual line was disposed.
        /// </summary>
        public bool IsDisposed => _phase == LifetimePhase.Disposed;

        internal void Dispose()
        {
            if (_phase == LifetimePhase.Disposed)
            {
                return;
            }

            Debug.Assert(_phase == LifetimePhase.Live);

            _phase = LifetimePhase.Disposed;

            if (_visual != null)
            {
                ((ISetLogicalParent)_visual).SetParent(null);
            }
        }

        /// <summary>
        /// Gets the next possible caret position after visualColumn, or -1 if there is no caret position.
        /// </summary>
        public int GetNextCaretPosition(int visualColumn, LogicalDirection direction, CaretPositioningMode mode, bool allowVirtualSpace)
        {
            if (!HasStopsInVirtualSpace(mode))
                allowVirtualSpace = false;

            if (_elements.Count == 0)
            {
                // special handling for empty visual lines:
                if (allowVirtualSpace)
                {
                    if (direction == LogicalDirection.Forward)
                        return Math.Max(0, visualColumn + 1);
                    if (visualColumn > 0)
                        return visualColumn - 1;
                    return -1;
                }
                // even though we don't have any elements,
                // there's a single caret stop at visualColumn 0
                if (visualColumn < 0 && direction == LogicalDirection.Forward)
                    return 0;
                if (visualColumn > 0 && direction == LogicalDirection.Backward)
                    return 0;
                return -1;
            }

            int i;
            if (direction == LogicalDirection.Backward)
            {
                // Search Backwards:
                // If the last element doesn't handle line borders, return the line end as caret stop

                if (visualColumn > VisualLength && !_elements[_elements.Count - 1].HandlesLineBorders && HasImplicitStopAtLineEnd())
                {
                    if (allowVirtualSpace)
                        return visualColumn - 1;
                    return VisualLength;
                }
                // skip elements that start after or at visualColumn
                for (i = _elements.Count - 1; i >= 0; i--)
                {
                    if (_elements[i].VisualColumn < visualColumn)
                        break;
                }
                // search last element that has a caret stop
                for (; i >= 0; i--)
                {
                    var pos = _elements[i].GetNextCaretPosition(
                        Math.Min(visualColumn, _elements[i].VisualColumn + _elements[i].VisualLength + 1),
                        direction, mode);
                    if (pos >= 0)
                        return pos;
                }
                // If we've found nothing, and the first element doesn't handle line borders,
                // return the line start as normal caret stop.
                if (visualColumn > 0 && !_elements[0].HandlesLineBorders && HasImplicitStopAtLineStart(mode))
                    return 0;
            }
            else
            {
                // Search Forwards:
                // If the first element doesn't handle line borders, return the line start as caret stop
                if (visualColumn < 0 && !_elements[0].HandlesLineBorders && HasImplicitStopAtLineStart(mode))
                    return 0;
                // skip elements that end before or at visualColumn
                for (i = 0; i < _elements.Count; i++)
                {
                    if (_elements[i].VisualColumn + _elements[i].VisualLength > visualColumn)
                        break;
                }
                // search first element that has a caret stop
                for (; i < _elements.Count; i++)
                {
                    var pos = _elements[i].GetNextCaretPosition(
                        Math.Max(visualColumn, _elements[i].VisualColumn - 1),
                        direction, mode);
                    if (pos >= 0)
                        return pos;
                }
                // if we've found nothing, and the last element doesn't handle line borders,
                // return the line end as caret stop
                if ((allowVirtualSpace || !_elements[_elements.Count - 1].HandlesLineBorders) && HasImplicitStopAtLineEnd())
                {
                    if (visualColumn < VisualLength)
                        return VisualLength;
                    if (allowVirtualSpace)
                        return visualColumn + 1;
                }
            }
            // we've found nothing, return -1 and let the caret search continue in the next line
            return -1;
        }

        private static bool HasStopsInVirtualSpace(CaretPositioningMode mode)
        {
            return mode == CaretPositioningMode.Normal || mode == CaretPositioningMode.EveryCodepoint;
        }

        private static bool HasImplicitStopAtLineStart(CaretPositioningMode mode)
        {
            return mode == CaretPositioningMode.Normal || mode == CaretPositioningMode.EveryCodepoint;
        }

        private static bool HasImplicitStopAtLineEnd() => true;

        private VisualLineDrawingVisual _visual;

        internal VisualLineDrawingVisual Render()
        {
            Debug.Assert(_phase == LifetimePhase.Live);

            if (_visual == null)
            {
                _visual = new VisualLineDrawingVisual(this);

                ((ISetLogicalParent)_visual).SetParent(TextView);
            }

            return _visual;
        }
    }

    // TODO: can inherit from Layoutable, but dev tools crash
    internal sealed class VisualLineDrawingVisual : Control
    {
        public VisualLine VisualLine { get; }
        public double LineHeight => VisualLine.Height;
        internal bool IsAdded { get; set; }

        public VisualLineDrawingVisual(VisualLine visualLine)
        {
            VisualLine = visualLine;
        }

        public override void Render(DrawingContext context)
        {
            double pos = 0;
            double defaultLineHeight = VisualLine.TextView.DefaultLineHeight;
            var textLines = VisualLine.TextLines;
            for (var i = 0; i < textLines.Count; i++)
            {
                var textLine = textLines[i];
                double textHeight = textLine.Height;
                double lineHeight = Math.Max(textHeight, defaultLineHeight);
                double textOffset = (lineHeight - textHeight) / 2;
                textLine.Draw(context, new Point(0, pos + textOffset));
                pos += lineHeight;
            }
        }
    }
}

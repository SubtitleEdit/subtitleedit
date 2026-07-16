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
using System.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Utils;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// Helper for creating a PathGeometry.
	/// </summary>
	public sealed class BackgroundGeometryBuilder
	{
		private double _cornerRadius;

		/// <summary>
		/// Gets/sets the radius of the rounded corners.
		/// </summary>
		public double CornerRadius {
			get { return _cornerRadius; }
			set { _cornerRadius = value; }
		}

		/// <summary>
		/// Gets/Sets whether to align to whole pixels.
		/// 
		/// If BorderThickness is set to 0, the geometry is aligned to whole pixels.
		/// If BorderThickness is set to a non-zero value, the outer edge of the border is aligned
		/// to whole pixels.
		/// 
		/// The default value is <c>false</c>.
		/// </summary>
		public bool AlignToWholePixels { get; set; }

		/// <summary>
		/// Gets/sets the border thickness.
		/// 
		/// This property only has an effect if <c>AlignToWholePixels</c> is enabled.
		/// When using the resulting geometry to paint a border, set this property to the border thickness.
		/// Otherwise, leave the property set to the default value <c>0</c>.
		/// </summary>
		public double BorderThickness { get; set; }

		/// <summary>
		/// Gets/Sets whether to extend the rectangles to full width at line end.
		/// </summary>
		public bool ExtendToFullWidthAtLineEnd { get; set; }

		/// <summary>
		/// Creates a new BackgroundGeometryBuilder instance.
		/// </summary>
		public BackgroundGeometryBuilder()
		{
		}

		/// <summary>
		/// Adds the specified segment to the geometry.
		/// </summary>
		public void AddSegment(TextView textView, ISegment segment)
		{
			if (textView == null)
				throw new ArgumentNullException("textView");
			Size pixelSize = PixelSnapHelpers.GetPixelSize(textView);
			foreach (Rect r in GetRectsForSegment(textView, segment, ExtendToFullWidthAtLineEnd)) {
				AddRectangle(pixelSize, r);
			}
		}

		/// <summary>
		/// Adds a rectangle to the geometry.
		/// </summary>
		/// <remarks>
		/// This overload will align the coordinates according to
		/// <see cref="AlignToWholePixels"/>.
		/// Use the <see cref="AddRectangle(double,double,double,double)"/>-overload instead if the coordinates should not be aligned.
		/// </remarks>
		public void AddRectangle(TextView textView, Rect rectangle)
		{
			AddRectangle(PixelSnapHelpers.GetPixelSize(textView), rectangle);
		}

		private void AddRectangle(Size pixelSize, Rect r)
		{
			if (AlignToWholePixels) {
				double halfBorder = 0.5 * BorderThickness;
				AddRectangle(PixelSnapHelpers.Round(r.Left - halfBorder, pixelSize.Width) + halfBorder,
							 PixelSnapHelpers.Round(r.Top - halfBorder, pixelSize.Height) + halfBorder,
							 PixelSnapHelpers.Round(r.Right + halfBorder, pixelSize.Width) - halfBorder,
							 PixelSnapHelpers.Round(r.Bottom + halfBorder, pixelSize.Height) - halfBorder);
				//Debug.WriteLine(r.ToString() + " -> " + new Rect(lastLeft, lastTop, lastRight-lastLeft, lastBottom-lastTop).ToString());
			} else {
				AddRectangle(r.Left, r.Top, r.Right, r.Bottom);
			}
		}

		/// <summary>
		/// Calculates the list of rectangle where the segment in shown.
		/// This method usually returns one rectangle for each line inside the segment
		/// (but potentially more, e.g. when bidirectional text is involved).
		/// </summary>
		public static IEnumerable<Rect> GetRectsForSegment(TextView textView, ISegment segment, bool extendToFullWidthAtLineEnd = false)
		{
			if (textView == null)
				throw new ArgumentNullException("textView");
			if (segment == null)
				throw new ArgumentNullException("segment");
			return GetRectsForSegmentImpl(textView, segment, extendToFullWidthAtLineEnd);
		}

		private static IEnumerable<Rect> GetRectsForSegmentImpl(TextView textView, ISegment segment, bool extendToFullWidthAtLineEnd)
		{
			int segmentStart = segment.Offset;
			int segmentEnd = segment.Offset + segment.Length;

			segmentStart = segmentStart.CoerceValue(0, textView.Document.TextLength);
			segmentEnd = segmentEnd.CoerceValue(0, textView.Document.TextLength);

			TextViewPosition start;
			TextViewPosition end;

			if (segment is SelectionSegment) {
				SelectionSegment sel = (SelectionSegment)segment;
				start = new TextViewPosition(textView.Document.GetLocation(sel.StartOffset), sel.StartVisualColumn);
				end = new TextViewPosition(textView.Document.GetLocation(sel.EndOffset), sel.EndVisualColumn);
			} else {
				start = new TextViewPosition(textView.Document.GetLocation(segmentStart));
				end = new TextViewPosition(textView.Document.GetLocation(segmentEnd));
			}

			foreach (VisualLine vl in textView.VisualLines) {
				int vlStartOffset = vl.FirstDocumentLine.Offset;
				if (vlStartOffset > segmentEnd)
					break;
				int vlEndOffset = vl.LastDocumentLine.Offset + vl.LastDocumentLine.Length;
				if (vlEndOffset < segmentStart)
					continue;

				int segmentStartVc;
				if (segmentStart < vlStartOffset)
					segmentStartVc = 0;
				else
					segmentStartVc = vl.ValidateVisualColumn(start, extendToFullWidthAtLineEnd);

				int segmentEndVc;
				if (segmentEnd > vlEndOffset)
					segmentEndVc = extendToFullWidthAtLineEnd ? int.MaxValue : vl.VisualLengthWithEndOfLineMarker;
				else
					segmentEndVc = vl.ValidateVisualColumn(end, extendToFullWidthAtLineEnd);

				foreach (var rect in ProcessTextLines(textView, vl, segmentStartVc, segmentEndVc))
					yield return rect;
			}
		}

		/// <summary>
		/// Calculates the rectangles for the visual column segment.
		/// This returns one rectangle for each line inside the segment.
		/// </summary>
		public static IEnumerable<Rect> GetRectsFromVisualSegment(TextView textView, VisualLine line, int startVc, int endVc)
		{
			if (textView == null)
				throw new ArgumentNullException("textView");
			if (line == null)
				throw new ArgumentNullException("line");
			return ProcessTextLines(textView, line, startVc, endVc);
		}

		private static IEnumerable<Rect> ProcessTextLines(TextView textView, VisualLine visualLine, int segmentStartVc, int segmentEndVc)
		{
			TextLine lastTextLine = visualLine.TextLines.Last();
			Vector scrollOffset = textView.ScrollOffset;

			for (int i = 0; i < visualLine.TextLines.Count; i++) {
				TextLine line = visualLine.TextLines[i];
				double y = visualLine.GetTextLineVisualYPosition(line, VisualYPosition.LineTop);
                double lineHeight = Math.Max(line.Height, textView.DefaultLineHeight);
				int visualStartCol = visualLine.GetTextLineVisualStartColumn(line);
				int visualEndCol = visualStartCol + line.Length;
				if (line == lastTextLine)
					visualEndCol -= 1; // 1 position for the TextEndOfParagraph
				else
					visualEndCol -= line.TrailingWhitespaceLength;

				if (segmentEndVc < visualStartCol)
					break;
				if (lastTextLine != line && segmentStartVc > visualEndCol)
					continue;
				int segmentStartVcInLine = Math.Max(segmentStartVc, visualStartCol);
				int segmentEndVcInLine = Math.Min(segmentEndVc, visualEndCol);
				y -= scrollOffset.Y;
				Rect lastRect = default;
				if (segmentStartVcInLine == segmentEndVcInLine) {
					// GetTextBounds crashes for length=0, so we'll handle this case with GetDistanceFromCharacterHit
					// We need to return a rectangle to ensure empty lines are still visible
					double pos = visualLine.GetTextLineVisualXPosition(line, segmentStartVcInLine);
					pos -= scrollOffset.X;
					// The following special cases are necessary to get rid of empty rectangles at the end of a TextLine if "Show Spaces" is active.
					// If not excluded once, the same rectangle is calculated (and added) twice (since the offset could be mapped to two visual positions; end/start of line), if there is no trailing whitespace.
					// Skip this TextLine segment, if it is at the end of this line and this line is not the last line of the VisualLine and the selection continues and there is no trailing whitespace.
					if (segmentEndVcInLine == visualEndCol && i < visualLine.TextLines.Count - 1 && segmentEndVc > segmentEndVcInLine && line.TrailingWhitespaceLength == 0)
						continue;
					if (segmentStartVcInLine == visualStartCol && i > 0 && segmentStartVc < segmentStartVcInLine && visualLine.TextLines[i - 1].TrailingWhitespaceLength == 0)
						continue;

					lastRect = new Rect(pos, y, textView.EmptyLineSelectionWidth, lineHeight);
				} else {
					if (segmentStartVcInLine <= visualEndCol) {
						foreach (var b in line.GetTextBounds(segmentStartVcInLine, segmentEndVcInLine - segmentStartVcInLine)) {
							double left = b.Rectangle.Left - scrollOffset.X;
							double right = b.Rectangle.Right - scrollOffset.X;
							if (lastRect != default)
								yield return lastRect;
							// left>right is possible in RTL languages
							lastRect = new Rect(Math.Min(left, right), y, Math.Abs(right - left), lineHeight);
						}
					}
				}
				// If the segment ends in virtual space, extend the last rectangle with the rectangle the portion of the selection
				// after the line end.
				// Also, when word-wrap is enabled and the segment continues into the next line, extend lastRect up to the end of the line.
				if (segmentEndVc > visualEndCol) {
					double left, right;
					if (segmentStartVc > visualLine.VisualLengthWithEndOfLineMarker) {
						// segmentStartVC is in virtual space
						left = visualLine.GetTextLineVisualXPosition(lastTextLine, segmentStartVc);
					} else {
						// Otherwise, we already processed the rects from segmentStartVC up to visualEndCol,
						// so we only need to do the remainder starting at visualEndCol.
						// For word-wrapped lines, visualEndCol doesn't include the whitespace hidden by the wrap,
						// so we'll need to include it here.
						// For the last line, visualEndCol already includes the whitespace.
						left = (line == lastTextLine ? line.WidthIncludingTrailingWhitespace : line.Width);
					}
					if (line != lastTextLine || segmentEndVc == int.MaxValue) {
						// If word-wrap is enabled and the segment continues into the next line,
						// or if the extendToFullWidthAtLineEnd option is used (segmentEndVC == int.MaxValue),
						// we select the full width of the viewport.
						right = Math.Max(((ILogicalScrollable)textView).Extent.Width, ((ILogicalScrollable)textView).Viewport.Width);
					} else {
						right = visualLine.GetTextLineVisualXPosition(lastTextLine, segmentEndVc);
					}

					left -= scrollOffset.X;
					right -= scrollOffset.X;
					Rect extendSelection = new Rect(Math.Min(left, right), y, Math.Abs(right - left), lineHeight);
					if (lastRect != default) {
						if (extendSelection.Intersects(lastRect)) {
							lastRect = lastRect.Union(extendSelection);
							yield return lastRect;
						} else {
							// If the end of the line is in an RTL segment, keep lastRect and extendSelection separate.
							yield return lastRect;
							yield return extendSelection;
						}
					} else
						yield return extendSelection;
				} else
					yield return lastRect;
			}
		}

		private readonly PathFigures _figures = new PathFigures();
		private PathFigure _figure;
		private int _insertionIndex;
		private double _lastTop, _lastBottom;
		private double _lastLeft, _lastRight;

		/// <summary>
		/// Adds a rectangle to the geometry.
		/// </summary>
		/// <remarks>
		/// This overload assumes that the coordinates are aligned properly
		/// (see <see cref="AlignToWholePixels"/>).
		/// Use the <see cref="AddRectangle(TextView,Rect)"/>-overload instead if the coordinates are not yet aligned.
		/// </remarks>
		public void AddRectangle(double left, double top, double right, double bottom)
		{
			if (!top.IsClose(_lastBottom)) {
				CloseFigure();
			}
			if (_figure == null) {
				_figure = new PathFigure();
				_figure.StartPoint = new Point(left, top + _cornerRadius);
				if (Math.Abs(left - right) > _cornerRadius) {
					_figure.Segments.Add(MakeArc(left + _cornerRadius, top, SweepDirection.Clockwise));
					_figure.Segments.Add(MakeLineSegment(right - _cornerRadius, top));
					_figure.Segments.Add(MakeArc(right, top + _cornerRadius, SweepDirection.Clockwise));
				}
				_figure.Segments.Add(MakeLineSegment(right, bottom - _cornerRadius));
				_insertionIndex = _figure.Segments.Count;
				//figure.Segments.Add(MakeArc(left, bottom - cornerRadius, SweepDirection.Clockwise));
			} else {
				if (!_lastRight.IsClose(right)) {
					double cr = right < _lastRight ? -_cornerRadius : _cornerRadius;
					SweepDirection dir1 = right < _lastRight ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
					SweepDirection dir2 = right < _lastRight ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
					_figure.Segments.Insert(_insertionIndex++, MakeArc(_lastRight + cr, _lastBottom, dir1));
					_figure.Segments.Insert(_insertionIndex++, MakeLineSegment(right - cr, top));
					_figure.Segments.Insert(_insertionIndex++, MakeArc(right, top + _cornerRadius, dir2));
				}
				_figure.Segments.Insert(_insertionIndex++, MakeLineSegment(right, bottom - _cornerRadius));
				_figure.Segments.Insert(_insertionIndex, MakeLineSegment(_lastLeft, _lastTop + _cornerRadius));
				if (!_lastLeft.IsClose(left)) {
					double cr = left < _lastLeft ? _cornerRadius : -_cornerRadius;
					SweepDirection dir1 = left < _lastLeft ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
					SweepDirection dir2 = left < _lastLeft ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
					_figure.Segments.Insert(_insertionIndex, MakeArc(_lastLeft, _lastBottom - _cornerRadius, dir1));
					_figure.Segments.Insert(_insertionIndex, MakeLineSegment(_lastLeft - cr, _lastBottom));
					_figure.Segments.Insert(_insertionIndex, MakeArc(left + cr, _lastBottom, dir2));
				}
			}
			this._lastTop = top;
			this._lastBottom = bottom;
			this._lastLeft = left;
			this._lastRight = right;
		}

		private ArcSegment MakeArc(double x, double y, SweepDirection dir)
		{
			var arc = new ArcSegment
			{
				Point = new Point(x, y),
				Size = new Size(CornerRadius, CornerRadius),
				SweepDirection = dir
			};
			return arc;
		}

		private static LineSegment MakeLineSegment(double x, double y)
		{
			return new LineSegment { Point = new Point(x, y) };
		}

		/// <summary>
		/// Closes the current figure.
		/// </summary>
		public void CloseFigure()
		{
			if (_figure != null) {
				_figure.Segments.Insert(_insertionIndex, MakeLineSegment(_lastLeft, _lastTop + _cornerRadius));
				if (Math.Abs(_lastLeft - _lastRight) > _cornerRadius) {
					_figure.Segments.Insert(_insertionIndex, MakeArc(_lastLeft, _lastBottom - _cornerRadius, SweepDirection.Clockwise));
					_figure.Segments.Insert(_insertionIndex, MakeLineSegment(_lastLeft + _cornerRadius, _lastBottom));
					_figure.Segments.Insert(_insertionIndex, MakeArc(_lastRight - _cornerRadius, _lastBottom, SweepDirection.Clockwise));
				}

				_figure.IsClosed = true;
				_figures.Add(_figure);
				_figure = null;
			}
		}

		/// <summary>
		/// Creates the geometry.
		/// Returns null when the geometry is empty!
		/// </summary>
		public Geometry CreateGeometry()
		{
			CloseFigure();
			return _figures.Count != 0 ? new PathGeometry { Figures = _figures } : null;
		}
	}
}

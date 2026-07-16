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
using System.Diagnostics;
using System.Globalization;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// Represents a simple segment (Offset,Length pair) that is not automatically updated
    /// on document changes.
    /// </summary>
    public readonly struct SimpleSegment : IEquatable<SimpleSegment>, ISegment
    {
        public static readonly SimpleSegment Invalid = new SimpleSegment(-1, -1);

        /// <summary>
        /// Gets the overlapping portion of the segments.
        /// Returns SimpleSegment.Invalid if the segments don't overlap.
        /// </summary>
        public static SimpleSegment GetOverlap(ISegment segment1, ISegment segment2)
        {
            var start = Math.Max(segment1.Offset, segment2.Offset);
            var end = Math.Min(segment1.EndOffset, segment2.EndOffset);
            return end < start ? Invalid : new SimpleSegment(start, end - start);
        }

        public readonly int Offset, Length;

        int ISegment.Offset => Offset;

        int ISegment.Length => Length;

        public int EndOffset => Offset + Length;

        public SimpleSegment(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        public SimpleSegment(ISegment segment)
        {
            Debug.Assert(segment != null);
            Offset = segment.Offset;
            Length = segment.Length;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Offset + 10301 * Length;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is SimpleSegment) && Equals((SimpleSegment)obj);
        }

        public bool Equals(SimpleSegment other)
        {
            return Offset == other.Offset && Length == other.Length;
        }

        public static bool operator ==(SimpleSegment left, SimpleSegment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SimpleSegment left, SimpleSegment right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"[Offset={Offset}, Length={Length}]");
        }
    }

    /// <summary>
    /// A segment using <see cref="TextAnchor"/>s as start and end positions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For the constructors creating new anchors, the start position will be AfterInsertion and the end position will be BeforeInsertion.
    /// Should the end position move before the start position, the segment will have length 0.
    /// </para>
    /// </remarks>
    /// <seealso cref="ISegment"/>
    /// <seealso cref="TextSegment"/>
    public sealed class AnchorSegment : ISegment
    {
        private readonly TextAnchor _start;
        private readonly TextAnchor _end;

        /// <inheritdoc/>
        public int Offset => _start.Offset;

        /// <inheritdoc/>
        public int Length => Math.Max(0, _end.Offset - _start.Offset);

        /// <inheritdoc/>
        public int EndOffset => Math.Max(_start.Offset, _end.Offset);

        /// <summary>
        /// Creates a new AnchorSegment using the specified anchors.
        /// The anchors must have <see cref="TextAnchor.SurviveDeletion"/> set to true.
        /// </summary>
        public AnchorSegment(TextAnchor start, TextAnchor end)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));
            if (end == null)
                throw new ArgumentNullException(nameof(end));
            if (!start.SurviveDeletion)
                throw new ArgumentException("Anchors for AnchorSegment must use SurviveDeletion", nameof(start));
            if (!end.SurviveDeletion)
                throw new ArgumentException("Anchors for AnchorSegment must use SurviveDeletion", nameof(end));
            _start = start;
            _end = end;
        }

        /// <summary>
        /// Creates a new AnchorSegment that creates new anchors.
        /// </summary>
        public AnchorSegment(TextDocument document, ISegment segment)
            : this(document, ThrowUtil.CheckNotNull(segment, "segment").Offset, segment.Length)
        {
        }

        /// <summary>
        /// Creates a new AnchorSegment that creates new anchors.
        /// </summary>
        public AnchorSegment(TextDocument document, int offset, int length)
        {
            _start = document?.CreateAnchor(offset) ?? throw new ArgumentNullException(nameof(document));
            _start.SurviveDeletion = true;
            _start.MovementType = AnchorMovementType.AfterInsertion;
            _end = document.CreateAnchor(offset + length);
            _end.SurviveDeletion = true;
            _end.MovementType = AnchorMovementType.BeforeInsertion;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"[{nameof(Offset)}={Offset}, {nameof(EndOffset)}={EndOffset}]");
        }
    }
}

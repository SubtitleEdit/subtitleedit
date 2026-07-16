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

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// Represents a string with a segment.
    /// Similar to System.ArraySegment&lt;T&gt;, but for strings instead of arrays.
    /// </summary>
    public readonly struct StringSegment : IEquatable<StringSegment>
    {
        /// <summary>
        /// Creates a new StringSegment.
        /// </summary>
        public StringSegment(string text, int offset, int count)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (offset < 0 || offset > text.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + count > text.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            Text = text;
            Offset = offset;
            Count = count;
        }

        /// <summary>
        /// Creates a new StringSegment.
        /// </summary>
        public StringSegment(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Offset = 0;
            Count = text.Length;
        }

        /// <summary>
        /// Gets the string used for this segment.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the start offset of the segment with the text.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the length of the segment.
        /// </summary>
        public int Count { get; }

        #region Equals and GetHashCode implementation
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is StringSegment)
                return Equals((StringSegment)obj); // use Equals method below
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(StringSegment other)
        {
            // add comparisons for all members here
            return ReferenceEquals(Text, other.Text) && Offset == other.Offset && Count == other.Count;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Text.GetHashCode() ^ Offset ^ Count;
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(StringSegment left, StringSegment right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(StringSegment left, StringSegment right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}

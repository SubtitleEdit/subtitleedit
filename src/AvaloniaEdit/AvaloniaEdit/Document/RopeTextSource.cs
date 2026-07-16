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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Document
{
	/// <summary>
	/// Implements the ITextSource interface using a rope.
	/// </summary>
	public sealed class RopeTextSource : ITextSource
	{
	    private readonly Rope<char> _rope;

        /// <summary>
        /// Creates a new RopeTextSource.
        /// </summary>
        public RopeTextSource(Rope<char> rope)
		{
		    _rope = rope?.Clone() ?? throw new ArgumentNullException(nameof(rope));
		}
		
		/// <summary>
		/// Creates a new RopeTextSource.
		/// </summary>
		public RopeTextSource(Rope<char> rope, ITextSourceVersion version)
		{
		    _rope = rope?.Clone() ?? throw new ArgumentNullException(nameof(rope));
			Version = version;
		}
		
		/// <summary>
		/// Returns a clone of the rope used for this text source.
		/// </summary>
		/// <remarks>
		/// RopeTextSource only publishes a copy of the contained rope to ensure that the underlying rope cannot be modified.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="Not a property because it creates a clone")]
		public Rope<char> GetRope()
		{
			return _rope.Clone();
		}
		
		/// <inheritdoc/>
		public string Text => _rope.ToString();

	    /// <inheritdoc/>
		public int TextLength => _rope.Length;

	    /// <inheritdoc/>
		public char GetCharAt(int offset)
		{
			return _rope[offset];
		}
		
		/// <inheritdoc/>
		public string GetText(int offset, int length)
		{
			return _rope.ToString(offset, length);
		}

		/// <inheritdoc/>
		public ReadOnlyMemory<char> GetTextAsMemory(int offset, int length)
		{
			return _rope.GetMemory(offset, length);
		}

		/// <inheritdoc/>
		public string GetText(ISegment segment)
		{
			return _rope.ToString(segment.Offset, segment.Length);
		}
		
		/// <inheritdoc/>
		public TextReader CreateReader()
		{
			return new RopeTextReader(_rope);
		}
		
		/// <inheritdoc/>
		public TextReader CreateReader(int offset, int length)
		{
			return new RopeTextReader(_rope.GetRange(offset, length));
		}
		
		/// <inheritdoc/>
		public ITextSource CreateSnapshot()
		{
			return this;
		}
		
		/// <inheritdoc/>
		public ITextSource CreateSnapshot(int offset, int length)
		{
			return new RopeTextSource(_rope.GetRange(offset, length));
		}
		
		/// <inheritdoc/>
		public int IndexOf(char c, int startIndex, int count)
		{
			return _rope.IndexOf(c, startIndex, count);
		}
		
		/// <inheritdoc/>
		public int IndexOfAny(char[] anyOf, int startIndex, int count)
		{
			return _rope.IndexOfAny(anyOf, startIndex, count);
		}
		
		/// <inheritdoc/>
		public int LastIndexOf(char c, int startIndex, int count)
		{
			return _rope.LastIndexOf(c, startIndex, count);
		}

        /// <inheritdoc/>
        public ITextSourceVersion Version { get; }

        /// <inheritdoc/>
        public int IndexOf(string searchText, int startIndex, int count, StringComparison comparisonType)
		{
			return _rope.IndexOf(searchText, startIndex, count, comparisonType);
		}
		
		/// <inheritdoc/>
		public int LastIndexOf(string searchText, int startIndex, int count, StringComparison comparisonType)
		{
			return _rope.LastIndexOf(searchText, startIndex, count, comparisonType);
		}
		
		/// <inheritdoc/>
		public void WriteTextTo(TextWriter writer)
		{
			_rope.WriteTo(writer, 0, _rope.Length);
		}
		
		/// <inheritdoc/>
		public void WriteTextTo(TextWriter writer, int offset, int length)
		{
			_rope.WriteTo(writer, offset, length);
		}
	}
}

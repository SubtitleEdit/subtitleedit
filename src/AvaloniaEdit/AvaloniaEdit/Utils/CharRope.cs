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
using System.IO;

namespace AvaloniaEdit.Utils
{
	/// <summary>
	/// Poor man's template specialization: extension methods for Rope&lt;char&gt;.
	/// </summary>
	public static class CharRope
	{
		/// <summary>
		/// Creates a new rope from the specified text.
		/// </summary>
		public static Rope<char> Create(string text)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			return new Rope<char>(InitFromString(text));
		}
		
		/// <summary>
		/// Retrieves the text for a portion of the rope.
		/// Runs in O(lg N + M), where M=<paramref name="length"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">offset or length is outside the valid range.</exception>
		/// <remarks>
		/// This method counts as a read access and may be called concurrently to other read accesses.
		/// </remarks>
		public static string ToString(this Rope<char> rope, int startIndex, int length)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			#if DEBUG
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Value must be >= 0");
			#endif
			if (length == 0)
				return string.Empty;
			
#if NET6_0_OR_GREATER
			return string.Create(length, (rope, startIndex, length), (dest, x) =>
				x.rope.CopyTo(x.startIndex, dest, 0, x.length));
#else
			char[] buffer = new char[length];
			rope.CopyTo(startIndex, buffer, 0, length);
			return new string(buffer);
#endif
		}

		/// <summary>
		/// Retrieves the text for a portion of the rope as ReadOnlyMemory.
		/// Runs in O(lg N + M), where M=<paramref name="length"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">offset or length is outside the valid range.</exception>
		/// <remarks>
		/// This method counts as a read access and may be called concurrently to other read accesses.
		/// When the requested range falls entirely within a single leaf node, this method returns
		/// a slice of the internal buffer without allocation. Otherwise, a new buffer is allocated.
		/// </remarks>
		public static ReadOnlyMemory<char> GetMemory(this Rope<char> rope, int startIndex, int length)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));

			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Value must be >= 0");

			if (length == 0)
				return ReadOnlyMemory<char>.Empty;

			rope.VerifyRange(startIndex, length);

			// Try to get a zero-allocation slice if the range fits within a single leaf node
			var entry = rope.FindNodeUsingCache(startIndex).PeekOrDefault();
			int offsetWithinNode = startIndex - entry.NodeStartIndex;

			// Check if the entire requested range fits within this leaf node
			if (offsetWithinNode + length <= entry.Node.Length)
			{
				// Return a slice of the existing buffer - no allocation needed
				return new ReadOnlyMemory<char>(entry.Node.Contents, offsetWithinNode, length);
			}

			// Range spans multiple nodes - must allocate and copy
			char[] buffer = new char[length];
			rope.CopyTo(startIndex, buffer, 0, length);
			return new ReadOnlyMemory<char>(buffer);
		}
		
		/// <summary>
		/// Retrieves the text for a portion of the rope and writes it to the specified text writer.
		/// Runs in O(lg N + M), where M=<paramref name="length"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">offset or length is outside the valid range.</exception>
		/// <remarks>
		/// This method counts as a read access and may be called concurrently to other read accesses.
		/// </remarks>
		public static void WriteTo(this Rope<char> rope, TextWriter output, int startIndex, int length)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			if (output == null)
				throw new ArgumentNullException(nameof(output));
			rope.VerifyRange(startIndex, length);
			rope.Root.WriteTo(startIndex, output, length);
		}
		
		/// <summary>
		/// Appends text to this rope.
		/// Runs in O(lg N + M).
		/// </summary>
		/// <exception cref="ArgumentNullException">newElements is null.</exception>
		public static void AddText(this Rope<char> rope, string text)
		{
			InsertText(rope, rope.Length, text);
		}
		
		/// <summary>
		/// Inserts text into this rope.
		/// Runs in O(lg N + M).
		/// </summary>
		/// <exception cref="ArgumentNullException">newElements is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">index or length is outside the valid range.</exception>
		public static void InsertText(this Rope<char> rope, int index, string text)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			rope.InsertRange(index, text.ToCharArray(), 0, text.Length);
			/*if (index < 0 || index > rope.Length) {
				throw new ArgumentOutOfRangeException("index", index, "0 <= index <= " + rope.Length.ToString(CultureInfo.InvariantCulture));
			}
			if (text == null)
				throw new ArgumentNullException("text");
			if (text.Length == 0)
				return;
			rope.root = rope.root.Insert(index, text);
			rope.OnChanged();*/
		}
		
		internal static RopeNode<char> InitFromString(string text)
		{
			if (text.Length == 0) {
				return RopeNode<char>.EmptyRopeNode;
			}
			RopeNode<char> node = RopeNode<char>.CreateNodes(text.Length);
			FillNode(node, text, 0);
			return node;
		}
		
		static void FillNode(RopeNode<char> node, string text, int start)
		{
			if (node.Contents != null) {
				text.CopyTo(start, node.Contents, 0, node.Length);
			} else {
				FillNode(node.Left, text, start);
				FillNode(node.Right, text, start + node.Left.Length);
			}
		}
		
		internal static void WriteTo(this RopeNode<char> node, int index, TextWriter output, int count)
		{
			if (node.Height == 0) {
				if (node.Contents == null) {
					// function node
					node.GetContentNode().WriteTo(index, output, count);
				} else {
					// leaf node: append data
					output.Write(node.Contents, index, count);
				}
			} else {
				// concat node: do recursive calls
				if (index + count <= node.Left.Length) {
					node.Left.WriteTo(index, output, count);
				} else if (index >= node.Left.Length) {
					node.Right.WriteTo(index - node.Left.Length, output, count);
				} else {
					int amountInLeft = node.Left.Length - index;
					node.Left.WriteTo(index, output, amountInLeft);
					node.Right.WriteTo(0, output, count - amountInLeft);
				}
			}
		}
		
		/// <summary>
		/// Gets the index of the first occurrence of any element in the specified array.
		/// </summary>
		/// <param name="rope">The target rope.</param>
		/// <param name="anyOf">Array of characters being searched.</param>
		/// <param name="startIndex">Start index of the search.</param>
		/// <param name="length">Length of the area to search.</param>
		/// <returns>The first index where any character was found; or -1 if no occurrence was found.</returns>
		public static int IndexOfAny(this Rope<char> rope, char[] anyOf, int startIndex, int length)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			if (anyOf == null)
				throw new ArgumentNullException(nameof(anyOf));
			rope.VerifyRange(startIndex, length);
			
			while (length > 0) {
				var entry = rope.FindNodeUsingCache(startIndex).PeekOrDefault();
				char[] contents = entry.Node.Contents;
				int startWithinNode = startIndex - entry.NodeStartIndex;
				int nodeLength = Math.Min(entry.Node.Length, startWithinNode + length);
				for (int i = startIndex - entry.NodeStartIndex; i < nodeLength; i++) {
					char element = contents[i];
					foreach (char needle in anyOf) {
						if (element == needle)
							return entry.NodeStartIndex + i;
					}
				}
				length -= nodeLength - startWithinNode;
				startIndex = entry.NodeStartIndex + nodeLength;
			}
			return -1;
		}
		
		/// <summary>
		/// Gets the index of the first occurrence of the search text.
		/// </summary>
		public static int IndexOf(this Rope<char> rope, string searchText, int startIndex, int length, StringComparison comparisonType)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			if (searchText == null)
				throw new ArgumentNullException(nameof(searchText));
			rope.VerifyRange(startIndex, length);
			int pos = rope.ToString(startIndex, length).IndexOf(searchText, comparisonType);
			if (pos < 0)
				return -1;
			else
				return pos + startIndex;
		}
		
		/// <summary>
		/// Gets the index of the last occurrence of the search text.
		/// </summary>
		public static int LastIndexOf(this Rope<char> rope, string searchText, int startIndex, int length, StringComparison comparisonType)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			if (searchText == null)
				throw new ArgumentNullException(nameof(searchText));
			rope.VerifyRange(startIndex, length);
			int pos = rope.ToString(startIndex, length).LastIndexOf(searchText, comparisonType);
			if (pos < 0)
				return -1;
			else
				return pos + startIndex;
		}
	}
}

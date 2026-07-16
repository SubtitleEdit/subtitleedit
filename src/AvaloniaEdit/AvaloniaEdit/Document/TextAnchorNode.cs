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
using System.Globalization;

namespace AvaloniaEdit.Document
{
	/// <summary>
	/// A TextAnchorNode is placed in the TextAnchorTree.
	/// It describes a section of text with a text anchor at the end of the section.
	/// A weak reference is used to refer to the TextAnchor. (to save memory, we derive from WeakReference instead of referencing it)
	/// </summary>
	internal sealed class TextAnchorNode : WeakReference
	{
		internal TextAnchorNode Left { get; set; }
	    internal TextAnchorNode Right { get; set; }
        internal TextAnchorNode Parent { get; set; }
        internal bool Color { get; set; }
        internal int Length { get; set; }
        internal int TotalLength { get; set; } // totalLength = length + left.totalLength + right.totalLength

        public TextAnchorNode(TextAnchor anchor) : base(anchor)
		{
		}
		
		internal TextAnchorNode LeftMost {
			get {
				TextAnchorNode node = this;
				while (node.Left != null)
					node = node.Left;
				return node;
			}
		}
		
		internal TextAnchorNode RightMost {
			get {
				TextAnchorNode node = this;
				while (node.Right != null)
					node = node.Right;
				return node;
			}
		}
		
		/// <summary>
		/// Gets the inorder successor of the node.
		/// </summary>
		internal TextAnchorNode Successor {
			get {
				if (Right != null) {
					return Right.LeftMost;
				} else {
					TextAnchorNode node = this;
					TextAnchorNode oldNode;
					do {
						oldNode = node;
						node = node.Parent;
						// go up until we are coming out of a left subtree
					} while (node != null && node.Right == oldNode);
					return node;
				}
			}
		}
		
		/// <summary>
		/// Gets the inorder predecessor of the node.
		/// </summary>
		internal TextAnchorNode Predecessor {
			get {
				if (Left != null) {
					return Left.RightMost;
				} else {
					TextAnchorNode node = this;
					TextAnchorNode oldNode;
					do {
						oldNode = node;
						node = node.Parent;
						// go up until we are coming out of a right subtree
					} while (node != null && node.Left == oldNode);
					return node;
				}
			}
		}
		
		public override string ToString()
		{
            return string.Create(CultureInfo.InvariantCulture, $"[{nameof(TextAnchorNode)} {nameof(Length)}={Length}, {nameof(TotalLength)}={TotalLength}, {nameof(Target)}={Target}]");
		}
	}
}

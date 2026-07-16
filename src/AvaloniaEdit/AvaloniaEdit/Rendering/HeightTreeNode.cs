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

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// A node in the text view's height tree.
	/// </summary>
	internal sealed class HeightTreeNode
	{
		internal readonly DocumentLine DocumentLine;
		internal HeightTreeLineNode LineNode;

		internal HeightTreeNode Left, Right, Parent;
		internal bool Color;

		internal HeightTreeNode()
		{
		}

		internal HeightTreeNode(DocumentLine documentLine, double height)
		{
			this.DocumentLine = documentLine;
			this.TotalCount = 1;
			this.LineNode = new HeightTreeLineNode(height);
			this.TotalHeight = height;
		}

		internal HeightTreeNode LeftMost {
			get {
				HeightTreeNode node = this;
				while (node.Left != null)
					node = node.Left;
				return node;
			}
		}

		internal HeightTreeNode RightMost {
			get {
				HeightTreeNode node = this;
				while (node.Right != null)
					node = node.Right;
				return node;
			}
		}

		/// <summary>
		/// Gets the inorder successor of the node.
		/// </summary>
		internal HeightTreeNode Successor {
			get {
				if (Right != null) {
					return Right.LeftMost;
				} else {
					HeightTreeNode node = this;
					HeightTreeNode oldNode;
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
		/// The number of lines in this node and its child nodes.
		/// Invariant:
		///   totalCount = 1 + left.totalCount + right.totalCount
		/// </summary>
		internal int TotalCount;

		/// <summary>
		/// The total height of this node and its child nodes, excluding directly collapsed nodes.
		/// Invariant:
		///   totalHeight = left.IsDirectlyCollapsed ? 0 : left.totalHeight
		///               + lineNode.IsDirectlyCollapsed ? 0 : lineNode.Height
		///               + right.IsDirectlyCollapsed ? 0 : right.totalHeight
		/// </summary>
		internal double TotalHeight;

		/// <summary>
		/// List of the sections that hold this node collapsed.
		/// Invariant 1:
		///   For each document line in the range described by a CollapsedSection, exactly one ancestor
		///   contains that CollapsedSection.
		/// Invariant 2:
		///   A CollapsedSection is contained either in left+middle or middle+right or just middle.
		/// Invariant 3:
		///   Start and end of a CollapsedSection always contain the collapsedSection in their
		///   documentLine (middle node).
		/// </summary>
		internal List<CollapsedLineSection> CollapsedSections;

		internal bool IsDirectlyCollapsed {
			get {
				return CollapsedSections != null;
			}
		}

		internal void AddDirectlyCollapsed(CollapsedLineSection section)
		{
			if (CollapsedSections == null) {
				CollapsedSections = new List<CollapsedLineSection>();
				TotalHeight = 0;
			}
			Debug.Assert(!CollapsedSections.Contains(section));
			CollapsedSections.Add(section);
		}


		internal void RemoveDirectlyCollapsed(CollapsedLineSection section)
		{
			Debug.Assert(CollapsedSections.Contains(section));
			CollapsedSections.Remove(section);
			if (CollapsedSections.Count == 0) {
				CollapsedSections = null;
				TotalHeight = LineNode.TotalHeight;
				if (Left != null)
					TotalHeight += Left.TotalHeight;
				if (Right != null)
					TotalHeight += Right.TotalHeight;
			}
		}

#if DEBUG
		public override string ToString()
		{
            return string.Create(CultureInfo.InvariantCulture,
                $"[{nameof(HeightTreeNode)} {DocumentLine.LineNumber} CS={GetCollapsedSections(CollapsedSections)} Line.CS={GetCollapsedSections(LineNode.CollapsedSections)} Line.Height={LineNode.Height} {nameof(TotalHeight)}={TotalHeight}]");
		}

		static string GetCollapsedSections(List<CollapsedLineSection> list)
		{
			if (list == null)
				return "{}";

            return string.Create(CultureInfo.InvariantCulture, $"{{{string.Join(",", list.ConvertAll(cs => cs.Id).ToArray())}}}");
		}
#endif
	}
}

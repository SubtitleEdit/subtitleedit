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
using System.Text;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Rendering
{
   /// <summary>
	/// Red-black tree similar to DocumentLineTree, augmented with collapsing and height data.
	/// </summary>
   internal sealed class HeightTree : ILineTracker, IDisposable
	{
		// TODO: Optimize this. This tree takes alot of memory.
		// (56 bytes for HeightTreeNode
		// We should try to get rid of the dictionary and find height nodes per index. (DONE!)
		// And we might do much better by compressing lines with the same height into a single node.
		// That would also improve load times because we would always start with just a single node.

		/* Idea:
		 class NewHeightTreeNode {
			int totalCount; // =count+left.count+right.count
			int count; // one node can represent multiple lines
			double height; // height of each line in this node
			double totalHeight; // =(collapsedSections!=null?0:height*count) + left.totalHeight + right.totalHeight
			List<CollapsedSection> collapsedSections; // sections holding this line collapsed
			// no "nodeCollapsedSections"/"totalCollapsedSections":
			NewHeightTreeNode left, right, parent;
			bool color;
		}
		totalCollapsedSections: are hard to update and not worth the effort. O(n log n) isn't too bad for
		 collapsing/uncollapsing, especially when compression reduces the n.
		 */

		#region Constructor

		private readonly TextDocument _document;
		private HeightTreeNode _root;
		private WeakLineTracker _weakLineTracker;

		public HeightTree(TextDocument document, double defaultLineHeight)
		{
			this._document = document;
			_weakLineTracker = WeakLineTracker.Register(document, this);
			this.DefaultLineHeight = defaultLineHeight;
			RebuildDocument();
		}

		public void Dispose()
		{
			if (_weakLineTracker != null)
				_weakLineTracker.Deregister();
			this._root = null;
			this._weakLineTracker = null;
		}

		public bool IsDisposed {
			get {
				return _root == null;
			}
		}

		private double _defaultLineHeight;

		public double DefaultLineHeight {
			get { return _defaultLineHeight; }
			set {
				var oldValue = _defaultLineHeight;
				if (oldValue == value)
					return;
				_defaultLineHeight = value;
				// update the stored value in all nodes:
				foreach (var node in AllNodes) {
					if (node.LineNode.Height == oldValue) {
						node.LineNode.Height = value;
						UpdateAugmentedData(node, UpdateAfterChildrenChangeRecursionMode.IfRequired);
					}
				}
			}
		}

		private HeightTreeNode GetNode(DocumentLine ls)
		{
			return GetNodeByIndex(ls.LineNumber - 1);
		}
		#endregion

		#region RebuildDocument
		void ILineTracker.ChangeComplete(DocumentChangeEventArgs e)
		{
		}

		void ILineTracker.SetLineLength(DocumentLine ls, int newTotalLength)
		{
		}

		/// <summary>
		/// Rebuild the tree, in O(n).
		/// </summary>
		public void RebuildDocument()
		{
			foreach (var s in GetAllCollapsedSections()) {
				s.Start = null;
				s.End = null;
			}

			var nodes = new HeightTreeNode[_document.LineCount];
			var lineNumber = 0;
			foreach (var ls in _document.Lines) {
				nodes[lineNumber++] = new HeightTreeNode(ls, _defaultLineHeight);
			}
			Debug.Assert(nodes.Length > 0);
			// now build the corresponding balanced tree
			var height = DocumentLineTree.GetTreeHeight(nodes.Length);
			Debug.WriteLine("HeightTree will have height: " + height);
			_root = BuildTree(nodes, 0, nodes.Length, height);
			_root.Color = Black;
#if DEBUG
			CheckProperties();
#endif
		}

		/// <summary>
		/// build a tree from a list of nodes
		/// </summary>
		private HeightTreeNode BuildTree(HeightTreeNode[] nodes, int start, int end, int subtreeHeight)
		{
			Debug.Assert(start <= end);
			if (start == end) {
				return null;
			}
			var middle = (start + end) / 2;
			var node = nodes[middle];
			node.Left = BuildTree(nodes, start, middle, subtreeHeight - 1);
			node.Right = BuildTree(nodes, middle + 1, end, subtreeHeight - 1);
			if (node.Left != null) node.Left.Parent = node;
			if (node.Right != null) node.Right.Parent = node;
			if (subtreeHeight == 1)
				node.Color = Red;
			UpdateAugmentedData(node, UpdateAfterChildrenChangeRecursionMode.None);
			return node;
		}
		#endregion

		#region Insert/Remove lines
		void ILineTracker.BeforeRemoveLine(DocumentLine line)
		{
			var node = GetNode(line);
			if (node.LineNode.CollapsedSections != null) {
				foreach (var cs in node.LineNode.CollapsedSections.ToArray()) {
					if (cs.Start == line && cs.End == line) {
						cs.Start = null;
						cs.End = null;
					} else if (cs.Start == line) {
						Uncollapse(cs);
						cs.Start = line.NextLine;
						AddCollapsedSection(cs, cs.End.LineNumber - cs.Start.LineNumber + 1);
					} else if (cs.End == line) {
						Uncollapse(cs);
						cs.End = line.PreviousLine;
						AddCollapsedSection(cs, cs.End.LineNumber - cs.Start.LineNumber + 1);
					}
				}
			}
			BeginRemoval();
			RemoveNode(node);
			// clear collapsedSections from removed line: prevent damage if removed line is in "nodesToCheckForMerging"
			node.LineNode.CollapsedSections = null;
			EndRemoval();
		}
		
//		void ILineTracker.AfterRemoveLine(DocumentLine line)
//		{
//
//		}
		
		void ILineTracker.LineInserted(DocumentLine insertionPos, DocumentLine newLine)
		{
			InsertAfter(GetNode(insertionPos), newLine);
#if DEBUG
			CheckProperties();
#endif
		}

		private HeightTreeNode InsertAfter(HeightTreeNode node, DocumentLine newLine)
		{
			var newNode = new HeightTreeNode(newLine, _defaultLineHeight);
			if (node.Right == null) {
				if (node.LineNode.CollapsedSections != null) {
					// we are inserting directly after node - so copy all collapsedSections
					// that do not end at node.
					foreach (var cs in node.LineNode.CollapsedSections) {
						if (cs.End != node.DocumentLine)
							newNode.AddDirectlyCollapsed(cs);
					}
				}
				InsertAsRight(node, newNode);
			} else {
				node = node.Right.LeftMost;
				if (node.LineNode.CollapsedSections != null) {
					// we are inserting directly before node - so copy all collapsedSections
					// that do not start at node.
					foreach (var cs in node.LineNode.CollapsedSections) {
						if (cs.Start != node.DocumentLine)
							newNode.AddDirectlyCollapsed(cs);
					}
				}
				InsertAsLeft(node, newNode);
			}
			return newNode;
		}
		#endregion

		#region Rotation callbacks

		private enum UpdateAfterChildrenChangeRecursionMode
		{
			None,
			IfRequired,
			WholeBranch
		}

		private static void UpdateAfterChildrenChange(HeightTreeNode node)
		{
			UpdateAugmentedData(node, UpdateAfterChildrenChangeRecursionMode.IfRequired);
		}

		private static void UpdateAugmentedData(HeightTreeNode node, UpdateAfterChildrenChangeRecursionMode mode)
		{
			var totalCount = 1;
			var totalHeight = node.LineNode.TotalHeight;
			if (node.Left != null) {
				totalCount += node.Left.TotalCount;
				totalHeight += node.Left.TotalHeight;
			}
			if (node.Right != null) {
				totalCount += node.Right.TotalCount;
				totalHeight += node.Right.TotalHeight;
			}
			if (node.IsDirectlyCollapsed)
				totalHeight = 0;
			if (totalCount != node.TotalCount
			    || !totalHeight.IsClose(node.TotalHeight)
			    || mode == UpdateAfterChildrenChangeRecursionMode.WholeBranch)
			{
				node.TotalCount = totalCount;
				node.TotalHeight = totalHeight;
				if (node.Parent != null && mode != UpdateAfterChildrenChangeRecursionMode.None)
					UpdateAugmentedData(node.Parent, mode);
			}
		}

		private void UpdateAfterRotateLeft(HeightTreeNode node)
		{
			// node = old parent
			// node.parent = pivot, new parent
			var collapsedP = node.Parent.CollapsedSections;
			var collapsedQ = node.CollapsedSections;
			// move collapsedSections from old parent to new parent
			node.Parent.CollapsedSections = collapsedQ;
			node.CollapsedSections = null;
			// split the collapsedSections from the new parent into its old children:
			if (collapsedP != null) {
				foreach (var cs in collapsedP) {
					if (node.Parent.Right != null)
						node.Parent.Right.AddDirectlyCollapsed(cs);
					node.Parent.LineNode.AddDirectlyCollapsed(cs);
					if (node.Right != null)
						node.Right.AddDirectlyCollapsed(cs);
				}
			}
			MergeCollapsedSectionsIfPossible(node);

			UpdateAfterChildrenChange(node);

			// not required: rotations only happen on insertions/deletions
			// -> totalCount changes -> the parent is always updated
			//UpdateAfterChildrenChange(node.parent);
		}

		private void UpdateAfterRotateRight(HeightTreeNode node)
		{
			// node = old parent
			// node.parent = pivot, new parent
			var collapsedP = node.Parent.CollapsedSections;
			var collapsedQ = node.CollapsedSections;
			// move collapsedSections from old parent to new parent
			node.Parent.CollapsedSections = collapsedQ;
			node.CollapsedSections = null;
			// split the collapsedSections from the new parent into its old children:
			if (collapsedP != null) {
				foreach (var cs in collapsedP) {
					if (node.Parent.Left != null)
						node.Parent.Left.AddDirectlyCollapsed(cs);
					node.Parent.LineNode.AddDirectlyCollapsed(cs);
					if (node.Left != null)
						node.Left.AddDirectlyCollapsed(cs);
				}
			}
			MergeCollapsedSectionsIfPossible(node);

			UpdateAfterChildrenChange(node);

			// not required: rotations only happen on insertions/deletions
			// -> totalCount changes -> the parent is always updated
			//UpdateAfterChildrenChange(node.parent);
		}

		// node removal:
		// a node in the middle of the tree is removed as following:
		//  its successor is removed
		//  it is replaced with its successor

		private void BeforeNodeRemove(HeightTreeNode removedNode)
		{
			Debug.Assert(removedNode.Left == null || removedNode.Right == null);

			var collapsed = removedNode.CollapsedSections;
			if (collapsed != null) {
				var childNode = removedNode.Left ?? removedNode.Right;
				if (childNode != null) {
					foreach (var cs in collapsed)
						childNode.AddDirectlyCollapsed(cs);
				}
			}
			if (removedNode.Parent != null)
				MergeCollapsedSectionsIfPossible(removedNode.Parent);
		}

		private void BeforeNodeReplace(HeightTreeNode removedNode, HeightTreeNode newNode, HeightTreeNode newNodeOldParent)
		{
			Debug.Assert(removedNode != null);
			Debug.Assert(newNode != null);
			while (newNodeOldParent != removedNode) {
				if (newNodeOldParent.CollapsedSections != null) {
					foreach (var cs in newNodeOldParent.CollapsedSections) {
						newNode.LineNode.AddDirectlyCollapsed(cs);
					}
				}
				newNodeOldParent = newNodeOldParent.Parent;
			}
			if (newNode.CollapsedSections != null) {
				foreach (var cs in newNode.CollapsedSections) {
					newNode.LineNode.AddDirectlyCollapsed(cs);
				}
			}
			newNode.CollapsedSections = removedNode.CollapsedSections;
			MergeCollapsedSectionsIfPossible(newNode);
		}

		private bool _inRemoval;
		private List<HeightTreeNode> _nodesToCheckForMerging;

		private void BeginRemoval()
		{
			Debug.Assert(!_inRemoval);
			if (_nodesToCheckForMerging == null) {
				_nodesToCheckForMerging = new List<HeightTreeNode>();
			}
			_inRemoval = true;
		}

		private void EndRemoval()
		{
			Debug.Assert(_inRemoval);
			_inRemoval = false;
			foreach (var node in _nodesToCheckForMerging) {
				MergeCollapsedSectionsIfPossible(node);
			}
			_nodesToCheckForMerging.Clear();
		}

		private void MergeCollapsedSectionsIfPossible(HeightTreeNode node)
		{
			Debug.Assert(node != null);
			if (_inRemoval) {
				_nodesToCheckForMerging.Add(node);
				return;
			}
			// now check if we need to merge collapsedSections together
			var merged = false;
			var collapsedL = node.LineNode.CollapsedSections;
			if (collapsedL != null) {
				for (var i = collapsedL.Count - 1; i >= 0; i--) {
					var cs = collapsedL[i];
					if (cs.Start == node.DocumentLine || cs.End == node.DocumentLine)
						continue;
					if (node.Left == null
					    || (node.Left.CollapsedSections != null && node.Left.CollapsedSections.Contains(cs)))
					{
						if (node.Right == null
						    || (node.Right.CollapsedSections != null && node.Right.CollapsedSections.Contains(cs)))
						{
							// all children of node contain cs: -> merge!
							if (node.Left != null) node.Left.RemoveDirectlyCollapsed(cs);
							if (node.Right != null) node.Right.RemoveDirectlyCollapsed(cs);
							collapsedL.RemoveAt(i);
							node.AddDirectlyCollapsed(cs);
							merged = true;
						}
					}
				}
				if (collapsedL.Count == 0)
					node.LineNode.CollapsedSections = null;
			}
			if (merged && node.Parent != null) {
				MergeCollapsedSectionsIfPossible(node.Parent);
			}
		}
		#endregion

		#region GetNodeBy... / Get...FromNode

		private HeightTreeNode GetNodeByIndex(int index)
		{
			Debug.Assert(index >= 0);
			Debug.Assert(index < _root.TotalCount);
			var node = _root;
			while (true) {
				if (node.Left != null && index < node.Left.TotalCount) {
					node = node.Left;
				} else {
					if (node.Left != null) {
						index -= node.Left.TotalCount;
					}
					if (index == 0)
						return node;
					index--;
					node = node.Right;
				}
			}
		}

		private HeightTreeNode GetNodeByVisualPosition(double position)
		{
			var node = _root;
			while (true) {
				var positionAfterLeft = position;
				if (node.Left != null) {
					positionAfterLeft -= node.Left.TotalHeight;
					if (positionAfterLeft < 0) {
						// Descend into left
						node = node.Left;
						continue;
					}
				}
				var positionBeforeRight = positionAfterLeft - node.LineNode.TotalHeight;
				if (positionBeforeRight < 0) {
					// Found the correct node
					return node;
				}
				if (node.Right == null || node.Right.TotalHeight == 0) {
					// Can happen when position>node.totalHeight,
					// i.e. at the end of the document, or due to rounding errors in previous loop iterations.

					// If node.lineNode isn't collapsed, return that.
					// Also return node.lineNode if there is no previous node that we could return instead.
					if (node.LineNode.TotalHeight > 0 || node.Left == null)
						return node;
					// Otherwise, descend into left (find the last non-collapsed node)
					node = node.Left;
				} else {
					// Descend into right
					position = positionBeforeRight;
					node = node.Right;
				}
			}
		}

		private static double GetVisualPositionFromNode(HeightTreeNode node)
		{
			var position = (node.Left != null) ? node.Left.TotalHeight : 0;
			while (node.Parent != null) {
				if (node.IsDirectlyCollapsed)
					position = 0;
				if (node == node.Parent.Right) {
					if (node.Parent.Left != null)
						position += node.Parent.Left.TotalHeight;
					position += node.Parent.LineNode.TotalHeight;
				}
				node = node.Parent;
			}
			return position;
		}
		#endregion

		#region Public methods
		public DocumentLine GetLineByNumber(int number)
		{
			return GetNodeByIndex(number - 1).DocumentLine;
		}

		public DocumentLine GetLineByVisualPosition(double position)
		{
			return GetNodeByVisualPosition(position).DocumentLine;
		}

		public double GetVisualPosition(DocumentLine line)
		{
			return GetVisualPositionFromNode(GetNode(line));
		}

		public double GetHeight(DocumentLine line)
		{
			return GetNode(line).LineNode.Height;
		}

		public void SetHeight(DocumentLine line, double val)
		{
			var node = GetNode(line);
			node.LineNode.Height = val;
			UpdateAfterChildrenChange(node);
		}

		public bool GetIsCollapsed(int lineNumber)
		{
			var node = GetNodeByIndex(lineNumber - 1);
			return node.LineNode.IsDirectlyCollapsed || GetIsCollapedFromNode(node);
		}

		/// <summary>
		/// Collapses the specified text section.
		/// Runtime: O(log n)
		/// </summary>
		public CollapsedLineSection CollapseText(DocumentLine start, DocumentLine end)
		{
			if (!_document.Lines.Contains(start))
				throw new ArgumentException("Line is not part of this document", nameof(start));
			if (!_document.Lines.Contains(end))
				throw new ArgumentException("Line is not part of this document", nameof(end));
			var length = end.LineNumber - start.LineNumber + 1;
			if (length < 0)
				throw new ArgumentException("start must be a line before end");
			var section = new CollapsedLineSection(this, start, end);
			AddCollapsedSection(section, length);
#if DEBUG
			CheckProperties();
#endif
			return section;
		}
		#endregion

		#region LineCount & TotalHeight
		public int LineCount {
			get {
				return _root.TotalCount;
			}
		}

		public double TotalHeight {
			get {
				return _root.TotalHeight;
			}
		}
		#endregion

		#region GetAllCollapsedSections

		private IEnumerable<HeightTreeNode> AllNodes {
			get {
				if (_root != null) {
					var node = _root.LeftMost;
					while (node != null) {
						yield return node;
						node = node.Successor;
					}
				}
			}
		}

		internal IEnumerable<CollapsedLineSection> GetAllCollapsedSections()
		{
			var emptyCsList = new List<CollapsedLineSection>();
			return System.Linq.Enumerable.Distinct(
				System.Linq.Enumerable.SelectMany(
					AllNodes, node => System.Linq.Enumerable.Concat(node.LineNode.CollapsedSections ?? emptyCsList,
																	node.CollapsedSections ?? emptyCsList)
				));
		}
		#endregion

		#region CheckProperties
#if DEBUG
		[Conditional("DATACONSISTENCYTEST")]
		internal void CheckProperties()
		{
			CheckProperties(_root);

			foreach (var cs in GetAllCollapsedSections()) {
				Debug.Assert(GetNode(cs.Start).LineNode.CollapsedSections.Contains(cs));
				Debug.Assert(GetNode(cs.End).LineNode.CollapsedSections.Contains(cs));
				var endLine = cs.End.LineNumber;
				for (var i = cs.Start.LineNumber; i <= endLine; i++) {
					CheckIsInSection(cs, GetLineByNumber(i));
				}
			}

			// check red-black property:
			var blackCount = -1;
			CheckNodeProperties(_root, null, Red, 0, ref blackCount);
		}

		private void CheckIsInSection(CollapsedLineSection cs, DocumentLine line)
		{
			var node = GetNode(line);
			if (node.LineNode.CollapsedSections != null && node.LineNode.CollapsedSections.Contains(cs))
				return;
			while (node != null) {
				if (node.CollapsedSections != null && node.CollapsedSections.Contains(cs))
					return;
				node = node.Parent;
			}
			throw new InvalidOperationException(cs + " not found for line " + line);
		}

		private void CheckProperties(HeightTreeNode node)
		{
			var totalCount = 1;
			var totalHeight = node.LineNode.TotalHeight;
			if (node.LineNode.IsDirectlyCollapsed)
				Debug.Assert(node.LineNode.CollapsedSections.Count > 0);
			if (node.Left != null) {
				CheckProperties(node.Left);
				totalCount += node.Left.TotalCount;
				totalHeight += node.Left.TotalHeight;

				CheckAllContainedIn(node.Left.CollapsedSections, node.LineNode.CollapsedSections);
			}
			if (node.Right != null) {
				CheckProperties(node.Right);
				totalCount += node.Right.TotalCount;
				totalHeight += node.Right.TotalHeight;

				CheckAllContainedIn(node.Right.CollapsedSections, node.LineNode.CollapsedSections);
			}
			if (node.Left != null && node.Right != null) {
				if (node.Left.CollapsedSections != null && node.Right.CollapsedSections != null) {
					var intersection = System.Linq.Enumerable.Intersect(node.Left.CollapsedSections, node.Right.CollapsedSections);
					Debug.Assert(System.Linq.Enumerable.Count(intersection) == 0);
				}
			}
			if (node.IsDirectlyCollapsed) {
				Debug.Assert(node.CollapsedSections.Count > 0);
				totalHeight = 0;
			}
			Debug.Assert(node.TotalCount == totalCount);
			Debug.Assert(node.TotalHeight.IsClose(totalHeight));
		}

		/// <summary>
		/// Checks that all elements in list1 are contained in list2.
		/// </summary>
		private static void CheckAllContainedIn(IEnumerable<CollapsedLineSection> list1, ICollection<CollapsedLineSection> list2)
		{
			if (list1 == null) list1 = new List<CollapsedLineSection>();
			if (list2 == null) list2 = new List<CollapsedLineSection>();
			foreach (var cs in list1) {
				Debug.Assert(list2.Contains(cs));
			}
		}

		/*
		1. A node is either red or black.
		2. The root is black.
		3. All leaves are black. (The leaves are the NIL children.)
		4. Both children of every red node are black. (So every red node must have a black parent.)
		5. Every simple path from a node to a descendant leaf contains the same number of black nodes. (Not counting the leaf node.)
		 */
		private void CheckNodeProperties(HeightTreeNode node, HeightTreeNode parentNode, bool parentColor, int blackCount, ref int expectedBlackCount)
		{
			if (node == null) return;

			Debug.Assert(node.Parent == parentNode);

			if (parentColor == Red) {
				Debug.Assert(node.Color == Black);
			}
			if (node.Color == Black) {
				blackCount++;
			}
			if (node.Left == null && node.Right == null) {
				// node is a leaf node:
				if (expectedBlackCount == -1)
					expectedBlackCount = blackCount;
				else
					Debug.Assert(expectedBlackCount == blackCount);
			}
			CheckNodeProperties(node.Left, node, node.Color, blackCount, ref expectedBlackCount);
			CheckNodeProperties(node.Right, node, node.Color, blackCount, ref expectedBlackCount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public string GetTreeAsString()
		{
			var b = new StringBuilder();
			AppendTreeToString(_root, b, 0);
			return b.ToString();
		}

		private static void AppendTreeToString(HeightTreeNode node, StringBuilder b, int indent)
		{
			if (node.Color == Red)
				b.Append("RED   ");
			else
				b.Append("BLACK ");
			b.AppendLine(node.ToString());
			indent += 2;
			if (node.Left != null) {
				b.Append(' ', indent);
				b.Append("L: ");
				AppendTreeToString(node.Left, b, indent);
			}
			if (node.Right != null) {
				b.Append(' ', indent);
				b.Append("R: ");
				AppendTreeToString(node.Right, b, indent);
			}
		}
#endif
		#endregion

		#region Red/Black Tree

		private const bool Red = true;
		private const bool Black = false;

		private void InsertAsLeft(HeightTreeNode parentNode, HeightTreeNode newNode)
		{
			Debug.Assert(parentNode.Left == null);
			parentNode.Left = newNode;
			newNode.Parent = parentNode;
			newNode.Color = Red;
			UpdateAfterChildrenChange(parentNode);
			FixTreeOnInsert(newNode);
		}

		private void InsertAsRight(HeightTreeNode parentNode, HeightTreeNode newNode)
		{
			Debug.Assert(parentNode.Right == null);
			parentNode.Right = newNode;
			newNode.Parent = parentNode;
			newNode.Color = Red;
			UpdateAfterChildrenChange(parentNode);
			FixTreeOnInsert(newNode);
		}

		private void FixTreeOnInsert(HeightTreeNode node)
		{
			Debug.Assert(node != null);
			Debug.Assert(node.Color == Red);
			Debug.Assert(node.Left == null || node.Left.Color == Black);
			Debug.Assert(node.Right == null || node.Right.Color == Black);

			var parentNode = node.Parent;
			if (parentNode == null) {
				// we inserted in the root -> the node must be black
				// since this is a root node, making the node black increments the number of black nodes
				// on all paths by one, so it is still the same for all paths.
				node.Color = Black;
				return;
			}
			if (parentNode.Color == Black) {
				// if the parent node where we inserted was black, our red node is placed correctly.
				// since we inserted a red node, the number of black nodes on each path is unchanged
				// -> the tree is still balanced
				return;
			}
			// parentNode is red, so there is a conflict here!

			// because the root is black, parentNode is not the root -> there is a grandparent node
			var grandparentNode = parentNode.Parent;
			var uncleNode = Sibling(parentNode);
			if (uncleNode != null && uncleNode.Color == Red) {
				parentNode.Color = Black;
				uncleNode.Color = Black;
				grandparentNode.Color = Red;
				FixTreeOnInsert(grandparentNode);
				return;
			}
			// now we know: parent is red but uncle is black
			// First rotation:
			if (node == parentNode.Right && parentNode == grandparentNode.Left) {
				RotateLeft(parentNode);
				node = node.Left;
			} else if (node == parentNode.Left && parentNode == grandparentNode.Right) {
				RotateRight(parentNode);
				node = node.Right;
			}
			// because node might have changed, reassign variables:
			parentNode = node.Parent;
			grandparentNode = parentNode.Parent;

			// Now recolor a bit:
			parentNode.Color = Black;
			grandparentNode.Color = Red;
			// Second rotation:
			if (node == parentNode.Left && parentNode == grandparentNode.Left) {
				RotateRight(grandparentNode);
			} else {
				// because of the first rotation, this is guaranteed:
				Debug.Assert(node == parentNode.Right && parentNode == grandparentNode.Right);
				RotateLeft(grandparentNode);
			}
		}

		private void RemoveNode(HeightTreeNode removedNode)
		{
			if (removedNode.Left != null && removedNode.Right != null) {
				// replace removedNode with it's in-order successor

				var leftMost = removedNode.Right.LeftMost;
				var parentOfLeftMost = leftMost.Parent;
				RemoveNode(leftMost); // remove leftMost from its current location

				BeforeNodeReplace(removedNode, leftMost, parentOfLeftMost);
				// and overwrite the removedNode with it
				ReplaceNode(removedNode, leftMost);
				leftMost.Left = removedNode.Left;
				if (leftMost.Left != null) leftMost.Left.Parent = leftMost;
				leftMost.Right = removedNode.Right;
				if (leftMost.Right != null) leftMost.Right.Parent = leftMost;
				leftMost.Color = removedNode.Color;

				UpdateAfterChildrenChange(leftMost);
				if (leftMost.Parent != null) UpdateAfterChildrenChange(leftMost.Parent);
				return;
			}

			// now either removedNode.left or removedNode.right is null
			// get the remaining child
			var parentNode = removedNode.Parent;
			var childNode = removedNode.Left ?? removedNode.Right;
			BeforeNodeRemove(removedNode);
			ReplaceNode(removedNode, childNode);
			if (parentNode != null) UpdateAfterChildrenChange(parentNode);
			if (removedNode.Color == Black) {
				if (childNode != null && childNode.Color == Red) {
					childNode.Color = Black;
				} else {
					FixTreeOnDelete(childNode, parentNode);
				}
			}
		}

		private void FixTreeOnDelete(HeightTreeNode node, HeightTreeNode parentNode)
		{
			Debug.Assert(node == null || node.Parent == parentNode);
			if (parentNode == null)
				return;

			// warning: node may be null
			var sibling = Sibling(node, parentNode);
			if (sibling.Color == Red) {
				parentNode.Color = Red;
				sibling.Color = Black;
				if (node == parentNode.Left) {
					RotateLeft(parentNode);
				} else {
					RotateRight(parentNode);
				}

				sibling = Sibling(node, parentNode); // update value of sibling after rotation
			}

			if (parentNode.Color == Black
			    && sibling.Color == Black
			    && GetColor(sibling.Left) == Black
			    && GetColor(sibling.Right) == Black)
			{
				sibling.Color = Red;
				FixTreeOnDelete(parentNode, parentNode.Parent);
				return;
			}

			if (parentNode.Color == Red
			    && sibling.Color == Black
			    && GetColor(sibling.Left) == Black
			    && GetColor(sibling.Right) == Black)
			{
				sibling.Color = Red;
				parentNode.Color = Black;
				return;
			}

			if (node == parentNode.Left &&
			    sibling.Color == Black &&
			    GetColor(sibling.Left) == Red &&
			    GetColor(sibling.Right) == Black)
			{
				sibling.Color = Red;
				sibling.Left.Color = Black;
				RotateRight(sibling);
			}
			else if (node == parentNode.Right &&
			         sibling.Color == Black &&
			         GetColor(sibling.Right) == Red &&
			         GetColor(sibling.Left) == Black)
			{
				sibling.Color = Red;
				sibling.Right.Color = Black;
				RotateLeft(sibling);
			}
			sibling = Sibling(node, parentNode); // update value of sibling after rotation

			sibling.Color = parentNode.Color;
			parentNode.Color = Black;
			if (node == parentNode.Left) {
				if (sibling.Right != null) {
					Debug.Assert(sibling.Right.Color == Red);
					sibling.Right.Color = Black;
				}
				RotateLeft(parentNode);
			} else {
				if (sibling.Left != null) {
					Debug.Assert(sibling.Left.Color == Red);
					sibling.Left.Color = Black;
				}
				RotateRight(parentNode);
			}
		}

		private void ReplaceNode(HeightTreeNode replacedNode, HeightTreeNode newNode)
		{
			if (replacedNode.Parent == null) {
				Debug.Assert(replacedNode == _root);
				_root = newNode;
			} else {
				if (replacedNode.Parent.Left == replacedNode)
					replacedNode.Parent.Left = newNode;
				else
					replacedNode.Parent.Right = newNode;
			}
			if (newNode != null) {
				newNode.Parent = replacedNode.Parent;
			}
			replacedNode.Parent = null;
		}

		private void RotateLeft(HeightTreeNode p)
		{
			// let q be p's right child
			var q = p.Right;
			Debug.Assert(q != null);
			Debug.Assert(q.Parent == p);
			// set q to be the new root
			ReplaceNode(p, q);

			// set p's right child to be q's left child
			p.Right = q.Left;
			if (p.Right != null) p.Right.Parent = p;
			// set q's left child to be p
			q.Left = p;
			p.Parent = q;
			UpdateAfterRotateLeft(p);
		}

		private void RotateRight(HeightTreeNode p)
		{
			// let q be p's left child
			var q = p.Left;
			Debug.Assert(q != null);
			Debug.Assert(q.Parent == p);
			// set q to be the new root
			ReplaceNode(p, q);

			// set p's left child to be q's right child
			p.Left = q.Right;
			if (p.Left != null) p.Left.Parent = p;
			// set q's right child to be p
			q.Right = p;
			p.Parent = q;
			UpdateAfterRotateRight(p);
		}

		private static HeightTreeNode Sibling(HeightTreeNode node)
		{
			if (node == node.Parent.Left)
				return node.Parent.Right;
			else
				return node.Parent.Left;
		}

		private static HeightTreeNode Sibling(HeightTreeNode node, HeightTreeNode parentNode)
		{
			Debug.Assert(node == null || node.Parent == parentNode);
			if (node == parentNode.Left)
				return parentNode.Right;
			else
				return parentNode.Left;
		}

		private static bool GetColor(HeightTreeNode node)
		{
			return node != null ? node.Color : Black;
		}
		#endregion

		#region Collapsing support

		private static bool GetIsCollapedFromNode(HeightTreeNode node)
		{
			while (node != null) {
				if (node.IsDirectlyCollapsed)
					return true;
				node = node.Parent;
			}
			return false;
		}

		internal void AddCollapsedSection(CollapsedLineSection section, int sectionLength)
		{
			AddRemoveCollapsedSection(section, sectionLength, true);
		}

		private void AddRemoveCollapsedSection(CollapsedLineSection section, int sectionLength, bool add)
		{
			Debug.Assert(sectionLength > 0);

			var node = GetNode(section.Start);
			// Go up in the tree.
			while (true) {
				// Mark all middle nodes as collapsed
				if (add)
					node.LineNode.AddDirectlyCollapsed(section);
				else
					node.LineNode.RemoveDirectlyCollapsed(section);
				sectionLength -= 1;
				if (sectionLength == 0) {
					// we are done!
					Debug.Assert(node.DocumentLine == section.End);
					break;
				}
				// Mark all right subtrees as collapsed.
				if (node.Right != null) {
					if (node.Right.TotalCount < sectionLength) {
						if (add)
							node.Right.AddDirectlyCollapsed(section);
						else
							node.Right.RemoveDirectlyCollapsed(section);
						sectionLength -= node.Right.TotalCount;
					} else {
						// mark partially into the right subtree: go down the right subtree.
						AddRemoveCollapsedSectionDown(section, node.Right, sectionLength, add);
						break;
					}
				}
				// go up to the next node
				var parentNode = node.Parent;
				Debug.Assert(parentNode != null);
				while (parentNode.Right == node) {
					node = parentNode;
					parentNode = node.Parent;
					Debug.Assert(parentNode != null);
				}
				node = parentNode;
			}
			UpdateAugmentedData(GetNode(section.Start), UpdateAfterChildrenChangeRecursionMode.WholeBranch);
			UpdateAugmentedData(GetNode(section.End), UpdateAfterChildrenChangeRecursionMode.WholeBranch);
		}

		private static void AddRemoveCollapsedSectionDown(CollapsedLineSection section, HeightTreeNode node, int sectionLength, bool add)
		{
			while (true) {
				if (node.Left != null) {
					if (node.Left.TotalCount < sectionLength) {
						// mark left subtree
						if (add)
							node.Left.AddDirectlyCollapsed(section);
						else
							node.Left.RemoveDirectlyCollapsed(section);
						sectionLength -= node.Left.TotalCount;
					} else {
						// mark only inside the left subtree
						node = node.Left;
						Debug.Assert(node != null);
						continue;
					}
				}
				if (add)
					node.LineNode.AddDirectlyCollapsed(section);
				else
					node.LineNode.RemoveDirectlyCollapsed(section);
				sectionLength -= 1;
				if (sectionLength == 0) {
					// done!
					Debug.Assert(node.DocumentLine == section.End);
					break;
				}
				// mark inside right subtree:
				node = node.Right;
				Debug.Assert(node != null);
			}
		}

		public void Uncollapse(CollapsedLineSection section)
		{
			var sectionLength = section.End.LineNumber - section.Start.LineNumber + 1;
			AddRemoveCollapsedSection(section, sectionLength, false);
			// do not call CheckProperties() in here - Uncollapse is also called during line removals
		}
		#endregion
	}
}

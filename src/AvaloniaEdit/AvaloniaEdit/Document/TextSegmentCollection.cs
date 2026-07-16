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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AvaloniaEdit.Utils;
using Avalonia.Threading;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// Interface to allow TextSegments to access the TextSegmentCollection - we cannot use a direct reference
    /// because TextSegmentCollection is generic.
    /// </summary>
    internal interface ISegmentTree
    {
        void Add(TextSegment s);
        void Remove(TextSegment s);
        void UpdateAugmentedData(TextSegment s);
    }

    /// <summary>
    /// <para>
    /// A collection of text segments that supports efficient lookup of segments
    /// intersecting with another segment.
    /// </para>
    /// </summary>
    /// <remarks><inheritdoc cref="TextSegment"/></remarks>
    /// <see cref="TextSegment"/>
    public sealed class TextSegmentCollection<T> : ICollection<T>, ISegmentTree where T : TextSegment
    {
        // Implementation: this is basically a mixture of an augmented interval tree
        // and the TextAnchorTree.

        // WARNING: you need to understand interval trees (the version with the augmented 'high'/'max' field)
        // and how the TextAnchorTree works before you have any chance of understanding this code.

        // This means that every node holds two "segments":
        // one like the segments in the text anchor tree to support efficient offset changes
        // and another that is the interval as seen by the user

        // So basically, the tree contains a list of contiguous node segments of the first kind,
        // with interval segments starting at the end of every node segment.

        // Performance:
        // Add is O(lg n)
        // Remove is O(lg n)
        // DocumentChanged is O(m * lg n), with m the number of segments that intersect with the changed document section
        // FindFirstSegmentWithStartAfter is O(m + lg n) with m being the number of segments at the same offset as the result segment
        // FindIntersectingSegments is O(m + lg n) with m being the number of intersecting segments.

        private TextSegment _root;
        private readonly bool _isConnectedToDocument;

        #region Constructor
        /// <summary>
        /// Creates a new TextSegmentCollection that needs manual calls to <see cref="UpdateOffsets(DocumentChangeEventArgs)"/>.
        /// </summary>
        public TextSegmentCollection()
        {
        }

        /// <summary>
        /// Creates a new TextSegmentCollection that updates the offsets automatically.
        /// </summary>
        /// <param name="textDocument">The document to which the text segments
        /// that will be added to the tree belong. When the document changes, the
        /// position of the text segments will be updated accordingly.</param>
        public TextSegmentCollection(TextDocument textDocument)
        {
            if (textDocument == null)
                throw new ArgumentNullException(nameof(textDocument));

            Dispatcher.UIThread.VerifyAccess();
            _isConnectedToDocument = true;
            TextDocumentWeakEventManager.Changed.AddHandler(textDocument, OnDocumentChanged);
        }
        #endregion

        public void Disconnect (TextDocument textDocument)
        {
            if (_isConnectedToDocument)
            {
                TextDocumentWeakEventManager.Changed.RemoveHandler(textDocument, OnDocumentChanged);
            }
        }

        #region OnDocumentChanged / UpdateOffsets
        /// <summary>
        /// Updates the start and end offsets of all segments stored in this collection.
        /// </summary>
        /// <param name="e">DocumentChangeEventArgs instance describing the change to the document.</param>
        public void UpdateOffsets(DocumentChangeEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (_isConnectedToDocument)
                throw new InvalidOperationException("This TextSegmentCollection will automatically update offsets; do not call UpdateOffsets manually!");
            OnDocumentChanged(this, e);
            CheckProperties();
        }

        private void OnDocumentChanged(object sender, DocumentChangeEventArgs e)
        {
            var map = e.OffsetChangeMapOrNull;
            if (map != null)
            {
                foreach (var entry in map)
                {
                    UpdateOffsetsInternal(entry);
                }
            }
            else
            {
                UpdateOffsetsInternal(e.CreateSingleChangeMapEntry());
            }
        }

        /// <summary>
        /// Updates the start and end offsets of all segments stored in this collection.
        /// </summary>
        /// <param name="change">OffsetChangeMapEntry instance describing the change to the document.</param>
        public void UpdateOffsets(OffsetChangeMapEntry change)
        {
            if (_isConnectedToDocument)
                throw new InvalidOperationException("This TextSegmentCollection will automatically update offsets; do not call UpdateOffsets manually!");
            UpdateOffsetsInternal(change);
            CheckProperties();
        }
        #endregion

        #region UpdateOffsets (implementation)

        private void UpdateOffsetsInternal(OffsetChangeMapEntry change)
        {
            // Special case pure insertions, because they don't always cause a text segment to increase in size when the replaced region
            // is inside a segment (when offset is at start or end of a text semgent).
            if (change.RemovalLength == 0)
            {
                InsertText(change.Offset, change.InsertionLength);
            }
            else
            {
                ReplaceText(change);
            }
        }

        private void InsertText(int offset, int length)
        {
            if (length == 0)
                return;

            // enlarge segments that contain offset (excluding those that have offset as endpoint)
            foreach (var segment in FindSegmentsContaining(offset))
            {
                if (segment.StartOffset < offset && offset < segment.EndOffset)
                {
                    segment.Length += length;
                }
            }

            // move start offsets of all segments >= offset
            TextSegment node = FindFirstSegmentWithStartAfter(offset);
            if (node != null)
            {
                node.NodeLength += length;
                UpdateAugmentedData(node);
            }
        }

        private void ReplaceText(OffsetChangeMapEntry change)
        {
            Debug.Assert(change.RemovalLength > 0);
            var offset = change.Offset;
            foreach (var segment in FindOverlappingSegments(offset, change.RemovalLength))
            {
                if (segment.StartOffset <= offset)
                {
                    if (segment.EndOffset >= offset + change.RemovalLength)
                    {
                        // Replacement inside segment: adjust segment length
                        segment.Length += change.InsertionLength - change.RemovalLength;
                    }
                    else
                    {
                        // Replacement starting inside segment and ending after segment end: set segment end to removal position
                        //segment.EndOffset = offset;
                        segment.Length = offset - segment.StartOffset;
                    }
                }
                else
                {
                    // Replacement starting in front of text segment and running into segment.
                    // Keep segment.EndOffset constant and move segment.StartOffset to the end of the replacement
                    var remainingLength = segment.EndOffset - (offset + change.RemovalLength);
                    RemoveSegment(segment);
                    segment.StartOffset = offset + change.RemovalLength;
                    segment.Length = Math.Max(0, remainingLength);
                    AddSegment(segment);
                }
            }
            // move start offsets of all segments > offset
            TextSegment node = FindFirstSegmentWithStartAfter(offset + 1);
            if (node != null)
            {
                Debug.Assert(node.NodeLength >= change.RemovalLength);
                node.NodeLength += change.InsertionLength - change.RemovalLength;
                UpdateAugmentedData(node);
            }
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds the specified segment to the tree. This will cause the segment to update when the
        /// document changes.
        /// </summary>
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.OwnerTree != null)
                throw new ArgumentException("The segment is already added to a SegmentCollection.");
            AddSegment(item);
        }

        void ISegmentTree.Add(TextSegment s)
        {
            AddSegment(s);
        }

        private void AddSegment(TextSegment node)
        {
            var insertionOffset = node.StartOffset;
            node.DistanceToMaxEnd = node.SegmentLength;
            if (_root == null)
            {
                _root = node;
                node.TotalNodeLength = node.NodeLength;
            }
            else if (insertionOffset >= _root.TotalNodeLength)
            {
                // append segment at end of tree
                node.NodeLength = node.TotalNodeLength = insertionOffset - _root.TotalNodeLength;
                InsertAsRight(_root.RightMost, node);
            }
            else
            {
                // insert in middle of tree
                var n = FindNode(ref insertionOffset);
                Debug.Assert(insertionOffset < n.NodeLength);
                // split node segment 'n' at offset
                node.TotalNodeLength = node.NodeLength = insertionOffset;
                n.NodeLength -= insertionOffset;
                InsertBefore(n, node);
            }
            node.OwnerTree = this;
            Count++;
            CheckProperties();
        }

        private void InsertBefore(TextSegment node, TextSegment newNode)
        {
            if (node.Left == null)
            {
                InsertAsLeft(node, newNode);
            }
            else
            {
                InsertAsRight(node.Left.RightMost, newNode);
            }
        }
        #endregion

        #region GetNextSegment / GetPreviousSegment
        /// <summary>
        /// Gets the next segment after the specified segment.
        /// Segments are sorted by their start offset.
        /// Returns null if segment is the last segment.
        /// </summary>
        public T GetNextSegment(T segment)
        {
            if (!Contains(segment))
                throw new ArgumentException("segment is not inside the segment tree");
            return (T)segment.Successor;
        }

        /// <summary>
        /// Gets the previous segment before the specified segment.
        /// Segments are sorted by their start offset.
        /// Returns null if segment is the first segment.
        /// </summary>
        public T GetPreviousSegment(T segment)
        {
            if (!Contains(segment))
                throw new ArgumentException("segment is not inside the segment tree");
            return (T)segment.Predecessor;
        }
        #endregion

        #region FirstSegment/LastSegment
        /// <summary>
        /// Returns the first segment in the collection or null, if the collection is empty.
        /// </summary>
        public T FirstSegment => (T)_root?.LeftMost;

        /// <summary>
        /// Returns the last segment in the collection or null, if the collection is empty.
        /// </summary>
        public T LastSegment => (T)_root?.RightMost;

        #endregion

        #region FindFirstSegmentWithStartAfter
        /// <summary>
        /// Gets the first segment with a start offset greater or equal to <paramref name="startOffset"/>.
        /// Returns null if no such segment is found.
        /// </summary>
        public T FindFirstSegmentWithStartAfter(int startOffset)
        {
            if (_root == null)
                return null;
            if (startOffset <= 0)
                return (T)_root.LeftMost;
            var s = FindNode(ref startOffset);
            // startOffset means that the previous segment is starting at the offset we were looking for
            while (startOffset == 0)
            {
                var p = (s == null) ? _root.RightMost : s.Predecessor;
                // There must always be a predecessor: if we were looking for the first node, we would have already
                // returned it as root.LeftMost above.
                Debug.Assert(p != null);
                startOffset += p.NodeLength;
                s = p;
            }
            return (T)s;
        }

        /// <summary>
        /// Finds the node at the specified offset.
        /// After the method has run, offset is relative to the beginning of the returned node.
        /// </summary>
        private TextSegment FindNode(ref int offset)
        {
            var n = _root;
            while (true)
            {
                if (n.Left != null)
                {
                    if (offset < n.Left.TotalNodeLength)
                    {
                        n = n.Left; // descend into left subtree
                        continue;
                    }
                    else
                    {
                        offset -= n.Left.TotalNodeLength; // skip left subtree
                    }
                }
                if (offset < n.NodeLength)
                {
                    return n; // found correct node
                }
                else
                {
                    offset -= n.NodeLength; // skip this node
                }
                if (n.Right != null)
                {
                    n = n.Right; // descend into right subtree
                }
                else
                {
                    // didn't find any node containing the offset
                    return null;
                }
            }
        }
        #endregion

        #region FindOverlappingSegments
        /// <summary>
        /// Finds all segments that contain the given offset.
        /// (StartOffset &lt;= offset &lt;= EndOffset)
        /// Segments are returned in the order given by GetNextSegment/GetPreviousSegment.
        /// </summary>
        /// <returns>Returns a new collection containing the results of the query.
        /// This means it is safe to modify the TextSegmentCollection while iterating through the result collection.</returns>
        public ReadOnlyCollection<T> FindSegmentsContaining(int offset)
        {
            return FindOverlappingSegments(offset, 0);
        }

        /// <summary>
        /// Finds all segments that overlap with the given segment (including touching segments).
        /// </summary>
        /// <returns>Returns a new collection containing the results of the query.
        /// This means it is safe to modify the TextSegmentCollection while iterating through the result collection.</returns>
        public ReadOnlyCollection<T> FindOverlappingSegments(ISegment segment)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));
            return FindOverlappingSegments(segment.Offset, segment.Length);
        }

        /// <summary>
        /// Finds all segments that overlap with the given segment (including touching segments).
        /// Segments are returned in the order given by GetNextSegment/GetPreviousSegment.
        /// </summary>
        /// <returns>Returns a new collection containing the results of the query.
        /// This means it is safe to modify the TextSegmentCollection while iterating through the result collection.</returns>
        public ReadOnlyCollection<T> FindOverlappingSegments(int offset, int length)
        {
            ThrowUtil.CheckNotNegative(length, "length");
            var results = new List<T>();
            if (_root != null)
            {
                FindOverlappingSegments(results, _root, offset, offset + length);
            }
            return new ReadOnlyCollection<T>(results);
        }

        private void FindOverlappingSegments(List<T> results, TextSegment node, int low, int high)
        {
            // low and high are relative to node.LeftMost startpos (not node.LeftMost.Offset)
            if (high < 0)
            {
                // node is irrelevant for search because all intervals in node are after high
                return;
            }

            // find values relative to node.Offset
            var nodeLow = low - node.NodeLength;
            var nodeHigh = high - node.NodeLength;
            if (node.Left != null)
            {
                nodeLow -= node.Left.TotalNodeLength;
                nodeHigh -= node.Left.TotalNodeLength;
            }

            if (node.DistanceToMaxEnd < nodeLow)
            {
                // node is irrelevant for search because all intervals in node are before low
                return;
            }

            if (node.Left != null)
                FindOverlappingSegments(results, node.Left, low, high);

            if (nodeHigh < 0)
            {
                // node and everything in node.right is before low
                return;
            }

            if (nodeLow <= node.SegmentLength)
            {
                results.Add((T)node);
            }

            if (node.Right != null)
                FindOverlappingSegments(results, node.Right, nodeLow, nodeHigh);
        }
        #endregion

        #region UpdateAugmentedData

        private void UpdateAugmentedData(TextSegment node)
        {
            var totalLength = node.NodeLength;
            var distanceToMaxEnd = node.SegmentLength;
            if (node.Left != null)
            {
                totalLength += node.Left.TotalNodeLength;

                var leftDtme = node.Left.DistanceToMaxEnd;
                // dtme is relative, so convert it to the coordinates of node:
                if (node.Left.Right != null)
                    leftDtme -= node.Left.Right.TotalNodeLength;
                leftDtme -= node.NodeLength;
                if (leftDtme > distanceToMaxEnd)
                    distanceToMaxEnd = leftDtme;
            }
            if (node.Right != null)
            {
                totalLength += node.Right.TotalNodeLength;

                var rightDtme = node.Right.DistanceToMaxEnd;
                // dtme is relative, so convert it to the coordinates of node:
                rightDtme += node.Right.NodeLength;
                if (node.Right.Left != null)
                    rightDtme += node.Right.Left.TotalNodeLength;
                if (rightDtme > distanceToMaxEnd)
                    distanceToMaxEnd = rightDtme;
            }
            if (node.TotalNodeLength != totalLength
                || node.DistanceToMaxEnd != distanceToMaxEnd)
            {
                node.TotalNodeLength = totalLength;
                node.DistanceToMaxEnd = distanceToMaxEnd;
                if (node.Parent != null)
                    UpdateAugmentedData(node.Parent);
            }
        }

        void ISegmentTree.UpdateAugmentedData(TextSegment node)
        {
            UpdateAugmentedData(node);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the specified segment from the tree. This will cause the segment to not update
        /// anymore when the document changes.
        /// </summary>
        public bool Remove(T item)
        {
            if (!Contains(item))
                return false;
            RemoveSegment(item);
            return true;
        }

        void ISegmentTree.Remove(TextSegment s)
        {
            RemoveSegment(s);
        }

        private void RemoveSegment(TextSegment s)
        {
            var oldOffset = s.StartOffset;
            var successor = s.Successor;
            if (successor != null)
                successor.NodeLength += s.NodeLength;
            RemoveNode(s);
            if (successor != null)
                UpdateAugmentedData(successor);
            Disconnect(s, oldOffset);
            CheckProperties();
        }

        private void Disconnect(TextSegment s, int offset)
        {
            s.Left = s.Right = s.Parent = null;
            s.OwnerTree = null;
            s.NodeLength = offset;
            Count--;
        }

        /// <summary>
        /// Removes all segments from the tree.
        /// </summary>
        public void Clear()
        {
            var segments = this.ToArray();
            _root = null;
            var offset = 0;
            foreach (var s in segments)
            {
                offset += s.NodeLength;
                Disconnect(s, offset);
            }
            CheckProperties();
        }
        #endregion

        #region CheckProperties
        [Conditional("DATACONSISTENCYTEST")]
        internal void CheckProperties()
        {
#if DEBUG
            if (_root != null)
            {
                CheckProperties(_root);

                // check red-black property:
                var blackCount = -1;
                CheckNodeProperties(_root, null, Red, 0, ref blackCount);
            }

            var expectedCount = 0;
            // we cannot trust LINQ not to call ICollection.Count, so we need this loop
            // to count the elements in the tree
            using (var en = GetEnumerator())
            {
                while (en.MoveNext()) expectedCount++;
            }
            Debug.Assert(Count == expectedCount);
#endif
        }

#if DEBUG

        private void CheckProperties(TextSegment node)
        {
            var totalLength = node.NodeLength;
            var distanceToMaxEnd = node.SegmentLength;
            if (node.Left != null)
            {
                CheckProperties(node.Left);
                totalLength += node.Left.TotalNodeLength;
                distanceToMaxEnd = Math.Max(distanceToMaxEnd,
                                            node.Left.DistanceToMaxEnd + node.Left.StartOffset - node.StartOffset);
            }
            if (node.Right != null)
            {
                CheckProperties(node.Right);
                totalLength += node.Right.TotalNodeLength;
                distanceToMaxEnd = Math.Max(distanceToMaxEnd,
                                            node.Right.DistanceToMaxEnd + node.Right.StartOffset - node.StartOffset);
            }
            Debug.Assert(node.TotalNodeLength == totalLength);
            Debug.Assert(node.DistanceToMaxEnd == distanceToMaxEnd);
        }

        /*
		1. A node is either red or black.
		2. The root is black.
		3. All leaves are black. (The leaves are the NIL children.)
		4. Both children of every red node are black. (So every red node must have a black parent.)
		5. Every simple path from a node to a descendant leaf contains the same number of black nodes. (Not counting the leaf node.)
		 */
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void CheckNodeProperties(TextSegment node, TextSegment parentNode, bool parentColor, int blackCount, ref int expectedBlackCount)
        {
            if (node == null) return;

            Debug.Assert(node.Parent == parentNode);

            if (parentColor == Red)
            {
                Debug.Assert(node.Color == Black);
            }
            if (node.Color == Black)
            {
                blackCount++;
            }
            if (node.Left == null && node.Right == null)
            {
                // node is a leaf node:
                if (expectedBlackCount == -1)
                    expectedBlackCount = blackCount;
                else
                    Debug.Assert(expectedBlackCount == blackCount);
            }
            CheckNodeProperties(node.Left, node, node.Color, blackCount, ref expectedBlackCount);
            CheckNodeProperties(node.Right, node, node.Color, blackCount, ref expectedBlackCount);
        }

        private static void AppendTreeToString(TextSegment node, StringBuilder b, int indent)
        {
            b.Append(node.Color == Red ? "RED   " : "BLACK ");
            b.AppendLine(node + node.ToDebugString());
            indent += 2;
            if (node.Left != null)
            {
                b.Append(' ', indent);
                b.Append("L: ");
                AppendTreeToString(node.Left, b, indent);
            }
            if (node.Right != null)
            {
                b.Append(' ', indent);
                b.Append("R: ");
                AppendTreeToString(node.Right, b, indent);
            }
        }
#endif

        internal string GetTreeAsString()
        {
#if DEBUG
            var b = new StringBuilder();
            if (_root != null)
                AppendTreeToString(_root, b, 0);
            return b.ToString();
#else
			return "Not available in release build.";
#endif
        }
        #endregion

        #region Red/Black Tree
        internal const bool Red = true;
        internal const bool Black = false;

        private void InsertAsLeft(TextSegment parentNode, TextSegment newNode)
        {
            Debug.Assert(parentNode.Left == null);
            parentNode.Left = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAugmentedData(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void InsertAsRight(TextSegment parentNode, TextSegment newNode)
        {
            Debug.Assert(parentNode.Right == null);
            parentNode.Right = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAugmentedData(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void FixTreeOnInsert(TextSegment node)
        {
            Debug.Assert(node != null);
            Debug.Assert(node.Color == Red);
            Debug.Assert(node.Left == null || node.Left.Color == Black);
            Debug.Assert(node.Right == null || node.Right.Color == Black);

            var parentNode = node.Parent;
            if (parentNode == null)
            {
                // we inserted in the root -> the node must be black
                // since this is a root node, making the node black increments the number of black nodes
                // on all paths by one, so it is still the same for all paths.
                node.Color = Black;
                return;
            }
            if (parentNode.Color == Black)
            {
                // if the parent node where we inserted was black, our red node is placed correctly.
                // since we inserted a red node, the number of black nodes on each path is unchanged
                // -> the tree is still balanced
                return;
            }
            // parentNode is red, so there is a conflict here!

            // because the root is black, parentNode is not the root -> there is a grandparent node
            var grandparentNode = parentNode.Parent;
            var uncleNode = Sibling(parentNode);
            if (uncleNode != null && uncleNode.Color == Red)
            {
                parentNode.Color = Black;
                uncleNode.Color = Black;
                grandparentNode.Color = Red;
                FixTreeOnInsert(grandparentNode);
                return;
            }
            // now we know: parent is red but uncle is black
            // First rotation:
            if (node == parentNode.Right && parentNode == grandparentNode.Left)
            {
                RotateLeft(parentNode);
                node = node.Left;
            }
            else if (node == parentNode.Left && parentNode == grandparentNode.Right)
            {
                RotateRight(parentNode);
                node = node.Right;
            }
            // because node might have changed, reassign variables:
            // ReSharper disable once PossibleNullReferenceException
            parentNode = node.Parent;
            grandparentNode = parentNode.Parent;

            // Now recolor a bit:
            parentNode.Color = Black;
            grandparentNode.Color = Red;
            // Second rotation:
            if (node == parentNode.Left && parentNode == grandparentNode.Left)
            {
                RotateRight(grandparentNode);
            }
            else
            {
                // because of the first rotation, this is guaranteed:
                Debug.Assert(node == parentNode.Right && parentNode == grandparentNode.Right);
                RotateLeft(grandparentNode);
            }
        }

        private void RemoveNode(TextSegment removedNode)
        {
            if (removedNode.Left != null && removedNode.Right != null)
            {
                // replace removedNode with it's in-order successor

                var leftMost = removedNode.Right.LeftMost;
                RemoveNode(leftMost); // remove leftMost from its current location

                // and overwrite the removedNode with it
                ReplaceNode(removedNode, leftMost);
                leftMost.Left = removedNode.Left;
                if (leftMost.Left != null) leftMost.Left.Parent = leftMost;
                leftMost.Right = removedNode.Right;
                if (leftMost.Right != null) leftMost.Right.Parent = leftMost;
                leftMost.Color = removedNode.Color;

                UpdateAugmentedData(leftMost);
                if (leftMost.Parent != null) UpdateAugmentedData(leftMost.Parent);
                return;
            }

            // now either removedNode.left or removedNode.right is null
            // get the remaining child
            var parentNode = removedNode.Parent;
            var childNode = removedNode.Left ?? removedNode.Right;
            ReplaceNode(removedNode, childNode);
            if (parentNode != null) UpdateAugmentedData(parentNode);
            if (removedNode.Color == Black)
            {
                if (childNode != null && childNode.Color == Red)
                {
                    childNode.Color = Black;
                }
                else
                {
                    FixTreeOnDelete(childNode, parentNode);
                }
            }
        }

        private void FixTreeOnDelete(TextSegment node, TextSegment parentNode)
        {
            Debug.Assert(node == null || node.Parent == parentNode);
            if (parentNode == null)
                return;

            // warning: node may be null
            var sibling = Sibling(node, parentNode);
            if (sibling.Color == Red)
            {
                parentNode.Color = Red;
                sibling.Color = Black;
                if (node == parentNode.Left)
                {
                    RotateLeft(parentNode);
                }
                else
                {
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
            if (node == parentNode.Left)
            {
                if (sibling.Right != null)
                {
                    Debug.Assert(sibling.Right.Color == Red);
                    sibling.Right.Color = Black;
                }
                RotateLeft(parentNode);
            }
            else
            {
                if (sibling.Left != null)
                {
                    Debug.Assert(sibling.Left.Color == Red);
                    sibling.Left.Color = Black;
                }
                RotateRight(parentNode);
            }
        }

        private void ReplaceNode(TextSegment replacedNode, TextSegment newNode)
        {
            if (replacedNode.Parent == null)
            {
                Debug.Assert(replacedNode == _root);
                _root = newNode;
            }
            else
            {
                if (replacedNode.Parent.Left == replacedNode)
                    replacedNode.Parent.Left = newNode;
                else
                    replacedNode.Parent.Right = newNode;
            }
            if (newNode != null)
            {
                newNode.Parent = replacedNode.Parent;
            }
            replacedNode.Parent = null;
        }

        private void RotateLeft(TextSegment p)
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
            UpdateAugmentedData(p);
            UpdateAugmentedData(q);
        }

        private void RotateRight(TextSegment p)
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
            UpdateAugmentedData(p);
            UpdateAugmentedData(q);
        }

        private static TextSegment Sibling(TextSegment node)
        {
            return node == node.Parent.Left ? node.Parent.Right : node.Parent.Left;
        }

        private static TextSegment Sibling(TextSegment node, TextSegment parentNode)
        {
            Debug.Assert(node == null || node.Parent == parentNode);
            if (node == parentNode.Left)
                return parentNode.Right;
            else
                return parentNode.Left;
        }

        private static bool GetColor(TextSegment node)
        {
            return node != null && node.Color;
        }
        #endregion

        #region ICollection<T> implementation
        /// <summary>
        /// Gets the number of segments in the tree.
        /// </summary>
        public int Count { get; private set; }

        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Gets whether this tree contains the specified item.
        /// </summary>
        public bool Contains(T item)
        {
            return item != null && item.OwnerTree == this;
        }

        /// <summary>
        /// Copies all segments in this SegmentTree to the specified array.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length < Count)
                throw new ArgumentException("The array is too small", nameof(array));
            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Value must be between 0 and " + (array.Length - Count));
            foreach (var s in this)
            {
                array[arrayIndex++] = s;
            }
        }

        /// <summary>
        /// Gets an enumerator to enumerate the segments.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            if (_root != null)
            {
                var current = _root.LeftMost;
                while (current != null)
                {
                    yield return (T)current;
                    // TODO: check if collection was modified during enumeration
                    current = current.Successor;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}

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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Avalonia.Threading;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// Data structure for efficient management of the document lines (most operations are O(lg n)).
    /// This implements an augmented red-black tree.
    /// See <see cref="DocumentLine"/> for the augmented data.
    /// 
    /// NOTE: The tree is never empty, initially it contains an empty line.
    /// </summary>
    internal sealed class DocumentLineTree : IList<DocumentLine>
    {
        #region Constructor

        private readonly TextDocument _document;
        private DocumentLine _root;

        public DocumentLineTree(TextDocument document)
        {
            _document = document;

            var emptyLine = new DocumentLine(document);
            _root = emptyLine.InitLineNode();
        }
        #endregion

        #region Rotation callbacks
        internal static void UpdateAfterChildrenChange(DocumentLine node)
        {
            var totalCount = 1;
            var totalLength = node.TotalLength;
            if (node.Left != null)
            {
                totalCount += node.Left.NodeTotalCount;
                totalLength += node.Left.NodeTotalLength;
            }
            if (node.Right != null)
            {
                totalCount += node.Right.NodeTotalCount;
                totalLength += node.Right.NodeTotalLength;
            }
            if (totalCount != node.NodeTotalCount
                || totalLength != node.NodeTotalLength)
            {
                node.NodeTotalCount = totalCount;
                node.NodeTotalLength = totalLength;
                if (node.Parent != null) UpdateAfterChildrenChange(node.Parent);
            }
        }

        private static void UpdateAfterRotateLeft(DocumentLine node)
        {
            UpdateAfterChildrenChange(node);

            // not required: rotations only happen on insertions/deletions
            // -> totalCount changes -> the parent is always updated
            //UpdateAfterChildrenChange(node.parent);
        }

        private static void UpdateAfterRotateRight(DocumentLine node)
        {
            UpdateAfterChildrenChange(node);

            // not required: rotations only happen on insertions/deletions
            // -> totalCount changes -> the parent is always updated
            //UpdateAfterChildrenChange(node.parent);
        }
        #endregion

        #region RebuildDocument
        /// <summary>
        /// Rebuild the tree, in O(n).
        /// </summary>
        public void RebuildTree(List<DocumentLine> documentLines)
        {
            var nodes = new DocumentLine[documentLines.Count];
            for (var i = 0; i < documentLines.Count; i++)
            {
                var ls = documentLines[i];
                var node = ls.InitLineNode();
                nodes[i] = node;
            }
            Debug.Assert(nodes.Length > 0);
            // now build the corresponding balanced tree
            var height = GetTreeHeight(nodes.Length);
            Debug.WriteLine("DocumentLineTree will have height: " + height);
            _root = BuildTree(nodes, 0, nodes.Length, height);
            _root.Color = Black;
#if DEBUG
            CheckProperties();
#endif
        }

        internal static int GetTreeHeight(int size)
        {
            if (size == 0)
                return 0;
            else
                return GetTreeHeight(size / 2) + 1;
        }

        /// <summary>
        /// build a tree from a list of nodes
        /// </summary>
        private DocumentLine BuildTree(DocumentLine[] nodes, int start, int end, int subtreeHeight)
        {
            Debug.Assert(start <= end);
            if (start == end)
            {
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
            UpdateAfterChildrenChange(node);
            return node;
        }
        #endregion

        #region GetNodeBy... / Get...FromNode

        private DocumentLine GetNodeByIndex(int index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < _root.NodeTotalCount);
            var node = _root;
            while (true)
            {
                if (node.Left != null && index < node.Left.NodeTotalCount)
                {
                    node = node.Left;
                }
                else
                {
                    if (node.Left != null)
                    {
                        index -= node.Left.NodeTotalCount;
                    }
                    if (index == 0)
                        return node;
                    index--;
                    node = node.Right;
                }
            }
        }

        internal static int GetIndexFromNode(DocumentLine node)
        {
            var index = node.Left?.NodeTotalCount ?? 0;
            while (node.Parent != null)
            {
                if (node == node.Parent.Right)
                {
                    if (node.Parent.Left != null)
                        index += node.Parent.Left.NodeTotalCount;
                    index++;
                }
                node = node.Parent;
            }
            return index;
        }

        private DocumentLine GetNodeByOffset(int offset)
        {
            Debug.Assert(offset >= 0);
            Debug.Assert(offset <= _root.NodeTotalLength);
            if (offset == _root.NodeTotalLength)
            {
                return _root.RightMost;
            }
            var node = _root;
            while (true)
            {
                if (node.Left != null && offset < node.Left.NodeTotalLength)
                {
                    node = node.Left;
                }
                else
                {
                    if (node.Left != null)
                    {
                        offset -= node.Left.NodeTotalLength;
                    }
                    offset -= node.TotalLength;
                    if (offset < 0)
                        return node;
                    node = node.Right;
                }
            }
        }

        internal static int GetOffsetFromNode(DocumentLine node)
        {
            var offset = node.Left?.NodeTotalLength ?? 0;
            while (node.Parent != null)
            {
                if (node == node.Parent.Right)
                {
                    if (node.Parent.Left != null)
                        offset += node.Parent.Left.NodeTotalLength;
                    offset += node.Parent.TotalLength;
                }
                node = node.Parent;
            }
            return offset;
        }
        #endregion

        #region GetLineBy
        public DocumentLine GetByNumber(int number)
        {
            return GetNodeByIndex(number - 1);
        }

        public DocumentLine GetByOffset(int offset)
        {
            return GetNodeByOffset(offset);
        }
        #endregion

        #region LineCount
        public int LineCount => _root.NodeTotalCount;

        #endregion

        #region CheckProperties
#if DEBUG
        [Conditional("DATACONSISTENCYTEST")]
        internal void CheckProperties()
        {
            Debug.Assert(_root.NodeTotalLength == _document.TextLength);
            CheckProperties(_root);

            // check red-black property:
            var blackCount = -1;
            CheckNodeProperties(_root, null, Red, 0, ref blackCount);
        }

        private void CheckProperties(DocumentLine node)
        {
            var totalCount = 1;
            var totalLength = node.TotalLength;
            if (node.Left != null)
            {
                CheckProperties(node.Left);
                totalCount += node.Left.NodeTotalCount;
                totalLength += node.Left.NodeTotalLength;
            }
            if (node.Right != null)
            {
                CheckProperties(node.Right);
                totalCount += node.Right.NodeTotalCount;
                totalLength += node.Right.NodeTotalLength;
            }
            Debug.Assert(node.NodeTotalCount == totalCount);
            Debug.Assert(node.NodeTotalLength == totalLength);
        }

        /*
		1. A node is either red or black.
		2. The root is black.
		3. All leaves are black. (The leaves are the NIL children.)
		4. Both children of every red node are black. (So every red node must have a black parent.)
		5. Every simple path from a node to a descendant leaf contains the same number of black nodes. (Not counting the leaf node.)
		 */
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void CheckNodeProperties(DocumentLine node, DocumentLine parentNode, bool parentColor, int blackCount, ref int expectedBlackCount)
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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string GetTreeAsString()
        {
            var b = new StringBuilder();
            AppendTreeToString(_root, b, 0);
            return b.ToString();
        }

        private static void AppendTreeToString(DocumentLine node, StringBuilder b, int indent)
        {
            b.Append(node.Color == Red ? "RED   " : "BLACK ");
            b.AppendLine(node.ToString());
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
        #endregion

        #region Insert/Remove lines
        public void RemoveLine(DocumentLine line)
        {
            RemoveNode(line);
            line.IsDeleted = true;
        }

        public DocumentLine InsertLineAfter(DocumentLine line, int totalLength)
        {
            var newLine = new DocumentLine(_document) {TotalLength = totalLength};

            InsertAfter(line, newLine);
            return newLine;
        }

        private void InsertAfter(DocumentLine node, DocumentLine newLine)
        {
            var newNode = newLine.InitLineNode();
            if (node.Right == null)
            {
                InsertAsRight(node, newNode);
            }
            else
            {
                InsertAsLeft(node.Right.LeftMost, newNode);
            }
        }
        #endregion

        #region Red/Black Tree
        internal const bool Red = true;
        internal const bool Black = false;

        private void InsertAsLeft(DocumentLine parentNode, DocumentLine newNode)
        {
            Debug.Assert(parentNode.Left == null);
            parentNode.Left = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAfterChildrenChange(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void InsertAsRight(DocumentLine parentNode, DocumentLine newNode)
        {
            Debug.Assert(parentNode.Right == null);
            parentNode.Right = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAfterChildrenChange(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void FixTreeOnInsert(DocumentLine node)
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

        private void RemoveNode(DocumentLine removedNode)
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

                UpdateAfterChildrenChange(leftMost);
                if (leftMost.Parent != null) UpdateAfterChildrenChange(leftMost.Parent);
                return;
            }

            // now either removedNode.left or removedNode.right is null
            // get the remaining child
            var parentNode = removedNode.Parent;
            var childNode = removedNode.Left ?? removedNode.Right;
            ReplaceNode(removedNode, childNode);
            if (parentNode != null) UpdateAfterChildrenChange(parentNode);
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

        private void FixTreeOnDelete(DocumentLine node, DocumentLine parentNode)
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

        private void ReplaceNode(DocumentLine replacedNode, DocumentLine newNode)
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

        private void RotateLeft(DocumentLine p)
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

        private void RotateRight(DocumentLine p)
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

        private static DocumentLine Sibling(DocumentLine node)
        {
            if (node == node.Parent.Left)
                return node.Parent.Right;
            else
                return node.Parent.Left;
        }

        private static DocumentLine Sibling(DocumentLine node, DocumentLine parentNode)
        {
            Debug.Assert(node == null || node.Parent == parentNode);
            if (node == parentNode.Left)
                return parentNode.Right;
            else
                return parentNode.Left;
        }

        private static bool GetColor(DocumentLine node)
        {
            return node != null && node.Color;
        }
        #endregion

        #region IList implementation
        DocumentLine IList<DocumentLine>.this[int index]
        {
            get
            {
                Dispatcher.UIThread.VerifyAccess();
                return GetByNumber(1 + index);
            }
            set => throw new NotSupportedException();
        }

        int ICollection<DocumentLine>.Count
        {
            get
            {
                Dispatcher.UIThread.VerifyAccess();
                return LineCount;
            }
        }

        bool ICollection<DocumentLine>.IsReadOnly => true;

        int IList<DocumentLine>.IndexOf(DocumentLine item)
        {
            Dispatcher.UIThread.VerifyAccess();
            if (item == null || item.IsDeleted)
                return -1;
            var index = item.LineNumber - 1;
            if (index < LineCount && GetNodeByIndex(index) == item)
                return index;
            else
                return -1;
        }

        void IList<DocumentLine>.Insert(int index, DocumentLine item)
        {
            throw new NotSupportedException();
        }

        void IList<DocumentLine>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<DocumentLine>.Add(DocumentLine item)
        {
            throw new NotSupportedException();
        }

        void ICollection<DocumentLine>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<DocumentLine>.Contains(DocumentLine item)
        {
            IList<DocumentLine> self = this;
            return self.IndexOf(item) >= 0;
        }

        void ICollection<DocumentLine>.CopyTo(DocumentLine[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length < LineCount)
                throw new ArgumentException("The array is too small", nameof(array));
            if (arrayIndex < 0 || arrayIndex + LineCount > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Value must be between 0 and " + (array.Length - LineCount));
            foreach (var ls in this)
            {
                array[arrayIndex++] = ls;
            }
        }

        bool ICollection<DocumentLine>.Remove(DocumentLine item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<DocumentLine> GetEnumerator()
        {
            Dispatcher.UIThread.VerifyAccess();
            return Enumerate();
        }

        private IEnumerator<DocumentLine> Enumerate()
        {
            Dispatcher.UIThread.VerifyAccess();
            var line = _root.LeftMost;
            while (line != null)
            {
                yield return line;
                line = line.NextLine;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}

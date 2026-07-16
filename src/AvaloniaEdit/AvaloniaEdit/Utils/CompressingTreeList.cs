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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// A IList{T} implementation that has efficient insertion and removal (in O(lg n) time)
    /// and that saves memory by allocating only one node when a value is repeated in adjacent indices.
    /// Based on this "compression", it also supports efficient InsertRange/SetRange/RemoveRange operations.
    /// </summary>
    /// <remarks>
    /// Current memory usage: 5*IntPtr.Size + 12 + sizeof(T) per node.
    /// Use this class only if lots of adjacent values are identical (can share one node).
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
                                                     Justification = "It's an IList<T> implementation")]
    internal sealed class CompressingTreeList<T> : IList<T>
    {
        // Further memory optimization: this tree could work without parent pointers. But that
        // requires changing most of tree manipulating logic.
        // Also possible is to remove the count field and calculate it as totalCount-left.totalCount-right.totalCount
        // - but that would make tree manipulations more difficult to handle.

        #region Node definition

        private sealed class Node
        {
            internal Node Left { get; set; }
            internal Node Right { get; set; }
            internal Node Parent { get; set; }
            internal bool Color { get; set; }
            internal int Count { get; set; }
            internal int TotalCount { get; set; }
            internal T Value { get; set; }

            public Node(T value, int count)
            {
                Value = value;
                Count = count;
                TotalCount = count;
            }

            internal Node LeftMost
            {
                get
                {
                    var node = this;
                    while (node.Left != null)
                        node = node.Left;
                    return node;
                }
            }

            internal Node RightMost
            {
                get
                {
                    var node = this;
                    while (node.Right != null)
                        node = node.Right;
                    return node;
                }
            }

            /// <summary>
            /// Gets the inorder predecessor of the node.
            /// </summary>
            internal Node Predecessor
            {
                get
                {
                    if (Left != null)
                    {
                        return Left.RightMost;
                    }
                    var node = this;
                    Node oldNode;
                    do
                    {
                        oldNode = node;
                        node = node.Parent;
                        // go up until we are coming out of a right subtree
                    } while (node != null && node.Left == oldNode);
                    return node;
                }
            }

            /// <summary>
            /// Gets the inorder successor of the node.
            /// </summary>
            internal Node Successor
            {
                get
                {
                    if (Right != null)
                    {
                        return Right.LeftMost;
                    }
                    var node = this;
                    Node oldNode;
                    do
                    {
                        oldNode = node;
                        node = node.Parent;
                        // go up until we are coming out of a left subtree
                    } while (node != null && node.Right == oldNode);
                    return node;
                }
            }

            public override string ToString()
            {
                return string.Create(CultureInfo.InvariantCulture, $"[{nameof(TotalCount)}={TotalCount} {nameof(Count)}={Count} {nameof(Value)}={Value}]");
            }
        }
        #endregion

        #region Fields and Constructor

        private readonly Func<T, T, bool> _comparisonFunc;
        private Node _root;

        /// <summary>
        /// Creates a new CompressingTreeList instance.
        /// </summary>
        /// <param name="equalityComparer">The equality comparer used for comparing consequtive values.
        /// A single node may be used to store the multiple values that are considered equal.</param>
        public CompressingTreeList(IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException(nameof(equalityComparer));
            _comparisonFunc = equalityComparer.Equals;
        }

        /// <summary>
        /// Creates a new CompressingTreeList instance.
        /// </summary>
        /// <param name="comparisonFunc">A function that checks two values for equality. If this
        /// function returns true, a single node may be used to store the two values.</param>
        public CompressingTreeList(Func<T, T, bool> comparisonFunc)
        {
            _comparisonFunc = comparisonFunc ?? throw new ArgumentNullException(nameof(comparisonFunc));
        }
        #endregion

        #region InsertRange
        /// <summary>
        /// Inserts <paramref name="item"/> <paramref name="count"/> times at position
        /// <paramref name="index"/>.
        /// </summary>
        public void InsertRange(int index, int count, T item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be between 0 and " + Count);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Value must not be negative");
            if (count == 0)
                return;
            unchecked
            {
                if (Count + count < 0)
                    throw new OverflowException("Cannot insert elements: total number of elements must not exceed int.MaxValue.");
            }

            if (_root == null)
            {
                _root = new Node(item, count);
            }
            else
            {
                var n = GetNode(ref index);
                // check if we can put the value into the node n:
                if (_comparisonFunc(n.Value, item))
                {
                    n.Count += count;
                    UpdateAugmentedData(n);
                }
                else if (index == n.Count)
                {
                    // this can only happen when appending at the end
                    Debug.Assert(n == _root.RightMost);
                    InsertAsRight(n, new Node(item, count));
                }
                else if (index == 0)
                {
                    // insert before:
                    // maybe we can put the value in the previous node?
                    var p = n.Predecessor;
                    if (p != null && _comparisonFunc(p.Value, item))
                    {
                        p.Count += count;
                        UpdateAugmentedData(p);
                    }
                    else
                    {
                        InsertBefore(n, new Node(item, count));
                    }
                }
                else
                {
                    Debug.Assert(index > 0 && index < n.Count);
                    // insert in the middle:
                    // split n into a new node and n
                    n.Count -= index;
                    InsertBefore(n, new Node(n.Value, index));
                    // then insert the new item in between
                    InsertBefore(n, new Node(item, count));
                    UpdateAugmentedData(n);
                }
            }
            CheckProperties();
        }

        private void InsertBefore(Node node, Node newNode)
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

        #region RemoveRange
        /// <summary>
        /// Removes <paramref name="count"/> items starting at position
        /// <paramref name="index"/>.
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be between 0 and " + Count);
            if (count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(count), count, "0 <= length, index(" + index + ")+count <= " + Count);
            if (count == 0)
                return;

            var n = GetNode(ref index);
            if (index + count < n.Count)
            {
                // just remove inside a single node
                n.Count -= count;
                UpdateAugmentedData(n);
            }
            else
            {
                // keep only the part of n from 0 to index
                Node firstNodeBeforeDeletedRange;
                if (index > 0)
                {
                    count -= (n.Count - index);
                    n.Count = index;
                    UpdateAugmentedData(n);
                    firstNodeBeforeDeletedRange = n;
                    n = n.Successor;
                }
                else
                {
                    Debug.Assert(index == 0);
                    firstNodeBeforeDeletedRange = n.Predecessor;
                }
                while (n != null && count >= n.Count)
                {
                    count -= n.Count;
                    var s = n.Successor;
                    RemoveNode(n);
                    n = s;
                }
                if (count > 0)
                {
                    Debug.Assert(n != null && count < n.Count);
                    n.Count -= count;
                    UpdateAugmentedData(n);
                }
                if (n != null)
                {
                    Debug.Assert(n.Predecessor == firstNodeBeforeDeletedRange);
                    if (firstNodeBeforeDeletedRange != null && _comparisonFunc(firstNodeBeforeDeletedRange.Value, n.Value))
                    {
                        firstNodeBeforeDeletedRange.Count += n.Count;
                        RemoveNode(n);
                        UpdateAugmentedData(firstNodeBeforeDeletedRange);
                    }
                }
            }

            CheckProperties();
        }
        #endregion

        #region SetRange
        /// <summary>
        /// Sets <paramref name="count"/> indices starting at <paramref name="index"/> to
        /// <paramref name="item"/>
        /// </summary>
        public void SetRange(int index, int count, T item)
        {
            RemoveRange(index, count);
            InsertRange(index, count, item);
        }
        #endregion

        #region GetNode

        private Node GetNode(ref int index)
        {
            var node = _root;
            while (true)
            {
                if (node.Left != null && index < node.Left.TotalCount)
                {
                    node = node.Left;
                }
                else
                {
                    if (node.Left != null)
                    {
                        index -= node.Left.TotalCount;
                    }
                    if (index < node.Count || node.Right == null)
                        return node;
                    index -= node.Count;
                    node = node.Right;
                }
            }
        }
        #endregion

        #region UpdateAugmentedData

        private void UpdateAugmentedData(Node node)
        {
            var totalCount = node.Count;
            if (node.Left != null) totalCount += node.Left.TotalCount;
            if (node.Right != null) totalCount += node.Right.TotalCount;
            if (node.TotalCount != totalCount)
            {
                node.TotalCount = totalCount;
                if (node.Parent != null)
                    UpdateAugmentedData(node.Parent);
            }
        }
        #endregion

        #region IList<T> implementation
        /// <summary>
        /// Gets or sets an item by index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be between 0 and " + (Count - 1));
                return GetNode(ref index).Value;
            }
            set
            {
                RemoveAt(index);
                Insert(index, value);
            }
        }

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public int Count
        {
            get
            {
                if (_root != null)
                    return _root.TotalCount;
                return 0;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Gets the index of the specified <paramref name="item"/>.
        /// </summary>
        public int IndexOf(T item)
        {
            var index = 0;
            if (_root != null)
            {
                var n = _root.LeftMost;
                while (n != null)
                {
                    if (_comparisonFunc(n.Value, item))
                        return index;
                    index += n.Count;
                    n = n.Successor;
                }
            }
            Debug.Assert(index == Count);
            return -1;
        }

        /// <summary>
        /// Gets the the first index so that all values from the result index to <paramref name="index"/>
        /// are equal.
        /// </summary>
        public int GetStartOfRun(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be between 0 and " + (Count - 1));
            var indexInRun = index;
            GetNode(ref indexInRun);
            return index - indexInRun;
        }

        /// <summary>
        /// Gets the first index after <paramref name="index"/> so that the value at the result index is not
        /// equal to the value at <paramref name="index"/>.
        /// That is, this method returns the exclusive end index of the run of equal values.
        /// </summary>
        public int GetEndOfRun(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be between 0 and " + (Count - 1));
            var indexInRun = index;
            var runLength = GetNode(ref indexInRun).Count;
            return index - indexInRun + runLength;
        }
        
        /// <summary>
        /// Applies the conversion function to all elements in this CompressingTreeList.
        /// </summary>
        public void Transform(Func<T, T> converter)
        {
            if (_root == null)
                return;
            Node prevNode = null;
            for (var n = _root.LeftMost; n != null; n = n.Successor)
            {
                n.Value = converter(n.Value);
                if (prevNode != null && _comparisonFunc(prevNode.Value, n.Value))
                {
                    n.Count += prevNode.Count;
                    UpdateAugmentedData(n);
                    RemoveNode(prevNode);
                }
                prevNode = n;
            }
            CheckProperties();
        }

        /// <summary>
        /// Applies the conversion function to the elements in the specified range.
        /// </summary>
        public void TransformRange(int index, int length, Func<T, T> converter)
        {
            if (_root == null)
                return;
            var endIndex = index + length;
            var pos = index;
            while (pos < endIndex)
            {
                var endPos = Math.Min(endIndex, GetEndOfRun(pos));
                var oldValue = this[pos];
                var newValue = converter(oldValue);
                SetRange(pos, endPos - pos, newValue);
                pos = endPos;
            }
        }

        /// <summary>
        /// Inserts the specified <paramref name="item"/> at <paramref name="index"/>
        /// </summary>
        public void Insert(int index, T item)
        {
            InsertRange(index, 1, item);
        }

        /// <summary>
        /// Removes one item at <paramref name="index"/>
        /// </summary>
        public void RemoveAt(int index)
        {
            RemoveRange(index, 1);
        }

        /// <summary>
        /// Adds the specified <paramref name="item"/> to the end of the list.
        /// </summary>
        public void Add(T item)
        {
            InsertRange(Count, 1, item);
        }

        /// <summary>
        /// Removes all items from this list.
        /// </summary>
        public void Clear()
        {
            _root = null;
        }

        /// <summary>
        /// Gets whether this list contains the specified item.
        /// </summary>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Copies all items in this list to the specified array.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length < Count)
                throw new ArgumentException("The array is too small", nameof(array));
            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Value must be between 0 and " + (array.Length - Count));
            foreach (var v in this)
            {
                array[arrayIndex++] = v;
            }
        }

        /// <summary>
        /// Removes the specified item from this list.
        /// </summary>
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        #endregion

        #region IEnumerable<T>
        /// <summary>
        /// Gets an enumerator for this list.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            if (_root != null)
            {
                var n = _root.LeftMost;
                while (n != null)
                {
                    for (var i = 0; i < n.Count; i++)
                    {
                        yield return n.Value;
                    }
                    n = n.Successor;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Red/Black Tree
        internal const bool Red = true;
        internal const bool Black = false;

        private void InsertAsLeft(Node parentNode, Node newNode)
        {
            Debug.Assert(parentNode.Left == null);
            parentNode.Left = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAugmentedData(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void InsertAsRight(Node parentNode, Node newNode)
        {
            Debug.Assert(parentNode.Right == null);
            parentNode.Right = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAugmentedData(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void FixTreeOnInsert(Node node)
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

        private void RemoveNode(Node removedNode)
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

        private void FixTreeOnDelete(Node node, Node parentNode)
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

        private void ReplaceNode(Node replacedNode, Node newNode)
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

        private void RotateLeft(Node p)
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

        private void RotateRight(Node p)
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

        private static Node Sibling(Node node)
        {
            if (node == node.Parent.Left)
                return node.Parent.Right;
            return node.Parent.Left;
        }

        private static Node Sibling(Node node, Node parentNode)
        {
            Debug.Assert(node == null || node.Parent == parentNode);
            if (node == parentNode.Left)
                return parentNode.Right;
            return parentNode.Left;
        }

        private static bool GetColor(Node node)
        {
            return node != null && node.Color;
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

                // ensure that the tree is compressed:
                var p = _root.LeftMost;
                var n = p.Successor;
                while (n != null)
                {
                    Debug.Assert(!_comparisonFunc(p.Value, n.Value));
                    p = n;
                    n = p.Successor;
                }
            }
#endif
        }

#if DEBUG

        private void CheckProperties(Node node)
        {
            Debug.Assert(node.Count > 0);
            var totalCount = node.Count;
            if (node.Left != null)
            {
                CheckProperties(node.Left);
                totalCount += node.Left.TotalCount;
            }
            if (node.Right != null)
            {
                CheckProperties(node.Right);
                totalCount += node.Right.TotalCount;
            }
            Debug.Assert(node.TotalCount == totalCount);
        }

        /*
		1. A node is either red or black.
		2. The root is black.
		3. All leaves are black. (The leaves are the NIL children.)
		4. Both children of every red node are black. (So every red node must have a black parent.)
		5. Every simple path from a node to a descendant leaf contains the same number of black nodes. (Not counting the leaf node.)
		 */
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void CheckNodeProperties(Node node, Node parentNode, bool parentColor, int blackCount, ref int expectedBlackCount)
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
#endif
        #endregion

        #region GetTreeAsString
        internal string GetTreeAsString()
        {
#if DEBUG
            if (_root == null)
                return "<empty tree>";
            var b = new StringBuilder();
            AppendTreeToString(_root, b, 0);
            return b.ToString();
#else
			return "Not available in release build.";
#endif
        }

#if DEBUG

        private static void AppendTreeToString(Node node, StringBuilder b, int indent)
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
    }
}

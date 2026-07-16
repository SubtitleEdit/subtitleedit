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
using System.Text;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// Class used to represent a node in the tree.
    /// </summary>
    /// <remarks>
    /// There are three types of nodes:
    /// Concat nodes: height&gt;0, left!=null, right!=null, contents==null
    /// Leaf nodes: height==0, left==null, right==null, contents!=null
    /// Function nodes: height==0, left==null, right==null, contents==null, are of type FunctionNode&lt;T&gt;
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    internal class RopeNode<T>
    {
        internal const int NodeSize = 256;

        internal static RopeNode<T> EmptyRopeNode { get; } = new RopeNode<T>(isShared: true) { Contents = new T[NodeSize] };

        // Fields for pointers to sub-nodes. Only non-null for concat nodes (height>=1)

        internal RopeNode<T> Left { get; set; }

        internal RopeNode<T> Right { get; set; }

        // specifies whether this node is shared between multiple ropes
        // the total length of all text in this subtree
        private volatile bool _isShared;

        internal int Length;
        // the height of this subtree: 0 for leaf nodes; 1+max(left.height,right.height) for concat nodes
        internal byte Height;

        // The character data. Only non-null for leaf nodes (height=0) that aren't function nodes.
        internal T[] Contents { get; set; }

        public RopeNode() { }

        protected RopeNode(bool isShared)
        {
            _isShared = isShared;
        }

        internal int Balance => Right.Height - Left.Height;

        [Conditional("DATACONSISTENCYTEST")]
        internal void CheckInvariants()
        {
            if (Height == 0)
            {
                Debug.Assert(Left == null && Right == null);
                if (Contents == null)
                {
                    Debug.Assert(this is FunctionNode<T>);
                    Debug.Assert(Length > 0);
                    Debug.Assert(_isShared);
                }
                else
                {
                    Debug.Assert(Contents != null && Contents.Length == NodeSize);
                    Debug.Assert(Length >= 0 && Length <= NodeSize);
                }
            }
            else
            {
                Debug.Assert(Left != null && Right != null);
                Debug.Assert(Contents == null);
                Debug.Assert(Length == Left.Length + Right.Length);
                Debug.Assert(Height == 1 + Math.Max(Left.Height, Right.Height));
                Debug.Assert(Math.Abs(Balance) <= 1);

                // this is an additional invariant that forces the tree to combine small leafs to prevent excessive memory usage:
                Debug.Assert(Length > NodeSize);
                // note that this invariant ensures that all nodes except for the empty rope's single node have at least length 1

                if (_isShared)
                    Debug.Assert(Left._isShared && Right._isShared);
                Left.CheckInvariants();
                Right.CheckInvariants();
            }
        }

        internal RopeNode<T> Clone()
        {
            if (Height == 0)
            {
                // If a function node needs cloning, we'll evaluate it.
                if (Contents == null)
                    return GetContentNode().Clone();
                var newContents = new T[NodeSize];
                Contents.CopyTo(newContents, 0);
                return new RopeNode<T>
                {
                    Length = Length,
                    Contents = newContents
                };
            }
            return new RopeNode<T>
            {
                Left = Left,
                Right = Right,
                Length = Length,
                Height = Height
            };
        }

        internal RopeNode<T> CloneIfShared()
        {
            if (_isShared)
                return Clone();
            return this;
        }

        internal void Publish()
        {
            if (!_isShared)
            {
                Left?.Publish();
                Right?.Publish();
                // it's important that isShared=true is set at the end:
                // Publish() must not return until the whole subtree is marked as shared, even when
                // Publish() is called concurrently.
                _isShared = true;
            }
        }

        internal static RopeNode<T> CreateFromArray(T[] arr, int index, int length)
        {
            if (length == 0)
            {
                return EmptyRopeNode;
            }
            var node = CreateNodes(length);
            return node.StoreElements(0, arr, index, length);
        }

        internal static RopeNode<T> CreateNodes(int totalLength)
        {
            var leafCount = (totalLength + NodeSize - 1) / NodeSize;
            return CreateNodes(leafCount, totalLength);
        }

        private static RopeNode<T> CreateNodes(int leafCount, int totalLength)
        {
            Debug.Assert(leafCount > 0);
            Debug.Assert(totalLength > 0);
            var result = new RopeNode<T> { Length = totalLength };
            if (leafCount == 1)
            {
                result.Contents = new T[NodeSize];
            }
            else
            {
                var rightSide = leafCount / 2;
                var leftSide = leafCount - rightSide;
                var leftLength = leftSide * NodeSize;
                result.Left = CreateNodes(leftSide, leftLength);
                result.Right = CreateNodes(rightSide, totalLength - leftLength);
                result.Height = (byte)(1 + Math.Max(result.Left.Height, result.Right.Height));
            }
            return result;
        }

        /// <summary>
        /// Balances this node and recomputes the 'height' field.
        /// This method assumes that the children of this node are already balanced and have an up-to-date 'height' value.
        /// </summary>
        internal void Rebalance()
        {
            // Rebalance() shouldn't be called on shared nodes - it's only called after modifications!
            Debug.Assert(!_isShared);
            // leaf nodes are always balanced (we don't use 'height' to detect leaf nodes here
            // because Balance is supposed to recompute the height).
            if (Left == null)
                return;

            // ensure we didn't miss a MergeIfPossible step
            Debug.Assert(Length > NodeSize);

            // We need to loop until it's balanced. Rotations might cause two small leaves to combine to a larger one,
            // which changes the height and might mean we need additional balancing steps.
            while (Math.Abs(Balance) > 1)
            {
                // AVL balancing
                // note: because we don't care about the identity of concat nodes, this works a little different than usual
                // tree rotations: in our implementation, the "this" node will stay at the top, only its children are rearranged
                if (Balance > 1)
                {
                    if (Right.Balance < 0)
                    {
                        Right = Right.CloneIfShared();
                        Right.RotateRight();
                    }
                    RotateLeft();
                    // If 'this' was unbalanced by more than 2, we've shifted some of the inbalance to the left node; so rebalance that.
                    Left.Rebalance();
                }
                else if (Balance < -1)
                {
                    if (Left.Balance > 0)
                    {
                        Left = Left.CloneIfShared();
                        Left.RotateLeft();
                    }
                    RotateRight();
                    // If 'this' was unbalanced by more than 2, we've shifted some of the inbalance to the right node; so rebalance that.
                    Right.Rebalance();
                }
            }

            Debug.Assert(Math.Abs(Balance) <= 1);
            Height = (byte)(1 + Math.Max(Left.Height, Right.Height));
        }

        private void RotateLeft()
        {
            Debug.Assert(!_isShared);

            /* Rotate tree to the left
			 * 
			 *       this               this
			 *       /  \               /  \
			 *      A   right   ===>  left  C
			 *           / \          / \
			 *          B   C        A   B
			 */
            var a = Left;
            var b = Right.Left;
            var c = Right.Right;
            // reuse right concat node, if possible
            Left = Right._isShared ? new RopeNode<T>() : Right;
            Left.Left = a;
            Left.Right = b;
            Left.Length = a.Length + b.Length;
            Left.Height = (byte)(1 + Math.Max(a.Height, b.Height));
            Right = c;

            Left.MergeIfPossible();
        }

        private void RotateRight()
        {
            Debug.Assert(!_isShared);

            /* Rotate tree to the right
			 * 
			 *       this             this
			 *       /  \             /  \
			 *     left  C   ===>    A  right
			 *     / \                   /  \
			 *    A   B                 B    C
			 */
            var a = Left.Left;
            var b = Left.Right;
            var c = Right;
            // reuse left concat node, if possible
            Right = Left._isShared ? new RopeNode<T>() : Left;
            Right.Left = b;
            Right.Right = c;
            Right.Length = b.Length + c.Length;
            Right.Height = (byte)(1 + Math.Max(b.Height, c.Height));
            Left = a;

            Right.MergeIfPossible();
        }

        private void MergeIfPossible()
        {
            Debug.Assert(!_isShared);

            if (Length <= NodeSize)
            {
                // Convert this concat node to leaf node.
                // We know left and right cannot be concat nodes (they would have merged already),
                // but they could be function nodes.
                Height = 0;
                var lengthOnLeftSide = Left.Length;
                if (Left._isShared)
                {
                    Contents = new T[NodeSize];
                    Left.CopyTo(0, Contents, 0, lengthOnLeftSide);
                }
                else
                {
                    // must be a leaf node: function nodes are always marked shared
                    Debug.Assert(Left.Contents != null);
                    // steal buffer from left side
                    Contents = Left.Contents;
#if DEBUG
                    // In debug builds, explicitly mark left node as 'damaged' - but no one else should be using it
                    // because it's not shared.
                    Left.Contents = Array.Empty<T>();
#endif
                }
                Left = null;
                Right.CopyTo(0, Contents, lengthOnLeftSide, Right.Length);
                Right = null;
            }
        }

        /// <summary>
        /// Copies from the array to this node.
        /// </summary>
        internal RopeNode<T> StoreElements(int index, T[] array, int arrayIndex, int count)
        {
            var result = CloneIfShared();
            // result cannot be function node after a call to Clone()
            if (result.Height == 0)
            {
                // leaf node:
                Array.Copy(array, arrayIndex, result.Contents, index, count);
            }
            else
            {
                // concat node:
                if (index + count <= result.Left.Length)
                {
                    result.Left = result.Left.StoreElements(index, array, arrayIndex, count);
                }
                else if (index >= Left.Length)
                {
                    result.Right = result.Right.StoreElements(index - result.Left.Length, array, arrayIndex, count);
                }
                else
                {
                    var amountInLeft = result.Left.Length - index;
                    result.Left = result.Left.StoreElements(index, array, arrayIndex, amountInLeft);
                    result.Right = result.Right.StoreElements(0, array, arrayIndex + amountInLeft, count - amountInLeft);
                }
                result.Rebalance(); // tree layout might have changed if function nodes were replaced with their content
            }
            return result;
        }

        /// <summary>
        /// Copies from this node to the array.
        /// </summary>
        internal void CopyTo(int index, Span<T> array, int arrayIndex, int count)
        {
            if (Height == 0)
            {
                if (Contents == null)
                {
                    // function node
                    GetContentNode().CopyTo(index, array, arrayIndex, count);
                }
                else
                {
                    // leaf node
                    Contents.AsSpan(index, count).CopyTo(array.Slice(arrayIndex, count));
                }
            }
            else
            {
                // concat node
                if (index + count <= Left.Length)
                {
                    Left.CopyTo(index, array, arrayIndex, count);
                }
                else if (index >= Left.Length)
                {
                    Right.CopyTo(index - Left.Length, array, arrayIndex, count);
                }
                else
                {
                    var amountInLeft = Left.Length - index;
                    Left.CopyTo(index, array, arrayIndex, amountInLeft);
                    Right.CopyTo(0, array, arrayIndex + amountInLeft, count - amountInLeft);
                }
            }
        }

        internal RopeNode<T> SetElement(int offset, T value)
        {
            var result = CloneIfShared();
            // result of CloneIfShared() is leaf or concat node
            if (result.Height == 0)
            {
                result.Contents[offset] = value;
            }
            else
            {
                if (offset < result.Left.Length)
                {
                    result.Left = result.Left.SetElement(offset, value);
                }
                else
                {
                    result.Right = result.Right.SetElement(offset - result.Left.Length, value);
                }
                result.Rebalance(); // tree layout might have changed if function nodes were replaced with their content
            }
            return result;
        }

        internal static RopeNode<T> Concat(RopeNode<T> left, RopeNode<T> right)
        {
            if (left.Length == 0)
                return right;
            if (right.Length == 0)
                return left;

            if (left.Length + right.Length <= NodeSize)
            {
                left = left.CloneIfShared();
                // left is guaranteed to be leaf node after cloning:
                // - it cannot be function node (due to clone)
                // - it cannot be concat node (too short)
                right.CopyTo(0, left.Contents, left.Length, right.Length);
                left.Length += right.Length;
                return left;
            }
            var concatNode = new RopeNode<T>
            {
                Left = left,
                Right = right,
                Length = left.Length + right.Length
            };
            concatNode.Rebalance();
            return concatNode;
        }

        /// <summary>
        /// Splits this leaf node at offset and returns a new node with the part of the text after offset.
        /// </summary>
        private RopeNode<T> SplitAfter(int offset)
        {
            Debug.Assert(!_isShared && Height == 0 && Contents != null);
            var newPart = new RopeNode<T>
            {
                Contents = new T[NodeSize],
                Length = Length - offset
            };
            Array.Copy(Contents, offset, newPart.Contents, 0, newPart.Length);
            Length = offset;
            return newPart;
        }

        internal RopeNode<T> Insert(int offset, RopeNode<T> newElements)
        {
            if (offset == 0)
            {
                return Concat(newElements, this);
            }
            if (offset == Length)
            {
                return Concat(this, newElements);
            }

            // first clone this node (converts function nodes to leaf or concat nodes)
            var result = CloneIfShared();
            if (result.Height == 0)
            {
                // leaf node: we'll need to split this node
                var left = result;
                var right = left.SplitAfter(offset);
                return Concat(Concat(left, newElements), right);
            }
            // concat node
            if (offset < result.Left.Length)
            {
                result.Left = result.Left.Insert(offset, newElements);
            }
            else
            {
                result.Right = result.Right.Insert(offset - result.Left.Length, newElements);
            }
            result.Length += newElements.Length;
            result.Rebalance();
            return result;
        }

        internal RopeNode<T> Insert(int offset, T[] array, int arrayIndex, int count)
        {
            Debug.Assert(count > 0);

            if (Length + count < RopeNode<char>.NodeSize)
            {
                var result = CloneIfShared();
                // result must be leaf node (Clone never returns function nodes, too short for concat node)
                var lengthAfterOffset = result.Length - offset;
                var resultContents = result.Contents;
                for (var i = lengthAfterOffset; i >= 0; i--)
                {
                    resultContents[i + offset + count] = resultContents[i + offset];
                }
                Array.Copy(array, arrayIndex, resultContents, offset, count);
                result.Length += count;
                return result;
            }
            if (Height == 0)
            {
                // TODO: implement this more efficiently?
                return Insert(offset, CreateFromArray(array, arrayIndex, count));
            }
            {
                // this is a concat node (both leafs and function nodes are handled by the case above)
                var result = CloneIfShared();
                if (offset < result.Left.Length)
                {
                    result.Left = result.Left.Insert(offset, array, arrayIndex, count);
                }
                else
                {
                    result.Right = result.Right.Insert(offset - result.Left.Length, array, arrayIndex, count);
                }
                result.Length += count;
                result.Rebalance();
                return result;
            }
        }

        internal RopeNode<T> RemoveRange(int index, int count)
        {
            Debug.Assert(count > 0);

            // produce empty node when one node is deleted completely
            if (index == 0 && count == Length)
                return EmptyRopeNode;

            var endIndex = index + count;
            var result = CloneIfShared(); // convert function node to concat/leaf
            if (result.Height == 0)
            {
                var remainingAfterEnd = result.Length - endIndex;
                for (var i = 0; i < remainingAfterEnd; i++)
                {
                    result.Contents[index + i] = result.Contents[endIndex + i];
                }
                result.Length -= count;
            }
            else
            {
                if (endIndex <= result.Left.Length)
                {
                    // deletion is only within the left part
                    result.Left = result.Left.RemoveRange(index, count);
                }
                else if (index >= result.Left.Length)
                {
                    // deletion is only within the right part
                    result.Right = result.Right.RemoveRange(index - result.Left.Length, count);
                }
                else
                {
                    // deletion overlaps both parts
                    var deletionAmountOnLeftSide = result.Left.Length - index;
                    result.Left = result.Left.RemoveRange(index, deletionAmountOnLeftSide);
                    result.Right = result.Right.RemoveRange(0, count - deletionAmountOnLeftSide);
                }
                // The deletion might have introduced empty nodes. Those must be removed.
                if (result.Left.Length == 0)
                    return result.Right;
                if (result.Right.Length == 0)
                    return result.Left;

                result.Length -= count;
                result.MergeIfPossible();
                result.Rebalance();
            }
            return result;
        }

        #region Debug Output
#if DEBUG
        internal virtual void AppendTreeToString(StringBuilder b, int indent)
        {
            b.AppendLine(ToString());
            indent += 2;
            if (Left != null)
            {
                b.Append(' ', indent);
                b.Append("L: ");
                Left.AppendTreeToString(b, indent);
            }
            if (Right != null)
            {
                b.Append(' ', indent);
                b.Append("R: ");
                Right.AppendTreeToString(b, indent);
            }
        }

        public override string ToString()
        {
            if (Contents != null)
            {
#pragma warning disable IDE0019 // Use pattern matching
                var charContents = Contents as char[];
#pragma warning restore IDE0019 // Use pattern matching
                if (charContents != null)
                    return "[Leaf length=" + Length + ", isShared=" + _isShared + ", text=\"" + new string(charContents, 0, Length) + "\"]";
                return "[Leaf length=" + Length + ", isShared=" + _isShared + "\"]";
            }
            return "[Concat length=" + Length + ", isShared=" + _isShared + ", height=" + Height + ", Balance=" + Balance + "]";
        }

        internal string GetTreeAsString()
        {
            var b = new StringBuilder();
            AppendTreeToString(b, 0);
            return b.ToString();
        }
#endif
        #endregion

        /// <summary>
        /// Gets the root node of the subtree from a lazily evaluated function node.
        /// Such nodes are always marked as shared.
        /// GetContentNode() will return either a Concat or Leaf node, never another FunctionNode.
        /// </summary>
        internal virtual RopeNode<T> GetContentNode()
        {
            throw new InvalidOperationException("Called GetContentNode() on non-FunctionNode.");
        }
    }

    internal sealed class FunctionNode<T> : RopeNode<T>
    {
        private Func<Rope<T>> _initializer;
        private RopeNode<T> _cachedResults;

        public FunctionNode(int length, Func<Rope<T>> initializer) : base(isShared: true)
        {
            Debug.Assert(length > 0);
            Debug.Assert(initializer != null);

            Length = length;
            _initializer = initializer;
            // Function nodes are immediately shared, but cannot be cloned.
            // This ensures we evaluate every initializer only once.
        }

        internal override RopeNode<T> GetContentNode()
        {
            lock (this)
            {
                if (_cachedResults == null)
                {
                    if (_initializer == null)
                        throw new InvalidOperationException("Trying to load this node recursively; or: a previous call to a rope initializer failed.");
                    var initializerCopy = _initializer;
                    _initializer = null;
                    var resultRope = initializerCopy();
                    if (resultRope == null)
                        throw new InvalidOperationException("Rope initializer returned null.");
                    var resultNode = resultRope.Root;
                    resultNode.Publish(); // result is shared between returned rope and the rope containing this function node
                    if (resultNode.Length != Length)
                        throw new InvalidOperationException("Rope initializer returned rope with incorrect length.");
                    if (resultNode.Height == 0 && resultNode.Contents == null)
                    {
                        // ResultNode is another function node.
                        // We want to guarantee that GetContentNode() never returns function nodes, so we have to
                        // go down further in the tree.
                        _cachedResults = resultNode.GetContentNode();
                    }
                    else
                    {
                        _cachedResults = resultNode;
                    }
                }
                return _cachedResults;
            }
        }

#if DEBUG
        internal override void AppendTreeToString(StringBuilder b, int indent)
        {
            RopeNode<T> resultNode;
            lock (this)
            {
                b.AppendLine(ToString());
                resultNode = _cachedResults;
            }
            indent += 2;
            if (resultNode != null)
            {
                b.Append(' ', indent);
                b.Append("C: ");
                resultNode.AppendTreeToString(b, indent);
            }
        }

        public override string ToString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"[{nameof(FunctionNode<>)} length={Length} initializerRan={(_initializer == null)}]");
        }
#endif
    }
}

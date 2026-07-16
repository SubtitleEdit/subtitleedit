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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// A kind of List&lt;T&gt;, but more efficient for random insertions/removal.
    /// Also has cheap Clone() and SubRope() implementations.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe: multiple concurrent write operations or writes concurrent to reads have undefined behaviour.
    /// Concurrent reads, however, are safe.
    /// However, clones of a rope are safe to use on other threads even though they share data with the original rope.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class Rope<T> : IList<T>, ICloneable
    {
        internal RopeNode<T> Root { get; set; }

        internal Rope(RopeNode<T> root)
        {
            Root = root;
            root.CheckInvariants();
        }

        /// <summary>
        /// Creates a new rope representing the empty string.
        /// </summary>
        public Rope()
        {
            // we'll construct the empty rope as a clone of an imaginary static empty rope
            Root = RopeNode<T>.EmptyRopeNode;
            Root.CheckInvariants();
        }

        /// <summary>
        /// Creates a rope from the specified input.
        /// This operation runs in O(N).
        /// </summary>
        /// <exception cref="ArgumentNullException">input is null.</exception>
        public Rope(IEnumerable<T> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (input is Rope<T> inputRope)
            {
                // clone ropes instead of copying them
                inputRope.Root.Publish();
                Root = inputRope.Root;
            }
            else
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                // ReSharper disable ExpressionIsAlwaysNull
                // ReSharper disable SuspiciousTypeConversion.Global
#pragma warning disable IDE0019 // Use pattern matching
                var text = input as string;
#pragma warning restore IDE0019 // Use pattern matching
                // ReSharper restore SuspiciousTypeConversion.Global
                if (text != null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                // ReSharper restore ExpressionIsAlwaysNull
                // ReSharper disable HeuristicUnreachableCode
                {
                    // if a string is IEnumerable<T>, then T must be char
                    ((Rope<char>)(object)this).Root = CharRope.InitFromString(text);
                }
                // ReSharper restore HeuristicUnreachableCode
                else
                {
                    var arr = ToArray(input);
                    Root = RopeNode<T>.CreateFromArray(arr, 0, arr.Length);
                }
            }
            Root.CheckInvariants();
        }

        /// <summary>
        /// Creates a rope from a part of the array.
        /// This operation runs in O(N).
        /// </summary>
        /// <exception cref="ArgumentNullException">input is null.</exception>
        public Rope(T[] array, int arrayIndex, int count)
        {
            VerifyArrayWithRange(array, arrayIndex, count);
            Root = RopeNode<T>.CreateFromArray(array, arrayIndex, count);
            Root.CheckInvariants();
        }

        /// <summary>
        /// Creates a new rope that lazily initalizes its content.
        /// </summary>
        /// <param name="length">The length of the rope that will be lazily loaded.</param>
        /// <param name="initializer">
        /// The callback that provides the content for this rope.
        /// <paramref name="initializer"/> will be called exactly once when the content of this rope is first requested.
        /// It must return a rope with the specified length.
        /// Because the initializer function is not called when a rope is cloned, and such clones may be used on another threads,
        /// it is possible for the initializer callback to occur on any thread.
        /// </param>
        /// <remarks>
        /// Any modifications inside the rope will also cause the content to be initialized.
        /// However, insertions at the beginning and the end, as well as inserting this rope into another or
        /// using the <see cref="Concat(Rope{T},Rope{T})"/> method, allows constructions of larger ropes where parts are
        /// lazily loaded.
        /// However, even methods like Concat may sometimes cause the initializer function to be called, e.g. when
        /// two short ropes are concatenated.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Rope(int length, Func<Rope<T>> initializer)
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must not be negative");
            Root = length == 0 ? RopeNode<T>.EmptyRopeNode : new FunctionNode<T>(length, initializer);
            Root.CheckInvariants();
        }

        private static T[] ToArray(IEnumerable<T> input)
        {
            var arr = input as T[];
            return arr ?? input.ToArray();
        }

        /// <summary>
        /// Clones the rope.
        /// This operation runs in linear time to the number of rope nodes touched since the last clone was created.
        /// If you count the per-node cost to the operation modifying the rope (doing this doesn't increase the complexity of the modification operations);
        /// the remainder of Clone() runs in O(1).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public Rope<T> Clone()
        {
            // The Publish() call actually modifies this rope instance; but this modification is thread-safe
            // as long as the tree structure doesn't change during the operation.
            Root.Publish();
            return new Rope<T>(Root);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Resets the rope to an empty list.
        /// Runs in O(1).
        /// </summary>
        public void Clear()
        {
            Root = RopeNode<T>.EmptyRopeNode;
            OnChanged();
        }

        /// <summary>
        /// Gets the length of the rope.
        /// Runs in O(1).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public int Length => Root.Length;

        /// <summary>
        /// Gets the length of the rope.
        /// Runs in O(1).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public int Count => Root.Length;

        /// <summary>
        /// Inserts another rope into this rope.
        /// Runs in O(lg N + lg M), plus a per-node cost as if <c>newElements.Clone()</c> was called.
        /// </summary>
        /// <exception cref="ArgumentNullException">newElements is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index or length is outside the valid range.</exception>
        public void InsertRange(int index, Rope<T> newElements)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "0 <= index <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
            if (newElements == null)
                throw new ArgumentNullException(nameof(newElements));
            newElements.Root.Publish();
            Root = Root.Insert(index, newElements.Root);
            OnChanged();
        }

        /// <summary>
        /// Inserts new elemetns into this rope.
        /// Runs in O(lg N + M), where N is the length of this rope and M is the number of new elements.
        /// </summary>
        /// <exception cref="ArgumentNullException">newElements is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index or length is outside the valid range.</exception>
        public void InsertRange(int index, IEnumerable<T> newElements)
        {
            if (newElements == null)
                throw new ArgumentNullException(nameof(newElements));
            if (newElements is Rope<T> newElementsRope)
            {
                InsertRange(index, newElementsRope);
            }
            else
            {
                var arr = ToArray(newElements);
                InsertRange(index, arr, 0, arr.Length);
            }
        }

        /// <summary>
        /// Inserts new elements into this rope.
        /// Runs in O(lg N + M), where N is the length of this rope and M is the number of new elements.
        /// </summary>
        /// <exception cref="ArgumentNullException">newElements is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index or length is outside the valid range.</exception>
        public void InsertRange(int index, T[] array, int arrayIndex, int count)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "0 <= index <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
            VerifyArrayWithRange(array, arrayIndex, count);
            if (count > 0)
            {
                Root = Root.Insert(index, array, arrayIndex, count);
                OnChanged();
            }
        }

        /// <summary>
        /// Appends multiple elements to the end of this rope.
        /// Runs in O(lg N + M), where N is the length of this rope and M is the number of new elements.
        /// </summary>
        /// <exception cref="ArgumentNullException">newElements is null.</exception>
        public void AddRange(IEnumerable<T> newElements)
        {
            InsertRange(Length, newElements);
        }

        /// <summary>
        /// Appends another rope to the end of this rope.
        /// Runs in O(lg N + lg M), plus a per-node cost as if <c>newElements.Clone()</c> was called.
        /// </summary>
        /// <exception cref="ArgumentNullException">newElements is null.</exception>
        public void AddRange(Rope<T> newElements)
        {
            InsertRange(Length, newElements);
        }

        /// <summary>
        /// Appends new elements to the end of this rope.
        /// Runs in O(lg N + M), where N is the length of this rope and M is the number of new elements.
        /// </summary>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        public void AddRange(T[] array, int arrayIndex, int count)
        {
            InsertRange(Length, array, arrayIndex, count);
        }

        /// <summary>
        /// Removes a range of elements from the rope.
        /// Runs in O(lg N).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">offset or length is outside the valid range.</exception>
        public void RemoveRange(int index, int count)
        {
            VerifyRange(index, count);
            if (count > 0)
            {
                Root = Root.RemoveRange(index, count);
                OnChanged();
            }
        }

        /// <summary>
        /// Copies a range of the specified array into the rope, overwriting existing elements.
        /// Runs in O(lg N + M).
        /// </summary>
        public void SetRange(int index, T[] array, int arrayIndex, int count)
        {
            VerifyRange(index, count);
            VerifyArrayWithRange(array, arrayIndex, count);
            if (count > 0)
            {
                Root = Root.StoreElements(index, array, arrayIndex, count);
                OnChanged();
            }
        }

        /// <summary>
        /// Creates a new rope and initializes it with a part of this rope.
        /// Runs in O(lg N) plus a per-node cost as if <c>this.Clone()</c> was called.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">offset or length is outside the valid range.</exception>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public Rope<T> GetRange(int index, int count)
        {
            VerifyRange(index, count);
            var newRope = Clone();
            var endIndex = index + count;
            newRope.RemoveRange(endIndex, newRope.Length - endIndex);
            newRope.RemoveRange(0, index);
            return newRope;
        }

        /*
        #region Equality
        /// <summary>
        /// Gets whether the two ropes have the same content.
        /// Runs in O(N + M).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public bool Equals(Rope other)
        {
            if (other == null)
                return false;
            // quick detection for ropes that are clones of each other:
            if (other.root == this.root)
                return true;
            if (other.Length != this.Length)
                return false;
            using (RopeTextReader a = new RopeTextReader(this, false)) {
                using (RopeTextReader b = new RopeTextReader(other, false)) {
                    int charA, charB;
                    do {
                        charA = a.Read();
                        charB = b.Read();
                        if (charA != charB)
                            return false;
                    } while (charA != -1);
                    return true;
                }
            }
        }
        
        /// <summary>
        /// Gets whether two ropes have the same content.
        /// Runs in O(N + M).
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as Rope);
        }
        
        /// <summary>
        /// Calculates the hash code of the rope's content.
        /// Runs in O(N).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public override int GetHashCode()
        {
            int hashcode = 0;
            using (RopeTextReader reader = new RopeTextReader(this, false)) {
                unchecked {
                    int val;
                    while ((val = reader.Read()) != -1) {
                        hashcode = hashcode * 31 + val;
                    }
                }
            }
            return hashcode;
        }
        #endregion
         */

        /// <summary>
        /// Concatenates two ropes. The input ropes are not modified.
        /// Runs in O(lg N + lg M).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Rope<T> Concat(Rope<T> left, Rope<T> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));
            left.Root.Publish();
            right.Root.Publish();
            return new Rope<T>(RopeNode<T>.Concat(left.Root, right.Root));
        }

        /// <summary>
        /// Concatenates multiple ropes. The input ropes are not modified.
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static Rope<T> Concat(params Rope<T>[] ropes)
        {
            if (ropes == null)
                throw new ArgumentNullException(nameof(ropes));
            var result = new Rope<T>();
            foreach (var r in ropes)
                result.AddRange(r);
            return result;
        }

        #region Caches / Changed event
        internal readonly struct RopeCacheEntry
        {
            internal RopeNode<T> Node { get; }
            internal int NodeStartIndex { get; }

            internal RopeCacheEntry(RopeNode<T> node, int nodeStartOffset)
            {
                Node = node;
                NodeStartIndex = nodeStartOffset;
            }

            internal bool IsInside(int offset)
            {
                return offset >= NodeStartIndex && offset < NodeStartIndex + Node.Length;
            }
        }

        // cached pointer to 'last used node', used to speed up accesses by index that are close together
        private volatile ImmutableStack<RopeCacheEntry> _lastUsedNodeStack;

        internal void OnChanged()
        {
            _lastUsedNodeStack = null;

            Root.CheckInvariants();
        }
        #endregion

        #region GetChar / SetChar
        /// <summary>
        /// Gets/Sets a single character.
        /// Runs in O(lg N) for random access. Sequential read-only access benefits from a special optimization and runs in amortized O(1).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Offset is outside the valid range (0 to Length-1).</exception>
        /// <remarks>
        /// The getter counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public T this[int index]
        {
            get
            {
                // use unsigned integers - this way negative values for index overflow and can be tested for with the same check
                if (unchecked((uint)index >= (uint)Length))
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "0 <= index < " + Length.ToString(CultureInfo.InvariantCulture));
                }
                var entry = FindNodeUsingCache(index).PeekOrDefault();
                return entry.Node.Contents[index - entry.NodeStartIndex];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "0 <= index < " + Length.ToString(CultureInfo.InvariantCulture));
                }
                Root = Root.SetElement(index, value);
                OnChanged();
                /* Here's a try at implementing the setter using the cached node stack (UNTESTED code!).
                 * However I don't use the code because it's complicated and doesn't integrate correctly with change notifications.
                 * Instead, I'll use the much easier to understand recursive solution.
                 * Oh, and it also doesn't work correctly with function nodes.
                ImmutableStack<RopeCacheEntry> nodeStack = FindNodeUsingCache(offset);
                RopeCacheEntry entry = nodeStack.Peek();
                if (!entry.node.isShared) {
                    entry.node.contents[offset - entry.nodeStartOffset] = value;
                    // missing: clear the caches except for the node stack cache (e.g. ToString() cache?)
                } else {
                    RopeNode oldNode = entry.node;
                    RopeNode newNode = oldNode.Clone();
                    newNode.contents[offset - entry.nodeStartOffset] = value;
                    for (nodeStack = nodeStack.Pop(); !nodeStack.IsEmpty; nodeStack = nodeStack.Pop()) {
                        RopeNode parentNode = nodeStack.Peek().node;
                        RopeNode newParentNode = parentNode.CloneIfShared();
                        if (newParentNode.left == oldNode) {
                            newParentNode.left = newNode;
                        } else {
                            Debug.Assert(newParentNode.right == oldNode);
                            newParentNode.right = newNode;
                        }
                        if (parentNode == newParentNode) {
                            // we were able to change the existing node (it was not shared);
                            // there's no reason to go further upwards
                            ClearCacheOnModification();
                            return;
                        } else {
                            oldNode = parentNode;
                            newNode = newParentNode;
                        }
                    }
                    // we reached the root of the rope.
                    Debug.Assert(root == oldNode);
                    root = newNode;
                    ClearCacheOnModification();
                }*/
            }
        }

        internal ImmutableStack<RopeCacheEntry> FindNodeUsingCache(int index)
        {
            Debug.Assert(index >= 0 && index < Length);

            // thread safety: fetch stack into local variable
            var stack = _lastUsedNodeStack;
            var oldStack = stack;

            if (stack == null)
            {
                stack = ImmutableStack<RopeCacheEntry>.Empty.Push(new RopeCacheEntry(Root, 0));
            }
            while (!stack.PeekOrDefault().IsInside(index))
                stack = stack.Pop();
            while (true)
            {
                var entry = stack.PeekOrDefault();
                // check if we've reached a leaf or function node
                if (entry.Node.Height == 0)
                {
                    if (entry.Node.Contents == null)
                    {
                        // this is a function node - go down into its subtree
                        entry = new RopeCacheEntry(entry.Node.GetContentNode(), entry.NodeStartIndex);
                        // entry is now guaranteed NOT to be another function node
                    }
                    if (entry.Node.Contents != null)
                    {
                        // this is a node containing actual content, so we're done
                        break;
                    }
                }
                // go down towards leaves
                stack = stack.Push(index - entry.NodeStartIndex >= entry.Node.Left.Length
                    ? new RopeCacheEntry(entry.Node.Right, entry.NodeStartIndex + entry.Node.Left.Length)
                    : new RopeCacheEntry(entry.Node.Left, entry.NodeStartIndex));
            }

            // write back stack to volatile cache variable
            // (in multithreaded access, it doesn't matter which of the threads wins - it's just a cache)
            if (oldStack != stack)
            {
                // no need to write when we the cache variable didn't change
                _lastUsedNodeStack = stack;
            }

            // this method guarantees that it finds a leaf node
            Debug.Assert(stack.Peek().Node.Contents != null);
            return stack;
        }
        #endregion

        #region ToString / WriteTo
        internal void VerifyRange(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "0 <= startIndex <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
            if (length < 0 || startIndex + length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "0 <= length, startIndex(" + startIndex + ")+length <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        internal static void VerifyArrayWithRange(Span<T> array, int arrayIndex, int count)
        {
            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "0 <= arrayIndex <= " + array.Length.ToString(CultureInfo.InvariantCulture));
            }
            if (count < 0 || arrayIndex + count > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "0 <= length, arrayIndex(" + arrayIndex + ")+count <= " + array.Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Creates a string from the rope. Runs in O(N).
        /// </summary>
        /// <returns>A string consisting of all elements in the rope as comma-separated list in {}.
        /// As a special case, Rope&lt;char&gt; will return its contents as string without any additional separators or braces,
        /// so it can be used like StringBuilder.ToString().</returns>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public override string ToString()
        {
#pragma warning disable IDE0019 // Use pattern matching
            var charRope = this as Rope<char>;
#pragma warning restore IDE0019 // Use pattern matching
            if (charRope != null)
            {
                return charRope.ToString(0, Length);
            }

            var b = new StringBuilder();
            foreach (var element in this)
            {
                if (b.Length == 0)
                    b.Append('{');
                else
                    b.Append(", ");
                b.Append(element);
            }
            b.Append('}');
            return b.ToString();
        }

        internal string GetTreeAsString()
        {
#if DEBUG
            return Root.GetTreeAsString();
#else
            return "Not available in release build.";
#endif
        }
        #endregion

        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Finds the first occurance of item.
        /// Runs in O(N).
        /// </summary>
        /// <returns>The index of the first occurance of item, or -1 if it cannot be found.</returns>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public int IndexOf(T item)
        {
            return IndexOf(item, 0, Length);
        }

        /// <summary>
        /// Gets the index of the first occurrence the specified item.
        /// </summary>
        /// <param name="item">Item to search for.</param>
        /// <param name="startIndex">Start index of the search.</param>
        /// <param name="count">Length of the area to search.</param>
        /// <returns>The first index where the item was found; or -1 if no occurrence was found.</returns>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public int IndexOf(T item, int startIndex, int count)
        {
            VerifyRange(startIndex, count);

            while (count > 0)
            {
                var entry = FindNodeUsingCache(startIndex).PeekOrDefault();
                var contents = entry.Node.Contents;
                var startWithinNode = startIndex - entry.NodeStartIndex;
                var nodeLength = Math.Min(entry.Node.Length, startWithinNode + count);
                var r = Array.IndexOf(contents, item, startWithinNode, nodeLength - startWithinNode);
                if (r >= 0)
                    return entry.NodeStartIndex + r;
                count -= nodeLength - startWithinNode;
                startIndex = entry.NodeStartIndex + nodeLength;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the last occurrence of the specified item in this rope.
        /// </summary>
        public int LastIndexOf(T item)
        {
            return LastIndexOf(item, 0, Length);
        }

        /// <summary>
        /// Gets the index of the last occurrence of the specified item in this rope.
        /// </summary>
        /// <param name="item">The search item</param>
        /// <param name="startIndex">Start index of the area to search.</param>
        /// <param name="count">Length of the area to search.</param>
        /// <returns>The last index where the item was found; or -1 if no occurrence was found.</returns>
        /// <remarks>The search proceeds backwards from (startIndex+count) to startIndex.
        /// This is different than the meaning of the parameters on Array.LastIndexOf!</remarks>
        public int LastIndexOf(T item, int startIndex, int count)
        {
            VerifyRange(startIndex, count);

            var comparer = EqualityComparer<T>.Default;
            for (var i = startIndex + count - 1; i >= startIndex; i--)
            {
                if (comparer.Equals(this[i], item))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Inserts the item at the specified index in the rope.
        /// Runs in O(lg N).
        /// </summary>
        public void Insert(int index, T item)
        {
            InsertRange(index, new[] { item }, 0, 1);
        }

        /// <summary>
        /// Removes a single item from the rope.
        /// Runs in O(lg N).
        /// </summary>
        public void RemoveAt(int index)
        {
            RemoveRange(index, 1);
        }

        /// <summary>
        /// Appends the item at the end of the rope.
        /// Runs in O(lg N).
        /// </summary>
        public void Add(T item)
        {
            InsertRange(Length, new[] { item }, 0, 1);
        }

        /// <summary>
        /// Searches the item in the rope.
        /// Runs in O(N).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Copies the whole content of the rope into the specified array.
        /// Runs in O(N).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array.AsSpan(), arrayIndex);
        }

        /// <summary>
        /// Copies the whole content of the rope into the specified array.
        /// Runs in O(N).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public void CopyTo(Span<T> array, int arrayIndex)
        {
            CopyTo(0, array, arrayIndex, Length);
        }

        /// <summary>
        /// Copies the a part of the rope into the specified array.
        /// Runs in O(lg N + M).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public void CopyTo(int index, Span<T> array, int arrayIndex, int count)
        {
            VerifyRange(index, count);
            VerifyArrayWithRange(array, arrayIndex, count);
            Root.CopyTo(index, array, arrayIndex, count);
        }

        /// <summary>
        /// Removes the first occurance of an item from the rope.
        /// Runs in O(N).
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

        /// <summary>
        /// Retrieves an enumerator to iterate through the rope.
        /// The enumerator will reflect the state of the rope from the GetEnumerator() call, further modifications
        /// to the rope will not be visible to the enumerator.
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            Root.Publish();
            return Enumerate(Root);
        }

        /// <summary>
        /// Creates an array and copies the contents of the rope into it.
        /// Runs in O(N).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public T[] ToArray()
        {
            var arr = new T[Length];
            Root.CopyTo(0, arr, 0, arr.Length);
            return arr;
        }

        /// <summary>
        /// Creates an array and copies the contents of the rope into it.
        /// Runs in O(N).
        /// </summary>
        /// <remarks>
        /// This method counts as a read access and may be called concurrently to other read accesses.
        /// </remarks>
        public T[] ToArray(int startIndex, int count)
        {
            VerifyRange(startIndex, count);
            var arr = new T[count];
            CopyTo(startIndex, arr, 0, count);
            return arr;
        }

        private static IEnumerator<T> Enumerate(RopeNode<T> node)
        {
            var stack = new Stack<RopeNode<T>>();
            while (node != null)
            {
                // go to leftmost node, pushing the right parts that we'll have to visit later
                while (node.Contents == null)
                {
                    if (node.Height == 0)
                    {
                        // go down into function nodes
                        node = node.GetContentNode();
                        continue;
                    }
                    Debug.Assert(node.Right != null);
                    stack.Push(node.Right);
                    node = node.Left;
                }
                // yield contents of leaf node
                for (var i = 0; i < node.Length; i++)
                {
                    yield return node.Contents[i];
                }
                // go up to the next node not visited yet
                node = stack.Count > 0 ? stack.Pop() : null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
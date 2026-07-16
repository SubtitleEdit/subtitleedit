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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// A tree of TextAnchorNodes.
    /// </summary>
    internal sealed class TextAnchorTree
    {
        // The text anchor tree has difficult requirements:
        // - it must QUICKLY update the offset in all anchors whenever there is a document change
        // - it must not reference text anchors directly, using weak references instead

        // Clearly, we cannot afford updating an Offset property on all anchors (that would be O(N)).
        // So instead, the anchors need to be able to calculate their offset from a data structure
        // that can be efficiently updated.

        // This implementation is built using an augmented red-black-tree.
        // There is a 'TextAnchorNode' for each text anchor.
        // Such a node represents a section of text (just the length is stored) with a (weakly referenced) text anchor at the end.

        // Basically, you can imagine the list of text anchors as a sorted list of text anchors, where each anchor
        // just stores the distance to the previous anchor.
        // (next node = TextAnchorNode.Successor, distance = TextAnchorNode.length)
        // Distances are never negative, so this representation means anchors are always sorted by offset
        // (the order of anchors at the same offset is undefined)

        // Of course, a linked list of anchors would be way too slow (one would need to traverse the whole list
        // every time the offset of an anchor is being looked up).
        // Instead, we use a red-black-tree. We aren't actually using the tree for sorting - it's just a binary tree
        // as storage format for what's conceptually a list, the red-black properties are used to keep the tree balanced.
        // Other balanced binary trees would work, too.

        // What makes the tree-form efficient is that is augments the data by a 'totalLength'. Where 'length'
        // represents the distance to the previous node, 'totalLength' is the sum of all 'length' values in the subtree
        // under that node.
        // This allows computing the Offset from an anchor by walking up the list of parent nodes instead of going
        // through all predecessor nodes. So computing the Offset runs in O(log N).

        private readonly TextDocument _document;
        private readonly List<TextAnchorNode> _nodesToDelete = new List<TextAnchorNode>();
        private TextAnchorNode _root;

        public TextAnchorTree(TextDocument document)
        {
            _document = document;
        }

        [Conditional("DEBUG")]
        private static void Log(string text)
        {
            Debug.WriteLine("TextAnchorTree: " + text);
        }

        #region Insert Text

        private void InsertText(int offset, int length, bool defaultAnchorMovementIsBeforeInsertion)
        {
            if (length == 0 || _root == null || offset > _root.TotalLength)
                return;

            // find the range of nodes that are placed exactly at offset
            // beginNode is inclusive, endNode is exclusive
            if (offset == _root.TotalLength)
            {
                PerformInsertText(FindActualBeginNode(_root.RightMost), null, length, defaultAnchorMovementIsBeforeInsertion);
            }
            else
            {
                TextAnchorNode endNode = FindNode(ref offset);
                Debug.Assert(endNode.Length > 0);

                if (offset > 0)
                {
                    // there are no nodes exactly at offset
                    endNode.Length += length;
                    UpdateAugmentedData(endNode);
                }
                else
                {
                    PerformInsertText(FindActualBeginNode(endNode.Predecessor), endNode, length, defaultAnchorMovementIsBeforeInsertion);
                }
            }
            DeleteMarkedNodes();
        }

        private TextAnchorNode FindActualBeginNode(TextAnchorNode node)
        {
            // now find the actual beginNode
            while (node != null && node.Length == 0)
                node = node.Predecessor;
            // no predecessor = beginNode is first node in tree
            return node ?? _root.LeftMost;
        }

        // Sorts the nodes in the range [beginNode, endNode) by MovementType
        // and inserts the length between the BeforeInsertion and the AfterInsertion nodes.
        private void PerformInsertText(TextAnchorNode beginNode, TextAnchorNode endNode, int length, bool defaultAnchorMovementIsBeforeInsertion)
        {
            Debug.Assert(beginNode != null);
            // endNode may be null at the end of the anchor tree

            // now we need to sort the nodes in the range [beginNode, endNode); putting those with
            // MovementType.BeforeInsertion in front of those with MovementType.AfterInsertion
            List<TextAnchorNode> beforeInsert = new List<TextAnchorNode>();
            //List<TextAnchorNode> afterInsert = new List<TextAnchorNode>();
            TextAnchorNode temp = beginNode;
            while (temp != endNode)
            {
                TextAnchor anchor = (TextAnchor)temp.Target;
                if (anchor == null)
                {
                    // afterInsert.Add(temp);
                    MarkNodeForDelete(temp);
                }
                else if (defaultAnchorMovementIsBeforeInsertion
                         ? anchor.MovementType != AnchorMovementType.AfterInsertion
                         : anchor.MovementType == AnchorMovementType.BeforeInsertion)
                {
                    beforeInsert.Add(temp);
                    //				} else {
                    //					afterInsert.Add(temp);
                }
                temp = temp.Successor;
            }
            // now again go through the range and swap the nodes with those in the beforeInsert list
            temp = beginNode;
            foreach (TextAnchorNode node in beforeInsert)
            {
                SwapAnchors(node, temp);
                temp = temp.Successor;
            }
            // now temp is pointing to the first node that is afterInsert,
            // or to endNode, if there is no afterInsert node at the offset
            // So add the length to temp
            if (temp == null)
            {
                // temp might be null if endNode==null and no afterInserts
                Debug.Assert(endNode == null);
            }
            else
            {
                temp.Length += length;
                UpdateAugmentedData(temp);
            }
        }

        /// <summary>
        /// Swaps the anchors stored in the two nodes.
        /// </summary>
        private void SwapAnchors(TextAnchorNode n1, TextAnchorNode n2)
        {
            if (n1 != n2)
            {
                TextAnchor anchor1 = (TextAnchor)n1.Target;
                TextAnchor anchor2 = (TextAnchor)n2.Target;
                if (anchor1 == null && anchor2 == null)
                {
                    // -> no swap required
                    return;
                }
                n1.Target = anchor2;
                n2.Target = anchor1;
                if (anchor1 == null)
                {
                    // unmark n1 from deletion, mark n2 for deletion
                    _nodesToDelete.Remove(n1);
                    MarkNodeForDelete(n2);
                    anchor2.Node = n1;
                }
                else if (anchor2 == null)
                {
                    // unmark n2 from deletion, mark n1 for deletion
                    _nodesToDelete.Remove(n2);
                    MarkNodeForDelete(n1);
                    anchor1.Node = n2;
                }
                else
                {
                    anchor1.Node = n2;
                    anchor2.Node = n1;
                }
            }
        }
        #endregion

        #region Remove or Replace text
        public void HandleTextChange(OffsetChangeMapEntry entry, DelayedEvents delayedEvents)
        {
            //Log("HandleTextChange(" + entry + ")");
            if (entry.RemovalLength == 0)
            {
                // This is a pure insertion.
                // Unlike a replace with removal, a pure insertion can result in nodes at the same location
                // to split depending on their MovementType.
                // Thus, we handle this case on a separate code path
                // (the code below looks like it does something similar, but it can only split
                // the set of deletion survivors, not all nodes at an offset)
                InsertText(entry.Offset, entry.InsertionLength, entry.DefaultAnchorMovementIsBeforeInsertion);
                return;
            }
            // When handling a replacing text change, we need to:
            // - find all anchors in the deleted segment and delete them / move them to the appropriate
            //   surviving side.
            // - adjust the segment size between the left and right side

            int offset = entry.Offset;
            int remainingRemovalLength = entry.RemovalLength;
            // if the text change is happening after the last anchor, we don't have to do anything
            if (_root == null || offset >= _root.TotalLength)
                return;
            TextAnchorNode node = FindNode(ref offset);
            TextAnchorNode firstDeletionSurvivor = null;
            // go forward through the tree and delete all nodes in the removal segment
            while (node != null && offset + remainingRemovalLength > node.Length)
            {
                TextAnchor anchor = (TextAnchor)node.Target;
                if (anchor != null && (anchor.SurviveDeletion || entry.RemovalNeverCausesAnchorDeletion))
                {
                    if (firstDeletionSurvivor == null)
                        firstDeletionSurvivor = node;
                    // This node should be deleted, but it wants to survive.
                    // We'll just remove the deleted length segment, so the node will be positioned
                    // in front of the removed segment.
                    remainingRemovalLength -= node.Length - offset;
                    node.Length = offset;
                    offset = 0;
                    UpdateAugmentedData(node);
                    node = node.Successor;
                }
                else
                {
                    // delete node
                    TextAnchorNode s = node.Successor;
                    remainingRemovalLength -= node.Length;
                    RemoveNode(node);
                    // we already deleted the node, don't delete it twice
                    _nodesToDelete.Remove(node);
                    anchor?.OnDeleted(delayedEvents);
                    node = s;
                }
            }
            // 'node' now is the first anchor after the deleted segment.
            // If there are no anchors after the deleted segment, 'node' is null.

            // firstDeletionSurvivor was set to the first node surviving deletion.
            // Because all non-surviving nodes up to 'node' were deleted, the node range
            // [firstDeletionSurvivor, node) now refers to the set of all deletion survivors.

            // do the remaining job of the removal
            if (node != null)
            {
                node.Length -= remainingRemovalLength;
                Debug.Assert(node.Length >= 0);
            }
            if (entry.InsertionLength > 0)
            {
                // we are performing a replacement
                if (firstDeletionSurvivor != null)
                {
                    // We got deletion survivors which need to be split into BeforeInsertion
                    // and AfterInsertion groups.
                    // Take care that we don't regroup everything at offset, but only the deletion
                    // survivors - from firstDeletionSurvivor (inclusive) to node (exclusive).
                    // This ensures that nodes immediately before or after the replaced segment
                    // stay where they are (independent from their MovementType)
                    PerformInsertText(firstDeletionSurvivor, node, entry.InsertionLength, entry.DefaultAnchorMovementIsBeforeInsertion);
                }
                else if (node != null)
                {
                    // No deletion survivors:
                    // just perform the insertion
                    node.Length += entry.InsertionLength;
                }
            }
            if (node != null)
            {
                UpdateAugmentedData(node);
            }
            DeleteMarkedNodes();
        }
        #endregion

        #region Node removal when TextAnchor was GC'ed

        private void MarkNodeForDelete(TextAnchorNode node)
        {
            if (!_nodesToDelete.Contains(node))
                _nodesToDelete.Add(node);
        }

        private void DeleteMarkedNodes()
        {
            CheckProperties();
            while (_nodesToDelete.Count > 0)
            {
                int pos = _nodesToDelete.Count - 1;
                TextAnchorNode n = _nodesToDelete[pos];
                // combine section of n with the following section
                TextAnchorNode s = n.Successor;
                if (s != null)
                {
                    s.Length += n.Length;
                }
                RemoveNode(n);
                if (s != null)
                {
                    UpdateAugmentedData(s);
                }
                _nodesToDelete.RemoveAt(pos);
                CheckProperties();
            }
            CheckProperties();
        }
        #endregion

        #region FindNode
        /// <summary>
        /// Finds the node at the specified offset.
        /// After the method has run, offset is relative to the beginning of the returned node.
        /// </summary>
        private TextAnchorNode FindNode(ref int offset)
        {
            TextAnchorNode n = _root;
            while (true)
            {
                if (n.Left != null)
                {
                    if (offset < n.Left.TotalLength)
                    {
                        n = n.Left; // descend into left subtree
                        continue;
                    }
                    offset -= n.Left.TotalLength; // skip left subtree
                }
                if (!n.IsAlive)
                    MarkNodeForDelete(n);
                if (offset < n.Length)
                {
                    return n; // found correct node
                }
                offset -= n.Length; // skip this node
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

        #region UpdateAugmentedData

        private void UpdateAugmentedData(TextAnchorNode n)
        {
            if (!n.IsAlive)
                MarkNodeForDelete(n);

            int totalLength = n.Length;
            if (n.Left != null)
                totalLength += n.Left.TotalLength;
            if (n.Right != null)
                totalLength += n.Right.TotalLength;
            if (n.TotalLength != totalLength)
            {
                n.TotalLength = totalLength;
                if (n.Parent != null)
                    UpdateAugmentedData(n.Parent);
            }
        }
        #endregion

        #region CreateAnchor
        public TextAnchor CreateAnchor(int offset)
        {
            Log("CreateAnchor(" + offset + ")");
            TextAnchor anchor = new TextAnchor(_document);
            anchor.Node = new TextAnchorNode(anchor);
            if (_root == null)
            {
                // creating the first text anchor
                _root = anchor.Node;
                _root.TotalLength = _root.Length = offset;
            }
            else if (offset >= _root.TotalLength)
            {
                // append anchor at end of tree
                anchor.Node.TotalLength = anchor.Node.Length = offset - _root.TotalLength;
                InsertAsRight(_root.RightMost, anchor.Node);
            }
            else
            {
                // insert anchor in middle of tree
                TextAnchorNode n = FindNode(ref offset);
                Debug.Assert(offset < n.Length);
                // split segment 'n' at offset
                anchor.Node.TotalLength = anchor.Node.Length = offset;
                n.Length -= offset;
                InsertBefore(n, anchor.Node);
            }
            DeleteMarkedNodes();
            return anchor;
        }

        private void InsertBefore(TextAnchorNode node, TextAnchorNode newNode)
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

        #region Red/Black Tree
        internal const bool Red = true;
        internal const bool Black = false;

        private void InsertAsLeft(TextAnchorNode parentNode, TextAnchorNode newNode)
        {
            Debug.Assert(parentNode.Left == null);
            parentNode.Left = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAugmentedData(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void InsertAsRight(TextAnchorNode parentNode, TextAnchorNode newNode)
        {
            Debug.Assert(parentNode.Right == null);
            parentNode.Right = newNode;
            newNode.Parent = parentNode;
            newNode.Color = Red;
            UpdateAugmentedData(parentNode);
            FixTreeOnInsert(newNode);
        }

        private void FixTreeOnInsert(TextAnchorNode node)
        {
            Debug.Assert(node != null);
            Debug.Assert(node.Color == Red);
            Debug.Assert(node.Left == null || node.Left.Color == Black);
            Debug.Assert(node.Right == null || node.Right.Color == Black);

            TextAnchorNode parentNode = node.Parent;
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
            TextAnchorNode grandparentNode = parentNode.Parent;
            TextAnchorNode uncleNode = Sibling(parentNode);
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

        private void RemoveNode(TextAnchorNode removedNode)
        {
            if (removedNode.Left != null && removedNode.Right != null)
            {
                // replace removedNode with it's in-order successor

                TextAnchorNode leftMost = removedNode.Right.LeftMost;
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
            TextAnchorNode parentNode = removedNode.Parent;
            TextAnchorNode childNode = removedNode.Left ?? removedNode.Right;
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

        private void FixTreeOnDelete(TextAnchorNode node, TextAnchorNode parentNode)
        {
            Debug.Assert(node == null || node.Parent == parentNode);
            if (parentNode == null)
                return;

            // warning: node may be null
            TextAnchorNode sibling = Sibling(node, parentNode);
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

        private void ReplaceNode(TextAnchorNode replacedNode, TextAnchorNode newNode)
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

        private void RotateLeft(TextAnchorNode p)
        {
            // let q be p's right child
            TextAnchorNode q = p.Right;
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

        private void RotateRight(TextAnchorNode p)
        {
            // let q be p's left child
            TextAnchorNode q = p.Left;
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

        private static TextAnchorNode Sibling(TextAnchorNode node)
        {
            if (node == node.Parent.Left)
                return node.Parent.Right;
            return node.Parent.Left;
        }

        private static TextAnchorNode Sibling(TextAnchorNode node, TextAnchorNode parentNode)
        {
            Debug.Assert(node == null || node.Parent == parentNode);
            if (node == parentNode.Left)
                return parentNode.Right;
            return parentNode.Left;
        }

        private static bool GetColor(TextAnchorNode node)
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
                int blackCount = -1;
                CheckNodeProperties(_root, null, Red, 0, ref blackCount);
            }
#endif
        }

#if DEBUG

        private void CheckProperties(TextAnchorNode node)
        {
            int totalLength = node.Length;
            if (node.Left != null)
            {
                CheckProperties(node.Left);
                totalLength += node.Left.TotalLength;
            }
            if (node.Right != null)
            {
                CheckProperties(node.Right);
                totalLength += node.Right.TotalLength;
            }
            Debug.Assert(node.TotalLength == totalLength);
        }

        /*
		1. A node is either red or black.
		2. The root is black.
		3. All leaves are black. (The leaves are the NIL children.)
		4. Both children of every red node are black. (So every red node must have a black parent.)
		5. Every simple path from a node to a descendant leaf contains the same number of black nodes. (Not counting the leaf node.)
		 */
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void CheckNodeProperties(TextAnchorNode node, TextAnchorNode parentNode, bool parentColor, int blackCount, ref int expectedBlackCount)
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
#if DEBUG
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string GetTreeAsString()
        {
            if (_root == null)
                return "<empty tree>";
            StringBuilder b = new StringBuilder();
            AppendTreeToString(_root, b, 0);
            return b.ToString();
        }

        private static void AppendTreeToString(TextAnchorNode node, StringBuilder b, int indent)
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

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

namespace AvaloniaEdit.Document
{
    // A tree node in the document line tree.
    // For the purpose of the invariants, "children", "descendents", "siblings" etc. include the DocumentLine object,
    // it is treated as a third child node between left and right.
    partial class DocumentLine
    {
        internal DocumentLine Left { get; set; }
        internal DocumentLine Right { get; set; }
        internal DocumentLine Parent { get; set; }

        internal bool Color { get; set; }

        // optimization note: I tried packing color and isDeleted into a single byte field, but that
        // actually increased the memory requirements. The JIT packs two bools and a byte (delimiterSize)
        // into a single DWORD, but two bytes get each their own DWORD. Three bytes end up in the same DWORD, so
        // apparently the JIT only optimizes for memory when there are at least three small fields.
        // Currently, DocumentLine takes 36 bytes on x86 (8 byte object overhead, 3 pointers, 3 ints, and another DWORD
        // for the small fields).
        // TODO: a possible optimization would be to combine 'totalLength' and the small fields into a single uint.
        // delimiterSize takes only two bits, the two bools take another two bits; so there's still 
        // 28 bits left for totalLength. 268435455 characters per line should be enough for everyone :)

        /// <summary>
        /// Resets the line to enable its reuse after a document rebuild.
        /// </summary>
        internal void ResetLine()
        {
            _totalLength = _delimiterLength = 0;
            _isDeleted = Color = false;
            Left = Right = Parent = null;
        }

        internal DocumentLine InitLineNode()
        {
            NodeTotalCount = 1;
            NodeTotalLength = TotalLength;
            return this;
        }

        internal DocumentLine LeftMost
        {
            get
            {
                DocumentLine node = this;
                while (node.Left != null)
                    node = node.Left;
                return node;
            }
        }

        internal DocumentLine RightMost
        {
            get
            {
                DocumentLine node = this;
                while (node.Right != null)
                    node = node.Right;
                return node;
            }
        }

        /// <summary>
        /// The number of lines in this node and its child nodes.
        /// Invariant:
        ///   nodeTotalCount = 1 + left.nodeTotalCount + right.nodeTotalCount
        /// </summary>
        internal int NodeTotalCount { get; set; }

        /// <summary>
        /// The total text length of this node and its child nodes.
        /// Invariant:
        ///   nodeTotalLength = left.nodeTotalLength + documentLine.TotalLength + right.nodeTotalLength
        /// </summary>
        internal int NodeTotalLength { get; set; }
    }
}

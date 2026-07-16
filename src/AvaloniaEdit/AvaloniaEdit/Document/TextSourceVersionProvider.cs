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
using System.Linq;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// Provides ITextSourceVersion instances.
    /// </summary>
    public class TextSourceVersionProvider
    {
        private Version _currentVersion;

        /// <summary>
        /// Creates a new TextSourceVersionProvider instance.
        /// </summary>
        public TextSourceVersionProvider()
        {
            _currentVersion = new Version(this);
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        public ITextSourceVersion CurrentVersion => _currentVersion;

        /// <summary>
        /// Replaces the current version with a new version.
        /// </summary>
        /// <param name="change">Change from current version to new version</param>
        public void AppendChange(TextChangeEventArgs change)
        {
            _currentVersion.Change = change ?? throw new ArgumentNullException(nameof(change));
            _currentVersion.Next = new Version(_currentVersion);
            _currentVersion = _currentVersion.Next;
        }

        [DebuggerDisplay("Version #{" + nameof(_id) + "}")]
        private sealed class Version : ITextSourceVersion
        {
            // Reference back to the provider.
            // Used to determine if two checkpoints belong to the same document.
            private readonly TextSourceVersionProvider _provider;
            // ID used for CompareAge()
            private readonly int _id;

            // the change from this version to the next version
            internal TextChangeEventArgs Change;
            internal Version Next;

            internal Version(TextSourceVersionProvider provider)
            {
                _provider = provider;
            }

            internal Version(Version prev)
            {
                _provider = prev._provider;
                _id = unchecked(prev._id + 1);
            }

            public bool BelongsToSameDocumentAs(ITextSourceVersion other)
            {
                var o = other as Version;
                return o != null && _provider == o._provider;
            }

            public int CompareAge(ITextSourceVersion other)
            {
                if (other == null)
                    throw new ArgumentNullException(nameof(other));
                var o = other as Version;
                if (o == null || _provider != o._provider)
                    throw new ArgumentException("Versions do not belong to the same document.");
                // We will allow overflows, but assume that the maximum distance between checkpoints is 2^31-1.
                // This is guaranteed on x86 because so many checkpoints don't fit into memory.
                return Math.Sign(unchecked(_id - o._id));
            }

            public IEnumerable<TextChangeEventArgs> GetChangesTo(ITextSourceVersion other)
            {
                var result = CompareAge(other);
                var o = (Version)other;
                if (result < 0)
                    return GetForwardChanges(o);
                if (result > 0)
                    return o.GetForwardChanges(this).Reverse().Select(c => c.Invert());
                return Array.Empty<TextChangeEventArgs>();
            }

            private IEnumerable<TextChangeEventArgs> GetForwardChanges(Version other)
            {
                // Return changes from this(inclusive) to other(exclusive).
                for (var node = this; node != other; node = node.Next)
                {
                    yield return node.Change;
                }
            }

            public int MoveOffsetTo(ITextSourceVersion other, int oldOffset, AnchorMovementType movement)
            {
                return GetChangesTo(other)
                    .Aggregate(oldOffset, (current, e) => e.GetNewOffset(current, movement));
            }
        }
    }
}

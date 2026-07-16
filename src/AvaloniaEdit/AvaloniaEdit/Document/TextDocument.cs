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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using AvaloniaEdit.Utils;
using System.Threading;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// This class is the main class of the text model. Basically, it is a <see cref="System.Text.StringBuilder"/> with events.
    /// </summary>
    /// <remarks>
    /// <b>Thread safety:</b>
    /// <inheritdoc cref="VerifyAccess"/>
    /// <para>However, there is a single method that is thread-safe: <see cref="CreateSnapshot()"/> (and its overloads).</para>
    /// </remarks>
    public sealed class TextDocument : IDocument, INotifyPropertyChanged
    {
        #region Thread ownership

        private readonly object _lockObject = new object();
        #endregion

        #region Fields + Constructor

        private readonly Rope<char> _rope;
        private readonly DocumentLineTree _lineTree;
        private readonly LineManager _lineManager;
        private readonly TextAnchorTree _anchorTree;
        private readonly TextSourceVersionProvider _versionProvider = new TextSourceVersionProvider();

        /// <summary>
        /// Create an empty text document.
        /// </summary>
        public TextDocument()
            : this(string.Empty.ToCharArray())
        {
        }

        /// <summary>
        /// Create a new text document with the specified initial text.
        /// </summary>
        public TextDocument(IEnumerable<char> initialText)
        {
            if (initialText == null)
                throw new ArgumentNullException(nameof(initialText));
            _rope = new Rope<char>(initialText);
            _lineTree = new DocumentLineTree(this);
            _lineManager = new LineManager(_lineTree, this);
            _lineTrackers.CollectionChanged += delegate
            {
                _lineManager.UpdateListOfLineTrackers();
            };

            _anchorTree = new TextAnchorTree(this);
            _undoStack = new UndoStack();
            FireChangeEvents();
        }

        /// <summary>
        /// Create a new text document with the specified initial text.
        /// </summary>
        public TextDocument(ITextSource initialText)
            : this(GetTextFromTextSource(initialText))
        {
        }

        // gets the text from a text source, directly retrieving the underlying rope where possible
        private static IEnumerable<char> GetTextFromTextSource(ITextSource textSource)
        {
            if (textSource == null)
                throw new ArgumentNullException(nameof(textSource));

            if (textSource is RopeTextSource rts)
            {
                return rts.GetRope();
            }

            if (textSource is TextDocument doc)
            {
                return doc._rope;
            }

            return textSource.Text.ToCharArray();
        }
        #endregion

        #region Text

        private void ThrowIfRangeInvalid(int offset, int length)
        {
            if (offset < 0 || offset > _rope.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "0 <= offset <= " + _rope.Length.ToString(CultureInfo.InvariantCulture));
            }
            if (length < 0 || offset + length > _rope.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "0 <= length, offset(" + offset + ")+length <= " + _rope.Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <inheritdoc/>
        public string GetText(int offset, int length)
        {
            VerifyAccess();
            return _rope.ToString(offset, length);
        }

        /// <inheritdoc/>
        public ReadOnlyMemory<char> GetTextAsMemory(int offset, int length)
        {
            VerifyAccess();
            return _rope.GetMemory(offset, length);
        }

        private Thread ownerThread = Thread.CurrentThread;

		/// <summary>
		/// Transfers ownership of the document to another thread. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// The owner can be set to null, which means that no thread can access the document. But, if the document
		/// has no owner thread, any thread may take ownership by calling <see cref="SetOwnerThread"/>.
		/// </para>
		/// </remarks>
		public void SetOwnerThread(Thread newOwner)
		{
            // We need to lock here to ensure that in the null owner case,
			// only one thread succeeds in taking ownership.
			lock (_lockObject) {
				if (ownerThread != null) {
					VerifyAccess();
				}
				ownerThread = newOwner;
			}
		}	

        private void VerifyAccess()
        {
            if(Thread.CurrentThread != ownerThread)
            {
                throw new InvalidOperationException("Call from invalid thread.");
            }
        }

        /// <summary>
        /// Retrieves the text for a portion of the document.
        /// </summary>
        public string GetText(ISegment segment)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));
            return GetText(segment.Offset, segment.Length);
        }

        /// <inheritdoc/>
        public int IndexOf(char c, int startIndex, int count)
        {
            DebugVerifyAccess();
            return _rope.IndexOf(c, startIndex, count);
        }

        /// <inheritdoc/>
        public int LastIndexOf(char c, int startIndex, int count)
        {
            DebugVerifyAccess();
            return _rope.LastIndexOf(c, startIndex, count);
        }

        /// <inheritdoc/>
        public int IndexOfAny(char[] anyOf, int startIndex, int count)
        {
            DebugVerifyAccess(); // frequently called (NewLineFinder), so must be fast in release builds
            return _rope.IndexOfAny(anyOf, startIndex, count);
        }

        /// <inheritdoc/>
        public int IndexOf(string searchText, int startIndex, int count, StringComparison comparisonType)
        {
            DebugVerifyAccess();
            return _rope.IndexOf(searchText, startIndex, count, comparisonType);
        }

        /// <inheritdoc/>
        public int LastIndexOf(string searchText, int startIndex, int count, StringComparison comparisonType)
        {
            DebugVerifyAccess();
            return _rope.LastIndexOf(searchText, startIndex, count, comparisonType);
        }

        /// <inheritdoc/>
        public char GetCharAt(int offset)
        {
            DebugVerifyAccess(); // frequently called, so must be fast in release builds
            return _rope[offset];
        }

        private WeakReference _cachedText;

        /// <summary>
        /// Gets/Sets the text of the whole document.
        /// </summary>
        public string Text
        {
            get
            {
                VerifyAccess();
                var completeText = _cachedText?.Target as string;
                if (completeText == null)
                {
                    completeText = _rope.ToString();
                    _cachedText = new WeakReference(completeText);
                }
                return completeText;
            }
            set
            {
                VerifyAccess();
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                Replace(0, _rope.Length, value);
            }
        }

        /// <summary>
        /// This event is called after a group of changes is completed.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public event EventHandler TextChanged;

        event EventHandler IDocument.ChangeCompleted
        {
            add => TextChanged += value;
            remove => TextChanged -= value;
        }

        /// <inheritdoc/>
        public int TextLength
        {
            get
            {
                VerifyAccess();
                return _rope.Length;
            }
        }

        /// <summary>
        /// Is raised when the TextLength property changes.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public event EventHandler TextLengthChanged;

        /// <summary>
        /// Is raised before the document changes.
        /// </summary>
        /// <remarks>
        /// <para>Here is the order in which events are raised during a document update:</para>
        /// <list type="bullet">
        /// <item><description><b><see cref="BeginUpdate">BeginUpdate()</see></b></description>
        ///   <list type="bullet">
        ///   <item><description>Start of change group (on undo stack)</description></item>
        ///   <item><description><see cref="UpdateStarted"/> event is raised</description></item>
        ///   </list></item>
        /// <item><description><b><see cref="Insert(int,string)">Insert()</see> / <see cref="Remove(int,int)">Remove()</see> / <see cref="Replace(int,int,string)">Replace()</see></b></description>
        ///   <list type="bullet">
        ///   <item><description><see cref="Changing"/> event is raised</description></item>
        ///   <item><description>The document is changed</description></item>
        ///   <item><description><see cref="TextAnchor.Deleted">TextAnchor.Deleted</see> event is raised if anchors were
        ///     in the deleted text portion</description></item>
        ///   <item><description><see cref="Changed"/> event is raised</description></item>
        ///   </list></item>
        /// <item><description><b><see cref="EndUpdate">EndUpdate()</see></b></description>
        ///   <list type="bullet">
        ///   <item><description><see cref="TextChanged"/> event is raised</description></item>
        ///   <item><description><see cref="PropertyChanged"/> event is raised (for the Text, TextLength, LineCount properties, in that order)</description></item>
        ///   <item><description>End of change group (on undo stack)</description></item>
        ///   <item><description><see cref="UpdateFinished"/> event is raised</description></item>
        ///   </list></item>
        /// </list>
        /// <para>
        /// If the insert/remove/replace methods are called without a call to <c>BeginUpdate()</c>,
        /// they will call <c>BeginUpdate()</c> and <c>EndUpdate()</c> to ensure no change happens outside of <c>UpdateStarted</c>/<c>UpdateFinished</c>.
        /// </para><para>
        /// There can be multiple document changes between the <c>BeginUpdate()</c> and <c>EndUpdate()</c> calls.
        /// In this case, the events associated with EndUpdate will be raised only once after the whole document update is done.
        /// </para><para>
        /// The <see cref="UndoStack"/> listens to the <c>UpdateStarted</c> and <c>UpdateFinished</c> events to group all changes into a single undo step.
        /// </para>
        /// </remarks>
        public event EventHandler<DocumentChangeEventArgs> Changing;

        // Unfortunately EventHandler<T> is invariant, so we have to use two separate events
        private event EventHandler<TextChangeEventArgs> TextChangingInternal;

        event EventHandler<TextChangeEventArgs> IDocument.TextChanging
        {
            add => TextChangingInternal += value;
            remove => TextChangingInternal -= value;
        }

        /// <summary>
        /// Is raised after the document has changed.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public event EventHandler<DocumentChangeEventArgs> Changed;

        private event EventHandler<TextChangeEventArgs> TextChangedInternal;

        event EventHandler<TextChangeEventArgs> IDocument.TextChanged
        {
            add => TextChangedInternal += value;
            remove => TextChangedInternal -= value;
        }

        /// <summary>
        /// Creates a snapshot of the current text.
        /// </summary>
        /// <remarks>
        /// <para>This method returns an immutable snapshot of the document, and may be safely called even when
        /// the document's owner thread is concurrently modifying the document.
        /// </para><para>
        /// This special thread-safety guarantee is valid only for TextDocument.CreateSnapshot(), not necessarily for other
        /// classes implementing ITextSource.CreateSnapshot().
        /// </para><para>
        /// </para>
        /// </remarks>
        public ITextSource CreateSnapshot()
        {
            lock (_lockObject)
            {
                return new RopeTextSource(_rope, _versionProvider.CurrentVersion);
            }
        }

        /// <summary>
        /// Creates a snapshot of a part of the current text.
        /// </summary>
        /// <remarks><inheritdoc cref="CreateSnapshot()"/></remarks>
        public ITextSource CreateSnapshot(int offset, int length)
        {
            lock (_lockObject)
            {
                return new RopeTextSource(_rope.GetRange(offset, length));
            }
        }

        /// <inheritdoc/>
        public ITextSourceVersion Version => _versionProvider.CurrentVersion;

        /// <inheritdoc/>
        public TextReader CreateReader()
        {
            lock (_lockObject)
            {
                return new RopeTextReader(_rope);
            }
        }

        /// <inheritdoc/>
        public TextReader CreateReader(int offset, int length)
        {
            lock (_lockObject)
            {
                return new RopeTextReader(_rope.GetRange(offset, length));
            }
        }

        /// <inheritdoc/>
        public void WriteTextTo(TextWriter writer)
        {
            VerifyAccess();
            _rope.WriteTo(writer, 0, _rope.Length);
        }

        /// <inheritdoc/>
        public void WriteTextTo(TextWriter writer, int offset, int length)
        {
            VerifyAccess();
            _rope.WriteTo(writer, offset, length);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region BeginUpdate / EndUpdate

        private int _beginUpdateCount;

        /// <summary>
        /// Gets if an update is running.
        /// </summary>
        /// <remarks><inheritdoc cref="BeginUpdate"/></remarks>
        public bool IsInUpdate
        {
            get
            {
                VerifyAccess();
                return _beginUpdateCount > 0;
            }
        }

        /// <summary>
        /// Immediately calls <see cref="BeginUpdate()"/>,
        /// and returns an IDisposable that calls <see cref="EndUpdate()"/>.
        /// </summary>
        /// <remarks><inheritdoc cref="BeginUpdate"/></remarks>
        public IDisposable RunUpdate()
        {
            BeginUpdate();
            return new CallbackOnDispose(EndUpdate);
        }

        /// <summary>
        /// <para>Begins a group of document changes.</para>
        /// <para>Some events are suspended until EndUpdate is called, and the <see cref="UndoStack"/> will
        /// group all changes into a single action.</para>
        /// <para>Calling BeginUpdate several times increments a counter, only after the appropriate number
        /// of EndUpdate calls the events resume their work.</para>
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public void BeginUpdate()
        {
            VerifyAccess();
            if (InDocumentChanging)
                throw new InvalidOperationException("Cannot change document within another document change.");
            _beginUpdateCount++;
            if (_beginUpdateCount == 1)
            {
                _undoStack.StartUndoGroup();
                UpdateStarted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Ends a group of document changes.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public void EndUpdate()
        {
            VerifyAccess();
            if (InDocumentChanging)
                throw new InvalidOperationException("Cannot end update within document change.");
            if (_beginUpdateCount == 0)
                throw new InvalidOperationException("No update is active.");
            if (_beginUpdateCount == 1)
            {
                // fire change events inside the change group - event handlers might add additional
                // document changes to the change group
                FireChangeEvents();
                _undoStack.EndUndoGroup();
                _beginUpdateCount = 0;
                UpdateFinished?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _beginUpdateCount -= 1;
            }
        }

        /// <summary>
        /// Occurs when a document change starts.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public event EventHandler UpdateStarted;

        /// <summary>
        /// Occurs when a document change is finished.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        public event EventHandler UpdateFinished;

        void IDocument.StartUndoableAction()
        {
            BeginUpdate();
        }

        void IDocument.EndUndoableAction()
        {
            EndUpdate();
        }

        IDisposable IDocument.OpenUndoGroup()
        {
            return RunUpdate();
        }
        #endregion

        #region Fire events after update

        private int _oldTextLength;
        private int _oldLineCount;
        private bool _fireTextChanged;

        /// <summary>
        /// Fires TextChanged, TextLengthChanged, LineCountChanged if required.
        /// </summary>
        internal void FireChangeEvents()
        {
            // it may be necessary to fire the event multiple times if the document is changed
            // from inside the event handlers
            while (_fireTextChanged)
            {
                _fireTextChanged = false;
                TextChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged("Text");

                var textLength = _rope.Length;
                if (textLength != _oldTextLength)
                {
                    _oldTextLength = textLength;
                    TextLengthChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged("TextLength");
                }
                var lineCount = _lineTree.LineCount;
                if (lineCount != _oldLineCount)
                {
                    _oldLineCount = lineCount;
                    LineCountChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged("LineCount");
                }
            }
        }

        #endregion

        #region Insert / Remove  / Replace
        /// <summary>
        /// Inserts text.
        /// </summary>
        /// <param name="offset">The offset at which the text is inserted.</param>
        /// <param name="text">The new text.</param>
        /// <remarks>
        /// Anchors positioned exactly at the insertion offset will move according to their movement type.
        /// For AnchorMovementType.Default, they will move behind the inserted text.
        /// The caret will also move behind the inserted text.
        /// </remarks>
        public void Insert(int offset, string text)
        {
            Replace(offset, 0, new StringTextSource(text), null);
        }

        /// <summary>
        /// Inserts text.
        /// </summary>
        /// <param name="offset">The offset at which the text is inserted.</param>
        /// <param name="text">The new text.</param>
        /// <remarks>
        /// Anchors positioned exactly at the insertion offset will move according to their movement type.
        /// For AnchorMovementType.Default, they will move behind the inserted text.
        /// The caret will also move behind the inserted text.
        /// </remarks>
        public void Insert(int offset, ITextSource text)
        {
            Replace(offset, 0, text, null);
        }

        /// <summary>
        /// Inserts text.
        /// </summary>
        /// <param name="offset">The offset at which the text is inserted.</param>
        /// <param name="text">The new text.</param>
        /// <param name="defaultAnchorMovementType">
        /// Anchors positioned exactly at the insertion offset will move according to the anchor's movement type.
        /// For AnchorMovementType.Default, they will move according to the movement type specified by this parameter.
        /// The caret will also move according to the <paramref name="defaultAnchorMovementType"/> parameter.
        /// </param>
        public void Insert(int offset, string text, AnchorMovementType defaultAnchorMovementType)
        {
            if (defaultAnchorMovementType == AnchorMovementType.BeforeInsertion)
            {
                Replace(offset, 0, new StringTextSource(text), OffsetChangeMappingType.KeepAnchorBeforeInsertion);
            }
            else
            {
                Replace(offset, 0, new StringTextSource(text), null);
            }
        }

        /// <summary>
        /// Inserts text.
        /// </summary>
        /// <param name="offset">The offset at which the text is inserted.</param>
        /// <param name="text">The new text.</param>
        /// <param name="defaultAnchorMovementType">
        /// Anchors positioned exactly at the insertion offset will move according to the anchor's movement type.
        /// For AnchorMovementType.Default, they will move according to the movement type specified by this parameter.
        /// The caret will also move according to the <paramref name="defaultAnchorMovementType"/> parameter.
        /// </param>
        public void Insert(int offset, ITextSource text, AnchorMovementType defaultAnchorMovementType)
        {
            if (defaultAnchorMovementType == AnchorMovementType.BeforeInsertion)
            {
                Replace(offset, 0, text, OffsetChangeMappingType.KeepAnchorBeforeInsertion);
            }
            else
            {
                Replace(offset, 0, text, null);
            }
        }

        /// <summary>
        /// Removes text.
        /// </summary>
        public void Remove(ISegment segment)
        {
            Replace(segment, string.Empty);
        }

        /// <summary>
        /// Removes text.
        /// </summary>
        /// <param name="offset">Starting offset of the text to be removed.</param>
        /// <param name="length">Length of the text to be removed.</param>
        public void Remove(int offset, int length)
        {
            Replace(offset, length, StringTextSource.Empty);
        }

        internal bool InDocumentChanging;

        /// <summary>
        /// Replaces text.
        /// </summary>
        public void Replace(ISegment segment, string text)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));
            Replace(segment.Offset, segment.Length, new StringTextSource(text), null);
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        public void Replace(ISegment segment, ITextSource text)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));
            Replace(segment.Offset, segment.Length, text, null);
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        /// <param name="offset">The starting offset of the text to be replaced.</param>
        /// <param name="length">The length of the text to be replaced.</param>
        /// <param name="text">The new text.</param>
        public void Replace(int offset, int length, string text)
        {
            Replace(offset, length, new StringTextSource(text), null);
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        /// <param name="offset">The starting offset of the text to be replaced.</param>
        /// <param name="length">The length of the text to be replaced.</param>
        /// <param name="text">The new text.</param>
        public void Replace(int offset, int length, ITextSource text)
        {
            Replace(offset, length, text, null);
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        /// <param name="offset">The starting offset of the text to be replaced.</param>
        /// <param name="length">The length of the text to be replaced.</param>
        /// <param name="text">The new text.</param>
        /// <param name="offsetChangeMappingType">The offsetChangeMappingType determines how offsets inside the old text are mapped to the new text.
        /// This affects how the anchors and segments inside the replaced region behave.</param>
        public void Replace(int offset, int length, string text, OffsetChangeMappingType offsetChangeMappingType)
        {
            Replace(offset, length, new StringTextSource(text), offsetChangeMappingType);
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        /// <param name="offset">The starting offset of the text to be replaced.</param>
        /// <param name="length">The length of the text to be replaced.</param>
        /// <param name="text">The new text.</param>
        /// <param name="offsetChangeMappingType">The offsetChangeMappingType determines how offsets inside the old text are mapped to the new text.
        /// This affects how the anchors and segments inside the replaced region behave.</param>
        public void Replace(int offset, int length, ITextSource text, OffsetChangeMappingType offsetChangeMappingType)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            // Please see OffsetChangeMappingType XML comments for details on how these modes work.
            switch (offsetChangeMappingType)
            {
                case OffsetChangeMappingType.Normal:
                    Replace(offset, length, text, null);
                    break;
                case OffsetChangeMappingType.KeepAnchorBeforeInsertion:
                    Replace(offset, length, text, OffsetChangeMap.FromSingleElement(
                        new OffsetChangeMapEntry(offset, length, text.TextLength, false, true)));
                    break;
                case OffsetChangeMappingType.RemoveAndInsert:
                    if (length == 0 || text.TextLength == 0)
                    {
                        // only insertion or only removal?
                        // OffsetChangeMappingType doesn't matter, just use Normal.
                        Replace(offset, length, text, null);
                    }
                    else
                    {
                        var map = new OffsetChangeMap(2)
                        {
                            new OffsetChangeMapEntry(offset, length, 0),
                            new OffsetChangeMapEntry(offset, 0, text.TextLength)
                        };
                        map.Freeze();
                        Replace(offset, length, text, map);
                    }
                    break;
                case OffsetChangeMappingType.CharacterReplace:
                    if (length == 0 || text.TextLength == 0)
                    {
                        // only insertion or only removal?
                        // OffsetChangeMappingType doesn't matter, just use Normal.
                        Replace(offset, length, text, null);
                    }
                    else if (text.TextLength > length)
                    {
                        // look at OffsetChangeMappingType.CharacterReplace XML comments on why we need to replace
                        // the last character
                        var entry = new OffsetChangeMapEntry(offset + length - 1, 1, 1 + text.TextLength - length);
                        Replace(offset, length, text, OffsetChangeMap.FromSingleElement(entry));
                    }
                    else if (text.TextLength < length)
                    {
                        var entry = new OffsetChangeMapEntry(offset + text.TextLength, length - text.TextLength, 0, true, false);
                        Replace(offset, length, text, OffsetChangeMap.FromSingleElement(entry));
                    }
                    else
                    {
                        Replace(offset, length, text, OffsetChangeMap.Empty);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(offsetChangeMappingType), offsetChangeMappingType, "Invalid enum value");
            }
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        /// <param name="offset">The starting offset of the text to be replaced.</param>
        /// <param name="length">The length of the text to be replaced.</param>
        /// <param name="text">The new text.</param>
        /// <param name="offsetChangeMap">The offsetChangeMap determines how offsets inside the old text are mapped to the new text.
        /// This affects how the anchors and segments inside the replaced region behave.
        /// If you pass null (the default when using one of the other overloads), the offsets are changed as
        /// in OffsetChangeMappingType.Normal mode.
        /// If you pass OffsetChangeMap.Empty, then everything will stay in its old place (OffsetChangeMappingType.CharacterReplace mode).
        /// The offsetChangeMap must be a valid 'explanation' for the document change. See <see cref="OffsetChangeMap.IsValidForDocumentChange"/>.
        /// Passing an OffsetChangeMap to the Replace method will automatically freeze it to ensure the thread safety of the resulting
        /// DocumentChangeEventArgs instance.
        /// </param>
        public void Replace(int offset, int length, string text, OffsetChangeMap offsetChangeMap)
        {
            Replace(offset, length, new StringTextSource(text), offsetChangeMap);
        }

        /// <summary>
        /// Replaces text.
        /// </summary>
        /// <param name="offset">The starting offset of the text to be replaced.</param>
        /// <param name="length">The length of the text to be replaced.</param>
        /// <param name="text">The new text.</param>
        /// <param name="offsetChangeMap">The offsetChangeMap determines how offsets inside the old text are mapped to the new text.
        /// This affects how the anchors and segments inside the replaced region behave.
        /// If you pass null (the default when using one of the other overloads), the offsets are changed as
        /// in OffsetChangeMappingType.Normal mode.
        /// If you pass OffsetChangeMap.Empty, then everything will stay in its old place (OffsetChangeMappingType.CharacterReplace mode).
        /// The offsetChangeMap must be a valid 'explanation' for the document change. See <see cref="OffsetChangeMap.IsValidForDocumentChange"/>.
        /// Passing an OffsetChangeMap to the Replace method will automatically freeze it to ensure the thread safety of the resulting
        /// DocumentChangeEventArgs instance.
        /// </param>
        public void Replace(int offset, int length, ITextSource text, OffsetChangeMap offsetChangeMap)
        {
            text = text?.CreateSnapshot() ?? throw new ArgumentNullException(nameof(text));
            offsetChangeMap?.Freeze();

            // Ensure that all changes take place inside an update group.
            // Will also take care of throwing an exception if inDocumentChanging is set.
            BeginUpdate();
            try
            {
                // protect document change against corruption by other changes inside the event handlers
                InDocumentChanging = true;
                try
                {
                    // The range verification must wait until after the BeginUpdate() call because the document
                    // might be modified inside the UpdateStarted event.
                    ThrowIfRangeInvalid(offset, length);

                    DoReplace(offset, length, text, offsetChangeMap);
                }
                finally
                {
                    InDocumentChanging = false;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        private void DoReplace(int offset, int length, ITextSource newText, OffsetChangeMap offsetChangeMap)
        {
            if (length == 0 && newText.TextLength == 0)
                return;

            // trying to replace a single character in 'Normal' mode?
            // for single characters, 'CharacterReplace' mode is equivalent, but more performant
            // (we don't have to touch the anchorTree at all in 'CharacterReplace' mode)
            if (length == 1 && newText.TextLength == 1 && offsetChangeMap == null)
                offsetChangeMap = OffsetChangeMap.Empty;

            ITextSource removedText;
            if (length == 0)
            {
                removedText = StringTextSource.Empty;
            }
            else if (length < 100)
            {
                removedText = new StringTextSource(_rope.ToString(offset, length));
            }
            else
            {
                // use a rope if the removed string is long
                removedText = new RopeTextSource(_rope.GetRange(offset, length));
            }
            var args = new DocumentChangeEventArgs(offset, removedText, newText, offsetChangeMap);

            // fire DocumentChanging event
            Changing?.Invoke(this, args);
            TextChangingInternal?.Invoke(this, args);

            _undoStack.Push(this, args);

            _cachedText = null; // reset cache of complete document text
            _fireTextChanged = true;
            var delayedEvents = new DelayedEvents();

            lock (_lockObject)
            {
                // create linked list of checkpoints
                _versionProvider.AppendChange(args);

                // now update the textBuffer and lineTree
                if (offset == 0 && length == _rope.Length)
                {
                    // optimize replacing the whole document
                    _rope.Clear();
                    if (newText is RopeTextSource newRopeTextSource)
                        _rope.InsertRange(0, newRopeTextSource.GetRope());
                    else
                        _rope.InsertText(0, newText.Text);
                    _lineManager.Rebuild();
                }
                else
                {
                    _rope.RemoveRange(offset, length);
                    _lineManager.Remove(offset, length);
#if DEBUG
                    _lineTree.CheckProperties();
#endif
                    if (newText is RopeTextSource newRopeTextSource)
                        _rope.InsertRange(offset, newRopeTextSource.GetRope());
                    else
                        _rope.InsertText(offset, newText.Text);
                    _lineManager.Insert(offset, newText);
#if DEBUG
                    _lineTree.CheckProperties();
#endif
                }
            }

            // update text anchors
            if (offsetChangeMap == null)
            {
                _anchorTree.HandleTextChange(args.CreateSingleChangeMapEntry(), delayedEvents);
            }
            else
            {
                foreach (var entry in offsetChangeMap)
                {
                    _anchorTree.HandleTextChange(entry, delayedEvents);
                }
            }

            _lineManager.ChangeComplete(args);

            // raise delayed events after our data structures are consistent again
            delayedEvents.RaiseEvents();

            // fire DocumentChanged event
            Changed?.Invoke(this, args);
            TextChangedInternal?.Invoke(this, args);
        }
        #endregion

        #region GetLineBy...
        /// <summary>
        /// Gets a read-only list of lines.
        /// </summary>
        /// <remarks><inheritdoc cref="DocumentLine"/></remarks>
        public IList<DocumentLine> Lines => _lineTree;

        /// <summary>
        /// Gets a line by the line number: O(log n)
        /// </summary>
        public DocumentLine GetLineByNumber(int number)
        {
            VerifyAccess();
            if (number < 1 || number > _lineTree.LineCount)
                throw new ArgumentOutOfRangeException(nameof(number), number, "Value must be between 1 and " + _lineTree.LineCount);
            return _lineTree.GetByNumber(number);
        }

        IDocumentLine IDocument.GetLineByNumber(int lineNumber)
        {
            return GetLineByNumber(lineNumber);
        }

        /// <summary>
        /// Gets a document lines by offset.
        /// Runtime: O(log n)
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        public DocumentLine GetLineByOffset(int offset)
        {
            VerifyAccess();
            if (offset < 0 || offset > _rope.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "0 <= offset <= " + _rope.Length);
            }
            return _lineTree.GetByOffset(offset);
        }

        IDocumentLine IDocument.GetLineByOffset(int offset)
        {
            return GetLineByOffset(offset);
        }
        #endregion

        #region GetOffset / GetLocation
        /// <summary>
        /// Gets the offset from a text location.
        /// </summary>
        /// <seealso cref="GetLocation"/>
        public int GetOffset(TextLocation location)
        {
            return GetOffset(location.Line, location.Column);
        }

        /// <summary>
        /// Gets the offset from a text location.
        /// </summary>
        /// <seealso cref="GetLocation"/>
        public int GetOffset(int line, int column)
        {
            var docLine = GetLineByNumber(line);
            if (column <= 0)
                return docLine.Offset;
            if (column > docLine.Length)
                return docLine.EndOffset;
            return docLine.Offset + column - 1;
        }

        /// <summary>
        /// Gets the location from an offset.
        /// </summary>
        /// <seealso cref="GetOffset(TextLocation)"/>
        public TextLocation GetLocation(int offset)
        {
            var line = GetLineByOffset(offset);
            return new TextLocation(line.LineNumber, offset - line.Offset + 1);
        }
        #endregion

        #region Line Trackers

        private readonly ObservableCollection<ILineTracker> _lineTrackers = new ObservableCollection<ILineTracker>();

        /// <summary>
        /// Gets the list of <see cref="ILineTracker"/>s attached to this document.
        /// You can add custom line trackers to this list.
        /// </summary>
        public IList<ILineTracker> LineTrackers
        {
            get
            {
                VerifyAccess();
                return _lineTrackers;
            }
        }
        #endregion

        #region UndoStack

        private UndoStack _undoStack;

        /// <summary>
        /// Gets the <see cref="UndoStack"/> of the document.
        /// </summary>
        /// <remarks>This property can also be used to set the undo stack, e.g. for sharing a common undo stack between multiple documents.</remarks>
        public UndoStack UndoStack
        {
            get => _undoStack;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                if (value != _undoStack)
                {
                    _undoStack.ClearAll(); // first clear old undo stack, so that it can't be used to perform unexpected changes on this document
                                          // ClearAll() will also throw an exception when it's not safe to replace the undo stack (e.g. update is currently in progress)
                    _undoStack = value;
                    OnPropertyChanged("UndoStack");
                }
            }
        }
        #endregion

        #region CreateAnchor
        /// <summary>
        /// Creates a new <see cref="TextAnchor"/> at the specified offset.
        /// </summary>
        /// <inheritdoc cref="TextAnchor" select="remarks|example"/>
        public TextAnchor CreateAnchor(int offset)
        {
            VerifyAccess();
            if (offset < 0 || offset > _rope.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "0 <= offset <= " + _rope.Length.ToString(CultureInfo.InvariantCulture));
            }
            return _anchorTree.CreateAnchor(offset);
        }

        ITextAnchor IDocument.CreateAnchor(int offset)
        {
            return CreateAnchor(offset);
        }
        #endregion

        #region LineCount
        /// <summary>
        /// Gets the total number of lines in the document.
        /// Runtime: O(1).
        /// </summary>
        public int LineCount
        {
            get
            {
                VerifyAccess();
                return _lineTree.LineCount;
            }
        }

        /// <summary>
        /// Is raised when the LineCount property changes.
        /// </summary>
        public event EventHandler LineCountChanged;
        #endregion

        #region Debugging
        [Conditional("DEBUG")]
        internal void DebugVerifyAccess()
        {
            VerifyAccess();
        }

        /// <summary>
        /// Gets the document lines tree in string form.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        internal string GetLineTreeAsString()
        {
#if DEBUG
            return _lineTree.GetTreeAsString();
#else
			return "Not available in release build.";
#endif
        }

        /// <summary>
        /// Gets the text anchor tree in string form.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        internal string GetTextAnchorTreeAsString()
        {
#if DEBUG
            return _anchorTree.GetTreeAsString();
#else
			return "Not available in release build.";
#endif
        }
        #endregion

        #region Service Provider

        internal IServiceProvider ServiceProvider
        {
            get
            {
                VerifyAccess();
                if (field == null)
                {
                    var container = new ServiceContainer();
                    container.AddService(this);
                    container.AddService<IDocument>(this);
                    field = container;
                }
                return field;
            }
            set
            {
                VerifyAccess();
                field = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        #endregion

        #region FileName

        /// <inheritdoc/>
        public event EventHandler FileNameChanged;

        private void OnFileNameChanged(EventArgs e)
        {
            FileNameChanged?.Invoke(this, e);
        }

        /// <inheritdoc/>
        public string FileName
        {
            get { return field; }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnFileNameChanged(EventArgs.Empty);
                }
            }
        }
        #endregion
    }
}

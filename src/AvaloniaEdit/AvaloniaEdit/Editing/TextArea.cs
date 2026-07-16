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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Control that wraps a TextView and adds support for user input and the caret.
    /// </summary>
    public class TextArea : TemplatedControl, ITextEditorComponent, IRoutedCommandBindable, ILogicalScrollable
    {
        /// <summary>
        /// This is the extra scrolling space that occurs after the last line.
        /// </summary>
        private const int AdditionalVerticalScrollAmount = 2;

        private readonly ILogicalScrollable _logicalScrollable;

        private readonly TextAreaTextInputMethodClient _imClient = new TextAreaTextInputMethodClient();

        #region Constructor
        static TextArea()
        {
            KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<TextArea>(KeyboardNavigationMode.None);
            FocusableProperty.OverrideDefaultValue<TextArea>(true);

            DocumentProperty.Changed.Subscribe(OnDocumentChanged);
            OptionsProperty.Changed.Subscribe(OnOptionsChanged);

            AffectsArrange<TextArea>(OffsetProperty);
            AffectsRender<TextArea>(OffsetProperty);

            TextInputMethodClientRequestedEvent.AddClassHandler<TextArea>((ta, e) =>
            {
                if (!ta.IsReadOnly)
                {
                    e.Client = ta._imClient;
                }
            });
        }

        /// <summary>
        /// Creates a new TextArea instance.
        /// </summary>
        public TextArea() : this(new TextView())
        {
            AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyUpEvent, OnPreviewKeyUp, RoutingStrategies.Tunnel);
        }

        /// <summary>
        /// Creates a new TextArea instance.
        /// </summary>
        protected TextArea(TextView textView)
        {
            TextView = textView ?? throw new ArgumentNullException(nameof(textView));
            _logicalScrollable = textView;
            Options = textView.Options;

            _selection = EmptySelection = new EmptySelection(this);

            textView.Services.AddService(this);

            textView.LineTransformers.Add(new SelectionColorizer(this));
            textView.InsertLayer(new SelectionLayer(this), KnownLayer.Selection, LayerInsertionPosition.Replace);

            Caret = new Caret(this);
            Caret.PositionChanged += (sender, e) => RequestSelectionValidation();
            Caret.PositionChanged += CaretPositionChanged;
            AttachTypingEvents();

            LeftMargins.CollectionChanged += LeftMargins_CollectionChanged;

            DefaultInputHandler = new TextAreaDefaultInputHandler(this);
            ActiveInputHandler = DefaultInputHandler;

            // TODO
            //textView.GetObservable(TextBlock.FontSizeProperty).Subscribe(_ =>
            //{
            //    TextView.SetScrollOffset(new Vector(_offset.X, _offset.Y * TextView.DefaultLineHeight));
            //});
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (e.NameScope.Find("PART_CP") is ContentPresenter contentPresenter)
            {
                contentPresenter.Content = TextView;
            }
        }

        internal void AddChild(Visual visual)
        {
            VisualChildren.Add(visual);
            InvalidateArrange();
        }

        internal void RemoveChild(Visual visual)
        {
            VisualChildren.Remove(visual);
        }

        #endregion

        #region Watermark
        /// <summary>
        /// Defines the <see cref="Watermark"/> property
        /// </summary>
        public static readonly StyledProperty<string> WatermarkProperty =
            AvaloniaProperty.Register<TextArea, string>(nameof(Watermark));

        /// <summary>
        /// Gets or sets the placeholder or descriptive text that is displayed even if the <see cref="Text"/>
        /// property is not yet set.
        /// </summary>
        public string Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }
        #endregion

        /// <summary>
        ///     Defines the <see cref="IScrollable.Offset" /> property.
        /// </summary>
        public static readonly DirectProperty<TextArea, Vector> OffsetProperty =
            AvaloniaProperty.RegisterDirect<TextArea, Vector>(
                nameof(IScrollable.Offset),
                static o => (o as IScrollable).Offset,
                static (o, v) => (o as IScrollable).Offset = v);

        #region InputHandler management
        /// <summary>
        /// Gets the default input handler.
        /// </summary>
        /// <remarks><inheritdoc cref="ITextAreaInputHandler"/></remarks>
        public TextAreaDefaultInputHandler DefaultInputHandler { get; }

        private bool _isChangingInputHandler;

        /// <summary>
        /// Gets/Sets the active input handler.
        /// This property does not return currently active stacked input handlers. Setting this property detached all stacked input handlers.
        /// </summary>
        /// <remarks><inheritdoc cref="ITextAreaInputHandler"/></remarks>
        public ITextAreaInputHandler ActiveInputHandler
        {
            get;
            set
            {
                if (value != null && value.TextArea != this)
                    throw new ArgumentException(
                        "The input handler was created for a different text area than this one.");
                if (_isChangingInputHandler)
                    throw new InvalidOperationException("Cannot set ActiveInputHandler recursively");
                if (field != value)
                {
                    _isChangingInputHandler = true;
                    try
                    {
                        // pop the whole stack
                        PopStackedInputHandler(StackedInputHandlers.LastOrDefault());
                        Debug.Assert(StackedInputHandlers.IsEmpty);

                        field?.Detach();
                        field = value;
                        value?.Attach();
                    }
                    finally
                    {
                        _isChangingInputHandler = false;
                    }

                    ActiveInputHandlerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the ActiveInputHandler property changes.
        /// </summary>
        public event EventHandler ActiveInputHandlerChanged;

        /// <summary>
        /// Gets the list of currently active stacked input handlers.
        /// </summary>
        /// <remarks><inheritdoc cref="ITextAreaInputHandler"/></remarks>
        public ImmutableStack<TextAreaStackedInputHandler> StackedInputHandlers { get; private set; } = ImmutableStack<TextAreaStackedInputHandler>.Empty;

        /// <summary>
        /// Pushes an input handler onto the list of stacked input handlers.
        /// </summary>
        /// <remarks><inheritdoc cref="ITextAreaInputHandler"/></remarks>
        public void PushStackedInputHandler(TextAreaStackedInputHandler inputHandler)
        {
            if (inputHandler == null)
                throw new ArgumentNullException(nameof(inputHandler));
            StackedInputHandlers = StackedInputHandlers.Push(inputHandler);
            inputHandler.Attach();
        }

        /// <summary>
        /// Pops the stacked input handler (and all input handlers above it).
        /// If <paramref name="inputHandler"/> is not found in the currently stacked input handlers, or is null, this method
        /// does nothing.
        /// </summary>
        /// <remarks><inheritdoc cref="ITextAreaInputHandler"/></remarks>
        public void PopStackedInputHandler(TextAreaStackedInputHandler inputHandler)
        {
            if (StackedInputHandlers.Any(i => i == inputHandler))
            {
                ITextAreaInputHandler oldHandler;
                do
                {
                    oldHandler = StackedInputHandlers.Peek();
                    StackedInputHandlers = StackedInputHandlers.Pop();
                    oldHandler.Detach();
                } while (oldHandler != inputHandler);
            }
        }
        #endregion

        #region Document property
        /// <summary>
        /// Document property.
        /// </summary>
        public static readonly StyledProperty<TextDocument> DocumentProperty
            = TextView.DocumentProperty.AddOwner<TextArea>();

        /// <summary>
        /// Gets/Sets the document displayed by the text editor.
        /// </summary>
        public TextDocument Document
        {
            get => GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        /// <inheritdoc/>
        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        /// <summary>
        /// Gets if the the document displayed by the text editor is readonly
        /// </summary>
        public bool IsReadOnly
        {
            get => ReadOnlySectionProvider == ReadOnlySectionDocument.Instance;
        }

        private static void OnDocumentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextArea)?.OnDocumentChanged((TextDocument)e.OldValue, (TextDocument)e.NewValue);
        }

        private void OnDocumentChanged(TextDocument oldValue, TextDocument newValue)
        {
            if (oldValue != null)
            {
                TextDocumentWeakEventManager.Changing.RemoveHandler(oldValue, OnDocumentChanging);
                TextDocumentWeakEventManager.Changed.RemoveHandler(oldValue, OnDocumentChanged);
                TextDocumentWeakEventManager.UpdateStarted.RemoveHandler(oldValue, OnUpdateStarted);
                TextDocumentWeakEventManager.UpdateFinished.RemoveHandler(oldValue, OnUpdateFinished);
            }
            TextView.Document = newValue;
            if (newValue != null)
            {
                TextDocumentWeakEventManager.Changing.AddHandler(newValue, OnDocumentChanging);
                TextDocumentWeakEventManager.Changed.AddHandler(newValue, OnDocumentChanged);
                TextDocumentWeakEventManager.UpdateStarted.AddHandler(newValue, OnUpdateStarted);
                TextDocumentWeakEventManager.UpdateFinished.AddHandler(newValue, OnUpdateFinished);

                InvalidateArrange();
            }
            // Reset caret location and selection: this is necessary because the caret/selection might be invalid
            // in the new document (e.g. if new document is shorter than the old document).
            Caret.Location = new TextLocation(1, 1);
            ClearSelection();
            DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(oldValue, newValue));
            //CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region Options property
        /// <summary>
        /// Options property.
        /// </summary>
        public static readonly StyledProperty<TextEditorOptions> OptionsProperty
            = TextView.OptionsProperty.AddOwner<TextArea>();

        /// <summary>
        /// Gets/Sets the document displayed by the text editor.
        /// </summary>
        public TextEditorOptions Options
        {
            get => GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        /// <summary>
        /// Occurs when a text editor option has changed.
        /// </summary>
        public event PropertyChangedEventHandler OptionChanged;

        private void OnOptionChanged(object sender, PropertyChangedEventArgs e)
        {
            OnOptionChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="OptionChanged"/> event.
        /// </summary>
        protected virtual void OnOptionChanged(PropertyChangedEventArgs e)
        {
            OptionChanged?.Invoke(this, e);
        }

        private static void OnOptionsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextArea)?.OnOptionsChanged((TextEditorOptions)e.OldValue, (TextEditorOptions)e.NewValue);
        }

        private void OnOptionsChanged(TextEditorOptions oldValue, TextEditorOptions newValue)
        {
            if (oldValue != null)
            {
                PropertyChangedWeakEventManager.RemoveHandler(oldValue, OnOptionChanged);
            }
            TextView.Options = newValue;
            if (newValue != null)
            {
                PropertyChangedWeakEventManager.AddHandler(newValue, OnOptionChanged);
            }
            OnOptionChanged(new PropertyChangedEventArgs(null));
        }
        #endregion

        #region Caret handling on document changes

        private void OnDocumentChanging(object sender, DocumentChangeEventArgs e)
        {
            Caret.OnDocumentChanging();
        }

        private void OnDocumentChanged(object sender, DocumentChangeEventArgs e)
        {
            Caret.OnDocumentChanged(e);
            Selection = _selection.UpdateOnDocumentChange(e);
        }

        private void OnUpdateStarted(object sender, EventArgs e)
        {
            Document.UndoStack.PushOptional(new RestoreCaretAndSelectionUndoAction(this));
        }

        private void OnUpdateFinished(object sender, EventArgs e)
        {
            Caret.OnDocumentUpdateFinished();
        }

        private sealed class RestoreCaretAndSelectionUndoAction : IUndoableOperation
        {
            // keep textarea in weak reference because the IUndoableOperation is stored with the document
            private readonly WeakReference _textAreaReference;

            private readonly TextViewPosition _caretPosition;
            private readonly Selection _selection;

            public RestoreCaretAndSelectionUndoAction(TextArea textArea)
            {
                _textAreaReference = new WeakReference(textArea);
                // Just save the old caret position, no need to validate here.
                // If we restore it, we'll validate it anyways.
                _caretPosition = textArea.Caret.NonValidatedPosition;
                _selection = textArea.Selection;
            }

            public void Undo()
            {
                var textArea = (TextArea)_textAreaReference.Target;
                if (textArea != null)
                {
                    textArea.Caret.Position = _caretPosition;
                    textArea.Selection = _selection;
                }
            }

            public void Redo()
            {
                // redo=undo: we just restore the caret/selection state
                Undo();
            }
        }
        #endregion

        #region TextView property

        /// <summary>
        /// Gets the text view used to display text in this text area.
        /// </summary>
        public TextView TextView { get; }

        #endregion

        #region Selection property
        internal readonly Selection EmptySelection;
        private Selection _selection;

        /// <summary>
        /// Occurs when the selection has changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Gets/Sets the selection in this text area.
        /// </summary>

        public Selection Selection
        {
            get => _selection;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value.TextArea != this)
                    throw new ArgumentException("Cannot use a Selection instance that belongs to another text area.");
                if (!Equals(_selection, value))
                {
                    if (TextView != null)
                    {
                        var oldSegment = _selection.SurroundingSegment;
                        var newSegment = value.SurroundingSegment;
                        if (!Selection.EnableVirtualSpace && (_selection is SimpleSelection && value is SimpleSelection && oldSegment != null && newSegment != null))
                        {
                            // perf optimization:
                            // When a simple selection changes, don't redraw the whole selection, but only the changed parts.
                            var oldSegmentOffset = oldSegment.Offset;
                            var newSegmentOffset = newSegment.Offset;
                            if (oldSegmentOffset != newSegmentOffset)
                            {
                                TextView.Redraw(Math.Min(oldSegmentOffset, newSegmentOffset),
                                                Math.Abs(oldSegmentOffset - newSegmentOffset));
                            }
                            var oldSegmentEndOffset = oldSegment.EndOffset;
                            var newSegmentEndOffset = newSegment.EndOffset;
                            if (oldSegmentEndOffset != newSegmentEndOffset)
                            {
                                TextView.Redraw(Math.Min(oldSegmentEndOffset, newSegmentEndOffset),
                                                Math.Abs(oldSegmentEndOffset - newSegmentEndOffset));
                            }
                        }
                        else
                        {
                            TextView.Redraw(oldSegment);
                            TextView.Redraw(newSegment);
                        }
                    }
                    _selection = value;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    // a selection change causes commands like copy/paste/etc. to change status
                    //CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            Selection = EmptySelection;
        }

        /// <summary>
        /// The <see cref="SelectionBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> SelectionBrushProperty =
            AvaloniaProperty.Register<TextArea, IBrush>("SelectionBrush");

        /// <summary>
        /// Gets/Sets the background brush used for the selection.
        /// </summary>
        public IBrush SelectionBrush
        {
            get => GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        /// <summary>
        /// The <see cref="SelectionForeground"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> SelectionForegroundProperty =
            AvaloniaProperty.Register<TextArea, IBrush>("SelectionForeground");

        /// <summary>
        /// Gets/Sets the foreground brush used for selected text.
        /// </summary>
        public IBrush SelectionForeground
        {
            get => GetValue(SelectionForegroundProperty);
            set => SetValue(SelectionForegroundProperty, value);
        }

        /// <summary>
        /// The <see cref="SelectionBorder"/> property.
        /// </summary>
        public static readonly StyledProperty<Pen> SelectionBorderProperty =
            AvaloniaProperty.Register<TextArea, Pen>("SelectionBorder");

        /// <summary>
        /// Gets/Sets the pen used for the border of the selection.
        /// </summary>
        public Pen SelectionBorder
        {
            get => GetValue(SelectionBorderProperty);
            set => SetValue(SelectionBorderProperty, value);
        }

        /// <summary>
        /// The <see cref="SelectionCornerRadius"/> property.
        /// </summary>
        public static readonly StyledProperty<double> SelectionCornerRadiusProperty =
            AvaloniaProperty.Register<TextArea, double>("SelectionCornerRadius", 3.0);

        /// <summary>
        /// Gets/Sets the corner radius of the selection.
        /// </summary>
        public double SelectionCornerRadius
        {
            get => GetValue(SelectionCornerRadiusProperty);
            set => SetValue(SelectionCornerRadiusProperty, value);
        }
        #endregion

        #region Force caret to stay inside selection

        private bool _ensureSelectionValidRequested;
        private int _allowCaretOutsideSelection;

        private void RequestSelectionValidation()
        {
            if (!_ensureSelectionValidRequested && _allowCaretOutsideSelection == 0)
            {
                _ensureSelectionValidRequested = true;
                Dispatcher.UIThread.Post(EnsureSelectionValid);
            }
        }

        /// <summary>
        /// Code that updates only the caret but not the selection can cause confusion when
        /// keys like 'Delete' delete the (possibly invisible) selected text and not the
        /// text around the caret.
        /// 
        /// So we'll ensure that the caret is inside the selection.
        /// (when the caret is not in the selection, we'll clear the selection)
        /// 
        /// This method is invoked using the Dispatcher so that code may temporarily violate this rule
        /// (e.g. most 'extend selection' methods work by first setting the caret, then the selection),
        /// it's sufficient to fix it after any event handlers have run.
        /// </summary>
        private void EnsureSelectionValid()
        {
            _ensureSelectionValidRequested = false;
            if (_allowCaretOutsideSelection == 0)
            {
                if (!_selection.IsEmpty && !_selection.Contains(Caret.Offset))
                {
                    ClearSelection();
                }
            }
        }

        /// <summary>
        /// Temporarily allows positioning the caret outside the selection.
        /// Dispose the returned IDisposable to revert the allowance.
        /// </summary>
        /// <remarks>
        /// The text area only forces the caret to be inside the selection when other events
        /// have finished running (using the dispatcher), so you don't have to use this method
        /// for temporarily positioning the caret in event handlers.
        /// This method is only necessary if you want to run the dispatcher, e.g. if you
        /// perform a drag'n'drop operation.
        /// </remarks>
        public IDisposable AllowCaretOutsideSelection()
        {
            VerifyAccess();
            _allowCaretOutsideSelection++;
            return new CallbackOnDispose(
                delegate
                {
                    VerifyAccess();
                    _allowCaretOutsideSelection--;
                    RequestSelectionValidation();
                });
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the Caret used for this text area.
        /// </summary>
        public Caret Caret { get; }

        /// <summary>
        /// Scrolls the text view so that the requested line is in the middle.
        /// If the textview can be scrolled.
        /// </summary>
        /// <param name="line">The line to scroll to.</param>
        public void ScrollToLine(int line)
        {
            var viewPortLines = (int)(this as IScrollable).Viewport.Height;

            if (viewPortLines < Document.LineCount)
            {
                ScrollToLine(line, 2, viewPortLines / 2);
            }
        }

        /// <summary>
        /// Scrolls the textview to a position with n lines above and below it.
        /// </summary>
        /// <param name="line">the requested line number.</param>
        /// <param name="linesEitherSide">The number of lines above and below.</param>
        public void ScrollToLine(int line, int linesEitherSide)
        {
            ScrollToLine(line, linesEitherSide, linesEitherSide);
        }

        /// <summary>
        /// Scrolls the textview to a position with n lines above and below it.
        /// </summary>
        /// <param name="line">the requested line number.</param>
        /// <param name="linesAbove">The number of lines above.</param>
        /// <param name="linesBelow">The number of lines below.</param>
        public void ScrollToLine(int line, int linesAbove, int linesBelow)
        {
            var offset = line - linesAbove;

            if (offset < 0)
            {
                offset = 0;
            }

            this.BringIntoView(new Rect(1, offset, 0, 1));

            offset = line + linesBelow;

            if (offset >= 0)
            {
                this.BringIntoView(new Rect(1, offset, 0, 1));
            }
        }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            if (TextView == null)
                return;

            TextView.HighlightedLine = Caret.Line;

            ScrollToLine(Caret.Line, 2);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                (this as ILogicalScrollable).RaiseScrollInvalidated(EventArgs.Empty);
            });
        }

        public static readonly DirectProperty<TextArea, ObservableCollection<Control>> LeftMarginsProperty
            = AvaloniaProperty.RegisterDirect<TextArea, ObservableCollection<Control>>(nameof(LeftMargins),
                c => c.LeftMargins);

        /// <summary>
        /// Gets the collection of margins displayed to the left of the text view.
        /// </summary>
        public ObservableCollection<Control> LeftMargins { get; } = new ObservableCollection<Control>();

        private void LeftMargins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var c in e.OldItems.OfType<ITextViewConnect>())
                {
                    c.RemoveFromTextView(TextView);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var c in e.NewItems.OfType<ITextViewConnect>())
                {
                    c.AddToTextView(TextView);
                }
            }
        }

        /// <summary>
        /// Gets/Sets an object that provides read-only sections for the text area.
        /// </summary>
        public IReadOnlySectionProvider ReadOnlySectionProvider
        {
            get;
            set => field = value ?? throw new ArgumentNullException(nameof(value));
        } = NoReadOnlySections.Instance;

        /// <summary>
        /// The <see cref="RightClickMovesCaret"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> RightClickMovesCaretProperty =
            AvaloniaProperty.Register<TextArea, bool>(nameof(RightClickMovesCaret), false);

        /// <summary>
        /// Determines whether caret position should be changed to the mouse position when you right click or not.
        /// </summary>
        public bool RightClickMovesCaret
        {
            get => GetValue(RightClickMovesCaretProperty);
            set => SetValue(RightClickMovesCaretProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="CaretBrush"/> property
        /// </summary>
        public static readonly DirectProperty<TextArea, IBrush> CaretBrushProperty =
            AvaloniaProperty.RegisterDirect<TextArea, IBrush>(nameof(CaretBrush),
                getter: (textArea) => textArea.Caret.CaretBrush,
                setter: (textArea, brush) => textArea.Caret.CaretBrush = brush);

        /// <summary>
        /// Gets or sets the brush used for Caret.
        /// </summary>
        public IBrush CaretBrush
        {
            get => GetValue(CaretBrushProperty);
            set => SetValue(CaretBrushProperty, value);
        }
        #endregion

        #region Focus Handling (Show/Hide Caret)

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Focus();
        }

        protected override void OnGotFocus(FocusChangedEventArgs e)
        {
            base.OnGotFocus(e);

            Caret.Show();

            _imClient.SetTextArea(this);
        }

        protected override void OnLostFocus(FocusChangedEventArgs e)
        {
            base.OnLostFocus(e);

            Caret.Hide();

            _imClient.SetTextArea(null);
        }
        #endregion

        #region OnTextInput / RemoveSelectedText / ReplaceSelectionWithText
        /// <summary>
        /// Occurs when the TextArea receives text input.
        /// but occurs immediately before the TextArea handles the TextInput event.
        /// </summary>
        public event EventHandler<TextInputEventArgs> TextEntering;

        /// <summary>
        /// Occurs when the TextArea receives text input.
        /// but occurs immediately after the TextArea handles the TextInput event.
        /// </summary>
        public event EventHandler<TextInputEventArgs> TextEntered;

        /// <summary>
        /// Raises the TextEntering event.
        /// </summary>
        protected virtual void OnTextEntering(TextInputEventArgs e)
        {
            TextEntering?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the TextEntered event.
        /// </summary>
        protected virtual void OnTextEntered(TextInputEventArgs e)
        {
            TextEntered?.Invoke(this, e);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            if (!e.Handled && Document != null)
            {
                if (string.IsNullOrEmpty(e.Text) || e.Text == "\x1b" || e.Text == "\b" || e.Text == "\u007f")
                {
                    // TODO: check this
                    // ASCII 0x1b = ESC.
                    // produces a TextInput event with that old ASCII control char
                    // when Escape is pressed. We'll just ignore it.

                    // A deadkey followed by backspace causes a textinput event for the BS character.

                    // Similarly, some shortcuts like Alt+Space produce an empty TextInput event.
                    // We have to ignore those (not handle them) to keep the shortcut working.
                    return;
                }
                HideMouseCursor();
                PerformTextInput(e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Performs text input.
        /// This raises the <see cref="TextEntering"/> event, replaces the selection with the text,
        /// and then raises the <see cref="TextEntered"/> event.
        /// </summary>
        public void PerformTextInput(string text)
        {
            var e = new TextInputEventArgs
            {
                Text = text,
                RoutedEvent = TextInputEvent
            };
            PerformTextInput(e);
        }

        /// <summary>
        /// Performs text input.
        /// This raises the <see cref="TextEntering"/> event, replaces the selection with the text,
        /// and then raises the <see cref="TextEntered"/> event.
        /// </summary>
        public void PerformTextInput(TextInputEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (Document == null)
                throw ThrowUtil.NoDocumentAssigned();
            OnTextEntering(e);
            if (!e.Handled)
            {
                if (e.Text == "\n" || e.Text == "\r" || e.Text == "\r\n")
                    ReplaceSelectionWithNewLine();
                else
                {
                    if (OverstrikeMode && Selection.IsEmpty && Document.GetLineByNumber(Caret.Line).EndOffset > Caret.Offset)
                        EditingCommands.SelectRightByCharacter.Execute(null, this);

                    ReplaceSelectionWithText(e.Text);
                }
                OnTextEntered(e);
                Caret.BringCaretToView();
            }
        }

        private void ReplaceSelectionWithNewLine()
        {
            var newLine = TextUtilities.GetNewLineFromDocument(Document, Caret.Line);
            using (Document.RunUpdate())
            {
                ReplaceSelectionWithText(newLine);
                if (IndentationStrategy != null)
                {
                    var line = Document.GetLineByNumber(Caret.Line);
                    var deletable = GetDeletableSegments(line);
                    if (deletable.Length == 1 && deletable[0].Offset == line.Offset && deletable[0].Length == line.Length)
                    {
                        // use indentation strategy only if the line is not read-only
                        IndentationStrategy.IndentLine(Document, line);
                    }
                }
            }
        }

        internal void RemoveSelectedText()
        {
            if (Document == null)
                throw ThrowUtil.NoDocumentAssigned();
            _selection.ReplaceSelectionWithText(string.Empty);
#if DEBUG
            if (!_selection.IsEmpty)
            {
                foreach (var s in _selection.Segments)
                {
                    Debug.Assert(!ReadOnlySectionProvider.GetDeletableSegments(s).Any());
                }
            }
#endif
        }

        internal void ReplaceSelectionWithText(string newText)
        {
            if (newText == null)
                throw new ArgumentNullException(nameof(newText));
            if (Document == null)
                throw ThrowUtil.NoDocumentAssigned();
            _selection.ReplaceSelectionWithText(newText);
        }

        internal ISegment[] GetDeletableSegments(ISegment segment)
        {
            var deletableSegments = ReadOnlySectionProvider.GetDeletableSegments(segment);
            if (deletableSegments == null)
                throw new InvalidOperationException("ReadOnlySectionProvider.GetDeletableSegments returned null");
            var array = deletableSegments.ToArray();
            var lastIndex = segment.Offset;
            foreach (var t in array)
            {
                if (t.Offset < lastIndex)
                    throw new InvalidOperationException("ReadOnlySectionProvider returned incorrect segments (outside of input segment / wrong order)");
                lastIndex = t.EndOffset;
            }
            if (lastIndex > segment.EndOffset)
                throw new InvalidOperationException("ReadOnlySectionProvider returned incorrect segments (outside of input segment / wrong order)");
            return array;
        }
        #endregion

        #region IndentationStrategy property
        /// <summary>
        /// IndentationStrategy property.
        /// </summary>
        public static readonly StyledProperty<IIndentationStrategy> IndentationStrategyProperty =
            AvaloniaProperty.Register<TextArea, IIndentationStrategy>("IndentationStrategy", new DefaultIndentationStrategy());

        /// <summary>
        /// Gets/Sets the indentation strategy used when inserting new lines.
        /// </summary>
        public IIndentationStrategy IndentationStrategy
        {
            get => GetValue(IndentationStrategyProperty);
            set => SetValue(IndentationStrategyProperty, value);
        }
        #endregion

        #region OnKeyDown/OnKeyUp

        // Make life easier for text editor extensions that use a different cursor based on the pressed modifier keys.
        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Tab && Options.AcceptsTab && IsFocused)
            {
                e.Handled = true;
                if (e.KeyModifiers == KeyModifiers.Shift)
                {
                    EditingCommandHandler.OnShiftTab(this, e);
                }
                else
                {
                    EditingCommandHandler.OnTab(this, e);
                }
            }

            TextView.InvalidateCursorIfPointerWithinTextView();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach (var h in StackedInputHandlers)
            {
                if (e.Handled)
                    break;
                h.OnPreviewKeyDown(e);
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            TextView.InvalidateCursorIfPointerWithinTextView();
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            foreach (var h in StackedInputHandlers)
            {
                if (e.Handled)
                    break;
                h.OnPreviewKeyUp(e);
            }
        }

        #endregion

        #region Hide Mouse Cursor While Typing

        private bool _isMouseCursorHidden;

        private void AttachTypingEvents()
        {
            // Use the PreviewMouseMove event in case some other editor layer consumes the MouseMove event (e.g. SD's InsertionCursorLayer)
            PointerEntered += delegate { ShowMouseCursor(); };
            PointerExited += delegate { ShowMouseCursor(); };
        }

        private void ShowMouseCursor()
        {
            if (_isMouseCursorHidden)
            {
                _isMouseCursorHidden = false;
            }
        }

        private void HideMouseCursor()
        {
            if (Options.HideCursorWhileTyping && !_isMouseCursorHidden && IsPointerOver)
            {
                _isMouseCursorHidden = true;
            }
        }

        #endregion

        #region Overstrike mode

        /// <summary>
        /// The <see cref="OverstrikeMode"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> OverstrikeModeProperty =
            AvaloniaProperty.Register<TextArea, bool>("OverstrikeMode");

        /// <summary>
        /// Gets/Sets whether overstrike mode is active.
        /// </summary>
        public bool OverstrikeMode
        {
            get => GetValue(OverstrikeModeProperty);
            set => SetValue(OverstrikeModeProperty, value);
        }

        #endregion

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectionBrushProperty
                || change.Property == SelectionBorderProperty
                || change.Property == SelectionCornerRadiusProperty)
            {
                TextView.InvalidateLayer(KnownLayer.Selection);
            }
            else if (change.Property == SelectionForegroundProperty)
            {
                TextView.Redraw();
            }
            else if (change.Property == OverstrikeModeProperty)
            {
                Caret.UpdateIfVisible();
            }
        }

        /// <summary>
        /// Gets the requested service.
        /// </summary>
        /// <returns>Returns the requested service instance, or null if the service cannot be found.</returns>
        public virtual object GetService(Type serviceType)
        {
            return TextView.GetService(serviceType);
        }

        /// <summary>
        /// Occurs when text inside the TextArea was copied.
        /// </summary>
        public event EventHandler<TextEventArgs> TextCopied;

        /// <summary>
        /// Accurs when new text is pasted inside the TextArea.
        /// </summary>
        public event EventHandler<TextEventArgs> TextPasted;

        event EventHandler ILogicalScrollable.ScrollInvalidated
        {
            add { if (_logicalScrollable != null) _logicalScrollable.ScrollInvalidated += value; }
            remove { if (_logicalScrollable != null) _logicalScrollable.ScrollInvalidated -= value; }
        }

        internal void OnTextCopied(TextEventArgs e)
        {
            TextCopied?.Invoke(this, e);
        }

        internal void OnTextPasted(TextEventArgs e)
        {
            TextPasted?.Invoke(this, e);
        }

        public IList<RoutedCommandBinding> CommandBindings { get; } = new List<RoutedCommandBinding>();

        bool ILogicalScrollable.IsLogicalScrollEnabled => _logicalScrollable?.IsLogicalScrollEnabled ?? default(bool);

        Size ILogicalScrollable.ScrollSize => _logicalScrollable?.ScrollSize ?? default(Size);

        Size ILogicalScrollable.PageScrollSize => _logicalScrollable?.PageScrollSize ?? default(Size);

        Size IScrollable.Extent => _logicalScrollable?.Extent ?? default(Size);

        Vector IScrollable.Offset
        {
            get => _logicalScrollable?.Offset ?? default(Vector);
            set
            {
                if (_logicalScrollable != null)
                {
                    _logicalScrollable.Offset = value;
                }
            }
        }

        Size IScrollable.Viewport => _logicalScrollable?.Viewport ?? default(Size);

        bool IScrollable.CanHorizontallyScroll
        {
            get => _logicalScrollable?.CanHorizontallyScroll ?? default(bool);
        }

        bool IScrollable.CanVerticallyScroll
        {
            get => _logicalScrollable?.CanVerticallyScroll ?? default(bool);
        }
        bool ILogicalScrollable.CanHorizontallyScroll
        {
            get => _logicalScrollable?.CanHorizontallyScroll ?? default(bool);
            set
            {
                if (_logicalScrollable != null)
                {
                    _logicalScrollable.CanHorizontallyScroll = value;
                }
            }
        }

        bool ILogicalScrollable.CanVerticallyScroll
        {
            get => _logicalScrollable?.CanVerticallyScroll ?? default(bool);
            set
            {
                if (_logicalScrollable != null)
                {
                    _logicalScrollable.CanVerticallyScroll = value;
                }
            }
        }

        public bool BringIntoView(Control target, Rect targetRect) =>
            _logicalScrollable?.BringIntoView(target, targetRect) ?? default(bool);

        Control ILogicalScrollable.GetControlInDirection(NavigationDirection direction, Control from)
            => _logicalScrollable?.GetControlInDirection(direction, from);

        public void RaiseScrollInvalidated(EventArgs e)
        {
            _logicalScrollable?.RaiseScrollInvalidated(e);
        }

        private class TextAreaTextInputMethodClient : TextInputMethodClient
        {
            private TextArea _textArea;

            public TextAreaTextInputMethodClient()
            {

            }

            public override Rect CursorRectangle
            {
                get
                {
                    if (_textArea == null)
                    {
                        return default;
                    }

                    var transform = _textArea.TextView.TransformToVisual(_textArea);

                    if (transform == null)
                    {
                        return default;
                    }

                    var rect = _textArea.Caret.CalculateCaretRectangle().TransformToAABB(transform.Value);

                    var scrollOffset = _textArea.TextView.ScrollOffset;

                    rect = rect.WithX(rect.X - scrollOffset.X).WithY(rect.Y - scrollOffset.Y);

                    return rect;
                }
            }

            public override Visual TextViewVisual => _textArea;

            public override bool SupportsPreedit => false;

            public override bool SupportsSurroundingText => true;

            public override string SurroundingText
            {
                get
                {
                    if (_textArea == null)
                    {
                        return default;
                    }

                    var lineIndex = _textArea.Caret.Line;

                    var documentLine = _textArea.Document.GetLineByNumber(lineIndex);

                    var text = _textArea.Document.GetText(documentLine.Offset, documentLine.Length);

                    return text;
                }
            }

            public override TextSelection Selection
            {
                get
                {
                    if (_textArea == null)
                        return new TextSelection(0, 0);
                    return new TextSelection(_textArea.Caret.Position.Column, _textArea.Caret.Position.Column + _textArea.Selection.Length);
                }
                set
                {
                    if (_textArea == null) return;
                    var selection = _textArea.Selection;
                    if (selection.StartPosition.Line == 0) return;

                    _textArea.Selection = selection.StartSelectionOrSetEndpoint(
                        new TextViewPosition(selection.StartPosition.Line, value.Start),
                        new TextViewPosition(selection.StartPosition.Line, value.End));
                }
            }

            public void SetTextArea(TextArea textArea)
            {
                if (_textArea != null)
                {
                    _textArea.Caret.PositionChanged -= Caret_PositionChanged;
                }

                _textArea = textArea;

                if (_textArea != null)
                {
                    _textArea.Caret.PositionChanged += Caret_PositionChanged;
                }

                RaiseTextViewVisualChanged();

                RaiseCursorRectangleChanged();

                RaiseSurroundingTextChanged();
            }

            private void Caret_PositionChanged(object sender, EventArgs e)
            {
                RaiseCursorRectangleChanged();
                RaiseSurroundingTextChanged();
                RaiseSelectionChanged();
            }

            public override void SetPreeditText(string text)
            {

            }
        }
    }

    /// <summary>
    /// EventArgs with text.
    /// </summary>
    public class TextEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a new TextEventArgs instance.
        /// </summary>
        public TextEventArgs(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}

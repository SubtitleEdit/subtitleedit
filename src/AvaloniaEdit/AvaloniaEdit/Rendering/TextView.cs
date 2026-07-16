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
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;

using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// A virtualizing panel producing+showing <see cref="VisualLine"/>s for a <see cref="TextDocument"/>.
    /// 
    /// This is the heart of the text editor, this class controls the text rendering process.
    /// 
    /// Taken as a standalone control, it's a text viewer without any editing capability.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
                                                     Justification = "The user usually doesn't work with TextView but with TextEditor; and nulling the Document property is sufficient to dispose everything.")]
    public class TextView : Control, ITextEditorComponent, ILogicalScrollable
    {
        private EventHandler _scrollInvalidated;

        #region Constructor
        static TextView()
        {
            ClipToBoundsProperty.OverrideDefaultValue<TextView>(true);
            FocusableProperty.OverrideDefaultValue<TextView>(false);
            OptionsProperty.Changed.Subscribe(OnOptionsChanged);

            DocumentProperty.Changed.Subscribe(OnDocumentChanged);

            // Re-layout when FlowDirection flips so RTL vs LTR line formatting is re-evaluated.
            FlowDirectionProperty.Changed.Subscribe(e => (e.Sender as TextView)?.Redraw());
        }

        private readonly ColumnRulerRenderer _columnRulerRenderer;
        private readonly CurrentLineHighlightRenderer _currentLineHighlightRenderer;
        private VisualLineElement _currentHoveredElement;

        /// <summary>
        /// Creates a new TextView instance.
        /// </summary>
        public TextView()
        {
            Services.AddService(this);

            TextLayer = new TextLayer(this);
            _elementGenerators = new ObserveAddRemoveCollection<VisualLineElementGenerator>(ElementGenerator_Added, ElementGenerator_Removed);
            _lineTransformers = new ObserveAddRemoveCollection<IVisualLineTransformer>(LineTransformer_Added, LineTransformer_Removed);
            _backgroundRenderers = new ObserveAddRemoveCollection<IBackgroundRenderer>(BackgroundRenderer_Added, BackgroundRenderer_Removed);
            _currentLineHighlightRenderer = new CurrentLineHighlightRenderer(this);
            _columnRulerRenderer = new ColumnRulerRenderer(this);
            Options = new TextEditorOptions();

            Debug.Assert(_singleCharacterElementGenerator != null); // assert that the option change created the builtin element generators

            Layers = new LayerCollection(this);
            InsertLayer(TextLayer, KnownLayer.Text, LayerInsertionPosition.Replace);

            _hoverLogic = new PointerHoverLogic(this);
            _hoverLogic.PointerHover += (sender, e) => RaiseHoverEventPair(e, PreviewPointerHoverEvent, PointerHoverEvent);
            _hoverLogic.PointerHoverStopped += (sender, e) => RaiseHoverEventPair(e, PreviewPointerHoverStoppedEvent, PointerHoverStoppedEvent);
        }

        #endregion

        #region Document Property
        /// <summary>
        /// Document property.
        /// </summary>
        public static readonly StyledProperty<TextDocument> DocumentProperty =
            AvaloniaProperty.Register<TextView, TextDocument>("Document");


        private TextDocument _document;
        private HeightTree _heightTree;

        /// <summary>
        /// Gets/Sets the document displayed by the text editor.
        /// </summary>
        public TextDocument Document
        {
            get => GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        internal double FontSize
        {
            get => GetValue(TemplatedControl.FontSizeProperty);
            set => SetValue(TemplatedControl.FontSizeProperty, value);
        }

        internal FontFamily FontFamily
        {
            get => GetValue(TemplatedControl.FontFamilyProperty);
            set => SetValue(TemplatedControl.FontFamilyProperty, value);
        }

        /// <summary>
        /// Occurs when the document property has changed.
        /// </summary>
        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        private static void OnDocumentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextView)?.OnDocumentChanged((TextDocument)e.OldValue, (TextDocument)e.NewValue);
        }

        private void OnDocumentChanged(TextDocument oldValue, TextDocument newValue)
        {
            if (oldValue != null)
            {
                _heightTree.Dispose();
                _heightTree = null;
                _formatter = null;
                CachedElements = null;
                TextDocumentWeakEventManager.Changing.RemoveHandler(oldValue, OnChanging);
            }
            _document = newValue;
            ClearScrollData();
            ClearVisualLines();
            if (newValue != null)
            {
                TextDocumentWeakEventManager.Changing.AddHandler(newValue, OnChanging);
                _formatter = TextFormatter.Current;
                InvalidateDefaultTextMetrics(); // measuring DefaultLineHeight depends on formatter
                _heightTree = new HeightTree(newValue, DefaultLineHeight);
                CachedElements = new TextViewCachedElements();
            }
            InvalidateMeasure();
            DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(oldValue, newValue));
        }

        private void RecreateCachedElements()
        {
            if (CachedElements != null)
            {
                CachedElements = new TextViewCachedElements();
            }
        }

        private void OnChanging(object sender, DocumentChangeEventArgs e)
        {
            Redraw(e.Offset, e.RemovalLength);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnOptionChanged(e);
        }

        #endregion

        #region Options property
        /// <summary>
        /// Options property.
        /// </summary>
        public static readonly StyledProperty<TextEditorOptions> OptionsProperty =
            AvaloniaProperty.Register<TextView, TextEditorOptions>("Options");

        /// <summary>
        /// Gets/Sets the options used by the text editor.
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

        /// <summary>
        /// Raises the <see cref="OptionChanged"/> event.
        /// </summary>
        protected virtual void OnOptionChanged(PropertyChangedEventArgs e)
        {
            OptionChanged?.Invoke(this, e);

            if (Options.ShowColumnRulers)
                _columnRulerRenderer.SetRuler(Options.ColumnRulerPositions, ColumnRulerPen);
            else
                _columnRulerRenderer.SetRuler(null, ColumnRulerPen);

            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(TextEditorOptions.LineHeightFactor))
                InvalidateDefaultTextMetrics();

            UpdateBuiltinElementGeneratorsFromOptions();
            Redraw();
        }

        private static void OnOptionsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextView)?.OnOptionsChanged((TextEditorOptions)e.OldValue, (TextEditorOptions)e.NewValue);
        }


        private void OnOptionsChanged(TextEditorOptions oldValue, TextEditorOptions newValue)
        {
            if (oldValue != null)
            {
                PropertyChangedWeakEventManager.RemoveHandler(oldValue, OnPropertyChanged);
            }
            if (newValue != null)
            {
                PropertyChangedWeakEventManager.AddHandler(newValue, OnPropertyChanged);
            }
            OnOptionChanged(new PropertyChangedEventArgs(null));
        }
        #endregion

        #region ElementGenerators+LineTransformers Properties

        private readonly ObserveAddRemoveCollection<VisualLineElementGenerator> _elementGenerators;

        /// <summary>
        /// Gets a collection where element generators can be registered.
        /// </summary>
        public IList<VisualLineElementGenerator> ElementGenerators => _elementGenerators;

        private void ElementGenerator_Added(VisualLineElementGenerator generator)
        {
            ConnectToTextView(generator);
            Redraw();
        }

        private void ElementGenerator_Removed(VisualLineElementGenerator generator)
        {
            DisconnectFromTextView(generator);
            Redraw();
        }

        private readonly ObserveAddRemoveCollection<IVisualLineTransformer> _lineTransformers;

        /// <summary>
        /// Gets a collection where line transformers can be registered.
        /// </summary>
        public IList<IVisualLineTransformer> LineTransformers => _lineTransformers;

        private void LineTransformer_Added(IVisualLineTransformer lineTransformer)
        {
            ConnectToTextView(lineTransformer);
            Redraw();
        }

        private void LineTransformer_Removed(IVisualLineTransformer lineTransformer)
        {
            DisconnectFromTextView(lineTransformer);
            Redraw();
        }
        #endregion

        #region Builtin ElementGenerators
        //		NewLineElementGenerator newLineElementGenerator;
        private SingleCharacterElementGenerator _singleCharacterElementGenerator;

        private LinkElementGenerator _linkElementGenerator;
        private MailLinkElementGenerator _mailLinkElementGenerator;

        private void UpdateBuiltinElementGeneratorsFromOptions()
        {
            var options = Options;

            //			AddRemoveDefaultElementGeneratorOnDemand(ref newLineElementGenerator, options.ShowEndOfLine);
            AddRemoveDefaultElementGeneratorOnDemand(ref _singleCharacterElementGenerator, options.ShowBoxForControlCharacters || options.ShowSpaces || options.ShowTabs);
            AddRemoveDefaultElementGeneratorOnDemand(ref _linkElementGenerator, options.EnableHyperlinks);
            AddRemoveDefaultElementGeneratorOnDemand(ref _mailLinkElementGenerator, options.EnableEmailHyperlinks);
        }

        private void AddRemoveDefaultElementGeneratorOnDemand<T>(ref T generator, bool demand)
            where T : VisualLineElementGenerator, IBuiltinElementGenerator, new()
        {
            var hasGenerator = generator != null;
            if (hasGenerator != demand)
            {
                if (demand)
                {
                    generator = new T();
                    ElementGenerators.Add(generator);
                }
                else
                {
                    ElementGenerators.Remove(generator);
                    generator = null;
                }
            }
            generator?.FetchOptions(Options);
        }
        #endregion

        #region Layers
        internal readonly TextLayer TextLayer;

        /// <summary>
        /// Gets the list of layers displayed in the text view.
        /// </summary>
        public LayerCollection Layers { get; }

        public sealed class LayerCollection : Collection<Control>
        {
            private readonly TextView _textView;

            public LayerCollection(TextView textView)
            {
                _textView = textView;
            }

            protected override void ClearItems()
            {
                foreach (var control in Items)
                {
                    _textView.VisualChildren.Remove(control);
                }
                base.ClearItems();
                _textView.LayersChanged();
            }

            protected override void InsertItem(int index, Control item)
            {
                base.InsertItem(index, item);
                _textView.VisualChildren.Insert(index, item);
                _textView.LayersChanged();
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                _textView.VisualChildren.RemoveAt(index);
                _textView.LayersChanged();
            }

            protected override void SetItem(int index, Control item)
            {
                _textView.VisualChildren.Remove(Items[index]);
                base.SetItem(index, item);
                _textView.VisualChildren.Add(item);
                _textView.LayersChanged();
            }
        }

        private void LayersChanged()
        {
            TextLayer.Index = Layers.IndexOf(TextLayer);
        }

        /// <summary>
        /// Inserts a new layer at a position specified relative to an existing layer.
        /// </summary>
        /// <param name="layer">The new layer to insert.</param>
        /// <param name="referencedLayer">The existing layer</param>
        /// <param name="position">Specifies whether the layer is inserted above,below, or replaces the referenced layer</param>
        public void InsertLayer(Control layer, KnownLayer referencedLayer, LayerInsertionPosition position)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            if (!Enum.IsDefined(typeof(KnownLayer), referencedLayer))
                throw new ArgumentOutOfRangeException(nameof(referencedLayer), (int)referencedLayer, nameof(KnownLayer));
            if (!Enum.IsDefined(typeof(LayerInsertionPosition), position))
                throw new ArgumentOutOfRangeException(nameof(position), (int)position, nameof(LayerInsertionPosition));
            if (referencedLayer == KnownLayer.Background && position != LayerInsertionPosition.Above)
                throw new InvalidOperationException("Cannot replace or insert below the background layer.");

            var newPosition = new LayerPosition(referencedLayer, position);
            LayerPosition.SetLayerPosition(layer, newPosition);
            for (var i = 0; i < Layers.Count; i++)
            {
                var p = LayerPosition.GetLayerPosition(Layers[i]);
                if (p != null)
                {
                    if (p.KnownLayer == referencedLayer && p.Position == LayerInsertionPosition.Replace)
                    {
                        // found the referenced layer
                        switch (position)
                        {
                            case LayerInsertionPosition.Below:
                                Layers.Insert(i, layer);
                                return;
                            case LayerInsertionPosition.Above:
                                Layers.Insert(i + 1, layer);
                                return;
                            case LayerInsertionPosition.Replace:
                                Layers[i] = layer;
                                return;
                        }
                    }
                    else if (p.KnownLayer == referencedLayer && p.Position == LayerInsertionPosition.Above
                             || p.KnownLayer > referencedLayer)
                    {
                        // we skipped the insertion position (referenced layer does not exist?)
                        Layers.Insert(i, layer);
                        return;
                    }
                }
            }
            // inserting after all existing layers:
            Layers.Add(layer);
        }

        #endregion

        #region Inline object handling

        private readonly List<InlineObjectRun> _inlineObjects = new List<InlineObjectRun>();

        /// <summary>
        /// Adds a new inline object.
        /// </summary>
        internal void AddInlineObject(InlineObjectRun inlineObject)
        {
            Debug.Assert(inlineObject.VisualLine != null);

            // Remove inline object if its already added, can happen e.g. when recreating textrun for word-wrapping
            var alreadyAdded = false;
            for (var i = 0; i < _inlineObjects.Count; i++)
            {
                if (_inlineObjects[i].Element == inlineObject.Element)
                {
                    RemoveInlineObjectRun(_inlineObjects[i], true);
                    _inlineObjects.RemoveAt(i);
                    alreadyAdded = true;
                    break;
                }
            }

            _inlineObjects.Add(inlineObject);
            if (!alreadyAdded)
            {
                VisualChildren.Add(inlineObject.Element);
                ((ISetLogicalParent)inlineObject.Element).SetParent(this);
            }
            inlineObject.Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            inlineObject.DesiredSize = inlineObject.Element.DesiredSize;
        }

        private void MeasureInlineObjects()
        {
            // As part of MeasureOverride(), re-measure the inline objects
            foreach (var inlineObject in _inlineObjects)
            {
                if (inlineObject.VisualLine.IsDisposed)
                {
                    // Don't re-measure inline objects that are going to be removed anyways.
                    // If the inline object will be reused in a different VisualLine, we'll measure it in the AddInlineObject() call.
                    continue;
                }
                inlineObject.Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (!inlineObject.Element.DesiredSize.IsClose(inlineObject.DesiredSize))
                {
                    // the element changed size -> recreate its parent visual line
                    inlineObject.DesiredSize = inlineObject.Element.DesiredSize;
                    if (_allVisualLines.Remove(inlineObject.VisualLine))
                    {
                        DisposeVisualLine(inlineObject.VisualLine);
                    }
                }
            }
        }

        private readonly List<VisualLine> _visualLinesWithOutstandingInlineObjects = new List<VisualLine>();

        private void RemoveInlineObjects(VisualLine visualLine)
        {
            // Delay removing inline objects:
            // A document change immediately invalidates affected visual lines, but it does not
            // cause an immediate redraw.
            // To prevent inline objects from flickering when they are recreated, we delay removing
            // inline objects until the next redraw.
            if (visualLine.HasInlineObjects)
            {
                _visualLinesWithOutstandingInlineObjects.Add(visualLine);
            }
        }

        /// <summary>
        /// Remove the inline objects that were marked for removal.
        /// </summary>
        private void RemoveInlineObjectsNow()
        {
            if (_visualLinesWithOutstandingInlineObjects.Count == 0)
                return;
            _inlineObjects.RemoveAll(
                ior =>
                {
                    if (_visualLinesWithOutstandingInlineObjects.Contains(ior.VisualLine))
                    {
                        RemoveInlineObjectRun(ior, false);
                        return true;
                    }
                    return false;
                });
            _visualLinesWithOutstandingInlineObjects.Clear();
        }

        // Remove InlineObjectRun.Element from TextLayer.
        // Caller of RemoveInlineObjectRun will remove it from inlineObjects collection.
        private void RemoveInlineObjectRun(InlineObjectRun ior, bool keepElement)
        {
            if (!keepElement && ior.Element.IsKeyboardFocusWithin)
            {
                // When the inline element that has the focus is removed, it will reset the
                // focus to the main window without raising appropriate LostKeyboardFocus events.
                // To work around this, we manually set focus to the next focusable parent.
                Control element = this;
                while (element != null && !element.Focusable)
                {
                    element = element.GetVisualParent() as Control;
                }
                element?.Focus();
            }
            ior.VisualLine = null;
            if (!keepElement)
                VisualChildren.Remove(ior.Element);
        }
        #endregion

        #region Brushes
        /// <summary>
        /// NonPrintableCharacterBrush dependency property.
        /// </summary>
        public static readonly StyledProperty<IBrush> NonPrintableCharacterBrushProperty =
            AvaloniaProperty.Register<TextView, IBrush>("NonPrintableCharacterBrush", new SolidColorBrush(Color.FromArgb(145, 128, 128, 128)));

        /// <summary>
        /// Gets/sets the Brush used for displaying non-printable characters.
        /// </summary>
        public IBrush NonPrintableCharacterBrush
        {
            get => GetValue(NonPrintableCharacterBrushProperty);
            set => SetValue(NonPrintableCharacterBrushProperty, value);
        }

        /// <summary>
        /// LinkTextForegroundBrush dependency property.
        /// </summary>
        public static readonly StyledProperty<IBrush> LinkTextForegroundBrushProperty =
            AvaloniaProperty.Register<TextView, IBrush>("LinkTextForegroundBrush", Brushes.Blue);

        /// <summary>
        /// Gets/sets the Brush used for displaying link texts.
        /// </summary>
        public IBrush LinkTextForegroundBrush
        {
            get => GetValue(LinkTextForegroundBrushProperty);
            set => SetValue(LinkTextForegroundBrushProperty, value);
        }

        /// <summary>
        /// LinkTextBackgroundBrush dependency property.
        /// </summary>
        public static readonly StyledProperty<IBrush> LinkTextBackgroundBrushProperty =
            AvaloniaProperty.Register<TextView, IBrush>("LinkTextBackgroundBrush", Brushes.Transparent);

        /// <summary>
        /// Gets/sets the Brush used for the background of link texts.
        /// </summary>
        public IBrush LinkTextBackgroundBrush
        {
            get => GetValue(LinkTextBackgroundBrushProperty);
            set => SetValue(LinkTextBackgroundBrushProperty, value);
        }
        #endregion

        /// <summary>
        /// LinkTextUnderlinedBrush dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> LinkTextUnderlineProperty =
            AvaloniaProperty.Register<TextView, bool>(nameof(LinkTextUnderline), true);

        /// <summary>
        /// Gets/sets whether to underline link texts.
        /// </summary>
        /// <remarks>
        /// Note that when setting this property to false, link text remains clickable and the LinkTextForegroundBrush (if any) is still applied.
        /// Set TextEditorOptions.EnableHyperlinks and EnableEmailHyperlinks to false to disable links completely.
        /// </remarks>
        public bool LinkTextUnderline
        {
            get => GetValue(LinkTextUnderlineProperty);
            set => SetValue(LinkTextUnderlineProperty, value);
        }

        #region Redraw methods / VisualLine invalidation
        /// <summary>
        /// Causes the text editor to regenerate all visual lines.
        /// </summary>
        public void Redraw()
        {
            VerifyAccess();
            ClearVisualLines();
            InvalidateMeasure();
        }

        /// <summary>
        /// Causes the text editor to regenerate the specified visual line.
        /// </summary>
        public void Redraw(VisualLine visualLine)
        {
            VerifyAccess();
            if (_allVisualLines.Remove(visualLine))
            {
                DisposeVisualLine(visualLine);
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Causes the text editor to redraw all lines overlapping with the specified segment.
        /// </summary>
        public void Redraw(int offset, int length)
        {
            VerifyAccess();
            var changedSomethingBeforeOrInLine = false;
            for (var i = 0; i < _allVisualLines.Count; i++)
            {
                var visualLine = _allVisualLines[i];
                var lineStart = visualLine.FirstDocumentLine.Offset;
                var lineEnd = visualLine.LastDocumentLine.Offset + visualLine.LastDocumentLine.TotalLength;
                if (offset <= lineEnd)
                {
                    changedSomethingBeforeOrInLine = true;

                    if (offset + length >= lineStart)
                    {
                        _allVisualLines.RemoveAt(i--);
                        DisposeVisualLine(visualLine);
                    }
                }
            }

            if (changedSomethingBeforeOrInLine)
            {
                // Repaint not only when something in visible area was changed, but also when anything in front of it
                // was changed. We might have to redraw the line number margin. Or the highlighting changed.
                // However, we'll try to reuse the existing VisualLines.
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Causes a known layer to redraw.
        /// This method does not invalidate visual lines;
        /// use the <see cref="Redraw()"/> method to do that.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "knownLayer",
                                                         Justification = "This method is meant to invalidate only a specific layer - I just haven't figured out how to do that, yet.")]
        public void InvalidateLayer(KnownLayer knownLayer)
        {
            InvalidateMeasure();
        }

        /// <summary>
        /// Causes the text editor to redraw all lines overlapping with the specified segment.
        /// Does nothing if segment is null.
        /// </summary>
        public void Redraw(ISegment segment)
        {
            if (segment != null)
            {
                Redraw(segment.Offset, segment.Length);
            }
        }

        /// <summary>
        /// Invalidates all visual lines.
        /// The caller of ClearVisualLines() must also call InvalidateMeasure() to ensure
        /// that the visual lines will be recreated.
        /// </summary>
        private void ClearVisualLines()
        {
            _visibleVisualLines = null;
            if (_allVisualLines.Count != 0)
            {
                foreach (var visualLine in _allVisualLines)
                {
                    DisposeVisualLine(visualLine);
                }
                _allVisualLines.Clear();
            }
        }

        private void DisposeVisualLine(VisualLine visualLine)
        {
            if (_newVisualLines != null && _newVisualLines.Contains(visualLine))
            {
                throw new ArgumentException("Cannot dispose visual line because it is in construction!");
            }

            _visibleVisualLines = null;
            visualLine.Dispose();
            RemoveInlineObjects(visualLine);
        }
        #endregion

        #region Get(OrConstruct)VisualLine
        /// <summary>
        /// Gets the visual line that contains the document line with the specified number.
        /// Returns null if the document line is outside the visible range.
        /// </summary>
        public VisualLine GetVisualLine(int documentLineNumber)
        {
            // TODO: EnsureVisualLines() ?
            foreach (var visualLine in _allVisualLines)
            {
                Debug.Assert(visualLine.IsDisposed == false);
                var start = visualLine.FirstDocumentLine.LineNumber;
                var end = visualLine.LastDocumentLine.LineNumber;
                if (documentLineNumber >= start && documentLineNumber <= end)
                    return visualLine;
            }
            return null;
        }

        /// <summary>
        /// Gets the visual line that contains the document line with the specified number.
        /// If that line is outside the visible range, a new VisualLine for that document line is constructed.
        /// </summary>
        public VisualLine GetOrConstructVisualLine(DocumentLine documentLine)
        {
            if (documentLine == null)
                throw new ArgumentNullException("documentLine");
            if (!this.Document.Lines.Contains(documentLine))
                throw new InvalidOperationException("Line belongs to wrong document");
            VerifyAccess();

            VisualLine l = GetVisualLine(documentLine.LineNumber);
            if (l == null)
            {
                TextRunProperties globalTextRunProperties = CreateGlobalTextRunProperties();
                VisualLineTextParagraphProperties paragraphProperties = CreateParagraphProperties(globalTextRunProperties);

                while (_heightTree.GetIsCollapsed(documentLine.LineNumber))
                {
                    documentLine = documentLine.PreviousLine;
                }

                l = BuildVisualLine(documentLine,
                                    globalTextRunProperties, paragraphProperties,
                                    _elementGenerators.ToArray(), _lineTransformers.ToArray(),
                                    _lastAvailableSize);
                _allVisualLines.Add(l);
                // update all visual top values (building the line might have changed visual top of other lines due to word wrapping)
                foreach (var line in _allVisualLines)
                {
                    line.VisualTop = _heightTree.GetVisualPosition(line.FirstDocumentLine);
                }
            }
            return l;
        }
        #endregion

        #region Visual Lines (fields and properties)

        private List<VisualLine> _allVisualLines = new List<VisualLine>();
        private ReadOnlyCollection<VisualLine> _visibleVisualLines;
        private double _clippedPixelsOnTop;
        private List<VisualLine> _newVisualLines;

        /// <summary>
        /// Gets the currently visible visual lines.
        /// </summary>
        /// <exception cref="VisualLinesInvalidException">
        /// Gets thrown if there are invalid visual lines when this property is accessed.
        /// You can use the <see cref="VisualLinesValid"/> property to check for this case,
        /// or use the <see cref="EnsureVisualLines()"/> method to force creating the visual lines
        /// when they are invalid.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public ReadOnlyCollection<VisualLine> VisualLines
        {
            get
            {
                if (_visibleVisualLines == null)
                    throw new VisualLinesInvalidException();
                return _visibleVisualLines;
            }
        }

        /// <summary>
        /// Gets whether the visual lines are valid.
        /// Will return false after a call to Redraw().
        /// Accessing the visual lines property will cause a <see cref="VisualLinesInvalidException"/>
        /// if this property is <c>false</c>.
        /// </summary>
        public bool VisualLinesValid => _visibleVisualLines != null;

        /// <summary>
        /// Occurs when the TextView is about to be measured and will regenerate its visual lines.
        /// This event may be used to mark visual lines as invalid that would otherwise be reused.
        /// </summary>
        public event EventHandler<VisualLineConstructionStartEventArgs> VisualLineConstructionStarting;

        /// <summary>
        /// Occurs when the TextView was measured and changed its visual lines.
        /// </summary>
        public event EventHandler VisualLinesChanged;

        /// <summary>
        /// If the visual lines are invalid, creates new visual lines for the visible part
        /// of the document.
        /// If all visual lines are valid, this method does nothing.
        /// </summary>
        /// <exception cref="InvalidOperationException">The visual line build process is already running.
        /// It is not allowed to call this method during the construction of a visual line.</exception>
        public void EnsureVisualLines()
        {
            Dispatcher.UIThread.VerifyAccess();
            if (_inMeasure)
                throw new InvalidOperationException("The visual line build process is already running! Cannot EnsureVisualLines() during Measure!");
            if (!VisualLinesValid)
            {
                // increase priority for re-measure
                InvalidateMeasure();
                // force immediate re-measure
                InvalidateVisual();
            }
            // Sometimes we still have invalid lines after UpdateLayout - work around the problem
            // by calling MeasureOverride directly.
            if (!VisualLinesValid)
            {
                Debug.WriteLine("UpdateLayout() failed in EnsureVisualLines");
                MeasureOverride(_lastAvailableSize);
            }
            if (!VisualLinesValid)
                throw new VisualLinesInvalidException("Internal error: visual lines invalid after EnsureVisualLines call");
        }
        #endregion

        #region Measure
        /// <summary>
        /// Additonal amount that allows horizontal scrolling past the end of the longest line.
        /// This is necessary to ensure the caret always is visible, even when it is at the end of the longest line.
        /// </summary>
        private const double AdditionalHorizontalScrollAmount = 3;

        private Size _lastAvailableSize;
        private bool _inMeasure;

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            // We don't support infinite available width, so we'll limit it to 32000 pixels.
            if (availableSize.Width > 32000)
                availableSize = availableSize.WithWidth(32000);

            if (!_canHorizontallyScroll && !availableSize.Width.IsClose(_lastAvailableSize.Width))
            {
                ClearVisualLines();
            }

            _lastAvailableSize = availableSize;

            foreach (var layer in Layers)
            {
                layer.Measure(availableSize);
            }

            InvalidateVisual(); // = InvalidateArrange+InvalidateRender

            MeasureInlineObjects();

            double maxWidth;
            if (_document == null)
            {
                // no document -> create empty list of lines
                _allVisualLines = new List<VisualLine>();
                _visibleVisualLines = new ReadOnlyCollection<VisualLine>(_allVisualLines.ToArray());
                maxWidth = 0;
            }
            else
            {
                _inMeasure = true;
                try
                {
                    maxWidth = CreateAndMeasureVisualLines(availableSize);
                }
                finally
                {
                    _inMeasure = false;
                }
            }

            // remove inline objects only at the end, so that inline objects that were re-used are not removed from the editor
            RemoveInlineObjectsNow();

            maxWidth += AdditionalHorizontalScrollAmount;
            var heightTreeHeight = DocumentHeight;
            var options = Options;
            double desiredHeight = Math.Min(availableSize.Height, heightTreeHeight);
            double extraHeightToAllowScrollBelowDocument = 0;
            if (options.AllowScrollBelowDocument)
            {
                if (!double.IsInfinity(_scrollViewport.Height))
                {
                    // HACK: we need to keep at least Caret.MinimumDistanceToViewBorder visible so that we don't scroll back up when the user types after
                    // scrolling to the very bottom.
                    var minVisibleDocumentHeight = DefaultLineHeight;
                    // increase the extend height to allow scrolling below the document
                    extraHeightToAllowScrollBelowDocument = desiredHeight - minVisibleDocumentHeight;
                }
            }

            TextLayer.SetVisualLines(_visibleVisualLines);

            SetScrollData(availableSize,
                          new Size(maxWidth, heightTreeHeight + extraHeightToAllowScrollBelowDocument),
                          _scrollOffset);

            VisualLinesChanged?.Invoke(this, EventArgs.Empty);

            return new Size(Math.Min(availableSize.Width, maxWidth), desiredHeight);
        }

        /// <summary>
        /// Build all VisualLines in the visible range.
        /// </summary>
        /// <returns>Width the longest line</returns>
        private double CreateAndMeasureVisualLines(Size availableSize)
        {
            TextRunProperties globalTextRunProperties = CreateGlobalTextRunProperties();
            VisualLineTextParagraphProperties paragraphProperties = CreateParagraphProperties(globalTextRunProperties);

            //Debug.WriteLine("Measure availableSize=" + availableSize + ", scrollOffset=" + _scrollOffset);
            var firstLineInView = _heightTree.GetLineByVisualPosition(_scrollOffset.Y);

            // number of pixels clipped from the first visual line(s)
            _clippedPixelsOnTop = _scrollOffset.Y - _heightTree.GetVisualPosition(firstLineInView);
            // clippedPixelsOnTop should be >= 0, except for floating point inaccurracy.
            Debug.Assert(_clippedPixelsOnTop >= -ExtensionMethods.Epsilon);

            _newVisualLines = new List<VisualLine>();

            VisualLineConstructionStarting?.Invoke(this, new VisualLineConstructionStartEventArgs(firstLineInView));

            var elementGeneratorsArray = _elementGenerators.ToArray();
            var lineTransformersArray = _lineTransformers.ToArray();
            var nextLine = firstLineInView;
            double maxWidth = 0;
            var yPos = -_clippedPixelsOnTop;
            while (yPos < availableSize.Height && nextLine != null)
            {
                var visualLine = GetVisualLine(nextLine.LineNumber) ??
                                        BuildVisualLine(nextLine,
                                            globalTextRunProperties, paragraphProperties,
                                            elementGeneratorsArray, lineTransformersArray,
                                            availableSize);

                visualLine.VisualTop = _scrollOffset.Y + yPos;

                nextLine = visualLine.LastDocumentLine.NextLine;

                yPos += visualLine.Height;

                foreach (var textLine in visualLine.TextLines)
                {
                    if (textLine.WidthIncludingTrailingWhitespace > maxWidth)
                        maxWidth = textLine.WidthIncludingTrailingWhitespace;
                }

                _newVisualLines.Add(visualLine);
            }

            foreach (var line in _allVisualLines)
            {
                Debug.Assert(line.IsDisposed == false);
                if (!_newVisualLines.Contains(line))
                    DisposeVisualLine(line);
            }

            _allVisualLines = _newVisualLines;
            // visibleVisualLines = readonly copy of visual lines
            _visibleVisualLines = new ReadOnlyCollection<VisualLine>(_newVisualLines.ToArray());
            _newVisualLines = null;

            if (_allVisualLines.Any(line => line.IsDisposed))
            {
                throw new InvalidOperationException("A visual line was disposed even though it is still in use.\n" +
                                                    "This can happen when Redraw() is called during measure for lines " +
                                                    "that are already constructed.");
            }
            return maxWidth;
        }
        #endregion

        #region BuildVisualLine

        private TextFormatter _formatter;
        internal TextViewCachedElements CachedElements;

        private TextRunProperties CreateGlobalTextRunProperties()
        {
            var p = new GlobalTextRunProperties();
            p.typeface = this.CreateTypeface();
            p.fontRenderingEmSize = FontSize;
            p.foregroundBrush = GetValue(TextElement.ForegroundProperty);
            ExtensionMethods.CheckIsFrozen(p.foregroundBrush);
            p.cultureInfo = CultureInfo.CurrentCulture;
            return p;
        }

        private VisualLineTextParagraphProperties CreateParagraphProperties(TextRunProperties defaultTextRunProperties)
        {
            return new VisualLineTextParagraphProperties
            {
                defaultTextRunProperties = defaultTextRunProperties,
                textWrapping = _canHorizontallyScroll ? TextWrapping.NoWrap : TextWrapping.Wrap,
                tabSize = Options.IndentationSize * WideSpaceWidth,
                flowDirection = FlowDirection
            };
        }

        private VisualLine BuildVisualLine(DocumentLine documentLine,
                                   TextRunProperties globalTextRunProperties,
                                   VisualLineTextParagraphProperties paragraphProperties,
                                   IReadOnlyList<VisualLineElementGenerator> elementGeneratorsArray,
                                   IReadOnlyList<IVisualLineTransformer> lineTransformersArray,
                                   Size availableSize)
        {
            if (_heightTree.GetIsCollapsed(documentLine.LineNumber))
                throw new InvalidOperationException("Trying to build visual line from collapsed line");

            //Debug.WriteLine("Building line " + documentLine.LineNumber);

            VisualLine visualLine = new VisualLine(this, documentLine);
            VisualLineTextSource textSource = new VisualLineTextSource(visualLine)
            {
                Document = _document,
                GlobalTextRunProperties = globalTextRunProperties,
                TextView = this
            };

            visualLine.ConstructVisualElements(textSource, elementGeneratorsArray);

            if (visualLine.FirstDocumentLine != visualLine.LastDocumentLine)
            {
                // Check whether the lines are collapsed correctly:
                double firstLinePos = _heightTree.GetVisualPosition(visualLine.FirstDocumentLine.NextLine);
                double lastLinePos = _heightTree.GetVisualPosition(visualLine.LastDocumentLine.NextLine ?? visualLine.LastDocumentLine);
                if (!firstLinePos.IsClose(lastLinePos))
                {
                    for (int i = visualLine.FirstDocumentLine.LineNumber + 1; i <= visualLine.LastDocumentLine.LineNumber; i++)
                    {
                        if (!_heightTree.GetIsCollapsed(i))
                            throw new InvalidOperationException("Line " + i + " was skipped by a VisualLineElementGenerator, but it is not collapsed.");
                    }
                    throw new InvalidOperationException("All lines collapsed but visual pos different - height tree inconsistency?");
                }
            }

            visualLine.RunTransformers(textSource, lineTransformersArray);

            // now construct textLines:
            TextLineBreak lastLineBreak = null;
            var textOffset = 0;
            var textLines = new List<TextLine>();

            while (textOffset <= visualLine.VisualLengthWithEndOfLineMarker)
            {
                var textLine = _formatter.FormatLine(
                    textSource,
                    textOffset,
                    availableSize.Width,
                    paragraphProperties,
                    lastLineBreak
                );

                textLines.Add(textLine);
                textOffset += textLine.Length;

                // exit loop so that we don't do the indentation calculation if there's only a single line
                if (textOffset >= visualLine.VisualLengthWithEndOfLineMarker)
                    break;

                if (paragraphProperties.firstLineInParagraph)
                {
                    paragraphProperties.firstLineInParagraph = false;

                    TextEditorOptions options = this.Options;
                    double indentation = 0;
                    if (options.InheritWordWrapIndentation)
                    {
                        // determine indentation for next line:
                        int indentVisualColumn = GetIndentationVisualColumn(visualLine);
                        if (indentVisualColumn > 0 && indentVisualColumn < textOffset)
                        {
                            indentation = textLine.GetDistanceFromCharacterHit(new CharacterHit(indentVisualColumn, 0));
                        }
                    }
                    indentation += options.WordWrapIndentation;
                    // apply the calculated indentation unless it's more than half of the text editor size:
                    if (indentation > 0 && indentation * 2 < availableSize.Width)
                        paragraphProperties.indent = indentation;
                }

                lastLineBreak = textLine.TextLineBreak;
            }
            visualLine.SetTextLines(textLines);
            _heightTree.SetHeight(visualLine.FirstDocumentLine, visualLine.Height);
            return visualLine;
        }

        private static int GetIndentationVisualColumn(VisualLine visualLine)
        {
            if (visualLine.Elements.Count == 0)
                return 0;
            var column = 0;
            var elementIndex = 0;
            var element = visualLine.Elements[elementIndex];
            while (element.IsWhitespace(column))
            {
                column++;
                if (column == element.VisualColumn + element.VisualLength)
                {
                    elementIndex++;
                    if (elementIndex == visualLine.Elements.Count)
                        break;
                    element = visualLine.Elements[elementIndex];
                }
            }
            return column;
        }
        #endregion

        #region Arrange
        /// <summary>
        /// Arrange implementation.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureVisualLines();

            foreach (var layer in Layers)
            {
                layer.Arrange(new Rect(new Point(0, 0), finalSize));
            }

            if (_document == null || _allVisualLines.Count == 0)
                return finalSize;

            // validate scroll position
            var newScrollOffsetX = _scrollOffset.X;
            var newScrollOffsetY = _scrollOffset.Y;
            if (_scrollOffset.X + finalSize.Width > _scrollExtent.Width)
            {
                newScrollOffsetX = Math.Max(0, _scrollExtent.Width - finalSize.Width);
            }
            if (_scrollOffset.Y + finalSize.Height > _scrollExtent.Height)
            {
                newScrollOffsetY = Math.Max(0, _scrollExtent.Height - finalSize.Height);
            }

            // Apply final view port and offset
            if (SetScrollData(_scrollViewport, _scrollExtent, new Vector(newScrollOffsetX, newScrollOffsetY)))
                InvalidateMeasure();

            if (_visibleVisualLines != null)
            {
                var pos = new Point(-_scrollOffset.X, -_clippedPixelsOnTop);
                var defaultLineHeight = _defaultLineHeight;
                foreach (var visualLine in _visibleVisualLines)
                {
                    var offset = 0;
                    foreach (var textLine in visualLine.TextLines)
                    {
                        foreach (var span in textLine.TextRuns)
                        {
                            var inline = span as InlineObjectRun;

                            if (inline?.VisualLine != null)
                            {
                                Debug.Assert(_inlineObjects.Contains(inline));

                                var desiredSize = inline.Element.DesiredSize;
                                var x = pos.X + textLine.GetDistanceFromCharacterHit(new CharacterHit(offset));
                                var y = pos.Y;
                                var width = desiredSize.Width;
                                var height = Math.Max(desiredSize.Height, defaultLineHeight);
                                inline.Element.Arrange(new Rect(x, y, width, height));
                            }

                            offset += span.Length;
                        }

                        var lineHeight = Math.Max(textLine.Height, defaultLineHeight);
                        pos = new Point(pos.X, pos.Y + lineHeight);
                    }
                }
            }

            InvalidateCursorIfPointerWithinTextView();

            return finalSize;
        }
        #endregion

        #region Render

        private readonly ObserveAddRemoveCollection<IBackgroundRenderer> _backgroundRenderers;

        /// <summary>
        /// Gets the list of background renderers.
        /// </summary>
        public IList<IBackgroundRenderer> BackgroundRenderers => _backgroundRenderers;

        private void BackgroundRenderer_Added(IBackgroundRenderer renderer)
        {
            ConnectToTextView(renderer);
            InvalidateLayer(renderer.Layer);
        }

        private void BackgroundRenderer_Removed(IBackgroundRenderer renderer)
        {
            DisconnectFromTextView(renderer);
            InvalidateLayer(renderer.Layer);
        }

        /// <inheritdoc/>
        public override void Render(DrawingContext drawingContext)
        {
            if (!VisualLinesValid)
            {
                return;
            }

            RenderBackground(drawingContext, KnownLayer.Background);
            foreach (var line in _visibleVisualLines)
            {
                IBrush currentBrush = null;
                var startVc = 0;
                var length = 0;
                foreach (var element in line.Elements)
                {
                    if (currentBrush == null || !currentBrush.Equals(element.BackgroundBrush))
                    {
                        if (currentBrush != null)
                        {
                            var builder =
                                new BackgroundGeometryBuilder
                                {
                                    AlignToWholePixels = true,
                                    CornerRadius = 3
                                };
                            foreach (var rect in BackgroundGeometryBuilder.GetRectsFromVisualSegment(this, line, startVc, startVc + length))
                                builder.AddRectangle(this, rect);
                            var geometry = builder.CreateGeometry();
                            if (geometry != null)
                            {
                                drawingContext.DrawGeometry(currentBrush, null, geometry);
                            }
                        }
                        startVc = element.VisualColumn;
                        length = element.DocumentLength;
                        currentBrush = element.BackgroundBrush;
                    }
                    else
                    {
                        length += element.VisualLength;
                    }
                }
                if (currentBrush != null)
                {
                    var builder = new BackgroundGeometryBuilder
                    {
                        AlignToWholePixels = true,
                        CornerRadius = 3
                    };
                    foreach (var rect in BackgroundGeometryBuilder.GetRectsFromVisualSegment(this, line, startVc, startVc + length))
                        builder.AddRectangle(this, rect);
                    var geometry = builder.CreateGeometry();
                    if (geometry != null)
                    {
                        drawingContext.DrawGeometry(currentBrush, null, geometry);
                    }
                }
            }
        }

        internal void RenderBackground(DrawingContext drawingContext, KnownLayer layer)
        {
            // this is necessary so hit-testing works properly and events get tunneled to the TextView.
            drawingContext.FillRectangle(Brushes.Transparent, Bounds);
            foreach (var bg in _backgroundRenderers)
            {
                if (bg.Layer == layer)
                {
                    bg.Draw(this, drawingContext);
                }
            }
        }

        internal void ArrangeTextLayer(IList<VisualLineDrawingVisual> visuals)
        {
            var pos = new Point(-_scrollOffset.X, -_clippedPixelsOnTop);
            foreach (var visual in visuals)
            {
                var t = visual.RenderTransform as TranslateTransform;
                if (t == null || t.X != pos.X || t.Y != pos.Y)
                {
                    visual.RenderTransform = new TranslateTransform(pos.X, pos.Y);
                }
                pos = new Point(pos.X, pos.Y + visual.LineHeight);
            }
        }
        #endregion

        #region IScrollInfo implementation
        /// <summary>
        /// Size of the scroll, in pixels.
        /// </summary>
        private Size _scrollExtent;

        /// <summary>
        /// Offset of the scroll position.
        /// </summary>
        private Vector _scrollOffset;

        /// <summary>
        /// Size of the viewport.
        /// </summary>
        private Size _scrollViewport;

        private void ClearScrollData()
        {
            SetScrollData(new Size(), new Size(), new Vector());
        }

        private bool SetScrollData(Size viewport, Size extent, Vector offset)
        {
            if (!(viewport.IsClose(_scrollViewport)
                  && extent.IsClose(_scrollExtent)
                  && offset.IsClose(_scrollOffset)))
            {
                _scrollViewport = viewport;
                _scrollExtent = extent;
                SetScrollOffset(offset);
                OnScrollChange();
                return true;
            }

            return false;
        }

        private void OnScrollChange()
        {
            ((ILogicalScrollable)this).RaiseScrollInvalidated(EventArgs.Empty);
        }

        private bool _canVerticallyScroll = true;

        private bool _canHorizontallyScroll = true;

        /// <summary>
        /// Gets the horizontal scroll offset.
        /// </summary>
        public double HorizontalOffset => _scrollOffset.X;

        /// <summary>
        /// Gets the vertical scroll offset.
        /// </summary>
        public double VerticalOffset => _scrollOffset.Y;

        /// <summary>
        /// Gets the scroll offset;
        /// </summary>
        public Vector ScrollOffset => _scrollOffset;

        /// <summary>
        /// Occurs when the scroll offset has changed.
        /// </summary>
        public event EventHandler ScrollOffsetChanged;

        internal void SetScrollOffset(Vector vector)
        {
            if (!_canHorizontallyScroll)
            {
                vector = new Vector(0, vector.Y);
            }

            if (!_canVerticallyScroll)
            {
                vector = new Vector(vector.X, 0);
            }

            if (!_scrollOffset.IsClose(vector))
            {
                _scrollOffset = vector;
                ScrollOffsetChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _defaultTextMetricsValid;
        private double _wideSpaceWidth;    // Width of an 'x'. Used as basis for the tab width, and for scrolling.
        private double _defaultTextHeight; // Height of a text containing 'x'.
        private double _defaultLineHeight; // Height of a line containing 'x' (= text height adjusted by LineHeightFactor).
                                           // Used for scrolling.
        private double _defaultBaseline;   // Baseline of text containing 'x'. Used for TextTop/TextBottom calculation.

        /// <summary>
        /// Gets the width of a 'wide space' (the space width used for calculating the tab size).
        /// </summary>
        /// <remarks>
        /// This is the width of an 'x' in the current font.
        /// We do not measure the width of an actual space as that would lead to tiny tabs in
        /// some proportional fonts.
        /// For monospaced fonts, this property will return the expected value, as 'x' and ' ' have the same width.
        /// </remarks>
        public double WideSpaceWidth
        {
            get
            {
                CalculateDefaultTextMetrics();
                return _wideSpaceWidth;
            }
        }

        /// <summary>
        /// Gets the default line height. This is the height of an empty line or a line containing regular text.
        /// Lines that include formatted text or custom UI elements may have a different line height.
        /// </summary>
        public double DefaultLineHeight
        {
            get
            {
                CalculateDefaultTextMetrics();
                return _defaultLineHeight;
            }
        }

        internal double DefaultTextHeight
        {
            get
            {
                CalculateDefaultTextMetrics();
                return _defaultTextHeight;
            }
        }

        /// <summary>
        /// Gets the default baseline position. This is the difference between <see cref="VisualYPosition.TextTop"/>
        /// and <see cref="VisualYPosition.Baseline"/> for a line containing regular text.
        /// Lines that include formatted text or custom UI elements may have a different baseline.
        /// </summary>
        public double DefaultBaseline
        {
            get
            {
                CalculateDefaultTextMetrics();
                return _defaultBaseline;
            }
        }

        private void InvalidateDefaultTextMetrics()
        {
            _defaultTextMetricsValid = false;
            if (_heightTree != null)
            {
                // calculate immediately so that height tree gets updated
                CalculateDefaultTextMetrics();
            }
        }

        private void CalculateDefaultTextMetrics()
        {
            if (_defaultTextMetricsValid)
                return;

            _defaultTextMetricsValid = true;

            TextLine line = null;
            if (_formatter != null)
            {
                var textRunProperties = CreateGlobalTextRunProperties();
                line = _formatter.FormatLine(
                    new SimpleTextSource("x", textRunProperties),
                    0, 32000,
                    new VisualLineTextParagraphProperties { defaultTextRunProperties = textRunProperties });
            }

            if (line != null)
            {
                _wideSpaceWidth = Math.Max(1, line.WidthIncludingTrailingWhitespace);
                _defaultBaseline = Math.Max(1, line.Baseline);
                _defaultTextHeight = Math.Max(1, line.Height);
            }
            else
            {
                _wideSpaceWidth = FontSize / 2;
                _defaultBaseline = FontSize;
                _defaultTextHeight = FontSize + 3;
            }

            _defaultLineHeight = _defaultTextHeight * Options.LineHeightFactor;

            // Update heightTree.DefaultLineHeight, if a document is loaded.
            if (_heightTree != null)
                _heightTree.DefaultLineHeight = _defaultLineHeight;
        }

        private static double ValidateVisualOffset(double offset)
        {
            if (double.IsNaN(offset))
                throw new ArgumentException("offset must not be NaN");
            if (offset < 0)
                return 0;
            return offset;
        }

        /// <summary>
        /// Scrolls the text view so that the specified rectangle gets visible.
        /// </summary>
        public virtual void MakeVisible(Rect rectangle)
        {
            var visibleRectangle = new Rect(_scrollOffset.X, _scrollOffset.Y,
                                             _scrollViewport.Width, _scrollViewport.Height);
            var newScrollOffsetX = _scrollOffset.X;
            var newScrollOffsetY = _scrollOffset.Y;
            if (rectangle.X < visibleRectangle.X)
            {
                if (rectangle.Right > visibleRectangle.Right)
                {
                    newScrollOffsetX = rectangle.X + rectangle.Width / 2;
                }
                else
                {
                    newScrollOffsetX = rectangle.X;
                }
            }
            else if (rectangle.Right > visibleRectangle.Right)
            {
                newScrollOffsetX = rectangle.Right - _scrollViewport.Width;
            }
            if (rectangle.Y < visibleRectangle.Y)
            {
                if (rectangle.Bottom > visibleRectangle.Bottom)
                {
                    newScrollOffsetY = rectangle.Y + rectangle.Height / 2;
                }
                else
                {
                    newScrollOffsetY = rectangle.Y;
                }
            }
            else if (rectangle.Bottom > visibleRectangle.Bottom)
            {
                newScrollOffsetY = rectangle.Bottom - _scrollViewport.Height;
            }
            newScrollOffsetX = ValidateVisualOffset(newScrollOffsetX);
            newScrollOffsetY = ValidateVisualOffset(newScrollOffsetY);
            var newScrollOffset = new Vector(newScrollOffsetX, newScrollOffsetY);
            if (!_scrollOffset.IsClose(newScrollOffset))
            {
                SetScrollOffset(newScrollOffset);
                OnScrollChange();
                InvalidateMeasure();
            }
        }
        #endregion

        #region Visual element pointer handling

        [ThreadStatic] private static bool _invalidCursor;
        //private VisualLineElement _currentHoveredElement;

        /// <summary>
        /// Updates the pointe cursor, but with background priority.
        /// </summary>
        public static void InvalidateCursor()
        {
            if (!_invalidCursor)
            {
                _invalidCursor = true;
                Dispatcher.UIThread.InvokeAsync(
                    delegate
                    {
                        _invalidCursor = false;
                        //MouseDevice.Instance.UpdateCursor();
                    },
                    DispatcherPriority.Background // fixes issue #288
                    );
            }
        }

        internal void InvalidateCursorIfPointerWithinTextView()
        {
            // Don't unnecessarily call Mouse.UpdateCursor() if the mouse is outside the text view.
            // Unnecessary updates may cause the mouse pointer to flicker
            // (e.g. if it is over a window border, it blinks between Resize and Normal)
            if (IsPointerOver)
            {
                InvalidateCursor();
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            var element = GetVisualLineElementFromPosition(e.GetPosition(this) + _scrollOffset);

            // Change back to default if hover on a different element
            if (_currentHoveredElement != element)
            {
                Cursor = Parent?.GetValue(CursorProperty); // uses TextArea's ContentPresenter cursor
                _currentHoveredElement = element;
            }

            element?.OnQueryCursor(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (!e.Handled)
            {
                EnsureVisualLines();
                var element = GetVisualLineElementFromPosition(e.GetPosition(this) + _scrollOffset);
                element?.OnPointerPressed(e);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (!e.Handled)
            {
                EnsureVisualLines();
                var element = GetVisualLineElementFromPosition(e.GetPosition(this) + _scrollOffset);
                element?.OnPointerReleased(e);
            }
        }
        #endregion

        #region Getting elements from Visual Position
        /// <summary>
        /// Gets the visual line at the specified document position (relative to start of document).
        /// Returns null if there is no visual line for the position (e.g. the position is outside the visible
        /// text area).
        /// </summary>
        public VisualLine GetVisualLineFromVisualTop(double visualTop)
        {
            // TODO: change this method to also work outside the visible range -
            // required to make GetPosition work as expected!
            EnsureVisualLines();
            foreach (var vl in VisualLines)
            {
                if (visualTop < vl.VisualTop)
                    continue;
                if (visualTop < vl.VisualTop + vl.Height)
                    return vl;
            }
            return null;
        }

        /// <summary>
        /// Gets the visual top position (relative to start of document) from a document line number.
        /// </summary>
        public double GetVisualTopByDocumentLine(int line)
        {
            VerifyAccess();
            if (_heightTree == null)
                throw ThrowUtil.NoDocumentAssigned();
            return _heightTree.GetVisualPosition(_heightTree.GetLineByNumber(line));
        }

        private VisualLineElement GetVisualLineElementFromPosition(Point visualPosition)
        {
            var vl = GetVisualLineFromVisualTop(visualPosition.Y);
            if (vl != null)
            {
                var column = vl.GetVisualColumnFloor(visualPosition);

                foreach (var element in vl.Elements)
                {
                    if (element.VisualColumn + element.VisualLength <= column)
                        continue;
                    return element;
                }
            }
            return null;
        }
        #endregion

        #region Visual Position <-> TextViewPosition
        /// <summary>
        /// Gets the visual position from a text view position.
        /// </summary>
        /// <param name="position">The text view position.</param>
        /// <param name="yPositionMode">The mode how to retrieve the Y position.</param>
        /// <returns>The position in device-independent pixels relative
        /// to the top left corner of the document.</returns>
        public Point GetVisualPosition(TextViewPosition position, VisualYPosition yPositionMode)
        {
            VerifyAccess();
            if (Document == null)
                throw ThrowUtil.NoDocumentAssigned();
            var documentLine = Document.GetLineByNumber(position.Line);
            var visualLine = GetOrConstructVisualLine(documentLine);
            var visualColumn = position.VisualColumn;
            if (visualColumn < 0)
            {
                var offset = documentLine.Offset + position.Column - 1;
                visualColumn = visualLine.GetVisualColumn(offset - visualLine.FirstDocumentLine.Offset);
            }
            return visualLine.GetVisualPosition(visualColumn, position.IsAtEndOfLine, yPositionMode);
        }

        /// <summary>
        /// Gets the text view position from the specified visual position.
        /// If the position is within a character, it is rounded to the next character boundary.
        /// </summary>
        /// <param name="visualPosition">The position in device-independent pixels relative
        /// to the top left corner of the document.</param>
        /// <returns>The logical position, or null if the position is outside the document.</returns>
        public TextViewPosition? GetPosition(Point visualPosition)
        {
            VerifyAccess();
            if (Document == null)
                throw ThrowUtil.NoDocumentAssigned();
            var line = GetVisualLineFromVisualTop(visualPosition.Y);
            return line?.GetTextViewPosition(visualPosition, Options.EnableVirtualSpace);
        }

        /// <summary>
        /// Gets the text view position from the specified visual position.
        /// If the position is inside a character, the position in front of the character is returned.
        /// </summary>
        /// <param name="visualPosition">The position in device-independent pixels relative
        /// to the top left corner of the document.</param>
        /// <returns>The logical position, or null if the position is outside the document.</returns>
        public TextViewPosition? GetPositionFloor(Point visualPosition)
        {
            VerifyAccess();
            if (Document == null)
                throw ThrowUtil.NoDocumentAssigned();
            var line = GetVisualLineFromVisualTop(visualPosition.Y);
            return line?.GetTextViewPositionFloor(visualPosition, Options.EnableVirtualSpace);
        }
        #endregion

        #region Service Provider

        /// <summary>
        /// Gets a service container used to associate services with the text view.
        /// </summary>
        internal IServiceContainer Services { get; } = new ServiceContainer();

        /// <summary>
        /// Retrieves a service from the text view.
        /// If the service is not found in the <see cref="Services"/> container,
        /// this method will also look for it in the current document's service provider.
        /// </summary>
        public virtual object GetService(Type serviceType)
        {
            var instance = Services.GetService(serviceType);
            if (instance == null && _document != null)
            {
                instance = _document.ServiceProvider.GetService(serviceType);
            }
            return instance;
        }

        private void ConnectToTextView(object obj)
        {
            var c = obj as ITextViewConnect;
            c?.AddToTextView(this);
        }

        private void DisconnectFromTextView(object obj)
        {
            var c = obj as ITextViewConnect;
            c?.RemoveFromTextView(this);
        }
        #endregion

        #region PointerHover
        /// <summary>
        /// The PreviewPointerHover event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PreviewPointerHoverEvent =
            RoutedEvent.Register<PointerEventArgs>(nameof(PreviewPointerHover), RoutingStrategies.Tunnel, typeof(TextView));

        /// <summary>
        /// The PointerHover event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PointerHoverEvent =
            RoutedEvent.Register<PointerEventArgs>(nameof(PointerHover), RoutingStrategies.Bubble,
                                             typeof(TextView));

        /// <summary>
        /// The PreviewPointerHoverStopped event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PreviewPointerHoverStoppedEvent =
            RoutedEvent.Register<PointerEventArgs>(nameof(PreviewPointerHoverStopped), RoutingStrategies.Tunnel,
                                             typeof(TextView));
        /// <summary>
        /// The PointerHoverStopped event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PointerHoverStoppedEvent =
            RoutedEvent.Register<PointerEventArgs>(nameof(PointerHoverStopped), RoutingStrategies.Bubble,
                                             typeof(TextView));


        /// <summary>
        /// Occurs when the pointer has hovered over a fixed location for some time.
        /// </summary>
        public event EventHandler<PointerEventArgs> PreviewPointerHover
        {
            add => AddHandler(PreviewPointerHoverEvent, value);
            remove => RemoveHandler(PreviewPointerHoverEvent, value);
        }

        /// <summary>
        /// Occurs when the pointer has hovered over a fixed location for some time.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerHover
        {
            add => AddHandler(PointerHoverEvent, value);
            remove => RemoveHandler(PointerHoverEvent, value);
        }

        /// <summary>
        /// Occurs when the pointe had previously hovered but now started moving again.
        /// </summary>
        public event EventHandler<PointerEventArgs> PreviewPointerHoverStopped
        {
            add => AddHandler(PreviewPointerHoverStoppedEvent, value);
            remove => RemoveHandler(PreviewPointerHoverStoppedEvent, value);
        }

        /// <summary>
        /// Occurs when the pointer had previously hovered but now started moving again.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerHoverStopped
        {
            add => AddHandler(PointerHoverStoppedEvent, value);
            remove => RemoveHandler(PointerHoverStoppedEvent, value);
        }


        private readonly PointerHoverLogic _hoverLogic;

        private void RaiseHoverEventPair(PointerEventArgs e, RoutedEvent tunnelingEvent, RoutedEvent bubblingEvent)
        {
            e.RoutedEvent = tunnelingEvent;
            RaiseEvent(e);
            e.RoutedEvent = bubblingEvent;
            RaiseEvent(e);
        }
        #endregion

        /// <summary>
        /// Collapses lines for the purpose of scrolling. <see cref="DocumentLine"/>s marked as collapsed will be hidden
        /// and not used to start the generation of a <see cref="VisualLine"/>.
        /// </summary>
        /// <remarks>
        /// This method is meant for <see cref="VisualLineElementGenerator"/>s that cause <see cref="VisualLine"/>s to span
        /// multiple <see cref="DocumentLine"/>s. Do not call it without providing a corresponding
        /// <see cref="VisualLineElementGenerator"/>.
        /// If you want to create collapsible text sections, see <see cref="Folding.FoldingManager"/>.
        /// 
        /// Note that if you want a VisualLineElement to span from line N to line M, then you need to collapse only the lines
        /// N+1 to M. Do not collapse line N itself.
        /// 
        /// When you no longer need the section to be collapsed, call <see cref="CollapsedLineSection.Uncollapse()"/> on the
        /// <see cref="CollapsedLineSection"/> returned from this method.
        /// </remarks>
        public CollapsedLineSection CollapseLines(DocumentLine start, DocumentLine end)
        {
            VerifyAccess();
            if (_heightTree == null)
                throw ThrowUtil.NoDocumentAssigned();
            return _heightTree.CollapseText(start, end);
        }

        /// <summary>
        /// Gets the height of the document.
        /// </summary>
        public double DocumentHeight => _heightTree?.TotalHeight ?? 0;

        /// <summary>
        /// Gets the document line at the specified visual position.
        /// </summary>
        public DocumentLine GetDocumentLineByVisualTop(double visualTop)
        {
            VerifyAccess();
            if (_heightTree == null)
                throw ThrowUtil.NoDocumentAssigned();
            return _heightTree.GetLineByVisualPosition(visualTop);
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TemplatedControl.ForegroundProperty
                     || change.Property == NonPrintableCharacterBrushProperty
                     || change.Property == LinkTextBackgroundBrushProperty
                     || change.Property == LinkTextForegroundBrushProperty
                     || change.Property == LinkTextUnderlineProperty)
            {
                // changing brushes requires recreating the cached elements
                RecreateCachedElements();
                Redraw();
            }
            if (change.Property == TemplatedControl.FontFamilyProperty
                || change.Property == TemplatedControl.FontSizeProperty
                || change.Property == TemplatedControl.FontStyleProperty
                || change.Property == TemplatedControl.FontWeightProperty)
            {
                // changing font properties requires recreating cached elements
                RecreateCachedElements();
                // and we need to re-measure the font metrics:
                InvalidateDefaultTextMetrics();
                Redraw();
            }
            if (change.Property == ColumnRulerPenProperty)
            {
                _columnRulerRenderer.SetRuler(Options.ColumnRulerPositions, ColumnRulerPen);
            }
            if (change.Property == CurrentLineBorderProperty)
            {
                _currentLineHighlightRenderer.BorderPen = CurrentLineBorder;
            }
            if (change.Property == CurrentLineBackgroundProperty)
            {
                _currentLineHighlightRenderer.BackgroundBrush = CurrentLineBackground;
            }
        }

        /// <summary>
        /// The pen used to draw the column ruler.
        /// <seealso cref="TextEditorOptions.ShowColumnRulers"/>
        /// </summary>
        public static readonly StyledProperty<IPen> ColumnRulerPenProperty =
            AvaloniaProperty.Register<TextView, IPen>("ColumnRulerBrush", CreateFrozenPen(new SolidColorBrush(Color.FromArgb(90, 128, 128, 128))));

        private static ImmutablePen CreateFrozenPen(IBrush brush)
        {
            var pen = new ImmutablePen(brush?.ToImmutable());
            return pen;
        }

        bool ILogicalScrollable.BringIntoView(Control target, Rect rectangle)
        {
            if (rectangle == default || target == null || target == this || !this.IsVisualAncestorOf(target))
            {
                return false;
            }

            // TODO:
            // Convert rectangle into our coordinate space.
            //var childTransform = target.TransformToVisual(this);
            //rectangle = childTransform.Value(rectangle);

            MakeVisible(rectangle.WithX(rectangle.X + _scrollOffset.X).WithY(rectangle.Y + _scrollOffset.Y));

            return true;
        }

        Control ILogicalScrollable.GetControlInDirection(NavigationDirection direction, Control from)
        {
            return null;
        }

        event EventHandler ILogicalScrollable.ScrollInvalidated
        {
            add => _scrollInvalidated += value;
            remove => _scrollInvalidated -= value;
        }

        void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e)
        {
            _scrollInvalidated?.Invoke(this, e);
        }

        /// <summary>
        /// Gets/Sets the pen used to draw the column ruler.
        /// <seealso cref="TextEditorOptions.ShowColumnRulers"/>
        /// </summary>
        public IPen ColumnRulerPen
        {
            get => GetValue(ColumnRulerPenProperty);
            set => SetValue(ColumnRulerPenProperty, value);
        }

        /// <summary>
        /// The <see cref="CurrentLineBackground"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> CurrentLineBackgroundProperty =
            AvaloniaProperty.Register<TextView, IBrush>("CurrentLineBackground");

        /// <summary>
        /// Gets/Sets the background brush used by current line highlighter.
        /// </summary>
        public IBrush CurrentLineBackground
        {
            get => GetValue(CurrentLineBackgroundProperty);
            set => SetValue(CurrentLineBackgroundProperty, value);
        }

        /// <summary>
        /// The <see cref="CurrentLineBorder"/> property.
        /// </summary>
        public static readonly StyledProperty<IPen> CurrentLineBorderProperty =
            AvaloniaProperty.Register<TextView, IPen>("CurrentLineBorder");

        /// <summary>
        /// Gets/Sets the background brush used for the current line.
        /// </summary>
        public IPen CurrentLineBorder
        {
            get => GetValue(CurrentLineBorderProperty);
            set => SetValue(CurrentLineBorderProperty, value);
        }

        /// <summary>
        /// Gets/Sets highlighted line number.
        /// </summary>
        public int HighlightedLine
        {
            get => _currentLineHighlightRenderer.Line;
            set => _currentLineHighlightRenderer.Line = value;
        }

        /// <summary>
        /// Empty line selection width.
        /// </summary>
        public virtual double EmptyLineSelectionWidth => 1;

        bool ILogicalScrollable.CanHorizontallyScroll
        {
            get => _canHorizontallyScroll;
            set
            {
                if (_canHorizontallyScroll != value)
                {
                    _canHorizontallyScroll = value;
                    ClearVisualLines();
                    InvalidateMeasure();
                }
            }
        }

        bool ILogicalScrollable.CanVerticallyScroll
        {
            get => _canVerticallyScroll;
            set
            {
                if (_canVerticallyScroll != value)
                {
                    _canVerticallyScroll = value;
                    ClearVisualLines();
                    InvalidateMeasure();
                }
            }
        }

        bool IScrollable.CanHorizontallyScroll => _canHorizontallyScroll;

        bool IScrollable.CanVerticallyScroll => _canVerticallyScroll;

        bool ILogicalScrollable.IsLogicalScrollEnabled => true;

        Size ILogicalScrollable.ScrollSize => new Size(10, 50);

        Size ILogicalScrollable.PageScrollSize => new Size(10, 100);

        Size IScrollable.Extent => _scrollExtent;

        Vector IScrollable.Offset
        {
            get => _scrollOffset;
            set
            {
                value = new Vector(ValidateVisualOffset(value.X), ValidateVisualOffset(value.Y));
                var isX = !_scrollOffset.X.IsClose(value.X);
                var isY = !_scrollOffset.Y.IsClose(value.Y);
                if (isX || isY)
                {
                    SetScrollOffset(value);

                    if (isX)
                    {
                        InvalidateVisual();
                        TextLayer.InvalidateVisual();
                    }

                    InvalidateMeasure();
                }
            }
        }

        Size IScrollable.Viewport => _scrollViewport;

        public void SetDefaultHighlightLineColors()
        {
            _currentLineHighlightRenderer?.SetDefaultColors();
        }
    }
}

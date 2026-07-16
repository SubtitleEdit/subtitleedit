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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Search
{
    /// <summary>
    /// Provides search functionality for AvalonEdit. It is displayed in the top-right corner of the TextArea.
    /// </summary>
    public class SearchPanel : TemplatedControl, IRoutedCommandBindable
    {
        private TextArea _textArea;
        private SearchInputHandler _handler;
        private TextDocument _currentDocument;
        private SearchResultBackgroundRenderer _renderer;
        private TextBox _searchTextBox;
        private TextBox _replaceTextBox;
        private TextEditor _textEditor { get; set; }
        private Border _border;
        private int _currentSearchResultIndex = -1;

        #region DependencyProperties
        /// <summary>
        /// Dependency property for <see cref="UseRegex"/>.
        /// </summary>
        public static readonly StyledProperty<bool> UseRegexProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(UseRegex));

        /// <summary>
        /// Gets/sets whether the search pattern should be interpreted as regular expression.
        /// </summary>
        public bool UseRegex
        {
            get => GetValue(UseRegexProperty);
            set => SetValue(UseRegexProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="MatchCase"/>.
        /// </summary>
        public static readonly StyledProperty<bool> MatchCaseProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(MatchCase));

        /// <summary>
        /// Gets/sets whether the search pattern should be interpreted case-sensitive.
        /// </summary>
        public bool MatchCase
        {
            get => GetValue(MatchCaseProperty);
            set => SetValue(MatchCaseProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="WholeWords"/>.
        /// </summary>
        public static readonly StyledProperty<bool> WholeWordsProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(WholeWords));

        /// <summary>
        /// Gets/sets whether the search pattern should only match whole words.
        /// </summary>
        public bool WholeWords
        {
            get => GetValue(WholeWordsProperty);
            set => SetValue(WholeWordsProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="SearchPattern"/>.
        /// </summary>
        public static readonly StyledProperty<string> SearchPatternProperty =
            AvaloniaProperty.Register<SearchPanel, string>(nameof(SearchPattern), "");

        /// <summary>
        /// Gets/sets the search pattern.
        /// </summary>
        public string SearchPattern
        {
            get => GetValue(SearchPatternProperty);
            set => SetValue(SearchPatternProperty, value);
        }

        public static readonly StyledProperty<bool> IsReplaceModeProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(IsReplaceMode));

        public bool IsReplaceMode
        {
            get => GetValue(IsReplaceModeProperty);
            set => SetValue(IsReplaceModeProperty, _textEditor?.IsReadOnly ?? false ? false : value);
        }

        public static readonly StyledProperty<string> ReplacePatternProperty =
            AvaloniaProperty.Register<SearchPanel, string>(nameof(ReplacePattern));

        public string ReplacePattern
        {
            get => GetValue(ReplacePatternProperty);
            set => SetValue(ReplacePatternProperty, value);
        }

        #endregion

        public TextEditor TextEditor => _textEditor;

        public void SetSearchResultsBrush(IBrush brush)
        {
            if (_renderer == null)
                return;

            _renderer.MarkerBrush = brush;
            _textEditor.TextArea.TextView.InvalidateVisual();
        }

        private ISearchStrategy _strategy;

        private static void SearchPatternChangedCallback(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is SearchPanel panel)
            {
                panel.UpdateSearch();
            }
        }

        private void UpdateSearch()
        {
            // only reset as long as there are results
            // if no results are found, the "no matches found" message should not flicker.
            // if results are found by the next run, the message will be hidden inside DoSearch ...
            try
            {
                if (_renderer.CurrentResults.Any() && _messageView != null)
                    _messageView.IsVisible = false;
                _strategy = SearchStrategyFactory.Create(SearchPattern ?? "", !MatchCase, WholeWords, UseRegex ? SearchMode.RegEx : SearchMode.Normal);
                OnSearchOptionsChanged(new SearchOptionsChangedEventArgs(SearchPattern, MatchCase, UseRegex, WholeWords));
                DoSearch(true);
            }
            catch (SearchPatternException)
            {
                CleanSearchResults();
                UpdateSearchLabel();
            }
        }

        static SearchPanel()
        {
            UseRegexProperty.Changed.Subscribe(SearchPatternChangedCallback);
            MatchCaseProperty.Changed.Subscribe(SearchPatternChangedCallback);
            WholeWordsProperty.Changed.Subscribe(SearchPatternChangedCallback);
            SearchPatternProperty.Changed.Subscribe(SearchPatternChangedCallback);
        }

        /// <summary>
        /// Creates a SearchPanel and installs it to the TextEditor's TextArea.
        /// </summary>
        /// <remarks>This is a convenience wrapper.</remarks>
        public static SearchPanel Install(TextEditor editor)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            if (editor.TextArea == null) throw new ArgumentNullException(nameof(editor.TextArea));

            TextArea textArea = editor.TextArea;

            var panel = new SearchPanel();
            panel.AttachInternal(editor);
            panel._handler = new SearchInputHandler(textArea, panel);
            textArea.DefaultInputHandler.NestedInputHandlers.Add(panel._handler);
            ((ISetLogicalParent)panel).SetParent(textArea);
            KeyboardNavigation.SetTabNavigation(panel, KeyboardNavigationMode.Cycle);
            return panel;
        }

        /// <summary>
        /// Adds the commands used by SearchPanel to the given CommandBindingCollection.
        /// </summary>
        public void RegisterCommands(ICollection<RoutedCommandBinding> commandBindings)
        {
            _handler.RegisterGlobalCommands(commandBindings);
        }

        /// <summary>
        /// Removes the SearchPanel from the TextArea.
        /// </summary>
        public void Uninstall()
        {
            Close();
            _textArea.DocumentChanged -= TextArea_DocumentChanged;
            if (_currentDocument != null)
                _currentDocument.TextChanged -= TextArea_Document_TextChanged;
            _textArea.DefaultInputHandler.NestedInputHandlers.Remove(_handler);
        }

        private void AttachInternal(TextEditor textEditor)
        {
            _textEditor = textEditor;
            _textArea = textEditor.TextArea;

            _renderer = new SearchResultBackgroundRenderer(textEditor.SearchResultsBrush);
            _currentDocument = _textArea.Document;
            if (_currentDocument != null)
                _currentDocument.TextChanged += TextArea_Document_TextChanged;
            _textArea.DocumentChanged += TextArea_DocumentChanged;
            KeyDown += SearchLayerKeyDown;

            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.SelectAll, (sender, e) => SelectAll(e)));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Copy, (sender, e) => Copy(e)));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Cut, (sender, e) => Cut(e)));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Paste, (sender, e) => Paste(e)));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Undo, (sender, e) => Undo(e)));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Redo, (sender, e) => Redo(e)));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.FindNext, (sender, e) => FindNext()));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.FindPrevious, (sender, e) => FindPrevious()));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.CloseSearchPanel, (sender, e) => Close()));

            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Find, (sender, e) =>
            {
                IsReplaceMode = false;
                Reactivate();
            }));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Replace, (sender, e) => IsReplaceMode = true));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.ReplaceNext, (sender, e) => ReplaceNext(), (sender, e) => e.CanExecute = IsReplaceMode));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.ReplaceAll, (sender, e) => ReplaceAll(), (sender, e) => e.CanExecute = IsReplaceMode));

            IsClosed = true;
        }

        private void TextArea_DocumentChanged(object sender, EventArgs e)
        {
            if (_currentDocument != null)
                _currentDocument.TextChanged -= TextArea_Document_TextChanged;
            _currentDocument = _textArea.Document;
            if (_currentDocument != null)
            {
                _currentDocument.TextChanged += TextArea_Document_TextChanged;
                DoSearch(false);
            }
        }

        private void TextArea_Document_TextChanged(object sender, EventArgs e)
        {
            DoSearch(false);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _border = e.NameScope.Find<Border>("PART_Border");
            _searchTextBox = e.NameScope.Find<TextBox>("PART_searchTextBox");
            _replaceTextBox = e.NameScope.Find<TextBox>("PART_replaceTextBox");
            _messageView = e.NameScope.Find<Panel>("PART_MessageView");
            _messageViewContent = e.NameScope.Find<TextBlock>("PART_MessageContent");
        }

        /// <summary>
        /// Reactivates the SearchPanel by setting the focus on the search box and selecting all text.
        /// </summary>
        public void Reactivate()
        {
            if (_searchTextBox == null)
                return;
            _searchTextBox.Focus();
            _searchTextBox.SelectionStart = 0;
            _searchTextBox.SelectionEnd = _searchTextBox.Text?.Length ?? 0;
        }

        void SelectAll(ExecutedRoutedEventArgs e)
        {
            TextBox focusedTextBox = GetFocusedTextBox();

            if (focusedTextBox == null)
                return;

            e.Handled = true;
            focusedTextBox.SelectAll();
        }

        void Cut(ExecutedRoutedEventArgs e)
        {
            TextBox focusedTextBox = GetFocusedTextBox();

            if (focusedTextBox == null)
                return;

            e.Handled = true;
            focusedTextBox.Cut();
        }

        void Copy(ExecutedRoutedEventArgs e)
        {
            TextBox focusedTextBox = GetFocusedTextBox();

            if (focusedTextBox == null)
                return;

            e.Handled = true;
            focusedTextBox.Copy();
        }

        void Paste(ExecutedRoutedEventArgs e)
        {
            TextBox focusedTextBox = GetFocusedTextBox();

            if (focusedTextBox == null)
                return;

            e.Handled = true;
            focusedTextBox.Paste();
        }

        void Undo(ExecutedRoutedEventArgs e)
        {
            TextBox focusedTextBox = GetFocusedTextBox();

            if (focusedTextBox == null)
                return;

            e.Handled = true;
            focusedTextBox.Undo();
        }

        void Redo(ExecutedRoutedEventArgs e)
        {
            TextBox focusedTextBox = GetFocusedTextBox();

            if (focusedTextBox == null)
                return;

            e.Handled = true;
            focusedTextBox.Redo();
        }

        TextBox GetFocusedTextBox()
        {
            if (_searchTextBox.IsFocused)
                return _searchTextBox;

            if (_replaceTextBox.IsFocused)
                return _replaceTextBox;

            return null;
        }


        /// <summary>
        /// Moves to the next occurrence in the file starting at the next position from current caret offset.
        /// </summary>
        public void FindNext(int startOffset = -1)
        {
            var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(startOffset == -1 ? _textArea.Caret.Offset : startOffset) ??
                         _renderer.CurrentResults.FirstSegment;
            if (result != null)
            {
                SetCurrentSearchResult(result);
            }
        }

        /// <summary>
        /// Moves to the previous occurrence in the file.
        /// </summary>
        public void FindPrevious()
        {
            var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(
                Math.Max(_textArea.Caret.Offset - _textArea.Selection.Length, 0));
            if (result != null)
                result = _renderer.CurrentResults.GetPreviousSegment(result);
            if (result == null)
                result = _renderer.CurrentResults.LastSegment;
            if (result != null)
            {
                SetCurrentSearchResult(result);
            }
        }

        public void ReplaceNext()
        {
            if (!IsReplaceMode) return;

            FindNext(Math.Max(_textArea.Caret.Offset - _textArea.Selection.Length, 0));
            if (!_textArea.Selection.IsEmpty)
            {
                _textArea.Selection.ReplaceSelectionWithText(ReplacePattern ?? string.Empty);
            }

            UpdateSearch();
        }

        public void ReplaceAll()
        {
            if (!IsReplaceMode) return;

            var replacement = ReplacePattern ?? string.Empty;
            var document = _textArea.Document;
            using (document.RunUpdate())
            {
                var segments = _renderer.CurrentResults.OrderByDescending(x => x.EndOffset).ToArray();
                foreach (var textSegment in segments)
                {
                    document.Replace(textSegment.StartOffset, textSegment.Length,
                        new StringTextSource(replacement));
                }
            }
        }

        private Panel _messageView;
        private TextBlock _messageViewContent;

        private void SetCurrentSearchResult(SearchResult result)
        {
            _currentSearchResultIndex = GetSearchResultIndex(_renderer.CurrentResults, result);
            SelectResult(result);
            UpdateSearchLabel();
        }

        private void DoSearch(bool changeSelection)
        {
            if (IsClosed)
                return;

            CleanSearchResults();

            var offset = Math.Max(_textArea.Caret.Offset - _textArea.Selection.Length, 0);

            if (changeSelection)
            {
                _textArea.ClearSelection();
            }
            
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                // We cast from ISearchResult to SearchResult; this is safe because we always use the built-in strategy
                foreach (var result in _strategy.FindAll(_textArea.Document, 0, _textArea.Document.TextLength).Cast<SearchResult>())
                {
                    _renderer.CurrentResults.Add(result);
                }

                if (changeSelection)
                {
                    // select the first result after the caret position
                    // or the first result in document order if there is no result after the caret
                    var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(offset) ??
                                 _renderer.CurrentResults.FirstSegment;

                    if (result != null)
                        SelectResult(result);

                    _currentSearchResultIndex = _renderer.CurrentResults.Count - 1;
                }
            }

            UpdateSearchLabel();
            _textArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }

        void CleanSearchResults()
        {
            _renderer.CurrentResults.Clear();
            _currentSearchResultIndex = -1;
        }

        void UpdateSearchLabel()
        {
            if (_messageView == null || _messageViewContent == null)
                return;

            _messageView.IsVisible = true;

            if (!_renderer.CurrentResults.Any())
            {
                _messageViewContent.Text = SR.SearchNoMatchesFoundText;
            }
            else
            {
                if (_currentSearchResultIndex == -1)
                {
                    if (_renderer.CurrentResults.Count == 1)
                    {
                        _messageViewContent.Text = SR.Search1Match;
                    }
                    else
                    {
                        _messageViewContent.Text = string.Format(SR.SearchXMatches,
                            _renderer.CurrentResults.Count);                        
                    }
                }
                else
                {
                    _messageViewContent.Text = string.Format(SR.SearchXOfY,
                        _currentSearchResultIndex + 1,
                        _renderer.CurrentResults.Count);
                }
            }
        }

        private void SelectResult(TextSegment result)
        {
            _textArea.Caret.Offset = result.EndOffset;
            _textArea.Selection = Selection.Create(_textArea, result.StartOffset, result.EndOffset);

            double distanceToViewBorder = _border == null ?
                Caret.MinimumDistanceToViewBorder :
                _border.Bounds.Height + _textArea.TextView.DefaultLineHeight;
            _textArea.Caret.BringCaretToView(distanceToViewBorder);

            // show caret even if the editor does not have the Keyboard Focus
            _textArea.Caret.Show();
        }

        private void SearchLayerKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        FindPrevious();
                    }
                    else
                    {
                        FindNext();
                    }
                    break;
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }

        /// <summary>
        /// Gets whether the Panel is already closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Gets whether the Panel is currently opened.
        /// </summary>
        public bool IsOpened => !IsClosed;

        /// <summary>
        /// Closes the SearchPanel.
        /// </summary>
        public void Close()
        {
            _textArea.RemoveChild(this);

            if (_messageView != null)
                _messageView.IsVisible = false;

            _textArea.TextView.BackgroundRenderers.Remove(_renderer);
            
            IsClosed = true;

            // Clear existing search results so that the segments don't have to be maintained
            _renderer.CurrentResults.Clear();
            _currentSearchResultIndex = -1;

            _textArea.Focus();
        }

        /// <summary>
        /// Opens the an existing search panel.
        /// </summary>
        public void Open()
        {
            if (!IsClosed) return;

            _textArea.AddChild(this);

            _textArea.TextView.BackgroundRenderers.Add(_renderer);
            IsClosed = false;
            DoSearch(false);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Handled = true;

            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            Cursor = Cursor.Default;
            base.OnPointerMoved(e);
        }

        protected override void OnGotFocus(FocusChangedEventArgs e)
        {
            e.Handled = true;

            base.OnGotFocus(e);
        }
        
        private static int GetSearchResultIndex(TextSegmentCollection<SearchResult> searchResults, SearchResult match)
        {
            int index = 0;
            foreach (SearchResult searchResult in searchResults)
            {
                if (searchResult.Equals(match))
                    return index;
                
                index++;
            }

            return -1;
        }

        /// <summary>
        /// Fired when SearchOptions are changed inside the SearchPanel.
        /// </summary>
        public event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged;

        /// <summary>
        /// Raises the <see cref="SearchOptionsChanged" /> event.
        /// </summary>
        protected virtual void OnSearchOptionsChanged(SearchOptionsChangedEventArgs e)
        {
            SearchOptionsChanged?.Invoke(this, e);
        }

        public IList<RoutedCommandBinding> CommandBindings { get; } = new List<RoutedCommandBinding>();
    }

    /// <summary>
    /// EventArgs for <see cref="SearchPanel.SearchOptionsChanged"/> event.
    /// </summary>
    public class SearchOptionsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the search pattern.
        /// </summary>
        public string SearchPattern { get; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted case-sensitive.
        /// </summary>
        public bool MatchCase { get; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted as regular expression.
        /// </summary>
        public bool UseRegex { get; }

        /// <summary>
        /// Gets whether the search pattern should only match whole words.
        /// </summary>
        public bool WholeWords { get; }

        /// <summary>
        /// Creates a new SearchOptionsChangedEventArgs instance.
        /// </summary>
        public SearchOptionsChangedEventArgs(string searchPattern, bool matchCase, bool useRegex, bool wholeWords)
        {
            SearchPattern = searchPattern;
            MatchCase = matchCase;
            UseRegex = useRegex;
            WholeWords = wholeWords;
        }
    }
}

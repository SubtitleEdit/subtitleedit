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
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Editing;

namespace AvaloniaEdit.Search
{
    /// <summary>
    /// Search commands for AvalonEdit.
    /// </summary>
    public static class SearchCommands
    {
        /// <summary>
        /// Finds the next occurrence in the file.
        /// </summary>
        public static readonly RoutedCommand FindNext = new RoutedCommand(nameof(FindNext), new KeyGesture(Key.F3));

        /// <summary>
        /// Finds the previous occurrence in the file.
        /// </summary>
        public static readonly RoutedCommand FindPrevious = new RoutedCommand(nameof(FindPrevious), new KeyGesture(Key.F3, KeyModifiers.Shift));

        /// <summary>
        /// Closes the SearchPanel.
        /// </summary>
        public static readonly RoutedCommand CloseSearchPanel = new RoutedCommand(nameof(CloseSearchPanel), new KeyGesture(Key.Escape));

        /// <summary>
        /// Replaces the next occurrence in the document.
        /// </summary>
        public static readonly RoutedCommand ReplaceNext = new RoutedCommand(nameof(ReplaceNext), new KeyGesture(Key.R, KeyModifiers.Alt));

        /// <summary>
        /// Replaces all the occurrences in the document.
        /// </summary>
        public static readonly RoutedCommand ReplaceAll = new RoutedCommand(nameof(ReplaceAll), new KeyGesture(Key.A, KeyModifiers.Alt));
    }

    /// <summary>
    /// TextAreaInputHandler that registers all search-related commands.
    /// </summary>
    internal class SearchInputHandler : TextAreaInputHandler
    {
        public SearchInputHandler(TextArea textArea, SearchPanel panel)
            : base(textArea)
        {
            RegisterCommands(CommandBindings);
            _panel = panel;
        }

        internal void RegisterGlobalCommands(ICollection<RoutedCommandBinding> commandBindings)
        {
            commandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Find, ExecuteFind));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Replace, ExecuteReplace));
            commandBindings.Add(new RoutedCommandBinding(SearchCommands.FindNext, ExecuteFindNext, CanExecuteWithOpenSearchPanel));
            commandBindings.Add(new RoutedCommandBinding(SearchCommands.FindPrevious, ExecuteFindPrevious, CanExecuteWithOpenSearchPanel));
        }

        private void RegisterCommands(ICollection<RoutedCommandBinding> commandBindings)
        {
            commandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Find, ExecuteFind));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Replace, ExecuteReplace));
            commandBindings.Add(new RoutedCommandBinding(SearchCommands.FindNext, ExecuteFindNext, CanExecuteWithOpenSearchPanel));
            commandBindings.Add(new RoutedCommandBinding(SearchCommands.FindPrevious, ExecuteFindPrevious, CanExecuteWithOpenSearchPanel));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.ReplaceNext, ExecuteReplaceNext, CanExecuteWithOpenSearchPanel));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.ReplaceAll, ExecuteReplaceAll, CanExecuteWithOpenSearchPanel));
            commandBindings.Add(new RoutedCommandBinding(SearchCommands.CloseSearchPanel, ExecuteCloseSearchPanel, CanExecuteWithOpenSearchPanel));
        }

        private readonly SearchPanel _panel;

        private void ExecuteFind(object sender, ExecutedRoutedEventArgs e)
        {
            FindOrReplace(isReplaceMode: false);
        }

        private void ExecuteReplace(object sender, ExecutedRoutedEventArgs e)
        {
            FindOrReplace(isReplaceMode: true);
        }

        private void FindOrReplace(bool isReplaceMode)
        {
            _panel.IsReplaceMode = isReplaceMode;
            _panel.Open();
            if (!(TextArea.Selection.IsEmpty || TextArea.Selection.IsMultiline))
                _panel.SearchPattern = TextArea.Selection.GetText();
            Dispatcher.UIThread.Post(_panel.Reactivate, DispatcherPriority.Input);
        }

        private void CanExecuteWithOpenSearchPanel(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_panel.IsClosed)
            {
                e.CanExecute = false;
                // Continue routing so that the key gesture can be consumed by another component.
                //e.ContinueRouting = true;
            }
            else
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void ExecuteFindNext(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_panel.IsClosed)
            {
                _panel.FindNext();
                e.Handled = true;
            }
        }

        private void ExecuteFindPrevious(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_panel.IsClosed)
            {
                _panel.FindPrevious();
                e.Handled = true;
            }
        }

        private void ExecuteReplaceNext(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_panel.IsClosed)
            {
                _panel.ReplaceNext();
                e.Handled = true;
            }
        }

        private void ExecuteReplaceAll(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_panel.IsClosed)
            {
                _panel.ReplaceAll();
                e.Handled = true;
            }
        }

        private void ExecuteCloseSearchPanel(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_panel.IsClosed)
            {
                _panel.Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Fired when SearchOptions are modified inside the SearchPanel.
        /// </summary>
        public event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged
        {
            add => _panel.SearchOptionsChanged += value;
            remove => _panel.SearchOptionsChanged -= value;
        }
    }
}

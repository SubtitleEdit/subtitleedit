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

using Avalonia.Input;
using AvaloniaEdit.Editing;

namespace AvaloniaEdit.Snippets
{
    internal sealed class SnippetInputHandler : TextAreaStackedInputHandler
    {
        private readonly InsertionContext _context;

        public SnippetInputHandler(InsertionContext context)
            : base(context.TextArea)
        {
            _context = context;
        }

        public override void Attach()
        {
            base.Attach();

            SelectElement(FindNextEditableElement(-1, false));
        }

        public override void Detach()
        {
            base.Detach();
            _context.Deactivate(new SnippetEventArgs(DeactivateReason.InputHandlerDetached));
        }

        public override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
            {
                _context.Deactivate(new SnippetEventArgs(DeactivateReason.EscapePressed));
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                _context.Deactivate(new SnippetEventArgs(DeactivateReason.ReturnPressed));
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                var backwards = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
                SelectElement(FindNextEditableElement(TextArea.Caret.Offset, backwards));
                e.Handled = true;
            }
        }

        private void SelectElement(IActiveElement element)
        {
            if (element != null)
            {
                TextArea.Selection = Selection.Create(TextArea, element.Segment);
                TextArea.Caret.Offset = element.Segment.EndOffset;
            }
        }

        private IActiveElement FindNextEditableElement(int offset, bool backwards)
        {
            IActiveElement firstEditableElement = null;
            IActiveElement lastEditableElement = null;
            IActiveElement previousEditableElement = null;

            foreach (IActiveElement element in _context.ActiveElements)
            {
                if (!element.IsEditable || element.Segment == null)
                {
                    continue;
                }

                if (firstEditableElement == null)
                {
                    firstEditableElement = element;
                }

                lastEditableElement = element;

                if (backwards)
                {
                    if (offset > element.Segment.EndOffset)
                    {
                        previousEditableElement = element;
                    }
                }
                else
                {
                    if (offset < element.Segment.Offset)
                    {
                        return element;
                    }
                }
            }

            if (backwards)
            {
                return previousEditableElement ?? lastEditableElement;
            }

            return firstEditableElement;
        }
    }
}

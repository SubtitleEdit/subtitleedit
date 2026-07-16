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
using System.Diagnostics;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Avalonia.Controls;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Base class for margins.
    /// Margins don't have to derive from this class, it just helps maintaining a reference to the TextView
    /// and the TextDocument.
    /// AbstractMargin derives from FrameworkElement, so if you don't want to handle visual children and rendering
    /// on your own, choose another base class for your margin!
    /// </summary>
    public abstract class AbstractMargin : Control, ITextViewConnect
    {
        public AbstractMargin()
        {
            this.GetPropertyChangedObservable(TextViewProperty).Subscribe(o =>
            {
                _wasAutoAddedToTextView = false;
                OnTextViewChanged(o.OldValue as TextView, o.NewValue as TextView);
            });
        }

        /// <summary>
        /// TextView property.
        /// </summary>
        public static readonly StyledProperty<TextView> TextViewProperty =
            AvaloniaProperty.Register<AbstractMargin, TextView>(nameof(TextView));

        /// <summary>
        /// Gets/sets the text view for which line numbers are displayed.
        /// </summary>
        /// <remarks>Adding a margin to <see cref="TextArea.LeftMargins"/> will automatically set this property to the text area's TextView.</remarks>
        public TextView TextView
        {
            get => GetValue(TextViewProperty);
            set => SetValue(TextViewProperty, value);
        }

        // automatically set/unset TextView property using ITextViewConnect
        private bool _wasAutoAddedToTextView;

        void ITextViewConnect.AddToTextView(TextView textView)
        {
            if (TextView == null)
            {
                TextView = textView;
                _wasAutoAddedToTextView = true;
            }
            else if (TextView != textView)
            {
                throw new InvalidOperationException("This margin belongs to a different TextView.");
            }
        }

        void ITextViewConnect.RemoveFromTextView(TextView textView)
        {
            if (_wasAutoAddedToTextView && TextView == textView)
            {
                TextView = null;
                Debug.Assert(!_wasAutoAddedToTextView); // setting this.TextView should have unset this flag
            }
        }

        /// <summary>
        /// Gets the document associated with the margin.
        /// </summary>
        public TextDocument Document { get; private set; }

        protected TextArea TextArea { get; set; }

        /// <summary>
        /// Called when the <see cref="TextView"/> is changing.
        /// </summary>
        protected virtual void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.DocumentChanged -= TextViewDocumentChanged;
            }

            if (newTextView != null)
            {
                newTextView.DocumentChanged += TextViewDocumentChanged;
            }

            TextViewDocumentChanged(null, null);

            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            }

            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += TextViewVisualLinesChanged;

                // find the text area belonging to the new text view
                TextArea = newTextView.GetService(typeof(TextArea)) as TextArea;
            }
            else
            {
                TextArea = null;
            }
        }

        /// <summary>
        /// Called when the attached textviews visual lines change.
        /// Default behavior is to Invalidate Margins Visual.
        /// </summary>
        protected virtual void OnTextViewVisualLinesChanged()
        {
            InvalidateVisual();
        }

        private void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            OnTextViewVisualLinesChanged();
        }

        private void TextViewDocumentChanged(object sender, EventArgs e)
        {
            OnDocumentChanged(Document, TextView?.Document);
        }

        /// <summary>
        /// Called when the <see cref="Document"/> is changing.
        /// </summary>
        protected virtual void OnDocumentChanged(TextDocument oldDocument, TextDocument newDocument)
        {
            Document = newDocument;
        }
    }
}

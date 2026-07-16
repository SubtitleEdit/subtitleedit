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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.CodeCompletion
{
    /// <summary>
    /// The list box used inside the CompletionList.
    /// </summary>
    public class CompletionListBox : ListBox
    {
        internal ScrollViewer ScrollViewer;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ScrollViewer = e.NameScope.Find("PART_ScrollViewer") as ScrollViewer;
        }

        /// <summary>
        /// Gets the number of the first visible item.
        /// </summary>
        public int FirstVisibleItem
        {
            get
            {
                if (ScrollViewer == null || ScrollViewer.Extent.Height == 0)
                {
                    return 0;
                }

                return (int)(ItemCount * ScrollViewer.Offset.Y / ScrollViewer.Extent.Height);
            }
            set
            {
                value = value.CoerceValue(0, ItemCount - VisibleItemCount);
                if (ScrollViewer != null)
                {
                    ScrollViewer.Offset = ScrollViewer.Offset.WithY((double)value / ItemCount * ScrollViewer.Extent.Height);
                }
            }
        }

        /// <summary>
        /// Gets the number of visible items.
        /// </summary>
        public int VisibleItemCount
        {
            get
            {
                if (ScrollViewer == null || ScrollViewer.Extent.Height == 0)
                {
                    return 10;
                }
                return Math.Max(
                    3,
                    (int)Math.Ceiling(ItemCount * ScrollViewer.Viewport.Height
                                      / ScrollViewer.Extent.Height));
            }
        }

        /// <summary>
        /// Removes the selection.
        /// </summary>
        public void ClearSelection()
        {
            SelectedIndex = -1;
        }

        /// <summary>
        /// Selects the item with the specified index and scrolls it into view.
        /// </summary>
        public void SelectIndex(int index)
        {
            if (index >= ItemCount)
                index = ItemCount - 1;
            if (index < 0)
                index = 0;
            SelectedIndex = index;
            ScrollIntoView(SelectedItem);
        }

        /// <summary>
        /// Centers the view on the item with the specified index.
        /// </summary>
        public void CenterViewOn(int index)
        {
            FirstVisibleItem = index - VisibleItemCount / 2;
        }
    }
}

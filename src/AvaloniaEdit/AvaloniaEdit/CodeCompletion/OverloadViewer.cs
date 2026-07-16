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
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace AvaloniaEdit.CodeCompletion
{
    /// <summary>
    /// Represents a text between "Up" and "Down" buttons.
    /// </summary>
    public class OverloadViewer : TemplatedControl
    {
        /// <summary>
        /// The text property.
        /// </summary>
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<OverloadViewer, string>("Text");

        /// <summary>
        /// Gets/Sets the text between the Up and Down buttons.
        /// </summary>
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs args)
        {
            base.OnApplyTemplate(args);

            var upButton = args.NameScope.Find<Button>("PART_UP");
            if (upButton != null)
            {
                upButton.Click += (sender, e) =>
                  {
                      e.Handled = true;
                      ChangeIndex(-1);
                  };
            }

            var downButton = args.NameScope.Find<Button>("PART_DOWN");
            if (downButton != null)
            {
                downButton.Click += (sender, e) =>
                  {
                      e.Handled = true;
                      ChangeIndex(+1);
                  };
            }
        }

        /// <summary>
        /// The ItemProvider property.
        /// </summary>
        public static readonly StyledProperty<IOverloadProvider> ProviderProperty =
            AvaloniaProperty.Register<OverloadViewer, IOverloadProvider>("Provider");

        /// <summary>
        /// Gets/Sets the item provider.
        /// </summary>
        public IOverloadProvider Provider
        {
            get => GetValue(ProviderProperty);
            set => SetValue(ProviderProperty, value);
        }

        /// <summary>
        /// Changes the selected index.
        /// </summary>
        /// <param name="relativeIndexChange">The relative index change - usual values are +1 or -1.</param>
        public void ChangeIndex(int relativeIndexChange)
        {
            var p = Provider;
            if (p != null)
            {
                var newIndex = p.SelectedIndex + relativeIndexChange;
                if (newIndex < 0)
                    newIndex = p.Count - 1;
                if (newIndex >= p.Count)
                    newIndex = 0;
                p.SelectedIndex = newIndex;
            }
        }
    }

    /// <summary>
    /// Converter to be used in the <see cref="OverloadViewer"/> control theme. Used to set the
    /// visibility of the part showing the number of overloads.
    /// </summary>
    public sealed class CollapseIfSingleOverloadConverter : IValueConverter
    {
        public static CollapseIfSingleOverloadConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Show the up/down arrows and the "i of n" text if there are 2 or more method overloads.
            if (value is int count)
                return count >= 2;

            return AvaloniaProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

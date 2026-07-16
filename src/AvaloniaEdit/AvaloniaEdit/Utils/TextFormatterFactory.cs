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
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AvaloniaEdit.Utils
{
    /// <summary>
	/// Creates TextFormatter instances that with the correct TextFormattingMode, if running on .NET 4.0.
	/// </summary>
	static class TextFormatterFactory
	{
		/// <summary>
		/// Creates a <see cref="TextFormatter"/> using the formatting mode used by the specified owner object.
		/// </summary>
		public static TextFormatter Create(Control owner)
		{
			return TextFormatter.Current;
		}

		/// <summary>
		/// Creates formatted text.
		/// </summary>
		/// <param name="element">The owner element. The text formatter setting are read from this element.</param>
		/// <param name="text">The text.</param>
		/// <param name="typeface">The typeface to use. If this parameter is null, the typeface of the <paramref name="element"/> will be used.</param>
		/// <param name="emSize">The font size. If this parameter is null, the font size of the <paramref name="element"/> will be used.</param>
		/// <param name="foreground">The foreground color. If this parameter is null, the foreground of the <paramref name="element"/> will be used.</param>
		/// <returns>A FormattedText object using the specified settings.</returns>
		public static FormattedText CreateFormattedText(Control element, string text, Typeface typeface, double? emSize, IBrush foreground)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (typeface == default)
				typeface = element.CreateTypeface();
			if (emSize == null)
				emSize = TextElement.GetFontSize(element);
			if (foreground == null)
				foreground = TextElement.GetForeground(element);
			
			return new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				typeface,
				emSize.Value,
				foreground);
		}
	}
}

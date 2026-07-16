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

using AvaloniaEdit.Rendering;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AvaloniaEdit.Highlighting
{
	/// <summary>
	/// A brush used for syntax highlighting. Can retrieve a real brush on-demand.
	/// </summary>
	public abstract class HighlightingBrush
	{
		/// <summary>
		/// Gets the real brush.
		/// </summary>
		/// <param name="context">The construction context. context can be null!</param>
		public abstract IBrush GetBrush(ITextRunConstructionContext context);
		
		/// <summary>
		/// Gets the color of the brush.
		/// </summary>
		/// <param name="context">The construction context. context can be null!</param>
		public virtual Color? GetColor(ITextRunConstructionContext context)
		{
		    if (GetBrush(context) is ISolidColorBrush scb)
                return scb.Color;
		    return null;
		}
	}
	
	/// <summary>
	/// Highlighting brush implementation that takes a frozen brush.
	/// </summary>
	public sealed class SimpleHighlightingBrush : HighlightingBrush
	{
	    private readonly ISolidColorBrush _brush;
		
		internal SimpleHighlightingBrush(ISolidColorBrush brush)
		{
			_brush = brush;
		}
		
		/// <summary>
		/// Creates a new HighlightingBrush with the specified color.
		/// </summary>
		public SimpleHighlightingBrush(Color color) : this(new ImmutableSolidColorBrush(color)) {}
		
		/// <inheritdoc/>
		public override IBrush GetBrush(ITextRunConstructionContext context)
		{
			return _brush;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return _brush.ToString();
		}
		
		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			SimpleHighlightingBrush other = obj as SimpleHighlightingBrush;
			if (other == null)
				return false;
			return _brush.Color.Equals(other._brush.Color);
		}
		
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return _brush.Color.GetHashCode();
		}
	}
}

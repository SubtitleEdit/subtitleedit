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
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Indentation.CSharp
{
	/// <summary>
	/// Interface used for the indentation class to access the document.
	/// </summary>
	public interface IDocumentAccessor
	{
		/// <summary>Gets if the current line is read only (because it is not in the
		/// selected text region)</summary>
		bool IsReadOnly { get; }
		/// <summary>Gets the number of the current line.</summary>
		int LineNumber { get; }
		/// <summary>Gets/Sets the text of the current line.</summary>
		string Text { get; set; }
		/// <summary>Advances to the next line.</summary>
		bool MoveNext();
	}
	
	#region TextDocumentAccessor
	/// <summary>
	/// Adapter IDocumentAccessor -> TextDocument
	/// </summary>
	public sealed class TextDocumentAccessor : IDocumentAccessor
	{
	    private readonly TextDocument _doc;
	    private readonly int _minLine;
	    private readonly int _maxLine;
		
		/// <summary>
		/// Creates a new TextDocumentAccessor.
		/// </summary>
		public TextDocumentAccessor(TextDocument document)
		{
            _doc = document ?? throw new ArgumentNullException(nameof(document));
			_minLine = 1;
			_maxLine = _doc.LineCount;
		}
		
		/// <summary>
		/// Creates a new TextDocumentAccessor that indents only a part of the document.
		/// </summary>
		public TextDocumentAccessor(TextDocument document, int minLine, int maxLine)
		{
            _doc = document ?? throw new ArgumentNullException(nameof(document));
			_minLine = minLine;
			_maxLine = maxLine;
		}

	    private int _num;
	    private string _text;
	    private DocumentLine _line;
		
		/// <inheritdoc/>
		public bool IsReadOnly => _num < _minLine;

	    /// <inheritdoc/>
		public int LineNumber => _num;

	    private bool _lineDirty;
		
		/// <inheritdoc/>
		public string Text {
			get => _text;
		    set {
				if (_num < _minLine) return;
				_text = value;
				_lineDirty = true;
			}
		}
		
		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (_lineDirty) {
				_doc.Replace(_line, _text);
				_lineDirty = false;
			}
			++_num;
			if (_num > _maxLine) return false;
			_line = _doc.GetLineByNumber(_num);
			_text = _doc.GetText(_line);
			return true;
		}
	}
	#endregion
}

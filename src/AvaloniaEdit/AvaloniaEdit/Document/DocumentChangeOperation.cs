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

using System.Diagnostics;

namespace AvaloniaEdit.Document
{
	/// <summary>
	/// Describes a change to a TextDocument.
	/// </summary>
	internal sealed class DocumentChangeOperation : IUndoableOperationWithContext
	{
	    private readonly TextDocument _document;
	    private readonly DocumentChangeEventArgs _change;
		
		public DocumentChangeOperation(TextDocument document, DocumentChangeEventArgs change)
		{
			_document = document;
			_change = change;
		}
		
		public void Undo(UndoStack stack)
		{
			Debug.Assert(stack.State == UndoStack.StatePlayback);
			stack.RegisterAffectedDocument(_document);
			stack.State = UndoStack.StatePlaybackModifyDocument;
			Undo();
			stack.State = UndoStack.StatePlayback;
		}
		
		public void Redo(UndoStack stack)
		{
			Debug.Assert(stack.State == UndoStack.StatePlayback);
			stack.RegisterAffectedDocument(_document);
			stack.State = UndoStack.StatePlaybackModifyDocument;
			Redo();
			stack.State = UndoStack.StatePlayback;
		}
		
		public void Undo()
		{
			var map = _change.OffsetChangeMapOrNull;
			_document.Replace(_change.Offset, _change.InsertionLength, _change.RemovedText, map?.Invert());
		}
		
		public void Redo()
		{
			_document.Replace(_change.Offset, _change.RemovalLength, _change.InsertedText, _change.OffsetChangeMapOrNull);
		}
	}
}

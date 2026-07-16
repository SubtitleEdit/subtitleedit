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
using System.Diagnostics;
using System.IO;

namespace AvaloniaEdit.Utils
{
	/// <summary>
	/// TextReader implementation that reads text from a rope.
	/// </summary>
	public sealed class RopeTextReader : TextReader
	{
	    private readonly Stack<RopeNode<char>> _stack = new Stack<RopeNode<char>>();
	    private RopeNode<char> _currentNode;
	    private int _indexInsideNode;
		
		/// <summary>
		/// Creates a new RopeTextReader.
		/// Internally, this method creates a Clone of the rope; so the text reader will always read through the old
		/// version of the rope if it is modified. <seealso cref="Rope{T}.Clone()"/>
		/// </summary>
		public RopeTextReader(Rope<char> rope)
		{
			if (rope == null)
				throw new ArgumentNullException(nameof(rope));
			
			// We force the user to iterate through a clone of the rope to keep the API contract of RopeTextReader simple
			// (what happens when a rope is modified while iterating through it?)
			rope.Root.Publish();
			
			// special case for the empty rope:
			// leave currentNode initialized to null (RopeTextReader doesn't support empty nodes)
			if (rope.Length != 0) {
				_currentNode = rope.Root;
				GoToLeftMostLeaf();
			}
		}

	    private void GoToLeftMostLeaf()
		{
			while (_currentNode.Contents == null) {
				if (_currentNode.Height == 0) {
					// this is a function node - move to its contained rope
					_currentNode = _currentNode.GetContentNode();
					continue;
				}
				Debug.Assert(_currentNode.Right != null);
				_stack.Push(_currentNode.Right);
				_currentNode = _currentNode.Left;
			}
			Debug.Assert(_currentNode.Height == 0);
		}
		
		/// <inheritdoc/>
		public override int Peek()
		{
			if (_currentNode == null)
				return -1;
			return _currentNode.Contents[_indexInsideNode];
		}
		
		/// <inheritdoc/>
		public override int Read()
		{
			if (_currentNode == null)
				return -1;
			char result = _currentNode.Contents[_indexInsideNode++];
			if (_indexInsideNode >= _currentNode.Length)
				GoToNextNode();
			return result;
		}

	    private void GoToNextNode()
		{
			if (_stack.Count == 0) {
				_currentNode = null;
			} else {
				_indexInsideNode = 0;
				_currentNode = _stack.Pop();
				GoToLeftMostLeaf();
			}
		}
		
		/// <inheritdoc/>
		public override int Read(char[] buffer, int index, int count)
		{
			if (_currentNode == null)
				return 0;
			int amountInCurrentNode = _currentNode.Length - _indexInsideNode;
			if (count < amountInCurrentNode) {
				Array.Copy(_currentNode.Contents, _indexInsideNode, buffer, index, count);
				_indexInsideNode += count;
				return count;
			} else {
				// read to end of current node
				Array.Copy(_currentNode.Contents, _indexInsideNode, buffer, index, amountInCurrentNode);
				GoToNextNode();
				return amountInCurrentNode;
			}
		}
	}
}

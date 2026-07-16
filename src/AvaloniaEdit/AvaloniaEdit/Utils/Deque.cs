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

namespace AvaloniaEdit.Utils
{
	/// <summary>
	/// Double-ended queue.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	internal sealed class Deque<T> : ICollection<T>
	{
        private T[] _arr = Array.Empty<T>();
	    private int _head;
	    private int _tail;

	    /// <inheritdoc/>
		public int Count { get; private set; }

	    /// <inheritdoc/>
		public void Clear()
		{
            _arr = Array.Empty<T>();
			Count = 0;
			_head = 0;
			_tail = 0;
		}
		
		/// <summary>
		/// Gets/Sets an element inside the deque.
		/// </summary>
		public T this[int index] {
			get {
				ThrowUtil.CheckInRangeInclusive(index, "index", 0, Count - 1);
				return _arr[(_head + index) % _arr.Length];
			}
			set {
				ThrowUtil.CheckInRangeInclusive(index, "index", 0, Count - 1);
				_arr[(_head + index) % _arr.Length] = value;
			}
		}
		
		/// <summary>
		/// Adds an element to the end of the deque.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PushBack")]
		public void PushBack(T item)
		{
			if (Count == _arr.Length)
				SetCapacity(Math.Max(4, _arr.Length * 2));
			_arr[_tail++] = item;
			if (_tail == _arr.Length) _tail = 0;
			Count++;
		}
		
		/// <summary>
		/// Pops an element from the end of the deque.
		/// </summary>
		public T PopBack()
		{
			if (Count == 0)
				throw new InvalidOperationException();
			if (_tail == 0)
				_tail = _arr.Length - 1;
			else
				_tail--;
			T val = _arr[_tail];
			_arr[_tail] = default(T); // allow GC to collect the element
			Count--;
			return val;
		}
		
		/// <summary>
		/// Adds an element to the front of the deque.
		/// </summary>
		public void PushFront(T item)
		{
			if (Count == _arr.Length)
				SetCapacity(Math.Max(4, _arr.Length * 2));
			if (_head == 0)
				_head = _arr.Length - 1;
			else
				_head--;
			_arr[_head] = item;
			Count++;
		}
		
		/// <summary>
		/// Pops an element from the end of the deque.
		/// </summary>
		public T PopFront()
		{
			if (Count == 0)
				throw new InvalidOperationException();
			T val = _arr[_head];
			_arr[_head] = default(T); // allow GC to collect the element
			_head++;
			if (_head == _arr.Length) _head = 0;
			Count--;
			return val;
		}

	    private void SetCapacity(int capacity)
		{
			T[] newArr = new T[capacity];
			CopyTo(newArr, 0);
			_head = 0;
			_tail = (Count == capacity) ? 0 : Count;
			_arr = newArr;
		}
		
		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			if (_head < _tail) {
				for (int i = _head; i < _tail; i++)
					yield return _arr[i];
			} else {
				for (int i = _head; i < _arr.Length; i++)
					yield return _arr[i];
				for (int i = 0; i < _tail; i++)
					yield return _arr[i];
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		bool ICollection<T>.IsReadOnly => false;

	    void ICollection<T>.Add(T item)
		{
			PushBack(item);
		}
		
		/// <inheritdoc/>
		public bool Contains(T item)
		{
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			foreach (T element in this)
				if (comparer.Equals(item, element))
					return true;
			return false;
		}
		
		/// <inheritdoc/>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (_head < _tail) {
				Array.Copy(_arr, _head, array, arrayIndex, _tail - _head);
			} else {
				int num1 = _arr.Length - _head;
				Array.Copy(_arr, _head, array, arrayIndex, num1);
				Array.Copy(_arr, 0, array, arrayIndex + num1, _tail);
			}
		}
		
		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
		}
	}
}

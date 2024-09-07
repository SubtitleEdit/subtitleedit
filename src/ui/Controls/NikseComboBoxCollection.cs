using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public class NikseComboBoxCollection : IList
    {
        private List<object> _items;

        public NikseComboBoxCollection(Control control)
        {
            _items = new List<object>();
            SyncRoot = control;
        }

        public List<object> ToList() => _items;

        public void AddRange(object[] items)
        {
            _items.AddRange(items);
        }

        public void AddRange(string[] items)
        {
            _items.AddRange(items);
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            _items.ToArray().CopyTo(array, index);
        }

        public int Count => _items.Count;
        public object SyncRoot { get; }
        public bool IsSynchronized => false;
        public int Add(object value)
        {
            _items.Add(value);
            return _items.Count - 1;
        }

        public bool Contains(object value)
        {
            return _items.Contains(value);
        }

        public void Clear()
        {
            _items.Clear();
            if (SyncRoot is NikseComboBox ncb)
            {
                ncb.SelectedIndexReset(); // do not fire change events
            }
        }

        public int IndexOf(object value)
        {
            return _items.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            _items.Insert(index, value);
        }

        public void Remove(object value)
        {
            _items.Remove(value);
            if (SyncRoot is NikseComboBox ncb && ncb.SelectedIndex >= _items.Count)
            {
                ncb.SelectedIndexReset(); // do not fire change events
            }
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            if (SyncRoot is NikseComboBox ncb && ncb.SelectedIndex >= _items.Count)
            {
                ncb.SelectedIndexReset(); // do not fire change events
            }
        }

        public object this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public bool IsReadOnly => false;
        public bool IsFixedSize => false;

        public void SortBy(Func<object, object> func)
        {
            _items = _items.OrderBy(func).ToList();
        }

        internal object FirstOrDefault(Func<object, bool> value)
        {
            return _items.FirstOrDefault(value);
        }
    }
}

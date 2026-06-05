using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> that can append many items while
/// raising a single <see cref="NotifyCollectionChangedAction.Reset"/> instead
/// of one <c>Add</c> notification per item.
///
/// Bulk replacing the subtitle grid (the common <c>Clear()</c> + <c>AddRange()</c>
/// pattern) otherwise makes the bound Avalonia DataGrid run one layout pass per
/// appended row - O(n) notifications for an n-line subtitle. A single Reset lets
/// it rebuild once. Consumers of <see cref="ObservableCollection{T}.CollectionChanged"/>
/// must treat Reset as "re-read everything" (there are no NewItems/OldItems).
/// </summary>
public class RangeObservableCollection<T> : ObservableCollection<T>
{
    public RangeObservableCollection()
    {
    }

    public RangeObservableCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var added = false;
        foreach (var item in items)
        {
            Items.Add(item); // protected backing list: no per-item notification
            added = true;
        }

        if (!added)
        {
            return;
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}

using System.Collections;
using System.Collections.ObjectModel;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The two allocations/scans that MainViewModel.SubtitleGridSelectionChanged does on every
/// selection change - so on every arrow key, and on every Ctrl+A.
/// </summary>
[MemoryDiagnoser]
public class SubtitleGridSelectionBenchmarks
{
    private ObservableCollection<SubtitleLineViewModel> _subtitles = new();
    private IList _selectedItems = Array.Empty<SubtitleLineViewModel>();
    private SubtitleLineViewModel _lastItem = new();

    [Params(1000, 5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var lines = SubtitleFactory.Make(Lines);
        _subtitles = new ObservableCollection<SubtitleLineViewModel>(lines);

        // Worst case for IndexOf: the selected line sits at the end of the subtitle.
        _lastItem = lines[^1];

        // Avalonia's DataGridSelectedItemsCollection implements only the non-generic IList, so
        // Enumerable.Cast cannot take its "already IEnumerable<T>" fast path and ToList cannot
        // use ICollection<T>.CopyTo. Benchmarking a plain List<T> here would measure neither.
        _selectedItems = new NonGenericList(lines);
    }

    /// <summary>Old: LINQ Cast + ToList (no capacity hint, extra iterator).</summary>
    [Benchmark(Baseline = true)]
    public int CastToList() => _selectedItems.Cast<SubtitleLineViewModel>().ToList().Count;

    /// <summary>New: preallocated list, plain foreach.</summary>
    [Benchmark]
    public int PreallocatedCopy()
    {
        var list = new List<SubtitleLineViewModel>(_selectedItems.Count);
        foreach (SubtitleLineViewModel item in _selectedItems)
        {
            list.Add(item);
        }

        return list.Count;
    }

    /// <summary>Preallocated list filled through AddRange(Cast&lt;T&gt;()).</summary>
    [Benchmark]
    public int PreallocatedAddRange()
    {
        var list = new List<SubtitleLineViewModel>(_selectedItems.Count);
        list.AddRange(_selectedItems.Cast<SubtitleLineViewModel>());
        return list.Count;
    }

    /// <summary>Only exposes the non-generic collection interfaces, like Avalonia's SelectedItems.</summary>
    private sealed class NonGenericList : IList
    {
        private readonly List<SubtitleLineViewModel> _inner;

        public NonGenericList(List<SubtitleLineViewModel> inner) => _inner = inner;

        public int Count => _inner.Count;
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;
        public object? this[int index] { get => _inner[index]; set => throw new NotSupportedException(); }
        public IEnumerator GetEnumerator() => _inner.GetEnumerator();
        public int Add(object? value) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(object? value) => throw new NotSupportedException();
        public void CopyTo(Array array, int index) => throw new NotSupportedException();
        public int IndexOf(object? value) => throw new NotSupportedException();
        public void Insert(int index, object? value) => throw new NotSupportedException();
        public void Remove(object? value) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
    }

    /// <summary>Old: linear scan through the whole ObservableCollection.</summary>
    [Benchmark]
    public int LinearIndexOf() => _subtitles.IndexOf(_lastItem);

    /// <summary>New: trust the grid's own selected index, verify it, only then fall back.</summary>
    [Benchmark]
    public int HintedIndexOf()
    {
        var hinted = _subtitles.Count - 1;
        if (hinted >= 0 && hinted < _subtitles.Count && ReferenceEquals(_subtitles[hinted], _lastItem))
        {
            return hinted;
        }

        return _subtitles.IndexOf(_lastItem);
    }
}

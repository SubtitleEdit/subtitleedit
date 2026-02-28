using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public class SortCriterion : ObservableObject
{
    private string _propertyName;
    private string _displayName;
    private bool _ascending;

    public string PropertyName
    {
        get => _propertyName;
        set => SetProperty(ref _propertyName, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (SetProperty(ref _displayName, value))
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public bool Ascending
    {
        get => _ascending;
        set
        {
            if (SetProperty(ref _ascending, value))
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public string DisplayText => Ascending ? $"{DisplayName} ↑" : $"{DisplayName} ↓";

    public SortCriterion(string propertyName, string displayName, bool ascending = true)
    {
        _propertyName = propertyName;
        _displayName = displayName;
        _ascending = ascending;
    }
}

public partial class SortByViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;
    [ObservableProperty] private ObservableCollection<string> _availableProperties;
    [ObservableProperty] private ObservableCollection<SortCriterion> _sortCriteria;
    [ObservableProperty] private SortCriterion? _selectedSortCriterion;
    [ObservableProperty] private string? _selectedAvailableProperty;
    [ObservableProperty] private bool _newCriterionAscending = true;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private readonly List<SubtitleLineViewModel> _originalSubtitles;
    private Dictionary<string, string> _propertyTranslationMap;
    private Dictionary<string, string> _displayNameToPropertyMap;


    public SortByViewModel()
    {
        _subtitles = new ObservableCollection<SubtitleLineViewModel>();
        _allSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        _originalSubtitles = new List<SubtitleLineViewModel>();
        _availableProperties = new ObservableCollection<string>();
        _sortCriteria = new ObservableCollection<SortCriterion>();
        _propertyTranslationMap = new Dictionary<string, string>();
        _displayNameToPropertyMap = new Dictionary<string, string>();

        InitializeAvailableProperties();
        LoadSettings();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };
    }

    private void InitializeAvailableProperties()
    {
        var lang = Se.Language.General;
        
        // Add properties with their translated display names
        _propertyTranslationMap = new Dictionary<string, string>
        {
            { "Number", lang.Number },
            { "StartTime", lang.Show },
            { "EndTime", lang.Hide },
            { "Duration", lang.Duration },
            { "Text", lang.Text },
            { "OriginalText", lang.OriginalText },
            { "Style", lang.Style },
            { "Actor", lang.Actor },
            { "Layer", lang.Layer },
            { "Gap", lang.Gap },
            { "CharactersPerSecond", lang.CharsPerSec },
            { "WordsPerMinute", lang.WordsPerMin }
        };

        // Reverse map for lookup
        _displayNameToPropertyMap = _propertyTranslationMap.ToDictionary(x => x.Value, x => x.Key);

        // Add translated display names to the UI
        foreach (var displayName in _propertyTranslationMap.Values)
        {
            AvailableProperties.Add(displayName);
        }
    }

    private void UpdatePreview()
    {
        if (SortCriteria.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var sorted = ApplySorting(_originalSubtitles);
            Subtitles.Clear();
            foreach (var item in sorted)
            {
                Subtitles.Add(item);
            }
        });
    }

    private List<SubtitleLineViewModel> ApplySorting(List<SubtitleLineViewModel> items)
    {
        if (SortCriteria.Count == 0)
        {
            return items.ToList();
        }

        IOrderedEnumerable<SubtitleLineViewModel>? orderedItems = null;

        for (var i = 0; i < SortCriteria.Count; i++)
        {
            var criterion = SortCriteria[i];
            var isFirst = i == 0;

            if (isFirst)
            {
                orderedItems = criterion.Ascending
                    ? items.OrderBy(item => GetPropertyValue(item, criterion.PropertyName))
                    : items.OrderByDescending(item => GetPropertyValue(item, criterion.PropertyName));
            }
            else
            {
                orderedItems = criterion.Ascending
                    ? orderedItems!.ThenBy(item => GetPropertyValue(item, criterion.PropertyName))
                    : orderedItems!.ThenByDescending(item => GetPropertyValue(item, criterion.PropertyName));
            }
        }

        return orderedItems?.ToList() ?? items.ToList();
    }

    private object GetPropertyValue(SubtitleLineViewModel item, string propertyName)
    {
        return propertyName switch
        {
            "Number" => item.Number,
            "StartTime" => item.StartTime.TotalMilliseconds,
            "EndTime" => item.EndTime.TotalMilliseconds,
            "Duration" => item.Duration.TotalMilliseconds,
            "Text" => item.Text ?? string.Empty,
            "OriginalText" => item.OriginalText ?? string.Empty,
            "Style" => item.Style ?? string.Empty,
            "Actor" => item.Actor ?? string.Empty,
            "Layer" => item.Layer,
            "Gap" => item.Gap,
            "CharactersPerSecond" => item.CharactersPerSecond,
            "WordsPerMinute" => item.WordsPerMinute,
            _ => string.Empty
        };
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        _originalSubtitles.Clear();
        _originalSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        
        AllSubtitles.Clear();
        AllSubtitles.AddRange(_originalSubtitles.Select(p => new SubtitleLineViewModel(p)));
        
        Subtitles.Clear();
        foreach (var sub in AllSubtitles)
        {
            Subtitles.Add(new SubtitleLineViewModel(sub));
        }
        
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
       
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void AddSortCriterion()
    {
        if (string.IsNullOrEmpty(SelectedAvailableProperty))
        {
            return;
        }

        // Convert display name to property name
        if (!_displayNameToPropertyMap.TryGetValue(SelectedAvailableProperty, out var propertyName))
        {
            return;
        }

        var criterion = new SortCriterion(propertyName, SelectedAvailableProperty, NewCriterionAscending);
        SortCriteria.Add(criterion);
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void RemoveSortCriterion()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        SortCriteria.Remove(SelectedSortCriterion);
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void MoveSortCriterionUp()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        var index = SortCriteria.IndexOf(SelectedSortCriterion);
        if (index > 0)
        {
            SortCriteria.Move(index, index - 1);
            _dirty = true;
            _timerUpdatePreview.Start();
        }
    }

    [RelayCommand]
    private void MoveSortCriterionDown()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        var index = SortCriteria.IndexOf(SelectedSortCriterion);
        if (index < SortCriteria.Count - 1)
        {
            SortCriteria.Move(index, index + 1);
            _dirty = true;
            _timerUpdatePreview.Start();
        }
    }

    [RelayCommand]
    private void ToggleSortDirection()
    {
        if (SelectedSortCriterion == null)
        {
            return;
        }

        SelectedSortCriterion.Ascending = !SelectedSortCriterion.Ascending;
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void ClearSortCriteria()
    {
        SortCriteria.Clear();
        Subtitles.Clear();
        foreach (var sub in AllSubtitles)
        {
            Subtitles.Add(new SubtitleLineViewModel(sub));
        }
    }

    [RelayCommand]
    private void Ok()
    {
        if (SortCriteria.Count > 0)
        {
            AllSubtitles.Clear();
            AllSubtitles.AddRange(Subtitles.Select(p => new SubtitleLineViewModel(p)));
        }
        
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/sort-by");
        }
    }

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }
}
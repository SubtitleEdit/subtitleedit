using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public partial class ModifySelectionViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ModifySelectionRule> _rules;
    [ObservableProperty] private ModifySelectionRule? _selectedRule;

    [ObservableProperty] private ObservableCollection<PreviewItem> _subtitles;
    [ObservableProperty] private PreviewItem? _selectedSubtitle;

    [ObservableProperty] private bool _selectionNew;
    [ObservableProperty] private bool _selectionAdd;
    [ObservableProperty] private bool _selectionSubtract;
    [ObservableProperty] private bool _selectionIntersect;

    public Window? Window { get; set; }
    public List<SubtitleLineViewModel> Selection;

    public bool OkPressed { get; private set; }

    private List<SubtitleLineViewModel> _allSubtitles;
    private readonly System.Timers.Timer _previewTimer;
    private bool _isDirty;

    public ModifySelectionViewModel()
    {
        Rules = new ObservableCollection<ModifySelectionRule>();
        Subtitles = new ObservableCollection<PreviewItem>();
        Selection = new List<SubtitleLineViewModel>();

        _allSubtitles = new List<SubtitleLineViewModel>();

        LoadSettings();

        _previewTimer = new System.Timers.Timer(250);
        _previewTimer.Elapsed += (sender, args) =>
        {
            _previewTimer.Stop();

            if (_isDirty)
            {
                _isDirty = false;
                UpdatePreview();
            }

            _previewTimer.Start();
        };
    }

    private void UpdatePreview()
    {
        var rule = SelectedRule;
        if (rule == null || _allSubtitles.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var item in _allSubtitles)
            {
                if (rule.IsMatch(item))
                {
                    var previewItem = new PreviewItem(item.Number, true, item.StartTime, item.Duration, item.Text, item);
                    Subtitles.Add(previewItem);
                }
            }
        });
    }

    private void LoadSettings()
    {
        var selectionMode = Se.Settings.Edit.ModifySelectionMode.ToLowerInvariant();
        if (selectionMode == "new")
        {
            SelectionNew = true;
        }
        else if (selectionMode == "add")
        {
            SelectionAdd = true;
        }
        else if (selectionMode == "subtract")
        {
            SelectionSubtract = true;
        }
        else if (selectionMode == "intersect")
        {
            SelectionIntersect = true;
        }
        else
        {
            SelectionNew = true;
        }
    }

    private void SaveSettings()
    {
        if (SelectionNew)
        {
            Se.Settings.Edit.ModifySelectionMode = "new";
        }
        else if (SelectionAdd)
        {
            Se.Settings.Edit.ModifySelectionMode = "add";
        }
        else if (SelectionSubtract)
        {
            Se.Settings.Edit.ModifySelectionMode = "subtract";
        }
        else if (SelectionIntersect)
        {
            Se.Settings.Edit.ModifySelectionMode = "intersect";
        }
        else
        {
            Se.Settings.Edit.ModifySelectionMode = "new";
        }

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        foreach (var s in Subtitles)
        {
            if (s.Apply)
            {
                Selection.Add(s.Subtitle);
            }
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels, List<SubtitleLineViewModel> selectedItems)
    {
        _allSubtitles = subtitleLineViewModels;

        foreach (var rule in ModifySelectionRule.List(subtitleLineViewModels))
        {
            Rules.Add(rule);
        }

        SelectedRule = Rules.First();

        _previewTimer.Start();
    }

    internal void OnRuleChanged()
    {
        _isDirty = true;
    }
}
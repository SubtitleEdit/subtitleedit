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
    private readonly Dictionary<RuleType, double> _ruleNumbers;
    private RuleType? _activeRuleType;
    private bool _isDirty;

    public ModifySelectionViewModel()
    {
        Rules = new ObservableCollection<ModifySelectionRule>();
        Subtitles = new ObservableCollection<PreviewItem>();
        Selection = new List<SubtitleLineViewModel>();
        _ruleNumbers = new Dictionary<RuleType, double>();

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

        // Swap the bound collection in one assignment rather than adding matches
        // one-by-one: each ObservableCollection.Add fires a CollectionChanged
        // notification (and grid re-layout), which makes a large result set
        // (e.g. "Contains space" on a 1500-line file) crawl.
        Dispatcher.UIThread.Post(() =>
        {
            var matches = new List<PreviewItem>();
            foreach (var item in _allSubtitles)
            {
                if (rule.IsMatch(item))
                {
                    matches.Add(new PreviewItem(item.Number, true, item.StartTime, item.Duration, item.Text, item));
                }
            }

            Subtitles = new ObservableCollection<PreviewItem>(matches);
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

        if (SelectedRule != null)
        {
            Se.Settings.Edit.ModifySelectionRule = SelectedRule.RuleType.ToString();
            Se.Settings.Edit.ModifySelectionText = SelectedRule.Text;
            Se.Settings.Edit.ModifySelectionMatchCase = SelectedRule.MatchCase;
            if (SelectedRule.HasNumber)
            {
                Se.Settings.Edit.ModifySelectionNumber = SelectedRule.Number;
            }
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
        InitializeRules(subtitleLineViewModels);
        _previewTimer.Start();
    }

    internal void InitializeRules(List<SubtitleLineViewModel> subtitleLineViewModels)
    {
        Rules.Clear();
        _ruleNumbers.Clear();
        _activeRuleType = null;
        foreach (var rule in ModifySelectionRule.List(subtitleLineViewModels))
        {
            if (rule.HasNumber)
            {
                rule.Number = GetDefaultNumber(rule);
                _ruleNumbers[rule.RuleType] = rule.Number;
            }

            Rules.Add(rule);
        }

        SelectedRule = GetSavedRuleOrDefault();
    }

    internal void OnRuleChanged()
    {
        _isDirty = true;
    }

    partial void OnSelectedRuleChanged(ModifySelectionRule? value)
    {
        if (value == null)
        {
            return;
        }

        SaveActiveRuleNumber();

        if (value.HasNumber && _ruleNumbers.TryGetValue(value.RuleType, out var number))
        {
            value.Number = number;
        }

        _activeRuleType = value.RuleType;
        OnRuleChanged();
    }

    // Restore the rule type, text, match-case and number last used (persisted across
    // restarts, like SE 4), falling back to the first rule when nothing is saved (#11429).
    private ModifySelectionRule GetSavedRuleOrDefault()
    {
        var savedRuleType = Se.Settings.Edit.ModifySelectionRule;
        var rule = Rules.FirstOrDefault(r => r.RuleType.ToString() == savedRuleType) ?? Rules.First();

        if (rule.HasText)
        {
            rule.Text = Se.Settings.Edit.ModifySelectionText ?? string.Empty;
        }

        if (rule.HasMatchCase)
        {
            rule.MatchCase = Se.Settings.Edit.ModifySelectionMatchCase;
        }

        if (rule.HasNumber)
        {
            if (rule.RuleType.ToString() == savedRuleType)
            {
                rule.Number = Se.Settings.Edit.ModifySelectionNumber;
                _ruleNumbers[rule.RuleType] = rule.Number;
            }
            else
            {
                rule.Number = GetDefaultNumber(rule);
                _ruleNumbers[rule.RuleType] = rule.Number;
            }
        }

        return rule;
    }

    private void SaveActiveRuleNumber()
    {
        if (_activeRuleType == null)
        {
            return;
        }

        var activeRule = Rules.FirstOrDefault(r => r.RuleType == _activeRuleType);
        if (activeRule?.HasNumber == true)
        {
            _ruleNumbers[activeRule.RuleType] = activeRule.Number;
        }
    }

    private static double GetDefaultNumber(ModifySelectionRule rule)
    {
        return rule.RuleType switch
        {
            RuleType.LengthLessThan or RuleType.LengthGreaterThan => Se.Settings.General.SubtitleLineMaximumLength,
            RuleType.PixelWidthLengthGreaterThan => Se.Settings.General.SubtitleLineMaximumPixelWidth,
            _ => rule.DefaultValue,
        };
    }
}

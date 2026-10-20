using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Options.CustomErrorRules;

/// <summary>
/// Edits the user's custom error rules (#10136): regular expressions that mark a line as an
/// error when its text matches, e.g. "[CHECK]" markers left by a translation pipeline. The
/// caller writes the result back, so the settings window can keep it until its own OK.
/// </summary>
public partial class CustomErrorRulesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CustomErrorRuleItem> _rules;
    [ObservableProperty] private CustomErrorRuleItem? _selectedRule;
    [ObservableProperty] private bool _hasSelectedRule;
    [ObservableProperty] private bool _isHighlightEnabled;
    [ObservableProperty] private string _testText;
    [ObservableProperty] private string _testResult;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public CustomErrorRulesViewModel()
    {
        _rules = new ObservableCollection<CustomErrorRuleItem>();
        _isHighlightEnabled = true;
        _testText = string.Empty;
        _testResult = string.Empty;
    }

    public void Initialize(bool isHighlightEnabled, IEnumerable<SeCustomErrorRule> rules)
    {
        IsHighlightEnabled = isHighlightEnabled;
        Rules.Clear();
        foreach (var rule in rules)
        {
            Rules.Add(CustomErrorRuleItem.FromSetting(rule));
        }

        SelectedRule = Rules.FirstOrDefault();
    }

    /// <summary>The edited rules; rows without a pattern are dropped.</summary>
    public List<SeCustomErrorRule> GetRules()
    {
        return Rules
            .Where(p => !string.IsNullOrEmpty(p.Pattern))
            .Select(p => p.ToSetting())
            .ToList();
    }

    partial void OnSelectedRuleChanged(CustomErrorRuleItem? oldValue, CustomErrorRuleItem? newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= SelectedRulePropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += SelectedRulePropertyChanged;
        }

        HasSelectedRule = newValue != null;
        UpdateTestResult();
    }

    private void SelectedRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CustomErrorRuleItem.Pattern) or nameof(CustomErrorRuleItem.MatchCase))
        {
            UpdateTestResult();
        }
    }

    partial void OnTestTextChanged(string value) => UpdateTestResult();

    private void UpdateTestResult()
    {
        var rule = SelectedRule;
        if (rule == null || string.IsNullOrEmpty(rule.Pattern) || string.IsNullOrEmpty(TestText))
        {
            TestResult = string.Empty;
            return;
        }

        var regex = CustomErrorRuleMatcher.TryCreateRegex(rule.Pattern, rule.MatchCase);
        if (regex == null)
        {
            TestResult = Se.Language.SourceView.InvalidRegularExpression;
            return;
        }

        var l = Se.Language.Options.Settings;
        try
        {
            TestResult = regex.IsMatch(TestText) ? l.CustomErrorRuleTestMatch : l.CustomErrorRuleTestNoMatch;
        }
        catch (RegexMatchTimeoutException)
        {
            TestResult = l.CustomErrorRuleTestNoMatch;
        }
    }

    [RelayCommand]
    private void Add()
    {
        var item = new CustomErrorRuleItem();
        Rules.Add(item);
        SelectedRule = item;
    }

    [RelayCommand]
    private void Remove()
    {
        var item = SelectedRule;
        if (item == null)
        {
            return;
        }

        var index = Rules.IndexOf(item);
        Rules.Remove(item);
        SelectedRule = Rules.Count == 0 ? null : Rules[Math.Min(index, Rules.Count - 1)];
    }

    [RelayCommand]
    private void MoveUp() => Move(-1);

    [RelayCommand]
    private void MoveDown() => Move(1);

    // The first matching rule decides the color, so the order matters.
    private void Move(int delta)
    {
        var item = SelectedRule;
        if (item == null)
        {
            return;
        }

        var index = Rules.IndexOf(item);
        var newIndex = index + delta;
        if (index < 0 || newIndex < 0 || newIndex >= Rules.Count)
        {
            return;
        }

        Rules.Move(index, newIndex);
        SelectedRule = item;
    }

    [RelayCommand]
    private void Ok()
    {
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
            UiUtil.ShowHelp("features/settings", "syntax-coloring");
        }
    }
}

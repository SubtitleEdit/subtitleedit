using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public record RegexTestSegment(string Text, bool IsMatch);

public partial class RegexRulesViewModel : ObservableObject
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(1);

    [ObservableProperty] private ObservableCollection<RegexRuleItem> _rules;
    [ObservableProperty] private RegexRuleItem? _selectedRule;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private int _selectedShowIndex;
    [ObservableProperty] private string _enabledCountText;
    [ObservableProperty] private string _title;

    [ObservableProperty] private string _editFind;
    [ObservableProperty] private string _editReplace;
    [ObservableProperty] private bool _isPatternValid;
    [ObservableProperty] private string _patternStatus;
    [ObservableProperty] private string _editorHint;
    [ObservableProperty] private bool _canAdd;
    [ObservableProperty] private bool _canUpdate;
    [ObservableProperty] private bool _canDelete;
    [ObservableProperty] private bool _canToggle;

    [ObservableProperty] private string _testText;
    [ObservableProperty] private bool _applyAllEnabled;
    [ObservableProperty] private string _matchInfo;
    [ObservableProperty] private IReadOnlyList<RegexTestSegment> _inputSegments;
    [ObservableProperty] private IReadOnlyList<RegexTestSegment> _outputSegments;

    public IReadOnlyList<string> ShowOptions { get; }
    public Window? Window { get; set; }

    private readonly List<RegexRuleItem> _allRules = new();
    private OcrFixReplaceList2? _replaceList;

    public RegexRulesViewModel()
    {
        var l = Se.Language.Options.WordLists;
        _rules = new ObservableCollection<RegexRuleItem>();
        _searchText = string.Empty;
        _enabledCountText = string.Empty;
        _title = string.Empty;
        _editFind = string.Empty;
        _editReplace = string.Empty;
        _patternStatus = string.Empty;
        _editorHint = string.Empty;
        _testText = string.Empty;
        _matchInfo = string.Empty;
        _inputSegments = Array.Empty<RegexTestSegment>();
        _outputSegments = Array.Empty<RegexTestSegment>();
        ShowOptions = new[] { l.ShowAllRules, l.ShowEnabledRules, l.ShowDisabledRules, l.ShowMyRules };
    }

    public void Initialize(string replaceListFileName, string languageName)
    {
        Title = string.Format(Se.Language.Options.WordLists.RegexRulesTitle, languageName);
        _replaceList = new OcrFixReplaceList2(replaceListFileName);
        _allRules.Clear();
        foreach (var rule in _replaceList.GetRegexRules())
        {
            _allRules.Add(MakeItem(rule.Find, rule.ReplaceWith, rule.IsUserRule, rule.IsEnabled));
        }

        ApplyFilter();
        UpdateEnabledCount();
        UpdateEditorState();
        UpdateTest();
    }

    private RegexRuleItem MakeItem(string find, string replaceWith, bool isUserRule, bool isEnabled)
    {
        return new RegexRuleItem(find, replaceWith, isUserRule, isEnabled) { EnabledChanging = OnRuleEnabledChanging };
    }

    private bool OnRuleEnabledChanging(RegexRuleItem item, bool enabled)
    {
        if (_replaceList == null || !_replaceList.SetRegexRuleEnabled(item.Find, item.ReplaceWith, enabled))
        {
            return false;
        }

        UpdateEnabledCount();
        if (SelectedShowIndex is 1 or 2)
        {
            ApplyFilter();
        }

        UpdateTest();
        return true;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedShowIndexChanged(int value) => ApplyFilter();

    partial void OnSelectedRuleChanged(RegexRuleItem? value)
    {
        if (value != null)
        {
            EditFind = value.Find;
            EditReplace = value.ReplaceWith;
        }

        UpdateEditorState();
    }

    partial void OnEditFindChanged(string value)
    {
        UpdateEditorState();
        UpdateTest();
    }

    partial void OnEditReplaceChanged(string value)
    {
        UpdateEditorState();
        UpdateTest();
    }

    partial void OnTestTextChanged(string value) => UpdateTest();
    partial void OnApplyAllEnabledChanged(bool value) => UpdateTest();

    private void ApplyFilter()
    {
        var search = SearchText.Trim();
        var selected = SelectedRule;
        var shown = _allRules.Where(r => SelectedShowIndex switch
        {
            1 => r.IsEnabled,
            2 => !r.IsEnabled,
            3 => r.IsUserRule,
            _ => true,
        });

        if (search.Length > 0)
        {
            shown = shown.Where(r => r.Find.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     r.ReplaceWith.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        Rules = new ObservableCollection<RegexRuleItem>(shown);
        if (selected != null && Rules.Contains(selected))
        {
            SelectedRule = selected;
        }
    }

    private void UpdateEnabledCount()
    {
        EnabledCountText = string.Format(CultureInfo.CurrentCulture, Se.Language.Options.WordLists.XOfYEnabled,
            _allRules.Count(r => r.IsEnabled), _allRules.Count);
    }

    private void UpdateEditorState()
    {
        var l = Se.Language.Options.WordLists;
        var error = GetPatternError(EditFind);
        IsPatternValid = EditFind.Length > 0 && error == null;
        PatternStatus = EditFind.Length == 0 ? string.Empty : error == null ? l.ValidPattern : string.Format(l.InvalidPatternX, error);

        var exists = _allRules.Any(r => r.Find == EditFind);
        var selected = SelectedRule;
        var isChanged = selected != null && (selected.Find != EditFind || selected.ReplaceWith != EditReplace);
        CanAdd = IsPatternValid && !exists;
        CanUpdate = IsPatternValid && selected is { IsUserRule: true } && isChanged && (selected.Find == EditFind || !exists);
        CanDelete = selected is { IsUserRule: true };
        CanToggle = selected != null;

        if (selected is { IsUserRule: false } && isChanged)
        {
            EditorHint = exists ? l.RuleAlreadyExists : l.BuiltInRulesCanOnlyBeDisabled;
        }
        else if (selected == null && exists && EditFind.Length > 0)
        {
            EditorHint = l.RuleAlreadyExists;
        }
        else
        {
            EditorHint = string.Empty;
        }
    }

    private static string? GetPatternError(string pattern)
    {
        if (pattern.Length == 0)
        {
            return null;
        }

        try
        {
            _ = new Regex(pattern, RegexOptions.Multiline);
            return null;
        }
        catch (ArgumentException exception)
        {
            return exception.Message;
        }
    }

    private void UpdateTest()
    {
        var l = Se.Language.Options.WordLists;
        var text = TestText.Replace("\r\n", "\n");
        if (text.Length == 0)
        {
            InputSegments = Array.Empty<RegexTestSegment>();
            OutputSegments = Array.Empty<RegexTestSegment>();
            MatchInfo = string.Empty;
            return;
        }

        if (ApplyAllEnabled)
        {
            var changedBy = 0;
            var result = text;
            foreach (var rule in _allRules.Where(r => r.IsEnabled))
            {
                try
                {
                    var replaced = Regex.Replace(result, rule.Find, rule.ReplaceWith, RegexOptions.Multiline, TestTimeout);
                    if (replaced != result)
                    {
                        changedBy++;
                        result = replaced;
                    }
                }
                catch (Exception exception) when (exception is ArgumentException or RegexMatchTimeoutException)
                {
                    // An unusable rule is skipped here, as the OCR fixer would not get past it either.
                }
            }

            InputSegments = new[] { new RegexTestSegment(text, false) };
            OutputSegments = new[] { new RegexTestSegment(result, result != text) };
            MatchInfo = string.Format(CultureInfo.CurrentCulture, l.XRulesChangedText, changedBy);
            return;
        }

        if (!IsPatternValid)
        {
            InputSegments = new[] { new RegexTestSegment(text, false) };
            OutputSegments = Array.Empty<RegexTestSegment>();
            MatchInfo = string.Empty;
            return;
        }

        try
        {
            var regex = new Regex(EditFind, RegexOptions.Multiline, TestTimeout);
            var matches = regex.Matches(text);
            var input = new List<RegexTestSegment>();
            var output = new List<RegexTestSegment>();
            var position = 0;
            foreach (Match match in matches)
            {
                if (match.Index > position)
                {
                    var between = text.Substring(position, match.Index - position);
                    input.Add(new RegexTestSegment(between, false));
                    output.Add(new RegexTestSegment(between, false));
                }

                input.Add(new RegexTestSegment(match.Value, true));
                output.Add(new RegexTestSegment(match.Result(EditReplace), true));
                position = match.Index + match.Length;
            }

            if (position < text.Length)
            {
                var rest = text.Substring(position);
                input.Add(new RegexTestSegment(rest, false));
                output.Add(new RegexTestSegment(rest, false));
            }

            InputSegments = input;
            OutputSegments = output;
            MatchInfo = matches.Count switch
            {
                0 => l.NoMatches,
                1 => l.OneMatch,
                _ => string.Format(CultureInfo.CurrentCulture, l.XMatches, matches.Count),
            };
        }
        catch (Exception exception) when (exception is ArgumentException or RegexMatchTimeoutException)
        {
            InputSegments = new[] { new RegexTestSegment(text, false) };
            OutputSegments = Array.Empty<RegexTestSegment>();
            MatchInfo = exception.Message;
        }
    }

    private async Task<bool> SetEnabled(IReadOnlyCollection<RegexRuleItem> items, bool enabled)
    {
        var changing = items.Where(r => r.IsEnabled != enabled).ToList();
        if (_replaceList == null || changing.Count == 0)
        {
            return true;
        }

        var pairs = changing.Select(r => new KeyValuePair<string, string>(r.Find, r.ReplaceWith)).ToList();
        if (!_replaceList.SetRegexRulesEnabled(pairs, enabled))
        {
            await ShowError(_replaceList.ErrorMessage);
            return false;
        }

        foreach (var item in changing)
        {
            item.SetEnabledWithoutSaving(enabled);
        }

        UpdateEnabledCount();
        ApplyFilter();
        UpdateTest();
        return true;
    }

    [RelayCommand]
    private async Task EnableShown() => await SetEnabled(Rules.ToList(), true);

    [RelayCommand]
    private async Task DisableShown() => await SetEnabled(Rules.ToList(), false);

    [RelayCommand]
    private async Task ToggleSelected()
    {
        if (SelectedRule is { } rule)
        {
            await SetEnabled(new[] { rule }, !rule.IsEnabled);
        }
    }

    [RelayCommand]
    private void NewRule()
    {
        SelectedRule = null;
        EditFind = string.Empty;
        EditReplace = string.Empty;
    }

    [RelayCommand]
    private async Task AddRule()
    {
        UpdateEditorState();
        if (_replaceList == null || !CanAdd)
        {
            return;
        }

        if (!_replaceList.AddRegexRule(EditFind, EditReplace))
        {
            await ShowError(Se.Language.Options.WordLists.UnableToAddItem);
            return;
        }

        var item = MakeItem(EditFind, EditReplace, true, true);
        _allRules.Add(item);
        if (SelectedShowIndex == 2)
        {
            SelectedShowIndex = 0;
        }

        SearchText = string.Empty;
        ApplyFilter();
        SelectedRule = item;
        UpdateEnabledCount();
    }

    [RelayCommand]
    private async Task UpdateRule()
    {
        UpdateEditorState();
        var selected = SelectedRule;
        if (_replaceList == null || selected == null || !CanUpdate)
        {
            return;
        }

        if (!_replaceList.UpdateUserRegexRule(selected.Find, EditFind, EditReplace))
        {
            await ShowError(Se.Language.Options.WordLists.UnableToAddItem);
            return;
        }

        selected.Find = EditFind;
        selected.ReplaceWith = EditReplace;
        UpdateEditorState();
        UpdateTest();
    }

    [RelayCommand]
    private async Task DeleteRule()
    {
        var selected = SelectedRule;
        if (_replaceList == null || selected is not { IsUserRule: true })
        {
            return;
        }

        if (!_replaceList.RemoveUserRegexRule(selected.Find))
        {
            await ShowError(Se.Language.Options.WordLists.UnableToRemoveItem);
            return;
        }

        var index = Rules.IndexOf(selected);
        _allRules.Remove(selected);
        ApplyFilter();
        SelectedRule = Rules.Count == 0 ? null : Rules[Math.Clamp(index, 0, Rules.Count - 1)];
        UpdateEnabledCount();
        UpdateTest();
    }

    [RelayCommand]
    private async Task CopyPattern()
    {
        if (Window != null && SelectedRule is { } rule)
        {
            await ClipboardHelper.SetTextAsync(Window, rule.Find);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    private async Task ShowError(string message)
    {
        if (Window == null)
        {
            return;
        }

        await MessageBox.Show(Window, Se.Language.General.Error,
            string.IsNullOrEmpty(message) ? Se.Language.Options.WordLists.UnableToAddItem : message,
            MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            UiUtil.ShowHelp("features/word-lists");
        }
    }

    internal void RuleListKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            _ = ToggleSelected();
        }
        else if (e.Key == Key.Delete && SelectedRule is { IsUserRule: true })
        {
            e.Handled = true;
            _ = DeleteRule();
        }
    }

    internal static string Describe(IEnumerable<RegexTestSegment> segments)
    {
        var sb = new StringBuilder();
        foreach (var segment in segments)
        {
            sb.Append(segment.Text);
        }

        return sb.ToString();
    }
}

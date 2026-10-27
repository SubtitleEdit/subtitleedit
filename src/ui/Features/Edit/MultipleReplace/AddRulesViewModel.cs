using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

/// <summary>
/// "Add rules": type a find/replace pair, Enter puts it on the list and the cursor back in
/// "Find what" for the next one, OK adds the whole list to the category. Subtitle Edit 4 had
/// the entry boxes in the Multiple replace window itself; a dialog per rule meant that a list
/// of 20 words took 20 rounds of "New rule" (#16215).
/// </summary>
public partial class AddRulesViewModel : ObservableObject
{
    [ObservableProperty] private string _findWhat;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWholeWordEnabled))]
    private bool _isRegularExpression;
    [ObservableProperty] private bool _isCaseSensitive;
    [ObservableProperty] private bool _isCaseInsensitive;
    [ObservableProperty] private bool _isWholeWord;
    [ObservableProperty] private MultipleReplaceRule? _selectedRule;
    [ObservableProperty] private string _rulesInfo;

    /// <summary>
    /// The rules added so far, in the order they were typed - the order they get in the category.
    /// </summary>
    public ObservableCollection<MultipleReplaceRule> Rules { get; }

    public bool IsWholeWordEnabled => !IsRegularExpression;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Title { get; private set; }

    /// <summary>
    /// Set by the window: puts keyboard focus back in "Find what" after a rule was added.
    /// </summary>
    public Action? FocusFindWhat { get; set; }

    public AddRulesViewModel()
    {
        FindWhat = string.Empty;
        ReplaceWith = string.Empty;
        IsCaseInsensitive = true;
        Rules = new ObservableCollection<MultipleReplaceRule>();
        Rules.CollectionChanged += (_, _) => UpdateRulesInfo();
        RulesInfo = string.Empty;
        Title = Se.Language.Edit.MultipleReplace.AddRules;
        UpdateRulesInfo();
    }

    /// <summary>
    /// The type and whole-word setting start as the neighbouring rule's: a list of words usually
    /// shares them, so they are set once and stick for every rule typed.
    /// </summary>
    public void Initialize(string categoryName, MultipleReplaceType type, bool wholeWord)
    {
        Title = string.Format(Se.Language.Edit.MultipleReplace.AddRulesToX, categoryName);
        SetType(type);
        IsWholeWord = wholeWord;
    }

    public MultipleReplaceType Type =>
        IsRegularExpression ? MultipleReplaceType.RegularExpression :
        IsCaseSensitive ? MultipleReplaceType.CaseSensitive :
        MultipleReplaceType.CaseInsensitive;

    private void SetType(MultipleReplaceType type)
    {
        IsRegularExpression = type == MultipleReplaceType.RegularExpression;
        IsCaseSensitive = type == MultipleReplaceType.CaseSensitive;
        IsCaseInsensitive = type == MultipleReplaceType.CaseInsensitive;
    }

    private void UpdateRulesInfo()
    {
        RulesInfo = string.Format(Se.Language.Edit.MultipleReplace.XRules, Rules.Count);
    }

    /// <summary>
    /// Puts the typed pair on the list and clears the boxes for the next one. Nothing to find is
    /// nothing to add - the cursor just goes back to "Find what". The text is kept as typed:
    /// a leading or trailing space can be the point of a rule.
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        if (!string.IsNullOrEmpty(FindWhat))
        {
            Rules.Add(MakeRule());
            FindWhat = string.Empty;
            ReplaceWith = string.Empty;
        }

        FocusFindWhat?.Invoke();
    }

    private MultipleReplaceRule MakeRule()
    {
        return new MultipleReplaceRule
        {
            Active = true,
            Find = FindWhat,
            ReplaceWith = ReplaceWith ?? string.Empty,
            Description = string.Empty,
            Type = Type,
            WholeWord = IsWholeWord && !IsRegularExpression,
        };
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        var rule = SelectedRule;
        if (rule == null)
        {
            return;
        }

        var index = Rules.IndexOf(rule);
        Rules.Remove(rule);
        if (Rules.Count > 0)
        {
            SelectedRule = Rules[Math.Min(index, Rules.Count - 1)];
        }
    }

    /// <summary>
    /// Double-click on a row: takes the rule back into the boxes so a typo can be fixed and the
    /// rule added again. It goes to the end of the list then.
    /// </summary>
    [RelayCommand]
    private void EditSelected()
    {
        var rule = SelectedRule;
        if (rule == null)
        {
            return;
        }

        Rules.Remove(rule);
        FindWhat = rule.Find;
        ReplaceWith = rule.ReplaceWith;
        SetType(rule.Type);
        IsWholeWord = rule.WholeWord;
        FocusFindWhat?.Invoke();
    }

    [RelayCommand]
    private void Ok()
    {
        // Text still in the boxes is a rule that was typed but not entered - OK means "add it".
        if (!string.IsNullOrEmpty(FindWhat))
        {
            Rules.Add(MakeRule());
            FindWhat = string.Empty;
            ReplaceWith = string.Empty;
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        // Escape after typing 20 rules must not just throw them away.
        if (Rules.Count > 0 && Window != null)
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.DiscardChangesTitle,
                string.Format(Se.Language.Edit.MultipleReplace.DiscardXRules, Rules.Count),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _ = Cancel();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/edit", "multiple-replace");
        }
        else if ((e.Key == Key.Enter || e.Key == Key.Return) &&
                 e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta)
        {
            // Ctrl+Enter: done, add the list (and whatever is still in the boxes) to the category.
            e.Handled = true;
            Ok();
        }
    }

    /// <summary>
    /// Enter in "Find what" or "Replace with" adds the rule, as in Subtitle Edit 4.
    /// </summary>
    internal void OnEntryKeyDown(object? sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Enter || e.Key == Key.Return) && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Add();
        }
    }

    internal void OnListKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            RemoveSelected();
        }
        else if ((e.Key == Key.Enter || e.Key == Key.Return) && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            EditSelected();
        }
    }
}

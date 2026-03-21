using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class FindRuleViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<RuleTreeNode> _rules;
    [ObservableProperty] private RuleTreeNode? _selectedRule;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private List<RuleTreeNode> _allRules = new();

    public FindRuleViewModel()
    {
        _searchText = string.Empty;
        _rules = new ObservableCollection<RuleTreeNode>();
    }

    public void Initialize(IEnumerable<RuleTreeNode> nodes)
    {
        _allRules = nodes
            .Where(n => n.IsCategory && n.SubNodes != null)
            .SelectMany(n => n.SubNodes!)
            .ToList();

        FilterRules();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterRules();
    }

    private void FilterRules()
    {
        var filtered = string.IsNullOrEmpty(SearchText)
            ? _allRules
            : _allRules.Where(r =>
                r.Find.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                r.ReplaceWith.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (r.Parent?.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        Rules.Clear();
        foreach (var rule in filtered)
        {
            Rules.Add(rule);
        }

        SelectedRule = Rules.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedRule == null)
        {
            return;
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            OkCommand.Execute(null);
        }
    }

    internal void DataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        OkCommand.Execute(null);
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

/// <summary>
/// Ticks off which rule categories an export writes out, or which ones an import brings in
/// (#13529 - before this the import took every category in the file, no questions asked).
/// </summary>
public partial class CategoryPickerViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<RuleTreeNode> _rules;
    [ObservableProperty] private RuleTreeNode? _selectedRule;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    // Read by the window constructor - the view model is initialized before the window is built.
    public string Title { get; private set; }

    private string _nothingSelectedMessage;

    public CategoryPickerViewModel()
    {
        Rules = new ObservableCollection<RuleTreeNode>();
        Title = Se.Language.Edit.MultipleReplace.ExportReplaceRules;
        _nothingSelectedMessage = Se.Language.Edit.MultipleReplace.NoCategoriesSelected;
    }

    /// <summary>Export: only the category the export was started from is ticked.</summary>
    public void InitializeForExport(List<RuleTreeNode> rules, RuleTreeNode? selectedNode)
    {
        Title = Se.Language.Edit.MultipleReplace.ExportReplaceRules;
        _nothingSelectedMessage = Se.Language.Edit.MultipleReplace.NoCategoriesSelected;
        Initialize(rules, node => node == selectedNode);
    }

    /// <summary>Import: everything found in the file is ticked, so plain OK keeps the old behavior.</summary>
    public void InitializeForImport(List<RuleTreeNode> rules)
    {
        Title = Se.Language.Edit.MultipleReplace.ImportReplaceRules;
        _nothingSelectedMessage = Se.Language.Edit.MultipleReplace.NoCategoriesSelected;
        Initialize(rules, _ => true);
    }

    private void Initialize(List<RuleTreeNode> rules, Func<RuleTreeNode, bool> isSelected)
    {
        Rules.Clear();
        foreach (var rule in rules)
        {
            rule.IsSelected = isSelected(rule);
            Rules.Add(rule);
        }

        SelectedRule = Rules.FirstOrDefault(p => p.IsSelected) ?? Rules.FirstOrDefault();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var rule in Rules)
        {
            rule.IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var rule in Rules)
        {
            rule.IsSelected = false;
        }
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var rule in Rules)
        {
            rule.IsSelected = !rule.IsSelected;
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        if (!Rules.Any(p => p.IsSelected))
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                _nothingSelectedMessage,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
            return;
        }

        // Ctrl/Cmd + A / D / Shift+I, the same three gestures the other tick-a-column lists use
        // (see RemoveTextForHearingImpairedViewModel.HandleFixesSelectionKey).
        var isCommand = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        if (!isCommand || e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return;
        }

        var isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (e.Key == Key.A && !isShift)
        {
            SelectAll();
        }
        else if (e.Key == Key.D && !isShift)
        {
            SelectNone();
        }
        else if (e.Key == Key.I && isShift)
        {
            InvertSelection();
        }
        else
        {
            return;
        }

        e.Handled = true;
    }
}

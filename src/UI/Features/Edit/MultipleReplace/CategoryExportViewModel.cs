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

public partial class CategoryExportViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<RuleTreeNode> _rules;
    [ObservableProperty] private RuleTreeNode? _selectedRule;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public CategoryExportViewModel()
    {
        Rules = new ObservableCollection<RuleTreeNode>();
    }

    public void Initialize(List<RuleTreeNode> rules, RuleTreeNode selectedNode)
    {
        Rules.Clear();
        foreach (var rule in rules)
        {
            rule.IsSelected = rule == selectedNode;
            Rules.Add(rule);
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        if (Enumerable.Where<RuleTreeNode>(Rules, p => p.IsSelected).Count() == 0)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                $"No rule categories selected for export",
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
        }
    }
}
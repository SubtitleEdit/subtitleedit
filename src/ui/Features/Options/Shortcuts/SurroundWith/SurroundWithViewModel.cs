using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.SurroundWith;

public partial class SurroundWithViewModel : ObservableObject
{
    [ObservableProperty] private string _before;
    [ObservableProperty] private string _after;
    [ObservableProperty] private ObservableCollection<SurroundWithBehaviorItem> _behaviors;
    [ObservableProperty] private SurroundWithBehaviorItem _selectedBehavior;
    [ObservableProperty] private ObservableCollection<SurroundWithScopeItem> _scopes;
    [ObservableProperty] private SurroundWithScopeItem _selectedScope;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public SurroundWithViewModel()
    {
        Before = string.Empty;
        After = string.Empty;
        Behaviors = new ObservableCollection<SurroundWithBehaviorItem>
        {
            new(SurroundWithBehavior.Toggle, Se.Language.Options.Shortcuts.SurroundWithBehaviorToggle),
            new(SurroundWithBehavior.Add, Se.Language.Options.Shortcuts.SurroundWithBehaviorAdd),
            new(SurroundWithBehavior.Remove, Se.Language.Options.Shortcuts.SurroundWithBehaviorRemove),
            new(SurroundWithBehavior.RemoveOnce, Se.Language.Options.Shortcuts.SurroundWithBehaviorRemoveOnce),
        };
        SelectedBehavior = Behaviors[0];
        Scopes = new ObservableCollection<SurroundWithScopeItem>
        {
            new(SurroundWithScope.SelectionOrText, Se.Language.Options.Shortcuts.SurroundWithScopeSelectionOrText),
            new(SurroundWithScope.EachLine, Se.Language.Options.Shortcuts.SurroundWithScopeEachLine),
        };
        SelectedScope = Scopes[0];
    }

    public SurroundWithBehavior Behavior => SelectedBehavior?.Behavior ?? SurroundWithBehavior.Toggle;

    public SurroundWithScope Scope => SelectedScope?.Scope ?? SurroundWithScope.SelectionOrText;

    public void Initialize(string before, string after, SurroundWithBehavior behavior, SurroundWithScope scope)
    {
        Before = before;
        After = after;
        SelectedBehavior = Behaviors.FirstOrDefault(b => b.Behavior == behavior) ?? Behaviors[0];
        SelectedScope = Scopes.FirstOrDefault(s => s.Scope == scope) ?? Scopes[0];
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
            return;
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/shortcuts");
        }
    }
}

public class SurroundWithBehaviorItem
{
    public SurroundWithBehavior Behavior { get; }
    public string Name { get; }

    public SurroundWithBehaviorItem(SurroundWithBehavior behavior, string name)
    {
        Behavior = behavior;
        Name = name;
    }

    public override string ToString() => Name;
}

public class SurroundWithScopeItem
{
    public SurroundWithScope Scope { get; }
    public string Name { get; }

    public SurroundWithScopeItem(SurroundWithScope scope, string name)
    {
        Scope = scope;
        Name = name;
    }

    public override string ToString() => Name;
}

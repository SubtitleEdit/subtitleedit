using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;

public class CustomShortcutStepItem
{
    public SeCustomShortcutStep Step { get; }
    public string Summary { get; }

    public CustomShortcutStepItem(SeCustomShortcutStep step, string summary)
    {
        Step = step;
        Summary = summary;
    }

    public override string ToString() => Summary;
}

public partial class CustomShortcutEditViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private ObservableCollection<CustomShortcutStepItem> _steps;
    [ObservableProperty] private CustomShortcutStepItem? _selectedStep;
    [ObservableProperty] private bool _isStepSelected;
    [ObservableProperty] private ObservableCollection<CustomShortcutChoice<ShortcutCategory>> _activeInChoices;
    [ObservableProperty] private CustomShortcutChoice<ShortcutCategory> _selectedActiveIn;
    [ObservableProperty] private bool _isTextBoxKeyHintVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public SeCustomShortcut CustomShortcut { get; private set; }

    private readonly IWindowService _windowService;
    private IReadOnlyList<CustomShortcutCommandItem> _commands;
    private Dictionary<string, CustomShortcutCommandItem> _commandLookup;

    public CustomShortcutEditViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        Name = string.Empty;
        Steps = new ObservableCollection<CustomShortcutStepItem>();
        ActiveInChoices = new ObservableCollection<CustomShortcutChoice<ShortcutCategory>>(CustomShortcutDisplay.GetActiveInChoices());
        SelectedActiveIn = ActiveInChoices[0];
        CustomShortcut = new SeCustomShortcut();
        _commands = new List<CustomShortcutCommandItem>();
        _commandLookup = new Dictionary<string, CustomShortcutCommandItem>();
    }

    public string Title { get; private set; } = string.Empty;

    public void Initialize(SeCustomShortcut customShortcut, IReadOnlyList<CustomShortcutCommandItem> commands, string title)
    {
        Title = title;
        CustomShortcut = customShortcut.Clone();
        _commands = commands;
        _commandLookup = commands.GroupBy(p => p.ActionName).ToDictionary(g => g.Key, g => g.First());
        Name = CustomShortcut.Name;
        SelectedActiveIn = ActiveInChoices.FirstOrDefault(p => p.Value == CustomShortcut.GetActiveIn()) ?? ActiveInChoices[0];
        Steps.Clear();
        foreach (var step in CustomShortcut.Steps)
        {
            Steps.Add(MakeItem(step));
        }

        SelectedStep = Steps.FirstOrDefault();
    }

    partial void OnSelectedActiveInChanged(CustomShortcutChoice<ShortcutCategory> value)
    {
        IsTextBoxKeyHintVisible = value?.Value is ShortcutCategory.TextBox or ShortcutCategory.SubtitleGridAndTextBox;
    }

    partial void OnSelectedStepChanged(CustomShortcutStepItem? value)
    {
        IsStepSelected = value != null;
    }

    private CustomShortcutStepItem MakeItem(SeCustomShortcutStep step)
    {
        return new CustomShortcutStepItem(step, CustomShortcutDisplay.GetStepSummary(step, _commandLookup));
    }

    [RelayCommand]
    private async Task AddStep()
    {
        var step = await ShowStepDialog(null);
        if (step == null)
        {
            return;
        }

        var item = MakeItem(step);
        var index = SelectedStep == null ? Steps.Count : Steps.IndexOf(SelectedStep) + 1;
        Steps.Insert(index, item);
        SelectedStep = item;
    }

    [RelayCommand]
    private async Task EditStep()
    {
        var selected = SelectedStep;
        if (selected == null)
        {
            return;
        }

        var step = await ShowStepDialog(selected.Step);
        if (step == null)
        {
            return;
        }

        var index = Steps.IndexOf(selected);
        var item = MakeItem(step);
        Steps[index] = item;
        SelectedStep = item;
    }

    private async Task<SeCustomShortcutStep?> ShowStepDialog(SeCustomShortcutStep? step)
    {
        if (Window == null)
        {
            return null;
        }

        var result = await _windowService.ShowDialogAsync<CustomShortcutStepWindow, CustomShortcutStepViewModel>(Window,
            vm => vm.Initialize(step, _commands));
        return result.OkPressed ? result.Step : null;
    }

    [RelayCommand]
    private void RemoveStep()
    {
        var selected = SelectedStep;
        if (selected == null)
        {
            return;
        }

        var index = Steps.IndexOf(selected);
        Steps.RemoveAt(index);
        SelectedStep = Steps.Count == 0 ? null : Steps[System.Math.Min(index, Steps.Count - 1)];
    }

    [RelayCommand]
    private void MoveStepUp()
    {
        MoveSelected(-1);
    }

    [RelayCommand]
    private void MoveStepDown()
    {
        MoveSelected(1);
    }

    private void MoveSelected(int delta)
    {
        var selected = SelectedStep;
        if (selected == null)
        {
            return;
        }

        var index = Steps.IndexOf(selected);
        var newIndex = index + delta;
        if (newIndex < 0 || newIndex >= Steps.Count)
        {
            return;
        }

        Steps.Move(index, newIndex);
        SelectedStep = selected;
    }

    // Name and steps are both optional: a slot without steps simply does nothing.
    [RelayCommand]
    private void Ok()
    {
        CustomShortcut.Name = Name?.Trim() ?? string.Empty;
        CustomShortcut.ActiveIn = (SelectedActiveIn?.Value ?? ShortcutCategory.General).ToString();
        CustomShortcut.Steps = Steps.Select(p => p.Step).ToList();
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
            UiUtil.ShowHelp("features/shortcuts");
        }
    }
}

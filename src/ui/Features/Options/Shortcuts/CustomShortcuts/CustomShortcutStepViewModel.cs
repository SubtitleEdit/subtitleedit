using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;

public partial class CustomShortcutStepViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CustomShortcutChoice<CustomShortcutStepType>> _stepTypes;
    [ObservableProperty] private CustomShortcutChoice<CustomShortcutStepType> _selectedStepType;
    [ObservableProperty] private bool _isRunCommand;
    [ObservableProperty] private bool _isInsertText;
    [ObservableProperty] private bool _isReplace;

    [ObservableProperty] private string _commandSearchText;
    [ObservableProperty] private ObservableCollection<CustomShortcutCommandItem> _filteredCommands;
    [ObservableProperty] private CustomShortcutCommandItem? _selectedCommand;

    [ObservableProperty] private string _text;
    [ObservableProperty] private ObservableCollection<CustomShortcutChoice<CustomShortcutInsertPosition>> _positions;
    [ObservableProperty] private CustomShortcutChoice<CustomShortcutInsertPosition> _selectedPosition;

    [ObservableProperty] private string _find;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty] private bool _useRegex;
    [ObservableProperty] private bool _caseSensitive;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public SeCustomShortcutStep Step { get; private set; }

    private List<CustomShortcutCommandItem> _allCommands;

    public CustomShortcutStepViewModel()
    {
        // Before any observable property: their change handlers read these.
        _allCommands = new List<CustomShortcutCommandItem>();
        FilteredCommands = new ObservableCollection<CustomShortcutCommandItem>();
        StepTypes = new ObservableCollection<CustomShortcutChoice<CustomShortcutStepType>>(CustomShortcutDisplay.GetStepTypes());
        SelectedStepType = StepTypes[0];
        Positions = new ObservableCollection<CustomShortcutChoice<CustomShortcutInsertPosition>>(CustomShortcutDisplay.GetInsertPositions());
        SelectedPosition = Positions[0];
        CommandSearchText = string.Empty;
        Text = string.Empty;
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Step = new SeCustomShortcutStep();
        UpdateVisibility();
    }

    public void Initialize(SeCustomShortcutStep? step, IReadOnlyList<CustomShortcutCommandItem> commands)
    {
        _allCommands = commands.ToList();
        step ??= new SeCustomShortcutStep();

        SelectedStepType = StepTypes.First(p => p.Value == step.GetStepType());
        UpdateFilteredCommands();
        SelectedCommand = _allCommands.FirstOrDefault(p => p.ActionName == step.ActionName);
        Text = step.Text;
        SelectedPosition = Positions.First(p => p.Value == step.GetPosition());
        Find = step.Find;
        ReplaceWith = step.ReplaceWith;
        UseRegex = step.UseRegex;
        CaseSensitive = step.CaseSensitive;
    }

    partial void OnSelectedStepTypeChanged(CustomShortcutChoice<CustomShortcutStepType> value)
    {
        UpdateVisibility();
    }

    partial void OnCommandSearchTextChanged(string value)
    {
        UpdateFilteredCommands();
    }

    private void UpdateVisibility()
    {
        var type = SelectedStepType?.Value ?? CustomShortcutStepType.RunCommand;
        IsRunCommand = type == CustomShortcutStepType.RunCommand;
        IsInsertText = type == CustomShortcutStepType.InsertText;
        IsReplace = type == CustomShortcutStepType.Replace;
    }

    private void UpdateFilteredCommands()
    {
        var selected = SelectedCommand;
        var search = CommandSearchText?.Trim() ?? string.Empty;
        FilteredCommands.Clear();
        foreach (var command in _allCommands)
        {
            if (search.Length == 0 ||
                command.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                command.GroupName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                command.ActionName.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredCommands.Add(command);
            }
        }

        if (selected != null && FilteredCommands.Contains(selected))
        {
            SelectedCommand = selected;
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        var language = Se.Language.Options.Shortcuts;
        var type = SelectedStepType.Value;
        var step = new SeCustomShortcutStep { Type = type.ToString() };
        switch (type)
        {
            case CustomShortcutStepType.RunCommand:
                if (SelectedCommand == null)
                {
                    await ShowError(language.CustomShortcutPickCommand);
                    return;
                }

                step.ActionName = SelectedCommand.ActionName;
                break;
            case CustomShortcutStepType.InsertText:
                if (string.IsNullOrEmpty(Text))
                {
                    await ShowError(language.CustomShortcutEnterText);
                    return;
                }

                step.Text = Text;
                step.Position = SelectedPosition.Value.ToString();
                break;
            case CustomShortcutStepType.Replace:
                if (string.IsNullOrEmpty(Find))
                {
                    await ShowError(language.CustomShortcutEnterFind);
                    return;
                }

                step.Find = Find;
                step.ReplaceWith = ReplaceWith ?? string.Empty;
                step.UseRegex = UseRegex;
                step.CaseSensitive = CaseSensitive;
                var regexError = CustomShortcutText.GetRegexError(step);
                if (regexError != null)
                {
                    await ShowError(string.Format(language.CustomShortcutInvalidRegexX, regexError));
                    return;
                }

                break;
        }

        Step = step;
        OkPressed = true;
        Window?.Close();
    }

    private async Task ShowError(string message)
    {
        if (Window != null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void CommandListDoubleTapped()
    {
        if (SelectedCommand != null)
        {
            OkCommand.Execute(null);
        }
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

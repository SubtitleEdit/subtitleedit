using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts.PickMilliseconds;
using Nikse.SubtitleEdit.Features.Options.Shortcuts.SurroundWith;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{
    public ObservableCollection<ShortcutTreeNode> FlatNodes { get; } = new();
    [ObservableProperty] private ObservableCollection<string> _shortcuts;
    [ObservableProperty] private string? _selectedShortcut;
    [ObservableProperty] private ObservableCollection<string> _filters;
    [ObservableProperty] private string _selectedFilter;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _isControlsEnabled;
    [ObservableProperty] private bool _ctrlIsSelected;
    [ObservableProperty] private bool _altIsSelected;
    [ObservableProperty] private bool _shiftIsSelected;
    [ObservableProperty] private bool _winIsSelected;
    [ObservableProperty] private bool _isConfigureVisible;
    [ObservableProperty] private ShortcutTreeNode? _selectedNode;

    public bool OkPressed { get; set; }
    public Window? Window { get; set; }
    public MainViewModel? MainViewModel { get; set; }

    private IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private List<ShortCut> _allShortcuts;
    private List<IRelayCommand> _configurableCommands;
    private Color _color1;
    private Color _color2;
    private Color _color3;
    private Color _color4;
    private Color _color5;
    private Color _color6;
    private Color _color7;
    private Color _color8;
    private string _surround1Left;
    private string _surround1Right;
    private string _surround2Left;
    private string _surround2Right;
    private string _surround3Left;
    private string _surround3Right;

    // Add this flag to prevent updates during selection changes
    private bool _isLoadingSelection = false;

    public ShortcutsViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        SearchText = string.Empty;
        Shortcuts = new ObservableCollection<string>(GetShortcutKeys());
        Filters = new ObservableCollection<string>
        {
            Se.Language.General.All,
            Se.Language.Options.Shortcuts.Assigned,
            Se.Language.Options.Shortcuts.Unassigned,
        };
        SelectedFilter = _filters[0];
        _allShortcuts = new List<ShortCut>();
        _configurableCommands = new List<IRelayCommand>();
        _color1 = Se.Settings.Color1.FromHexToColor();
        _color2 = Se.Settings.Color2.FromHexToColor();
        _color3 = Se.Settings.Color3.FromHexToColor();
        _color4 = Se.Settings.Color4.FromHexToColor();
        _color5 = Se.Settings.Color5.FromHexToColor();
        _color6 = Se.Settings.Color6.FromHexToColor();
        _color7 = Se.Settings.Color7.FromHexToColor();
        _color8 = Se.Settings.Color8.FromHexToColor();
        _surround1Left = Se.Settings.Surround1Left;
        _surround1Right = Se.Settings.Surround1Right;
        _surround2Left = Se.Settings.Surround2Left;
        _surround2Right = Se.Settings.Surround2Right;
        _surround3Left = Se.Settings.Surround3Left;
        _surround3Right = Se.Settings.Surround3Right;
    }

    partial void OnCtrlIsSelectedChanged(bool value)
    {
        if (!_isLoadingSelection)
        {
            UpdateShortcutDo();
        }
    }

    partial void OnAltIsSelectedChanged(bool value)
    {
        if (!_isLoadingSelection)
        {
            UpdateShortcutDo();
        }
    }

    partial void OnShiftIsSelectedChanged(bool value)
    {
        if (!_isLoadingSelection)
        {
            UpdateShortcutDo();
        }
    }

    partial void OnWinIsSelectedChanged(bool value)
    {
        if (!_isLoadingSelection)
        {
            UpdateShortcutDo();
        }
    }

    partial void OnSelectedShortcutChanged(string? value)
    {
        if (!_isLoadingSelection)
        {
            UpdateShortcutDo();
        }
    }

    private static List<string> GetShortcutKeys()
    {
        var result = new List<string>();
        var all = Enum.GetValues(typeof(Key)).Cast<Key>().Select(p => p.ToString()).Distinct();
        foreach (var key in all)
        {
            if (key == nameof(Key.None) ||
                key == nameof(Key.LeftCtrl) ||
                key == nameof(Key.RightCtrl) ||
                key == nameof(Key.LeftAlt) ||
                key == nameof(Key.RightAlt) ||
                key == nameof(Key.LeftShift) ||
                key == nameof(Key.RightShift))
            {
                continue;
            }

            result.Add(key);
        }

        return result;
    }

    public void LoadShortCuts(MainViewModel vm)
    {
        MainViewModel = vm;
        _allShortcuts = ShortcutsMain.GetAllShortcuts(vm);
        UpdateVisibleShortcuts(string.Empty);

        _configurableCommands.Add(vm.SetColor1Command);
        _configurableCommands.Add(vm.SetColor2Command);
        _configurableCommands.Add(vm.SetColor3Command);
        _configurableCommands.Add(vm.SetColor4Command);
        _configurableCommands.Add(vm.SetColor5Command);
        _configurableCommands.Add(vm.SetColor6Command);
        _configurableCommands.Add(vm.SetColor7Command);
        _configurableCommands.Add(vm.SetColor8Command);
        _configurableCommands.Add(vm.SurroundWith1Command);
        _configurableCommands.Add(vm.SurroundWith2Command);
        _configurableCommands.Add(vm.SurroundWith3Command);
        _configurableCommands.Add(vm.VideoMoveCustom1BackCommand);
        _configurableCommands.Add(vm.VideoMoveCustom1ForwardCommand);
        _configurableCommands.Add(vm.VideoMoveCustom2BackCommand);
        _configurableCommands.Add(vm.VideoMoveCustom2ForwardCommand);
    }

    internal void UpdateVisibleShortcuts(string searchText)
    {
        FlatNodes.Clear();
        AddShortcuts(ShortcutCategory.General, Se.Language.Options.Shortcuts.CategoryGeneral, searchText);
        AddShortcuts(ShortcutCategory.SubtitleGridAndTextBox,
            Se.Language.Options.Shortcuts.CategorySubtitleGridAndTextBox, searchText);
        AddShortcuts(ShortcutCategory.SubtitleGrid, Se.Language.Options.Shortcuts.CategorySubtitleGrid, searchText);
        AddShortcuts(ShortcutCategory.Waveform, Se.Language.Options.Shortcuts.CategoryWaveform, searchText);
    }

    private void AddShortcuts(ShortcutCategory category, string categoryName, string searchText)
    {
        var shortcuts = _allShortcuts.Where(p => p.Category == category && Search(searchText, p)).ToList();

        foreach (var x in shortcuts)
        {
            var leaf = new ShortcutTreeNode(categoryName, MakeDisplayName(x, false), MakeDisplayShortCut(x), x);
            FlatNodes.Add(leaf);
        }
    }

    private static string MakeDisplayName(ShortCut x, bool includeShortCutKeys = true)
    {
        var name = ShortcutsMain.CommandTranslationLookup.TryGetValue(x.Name, out var displayName)
            ? displayName
            : x.Name;

        if (includeShortCutKeys)
        {
            return name + " " + MakeDisplayShortCut(x);
        }

        return name;
    }

    private static string MakeDisplayShortCut(ShortCut shortCut)
    {
        if (shortCut.Keys.Count > 0)
        {
            var keys = shortCut.Keys.Select(k => ShortcutManager.GetKeyDisplayName(k)).ToList();
            return string.Join(" + ", keys);
        }

        return string.Empty;
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Options.Shortcuts.ImportShortcutsTitle, "Shortcuts Files", ".shortcuts");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            var json = await System.IO.File.ReadAllTextAsync(fileName, System.Text.Encoding.UTF8);
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            var importedShortcuts = System.Text.Json.JsonSerializer.Deserialize<List<SeShortCut>>(json, options);
            if (importedShortcuts == null || importedShortcuts.Count == 0)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, "No shortcuts found in file.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var importCount = 0;
            foreach (var importedShortcut in importedShortcuts)
            {
                // Remove existing shortcut with same action name
                var existing = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == importedShortcut.ActionName);
                if (existing != null)
                {
                    Se.Settings.Shortcuts.Remove(existing);
                }

                Se.Settings.Shortcuts.Add(importedShortcut);
                importCount++;
            }

            // Reload shortcuts in UI
            if (MainViewModel != null)
            {
                _allShortcuts = ShortcutsMain.GetAllShortcuts(MainViewModel);
                UpdateVisibleShortcuts(string.Empty);
            }

            await MessageBox.Show(Window, Se.Language.General.Information,
                string.Format(Se.Language.Options.Shortcuts.XShortcutsImportedFromY, importCount, System.IO.Path.GetFileName(fileName)),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                $"Failed to import shortcuts:\r\n{ex.Message}",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, "shortcuts", "se.shortcuts", Se.Language.Options.Shortcuts.ExportShortcutsTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            // Get all configured shortcuts
            var shortcuts = new List<SeShortCut>();
            foreach (var shortcut in _allShortcuts)
            {
                if (shortcut != null) // && !IsEmpty(shortcut))
                {
                    shortcuts.Add(new SeShortCut(shortcut));
                }
            }

            // Serialize to JSON
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(shortcuts, options);
            await System.IO.File.WriteAllTextAsync(fileName, json, System.Text.Encoding.UTF8);

            _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
                vm =>
                {
                    vm.Initialize(Se.Language.General.FileSaved,
                        string.Format(Se.Language.Options.Shortcuts.XShortcutsExportedToY, shortcuts.Count, System.IO.Path.GetFileName(fileName)), fileName, true, true);
                });
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                $"Failed to export shortcuts:\r\n{ex.Message}",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private async Task CommandOk()
    {
        if (Window == null)
        {
            return;
        }

        var duplicates = FindDuplicateShortcuts();
        if (duplicates.Any())
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Se.Language.Options.Shortcuts.DuplicatesFound);
            sb.AppendLine();
            foreach (var duplicate in duplicates)
            {
                sb.AppendLine($"• {duplicate}");
            }
            sb.AppendLine();
            sb.Append("Save anyway?");

            var answer = await MessageBox.Show(
                      Window!,
                      Se.Language.General.Question,
                      sb.ToString(),
                      MessageBoxButtons.YesNoCancel,
                      MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var shortcuts = new List<SeShortCut>();
        foreach (var shortcut in _allShortcuts)
        {
            if (shortcut != null && !IsEmpty(shortcut))
            {
                shortcuts.Add(new SeShortCut(shortcut));
            }
        }

        Se.Settings.Shortcuts = shortcuts;

        Se.Settings.Color1 = _color1.FromColorToHex();
        Se.Settings.Color2 = _color2.FromColorToHex();
        Se.Settings.Color3 = _color3.FromColorToHex();
        Se.Settings.Color4 = _color4.FromColorToHex();
        Se.Settings.Color5 = _color5.FromColorToHex();
        Se.Settings.Color6 = _color6.FromColorToHex();
        Se.Settings.Color7 = _color7.FromColorToHex();
        Se.Settings.Color8 = _color8.FromColorToHex();
        Se.Settings.Surround1Left = _surround1Left;
        Se.Settings.Surround1Right = _surround1Right;
        Se.Settings.Surround2Left = _surround2Left;
        Se.Settings.Surround2Right = _surround2Right;
        Se.Settings.Surround3Left = _surround3Left;
        Se.Settings.Surround3Right = _surround3Right;

        ShortcutsMain.CommandTranslationLookup[nameof(MainViewModel.SurroundWith1Command)] = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround1Left, Se.Settings.Surround1Right);
        ShortcutsMain.CommandTranslationLookup[nameof(MainViewModel.SurroundWith2Command)] = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround2Left, Se.Settings.Surround2Right);
        ShortcutsMain.CommandTranslationLookup[nameof(MainViewModel.SurroundWith3Command)] = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround3Left, Se.Settings.Surround3Right);

        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task Configure()
    {
        var node = SelectedNode;
        if (Window == null || MainViewModel == null || node?.ShortCut == null)
        {
            return;
        }

        if (node.ShortCut.Action == MainViewModel.SetColor1Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color1);
            });
            if (result.OkPressed)
            {
                _color1 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor2Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color2);
            });
            if (result.OkPressed)
            {
                _color2 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor3Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color3);
            });
            if (result.OkPressed)
            {
                _color3 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor4Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color4);
            });
            if (result.OkPressed)
            {
                _color4 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor5Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color5);
            });
            if (result.OkPressed)
            {
                _color5 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor6Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color6);
            });
            if (result.OkPressed)
            {
                _color6 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor7Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color7);
            });
            if (result.OkPressed)
            {
                _color7 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SetColor8Command)
        {
            var result = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(Window, vm =>
            {
                vm.Initialize(_color8);
            });
            if (result.OkPressed)
            {
                _color8 = result.SelectedColor;
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SurroundWith1Command)
        {
            var result = await _windowService.ShowDialogAsync<SurroundWithWindow, SurroundWithViewModel>(Window, vm =>
            {
                vm.Initialize(_surround1Left, _surround1Right);
            });
            if (result.OkPressed)
            {
                _surround1Left = result.Before;
                _surround1Right = result.After;

                var flatNodeBack = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.SurroundWith1Command);
                if (flatNodeBack != null)
                {
                    flatNodeBack.Title = string.Format(string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, _surround1Left, _surround1Right));
                }
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SurroundWith2Command)
        {
            var result = await _windowService.ShowDialogAsync<SurroundWithWindow, SurroundWithViewModel>(Window, vm =>
            {
                vm.Initialize(_surround2Left, _surround2Right);
            });
            if (result.OkPressed)
            {
                _surround2Left = result.Before;
                _surround2Right = result.After;

                var flatNodeBack = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.SurroundWith2Command);
                if (flatNodeBack != null)
                {
                    flatNodeBack.Title = string.Format(string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, _surround2Left, _surround2Right));
                }
            }
        }
        else if (node.ShortCut.Action == MainViewModel.SurroundWith3Command)
        {
            var result = await _windowService.ShowDialogAsync<SurroundWithWindow, SurroundWithViewModel>(Window, vm =>
            {
                vm.Initialize(_surround3Left, _surround3Right);
            });
            if (result.OkPressed)
            {
                _surround3Left = result.Before;
                _surround3Right = result.After;

                var flatNodeBack = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.SurroundWith3Command);
                if (flatNodeBack != null)
                {
                    flatNodeBack.Title = string.Format(string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, _surround3Left, _surround3Right));
                }
            }
        }
        else if (node.ShortCut.Action == MainViewModel.VideoMoveCustom1BackCommand)
        {
            var result = await _windowService.ShowDialogAsync<PickMillisecondsWindow, PickMillisecondsViewModel>(Window, vm =>
            {
                vm.Initialize(Se.Settings.Video.MoveVideoPositionCustom1Back);
            });
            if (result.OkPressed)
            {
                Se.Settings.Video.MoveVideoPositionCustom1Back = result.Milliseconds;

                var flatNodeBack = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.VideoMoveCustom1BackCommand);
                if (flatNodeBack != null)
                {
                    flatNodeBack.Title = string.Format(Se.Language.General.VideoCustom1BackX, Se.Settings.Video.MoveVideoPositionCustom1Back);
                }
            }
        }
        else if (node.ShortCut.Action == MainViewModel.VideoMoveCustom1ForwardCommand)
        {
            var result = await _windowService.ShowDialogAsync<PickMillisecondsWindow, PickMillisecondsViewModel>(Window, vm =>
            {
                vm.Initialize(Se.Settings.Video.MoveVideoPositionCustom1Forward);
            });
            if (result.OkPressed)
            {
                Se.Settings.Video.MoveVideoPositionCustom1Forward = result.Milliseconds;

                var flatNodeForward = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.VideoMoveCustom1ForwardCommand);
                if (flatNodeForward != null)
                {
                    flatNodeForward.Title = string.Format(Se.Language.General.VideoCustom1ForwardX, Se.Settings.Video.MoveVideoPositionCustom1Forward);
                }
            }
        }
        else if (node.ShortCut.Action == MainViewModel.VideoMoveCustom2BackCommand)
        {
            var result = await _windowService.ShowDialogAsync<PickMillisecondsWindow, PickMillisecondsViewModel>(Window, vm =>
            {
                vm.Initialize(Se.Settings.Video.MoveVideoPositionCustom2Back);
            });
            if (result.OkPressed)
            {
                Se.Settings.Video.MoveVideoPositionCustom2Back = result.Milliseconds;

                var flatNodeBack = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.VideoMoveCustom2BackCommand);
                if (flatNodeBack != null)
                {
                    flatNodeBack.Title = string.Format(Se.Language.General.VideoCustom2BackX, Se.Settings.Video.MoveVideoPositionCustom2Back);
                }
            }
        }
        else if (node.ShortCut.Action == MainViewModel.VideoMoveCustom2ForwardCommand)
        {
            var result = await _windowService.ShowDialogAsync<PickMillisecondsWindow, PickMillisecondsViewModel>(Window, vm =>
            {
                vm.Initialize(Se.Settings.Video.MoveVideoPositionCustom2Forward);
            });
            if (result.OkPressed)
            {
                Se.Settings.Video.MoveVideoPositionCustom2Forward = result.Milliseconds;

                var flatNodeForward = FlatNodes.FirstOrDefault(n => n?.ShortCut?.Action == MainViewModel.VideoMoveCustom2ForwardCommand);
                if (flatNodeForward != null)
                {
                    flatNodeForward.Title = string.Format(Se.Language.General.VideoCustom2ForwardX, Se.Settings.Video.MoveVideoPositionCustom2Forward);
                }
            }
        }
    }

    private List<string> FindDuplicateShortcuts()
    {
        var duplicates = new List<string>();
        var shortcutGroups = new Dictionary<string, List<ShortCut>>();

        foreach (var shortcut in _allShortcuts)
        {
            if (IsEmpty(shortcut))
            {
                continue;
            }

            var keysCombination = string.Join("+", shortcut.Keys.OrderBy(k => k));
            if (string.IsNullOrWhiteSpace(keysCombination))
            {
                continue;
            }

            if (!shortcutGroups.ContainsKey(keysCombination))
            {
                shortcutGroups[keysCombination] = new List<ShortCut>();
            }
            shortcutGroups[keysCombination].Add(shortcut);
        }

        foreach (var group in shortcutGroups.Where(g => g.Value.Count > 1))
        {
            var shortcuts = group.Value;
            var hasGeneral = shortcuts.Any(s => s.Category == ShortcutCategory.General);

            if (hasGeneral)
            {
                var names = shortcuts.Select(s => MakeDisplayName(s, false)).ToList();
                duplicates.Add($"{group.Key}: {string.Join(", ", names)} (\"General\" conflicts with all categories)");
            }
            else
            {
                var differentCategories = shortcuts.Select(s => s.Category).Distinct().Count() > 1;
                if (!differentCategories)
                {
                    var names = shortcuts.Select(s => MakeDisplayName(s, false)).ToList();
                    var category = shortcuts.First().Category.ToString();
                    duplicates.Add($"{group.Key}: {string.Join(", ", names)} (in \"{category}\")");
                }
            }
        }

        return duplicates;
    }

    private static bool IsEmpty(ShortCut shortcut)
    {
        var modifiers = new List<string>()
        {
            "Control",
            "Ctrl",
            "Alt",
            "Shift",
            "Win",
            Key.LeftCtrl.ToString(),
            Key.RightCtrl.ToString(),
            Key.LeftAlt.ToString(),
            Key.RightAlt.ToString(),
            Key.LeftShift.ToString(),
            Key.RightShift.ToString(),
            Key.LWin.ToString(),
            Key.RWin.ToString()
        };

        if (shortcut.Keys.Any(k => !modifiers.Contains(k)))
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task ShowGetKey()
    {
        var node = SelectedNode;
        if (node?.ShortCut == null || Window == null)
        {
            return;
        }

        var result =
            await _windowService
                .ShowDialogAsync<GetKeyWindow, GetKeyViewModel>(Window, vm =>
                {
                    vm.Initialize(string.Format(Se.Language.Options.Shortcuts.SetShortcutForX, MakeDisplayName(node.ShortCut, false)));
                });

        if (result.OkPressed && !string.IsNullOrEmpty(result.PressedKey))
        {
            SelectedShortcut = result.PressedKeyOnly;
            CtrlIsSelected = result.IsControlPressed;
            AltIsSelected = result.IsAltPressed;
            ShiftIsSelected = result.IsShiftPressed;
            WinIsSelected = result.IsWinPressed;
            UpdateShortcutDo();
        }
    }

    private void UpdateShortcutDo()
    {
        var shortcut = SelectedShortcut;
        var node = SelectedNode;
        if (node == null || node.ShortCut is null)
        {
            return;
        }

        var keys = new List<string>();

        if (ShiftIsSelected)
        {
            keys.Add("Shift");
        }

        if (CtrlIsSelected)
        {
            keys.Add("Ctrl");
        }

        if (AltIsSelected)
        {
            keys.Add("Alt");
        }

        if (WinIsSelected)
        {
            keys.Add("Win");
        }

        if (!string.IsNullOrEmpty(shortcut))
        {
            keys.Add(shortcut);
        }

        node.ShortCut.Keys = keys;
        node.DisplayShortcut = MakeDisplayShortCut(node.ShortCut);
    }

    [RelayCommand]
    private void ResetShortcut()
    {
        var shortcut = SelectedShortcut;
        var node = SelectedNode;
        if (string.IsNullOrEmpty(shortcut) || node?.ShortCut is null)
        {
            return;
        }

        node.ShortCut.Keys = new List<string>();
        node.Title = MakeDisplayName(node.ShortCut!);
        CtrlIsSelected = false;
        AltIsSelected = false;
        ShiftIsSelected = false;
        WinIsSelected = false;
        SelectedShortcut = null;
        UpdateShortcutDo();
    }

    [RelayCommand]
    private async Task ResetAllShortcuts()
    {
        if (MainViewModel == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
                  Window!,
                  Se.Language.Options.Shortcuts.ResetShortcuts,
                  Se.Language.Options.Shortcuts.ResetShortcutsDetail,
                  MessageBoxButtons.YesNoCancel,
                  MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        Se.Settings.Shortcuts.Clear();
        Se.Settings.InitializeMainShortcuts(MainViewModel);
        _allShortcuts = ShortcutsMain.GetAllShortcuts(MainViewModel);
        UpdateVisibleShortcuts(SearchText);
    }

    private bool Search(string searchText, ShortCut p)
    {
        var filterOk = SelectedFilter == Se.Language.General.All ||
                       SelectedFilter == Se.Language.Options.Shortcuts.Unassigned && p.Keys.Count == 0 ||
                       SelectedFilter == Se.Language.Options.Shortcuts.Assigned && p.Keys.Count > 0;
        if (!filterOk)
        {
            return false;
        }

        if (string.IsNullOrEmpty(searchText))
        {
            return true;
        }

        var title = MakeDisplayName(p);
        return title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    internal void ShortcutsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems == null || e.AddedItems.Count == 0 || e.AddedItems[0] is not ShortcutTreeNode node ||
            node.ShortCut == null)
        {
            IsControlsEnabled = false;
            IsConfigureVisible = false;
            return;
        }

        IsConfigureVisible = _configurableCommands.Contains(node.ShortCut.Action);

        // Set flag to prevent UpdateShortcut from running during selection load
        _isLoadingSelection = true;

        try
        {
            IsControlsEnabled = true;
            CtrlIsSelected = node.ShortCut.Keys.Contains("Ctrl") ||
                             node.ShortCut.Keys.Contains("Control") ||
                             node.ShortCut.Keys.Contains(Key.LeftCtrl.ToString()) ||
                             node.ShortCut.Keys.Contains(Key.RightCtrl.ToString());
            AltIsSelected = node.ShortCut.Keys.Contains("Alt") ||
                            node.ShortCut.Keys.Contains(Key.LeftAlt.ToString()) ||
                            node.ShortCut.Keys.Contains(Key.RightAlt.ToString());
            ShiftIsSelected = node.ShortCut.Keys.Contains("Shift") ||
                              node.ShortCut.Keys.Contains(Key.LeftShift.ToString()) ||
                              node.ShortCut.Keys.Contains(Key.RightShift.ToString());
            WinIsSelected = node.ShortCut.Keys.Contains("Win") ||
                              node.ShortCut.Keys.Contains(Key.LWin.ToString()) ||
                              node.ShortCut.Keys.Contains(Key.RWin.ToString());

            var modifiers = new List<string>()
            {
                "Control",
                "Ctrl",
                "Alt",
                "Shift",
                "Win",
                Key.LeftCtrl.ToString(),
                Key.RightCtrl.ToString(),
                Key.LeftAlt.ToString(),
                Key.RightAlt.ToString(),
                Key.LeftShift.ToString(),
                Key.RightShift.ToString(),
                Key.LWin.ToString(),
                Key.RWin.ToString()
            };

            foreach (var key in node.ShortCut.Keys)
            {
                if (modifiers.Contains(key))
                {
                    continue;
                }

                SelectedShortcut = key;
                return;
            }

            SelectedShortcut = null;
        }
        finally
        {
            // Always reset the flag, even if an exception occurs
            _isLoadingSelection = false;
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

    internal void ComboBoxFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateVisibleShortcuts(SearchText);
    }

    internal void ShortcutsDataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        _ = ShowGetKey();
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }
}
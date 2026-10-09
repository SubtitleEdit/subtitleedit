using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// Find and replace in the OCR window, using the main window's Find/Replace dialogs. Many users
/// proof-read in the OCR window itself, where the image sits next to the text, and had to leave
/// it (OK to the main window) just to search.
/// </summary>
public partial class OcrViewModel : IFindResult
{
    private readonly IFindService _findService = new FindService();
    private FindViewModel? _findViewModel;
    private ReplaceViewModel? _replaceViewModel;
    private bool _findClosingProgrammatically;

    /// <summary>The text box below the grid - find selects the match in it.</summary>
    internal TextBox? EditTextBox { get; set; }

    /// <summary>
    /// Ctrl+F (find), Ctrl+H (replace - Cmd+Alt+F on macOS, where Cmd+H hides the app), F3 and
    /// Shift+F3 (find next/previous). Returns true when the key was one of those.
    /// </summary>
    internal bool HandleFindReplaceKeys(KeyEventArgs e)
    {
        var commandModifier = OperatingSystem.IsMacOS()
            ? (e.KeyModifiers & KeyModifiers.Meta) != 0
            : (e.KeyModifiers & KeyModifiers.Control) != 0;
        var shift = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var alt = (e.KeyModifiers & KeyModifiers.Alt) != 0;

        if ((e.Key == Key.H && commandModifier) || (e.Key == Key.F && commandModifier && alt))
        {
            e.Handled = true;
            ShowReplace();
            return true;
        }

        if (e.Key == Key.F && commandModifier && !shift)
        {
            e.Handled = true;
            ShowFind();
            return true;
        }

        if (e.Key == Key.F3 && !commandModifier && !alt)
        {
            e.Handled = true;
            Dispatcher.UIThread.Post(async void () => await FindAgain(forward: !shift));
            return true;
        }

        return false;
    }

    [RelayCommand]
    internal void ShowFind()
    {
        if (OcrSubtitleItems.Count == 0 || Window == null)
        {
            return;
        }

        if (_replaceViewModel?.Window?.IsVisible == true)
        {
            _replaceViewModel.Window.Activate();
            _replaceViewModel.FocusSearchBox?.Invoke();
            return;
        }

        if (_findViewModel?.Window?.IsVisible == true)
        {
            _findViewModel.ResultFound = false;
            _findViewModel.Window.Activate();
            _findViewModel.FocusSearchBox?.Invoke();
            return;
        }

        var owner = Window;
        _windowService.ShowWindow<FindWindow, FindViewModel>(owner, (window, vm) =>
        {
            Nikse.SubtitleEdit.Logic.WindowService.KeepTopmostWhileOwnerActive(window, owner);
            _findViewModel = vm;
            vm.InitializeFindData(_findService, GetFindLines(), GetInitialSearchText(), this);
            window.KeyDown += async (_, e) =>
            {
                if (e.Handled)
                {
                    return;
                }

                if (e.Key == Key.F3)
                {
                    e.Handled = true;
                    if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
                    {
                        await vm.FindPreviousCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        await vm.FindNextCommand.ExecuteAsync(null);
                    }
                }
                else if (IsReplaceKey(e))
                {
                    e.Handled = true;
                    ShowReplace();
                }
            };
            window.Closed += (_, _) =>
            {
                if (_findClosingProgrammatically)
                {
                    _findClosingProgrammatically = false;
                    return;
                }

                FocusAfterDialog(vm.ResultFound);
            };
        });
    }

    [RelayCommand]
    internal void ShowReplace()
    {
        if (OcrSubtitleItems.Count == 0 || Window == null)
        {
            return;
        }

        // Replace takes over from an open find window, carrying its search over.
        var activeFindVm = _findViewModel?.Window?.IsVisible == true ? _findViewModel : null;
        if (_findViewModel != null)
        {
            _findClosingProgrammatically = activeFindVm != null;
            _findViewModel.Window?.Close();
            _findViewModel = null;
        }

        if (_replaceViewModel?.Window?.IsVisible == true)
        {
            _replaceViewModel.ResultFound = false;
            _replaceViewModel.Window.Activate();
            _replaceViewModel.FocusSearchBox?.Invoke();
            return;
        }

        var owner = Window;
        _windowService.ShowWindow<ReplaceWindow, ReplaceViewModel>(owner, (window, vm) =>
        {
            Nikse.SubtitleEdit.Logic.WindowService.KeepTopmostWhileOwnerActive(window, owner);
            _replaceViewModel = vm;
            vm.InitializeFindData(_findService, GetFindLines(), GetInitialSearchText(), this);
            if (activeFindVm != null && !string.IsNullOrEmpty(activeFindVm.SearchText))
            {
                vm.SearchText = activeFindVm.SearchText;
                vm.FindMode = activeFindVm.FindMode;
                vm.WholeWord = activeFindVm.WholeWord;
                vm.FocusReplaceOnOpen = true;
            }

            window.KeyDown += async (_, e) =>
            {
                if (!e.Handled && e.Key == Key.F3 && (e.KeyModifiers & KeyModifiers.Shift) == 0)
                {
                    e.Handled = true;
                    await vm.FindNextCommand.ExecuteAsync(null);
                }
            };
            window.Closed += (_, _) =>
            {
                _replaceViewModel = null;
                FocusAfterDialog(vm.ResultFound);
            };
        });
    }

    private static bool IsReplaceKey(KeyEventArgs e)
    {
        var commandModifier = OperatingSystem.IsMacOS()
            ? (e.KeyModifiers & KeyModifiers.Meta) != 0
            : (e.KeyModifiers & KeyModifiers.Control) != 0;
        return commandModifier && (e.Key == Key.H || (e.Key == Key.F && (e.KeyModifiers & KeyModifiers.Alt) != 0));
    }

    private void FocusAfterDialog(bool resultFound)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (resultFound && EditTextBox != null)
            {
                EditTextBox.Focus();
            }
            else
            {
                TableViewExtras.FocusRow(SubtitleGrid);
            }
        });
    }

    private string GetInitialSearchText()
    {
        var selectedText = EditTextBox?.SelectedText ?? string.Empty;
        if (!string.IsNullOrEmpty(selectedText) && selectedText.IndexOfAny(['\r', '\n']) < 0)
        {
            return selectedText.Trim();
        }

        return _findService.SearchText ?? string.Empty;
    }

    private List<string> GetFindLines()
    {
        return OcrSubtitleItems.Select(p => p.Text ?? string.Empty).ToList();
    }

    private int GetCurrentLineIndex()
    {
        var item = SelectedOcrSubtitleItem;
        var idx = item == null ? -1 : OcrSubtitleItems.IndexOf(item);
        return Math.Max(0, idx);
    }

    /// <summary>
    /// Caret position to continue from - only meaningful while the text box shows the current
    /// line, which it does unless the selection just moved and the binding has not caught up.
    /// </summary>
    private (int SelectionStart, int SelectionEnd) GetCaret()
    {
        var textBox = EditTextBox;
        var item = SelectedOcrSubtitleItem;
        if (textBox == null || item == null || textBox.Text != item.Text)
        {
            return (0, 0);
        }

        return (Math.Min(textBox.SelectionStart, textBox.SelectionEnd), Math.Max(textBox.SelectionStart, textBox.SelectionEnd));
    }

    public void RequestFindData()
    {
        var lines = GetFindLines();
        _findViewModel?.InitializeFindData(_findService, lines, _findService.SearchText, this);
        _replaceViewModel?.RefreshSubtitles(lines);
    }

    public async Task HandleFindResult(FindViewModel result)
    {
        result.ResultFound = false;
        if (OcrSubtitleItems.Count == 0 || string.IsNullOrEmpty(result.SearchText) ||
            !(result.FindNextPressed || result.FindPreviousPressed))
        {
            return;
        }

        var lines = GetFindLines();
        var currentLineIndex = GetCurrentLineIndex();
        var caret = GetCaret();
        _findService.Initialize(lines, currentLineIndex, result.WholeWord, result.FindMode);

        var idx = result.FindNextPressed
            ? _findService.FindNext(result.SearchText, lines, currentLineIndex, caret.SelectionEnd)
            : _findService.FindPrevious(result.SearchText, lines, currentLineIndex, caret.SelectionStart - 1);
        if (idx < 0)
        {
            idx = await WrapAround(result.FindNextPressed, result.SearchText, lines, result.Window);
            if (idx < 0)
            {
                ShowNotFound(result);
                return;
            }
        }

        result.ResultFound = true;
        ShowFindMatch(idx);
    }

    public async Task HandleReplaceResult(ReplaceViewModel result)
    {
        result.ResultFound = false;
        if (OcrSubtitleItems.Count == 0 || string.IsNullOrEmpty(result.SearchText) ||
            !(result.FindNextPressed || result.ReplacePressed || result.ReplaceAllPressed))
        {
            return;
        }

        if (IsOcrRunning && !result.FindNextPressed)
        {
            return; // the OCR run writes the same rows
        }

        var lines = GetFindLines();
        var currentLineIndex = GetCurrentLineIndex();
        var caret = GetCaret();

        // Initialize wipes the last match, which Replace needs to know what to replace.
        var savedFoundLine = _findService.CurrentLineNumber;
        var savedFoundIndex = _findService.CurrentTextIndex;
        var savedFoundText = _findService.CurrentTextFound;

        _findService.CurrentScope = FindScope.TextAndOriginal;
        _findService.Initialize(lines, currentLineIndex, result.WholeWord, result.FindMode);

        int idx;
        if (result.ReplaceAllPressed)
        {
            var replaceCount = _findService.ReplaceAll(result.SearchText, result.ReplaceText);
            for (var i = 0; i < OcrSubtitleItems.Count && i < lines.Count; i++)
            {
                SetItemText(i, lines[i]);
            }

            result.ReportReplaceAll(replaceCount);
            return;
        }

        if (result.FindNextPressed)
        {
            idx = _findService.FindNext(result.SearchText, lines, currentLineIndex, caret.SelectionEnd);
        }
        else
        {
            var nextStartLine = currentLineIndex;
            var nextStartIndex = caret.SelectionEnd;
            if (savedFoundLine >= 0 && savedFoundLine < lines.Count &&
                savedFoundIndex >= 0 && !string.IsNullOrEmpty(savedFoundText) &&
                savedFoundIndex + savedFoundText.Length <= lines[savedFoundLine].Length)
            {
                var replaced = MainViewModel.TryBuildReplacement(lines[savedFoundLine], savedFoundIndex, savedFoundText, result, out var newLine);
                if (replaced.HasValue)
                {
                    lines[savedFoundLine] = newLine;
                    SetItemText(savedFoundLine, newLine);
                    nextStartLine = savedFoundLine;
                    nextStartIndex = savedFoundIndex + replaced.Value;
                    result.ReportReplaced(1);
                }
            }

            idx = _findService.FindNext(result.SearchText, lines, nextStartLine, nextStartIndex);
        }

        if (idx < 0)
        {
            idx = await WrapAround(forward: true, result.SearchText, lines, result.Window);
            if (idx < 0)
            {
                if (result.ReplacedCount == 0)
                {
                    result.ResultIcon = IconNames.Information;
                    result.CountResult = string.Format(Se.Language.General.XNotFound, result.SearchText);
                }

                return;
            }
        }

        result.ResultFound = true;
        ShowFindMatch(idx);
    }

    /// <summary>F3 / Shift+F3 in the OCR window: repeat the last search without a dialog.</summary>
    private async Task FindAgain(bool forward)
    {
        if (OcrSubtitleItems.Count == 0)
        {
            return;
        }

        var searchText = _findService.SearchText;
        if (string.IsNullOrEmpty(searchText))
        {
            ShowFind();
            return;
        }

        var lines = GetFindLines();
        var currentLineIndex = GetCurrentLineIndex();
        var caret = GetCaret();
        var idx = forward
            ? _findService.FindNext(searchText, lines, currentLineIndex, caret.SelectionEnd)
            : _findService.FindPrevious(searchText, lines, currentLineIndex, caret.SelectionStart - 1);
        if (idx < 0)
        {
            idx = await WrapAround(forward, searchText, lines, null);
            if (idx < 0)
            {
                return;
            }
        }

        ShowFindMatch(idx);
        Dispatcher.UIThread.Post(() => EditTextBox?.Focus());
    }

    private async Task<int> WrapAround(bool forward, string searchText, List<string> lines, Window? dialogWindow)
    {
        var parent = dialogWindow?.IsVisible == true ? dialogWindow : Window;
        if (parent == null)
        {
            return -1;
        }

        var message = forward
            ? Se.Language.General.SearchItemNotFoundContinueFromTop
            : Se.Language.General.SearchItemNotFoundContinueFromBottom;
        var answer = await MessageBox.Show(parent, Se.Language.General.ContinueFindTitle, message, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return -1;
        }

        return forward
            ? _findService.FindNext(searchText, lines, 0, 0)
            : _findService.FindPrevious(searchText, lines, lines.Count, 0);
    }

    private static void ShowNotFound(FindViewModel result)
    {
        result.ResultIcon = IconNames.Information;
        result.CountResult = string.Format(Se.Language.General.XNotFound, result.SearchText);
    }

    /// <summary>
    /// Sets a row's text the way typing in the text box does: the grid draws the fix engine's
    /// formatted text while a row has one, so a stale fix result would hide the replacement.
    /// </summary>
    private void SetItemText(int index, string text)
    {
        var item = OcrSubtitleItems[index];
        if (item.Text == text)
        {
            return;
        }

        item.Text = text;
        if (item.FixResult != null && item.FixResult.GetText() != text)
        {
            item.FixResult = new OcrFixLineResult(index, text);
        }
    }

    private void ShowFindMatch(int idx)
    {
        var foundIndex = _findService.CurrentTextIndex;
        var foundLength = _findService.CurrentTextFound?.Length ?? 0;
        Dispatcher.UIThread.Post(() =>
        {
            if (idx < 0 || idx >= OcrSubtitleItems.Count)
            {
                return;
            }

            var item = OcrSubtitleItems[idx];
            SelectedOcrSubtitleItem = item;
            SubtitleGrid.SelectedItem = item;
            SubtitleGrid.ScrollIntoView(idx);

            var textBox = EditTextBox;
            if (textBox == null)
            {
                return;
            }

            // The text-box binding may not have caught up with the new row yet.
            if (textBox.Text != item.Text)
            {
                textBox.Text = item.Text;
            }

            var length = textBox.Text?.Length ?? 0;
            var start = Math.Clamp(foundIndex, 0, length);
            textBox.SelectionStart = start;
            textBox.SelectionEnd = Math.Clamp(start + foundLength, start, length);
        });
    }
}

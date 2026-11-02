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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Find and replace in the translation column, using the main window's Find/Replace dialogs (#16556),
/// so a recurring slip of the engine is fixed here instead of after closing the window. The source
/// column is left out: it cannot be edited here.
/// </summary>
public partial class AutoTranslateViewModel : IFindResult
{
    private readonly IFindService _findService = new FindService();
    private FindViewModel? _findViewModel;
    private ReplaceViewModel? _replaceViewModel;

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

        if (IsReplaceKey(e))
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

    private static bool IsReplaceKey(KeyEventArgs e)
    {
        var commandModifier = OperatingSystem.IsMacOS()
            ? (e.KeyModifiers & KeyModifiers.Meta) != 0
            : (e.KeyModifiers & KeyModifiers.Control) != 0;
        return commandModifier && (e.Key == Key.H || (e.Key == Key.F && (e.KeyModifiers & KeyModifiers.Alt) != 0));
    }

    [RelayCommand]
    internal void ShowFind()
    {
        if (Rows.Count == 0 || Window == null)
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

        CommitOpenCellEdit();

        var owner = Window;
        _windowService.ShowWindow<FindWindow, FindViewModel>(owner, (window, vm) =>
        {
            WindowService.KeepTopmostWhileOwnerActive(window, owner);
            _findViewModel = vm;
            vm.InitializeFindData(_findService, GetFindLines(), _findService.SearchText ?? string.Empty, this);
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
                // Null already when Replace took over - the focus then belongs to its dialog.
                if (ReferenceEquals(_findViewModel, vm))
                {
                    _findViewModel = null;
                    FocusRowAfterDialog();
                }
            };
        });
    }

    [RelayCommand]
    internal void ShowReplace()
    {
        if (Rows.Count == 0 || Window == null)
        {
            return;
        }

        // Replace takes over from an open find window, carrying its search over.
        var activeFindVm = _findViewModel;
        _findViewModel = null;
        activeFindVm?.Window?.Close();

        if (_replaceViewModel?.Window?.IsVisible == true)
        {
            _replaceViewModel.ResultFound = false;
            _replaceViewModel.Window.Activate();
            _replaceViewModel.FocusSearchBox?.Invoke();
            return;
        }

        CommitOpenCellEdit();

        var owner = Window;
        _windowService.ShowWindow<ReplaceWindow, ReplaceViewModel>(owner, (window, vm) =>
        {
            WindowService.KeepTopmostWhileOwnerActive(window, owner);
            _replaceViewModel = vm;
            vm.InitializeFindData(_findService, GetFindLines(), _findService.SearchText ?? string.Empty, this);
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
                FocusRowAfterDialog();
            };
        });
    }

    /// <summary>
    /// An open cell editor commits when it loses focus. Do that before searching: the dialog would
    /// otherwise see the old text, and the editor could later commit over a replacement.
    /// </summary>
    private void CommitOpenCellEdit()
    {
        if (RowGrid != null)
        {
            TableViewExtras.FocusRow(RowGrid);
        }
    }

    private void FocusRowAfterDialog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (RowGrid != null)
            {
                TableViewExtras.FocusRow(RowGrid);
            }
        });
    }

    private List<string> GetFindLines()
    {
        return Rows.Select(p => p.TranslatedText ?? string.Empty).ToList();
    }

    private int GetCurrentLineIndex()
    {
        var row = SelectedTranslateRow;
        var index = row == null ? -1 : Rows.IndexOf(row);
        return Math.Max(0, index);
    }

    /// <summary>
    /// The last match, if the selected row still holds it. Without a text box there is no caret to
    /// continue from, so the find service's last match is the search position - selecting another
    /// row or editing this one starts over, like moving the caret would.
    /// </summary>
    private bool TryGetCurrentMatch(List<string> lines, int lineIndex, out int matchIndex, out string matchText)
    {
        matchIndex = _findService.CurrentTextIndex;
        matchText = _findService.CurrentTextFound ?? string.Empty;
        return _findService.CurrentLineNumber == lineIndex &&
               lineIndex < lines.Count &&
               matchIndex >= 0 &&
               matchText.Length > 0 &&
               matchIndex + matchText.Length <= lines[lineIndex].Length;
    }

    private int FindFromCurrentMatch(bool forward, string searchText, List<string> lines, int lineIndex, bool hasMatch, int matchIndex, string matchText)
    {
        if (forward)
        {
            return _findService.FindNext(searchText, lines, lineIndex, hasMatch ? matchIndex + matchText.Length : 0);
        }

        var line = lineIndex < lines.Count ? lines[lineIndex] : string.Empty;
        return _findService.FindPrevious(searchText, lines, lineIndex, hasMatch ? matchIndex - 1 : line.Length - 1);
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
        if (Rows.Count == 0 || string.IsNullOrEmpty(result.SearchText) ||
            !(result.FindNextPressed || result.FindPreviousPressed))
        {
            return;
        }

        var lines = GetFindLines();
        var currentLineIndex = GetCurrentLineIndex();
        var hasMatch = TryGetCurrentMatch(lines, currentLineIndex, out var matchIndex, out var matchText);
        _findService.Initialize(lines, currentLineIndex, result.WholeWord, result.FindMode);

        var idx = FindFromCurrentMatch(result.FindNextPressed, result.SearchText, lines, currentLineIndex, hasMatch, matchIndex, matchText);
        if (idx < 0)
        {
            idx = await WrapAround(result.FindNextPressed, result.SearchText, lines, result.Window);
            if (idx < 0)
            {
                result.ResultIcon = IconNames.Information;
                result.CountResult = string.Format(Se.Language.General.XNotFound, result.SearchText);
                return;
            }
        }

        result.ResultFound = true;
        ShowFindMatch(idx);
    }

    public async Task HandleReplaceResult(ReplaceViewModel result)
    {
        result.ResultFound = false;
        if (Rows.Count == 0 || string.IsNullOrEmpty(result.SearchText) ||
            !(result.FindNextPressed || result.ReplacePressed || result.ReplaceAllPressed))
        {
            return;
        }

        if (!IsTranslateEnabled && !result.FindNextPressed)
        {
            return; // the translation run writes the same rows
        }

        if (result.ReplaceAllPressed)
        {
            await ReplaceAll(result);
            return;
        }

        var lines = GetFindLines();
        var currentLineIndex = GetCurrentLineIndex();

        // Initialize wipes the last match, which Replace needs to know what to replace.
        var hasMatch = TryGetCurrentMatch(lines, currentLineIndex, out var matchIndex, out var matchText);
        _findService.Initialize(lines, currentLineIndex, result.WholeWord, result.FindMode);

        var nextStartIndex = hasMatch ? matchIndex + matchText.Length : 0;
        if (result.ReplacePressed && hasMatch)
        {
            var replaced = MainViewModel.TryBuildReplacement(lines[currentLineIndex], matchIndex, matchText, result, out var newLine);
            if (replaced.HasValue)
            {
                lines[currentLineIndex] = newLine;
                nextStartIndex = matchIndex + replaced.Value;
                SetTranslations(lines);
                result.ReportReplaced(1);
            }
        }

        var idx = _findService.FindNext(result.SearchText, lines, currentLineIndex, nextStartIndex);
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

    /// <summary>
    /// Same as the main window (#16453): current line to the end first, then ask before going on
    /// from the top.
    /// </summary>
    private async Task ReplaceAll(ReplaceViewModel result)
    {
        var startLine = GetCurrentLineIndex();
        var replaceCount = ReplaceAllInRange(result, startLine, int.MaxValue);

        var parent = result.Window?.IsVisible == true ? result.Window : Window;
        if (startLine > 0 && parent != null)
        {
            var message = replaceCount > 0
                ? string.Format(Se.Language.General.ReplacedXContinueFromTop, replaceCount)
                : Se.Language.General.ReplaceNotFoundContinueFromTop;
            var answer = await MessageBox.Show(parent, Se.Language.General.ContinueReplaceTitle, message, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // The main window stays usable behind the question, so a translation may have started.
            if (answer == MessageBoxResult.Yes && IsTranslateEnabled)
            {
                replaceCount += ReplaceAllInRange(result, 0, startLine);
            }
        }

        result.ReportReplaceAll(replaceCount);
    }

    private int ReplaceAllInRange(ReplaceViewModel result, int startLine, int stopAtLine)
    {
        var lines = GetFindLines();
        _findService.Initialize(lines, startLine, result.WholeWord, result.FindMode);
        var replaceCount = _findService.ReplaceAll(result.SearchText, result.ReplaceText, startLine, stopAtLine);
        SetTranslations(lines);
        return replaceCount;
    }

    /// <summary>
    /// Writes back the rows that changed. A replacement can clear the last translation, which
    /// disables OK again - see <see cref="OnTranslationEdited"/>.
    /// </summary>
    private void SetTranslations(List<string> lines)
    {
        for (var i = 0; i < Rows.Count && i < lines.Count; i++)
        {
            if ((Rows[i].TranslatedText ?? string.Empty) != lines[i])
            {
                Rows[i].TranslatedText = lines[i];
            }
        }

        HasTranslatedSomething = HasUsableTranslation;
    }

    /// <summary>F3 / Shift+F3 in the window: repeat the last search without a dialog.</summary>
    private async Task FindAgain(bool forward)
    {
        if (Rows.Count == 0)
        {
            return;
        }

        var searchText = _findService.SearchText;
        if (string.IsNullOrEmpty(searchText))
        {
            ShowFind();
            return;
        }

        CommitOpenCellEdit();

        var lines = GetFindLines();
        var currentLineIndex = GetCurrentLineIndex();
        var hasMatch = TryGetCurrentMatch(lines, currentLineIndex, out var matchIndex, out var matchText);
        var idx = FindFromCurrentMatch(forward, searchText, lines, currentLineIndex, hasMatch, matchIndex, matchText);
        if (idx < 0)
        {
            idx = await WrapAround(forward, searchText, lines, null);
            if (idx < 0)
            {
                return;
            }
        }

        ShowFindMatch(idx);
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

    private void ShowFindMatch(int idx)
    {
        if (idx < 0 || idx >= Rows.Count)
        {
            return;
        }

        var row = Rows[idx];
        SelectedTranslateRow = row;
        if (RowGrid != null)
        {
            RowGrid.SelectedItem = row;
            RowGrid.ScrollIntoView(row);
        }
    }
}

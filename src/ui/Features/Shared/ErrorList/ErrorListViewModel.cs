using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

public partial class ErrorListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ErrorListItem> _subtitles;
    [ObservableProperty] private ErrorListItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasErrors;
    [ObservableProperty] private bool _canExport;
    [ObservableProperty] private string _summary = string.Empty;
    [ObservableProperty] private string _exportStatus = string.Empty;

    public ObservableCollection<SummaryCard> Cards { get; } = new();

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    private readonly List<ErrorListItem> _allItems = new();
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _subtitleFileName = string.Empty;

    public ErrorListViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles = new ObservableCollection<ErrorListItem>();
    }

    /// <summary>
    /// Puts what the window shows - the active card filter included - on the clipboard, tab
    /// separated, so it pastes as columns into Excel/Sheets and as a readable block anywhere else.
    /// </summary>
    [RelayCommand]
    private async Task CopyToClipboard()
    {
        if (Window == null || Subtitles.Count == 0)
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, ErrorListExport.ToTabSeparated(Subtitles));

        // Not awaited: the command would stay "running" - and its menu item disabled - for as
        // long as the status is shown.
        _ = ShowExportStatus(Se.Language.General.CopiedToClipboard);
    }

    [RelayCommand]
    private Task ExportText()
    {
        return ExportToFile(".txt", () => Encoding.UTF8.GetBytes(ErrorListExport.ToPlainText(Subtitles, Summary, _subtitleFileName)));
    }

    [RelayCommand]
    private Task ExportExcel()
    {
        return ExportToFile(".xlsx", () => ErrorListExport.ToXlsx(Subtitles));
    }

    [RelayCommand]
    private Task ExportHtml()
    {
        return ExportToFile(".html", () => Encoding.UTF8.GetBytes(ErrorListExport.ToHtml(Subtitles, Summary, _subtitleFileName)));
    }

    private async Task ExportToFile(string extension, Func<byte[]> makeContent)
    {
        if (Window == null || Subtitles.Count == 0)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, extension, GetSuggestedFileName() + extension, Se.Language.General.Export);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await File.WriteAllBytesAsync(fileName, makeContent());

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.FileSaved, string.Format(Se.Language.General.FileSavedToX, fileName), fileName, true, true);
        });
    }

    private string GetSuggestedFileName()
    {
        var name = string.IsNullOrEmpty(_subtitleFileName)
            ? "subtitle"
            : Path.GetFileNameWithoutExtension(_subtitleFileName);

        return string.IsNullOrWhiteSpace(name) ? "subtitle-errors" : name + "-errors";
    }

    /// <summary>Clipboard copies are silent otherwise - the status line says it happened, then fades.</summary>
    private async Task ShowExportStatus(string text)
    {
        ExportStatus = text;
        await Task.Delay(3000);
        if (ExportStatus == text)
        {
            ExportStatus = string.Empty;
        }
    }

    [RelayCommand]
    private void GoTo()
    {
        if (SelectedSubtitle == null)
        {
            return;
        }

        GoToPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SetFilter(SummaryCard? card)
    {
        if (card == null)
        {
            return;
        }

        ApplyFilter(card);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    /// <summary>
    /// The items come ready-made from the caller: it is the only place that still knows each
    /// line's real neighbours, which the gap/overlap wording needs.
    /// </summary>
    internal void Initialize(List<ErrorListItem> errorListItems, int totalLines, string? subtitleFileName = null)
    {
        var l = Se.Language.ErrorList;
        _subtitleFileName = subtitleFileName ?? string.Empty;
        _allItems.Clear();
        _allItems.AddRange(errorListItems);

        var lineCount = errorListItems.Select(p => p.Number).Distinct().Count();
        Summary = errorListItems.Count == 0
            ? l.NoErrors
            : string.Format(l.SummaryX, errorListItems.Count, lineCount, totalLines);

        Cards.Clear();
        Cards.Add(new SummaryCard { Key = null, Label = l.All, Count = errorListItems.Count, Brush = LineError.AllBrush, IsActive = true });
        foreach (var type in Enum.GetValues<LineErrorType>())
        {
            var count = errorListItems.Count(p => p.Type == type);
            Cards.Add(new SummaryCard { Key = type, Label = LineError.GetLabel(type), Hint = LineError.GetHint(type), Count = count, Brush = LineError.GetBrush(type) });
        }

        ApplyFilter(Cards[0]);
    }

    private void ApplyFilter(SummaryCard card)
    {
        foreach (var c in Cards)
        {
            c.IsActive = ReferenceEquals(c, card);
        }

        var filtered = card.Key is LineErrorType type
            ? _allItems.Where(p => p.Type == type)
            : _allItems;

        Subtitles = new ObservableCollection<ErrorListItem>(filtered);
        SelectedSubtitle = Subtitles.FirstOrDefault();
        CanExport = Subtitles.Count > 0;
    }

    /// <summary>
    /// Follows the selection itself instead of the grid's SelectionChanged event: the row the
    /// grid selects on its own (AlwaysSelected) never raises that event, which left Go to
    /// disabled while row 0 looked selected.
    /// </summary>
    partial void OnSelectedSubtitleChanged(ErrorListItem? value)
    {
        HasErrors = value != null;
    }

    internal void OnGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(GoTo);
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            GoTo();
        }
    }
}

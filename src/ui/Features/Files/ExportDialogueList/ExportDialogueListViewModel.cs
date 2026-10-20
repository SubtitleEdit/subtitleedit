using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Files.ExportDialogueList;

/// <summary>
/// File > Export > Excel dialogue list: pick and order the columns, how time codes and line
/// breaks are written, preview the first rows, save as .xlsx. The choices are remembered.
/// </summary>
public partial class ExportDialogueListViewModel : ObservableObject
{
    public const int PreviewRowCount = 12;

    [ObservableProperty] private ObservableCollection<DialogueListColumnItem> _columns = new();
    [ObservableProperty] private DialogueListColumnItem? _selectedColumn;
    [ObservableProperty] private ObservableCollection<string> _timeCodeFormats;
    [ObservableProperty] private int _selectedTimeCodeFormatIndex;
    [ObservableProperty] private bool _lineBreaksInCell;
    [ObservableProperty] private bool _includeHeader;
    [ObservableProperty] private bool _removeFormatting;

    public Window? Window { get; set; }

    /// <summary>Raised when anything the preview shows changed.</summary>
    public event Action? PreviewChanged;

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private List<DialogueListRow> _rows = new();
    private string? _subtitleFileName;
    private double _frameRate = 25;

    public ExportDialogueListViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        _timeCodeFormats = new ObservableCollection<string>
        {
            Se.Language.File.Export.DialogueListFramesFormat,
            "hh:mm:ss,zzz",
            Se.Language.General.Seconds,
        };
    }

    public void Initialize(List<DialogueListRow> rows, string? subtitleFileName, double frameRate)
    {
        _rows = rows;
        _subtitleFileName = subtitleFileName;
        _frameRate = frameRate;

        var settings = Se.Settings.File.ExportDialogueList;
        SelectedTimeCodeFormatIndex = Enum.TryParse<DialogueListTimeCodeFormat>(settings.TimeCodeFormat, out var format) ? (int)format : 0;
        LineBreaksInCell = settings.LineBreaksInCell;
        IncludeHeader = settings.IncludeHeader;
        RemoveFormatting = settings.RemoveFormatting;

        foreach (var column in GetColumnOrder(settings.ColumnOrder))
        {
            var isAvailable = column != DialogueListColumn.OriginalText || DialogueListExporter.HasData(column, rows);
            var hasData = DialogueListExporter.HasData(column, rows);
            var isChecked = settings.SelectedColumns == null
                ? IsDefaultColumn(column) && hasData
                : settings.SelectedColumns.Contains(column.ToString()) && hasData;
            var item = new DialogueListColumnItem(column, GetDisplayName(column), isAvailable, hasData, isChecked);
            item.PropertyChanged += ColumnItemPropertyChanged;
            Columns.Add(item);
        }

        SelectedColumn = Columns.FirstOrDefault();
    }

    private static bool IsDefaultColumn(DialogueListColumn column) =>
        column is DialogueListColumn.Number or DialogueListColumn.Start or DialogueListColumn.End or
            DialogueListColumn.Text or DialogueListColumn.OriginalText or DialogueListColumn.Actor;

    private static List<DialogueListColumn> GetColumnOrder(List<string>? savedOrder)
    {
        var order = new List<DialogueListColumn>();
        foreach (var name in savedOrder ?? new List<string>())
        {
            if (Enum.TryParse<DialogueListColumn>(name, out var column) && !order.Contains(column))
            {
                order.Add(column);
            }
        }

        order.AddRange(Enum.GetValues<DialogueListColumn>().Where(c => !order.Contains(c)));
        return order;
    }

    public static string GetDisplayName(DialogueListColumn column) => column switch
    {
        DialogueListColumn.Number => Se.Language.General.Number,
        DialogueListColumn.Start => Se.Language.General.Start,
        DialogueListColumn.End => Se.Language.File.Export.DialogueListEnd,
        DialogueListColumn.Duration => Se.Language.General.Duration,
        DialogueListColumn.Gap => Se.Language.General.Gap,
        DialogueListColumn.Text => Se.Language.General.Text,
        DialogueListColumn.OriginalText => Se.Language.General.OriginalText,
        DialogueListColumn.Actor => Se.Language.General.Actor,
        DialogueListColumn.Style => Se.Language.General.Style,
        DialogueListColumn.Layer => Se.Language.General.Layer,
        DialogueListColumn.Forced => Se.Language.General.Forced,
        DialogueListColumn.Cps => Se.Language.General.Cps,
        DialogueListColumn.Wpm => Se.Language.General.Wpm,
        DialogueListColumn.Characters => Se.Language.General.Characters,
        _ => column.ToString(),
    };

    private void ColumnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DialogueListColumnItem.IsChecked))
        {
            PreviewChanged?.Invoke();
        }
    }

    partial void OnSelectedTimeCodeFormatIndexChanged(int value) => PreviewChanged?.Invoke();
    partial void OnLineBreaksInCellChanged(bool value) => PreviewChanged?.Invoke();
    partial void OnIncludeHeaderChanged(bool value) => PreviewChanged?.Invoke();
    partial void OnRemoveFormattingChanged(bool value) => PreviewChanged?.Invoke();

    public DialogueListOptions GetOptions()
    {
        return new DialogueListOptions
        {
            Columns = Columns.Where(c => c.IsChecked && c.IsAvailable).Select(c => c.Column).ToList(),
            TimeCodeFormat = (DialogueListTimeCodeFormat)Math.Clamp(SelectedTimeCodeFormatIndex, 0, 2),
            LineBreaksInCell = LineBreaksInCell,
            IncludeHeader = IncludeHeader,
            RemoveFormatting = RemoveFormatting,
        };
    }

    /// <summary>The header names and the cells of the first rows, for the preview grid.</summary>
    public (List<string> Headers, List<IReadOnlyList<object?>> Rows) GetPreview()
    {
        var options = GetOptions();
        var headers = options.Columns.Select(DialogueListExporter.GetHeader).ToList();
        var rows = DialogueListExporter.MakeCells(_rows.Take(PreviewRowCount + 1).ToList(), options, _frameRate);
        return (headers, rows.Take(PreviewRowCount).ToList());
    }

    public byte[] GetXlsxBytes() => DialogueListExporter.CreateXlsx(_rows, GetOptions(), _frameRate);

    private void SaveSettings()
    {
        var settings = Se.Settings.File.ExportDialogueList;
        var previous = settings.SelectedColumns ?? new List<string>();

        // A column left unticked only because this file has no data for it keeps its old choice.
        settings.SelectedColumns = Columns
            .Where(c => c.IsChecked || ((!c.HasData || !c.IsAvailable) && previous.Contains(c.Column.ToString())))
            .Select(c => c.Column.ToString())
            .ToList();
        settings.ColumnOrder = Columns.Select(c => c.Column.ToString()).ToList();
        settings.TimeCodeFormat = ((DialogueListTimeCodeFormat)Math.Clamp(SelectedTimeCodeFormatIndex, 0, 2)).ToString();
        settings.LineBreaksInCell = LineBreaksInCell;
        settings.IncludeHeader = IncludeHeader;
        settings.RemoveFormatting = RemoveFormatting;
        Se.SaveSettings();
    }

    [RelayCommand]
    private void MoveUp()
    {
        var selected = SelectedColumn;
        var index = selected == null ? -1 : Columns.IndexOf(selected);
        if (index <= 0)
        {
            return;
        }

        Columns.Move(index, index - 1);
        SelectedColumn = selected;
        PreviewChanged?.Invoke();
    }

    [RelayCommand]
    private void MoveDown()
    {
        var selected = SelectedColumn;
        var index = selected == null ? -1 : Columns.IndexOf(selected);
        if (index < 0 || index >= Columns.Count - 1)
        {
            return;
        }

        Columns.Move(index, index + 1);
        SelectedColumn = selected;
        PreviewChanged?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAs()
    {
        if (Window == null)
        {
            return;
        }

        if (GetOptions().Columns.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.File.Export.DialogueListNoColumns,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var suggestedName = string.IsNullOrEmpty(_subtitleFileName)
            ? string.Empty
            : Utilities.GetPathAndFileNameWithoutExtension(_subtitleFileName);
        var fileName = await _fileHelper.PickSaveFile(Window, ".xlsx", suggestedName, Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        try
        {
            await File.WriteAllBytesAsync(fileName, GetXlsxBytes());
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Export of Excel dialogue list failed for " + fileName);
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.CouldNotSaveFileXErrorY, fileName, ex.Message),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(
                Se.Language.General.FileSaved,
                string.Format(Se.Language.General.FileSavedToX, fileName),
                fileName,
                true,
                true);
        });
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
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;

public partial class BatchErrorListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BatchErrorListItem> _subtitles;
    [ObservableProperty] private BatchErrorListItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasErrors;

    public Window? Window { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    public BatchErrorListViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles = new ObservableCollection<BatchErrorListItem>();
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var suggestedFileName = "Subtitle-file-errors";
        var fileName = await _fileHelper.PickSaveFile(Window, ".csv", suggestedFileName, Se.Language.General.Export);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("FileName,LineNumber,Text,Error");
        foreach (var errorItem in Subtitles)
        {
            sb.AppendLine($"{CsvTextEncode(errorItem.FileName)},{errorItem.Number},{CsvTextEncode(errorItem.Text)},{CsvTextEncode(errorItem.Error)}");
        }

        await File.WriteAllTextAsync(fileName, sb.ToString(), Encoding.UTF8);

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
        vm =>
        {
            vm.Initialize(Se.Language.General.FileSaved,
                string.Format(Se.Language.Tools.BatchConvert.ErrorsExportedX, Subtitles.Count), fileName, true, true);
        });
    }

    private static string CsvTextEncode(string s)
    {
        s = s.Replace("\"", "\"\"");
        s = s.Replace("\r", "\\r");
        s = s.Replace("\n", "\\n");
        return $"\"{s}\"";
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
    }

    internal void Initialize(List<BatchConvertItem> batchItems)
    {
        foreach (var batchItem in batchItems)
        {
            if (batchItem.Subtitle == null)
            {
                continue;
            }

            for (int i = 0; i < batchItem.Subtitle.Paragraphs.Count; i++)
            {
                Core.Common.Paragraph? p = batchItem.Subtitle.Paragraphs[i];
                var prev = i > 0 ? batchItem.Subtitle.Paragraphs[i - 1] : null;
                var next = i < batchItem.Subtitle.Paragraphs.Count - 1 ? batchItem.Subtitle.Paragraphs[i + 1] : null;
                var format = batchItem.Subtitle.OriginalFormat ?? new SubRip();
                var errorItem = new BatchErrorListItem(batchItem.FileName, new SubtitleLineViewModel(p, format), prev, next);
                if (!string.IsNullOrEmpty(errorItem.Error))
                {
                    Subtitles.Add(errorItem);
                }
            }
        }

        HasErrors = Subtitles.Count > 0;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasErrors = SelectedSubtitle != null;
    }
}
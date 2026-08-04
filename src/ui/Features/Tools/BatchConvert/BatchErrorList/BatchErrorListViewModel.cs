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
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
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

            // One view model per paragraph, reused as its neighbours' prev/next: the previous
            // version built three of them (plus a format) for every paragraph of every file and
            // threw away all but the ones with an error.
            var format = batchItem.Subtitle.OriginalFormat ?? new SubRip();
            var lines = new List<SubtitleLineViewModel>(batchItem.Subtitle.Paragraphs.Count);
            foreach (var p in batchItem.Subtitle.Paragraphs)
            {
                lines.Add(new SubtitleLineViewModel(p, format));
            }

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var prev = i > 0 ? lines[i - 1] : null;
                var next = i < lines.Count - 1 ? lines[i + 1] : null;
                if (line.HasErrors(prev, next))
                {
                    Subtitles.Add(new BatchErrorListItem(batchItem.FileName, line, prev, next));
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
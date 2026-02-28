using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class CopyPasteTranslateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private int? _maxBlockSize;
    [ObservableProperty] private string _lineSeparator;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; internal set; }

    private readonly IWindowService _windowService;

    public CopyPasteTranslateViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        SubtitleGrid = new DataGrid();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        MaxBlockSize = Se.Settings.AutoTranslate.CopyPasteMaxBlockSize;
        LineSeparator = Se.Settings.AutoTranslate.CopyPasteLineSeparator;
    }

    private void SaveSettings()
    {
        if (MaxBlockSize.HasValue)
        {
            Se.Settings.AutoTranslate.CopyPasteMaxBlockSize = MaxBlockSize.Value;
        }

        Se.Settings.AutoTranslate.CopyPasteLineSeparator = LineSeparator ?? ".";

        Se.SaveSettings();
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        Subtitles.Clear();

        foreach (var s in subtitles)
        {
            var s2 = new SubtitleLineViewModel(s);
            s2.OriginalText = s.Text;
            s2.Text = s.OriginalText;
            Subtitles.Add(s2);
        }

        if (Subtitles.Count > 0)
        {
            SelectedSubtitle = Subtitles[0];
        }
    }

    [RelayCommand]
    private async Task Translate()
    {
        if (Window == null)
        {
            return;
        }

        var log = new StringBuilder();
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        var startIndex = selectedItems.Count <= 0 ? 0 : Subtitles.IndexOf(selectedItems[0]);
        var start = startIndex;
        var index = startIndex;

        List<Core.Common.Paragraph> paragraphs = new List<Core.Common.Paragraph>();
        for (var i = startIndex; i < Subtitles.Count; i++)
        {
            var item = Subtitles[i];
            var p = new Core.Common.Paragraph
            {
                Text = item.OriginalText,
                StartTime = new Core.Common.TimeCode(item.StartTime),
                EndTime = new Core.Common.TimeCode(item.EndTime),
            };
            paragraphs.Add(p);
        }


        var translator = new CopyPasteTranslator(paragraphs, LineSeparator);

        var blocks = translator.BuildBlocks(MaxBlockSize ?? 5000, string.Empty, startIndex);
        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var result = await _windowService.ShowDialogAsync<CopyPasteTranslateBlockWindow, CopyPasteTranslateBlockViewModel>(Window!, vm =>
            {
                vm.Initialize(i + 1, blocks.Count, block.TargetText);
            });

            if (!result.OkPressed)
            {
                return;
            }

            var translatedLines = translator.GetTranslationResult(string.Empty, result.TranslatedText, block);
            FillTranslatedText(translatedLines, start, index);
            index += block.Paragraphs.Count;
            start = index;
            SelectAndScrollToRow(index);
        }
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = index;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
        });
    }


    private void FillTranslatedText(List<string> translatedLines, int start, int end)
    {
        var index = start;
        foreach (string s in translatedLines)
        {
            if (index < Subtitles.Count)
            {
                var item = Subtitles[index];
                item.Text = s;
            }
            index++;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
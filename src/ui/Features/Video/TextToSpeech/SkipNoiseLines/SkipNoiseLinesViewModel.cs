using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

public partial class SkipNoiseLinesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SkipNoiseLineRow> _rows;
    [ObservableProperty] private SkipNoiseLineRow? _selectedRow;
    [ObservableProperty] private string _rowsInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The lines the user confirmed should stay silent - no speech is generated for them.</summary>
    public List<Paragraph> SelectedParagraphs { get; private set; }

    public SkipNoiseLinesViewModel()
    {
        Rows = new ObservableCollection<SkipNoiseLineRow>();
        SelectedParagraphs = new List<Paragraph>();
        RowsInfo = string.Empty;
    }

    public void Initialize(List<Paragraph> noiseLines)
    {
        Rows.Clear();
        foreach (var paragraph in noiseLines)
        {
            Rows.Add(new SkipNoiseLineRow(paragraph));
        }

        RowsInfo = string.Format(Se.Language.Video.TextToSpeech.SkipNoiseLinesFoundX, Rows.Count);
    }

    [RelayCommand]
    private void Ok()
    {
        SelectedParagraphs = Rows.Where(r => r.IsSelected).Select(r => r.Paragraph).ToList();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = true;
        }
    }

    [RelayCommand]
    private void InverseSelection()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = !row.IsSelected;
        }
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

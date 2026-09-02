using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main.AssistedMove;

public partial class AssistedMoveViewModel : ObservableObject
{
    [ObservableProperty] private string _subtitleInfo;

    public List<AssistedMoveCandidate> Candidates { get; private set; }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public AssistedMoveCandidate? SelectedCandidate { get; private set; }

    public AssistedMoveViewModel()
    {
        SubtitleInfo = string.Empty;
        Candidates = new List<AssistedMoveCandidate>();
    }

    public void Initialize(SubtitleLineViewModel subtitle, List<AssistedMoveCandidate> candidates)
    {
        Candidates = candidates;
        SubtitleInfo = subtitle.Text;
    }

    [RelayCommand]
    private void Pick(AssistedMoveCandidate candidate)
    {
        SelectedCandidate = candidate;
        OkPressed = true;
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
            return;
        }

        var number = e.Key switch
        {
            Key.D1 or Key.NumPad1 => 1,
            Key.D2 or Key.NumPad2 => 2,
            Key.D3 or Key.NumPad3 => 3,
            Key.D4 or Key.NumPad4 => 4,
            Key.D5 or Key.NumPad5 => 5,
            Key.D6 or Key.NumPad6 => 6,
            _ => 0,
        };

        if (number > 0 && number <= Candidates.Count)
        {
            e.Handled = true;
            Pick(Candidates[number - 1]);
        }
    }
}

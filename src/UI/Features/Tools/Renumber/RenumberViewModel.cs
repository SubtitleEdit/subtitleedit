using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.Renumber;

public partial class RenumberViewModel : ObservableObject
{
    [ObservableProperty] private int _startNumber;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private List<SubtitleLineViewModel> _subtitles;

    public RenumberViewModel()
    {
        _subtitles = new List<SubtitleLineViewModel>();
        StartNumber = 1;
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles) => _subtitles = subtitles;

    [RelayCommand]
    private void Ok()
    {
        for (var i = 0; i < _subtitles.Count; i++)
        {
            _subtitles[i].Number = StartNumber + i;
        }

        OkPressed = true;
        Window?.Close();
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
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/renumber");
        }
    }
}

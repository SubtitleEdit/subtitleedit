using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

public partial class ConvertActorsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public List<SubtitleLineViewModel> FixedSubtitle { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private SubtitleFormat _format = new SubRip();

    public ConvertActorsViewModel()
    {
        AllSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        FixedSubtitle = new List<SubtitleLineViewModel>();
        //StatusText = string.Empty;

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };
    }

    private void UpdatePreview()
    {
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, SubtitleFormat format)
    {
        AllSubtitles.Clear();
        AllSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        _dirty = true;
        _format = format;
        _timerUpdatePreview.Start();
    }

    [RelayCommand]
    private void Ok()
    {
        //FixedSubtitle = Subtitles.Select(p => new SubtitleLineViewModel(p.SubtitleLineViewModel)).ToList();
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/change-formatting");
        }
    }
}
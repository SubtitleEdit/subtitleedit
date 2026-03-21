using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInEffectViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BurnInEffectItem> _effects;

    [ObservableProperty] private ObservableCollection<BurnInEffectItem> _selectedEffects;

    public Window? Window { get; set; }
    public string VideoFileName { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public double PositionInSeconds { get; set; }
    public List<CheckBox> CheckBoxes { get; set; }

    public BurnInEffectViewModel()
    {
        VideoFileName = string.Empty;
        Effects = new ObservableCollection<BurnInEffectItem>(BurnInEffectItem.List());
        SelectedEffects = new ObservableCollection<BurnInEffectItem>();
        CheckBoxes = new List<CheckBox>();  
    }

    public void Initialize(string videoFileName, List<BurnInEffectItem> selectedEffects)
    {
        Dispatcher.UIThread.Post(() =>
        {
            VideoFileName = videoFileName;

            foreach (var effect in Effects.Where(p=>selectedEffects.Any(s => s.Name == p.Name)))
            {
                AddSelectedEffect(effect);
                if (CheckBoxes.Any(c => c.Name == effect.Type.ToString()))
                {
                    CheckBoxes.First(c => c.Name == effect.Type.ToString()).IsChecked = true;
                }
            }
        });
    }

    [RelayCommand]
    private void Ok()
    {
        PositionInSeconds = VideoPlayerControl?.Position ?? 0;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void AddSelectedEffect(BurnInEffectItem effect)
    {
        if (!SelectedEffects.Contains(effect))
        {
            SelectedEffects.Add(effect);
        }
    }

    public void RemoveSelectedEffect(BurnInEffectItem effect)
    {
        SelectedEffects.Remove(effect);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void OnLoaded()
    {
        //Dispatcher.UIThread.Post(async() =>
        //{
        //    await VideoPlayerControl!.Open(VideoFileName);
        //});
    }
}
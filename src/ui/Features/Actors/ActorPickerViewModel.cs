using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Actors;

public partial class ActorPickerViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ActorDisplayItem> _actors = new();

    // Actor to highlight, set by MainViewModel from the current grid selection.
    [ObservableProperty] private string? _highlightedActor;

    [ObservableProperty] private bool _isClearEnabled;

    public Window? Window { get; set; }

    // Also called by MainViewModel after replacing Actors, since new rows start unhighlighted
    // and re-setting HighlightedActor to the same value would not trigger this on its own.
    public void RefreshHighlight()
    {
        foreach (var actor in Actors)
        {
            actor.IsHighlighted = actor.Name == HighlightedActor;
        }
    }

    partial void OnHighlightedActorChanged(string? value) => RefreshHighlight();

    // The four delegates below are set by MainViewModel when it opens the picker, each reusing
    // an existing MainViewModel command instead of duplicating its logic here.
    public Action<string>? OnActorSelected { get; set; }
    public Func<Task>? OnNewActorRequested { get; set; }
    public Action? OnClearRequested { get; set; }
    public Func<string, Task>? OnRenameRequested { get; set; }

    [RelayCommand]
    private void SetActorByName(ActorDisplayItem? actor)
    {
        if (actor != null)
        {
            OnActorSelected?.Invoke(actor.Name);
        }
    }

    [RelayCommand]
    private async Task RenameActor(ActorDisplayItem? actor)
    {
        if (actor != null && OnRenameRequested != null)
        {
            await OnRenameRequested(actor.Name);
        }
    }

    [RelayCommand]
    private async Task NewActor()
    {
        if (OnNewActorRequested != null)
        {
            await OnNewActorRequested();
        }
    }

    [RelayCommand]
    private void Clear()
    {
        OnClearRequested?.Invoke();
    }
}

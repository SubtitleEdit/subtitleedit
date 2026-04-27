using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickRuleProfile;

public partial class PickRuleProfileViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ProfileDisplay> _profiles;
    [ObservableProperty] private ProfileDisplay? _selectedProfile;
    [ObservableProperty] private ObservableCollection<DialogStyleDisplay> _dialogStyles;
    [ObservableProperty] private ObservableCollection<ContinuationStyleDisplay> _continuationStyles;
    [ObservableProperty] private ObservableCollection<CpsLineLengthStrategyDisplay> _cpsLineLengthStrategies;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;

    public PickRuleProfileViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        Profiles = new ObservableCollection<ProfileDisplay>();
        DialogStyles = new ObservableCollection<DialogStyleDisplay>(DialogStyleDisplay.List());
        ContinuationStyles = new ObservableCollection<ContinuationStyleDisplay>(ContinuationStyleDisplay.List());
        CpsLineLengthStrategies = new ObservableCollection<CpsLineLengthStrategyDisplay>(CpsLineLengthStrategyDisplay.List());

        foreach (var profile in Se.Settings.General.Profiles)
        {
            Profiles.Add(new ProfileDisplay(profile, DialogStyles.ToList(), ContinuationStyles.ToList(), CpsLineLengthStrategies.ToList()));
        }
        SelectedProfile = Profiles.FirstOrDefault(p => p.Name == Se.Settings.General.CurrentProfile);
    }

    [RelayCommand]
    private void Ok()
    {
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
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }

    internal void DataGridDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }

    public void DataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedProfile != null)
        {
            e.Handled = true;
            Ok();
        }
    }
}
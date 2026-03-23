using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Shared;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixCommonErrorsProfileViewModel : ObservableObject
{
    public ObservableCollection<ProfileDisplayItem> Profiles { get; set; }
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;
    [ObservableProperty] private bool _isProfileSelected;
    [ObservableProperty] private bool _isProfileDeleteEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public TextBox ProfileNameTextBox { get; internal set; }

    private List<FixRuleDisplayItem> _fixRules;

    public FixCommonErrorsProfileViewModel()
    {
        _fixRules = new List<FixRuleDisplayItem>();
        Profiles = new ObservableCollection<ProfileDisplayItem>();
        SelectedProfile = null;
        IsProfileSelected = true;
        ProfileNameTextBox = new TextBox();
    }

    public void Initialize(List<FixRuleDisplayItem> allFixRules, string? selectedProfileName)
    {
        _fixRules = allFixRules;

        foreach (var rule in Se.Settings.Tools.FixCommonErrors.Profiles)
        {
            var profile = new ProfileDisplayItem
            {
                Name = rule.ProfileName,
                FixRules = new ObservableCollection<FixRuleDisplayItem>()
            };

            foreach (var fixRule in allFixRules)
            {
                var isSelected = rule.SelectedRules.Any(x => x == fixRule.FixCommonErrorFunctionName);
                profile.FixRules.Add(new FixRuleDisplayItem(fixRule) { IsSelected = isSelected });
            }

            Profiles.Add(profile);
        }

        SelectedProfile = Profiles.FirstOrDefault(p => p.Name.Equals(selectedProfileName, StringComparison.OrdinalIgnoreCase)) 
                          ?? Profiles.FirstOrDefault();
        
        IsProfileDeleteEnabled = Profiles.Count > 1;
    }

    [RelayCommand]
    private void NewProfile()
    {
        var newProfile = MakeDefaultProfile("");
        Profiles.Add(newProfile);
        SelectedProfile = newProfile;

        Dispatcher.UIThread.Invoke(() =>
        {
            ProfileNameTextBox.Focus();
            IsProfileDeleteEnabled = Profiles.Count > 1;
        });
    }

    [RelayCommand]
    private void Delete(ProfileDisplayItem? profile)
    {
        if (profile == null || !Profiles.Contains(profile))
        {
            return;
        }

        Profiles.Remove(profile);
        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
        IsProfileDeleteEnabled = Profiles.Count > 1;
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Profiles.Count == 0)
        {
            Profiles.Add(MakeDefaultProfile("Default"));
        }

        foreach (var profile in Profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                await MessageBox.Show(
                    Window!,
                    "Error",
                    "Please enter a profile name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (Profiles.Count(p => p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase)) > 1)
            {
                await MessageBox.Show(
                    Window!,
                    "Error",
                    $"Profile name '{profile.Name}' can only be used once. Please choose a different name.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        var profiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        profiles.Clear();
        foreach (var profile in Profiles)
        {
            profiles.Add(new SeFixCommonErrorsProfile
            {
                ProfileName = profile.Name,
                SelectedRules = profile.FixRules.Where(p => p.IsSelected).Select(p => p.FixCommonErrorFunctionName).ToList(),
            });
        }

        OkPressed = true;
        Se.SaveSettings();
        Window?.Close();
    }

    private ProfileDisplayItem MakeDefaultProfile(string name)
    {
        var profile = new ProfileDisplayItem
        {
            Name = name,
            FixRules = new ObservableCollection<FixRuleDisplayItem>(_fixRules.Select(p => new FixRuleDisplayItem(p) 
            { 
                IsSelected = SeFixCommonErrorsProfile.DefaultFixes.Contains(p.FixCommonErrorFunctionName) 
            }))
        };

        return profile;
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

    internal void ProfileNameTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            OkCommand.Execute(null);
        }
    }
}
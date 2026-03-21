using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ImageBasedProfileViewModel : ObservableObject
{
    public ObservableCollection<ProfileDisplayItem> Profiles { get; set; }
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;
    [ObservableProperty] private bool _isProfileSelected;
    [ObservableProperty] private bool _isProfileDeleteEnabled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public TextBox ProfileNameTextBox { get; internal set; }

    public ImageBasedProfileViewModel()
    {
        Profiles = new ObservableCollection<ProfileDisplayItem>();
        SelectedProfile = null;
        IsProfileSelected = true;
        ProfileNameTextBox = new TextBox();
    }

    public void Initialize(ObservableCollection<SeExportImagesProfile> profiles, SeExportImagesProfile? selectedProfile)
    {
        foreach (var profile in profiles)
        {
            Profiles.Add(new ProfileDisplayItem(profile.ProfileName, profile == selectedProfile, profile));
        }

        SelectedProfile =
            Profiles.FirstOrDefault(p => p.Name.Equals(selectedProfile?.ProfileName, StringComparison.OrdinalIgnoreCase))
            ?? Profiles.FirstOrDefault();

        IsProfileDeleteEnabled = Profiles.Count > 1;
    }

    [RelayCommand]
    private void NewProfile()
    {
        var newProfile = MakeDefaultProfile("");
        Profiles.Add(newProfile);
        SelectedProfile = newProfile;

        Dispatcher.UIThread.Invoke(() => { ProfileNameTextBox.Focus(); });
        IsProfileDeleteEnabled = Profiles.Count > 1;
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

        OkPressed = true;

        Se.Settings.File.ExportImages.Profiles.Clear();
        foreach (var profile in Profiles)
        {
            if (profile.Profile is SeExportImagesProfile seProfile)
            {
                seProfile.ProfileName = profile.Name;
                Se.Settings.File.ExportImages.Profiles.Add(seProfile);
            }
        }

        Se.SaveSettings();
        Window?.Close();
    }

    private ProfileDisplayItem MakeDefaultProfile(string name)
    {
        return new ProfileDisplayItem(string.Empty, false, new SeExportImagesProfile());
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
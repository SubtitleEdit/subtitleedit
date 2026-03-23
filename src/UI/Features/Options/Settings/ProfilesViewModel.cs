using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class ProfilesViewModel : ObservableObject
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

    public ProfilesViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        Profiles = new ObservableCollection<ProfileDisplay>();
        DialogStyles = new ObservableCollection<DialogStyleDisplay>(DialogStyleDisplay.List());
        ContinuationStyles = new ObservableCollection<ContinuationStyleDisplay>(ContinuationStyleDisplay.List());
        CpsLineLengthStrategies = new ObservableCollection<CpsLineLengthStrategyDisplay>(CpsLineLengthStrategyDisplay.List());
    }

    public void Initialize(List<ProfileDisplay> profiles, string profileName)
    {
        Profiles.Clear();
        foreach (var profile in profiles)
        {
            var pd = new ProfileDisplay(profile);
            pd.DialogStyle = DialogStyles.FirstOrDefault(d => d.Code == profile.DialogStyle?.Code) ?? DialogStyles.First();   
            pd.ContinuationStyle = ContinuationStyles.FirstOrDefault(c => c.Code == profile.ContinuationStyle?.Code) ?? ContinuationStyles.First();
            pd.CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(c => c.Code == profile.CpsLineLengthStrategy?.Code) ?? CpsLineLengthStrategies.First();
            Profiles.Add(pd);
        }

        SelectedProfile = Profiles.FirstOrDefault(p => p.Name == profileName);
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService
            .ShowDialogAsync<ProfilesExportWindow, ProfilesExportViewModel>(Window, vm =>
            {
                vm.Initialize(Profiles.ToList());
            });

        if (!result.OkPressed)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".profile", "SE_Rules_profile",  Se.Language.Options.Settings.SaveRuleProfilesFile);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var toExport = result.Profiles.Where(p => p.IsSelected).ToList();
        if (toExport.Count == 0)
        {
            return;
        }

        var export = new ProfileImportExport(toExport);
        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        System.IO.File.WriteAllText(fileName, json);

        await MessageBox.Show(
            Window!,
            Se.Language.General.Information,
            string.Format(Se.Language.Options.Settings.RuleProfilesExportedX, toExport.Count),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Options.Settings.OpenRuleFile, "Rule profile", ".profile");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        // import from json
        List<ProfileDisplay>? imported = null;
        try
        {
            var json = System.IO.File.ReadAllText(fileName);
            var temp = JsonSerializer.Deserialize<ProfileImportExport>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (temp == null)
            {
                imported = new List<ProfileDisplay>();
            }
            else
            {
                imported = temp.ToProfileDisplayList();
            }
        }
        catch (Exception exception)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "Unable to import profiles: " + exception.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        if (imported == null || imported.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "No profiles found in file",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        foreach (var profile in imported)
        {
            var exists = Profiles.FirstOrDefault(p => p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase));
            if (exists != null)
            {
                // rename
                var idx = 2;
                var newName = profile.Name + " " + idx;
                while (Profiles.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    idx++;
                    newName = profile.Name + " " + idx;
                }
                profile.Name = newName;
            }
            Profiles.Add(profile);
        }

        await MessageBox.Show(
            Window!,
            Se.Language.General.Information,
            string.Format(Se.Language.Options.Settings.RuleProfilesImportedX, imported.Count),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    [RelayCommand]
    private void Copy()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        var newProfile = new ProfileDisplay(SelectedProfile);
        var idx = Profiles.IndexOf(SelectedProfile);

        newProfile.Name = SelectedProfile.Name + " 2";
        Profiles.Insert(idx + 1, newProfile);
    }

    [RelayCommand]
    private void Delete()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        var idx = Profiles.IndexOf(SelectedProfile);
        Profiles.Remove(SelectedProfile);
        if (Profiles.Count == 0)
        {
            SelectedProfile = null;
        }
        else if (idx >= Profiles.Count)
        {
            SelectedProfile = Profiles[Profiles.Count - 1];
        }
        else
        {
            SelectedProfile = Profiles[idx];
        }
    }

    [RelayCommand]
    private void Clear()
    {
        Profiles.Clear();
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
    }
}
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertFixCommonErrorsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ProfileDisplayItem> _profiles;
    [ObservableProperty] private ProfileDisplayItem? _selectedProfile;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BatchConvertFixCommonErrorsSettingsViewModel()
    {
        Profiles = new  ObservableCollection<ProfileDisplayItem>(); 
        var profiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        var allFixRules = FixCommonErrorsViewModel.MakeDefaultRules();
        foreach (var setting in profiles)
        {
            var profile = new ProfileDisplayItem()
            {
                Name = setting.ProfileName,
                FixRules = new ObservableCollection<FixRuleDisplayItem>(allFixRules.Select(rule => new FixRuleDisplayItem(rule)
                {
                    IsSelected = setting.SelectedRules.Contains(rule.FixCommonErrorFunctionName)
                }))
            };

            Profiles.Add(profile);
        }
    }

    public void Initialize(Tools.FixCommonErrors.ProfileDisplayItem profile)
    {
        SelectedProfile = Enumerable.FirstOrDefault<ProfileDisplayItem>(Profiles, p=>p.Name == profile.Name);
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
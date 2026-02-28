using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public partial class ChangeCasingViewModel : ObservableObject
{
    [ObservableProperty] private bool _normalCasing;
    [ObservableProperty] private bool _normalCasingFixNames;
    [ObservableProperty] private bool _normalCasingOnlyUpper;
    [ObservableProperty] private bool _fixNamesOnly;
    [ObservableProperty] private bool _allUppercase;
    [ObservableProperty] private bool _allLowercase;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Info { get; private set; }
    public Subtitle Subtitle { get; private set; }

    private readonly IWindowService _windowService;
    private Subtitle _subtitle;

    public ChangeCasingViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        NormalCasing = Se.Settings.Tools.ChangeCasing.NormalCasing;
        NormalCasingFixNames = Se.Settings.Tools.ChangeCasing.NormalCasingFixNames;
        NormalCasingOnlyUpper = Se.Settings.Tools.ChangeCasing.NormalCasingOnlyUpper;
        FixNamesOnly = Se.Settings.Tools.ChangeCasing.FixNamesOnly;
        AllUppercase = Se.Settings.Tools.ChangeCasing.AllUppercase;
        AllLowercase = Se.Settings.Tools.ChangeCasing.AllLowercase;
        _subtitle = new Subtitle();
        Subtitle = new Subtitle();
        Info = string.Empty;
        LoadSettings();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
    }

    private void LoadSettings()
    {
        NormalCasing = Se.Settings.Tools.ChangeCasing.NormalCasing;
        NormalCasingFixNames = Se.Settings.Tools.ChangeCasing.NormalCasingFixNames;
        NormalCasingOnlyUpper = Se.Settings.Tools.ChangeCasing.NormalCasingOnlyUpper;
        FixNamesOnly = Se.Settings.Tools.ChangeCasing.FixNamesOnly;
        AllUppercase = Se.Settings.Tools.ChangeCasing.AllUppercase;
        AllLowercase = Se.Settings.Tools.ChangeCasing.AllLowercase;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.ChangeCasing.NormalCasing = NormalCasing;
        Se.Settings.Tools.ChangeCasing.NormalCasingFixNames = NormalCasingFixNames;
        Se.Settings.Tools.ChangeCasing.NormalCasingOnlyUpper = NormalCasingOnlyUpper;
        Se.Settings.Tools.ChangeCasing.FixNamesOnly = FixNamesOnly;
        Se.Settings.Tools.ChangeCasing.AllUppercase = AllUppercase;
        Se.Settings.Tools.ChangeCasing.AllLowercase = AllLowercase;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        SaveSettings();

        if (FixNamesOnly)
        {
            var result = await ShowFixNames(_subtitle, 0);
            OkPressed = result.OkPressed;
            Subtitle = result.Subtitle;
            Info = result.Info;
            Window?.Close();
            return;
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
        var fixCasing = new FixCasing(language)
        {
            FixNormal = NormalCasing,
            FixNormalOnlyAllUppercase = NormalCasingOnlyUpper,
            FixMakeUppercase = AllUppercase,
            FixMakeLowercase = AllLowercase,
            FixMakeProperCase = false,
            FixProperCaseOnlyAllUppercase = false,
            Format = _subtitle.OriginalFormat,
        };
        fixCasing.Fix(_subtitle);

        if (NormalCasing)
        {
            Info = $"Normal Casing - lines changed: {fixCasing.NoOfLinesChanged}";
            if (NormalCasingFixNames)
            {
                var result = await ShowFixNames(_subtitle, fixCasing.NoOfLinesChanged);
                Subtitle = result.Subtitle;
                OkPressed = result.OkPressed;
                Window?.Close();
                return;
            }
        }
        else if (AllUppercase)
        {
            Info = $"Uppercase - lines changed: {fixCasing.NoOfLinesChanged}";
        }
        else if (AllLowercase)
        {
            Info = $"Lowercase - lines changed: {fixCasing.NoOfLinesChanged}";
        }

        Subtitle = _subtitle;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private async Task<FixNamesViewModel> ShowFixNames(Subtitle subtitle, int noOfFixes)
    {
        var result = await _windowService.ShowDialogAsync<FixNamesWindow, FixNamesViewModel>(Window!, vm =>
        {
            vm.Initialize(subtitle);
        });

        return result;
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
            UiUtil.ShowHelp("features/change-casing");
        }
    }
}
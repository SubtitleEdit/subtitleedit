using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;
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
    [ObservableProperty] private string _normalCasingSample;
    [ObservableProperty] private string _allUppercaseSample;
    [ObservableProperty] private string _allLowercaseSample;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Info { get; private set; }
    public Subtitle Subtitle { get; private set; }

    private readonly IWindowService _windowService;
    private Subtitle _subtitle;
    private string _sampleLine;
    private string _sampleLanguage;

    public ChangeCasingViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _subtitle = new Subtitle();
        Subtitle = new Subtitle();
        Info = string.Empty;
        _normalCasingSample = string.Empty;
        _allUppercaseSample = string.Empty;
        _allLowercaseSample = string.Empty;
        _sampleLine = string.Empty;
        _sampleLanguage = string.Empty;
        LoadSettings();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
        _sampleLine = FindSampleLine(subtitle);
        if (_sampleLine.Length == 0)
        {
            return;
        }

        _sampleLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        AllUppercaseSample = MakeSample(fixCasing => fixCasing.FixMakeUppercase = true);
        AllLowercaseSample = MakeSample(fixCasing => fixCasing.FixMakeLowercase = true);
        UpdateNormalCasingSample();
    }

    /// <summary>
    /// The first line of more than one word, so a card shows the user's own text with enough of it
    /// to see the casing; a line of one word when there is nothing better.
    /// </summary>
    private static string FindSampleLine(Subtitle subtitle)
    {
        var lines = subtitle.Paragraphs
            .Select(p => HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, " ").Replace('\n', ' ').Trim())
            .Where(line => line.Any(char.IsLetter))
            .ToList();
        return lines.FirstOrDefault(line => line.Contains(' ')) ?? lines.FirstOrDefault() ?? string.Empty;
    }

    private string MakeSample(Action<FixCasing> configure)
    {
        var sample = new Subtitle();
        sample.Paragraphs.Add(new Paragraph(_sampleLine, 0, 1000));
        var fixCasing = new FixCasing(_sampleLanguage) { Format = _subtitle.OriginalFormat };
        configure(fixCasing);
        fixCasing.Fix(sample);
        return sample.Paragraphs[0].Text;
    }

    private void UpdateNormalCasingSample()
    {
        if (_sampleLine.Length > 0)
        {
            NormalCasingSample = MakeSample(fixCasing =>
            {
                fixCasing.FixNormal = true;
                fixCasing.FixNormalOnlyAllUppercase = NormalCasingOnlyUpper;
            });
        }
    }

    partial void OnNormalCasingOnlyUpperChanged(bool value) => UpdateNormalCasingSample();

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
                if (result.OkPressed)
                {
                    Subtitle = result.Subtitle;
                    Info = result.Info;
                }
                else
                {
                    // Cancelling the names step only skips the name fixes - the normal casing
                    // was already confirmed with OK on this dialog, so it still applies (as in SE4)
                    Subtitle = _subtitle;
                }

                OkPressed = true;
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
            vm.Initialize(subtitle, noOfFixes);
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
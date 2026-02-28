using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help;

public partial class AboutViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public string TitleText => $"Subtitle Edit {Se.Version}";
    public string LicenseText => "Subtitle Edit is free software under the MIT license.";
    public string DescriptionText =>
       "Subtitle Edit 5 beta is a development version of our upcoming major release." + Environment.NewLine +
       "We are actively refining the new tools and appreciate your help in testing." + Environment.NewLine +
       "Please share your feedback to help us ensure the best possible final version." + Environment.NewLine +
       Environment.NewLine +
       "Thank you for being part of the Subtitle Edit community! :)";

    [RelayCommand]
    private async Task OpenGitHub()
    {
        if (Window == null)
        {
            return;
        }

        await Window.Launcher.LaunchUriAsync(new Uri("https://github.com/SubtitleEdit/subtitleedit"));
    }

    [RelayCommand]
    private async Task OpenPayPal()
    {
        if (Window == null)
        {
            return;
        }
        
        await Window.Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU"));
    }

    [RelayCommand]
    private async Task OpenGitHubSponsor()
    {
        if (Window == null)
        {
            return;
        }
        
        await Window.Launcher.LaunchUriAsync(new Uri("https://github.com/sponsors/niksedk"));
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp();
        }   
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Help;

public class AboutWindow : Window
{
    public AboutWindow(AboutViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Help.AboutSubtitleEdit;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = vm.TitleText,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };

        var translatedByText = new TextBlock
        {
            Text = vm.TranslatedBy,
            FontSize = 12,
        };
        translatedByText.IsVisible = !string.IsNullOrEmpty(Se.Language.TranslatedBy);

        var licenseText = new TextBlock
        {
            Text = vm.LicenseText,
        };

        var uri = new Uri("avares://SubtitleEdit/Assets/about.png");
        var imageAbout = new Image
        {
            Source = new Bitmap(AssetLoader.Open(uri)),
            Stretch = Stretch.Uniform,
            Margin = new Thickness(10),
            Width = 128,
            Height = 128,
        };

        var descriptionText = new TextBlock
        {
            Text = vm.DescriptionText,
            Margin = new Thickness(0, 10, 0, 10)
        };

        var githubLink = new TextBlock
        {
            Text = Se.Language.About.GitHub,
            Foreground = UiUtil.MakeLinkForeground(),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        githubLink.PointerPressed += (_, _) => vm.OpenGitHubCommand.Execute(null);

        var panelGithub = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = Se.Language.About.IssueTrackingAndSourceCode },
                githubLink,
            }
        };

        var paypalLink = new TextBlock
        {
            Text = Se.Language.About.PayPal,
            Foreground = UiUtil.MakeLinkForeground(),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        paypalLink.PointerPressed += (_, _) => vm.OpenPayPalCommand.Execute(null);

        var githubSponsorLink = new TextBlock
        {
            Text = Se.Language.About.GitHubSponsor,
            Foreground = UiUtil.MakeLinkForeground(),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        githubSponsorLink.PointerPressed += (_, _) => vm.OpenGitHubSponsorCommand.Execute(null);

        var panelDonate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new TextBlock { Text = Se.Language.About.Donate },
                paypalLink,
                new TextBlock { Text = Se.Language.About.Or },
                githubSponsorLink
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                translatedByText,
                licenseText,
                imageAbout,
                descriptionText,
                panelGithub,
                panelDonate,
                panelButtons,
            }
        };

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }
}
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help.About;

/// <summary>
/// About in the editor look (see <see cref="EditorChrome"/>): the cycling picture beside the title
/// and license, the description and links under it, OK in the button bar. Wording is unchanged.
/// </summary>
public class AboutWindow : Window
{
    public AboutWindow(AboutViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Help.AboutSubtitleEdit;
        Width = 520;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = vm.TitleText,
            FontSize = UiUtil.ScaledFontSize(20),
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
        };

        var translatedByText = new TextBlock
        {
            Text = vm.TranslatedBy,
            FontSize = UiUtil.ScaledFontSize(12),
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };
        translatedByText.IsVisible = !string.IsNullOrEmpty(Se.Language.TranslatedBy);

        var licenseText = new TextBlock
        {
            Text = vm.LicenseText,
            FontSize = 12.5,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };

        var imageNames = new[] { "about.png", "about1.png", "about2.png", "about3.png" };
        var bitmaps = new Bitmap[imageNames.Length];
        for (var i = 0; i < imageNames.Length; i++)
        {
            var assetUri = new Uri($"avares://SubtitleEdit/Assets/{imageNames[i]}");
            bitmaps[i] = new Bitmap(AssetLoader.Open(assetUri));
        }

        var imageIndex = Random.Shared.Next(bitmaps.Length); // start on a random image

        // Two stacked image layers that cross-dissolve: each cycle the old
        // picture fades out on one layer while the next fades in on the other.
        Image MakeImageLayer() => new()
        {
            Stretch = Stretch.Uniform,
            Opacity = 0,
            Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(700),
                },
            },
        };

        var layerA = MakeImageLayer();
        var layerB = MakeImageLayer();
        layerA.Source = bitmaps[imageIndex];
        var currentLayer = layerA;

        var imageAbout = new Panel
        {
            Width = 128,
            Height = 128,
            VerticalAlignment = VerticalAlignment.Top,
            Children = { layerA, layerB },
        };

        var cycleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(5500) };
        cycleTimer.Tick += (_, _) =>
        {
            imageIndex = (imageIndex + 1) % bitmaps.Length;
            var nextLayer = ReferenceEquals(currentLayer, layerA) ? layerB : layerA;
            nextLayer.Source = bitmaps[imageIndex];
            nextLayer.Opacity = 1;    // new image fades in...
            currentLayer.Opacity = 0; // ...as the old image fades out at the same time
            currentLayer = nextLayer;
        };
        Opened += async (_, _) =>
        {
            await Task.Delay(50); // let the layout settle so the transition runs
            currentLayer.Opacity = 1; // fade the first image in
            cycleTimer.Start();
        };

        Closed += delegate
        {
            cycleTimer.Stop();
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }
        };

        var panelTitle = new StackPanel
        {
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { titleText, translatedByText, licenseText },
        };

        var header = new Grid
        {
            ColumnSpacing = 18,
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
        };
        header.Add(imageAbout, 0, 0);
        header.Add(panelTitle, 0, 1);
        var headerSection = new Border
        {
            Child = header,
            Padding = new Thickness(18, 16),
        };

        var descriptionText = new TextBlock
        {
            Text = vm.DescriptionText,
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
        };

        var githubLink = new TextBlock
        {
            Text = Se.Language.About.GitHub,
            Foreground = UiUtil.MakeLinkForeground(),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        githubLink.PointerPressed += (_, _) => vm.OpenGitHubCommand.Execute(null);

        var panelGithub = new WrapPanel
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

        var panelDonate = new WrapPanel
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

        var panelLinks = new StackPanel
        {
            Spacing = 6,
            Children = { panelGithub, panelDonate },
            [TextBlock.FontSizeProperty] = 12.5,
        };

        var bodySection = new Border
        {
            Child = new StackPanel { Spacing = 12, Children = { descriptionText, panelLinks } },
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(18, 14, 18, 16),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        var sections = new StackPanel { Children = { headerSection, bodySection } };
        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        UiUtil.FocusOnFirstActivation(this, buttonOk); // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }
}

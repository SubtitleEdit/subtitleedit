using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public class OpenFromUrlWindow : Window
{
    private const double CardCornerRadius = 8;
    private const double CardPadding = 16;
    private const double CardSpacing = 12;

    private readonly OpenFromUrlViewModel _vm;
    private readonly TextBox _urlTextBox;

    public OpenFromUrlWindow(OpenFromUrlViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.OpenFromUrlTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Width = 520;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var heading = new TextBlock
        {
            Text = Se.Language.Video.OpenFromUrlTitle,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var urlLabel = new TextBlock
        {
            Text = Se.Language.General.Url,
            Opacity = 0.75,
            Margin = new Thickness(0, 4, 0, 4),
        };

        _urlTextBox = new TextBox
        {
            PlaceholderText = "https://...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Url)) { Mode = BindingMode.TwoWay },
        };

        var downloadSubtitlesCheckBox = new CheckBox
        {
            Content = Se.Language.Video.OpenFromUrlDownloadSubtitles,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 4, 0, 4),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DownloadSubtitles)) { Mode = BindingMode.TwoWay },
        };

        var openOnlineCard = BuildCard(
            "▶", // ▶
            Se.Language.Video.OpenFromUrlOpenOnline,
            Se.Language.Video.OpenFromUrlOpenOnlineDescription,
            Se.Language.Video.OpenFromUrlOpenOnlineNote,
            vm.OpenOnlineCommand,
            isRecommended: false);

        var downloadCard = BuildCard(
            "⬇", // ⬇
            Se.Language.Video.OpenFromUrlDownloadAndOpen,
            Se.Language.Video.OpenFromUrlDownloadAndOpenDescription,
            Se.Language.Video.OpenFromUrlDownloadAndOpenNote,
            vm.DownloadAndOpenCommand,
            isRecommended: true);

        var cancelBar = UiUtil.MakeButtonBar(UiUtil.MakeButtonCancel(vm.CancelCommand));

        var content = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 8,
            Children =
            {
                heading,
                urlLabel,
                _urlTextBox,
                new Border { Height = 4 }, // spacer
                openOnlineCard,
                downloadCard,
                downloadSubtitlesCheckBox,
                cancelBar,
            },
        };

        Content = content;

        // Activated fires before the visual tree is fully ready; deferring the
        // Focus call to Loaded + Input priority makes it actually stick when the
        // window first opens.
        Loaded += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
                () => _urlTextBox.Focus(),
                Avalonia.Threading.DispatcherPriority.Input);
        };
    }

    private Button BuildCard(string iconGlyph, string title, string description, string note, IRelayCommand command, bool isRecommended)
    {
        var accent = isRecommended ? AccentBrush(0.9) : UiUtil.GetBorderBrush();
        var hoverAccent = AccentBrush(1.0);

        var iconText = new TextBlock
        {
            Text = iconGlyph,
            FontSize = 28,
            Foreground = accent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, 16, 0),
            Width = 32,
            TextAlignment = TextAlignment.Center,
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var descriptionText = new TextBlock
        {
            Text = description,
            Opacity = 0.85,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8),
        };

        var noteIcon = new TextBlock
        {
            Text = "ⓘ", // ⓘ
            FontSize = 13,
            Foreground = accent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 6, 0),
        };

        var noteText = new TextBlock
        {
            Text = note,
            Opacity = 0.7,
            FontStyle = FontStyle.Italic,
            TextWrapping = TextWrapping.Wrap,
        };

        var noteRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { noteIcon, noteText },
        };

        var textStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children = { titleText, descriptionText, noteRow },
        };

        var row = new DockPanel
        {
            LastChildFill = true,
        };
        DockPanel.SetDock(iconText, Dock.Left);
        row.Children.Add(iconText);
        row.Children.Add(textStack);

        var button = new Button
        {
            Content = row,
            Padding = new Thickness(CardPadding),
            Margin = new Thickness(0, CardSpacing / 2, 0, CardSpacing / 2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            BorderBrush = accent,
            BorderThickness = new Thickness(isRecommended ? 2 : 1),
            CornerRadius = new CornerRadius(CardCornerRadius),
            Cursor = new Cursor(StandardCursorType.Hand),
            Command = command,
        };

        var hoverStyle = new Style(x => x.OfType<Button>().Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BorderBrushProperty, hoverAccent),
            },
        };
        button.Styles.Add(hoverStyle);

        return button;
    }

    private static IBrush AccentBrush(double opacity)
    {
        var app = Application.Current;
        var isDark = app?.ActualThemeVariant == ThemeVariant.Dark;
        // Pleasant blue that works on both themes
        var color = isDark
            ? Color.FromRgb(0x6F, 0xB1, 0xFF)
            : Color.FromRgb(0x16, 0x6E, 0xD6);
        return new SolidColorBrush(color, opacity);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

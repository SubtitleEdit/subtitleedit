using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

public class TranslationErrorWindow : Window
{
    public TranslationErrorWindow(TranslationErrorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Translate.TranslationError;
        Width = 520;
        MinWidth = 420;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;
        vm.Window = this;

        // ── Header row: icon + bold title ──────────────────────────────────────
        var errorIcon = new TextBlock
        {
            Text = "⚠",
            FontSize = 32,
            Foreground = new SolidColorBrush(Color.Parse("#E8A020")),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
        };

        var titleBlock = new TextBlock
        {
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };
        titleBlock.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FriendlyMessage)));

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10),
        };
        headerPanel.Children.Add(errorIcon);
        headerPanel.Children.Add(titleBlock);

        // ── Hint text ──────────────────────────────────────────────────────────
        var hintBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.85,
            Margin = new Thickness(0, 0, 0, 14),
        };
        hintBlock.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Hint)));

        // ── Details expander ───────────────────────────────────────────────────
        var detailsLabel = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 4),
            Text = Se.Language.Translate.TechnicalDetails,
        };

        var detailsTextBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 200,
            FontFamily = new FontFamily("Consolas,Menlo,monospace"),
            FontSize = 11,
            Margin = new Thickness(0),
        };
        detailsTextBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.TechnicalDetails)));

        var detailsScroll = new ScrollViewer
        {
            Content = detailsTextBox,
            MaxHeight = 200,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };

        var detailsPanel = new StackPanel
        {
            Spacing = 0,
        };
        detailsPanel.Children.Add(detailsLabel);
        detailsPanel.Children.Add(detailsScroll);
        detailsPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.DetailsExpanded)));

        // ── Toggle details link ────────────────────────────────────────────────
        var toggleButton = UiUtil.MakeLink(vm.ToggleDetailsText, vm.ToggleDetailsCommand, vm, nameof(vm.ToggleDetailsText));
        toggleButton.HorizontalAlignment = HorizontalAlignment.Left;
        toggleButton.Margin = new Thickness(0, 0, 0, 8);

        // ── OK button ─────────────────────────────────────────────────────────
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk);

        // ── Separator line ────────────────────────────────────────────────────
        var separator = new Border
        {
            Height = 1,
            Margin = new Thickness(0, 10, 0, 10),
            Background = new SolidColorBrush(Color.Parse("#33FFFFFF")),
        };

        // ── Main content stack ────────────────────────────────────────────────
        var content = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 0,
        };
        content.Children.Add(headerPanel);
        content.Children.Add(hintBlock);
        content.Children.Add(toggleButton);
        content.Children.Add(detailsPanel);
        content.Children.Add(separator);
        content.Children.Add(buttonBar);

        Content = content;

        Activated += delegate { buttonOk.Focus(); };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        ((TranslationErrorViewModel)DataContext!).KeyDown(e);
    }
}

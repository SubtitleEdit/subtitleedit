using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// A failed translation in the editor look (see <see cref="EditorChrome"/>): the message and hint
/// on top, the technical details in a section under it on demand, the details link and OK in the
/// button bar.
/// </summary>
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

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        // ── Header row: icon + bold title ──────────────────────────────────────
        var errorIcon = new TextBlock
        {
            Text = "⚠",
            FontSize = UiUtil.ScaledFontSize(28),
            // Darker amber on the light theme - the dark-theme tone is too faint on white (#15768).
            Foreground = new SolidColorBrush(Color.Parse(UiTheme.IsDarkThemeEnabled() ? "#E8A020" : "#9E5500")),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, -4, 12, 0),
        };

        var titleBlock = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(15),
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };
        titleBlock.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FriendlyMessage)));

        // ── Hint text ──────────────────────────────────────────────────────────
        var hintBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
        };
        hintBlock.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Hint)));

        var messagePanel = new StackPanel
        {
            Spacing = 6,
            Children = { titleBlock, hintBlock },
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
        };
        headerGrid.Add(errorIcon, 0, 0);
        headerGrid.Add(messagePanel, 0, 1);

        // ── Details ────────────────────────────────────────────────────────────
        var detailsTextBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 200,
            FontFamily = new FontFamily("Consolas,Menlo,monospace"),
            FontSize = UiUtil.ScaledFontSize(11),
            VerticalContentAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0),
        };
        detailsTextBox.Classes.Add("text");
        AutomationProperties.SetName(detailsTextBox, Se.Language.Translate.TechnicalDetails);
        detailsTextBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.TechnicalDetails)));

        var detailsScroll = new ScrollViewer
        {
            Content = detailsTextBox,
            MaxHeight = 200,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };

        // ── Toggle details link ────────────────────────────────────────────────
        var toggleButton = UiUtil.MakeLink(vm.ToggleDetailsText, vm.ToggleDetailsCommand, vm, nameof(vm.ToggleDetailsText));
        toggleButton.HorizontalAlignment = HorizontalAlignment.Left;

        var detailsSection = EditorChrome.MakeEditorSection(Se.Language.Translate.TechnicalDetails, detailsScroll);
        detailsSection.Bind(IsVisibleProperty, new Binding(nameof(vm.DetailsExpanded)));

        // ── OK button ─────────────────────────────────────────────────────────
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonBar = EditorChrome.MakeButtonBar(toggleButton, buttonOk);

        // ── Main content stack ────────────────────────────────────────────────
        var sections = new StackPanel
        {
            Children =
            {
                new Border { Child = headerGrid, Padding = new Thickness(16, 16, 16, 16) },
                detailsSection,
            },
        };

        Content = EditorChrome.MakeRoot(sections, buttonBar);

        UiUtil.FocusOnFirstActivation(this, buttonOk);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        ((TranslationErrorViewModel)DataContext!).KeyDown(e);
    }
}

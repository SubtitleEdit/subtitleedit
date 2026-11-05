using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;

/// <summary>
/// Chatterbox engine and model status, in the editor look (see <see cref="EditorChrome"/>): engine
/// name and description on top, then engine, models and voice cloning sections, Close in the
/// button bar.
/// </summary>
public class ChatterboxTtsSettingsWindow : Window
{
    private const double LabelWidth = 120;
    private const double ValueWidth = 340;

    private readonly ChatterboxTtsSettingsViewModel _vm;

    public ChatterboxTtsSettingsWindow(ChatterboxTtsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ChatterboxTtsSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 560;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(MakeHeader("Chatterbox TTS", new ChatterboxTtsCpp().Description));
        sections.Children.Add(MakeEngineSection(vm));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Models, MakeModelList(vm)));
        sections.Children.Add(MakeVoiceCloningSection(vm, out var sourceLanguageCombo));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, sourceLanguageCombo);
    }

    /// <summary>
    /// Compact combo boxes.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
    }

    private static Border MakeHeader(string title, string description)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = EditorChrome.ScaledFont(16),
            FontWeight = FontWeight.SemiBold,
        });
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 500,
            HorizontalAlignment = HorizontalAlignment.Left,
        });

        return new Border { Child = panel, Padding = new Thickness(14, 14, 14, 12) };
    }

    private static Border MakeEngineSection(ChatterboxTtsSettingsViewModel vm)
    {
        var rows = new StackPanel { Spacing = 8 };

        var enginePanel = MakeStatusPanel(nameof(vm.EngineBrush), nameof(vm.EngineLabel));
        var engineButton = UiUtil.MakeButton(string.Empty, vm.RedownloadEngineCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.EngineDownloadButtonText));
        enginePanel.Children.Add(engineButton);
        rows.Children.Add(Row(Se.Language.General.Status, enginePanel));

        var folderText = new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 0),
            MinHeight = 28,
            Height = EditorChrome.ScaledSize(28),
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = EditorChrome.FieldFontSize,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ModelsFolder)),
        };
        AutomationProperties.SetName(folderText, Se.Language.General.InstallFolder);
        var openFolder = EditorChrome.CreateToolButton(IconNames.FolderOpen, Se.Language.General.OpenContainingFolder, vm.OpenModelsFolderCommand, 28);
        rows.Children.Add(Row(Se.Language.General.InstallFolder, new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { folderText, openFolder },
        }));

        return EditorChrome.MakeEditorSection(Se.Language.General.Engine, rows);
    }

    /// <summary>
    /// One row per downloadable model pair: name, install-state dot, and its own download
    /// button. Built as a list rather than fixed rows because the Base weights ship in three
    /// quantizations (Q8_0 / F16 / Q4_K) alongside Turbo.
    /// </summary>
    private static ItemsControl MakeModelList(ChatterboxTtsSettingsViewModel vm)
    {
        return new ItemsControl
        {
            ItemsSource = vm.Models,
            ItemTemplate = new FuncDataTemplate<ChatterboxModelStatusViewModel>((_, _) =>
            {
                var name = new TextBlock
                {
                    Width = LabelWidth + 30,
                    FontSize = EditorChrome.FieldFontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(ChatterboxModelStatusViewModel.DisplayName)),
                };
                var status = MakeStatusPanel(
                    nameof(ChatterboxModelStatusViewModel.StatusBrush),
                    nameof(ChatterboxModelStatusViewModel.StatusLabel));
                status.Width = 120;
                var button = UiUtil.MakeButton(string.Empty)
                    .WithIconLeftBindText(IconNames.Download,
                        nameof(ChatterboxModelStatusViewModel.DownloadButtonText),
                        nameof(ChatterboxModelStatusViewModel.DownloadButtonAccessibleName));
                button.Bind(Button.CommandProperty,
                    new Binding(nameof(ChatterboxModelStatusViewModel.DownloadCommand)));

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Margin = new Thickness(0, 0, 0, 6),
                    Children = { name, status, button },
                };
            }, true),
        };
    }

    private static Border MakeVoiceCloningSection(ChatterboxTtsSettingsViewModel vm, out ComboBox sourceLanguageCombo)
    {
        // Language spoken in imported reference WAVs — sent as `source_lang` so cross-lingual
        // cloning engages when the target language differs. Chatterbox clones from the WAV alone
        // and never asks for a transcript, so there is normally nothing to detect this from.
        sourceLanguageCombo = UiUtil.MakeComboBox(vm.SourceLanguages, vm, nameof(vm.SelectedSourceLanguage));
        sourceLanguageCombo.MinWidth = 160;
        AutomationProperties.SetName(sourceLanguageCombo, "Reference language");
        var sourceLanguageHint = new TextBlock
        {
            Text = "Language spoken in imported reference WAVs (for cross-lingual cloning).",
            FontSize = EditorChrome.ScaledFont(11.5),
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ValueWidth,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var rows = new StackPanel { Spacing = 6 };
        rows.Children.Add(Row("Reference language", sourceLanguageCombo));
        rows.Children.Add(Row(string.Empty, sourceLanguageHint));
        return EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.AdvancedTtsVoiceCloning, rows);
    }

    /// <summary>
    /// A dim label of fixed width and the content after it, so the rows line up.
    /// </summary>
    private static Grid Row(string label, Control content)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    private static StackPanel MakeStatusPanel(string brushBindingPath, string labelBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 9,
            Height = 9,
            VerticalAlignment = VerticalAlignment.Center,
            [!Ellipse.FillProperty] = new Binding(brushBindingPath),
        };
        var text = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { dot, text },
        };
    }

    private static Border MakeButtonBar(ChatterboxTtsSettingsViewModel vm)
    {
        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        close.Classes.Add("primary");

        return EditorChrome.WrapButtonBar(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children = { close },
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

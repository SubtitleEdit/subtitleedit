using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText.ForcedAlignerSetup;

public class ForcedAlignerSetupWindow : Window
{
    public ForcedAlignerSetupWindow(ForcedAlignerSetupViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        // Explicit width plus height-only auto-sizing: WidthAndHeight comes out far too
        // wide on macOS.
        Width = 620;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Title = Se.Language.File.Import.ForcedAlignerSetupTitle;
        vm.Window = this;
        DataContext = vm;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var labelIntro = UiUtil.MakeTextBlock(Se.Language.File.Import.ForcedAlignerSetupIntro);
        labelIntro.TextWrapping = TextWrapping.Wrap;
        labelIntro.FontSize = 12.5;
        labelIntro.VerticalAlignment = VerticalAlignment.Center;

        var introIcon = new ContentControl
        {
            FontSize = UiUtil.ScaledFontSize(15),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Attached.SetIcon(introIcon, IconNames.Waveform);
        // Keep the glyph white on the colored square in the dark theme too (#12717).
        introIcon.Classes.Add(UiTheme.IconOnAccentClassName);

        var introGlyph = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(7),
            Background = StatusDots.Green,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Child = introIcon,
        };

        var panelIntro = new DockPanel();
        DockPanel.SetDock(introGlyph, Dock.Left);
        panelIntro.Children.Add(introGlyph);
        panelIntro.Children.Add(labelIntro);

        // Same dot vocabulary as everywhere else: green installed, amber update waiting,
        // grey not installed - always paired with text, never colour alone.
        var engineDot = new Ellipse
        {
            Width = 10,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
        };
        engineDot.Bind(Shape.FillProperty, new Binding(nameof(vm.EngineDotBrush)));

        var labelEngineStatus = UiUtil.MakeTextBlock(string.Empty, vm, nameof(vm.EngineStatus), null);
        labelEngineStatus.VerticalAlignment = VerticalAlignment.Center;
        labelEngineStatus.FontSize = 12.5;
        labelEngineStatus.TextWrapping = TextWrapping.Wrap;

        var panelEngineStatus = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { engineDot, labelEngineStatus },
        };

        var buttonDownloadEngine = EditorChrome.MakeTextToolButton(IconNames.CloudDownload, Se.Language.File.Import.ForcedAlignerDownloadEngine, vm.DownloadEngineCommand);
        buttonDownloadEngine.Margin = new Thickness(10, 0, 0, 0);

        var comboAligner = UiUtil.MakeComboBox(vm.Aligners, vm, nameof(vm.SelectedAligner));
        comboAligner.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboAligner.Width = double.NaN;
        comboAligner.WithAccessibleName(Se.Language.File.Import.ForcedAlignerModel);
        comboAligner.ItemTemplate = StatusDots.ComboItemTemplate<ForcedAlignerOption>(
            aligner => aligner.BaseDisplay,
            aligner => aligner.Size,
            aligner => aligner.IsInstalled ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled);

        var alignerDot = new Ellipse
        {
            Width = 10,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
        };
        alignerDot.Bind(Shape.FillProperty, new Binding(nameof(vm.AlignerDotBrush)));

        var labelAlignerStatusText = UiUtil.MakeTextBlock(string.Empty, vm, nameof(vm.AlignerStatus), null);
        labelAlignerStatusText.VerticalAlignment = VerticalAlignment.Center;
        labelAlignerStatusText.FontSize = 12.5;
        labelAlignerStatusText.Foreground = EditorChrome.DimTextBrush;

        var labelAlignerStatus = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { alignerDot, labelAlignerStatusText },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var rowEndsFromSpeech = EditorChrome.MakeToggleRow(nameof(vm.EndsFromIsolatedSpeech), Se.Language.File.Import.ForcedAlignerEndsFromSpeech, out _);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(rowEndsFromSpeech, Se.Language.File.Import.ForcedAlignerEndsFromSpeechHint);
        }

        var intro = new Border
        {
            Padding = new Thickness(14, 14, 14, 12),
            Child = panelIntro,
        };

        // Engine: the status on the left, the download on the right
        var panelEngine = new DockPanel();
        DockPanel.SetDock(buttonDownloadEngine, Dock.Right);
        panelEngine.Children.Add(buttonDownloadEngine);
        panelEngine.Children.Add(panelEngineStatus);

        var panelAligner = new StackPanel
        {
            Spacing = 10,
            Children = { comboAligner, labelAlignerStatus, rowEndsFromSpeech },
        };

        var sections = new StackPanel
        {
            Children =
            {
                intro,
                EditorChrome.MakeEditorSection(Se.Language.General.Engine, panelEngine),
                EditorChrome.MakeEditorSection(Se.Language.File.Import.ForcedAlignerModel, panelAligner),
            },
        };

        Content = EditorChrome.MakeCancelOkRoot(sections, null, buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, comboAligner); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}

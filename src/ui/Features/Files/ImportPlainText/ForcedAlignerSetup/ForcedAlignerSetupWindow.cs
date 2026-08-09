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

        var labelIntro = UiUtil.MakeTextBlock(Se.Language.File.Import.ForcedAlignerSetupIntro);
        labelIntro.TextWrapping = TextWrapping.Wrap;
        labelIntro.MaxWidth = 530;
        labelIntro.VerticalAlignment = VerticalAlignment.Center;

        var introIcon = new ContentControl
        {
            FontSize = 15,
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

        var panelIntro = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { introGlyph, labelIntro },
        };

        var labelEngine = UiUtil.MakeLabel(Se.Language.General.Engine);

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

        var panelEngineStatus = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { engineDot, labelEngineStatus },
        };

        var buttonDownloadEngine = UiUtil.MakeButton(
            Se.Language.File.Import.ForcedAlignerDownloadEngine, vm.DownloadEngineCommand);
        Attached.SetIcon(buttonDownloadEngine, IconNames.CloudDownload);

        var labelAligner = UiUtil.MakeLabel(Se.Language.File.Import.ForcedAlignerModel);
        var comboAligner = UiUtil.MakeComboBox(vm.Aligners, vm, nameof(vm.SelectedAligner));
        comboAligner.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboAligner.Width = 420;
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

        var labelAlignerStatus = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { alignerDot, labelAlignerStatusText },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // intro
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // engine
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // engine status
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // aligner
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // aligner status
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelIntro, 0, 0, 1, 2);
        grid.Add(labelEngine, 1, 0);
        grid.Add(buttonDownloadEngine, 1, 1);
        grid.Add(panelEngineStatus, 2, 0, 1, 2);
        grid.Add(labelAligner, 3, 0);
        grid.Add(comboAligner, 3, 1);
        grid.Add(labelAlignerStatus, 4, 0, 1, 2);
        grid.Add(panelButtons, 5, 0, 1, 2);

        Content = grid;

        Activated += delegate { comboAligner.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}

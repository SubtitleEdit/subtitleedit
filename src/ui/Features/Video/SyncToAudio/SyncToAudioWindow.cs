using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SyncToAudio;

/// <summary>
/// Sync subtitle to audio, in the editor look (see <see cref="EditorChrome"/>): what to sync
/// against, the two options, the result, and Analyze / Cancel / Apply in the button bar.
/// </summary>
public class SyncToAudioWindow : Window
{
    public SyncToAudioWindow(SyncToAudioViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Video.SyncToAudioTitle);
        SizeToContent = SizeToContent.Height;
        Width = 560;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var lang = Se.Language.Video;

        // Sync against
        var radioAudio = new RadioButton
        {
            Content = lang.SyncToAudioReferenceAudio,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.UseAudioReference)) { Mode = BindingMode.TwoWay },
            [!IsEnabledProperty] = new Binding(nameof(vm.IsAudioReferenceAvailable)),
        };
        var radioSubtitle = new RadioButton
        {
            Content = lang.SyncToAudioReferenceSubtitle,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.UseSubtitleReference)) { Mode = BindingMode.TwoWay },
        };
        var textReferenceFile = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = EditorChrome.DimTextBrush,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ReferenceSubtitleDisplayName)),
        };
        var buttonBrowse = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", lang.SyncToAudioPickReferenceSubtitle, vm.BrowseReferenceSubtitleCommand, 28);
        var panelReferenceFile = new DockPanel
        {
            Margin = new Thickness(28, 0, 0, 0),
            [!IsEnabledProperty] = new Binding(nameof(vm.UseSubtitleReference)),
        };
        DockPanel.SetDock(buttonBrowse, Dock.Right);
        panelReferenceFile.Children.Add(buttonBrowse);
        panelReferenceFile.Children.Add(textReferenceFile);
        var panelReference = new StackPanel
        {
            Spacing = 6,
            Children = { radioAudio, radioSubtitle, panelReferenceFile },
        };

        // Options
        var numericMaxOffset = UiUtil.MakeNumericUpDownInt(1, 36000, 120, 110, vm, nameof(vm.MaxOffsetSeconds))
            .WithAccessibleName(lang.SyncToAudioMaxOffsetSeconds);
        numericMaxOffset.Increment = 10;
        numericMaxOffset.Classes.Add("field"); // the compact field, not the theme's big spinner
        var panelMaxOffset = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { EditorChrome.MakeDimLabel(lang.SyncToAudioMaxOffsetSeconds, new Thickness(0, 0, 8, 0)), numericMaxOffset },
        };
        var panelOptions = new StackPanel
        {
            Spacing = 8,
            Children = { panelMaxOffset, EditorChrome.MakeToggleRow(nameof(vm.TryFrameRates), lang.SyncToAudioTryFrameRates) },
        };

        // Result
        var textOffset = MakeResultText(nameof(vm.ResultOffsetText));
        textOffset.FontWeight = FontWeight.SemiBold;
        var panelResultLines = new StackPanel
        {
            Spacing = 2,
            Children = { textOffset, MakeResultText(nameof(vm.ResultFrameRateText)), MakeResultText(nameof(vm.ResultConfidenceText)) },
            [!IsVisibleProperty] = new Binding(nameof(vm.HasResult)),
        };
        var textLowConfidence = new TextBlock
        {
            Text = lang.SyncToAudioLowConfidence,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            [!IsVisibleProperty] = new Binding(nameof(vm.IsLowConfidence)),
        };
        var textError = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ErrorText)),
        };
        var textHint = new TextBlock
        {
            Text = lang.SyncToAudioHint,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorChrome.DimTextBrush,
        };
        var panelResult = new StackPanel
        {
            Spacing = 8,
            Children = { panelResultLines, textLowConfidence, textError, textHint },
        };

        var content = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(lang.SyncToAudioReference, panelReference, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Options, panelOptions),
                EditorChrome.MakeEditorSection(lang.SyncToAudioResult, panelResult),
            },
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressView = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsAnalyzing));

        var buttonAnalyze = UiUtil.MakeButton(lang.SyncToAudioAnalyze, vm.AnalyzeCommand);
        buttonAnalyze.Bind(IsEnabledProperty, new Binding(nameof(vm.IsAnalyzing)) { Converter = Logic.ValueConverters.InverseBooleanConverter.Instance });
        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.OkCommand);
        buttonApply.Classes.Add("primary");
        buttonApply.Bind(IsEnabledProperty, new Binding(nameof(vm.HasResult)));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeDockedButtonBar(buttonAnalyze, buttonCancel, buttonApply);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(EditorChrome.MakePanel(content), 0, 0);
        root.Add(progressView, 1, 0);
        root.Add(buttonBar, 2, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, radioAudio); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) =>
        {
            UiUtil.RestoreWindowPosition(this);
            vm.OnLoaded();
        };
        Closing += (_, _) =>
        {
            UiUtil.SaveWindowPosition(this);
            vm.OnClosing();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TextBlock MakeResultText(string property)
    {
        return new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            [!TextBlock.TextProperty] = new Binding(property),
        };
    }
}

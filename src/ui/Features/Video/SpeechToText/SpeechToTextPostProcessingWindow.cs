using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class SpeechToTextPostProcessingWindow : Window
{
    private readonly SpeechToTextPostProcessingViewModel _vm;

    public SpeechToTextPostProcessingWindow(SpeechToTextPostProcessingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.WhisperPostProcessingTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelAdjustTimings = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.AdjustTimings);
        var checkAdjustTimings = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.AdjustTimings));

        var labelMergeShortLines = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.MergeShortLines);
        var checkMergeShortLines = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.MergeShortLines));

        var labelBreakSplitLongLines = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.BreakSplitLongLines);
        var checkBreakSplitLongLines = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.BreakSplitLongLines));

        var labelFixShortDuration = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.FixShortDuration);
        var checkFixShortDuration = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.FixShortDuration));

        var labelFixCasing = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.FixCasing);
        var checkFixCasing = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.FixCasing));

        var labelAddPeriods = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.AddPeriods);
        var checkAddPeriods = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.AddPeriods));

        var labelChangeUnderlineToColor = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.ChangeUnderlineToColor);
        var checkChangeUnderlineToColor = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.ChangeUnderlineToColor));
        var colorPickerUnderlineToColor = UiUtil.MakeColorPickerButton(_vm, nameof(_vm.ChangeUnderlineToColorColor));

        // Cue building for the helper-script engines (MLX Whisper / Faster Whisper Mac):
        // subtitle cues rebuilt from word timestamps per the Netflix Timed Text Style
        // Guide / BBC guideline limits, editable here instead of via command-line flags.
        var labelCueBuilding = UiUtil.MakeLabel(Se.Language.Video.AudioToText.CueBuilding).WithBold();
        var labelCueRebuild = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.CueRebuild);
        var checkCueRebuild = UiUtil.MakeCheckBox(vm, nameof(SpeechToTextPostProcessingViewModel.CueRebuild));
        var labelCueMaxChars = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.CueMaxChars);
        var numericCueMaxChars = UiUtil.MakeNumericUpDownInt(20, 200, 84, 120, vm, nameof(SpeechToTextPostProcessingViewModel.CueMaxChars));
        var labelCueMaxSeconds = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.CueMaxSeconds);
        var numericCueMaxSeconds = UiUtil.MakeNumericUpDownDouble(1, 15, 7, 120, vm, nameof(SpeechToTextPostProcessingViewModel.CueMaxSeconds));
        var labelCueMaxCps = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.CueMaxCps);
        var numericCueMaxCps = UiUtil.MakeNumericUpDownDouble(5, 60, 20, 120, vm, nameof(SpeechToTextPostProcessingViewModel.CueMaxCps));
        var labelVocabularyPrompt = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.VocabularyPrompt);
        var textBoxVocabularyPrompt = UiUtil.MakeTextBox(260, vm, nameof(SpeechToTextPostProcessingViewModel.VocabularyPrompt));
        var labelBeamSize = UiUtil.MakeTextBlock(Se.Language.Video.AudioToText.BeamSize);
        var numericBeamSize = UiUtil.MakeNumericUpDownInt(0, 10, 5, 120, vm, nameof(SpeechToTextPostProcessingViewModel.BeamSize));


        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.Ok, vm.OKCommand),
            UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;

        grid.Add(labelAdjustTimings, row, 0);
        grid.Add(checkAdjustTimings, row, 1);
        row++;

        grid.Add(labelMergeShortLines, row, 0);
        grid.Add(checkMergeShortLines, row, 1);
        row++;

        grid.Add(labelBreakSplitLongLines, row, 0);
        grid.Add(checkBreakSplitLongLines, row, 1);
        row++;

        grid.Add(labelFixShortDuration, row, 0);
        grid.Add(checkFixShortDuration, row, 1);
        row++;

        grid.Add(labelFixCasing, row, 0);
        grid.Add(checkFixCasing, row, 1);
        row++;

        grid.Add(labelAddPeriods, row, 0);
        grid.Add(checkAddPeriods, row, 1);
        row++;

        grid.Add(labelChangeUnderlineToColor, row, 0);
        grid.Add(checkChangeUnderlineToColor, row, 1);
        grid.Add(colorPickerUnderlineToColor, row, 2);
        row++;

        grid.Add(labelCueBuilding, row, 0, 1, 3);
        row++;

        grid.Add(labelCueRebuild, row, 0);
        grid.Add(checkCueRebuild, row, 1);
        row++;

        grid.Add(labelCueMaxChars, row, 0);
        grid.Add(numericCueMaxChars, row, 1, 1, 2);
        row++;

        grid.Add(labelCueMaxSeconds, row, 0);
        grid.Add(numericCueMaxSeconds, row, 1, 1, 2);
        row++;

        grid.Add(labelCueMaxCps, row, 0);
        grid.Add(numericCueMaxCps, row, 1, 1, 2);
        row++;
        grid.Add(labelVocabularyPrompt, row, 0);
        grid.Add(textBoxVocabularyPrompt, row, 1, 1, 2);
        row++;
        grid.Add(labelBeamSize, row, 0);
        grid.Add(numericBeamSize, row, 1, 1, 2);
        row++;

        grid.Add(buttonPanel, row, 0, 1, 3);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

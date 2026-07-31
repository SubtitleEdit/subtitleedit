using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

public class LlamaCppAdvancedSettingsWindow : Window
{
    private const int LabelWidth = 170;
    private const int EditorWidth = 420;

    private readonly LlamaCppAdvancedSettingsViewModel _vm;

    public LlamaCppAdvancedSettingsWindow(LlamaCppAdvancedSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Translate.AdvancedSettings;
        // Explicit width: WidthAndHeight sizing comes out too wide on macOS.
        Width = 680;
        SizeToContent = SizeToContent.Height;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);
    }

    private static Border BuildContent(LlamaCppAdvancedSettingsViewModel vm)
    {
        var header = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                new TextBlock
                {
                    Text = Se.Language.Translate.AdvancedSettings,
                    FontSize = 18,
                    FontWeight = FontWeight.SemiBold,
                },
                new TextBlock
                {
                    Text = Se.Language.Translate.AdvancedSettingsSubtitle,
                    FontSize = 12,
                    Opacity = 0.75,
                    Margin = new Thickness(0, 2, 0, 0),
                },
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                header,
                MakeCard(BuildContextSection(vm)),
                MakeCard(BuildSamplingSection(vm)),
                buttonPanel,
            },
        };

        return new Border
        {
            Child = stack,
            Padding = UiUtil.MakeWindowMargin(),
        };
    }

    private static Grid BuildContextSection(LlamaCppAdvancedSettingsViewModel vm)
    {
        var grid = MakeFormGrid(6);

        var synopsisBox = MakeMultilineTextBox(vm, nameof(vm.Synopsis), 56);
        SetHint(synopsisBox, Se.Language.Translate.SynopsisHint);
        AddRow(grid, 0, Se.Language.Translate.Synopsis, synopsisBox);

        var glossaryBox = MakeMultilineTextBox(vm, nameof(vm.Glossary), 76);
        glossaryBox.PlaceholderText = Se.Language.Translate.GlossaryHint;
        SetHint(glossaryBox, Se.Language.Translate.GlossaryHint);
        AddRow(grid, 1, Se.Language.Translate.Glossary, glossaryBox);

        var promptBox = MakeMultilineTextBox(vm, nameof(vm.Prompt), 56);
        promptBox.PlaceholderText = LlamaCppAdvancedProtocol.DefaultPrompt;
        SetHint(promptBox, Se.Language.Translate.CustomPromptHint);
        AddRow(grid, 2, Se.Language.Translate.PromptText, promptBox);

        AddRow(grid, 3, Se.Language.Translate.PreviousLinesAsContext,
            UiUtil.MakeNumericUpDownInt(0, 50, 12, 120, vm, nameof(vm.HistoryPairs)));

        AddRow(grid, 4, Se.Language.Translate.LinesPerBatch,
            UiUtil.MakeNumericUpDownInt(1, 50, 10, 120, vm, nameof(vm.BatchSize)));

        var stylePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                UiUtil.MakeComboBox(vm.Formalities, vm, nameof(vm.SelectedFormality)).WithWidth(120),
                UiUtil.MakeCheckBox(Se.Language.Translate.KeepLineBreaks, vm, nameof(vm.KeepLineBreaks)),
            },
        };
        AddRow(grid, 5, Se.Language.General.Formality, stylePanel);

        return grid;
    }

    private static Grid BuildSamplingSection(LlamaCppAdvancedSettingsViewModel vm)
    {
        // Four columns (label, editor, label, editor) so the four sampling values sit in a
        // 2x2 block instead of one long row that overflows the window width.
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };
        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        var samplingHeader = new TextBlock
        {
            Text = Se.Language.Translate.SamplingParameters + " (" + Se.Language.Translate.MinusOneMeansDefault + ")",
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 4),
        };
        grid.Add(samplingHeader, 0, 0);
        Grid.SetColumnSpan(samplingHeader, 4);

        grid.Add(MakeSmallLabel(Se.Language.Translate.Temperature), 1, 0);
        grid.Add(UiUtil.MakeNumericUpDownTwoDecimals(-1, 2, 110, vm, nameof(vm.Temperature)), 1, 1);
        grid.Add(MakeSmallLabel(Se.Language.Translate.TopP), 1, 2);
        grid.Add(UiUtil.MakeNumericUpDownTwoDecimals(-1, 1, 110, vm, nameof(vm.TopP)), 1, 3);

        grid.Add(MakeSmallLabel(Se.Language.Translate.TopK), 2, 0);
        grid.Add(UiUtil.MakeNumericUpDownInt(-1, 500, -1, 110, vm, nameof(vm.TopK)), 2, 1);
        grid.Add(MakeSmallLabel(Se.Language.Translate.RepeatPenalty), 2, 2);
        grid.Add(UiUtil.MakeNumericUpDownTwoDecimals(-1, 3, 110, vm, nameof(vm.RepeatPenalty)), 2, 3);

        var maxTokens = UiUtil.MakeNumericUpDownInt(-1, 32768, -1, 150, vm, nameof(vm.MaxTokens));
        grid.Add(MakeSmallLabel(Se.Language.Translate.MaxTokensPerReply), 3, 0);
        grid.Add(maxTokens, 3, 1);
        Grid.SetColumnSpan(maxTokens, 3);

        var contextSize = UiUtil.MakeNumericUpDownInt(2048, 262144, 16384, 150, vm, nameof(vm.ContextSize));
        contextSize.Increment = 2048;
        grid.Add(MakeSmallLabel(Se.Language.Translate.ServerContextSizeTokens), 4, 0);
        grid.Add(contextSize, 4, 1);
        Grid.SetColumnSpan(contextSize, 3);

        return grid;
    }

    private static Grid MakeFormGrid(int rows)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };

        for (var i = 0; i < rows; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        return grid;
    }

    private static void AddRow(Grid grid, int row, string label, Control editor)
    {
        var labelBlock = new TextBlock
        {
            Text = label,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center,
        };
        grid.Add(labelBlock, row, 0);
        grid.Add(editor, row, 1);
    }

    private static TextBlock MakeSmallLabel(string text) => new()
    {
        Text = text,
        Opacity = 0.7,
        VerticalAlignment = VerticalAlignment.Center,
    };

    private static TextBox MakeMultilineTextBox(LlamaCppAdvancedSettingsViewModel vm, string propertyPath, double height)
    {
        var textBox = UiUtil.MakeTextBox(EditorWidth, vm, propertyPath);
        textBox.Height = height;
        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.VerticalContentAlignment = VerticalAlignment.Top;
        return textBox;
    }

    private static void SetHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(control, hint);
        }
    }

    private static Border MakeCard(Control child)
    {
        return new Border
        {
            Child = child,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

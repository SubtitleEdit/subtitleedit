using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// Advanced LLM translation settings in the editor look (see <see cref="EditorChrome"/>): the
/// context (synopsis, glossary, prompt, batching, style) and the sampling/server parameters in
/// sections that scroll, with Cancel and OK in the button bar.
/// </summary>
public class LlamaCppAdvancedSettingsWindow : Window
{
    private const int LabelWidth = 170;

    private readonly LlamaCppAdvancedSettingsViewModel _vm;

    public LlamaCppAdvancedSettingsWindow(LlamaCppAdvancedSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AdvancedSettings;
        // Explicit width: WidthAndHeight sizing comes out too wide on macOS.
        Width = 680;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);
    }

    private static Grid BuildContent(LlamaCppAdvancedSettingsViewModel vm)
    {
        var header = EditorChrome.MakeDialogHeader(Se.Language.General.AdvancedSettings, Se.Language.Translate.AdvancedSettingsSubtitle);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeFlushButtonBar(null, buttonCancel, buttonOk);

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.Translate.Context, BuildContextSection(vm)),
                EditorChrome.MakeEditorSection(Se.Language.Translate.SamplingParameters, BuildSamplingSection(vm),
                    EditorChrome.MakeDimLabel(Se.Language.Translate.MinusOneMeansDefault, new Thickness(0))),
            },
        };

        // The sections scroll while the header and OK/Cancel stay put: at high display scaling
        // (e.g. 175%) the window is clamped to the screen height and the lower settings and
        // buttons were cut off with no way to reach them (#15973).
        var scrollViewer = new ScrollViewer
        {
            Content = sections,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var body = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };
        body.Add(header, 0, 0);
        body.Add(scrollViewer, 1, 0);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        grid.Add(new Border { Child = body, Background = EditorChrome.PanelBrush }, 0, 0);
        grid.Add(buttonBar, 1, 0);
        return grid;
    }

    private static StackPanel BuildContextSection(LlamaCppAdvancedSettingsViewModel vm)
    {
        var synopsisBox = MakeMultilineTextBox(vm, nameof(vm.Synopsis), 56, Se.Language.Translate.Synopsis);
        SetHint(synopsisBox, Se.Language.Translate.SynopsisHint);

        var glossaryBox = MakeMultilineTextBox(vm, nameof(vm.Glossary), 76, Se.Language.Translate.Glossary);
        glossaryBox.PlaceholderText = Se.Language.Translate.GlossaryHint;
        SetHint(glossaryBox, Se.Language.Translate.GlossaryHint);

        var promptBox = MakeMultilineTextBox(vm, nameof(vm.Prompt), 56, Se.Language.Translate.PromptText);
        promptBox.PlaceholderText = LlamaCppAdvancedProtocol.DefaultPrompt;
        SetHint(promptBox, Se.Language.Translate.CustomPromptHint);

        // The prompt label carries the reset button, so the editor column keeps its full width.
        var promptLabel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Translate.PromptText, new Thickness(0)),
                EditorChrome.CreateToolButton(IconNames.Restore, Se.Language.Translate.ResetPromptToDefault, vm.ResetPromptCommand, 24),
            },
        };
        var promptRow = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        promptRow.Add(promptLabel, 0, 0);
        promptRow.Add(promptBox, 0, 1);

        var historyPairs = Field(UiUtil.MakeNumericUpDownInt(0, 50, 12, 110, vm, nameof(vm.HistoryPairs)), Se.Language.Translate.PreviousLinesAsContext);
        var batchSize = Field(UiUtil.MakeNumericUpDownInt(1, 50, 10, 110, vm, nameof(vm.BatchSize)), Se.Language.Translate.LinesPerBatch);

        var comboFormality = UiUtil.MakeComboBox(vm.Formalities, vm, nameof(vm.SelectedFormality)).WithWidth(130)
            .WithAccessibleName(Se.Language.General.Formality);
        var switchKeepLineBreaks = EditorChrome.MakeSwitch(nameof(vm.KeepLineBreaks), Se.Language.Translate.KeepLineBreaks);
        var stylePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20,
            Children =
            {
                comboFormality,
                EditorChrome.MakeSwitchRow(switchKeepLineBreaks, Se.Language.Translate.KeepLineBreaks),
            },
        };

        return new StackPanel
        {
            Spacing = 10,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.Translate.Synopsis, synopsisBox, LabelWidth, spacing: 10, alignTop: true),
                EditorChrome.MakeLabeledRow(Se.Language.Translate.Glossary, glossaryBox, LabelWidth, spacing: 10, alignTop: true),
                promptRow,
                EditorChrome.MakeLabeledRow(Se.Language.Translate.PreviousLinesAsContext, historyPairs, LabelWidth, spacing: 10),
                EditorChrome.MakeLabeledRow(Se.Language.Translate.LinesPerBatch, batchSize, LabelWidth, spacing: 10),
                EditorChrome.MakeLabeledRow(Se.Language.General.Formality, stylePanel, LabelWidth, spacing: 10),
            },
        };
    }

    private static StackPanel BuildSamplingSection(LlamaCppAdvancedSettingsViewModel vm)
    {
        // Four columns (label, editor, label, editor) so the four sampling values sit in a
        // 2x2 block instead of one long row that overflows the window width.
        var sampling = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        sampling.Add(Label(Se.Language.General.Temperature), 0, 0);
        sampling.Add(Field(UiUtil.MakeNumericUpDownTwoDecimals(-1, 2, 110, vm, nameof(vm.Temperature)), Se.Language.General.Temperature), 0, 1);
        sampling.Add(Label(Se.Language.Translate.TopP, new Thickness(16, 0, 0, 0)), 0, 2);
        sampling.Add(Field(UiUtil.MakeNumericUpDownTwoDecimals(-1, 1, 110, vm, nameof(vm.TopP)), Se.Language.Translate.TopP), 0, 3);

        sampling.Add(Label(Se.Language.Translate.TopK), 1, 0);
        sampling.Add(Field(UiUtil.MakeNumericUpDownInt(-1, 500, -1, 110, vm, nameof(vm.TopK)), Se.Language.Translate.TopK), 1, 1);
        sampling.Add(Label(Se.Language.Translate.RepeatPenalty, new Thickness(16, 0, 0, 0)), 1, 2);
        sampling.Add(Field(UiUtil.MakeNumericUpDownTwoDecimals(-1, 3, 110, vm, nameof(vm.RepeatPenalty)), Se.Language.Translate.RepeatPenalty), 1, 3);

        sampling.Children[4].IsVisible = sampling.Children[5].IsVisible = vm.ShowTopK;
        sampling.Children[6].IsVisible = sampling.Children[7].IsVisible = vm.ShowRepeatPenalty;

        var maxTokens = Field(UiUtil.MakeNumericUpDownInt(-1, 32768, -1, 150, vm, nameof(vm.MaxTokens)), Se.Language.Translate.MaxTokensPerReply);

        var contextSize = Field(UiUtil.MakeNumericUpDownInt(2048, 262144, 16384, 150, vm, nameof(vm.ContextSize)), Se.Language.Translate.ServerContextSizeTokens);
        contextSize.Increment = 2048;

        // Free-form llama-server arguments for the SE-managed local server (#13830) - lets users
        // tune flags SE does not curate (e.g. partial GPU offload on a too-small card).
        var serverArguments = UiUtil.MakeTextBox(300, vm, nameof(vm.ServerArguments))
            .WithAccessibleName(Se.Language.Translate.ExtraServerParameters);
        serverArguments.Classes.Add(EditorChrome.TextClass);
        serverArguments.HorizontalAlignment = HorizontalAlignment.Left;
        SetHint(serverArguments, Se.Language.Translate.ExtraServerParametersHint);

        // Opt out of SE's own launch flags entirely (#13865): appending user arguments last already
        // overrides anything that takes a value, but a bare switch like --swa-full cannot be undone.
        var switchServerArgumentsOnly = EditorChrome.MakeSwitch(nameof(vm.ServerArgumentsOnly), Se.Language.Translate.UseOnlyExtraServerParameters);
        var rowServerArgumentsOnly = EditorChrome.MakeSwitchRow(switchServerArgumentsOnly, Se.Language.Translate.UseOnlyExtraServerParameters);
        SetHint(rowServerArgumentsOnly, Se.Language.Translate.UseOnlyExtraServerParametersHint);

        return new StackPanel
        {
            Spacing = 10,
            Children =
            {
                sampling,
                EditorChrome.MakeLabeledRow(Se.Language.Translate.MaxTokensPerReply, maxTokens, LabelWidth, spacing: 10),
                Visible(EditorChrome.MakeLabeledRow(Se.Language.Translate.ServerContextSizeTokens, contextSize, LabelWidth, spacing: 10), vm.ShowServerOptions),
                Visible(EditorChrome.MakeLabeledRow(Se.Language.Translate.ExtraServerParameters, serverArguments, LabelWidth, spacing: 10), vm.ShowServerOptions),
                Visible(EditorChrome.MakeLabeledRow(string.Empty, rowServerArgumentsOnly, LabelWidth, spacing: 10), vm.ShowServerOptions),
            },
        };
    }

    private static Control Visible(Control control, bool visible) { control.IsVisible = visible; return control; }

    private static TextBlock Label(string text, Thickness? margin = null)
    {
        var label = EditorChrome.MakeDimLabel(text, margin ?? new Thickness(0));
        label.TextTrimming = TextTrimming.CharacterEllipsis;
        return label;
    }

    private static NumericUpDown Field(NumericUpDown field, string name)
    {
        field.Classes.Add("field");
        field.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(field, name);
        return field;
    }

    private static TextBox MakeMultilineTextBox(LlamaCppAdvancedSettingsViewModel vm, string propertyPath, double height, string name)
    {
        var textBox = UiUtil.MakeTextBox(double.NaN, vm, propertyPath);
        textBox.Classes.Add(EditorChrome.TextClass);
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBox.Height = height;
        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.VerticalContentAlignment = VerticalAlignment.Top;
        AutomationProperties.SetName(textBox, name);
        return textBox;
    }

    private static void SetHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(control, hint);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

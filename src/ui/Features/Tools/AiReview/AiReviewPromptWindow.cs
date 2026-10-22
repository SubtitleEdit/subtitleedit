using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// The AI review prompt editor in the editor look (see <see cref="EditorChrome"/>): the prompt
/// and the reference context as two sections, the JSON contract note under them, Reset/Cancel/OK
/// in the button bar.
/// </summary>
public class AiReviewPromptWindow : Window
{
    public AiReviewPromptWindow(AiReviewPromptViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.AiReview.EditPromptTitle;
        Width = 640;
        MinWidth = 480;
        Height = 700;
        MinHeight = 520;
        CanResize = true;
        // The Edit dialogs' styles: compact fields that keep the field color while focused
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.AiReview;

        // ---------- prompt ----------
        var textBox = MakeMultiLineTextBox(nameof(vm.PromptText), 140);
        Avalonia.Automation.AutomationProperties.SetName(textBox, l.EditPromptTitle);

        var comboTemplate = new ComboBox
        {
            ItemsSource = vm.Templates,
            PlaceholderText = l.TemplatePlaceholder,
            MinWidth = 280,
            VerticalAlignment = VerticalAlignment.Center,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedTemplate)) { Mode = BindingMode.TwoWay },
        };
        Avalonia.Automation.AutomationProperties.SetName(comboTemplate, l.Template);
        var panelTemplate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { EditorChrome.MakeDimLabel(l.Template, new Thickness(0)), comboTemplate },
        };

        var promptSection = MakeSection(Se.Language.General.Prompt, panelTemplate, l.PromptInfo, textBox, first: true);

        // ---------- reference context ----------
        var buttonGenerate = UiUtil.MakeButton(l.GenerateContext, vm.GenerateContextCommand);
        buttonGenerate.Bind(Button.ContentProperty, new Binding(nameof(vm.GenerateButtonText)));
        buttonGenerate.Bind(IsVisibleProperty, new Binding(nameof(vm.CanGenerate)));
        buttonGenerate.MinHeight = 28;
        buttonGenerate.Height = 28;
        buttonGenerate.Padding = new Thickness(10, 0);
        buttonGenerate.FontSize = EditorChrome.FieldFontSize;
        buttonGenerate.VerticalContentAlignment = VerticalAlignment.Center;
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonGenerate, l.GenerateContextHint);
        }

        var labelGenerateStatus = EditorChrome.MakeBoundHint(nameof(vm.GenerateStatus));
        labelGenerateStatus.FontSize = EditorChrome.FieldFontSize;
        labelGenerateStatus.MaxWidth = 260;
        var panelGenerate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { labelGenerateStatus, buttonGenerate },
        };

        var textBoxContext = MakeMultiLineTextBox(nameof(vm.ContextText), 90);
        textBoxContext.PlaceholderText = l.ReferenceContextPlaceholder;
        textBoxContext[!TextBox.IsReadOnlyProperty] = new Binding(nameof(vm.IsGenerating));
        Avalonia.Automation.AutomationProperties.SetName(textBoxContext, l.ReferenceContext);

        var contextSection = MakeSection(l.ReferenceContext, panelGenerate, l.ReferenceContextInfo, textBoxContext, first: false);

        // ---------- protocol note ----------
        var labelProtocol = EditorChrome.MakeHint(l.ProtocolInfo);
        var borderProtocol = new Border
        {
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 12),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                ColumnSpacing = 8,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon
                    {
                        Value = "fa-solid fa-circle-info",
                        FontSize = 12,
                        Foreground = EditorChrome.FaintTextBrush,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 2, 0, 0),
                    },
                    labelProtocol,
                },
            },
        };
        Grid.SetColumn(labelProtocol, 1);

        // ---------- buttons ----------
        var buttonReset = EditorChrome.MakeActionButton("fa-solid fa-rotate-left", l.ResetToDefault, vm.ResetToDefaultCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(buttonReset, buttonCancel, buttonOk);

        // The prompt editor takes the star row so a long prompt scrolls inside the
        // text box instead of growing the window past the screen edge and pushing
        // the OK/Cancel buttons out of reach.
        var sections = new Grid
        {
            RowDefinitions = new RowDefinitions("3*,2*,Auto"),
        };
        sections.Add(promptSection, 0, 0);
        sections.Add(contextSection, 1, 0);
        sections.Add(borderProtocol, 2, 0);

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        root.Add(EditorChrome.MakePanel(sections), 0, 0);
        root.Add(buttonBar, 1, 0);

        Content = root;

        Loaded += delegate
        {
            textBox.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate
        {
            vm.OnClosing();
            UiUtil.SaveWindowPosition(this);
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TextBox MakeMultiLineTextBox(string property, double minHeight)
    {
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = minHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            Padding = new Thickness(8, 6),
            [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        textBox.Classes.Add("text");
        return textBox;
    }

    /// <summary>
    /// A section whose text box takes the rest of its height: header (with actions on the right),
    /// a dim explanation, then the text box.
    /// </summary>
    private static Border MakeSection(string title, Control headerRight, string info, TextBox textBox, bool first)
    {
        var header = new DockPanel { MinHeight = 28 };
        DockPanel.SetDock(headerRight, Dock.Right);
        header.Children.Add(headerRight);
        header.Children.Add(EditorChrome.MakeSectionHeader(title));

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*"),
            RowSpacing = 8,
        };
        grid.Add(header, 0, 0);
        grid.Add(EditorChrome.MakeHint(info), 1, 0);
        grid.Add(textBox, 2, 0);

        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }
}

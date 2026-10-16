using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class EditRuleWindow : Window
{
    public EditRuleWindow(EditRuleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);

        // Rules are often long regular expressions, so the window is resizable and the
        // find/replace boxes wrap and grow with it - a fixed-width single-line box only
        // ever showed ~60 characters of a rule (#13530).
        Width = 700;
        Height = 460;
        MinWidth = 450;
        MinHeight = 320;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var labelFindWhat = MakeTopLabel(Se.Language.Edit.MultipleReplace.FindWhat);
        var textBoxFindWhat = MakeWrappingTextBox(vm, nameof(vm.FindWhat), Se.Language.Edit.MultipleReplace.FindWhat);

        var labelReplaceWith = MakeTopLabel(Se.Language.General.ReplaceWith);
        var textBoxReplaceWith = MakeWrappingTextBox(vm, nameof(vm.ReplaceWith), Se.Language.General.ReplaceWith);

        var labelDescription = EditorChrome.MakeDimLabel(Se.Language.Edit.MultipleReplace.DescriptionOptional, new Thickness(0));
        var textBoxDescription = MakeStretchingTextBox(vm, nameof(vm.Description), Se.Language.General.Description);

        var radioButtonRegularExpression = UiUtil.MakeRadioButton(Se.Language.General.RegularExpression, vm, nameof(vm.IsRegularExpression));
        var radioButtonCaseSensitive = UiUtil.MakeRadioButton(Se.Language.General.CaseSensitive, vm, nameof(vm.IsCaseSensitive));
        var radioButtonCaseInsensitive = UiUtil.MakeRadioButton(Se.Language.General.CaseInsensitive, vm, nameof(vm.IsCaseInsensitive));
        var panelType = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 18,
            Children =
            {
                radioButtonRegularExpression,
                radioButtonCaseSensitive,
                radioButtonCaseInsensitive,
            }
        };

        var switchWholeWord = EditorChrome.MakeSwitch(nameof(vm.IsWholeWord), Se.Language.Edit.Find.WholeWord)
            .WithBindIsEnabled(nameof(vm.IsWholeWordEnabled));
        var panelOptions = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                panelType,
                EditorChrome.MakeSwitchRow(switchWholeWord, Se.Language.Edit.Find.WholeWord),
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        // Label column and a star field column: nothing pins the width of the boxes (#13530).
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(16, 14, 16, 14),
            ColumnSpacing = 12,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelFindWhat, 0, 0);
        grid.Add(textBoxFindWhat, 0, 1);

        grid.Add(labelReplaceWith, 1, 0);
        grid.Add(textBoxReplaceWith, 1, 1);

        grid.Add(labelDescription, 2, 0);
        grid.Add(textBoxDescription, 2, 1);

        var sectionOptions = EditorChrome.MakeEditorSection(Se.Language.General.Type, panelOptions);
        sectionOptions.Padding = new Thickness(16, 12, 16, 14);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        root.Add(EditorChrome.MakePanel(grid), 0, 0);
        root.Add(new Border { Child = sectionOptions, Background = EditorChrome.PanelBrush }, 1, 0);
        root.Add(buttonPanel, 2, 0);

        Content = root;

        RegexContextFlyout.Attach(textBoxFindWhat, vm, () => vm.IsRegularExpression);
        RegexContextFlyout.Attach(textBoxReplaceWith, vm, () => vm.IsRegularExpression, isReplaceBox: true);

        UiUtil.FocusOnFirstActivation(this, textBoxFindWhat); // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
        Loaded += (s, e) =>
        {
            Title = vm.Title;
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
    }

    private static TextBlock MakeTopLabel(string text)
    {
        // Lines up with the first text line of the wrapping box beside it.
        var label = EditorChrome.MakeDimLabel(text, new Thickness(0, 6, 0, 0));
        label.VerticalAlignment = VerticalAlignment.Top;
        return label;
    }

    /// <summary>
    /// Find/replace box: fills the window and wraps, so a long rule can be read in full.
    /// AcceptsReturn stays false - Enter is the dialog's OK shortcut.
    /// </summary>
    private static TextBox MakeWrappingTextBox(EditRuleViewModel vm, string propertyTextPath, string automationName)
    {
        var textBox = new TextBox
        {
            AcceptsReturn = false,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            Classes = { "text" },
            DataContext = vm,
            [!TextBox.TextProperty] = new Binding(propertyTextPath) { Mode = BindingMode.TwoWay },
        };

        ScrollViewer.SetVerticalScrollBarVisibility(textBox, ScrollBarVisibility.Auto);
        AutomationProperties.SetName(textBox, automationName);

        return textBox;
    }

    private static TextBox MakeStretchingTextBox(EditRuleViewModel vm, string propertyTextPath, string automationName)
    {
        var textBox = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Classes = { "text" },
            DataContext = vm,
            [!TextBox.TextProperty] = new Binding(propertyTextPath) { Mode = BindingMode.TwoWay },
        };

        AutomationProperties.SetName(textBox, automationName);

        return textBox;
    }
}

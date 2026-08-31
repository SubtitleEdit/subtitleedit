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
        vm.Window = this;
        DataContext = vm;

        var labelFindWhat = MakeTopLabel(Se.Language.Edit.MultipleReplace.FindWhat);
        var textBoxFindWhat = MakeWrappingTextBox(vm, nameof(vm.FindWhat), Se.Language.Edit.MultipleReplace.FindWhat);

        var labelReplaceWith = MakeTopLabel(Se.Language.General.ReplaceWith);
        var textBoxReplaceWith = MakeWrappingTextBox(vm, nameof(vm.ReplaceWith), Se.Language.General.ReplaceWith);

        var labelDescription = UiUtil.MakeLabel(Se.Language.Edit.MultipleReplace.DescriptionOptional);
        var textBoxDescription = MakeStretchingTextBox(vm, nameof(vm.Description), Se.Language.General.Description);

        var radioButtonRegularExpression = UiUtil.MakeRadioButton(Se.Language.General.RegularExpression, vm, nameof(vm.IsRegularExpression));
        var radioButtonCaseSensitive = UiUtil.MakeRadioButton(Se.Language.General.CaseSensitive, vm, nameof(vm.IsCaseSensitive));
        var radioButtonCaseInsensitive = UiUtil.MakeRadioButton(Se.Language.General.CaseInsensitive, vm, nameof(vm.IsCaseInsensitive));
        var panelType = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                radioButtonRegularExpression,
                radioButtonCaseSensitive,
                radioButtonCaseInsensitive
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
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

        grid.Add(panelType, 3, 1);

        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;

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

    private static Label MakeTopLabel(string text)
    {
        var label = UiUtil.MakeLabel(text);
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
            DataContext = vm,
            [!TextBox.TextProperty] = new Binding(propertyTextPath) { Mode = BindingMode.TwoWay },
        };

        AutomationProperties.SetName(textBox, automationName);

        return textBox;
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

/// <summary>
/// Change casing, in the editor look (see <see cref="EditorChrome"/>): the casing choices as cards
/// with what each makes of the first line (normal casing with its two options as switches) in one
/// section, Cancel and OK at the bottom.
/// </summary>
public class ChangeCasingWindow : Window
{
    private const string CasingGroupName = "Casing";
    private const double CardTextIndent = 28;

    public ChangeCasingWindow(ChangeCasingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.ChangeCasing);
        SizeToContent = SizeToContent.Height;
        Width = 440;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var radioButtonNormalCasing = MakeRadio(Se.Language.General.NormalCasing, nameof(vm.NormalCasing));

        var rowNormalCasingFixNames = EditorChrome.MakeToggleRow(nameof(vm.NormalCasingFixNames), Se.Language.Tools.ChangeCasing.FixNames);
        rowNormalCasingFixNames.Bind(IsEnabledProperty, new Binding(nameof(vm.NormalCasing)));
        rowNormalCasingFixNames.Bind(OpacityProperty, new Binding(nameof(vm.NormalCasing)) { Converter = EnabledOpacityConverter });

        var rowNormalCasingOnlyUpper = EditorChrome.MakeToggleRow(nameof(vm.NormalCasingOnlyUpper), Se.Language.Tools.ChangeCasing.OnlyFixUppercaseLines);
        rowNormalCasingOnlyUpper.Bind(IsEnabledProperty, new Binding(nameof(vm.NormalCasing)));
        rowNormalCasingOnlyUpper.Bind(OpacityProperty, new Binding(nameof(vm.NormalCasing)) { Converter = EnabledOpacityConverter });

        var normalCasingOptions = new Border
        {
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Margin = new Thickness(CardTextIndent, 8, 0, 0),
            Padding = new Thickness(0, 8, 0, 0),
            Child = new StackPanel { Spacing = 6, Children = { rowNormalCasingFixNames, rowNormalCasingOnlyUpper } },
        };

        var panel = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                MakeCard(radioButtonNormalCasing, nameof(vm.NormalCasingSample), normalCasingOptions),
                MakeCard(MakeRadio(Se.Language.Tools.ChangeCasing.FixNamesOnly, nameof(vm.FixNamesOnly))),
                MakeCard(MakeRadio(Se.Language.Tools.ChangeCasing.AllUppercase, nameof(vm.AllUppercase)), nameof(vm.AllUppercaseSample)),
                MakeCard(MakeRadio(Se.Language.Tools.ChangeCasing.AllLowercase, nameof(vm.AllLowercase)), nameof(vm.AllLowercaseSample)),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = new StackPanel();
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Casing, panel, first: true));

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, radioButtonNormalCasing); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    // a disabled row's label (plain TextBlock) has no disabled look, so dim the whole row
    private static readonly FuncValueConverter<bool, double> EnabledOpacityConverter = new(on => on ? 1.0 : 0.5);

    private static RadioButton MakeRadio(string text, string property)
    {
        return new RadioButton
        {
            Content = text,
            GroupName = CasingGroupName, // each one sits in its own card, not in one parent
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
    }

    /// <summary>
    /// A choice as a card: the radio button, what it makes of the first line (a choice without
    /// a sample shows none), and its options. The whole card picks the choice.
    /// </summary>
    private static Border MakeCard(RadioButton radioButton, string? sampleProperty = null, Control? options = null)
    {
        var panel = new StackPanel();
        panel.Children.Add(radioButton);

        var sample = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(12),
            Margin = new Thickness(CardTextIndent, -4, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        if (sampleProperty != null)
        {
            sample.Bind(TextBlock.TextProperty, new Binding(sampleProperty));
            sample.Bind(IsVisibleProperty, new Binding(sampleProperty) { Converter = StringConverters.IsNotNullOrEmpty });
            panel.Children.Add(sample);
        }

        if (options != null)
        {
            panel.Children.Add(options);
        }

        var card = new Border
        {
            Child = panel,
            Padding = new Thickness(12, 6, 12, 8),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        card.PointerPressed += (_, _) => radioButton.IsChecked = true;

        void ShowChecked()
        {
            var isChecked = radioButton.IsChecked == true;
            card.Background = isChecked ? EditorChrome.AccentSoftBrush : EditorChrome.FieldBrush;
            card.BorderBrush = isChecked ? EditorChrome.AccentBrush : EditorChrome.LineBrush;
            radioButton.FontWeight = isChecked ? FontWeight.SemiBold : FontWeight.Normal;
            sample.Foreground = isChecked ? EditorChrome.StrongTextBrush : EditorChrome.DimTextBrush;
        }

        radioButton.IsCheckedChanged += (_, _) => ShowChecked();
        ShowChecked();
        return card;
    }
}

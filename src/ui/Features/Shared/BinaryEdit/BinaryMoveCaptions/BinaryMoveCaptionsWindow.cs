using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryMoveCaptions;

/// <summary>
/// Move captions into or out of the letterbox bars, in the editor look (see <see cref="EditorChrome"/>):
/// the letterbox, where to move the captions and the result summary, Cancel/OK at the bottom.
/// </summary>
public class BinaryMoveCaptionsWindow : Window
{
    private const double LabelWidth = 140;

    public BinaryMoveCaptionsWindow(BinaryMoveCaptionsViewModel vm)
    {
        var l = Se.Language.Tools.ImageBasedEdit;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = l.MoveCaptions;
        Width = 460;
        SizeToContent = SizeToContent.Height; // a fixed 400 px left an empty band above the buttons
        CanResize = false;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        // Letterbox
        var comboBoxRatio = UiUtil.MakeComboBox(vm.LetterboxRatios, vm, nameof(vm.SelectedLetterboxRatio), null);
        comboBoxRatio.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxRatio, l.Letterbox);

        var numericBarHeight = UiUtil.MakeNumericUpDownInt(0, 8192, 0, 100, vm, nameof(vm.BarHeight));
        numericBarHeight.Classes.Add("field");
        numericBarHeight.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsBarHeightEditable)));

        var letterboxPanel = new StackPanel { Spacing = 10 };
        letterboxPanel.Children.Add(comboBoxRatio);
        letterboxPanel.Children.Add(BinaryEditChrome.MakeRow(l.BarHeightPx, numericBarHeight, LabelWidth));

        // Move to
        var numericOffset = UiUtil.MakeNumericUpDownInt(0, 8192, 10, 100, vm, nameof(vm.Offset));
        numericOffset.Classes.Add("field");

        var moveToPanel = new StackPanel { Spacing = 2 };
        var radioIntoBars = UiUtil.MakeRadioButton(l.MoveCaptionsIntoBars, vm, nameof(vm.MoveIntoBars), "mode");
        var radioIntoPicture = UiUtil.MakeRadioButton(l.MoveCaptionsIntoPicture, vm, nameof(vm.MoveIntoPicture), "mode");
        radioIntoBars.FontSize = EditorChrome.FieldFontSize;
        radioIntoPicture.FontSize = EditorChrome.FieldFontSize;
        moveToPanel.Children.Add(radioIntoBars);
        moveToPanel.Children.Add(radioIntoPicture);
        var offsetRow = BinaryEditChrome.MakeRow(l.MoveCaptionsOffset, numericOffset, LabelWidth);
        offsetRow.Margin = new Thickness(0, 8, 0, 0);
        moveToPanel.Children.Add(offsetRow);

        var infoText = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InfoText)),
            TextWrapping = TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(14, 12, 14, 14),
        };

        var sections = new StackPanel();
        sections.Children.Add(EditorChrome.MakeEditorSection(l.Letterbox, letterboxPanel, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(l.MoveCaptionsTo, moveToPanel));
        sections.Children.Add(new Border
        {
            Child = infoText,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        });

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = EditorChrome.MakeRoot(new ScrollViewer { Content = sections, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled }, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, comboBoxRatio); // an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

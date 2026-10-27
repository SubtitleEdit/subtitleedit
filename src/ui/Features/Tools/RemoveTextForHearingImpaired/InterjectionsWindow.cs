using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

/// <summary>
/// The interjections list and the "skip if start with" list side by side, in the editor look
/// (see <see cref="EditorChrome"/>), Cancel and OK at the bottom.
/// </summary>
public class InterjectionsWindow : Window
{
    private readonly InterjectionsViewModel _vm;

    public InterjectionsWindow(InterjectionsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Interjections;
        CanResize = false;
        Width = 700;
        Height = 600;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var textBoxInterjections = MakeListTextBox(nameof(vm.InterjectionsText), Se.Language.General.Interjections);
        var textBoxSkipList = MakeListTextBox(nameof(vm.InterjectionsSkipStartText), Se.Language.Tools.RemoveTextForHearingImpaired.SkipIfStartWith);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(MakeListSection(Se.Language.General.Interjections, textBoxInterjections, false), 0, 0);
        sections.Add(MakeListSection(Se.Language.Tools.RemoveTextForHearingImpaired.SkipIfStartWith, textBoxSkipList, true), 0, 1);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxInterjections); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    private TextBox MakeListTextBox(string property, string accessibleName)
    {
        var textBox = new TextBox
        {
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
        }.BindText(_vm, property).WithAccessibleName(accessibleName);
        textBox.Classes.Add("text");
        textBox.VerticalContentAlignment = VerticalAlignment.Top; // one word per line, from the top
        return textBox;
    }

    /// <summary>
    /// A section whose text box fills the height, with a line to its left when it is the second one.
    /// </summary>
    private static Border MakeListSection(string title, Control content, bool lineLeft)
    {
        var grid = new Grid
        {
            RowSpacing = 10,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(EditorChrome.MakeSectionHeader(title), 0, 0);
        grid.Add(content, 1, 0);
        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(lineLeft ? 1 : 0, 0, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

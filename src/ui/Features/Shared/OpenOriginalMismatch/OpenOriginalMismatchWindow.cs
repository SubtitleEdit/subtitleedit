using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Shared.OpenOriginalMismatch;

/// <summary>
/// Asked when the original subtitle being opened does not line up 1:1 with the current subtitle.
/// The two modes are shown as cards rather than a plain radio list, because the choice decides what
/// the grid will look like for the rest of the session and what happens to the lines with no
/// counterpart - see <see cref="OpenOriginalMismatchViewModel"/> and issue #13449. In the editor
/// look (see <see cref="EditorChrome"/>).
/// </summary>
public class OpenOriginalMismatchWindow : Window
{
    private const double ContentWidth = 520;

    public OpenOriginalMismatchWindow(OpenOriginalMismatchViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.OpenOriginalSubtitleFileTitle;
        SizeToContent = SizeToContent.Height;
        Width = ContentWidth + 40;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var header = MakeHeader(vm);

        // Matching-lines-only first: it is the plain side-by-side view and the default.
        var cardMatching = MakeModeCard(
            IconNames.Filter,
            nameof(vm.ShowMatchingLinesOnlyText),
            nameof(vm.ShowMatchingLinesOnlyHint),
            nameof(vm.ShowMatchingLinesOnly),
            IconNames.Alert,
            nameof(vm.ShowMatchingLinesOnlyNote),
            out var radioMatching);

        var cardAll = MakeModeCard(
            IconNames.ViewSplitVertical,
            nameof(vm.ShowAllOriginalLinesText),
            nameof(vm.ShowAllOriginalLinesHint),
            nameof(vm.ShowAllOriginalLines),
            IconNames.Pencil,
            nameof(vm.ShowAllOriginalLinesNote),
            out _);

        var toggleAllowEdit = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.AllowEditOfOriginal)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggleAllowEdit, Se.Language.Main.AllowEditOfOriginalSubtitle);

        var allowEditRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                toggleAllowEdit,
                new TextBlock
                {
                    Text = Se.Language.Main.AllowEditOfOriginalSubtitle,
                    FontSize = 12.5,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

        // Reads off the selected mode, so the consequence of "allow edit" is spelled out for the
        // mode the user is actually about to pick.
        var allowEditHint = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ContentWidth - 60,
            Margin = new Thickness(60, 0, 0, 0), // under the label, past the switch
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.AllowEditHint)),
        };

        var allowEdit = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children = { allowEditRow, allowEditHint },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var modes = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children = { header, cardMatching, cardAll },
        };

        var sections = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                new Border { Child = modes, Padding = new Thickness(16, 16, 16, 14) },
                EditorChrome.MakeEditorSection(Se.Language.General.Options, allowEdit),
            },
        };

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, radioMatching); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Control MakeHeader(OpenOriginalMismatchViewModel vm)
    {
        var text = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ContentWidth - 40,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InfoText)),
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new Icon
                {
                    Value = IconNames.Information,
                    FontSize = 24,
                    Foreground = EditorChrome.AccentBrush,
                    VerticalAlignment = VerticalAlignment.Top,
                    IsHitTestVisible = false,
                },
                text,
            },
        };
    }

    /// <summary>
    /// One selectable mode: a bordered card holding the radio button, an icon for the mode, the
    /// explanation, and a footnote (locked time codes / what happens to the lines left out).
    /// </summary>
    private static Control MakeModeCard(
        string modeIcon,
        string titleProperty,
        string hintProperty,
        string isCheckedProperty,
        string noteIcon,
        string noteProperty,
        out RadioButton radioButton)
    {
        radioButton = new RadioButton
        {
            GroupName = "OpenOriginalMismatchMode",
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            [!ContentControl.ContentProperty] = new Binding(titleProperty),
            [!ToggleButton.IsCheckedProperty] = new Binding(isCheckedProperty) { Mode = BindingMode.TwoWay },
        };

        var titleRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new Icon
                {
                    Value = modeIcon,
                    FontSize = 18,
                    Foreground = EditorChrome.IconBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false,
                },
                radioButton,
            },
        };

        var hintText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ContentWidth - 60,
            Margin = new Thickness(28, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(hintProperty),
        };

        var noteText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ContentWidth - 90,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(noteProperty),
        };

        var noteRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(28, 2, 0, 0),
            Children =
            {
                new Icon
                {
                    Value = noteIcon,
                    FontSize = 14,
                    Foreground = EditorChrome.IconBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false,
                },
                noteText,
            },
        };

        // The picked mode's card gets the accent border, so the choice shows on the whole card.
        return new Border
        {
            BorderThickness = new Thickness(1),
            Background = EditorChrome.FieldBrush,
            [!Border.BorderBrushProperty] = new Binding(isCheckedProperty)
            {
                Converter = new FuncValueConverter<bool, IBrush>(isChecked => isChecked ? EditorChrome.AccentBrush : EditorChrome.FieldBorderBrush),
            },
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 10),
            Child = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Children = { titleRow, hintText, noteRow },
            },
        };
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
/// counterpart - see <see cref="OpenOriginalMismatchViewModel"/> and issue #13449.
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
        vm.Window = this;
        DataContext = vm;

        var header = MakeHeader(vm);

        var cardAll = MakeModeCard(
            IconNames.ViewSplitVertical,
            nameof(vm.ShowAllOriginalLinesText),
            nameof(vm.ShowAllOriginalLinesHint),
            nameof(vm.ShowAllOriginalLines),
            IconNames.LockClock,
            nameof(vm.ShowAllOriginalLinesNote),
            out var radioAll);

        var cardMatching = MakeModeCard(
            IconNames.Filter,
            nameof(vm.ShowMatchingLinesOnlyText),
            nameof(vm.ShowMatchingLinesOnlyHint),
            nameof(vm.ShowMatchingLinesOnly),
            IconNames.Alert,
            nameof(vm.ShowMatchingLinesOnlyNote),
            out _);

        var checkBoxAllowEdit = new CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = Se.Language.Main.AllowEditOfOriginalSubtitle,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.AllowEditOfOriginal)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(checkBoxAllowEdit, Se.Language.Main.AllowEditOfOriginalSubtitle);

        var allowEditRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(2, 4, 0, 0),
            Children =
            {
                new Icon
                {
                    Value = IconNames.Pencil,
                    FontSize = 18,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false,
                },
                checkBoxAllowEdit,
            },
        };

        // Reads off the selected mode, so the consequence of "allow edit" is spelled out for the
        // mode the user is actually about to pick.
        var allowEditHint = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ContentWidth - 28,
            Margin = new Thickness(28, 0, 0, 0),
            Opacity = 0.75,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.AllowEditHint)),
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        Content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                header,
                cardAll,
                cardMatching,
                allowEditRow,
                allowEditHint,
                buttonPanel,
            },
        };

        Activated += delegate { radioAll.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Control MakeHeader(OpenOriginalMismatchViewModel vm)
    {
        var text = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = ContentWidth - 40,
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
                    FontSize = 26,
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
                    FontSize = 20,
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
            Opacity = 0.8,
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
            Opacity = 0.9,
            Children =
            {
                new Icon
                {
                    Value = noteIcon,
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false,
                },
                noteText,
            },
        };

        return new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray, 0.35),
            CornerRadius = new CornerRadius(6),
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

using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.WebVttProperties;

/// <summary>
/// WebVTT properties in the editor look (see <see cref="FormatPropertiesChrome"/>): the cue
/// settings per alignment (with Reset in the section header) and the save options as switches.
/// </summary>
public class WebVttPropertiesWindow : Window
{
    public WebVttPropertiesWindow(WebVttPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, new WebVTT().Name);
        SizeToContent = SizeToContent.Height;
        Width = 700;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.File.WebVtt;

        // The cue settings are laid out like the numpad, so {\an7} (top-left) sits top-left etc.
        var alignmentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };

        var textBoxAn7 = MakeCueSetting(alignmentGrid, 0, 0, Se.Language.General.TopLeft, vm, nameof(vm.CueAn7));
        MakeCueSetting(alignmentGrid, 0, 1, Se.Language.General.TopCenter, vm, nameof(vm.CueAn8));
        MakeCueSetting(alignmentGrid, 0, 2, Se.Language.General.TopRight, vm, nameof(vm.CueAn9));
        MakeCueSetting(alignmentGrid, 1, 0, Se.Language.General.MiddleLeft, vm, nameof(vm.CueAn4));
        MakeCueSetting(alignmentGrid, 1, 1, Se.Language.General.MiddleCenter, vm, nameof(vm.CueAn5));
        MakeCueSetting(alignmentGrid, 1, 2, Se.Language.General.MiddleRight, vm, nameof(vm.CueAn6));
        MakeCueSetting(alignmentGrid, 2, 0, Se.Language.General.BottomLeft, vm, nameof(vm.CueAn1));
        MakeCueSetting(alignmentGrid, 2, 1, Se.Language.General.BottomCenter, vm, nameof(vm.CueAn2));
        MakeCueSetting(alignmentGrid, 2, 2, Se.Language.General.BottomRight, vm, nameof(vm.CueAn3));

        var cueSettings = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                FormatPropertiesChrome.Hint(l.CueSettingsHint, 640),
                alignmentGrid,
            }
        };
        var buttonReset = FormatPropertiesChrome.MakeAction("fa-solid fa-rotate-left", Se.Language.General.Reset, vm.ResetCueSettingsCommand);

        var options = FormatPropertiesChrome.MakeRows(
            EditorChrome.MakeSwitchRow(nameof(vm.UseXTimestampMap), l.UseXTimeStamp),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeLinesWithSameText), l.MergeLines),
            EditorChrome.MakeSwitchRow(nameof(vm.MergeStyleTags), l.MergeStyleTags));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(l.CueSettings, cueSettings, buttonReset, first: true),
            EditorChrome.MakeEditorSection(Se.Language.General.Options, options));

        UiUtil.FocusOnFirstActivation(this, textBoxAn7); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TextBox MakeCueSetting(Grid grid, int row, int column, string title, WebVttPropertiesViewModel vm, string propertyName)
    {
        var textBox = EditorChrome.AsTextField(UiUtil.MakeTextBox(180, vm, propertyName), title);
        textBox.Width = double.NaN;
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                EditorChrome.MakeDimLabel(title, new Avalonia.Thickness(0)),
                textBox,
            }
        };

        grid.Add(panel, row, column);

        return textBox;
    }
}

using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.WebVttProperties;

public class WebVttPropertiesWindow : Window
{
    public WebVttPropertiesWindow(WebVttPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, new WebVTT().Name);
        SizeToContent = SizeToContent.Height;
        Width = 700;
        CanResize = false;
        MinHeight = 200;
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
            ColumnSpacing = 10,
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

        var alignmentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                UiUtil.MakeLabel(l.CueSettingsHint),
                alignmentGrid,
            }
        };

        var checkBoxPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 5,
            Children =
            {
                UiUtil.MakeCheckBox(l.UseXTimeStamp, vm, nameof(vm.UseXTimestampMap)),
                UiUtil.MakeCheckBox(l.MergeLines, vm, nameof(vm.MergeLinesWithSameText)),
                UiUtil.MakeCheckBox(l.MergeStyleTags, vm, nameof(vm.MergeStyleTags)),
            }
        };

        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCueSettingsCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonReset, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeLabel(l.CueSettings).WithBold(), 0);
        grid.Add(UiUtil.MakeBorderForControl(alignmentPanel), 1);
        grid.Add(checkBoxPanel, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxAn7); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TextBox MakeCueSetting(Grid grid, int row, int column, string title, WebVttPropertiesViewModel vm, string propertyName)
    {
        var textBox = UiUtil.MakeTextBox(180, vm, propertyName);
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                UiUtil.MakeLabel(title),
                textBox,
            }
        };

        grid.Add(panel, row, column);

        return textBox;
    }
}

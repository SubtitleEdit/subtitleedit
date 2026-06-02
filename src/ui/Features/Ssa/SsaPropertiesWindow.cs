using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ssa;

public class SsaPropertiesWindow : Window
{
    public SsaPropertiesWindow(SsaPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        vm.Window = this;
        DataContext = vm;

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
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(MakeScriptView(vm), 0);
        grid.Add(MakeResolutionView(vm), 1);
        grid.Add(MakeOptionsView(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeScriptView(SsaPropertiesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Script).WithBold().WithMarginBottom(10);

        var labelTitle = UiUtil.MakeLabel(Se.Language.General.Title);
        var textBoxTitle = UiUtil.MakeTextBox(200, vm, nameof(vm.ScriptTitle));

        var labelOriginalScript = UiUtil.MakeLabel(Se.Language.Assa.OriginalScript);
        var textBoxOriginalScript = UiUtil.MakeTextBox(200, vm, nameof(vm.OriginalScript));

        var labelTranslation = UiUtil.MakeLabel(Se.Language.General.Translation);
        var textBoxTranslation = UiUtil.MakeTextBox(200, vm, nameof(vm.Translation));

        var labelEditing = UiUtil.MakeLabel(Se.Language.General.Editing);
        var textBoxEditing = UiUtil.MakeTextBox(200, vm, nameof(vm.Editing));

        var labelTiming = UiUtil.MakeLabel(Se.Language.General.Timing);
        var textBoxTiming = UiUtil.MakeTextBox(200, vm, nameof(vm.Timing));

        var labelSyncPoint = UiUtil.MakeLabel(Se.Language.General.Sync);
        var textBoxSyncPoint = UiUtil.MakeTextBox(200, vm, nameof(vm.SyncPoint));

        var labelUpdatedBy = UiUtil.MakeLabel(Se.Language.General.UpdatedBy);
        var textBoxUpdatedBy = UiUtil.MakeTextBox(200, vm, nameof(vm.UpdatedBy));

        var labelUpdateDetails = UiUtil.MakeLabel(Se.Language.General.UpdateDetails);
        var textBoxUpdateDetails = UiUtil.MakeTextBox(200, vm, nameof(vm.UpdateDetails));

        grid.Add(label, 0, 0, 1, 2);
        grid.Add(labelTitle, 1, 0);
        grid.Add(textBoxTitle, 1, 1);
        grid.Add(labelOriginalScript, 2, 0);
        grid.Add(textBoxOriginalScript, 2, 1);
        grid.Add(labelTranslation, 3, 0);
        grid.Add(textBoxTranslation, 3, 1);
        grid.Add(labelEditing, 4, 0);
        grid.Add(textBoxEditing, 4, 1);
        grid.Add(labelTiming, 5, 0);
        grid.Add(textBoxTiming, 5, 1);
        grid.Add(labelSyncPoint, 6, 0);
        grid.Add(textBoxSyncPoint, 6, 1);
        grid.Add(labelUpdatedBy, 7, 0);
        grid.Add(textBoxUpdatedBy, 7, 1);
        grid.Add(labelUpdateDetails, 8, 0);
        grid.Add(textBoxUpdateDetails, 8, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeResolutionView(SsaPropertiesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Resolution).WithBold().WithMarginBottom(10);

        var labelVideoResolution = UiUtil.MakeLabel(Se.Language.General.VideoResolution);
        var numericUpDownWidth = UiUtil.MakeNumericUpDownInt(0, 10000, 1920, 120, vm, nameof(vm.VideoWidth));
        var labelX = UiUtil.MakeLabel("x");
        var numericUpDownHeight = UiUtil.MakeNumericUpDownInt(0, 10000, 1080, 120, vm, nameof(vm.VideoHeight));
        var buttonBrowseResolution = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand);
        var buttonFromCurrentVideo = UiUtil.MakeButton(Se.Language.General.PickResolutionFromCurrentVideo, vm.GetResolutionFromCurrentVideoCommand);

        grid.Add(label, 0, 0, 1, 6);
        grid.Add(labelVideoResolution, 1, 0);
        grid.Add(numericUpDownWidth, 1, 1);
        grid.Add(labelX, 1, 2);
        grid.Add(numericUpDownHeight, 1, 3);
        grid.Add(buttonBrowseResolution, 1, 4);
        grid.Add(buttonFromCurrentVideo, 1, 5);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeOptionsView(SsaPropertiesViewModel vm)
    {
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Options).WithBold().WithMarginBottom(10);

        var labelCollisions = UiUtil.MakeLabel(Se.Language.Assa.Collisions);
        var comboBoxCollisions = UiUtil.MakeComboBox(vm.Collisions, vm, nameof(vm.SelectedCollision));

        var labelPlayDepth = UiUtil.MakeLabel(Se.Language.Assa.PlayDepth);
        var numericUpDownPlayDepth = UiUtil.MakeNumericUpDownInt(0, 32, 0, 120, vm, nameof(vm.PlayDepth));

        var labelTimer = UiUtil.MakeLabel(Se.Language.Assa.Timer);
        var textBoxTimer = UiUtil.MakeTextBox(120, vm, nameof(vm.Timer));

        grid.Add(label, 0, 0, 1, 2);
        grid.Add(labelCollisions, 1, 0);
        grid.Add(comboBoxCollisions, 1, 1);
        grid.Add(labelPlayDepth, 2, 0);
        grid.Add(numericUpDownPlayDepth, 2, 1);
        grid.Add(labelTimer, 3, 0);
        grid.Add(textBoxTimer, 3, 1);

        return UiUtil.MakeBorderForControl(grid);
    }
}

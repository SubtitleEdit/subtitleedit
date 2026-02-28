using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertAssaWindow : Window
{
    public BatchConvertAssaWindow(BatchConvertAssaViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.BatchConvertSettings;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var checkBoxOverwrite = new CheckBox
        {
            Content = Se.Language.Tools.BatchConvert.UseSourceStylesIfPossible,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.UseSourceStylesIfPossible)),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(checkBoxOverwrite, 0);
        grid.Add(MakeEditView(vm), 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Loaded += (_, _) =>
        {
            vm.Loaded();
            buttonOk.Focus();
        };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeEditView(BatchConvertAssaViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };
        
        var labelAssaSource = UiUtil.MakeLabel("ASSA source").WithBold();
        var contentBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };
        vm.TextBoxContainer = contentBorder;
        
        var buttonEditStyle = UiUtil.MakeButton(Se.Language.Tools.BatchConvert.EditStyles, vm.EditStylesCommand);
        var buttonEditProperties = UiUtil.MakeButton(Se.Language.Tools.BatchConvert.EditProperties, vm.EditPropertiesCommand);
        var buttonEditAttachment = UiUtil.MakeButton(Se.Language.Tools.BatchConvert.EditAttachments, vm.EditAttachmentCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonEditStyle, buttonEditProperties, buttonEditAttachment);

        grid.Add(labelAssaSource, 0);
        grid.Add(contentBorder, 1);
        grid.Add(panelButtons, 2);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }
}

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PickOllamaModelWindow : Window
{
    public PickOllamaModelWindow(PickOllamaModelViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        CanResize = true;
        Width = 500;
        Height = 500;
        MinWidth = 400;
        MinHeight = 200;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        var listBox = new ListBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Models)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedModel)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
        };

        // Only the OCR callers filter to vision-capable models, so only they get the "show all" escape
        // hatch; for the text callers the list is unfiltered already and the checkbox would be a no-op.
        var checkBoxShowAll = UiUtil.MakeCheckBox(Se.Language.Ocr.ShowAllOllamaModels, vm, nameof(vm.ShowAllModels));
        checkBoxShowAll.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowAllModelsVisible)));

        grid.Add(listBox, 0);
        grid.Add(checkBoxShowAll, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

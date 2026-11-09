using Avalonia;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var listBox = new ListBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = EditorChrome.FieldBrush,
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.General.Models,
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Models)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedModel)) { Mode = BindingMode.TwoWay },
            Width = double.NaN,
            Height = double.NaN,
        };

        // Only the OCR callers filter to vision-capable models, so only they get the "show all" escape
        // hatch; for the text callers the list is unfiltered already and the checkbox would be a no-op.
        var switchShowAll = EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.ShowAllModels), Se.Language.Ocr.ShowAllOllamaModels), Se.Language.Ocr.ShowAllOllamaModels);
        switchShowAll.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowAllModelsVisible)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = new Thickness(14, 12, 14, 14),
            RowSpacing = 8,
        };

        grid.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Models), 0);
        grid.Add(EditorChrome.MakeListFrame(listBox), 1);
        grid.Add(switchShowAll, 2);

        Content = EditorChrome.MakeRoot(grid, buttonBar, stretch: true);

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, listBox);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

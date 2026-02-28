using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickSpellCheckDictionary;

public class PickSpellCheckDictionaryWindow : Window
{
    public PickSpellCheckDictionaryWindow(PickSpellCheckDictionaryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.ChooseSpellCheckDictionary;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;

        var combo = new ComboBox
        {
            ItemsSource = vm.Dictionaries,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
            Margin = new Thickness(0, 10, 10, 2),
            [!ComboBox.SelectedValueProperty] = new Binding(nameof(vm.SelectedDictionary)),
        };

        var buttonDownload = UiUtil
            .MakeButtonBrowse(vm.BrowseDictionaryCommand)
            .WithLeftAlignment()
            .WithMargin(0, 10, 10, 2);

        var panelDownload = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                combo,
                buttonDownload
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelDownload, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

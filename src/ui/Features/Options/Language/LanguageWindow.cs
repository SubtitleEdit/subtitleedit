using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Language;

public class LanguageWindow : Window
{
    public LanguageWindow(LanguageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.ChooseLanguage.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.General.Language,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var combo = new ComboBox
        {
            ItemsSource = vm.Languages,
            SelectedValue = vm.SelectedLanguage,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(LanguageViewModel.SelectedLanguage)),
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(label, 0);
        grid.Add(combo, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        Loaded += (_, _) => vm.OnLoaded();
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

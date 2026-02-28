using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInResolutionPickerWindow : Window
{
    private readonly BurnInResolutionPickerViewModel _vm;

    public BurnInResolutionPickerWindow(BurnInResolutionPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Pick resolution";
        SizeToContent = SizeToContent.WidthAndHeight;
        MaxHeight = 900;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var listBoxResolutions = new ListBox
        {
            ItemsSource = vm.Resolutions,
            SelectedItem = vm.SelectedResolution,
            VerticalAlignment = VerticalAlignment.Center,
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedResolution)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            ItemTemplate = new FuncDataTemplate<ResolutionItem>((item, namescope) =>
            {
                if (item.IsSeparator)
                {
                    return new Separator
                    {
                        Margin = new Thickness(0, 2, 0, 2),
                        IsHitTestVisible = false, 
                    };
                }
                else
                {
                    // Selectable item
                    var border = new Border();
                    border.Bind(Border.BackgroundProperty, new Binding(nameof(ResolutionItem.BackgroundColor)));

                    var textBlock = new TextBlock();
                    textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ResolutionItem.DisplayName)));
                    textBlock.Bind(TextBlock.ForegroundProperty, new Binding(nameof(ResolutionItem.TextColor)));

                    border.Child = textBlock;

                    border.PointerPressed += (s, e) =>
                    {
                        vm.ResolutionItemClicked(item); 
                    };

                    return border;
                }
            }, true),
        };

        var style = new Style(x => x.OfType<ListBoxItem>());
        style.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0, 4, 0, 4)));
        style.Setters.Add(new Setter(ListBoxItem.MarginProperty, new Thickness(0)));
        listBoxResolutions.Styles.Add(style);

        listBoxResolutions.SelectionChanged += (s, e) =>
        {
            if (listBoxResolutions.SelectedItem is ResolutionItem selected && selected.IsSeparator)
            {
                listBoxResolutions.SelectedItem = null;
            }
        };

        listBoxResolutions.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                if (listBoxResolutions.SelectedItem is ResolutionItem selected && !selected.IsSeparator)
                {
                    vm.ResolutionItemClicked(selected);
                    e.Handled = true; 
                }
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(listBoxResolutions, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

public class PickSubtitleFormatWindow : Window
{
    public PickSubtitleFormatWindow(PickSubtitleFormatViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.PickSubtitleFormat;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = new TextBox
        {
            Watermark = Se.Language.General.SearchSubtitleFormats,
            Margin = new Thickness(5, 0, 0, 0),
            Width = 250,
        };
        textBoxSearch.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        textBoxSearch.TextChanged += (_, _) => vm.SearchTextChanged();
        
        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

        var listBoxFormats = new ListBox
        {
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxFormats.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(4, 2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(0)),
            }
        });
        listBoxFormats.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.SubtitleFormatNames)));
        listBoxFormats.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitleFormatName)) { Mode = BindingMode.TwoWay });
        listBoxFormats.SelectionChanged += (_, _) => vm.SelectedSubtitleFormatNameChanged();
        listBoxFormats.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);
        
        var listBoxBorder = UiUtil.MakeBorderForControl(listBoxFormats);
        
        var labelPreview = UiUtil.MakeLabel(Se.Language.General.Preview);
        labelPreview.Margin = new Thickness(0, 10, 0, 5);
        
        // Use the PreviewContainer from ViewModel (no border wrapper needed)
        vm.PreviewContainer.VerticalAlignment = VerticalAlignment.Stretch;
        vm.PreviewContainer.HorizontalAlignment = HorizontalAlignment.Stretch;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Search
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },  // ListBox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Preview label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },  // Preview
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelSearch, 0);
        grid.Add(listBoxBorder, 1);
        grid.Add(labelPreview, 2);
        grid.Add(UiUtil.MakeBorderForControl(vm.PreviewContainer), 3);
        grid.Add(buttonPanel, 4);

        Content = grid;

        Activated += delegate
        {
            textBoxSearch.Focus();
        };
        
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

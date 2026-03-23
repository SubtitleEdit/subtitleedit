using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public class BinaryOcrCharacterHistoryWindow : Window
{
    public BinaryOcrCharacterHistoryWindow(BinaryOcrCharacterHistoryViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Empty;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var listView = MakeListView(vm);
        var detailsView = MakeDetailsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk);

        grid.Add(listView, 0, 0);
        grid.Add(detailsView, 0, 1);
        grid.Add(buttonBar, 1, 0, 1, 2);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        };
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_, e) => { Title = vm.Title; };
    }

    private static Border MakeListView(BinaryOcrCharacterHistoryViewModel vm)
    {
        var listBoxCurrentItems = new ListBox
        {
            Margin = new Thickness(0, 5, 0, 0),
        };
        listBoxCurrentItems.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedHistoryItem)));
        listBoxCurrentItems.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.HistoryItems)));
        listBoxCurrentItems.SelectionChanged += vm.HistoryItemChanged;

        return UiUtil.MakeBorderForControl(listBoxCurrentItems);
    }

    private static Grid MakeDetailsView(BinaryOcrCharacterHistoryViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText));
        var image = new Image
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            MaxWidth = 400,
            MaxHeight = 400,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));

        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(5, 2, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel("Current Image").WithBold(),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.LineIndex)),
                panelCurrentImage,
                vm.TextBoxNew,
                checkBoxItalic,
                UiUtil.MakeButton(Se.Language.General.Update, vm.UpdateCommand).WithMarginTop(25).WithLeftAlignment(),
                UiUtil.MakeButton(Se.Language.General.UpdateAndClose, vm.UpdateAndCloseCommand).WithMarginTop(25).WithLeftAlignment(),
                UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand).WithMarginTop(5).WithLeftAlignment(),
                UiUtil.MakeLabel("Binary OCR uses pixel-by-pixel comparison.").WithMarginTop(25),
                UiUtil.MakeLabel("No manual drawing required.").WithMarginTop(5),
            },
        };

        grid.Add(panelCurrent, 0, 0);

        return grid;
    }
}

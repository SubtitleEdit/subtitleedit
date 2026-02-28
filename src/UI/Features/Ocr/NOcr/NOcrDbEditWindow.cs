using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrDbEditWindow : Window
{
    public NOcrDbEditWindow(NOcrDbEditViewModel vm)
    {
        Title = Se.Language.Ocr.EditNOcrDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;
        Width = 955;
        Height = 800;
        MinWidth = 950;
        MinHeight = 600;

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

        var charactersView = MakeCharacterControlsView(vm);
        var currentItemView = MakeCurrentItemControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(charactersView, 0, 0);
        grid.Add(currentItemView, 0, 1);
        grid.Add(buttonBar, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_,_) => Title = vm.Title;    
    }

    private static Border MakeCharacterControlsView(NOcrDbEditViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var labelCharacters = UiUtil.MakeLabel("Character(s)");
        var charactersComboBox = UiUtil.MakeComboBox(vm.Characters, vm, nameof(vm.SelectedCharacter));
        charactersComboBox.SelectionChanged += vm.CharactersChanged;
        var listBoxCurrentItems = new ListBox
        {
            Margin = new Thickness(0, 5, 0, 0),
        };
        listBoxCurrentItems.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedCurrentCharacterItem)));
        listBoxCurrentItems.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.CurrentCharacterItems)));
        listBoxCurrentItems.SelectionChanged += vm.CurrentCharacterItemsChanged;

        grid.Add(labelCharacters, 0, 0);
        grid.Add(charactersComboBox, 1, 0);
        grid.Add(listBoxCurrentItems, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeCurrentItemControlsView(NOcrDbEditViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
        };

        vm.TextBoxItem = UiUtil.MakeTextBox(100, vm, nameof(vm.ItemText));

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                vm.TextBoxItem,
                UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsItemItalic)),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ExpandInfo)),
                UiUtil.MakeButton("Update", vm.UpdateCommand).WithMarginTop(25).WithLeftAlignment(),
                UiUtil.MakeButton("Delete", vm.DeleteCommand).WithMarginTop(5).WithLeftAlignment(),
            },
        };

        vm.NOcrDrawingCanvas.SetStrokeWidth(1);
        var borderDrawingCanvas = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
            Child = vm.NOcrDrawingCanvas,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };

        var toggleButtonForeground = new ToggleButton
        {
            Content = Se.Language.General.Foreground,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsNewLinesForegroundActive))
            {
                Source = vm,
            },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0),
        };
        toggleButtonForeground.IsCheckedChanged += vm.DrawModeForegroundChanged;
        var toggleButtonBackground = new ToggleButton
        {
            Content = Se.Language.General.Background,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsNewLinesBackgroundActive))
            {
                Source = vm,
            },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        toggleButtonBackground.IsCheckedChanged += vm.DrawModeBackgroundChanged;

        var panelDrawMode = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children =
            {
                toggleButtonForeground,
                toggleButtonBackground,
            }
        };

        var panelZoom = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                UiUtil.MakeButton(vm.ZoomOutCommand, IconNames.Minus).WithFontSize(20),
                UiUtil.MakeButton(vm.ZoomInCommand, IconNames.Plus).WithFontSize(20),
                UiUtil.MakeLabel(string.Empty).WithMarginLeft(10).WithBindText(vm, nameof(vm.ZoomFactorInfo)),
                UiUtil.MakeLabel(Se.Language.Ocr.DrawMode).WithMarginLeft(10),
                panelDrawMode,
            }
        };

        var panelImage = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Children =
            {
                panelZoom,
                borderDrawingCanvas,
            }
        };

        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelImage, 0, 2);

        return UiUtil.MakeBorderForControl(grid);
    }
}

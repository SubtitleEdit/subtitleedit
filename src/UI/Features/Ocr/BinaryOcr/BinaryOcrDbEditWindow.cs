using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public class BinaryOcrDbEditWindow : Window
{
    public BinaryOcrDbEditWindow(BinaryOcrDbEditViewModel vm)
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
        KeyDown += (_, e) => vm.KeyDown(e);
        Loaded += (_, _) => Title = vm.Title;
    }

    private static Border MakeCharacterControlsView(BinaryOcrDbEditViewModel vm)
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

    private static Border MakeCurrentItemControlsView(BinaryOcrDbEditViewModel vm)
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

        var image = new Image
        {
            Margin = new Thickness(0, 5, 0, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.ItemBitmap)));
        var imageBorder = new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(5),
            Child = image,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
        };

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                imageBorder,
                vm.TextBoxItem,
                UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsItemItalic)),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ExpandInfo)),
                UiUtil.MakeButton(Se.Language.General.Update, vm.UpdateCommand).WithMarginTop(25).WithLeftAlignment(),
                UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand).WithMarginTop(5).WithLeftAlignment(),
            },
            Spacing = 5,
        };

        grid.Add(panelCurrent, 0, 0);

        return UiUtil.MakeBorderForControl(grid);
    }
}

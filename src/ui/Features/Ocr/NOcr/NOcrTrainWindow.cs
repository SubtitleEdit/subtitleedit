using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrTrainWindow : Window
{
    public NOcrTrainWindow(NOcrTrainViewModel vm)
    {
        Title = Se.Language.Ocr.TrainNOcrDatabase.TrimEnd('.');
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Width = 760;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // fonts + options
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // characters
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // merged letters
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // status
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        // Fonts (checkbox list, each rendered in its own typeface)
        var fontsListBox = new ListBox
        {
            Height = 260,
            ItemsSource = vm.Fonts,
            ItemTemplate = new FuncDataTemplate<NOcrTrainFontItem>((item, _) => new CheckBox
            {
                [!CheckBox.IsCheckedProperty] = new Binding(nameof(NOcrTrainFontItem.IsSelected)),
                Content = item.Name,
                FontFamily = new FontFamily(item.Name),
            }),
        };
        var fontsPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        fontsPanel.Children.Add(UiUtil.MakeLabel(Se.Language.General.Fonts));
        fontsPanel.Children.Add(UiUtil.MakeBorderForControlNoPadding(fontsListBox));
        var fontsGroup = fontsPanel;

        // Options
        var optionsPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 6 };
        optionsPanel.Children.Add(UiUtil.MakeLabel(Se.Language.General.Name));
        optionsPanel.Children.Add(UiUtil.MakeTextBox(220, vm, nameof(vm.DatabaseName)));
        optionsPanel.Children.Add(UiUtil.MakeLabel(Se.Language.General.FontSize));
        optionsPanel.Children.Add(UiUtil.MakeNumericUpDownInt(20, 100, 30, 120, vm, nameof(vm.FontSize)));
        optionsPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Ocr.NumberOfLineSegments));
        optionsPanel.Children.Add(UiUtil.MakeNumericUpDownInt(10, 500, 60, 120, vm, nameof(vm.NumberOfSegments)));
        optionsPanel.Children.Add(UiUtil.MakeCheckBox(Se.Language.Ocr.AlsoTrainBold, vm, nameof(vm.TrainBold)));
        optionsPanel.Children.Add(UiUtil.MakeCheckBox(Se.Language.Ocr.AlsoTrainItalic, vm, nameof(vm.TrainItalic)));
        var optionsOuterPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        optionsOuterPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Ocr.TrainingOptions));
        optionsOuterPanel.Children.Add(UiUtil.MakeBorderForControl(optionsPanel));
        var optionsGroup = optionsOuterPanel;

        // Characters to train + import from subtitle file
        var charactersPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 6 };
        var charactersTextBox = UiUtil.MakeTextBox(710, vm, nameof(vm.CharactersToTrain));
        charactersTextBox.TextWrapping = TextWrapping.Wrap;
        charactersTextBox.AcceptsReturn = false;
        charactersTextBox.Height = 60;
        charactersPanel.Children.Add(charactersTextBox);
        charactersPanel.Children.Add(UiUtil.MakeButton(Se.Language.Ocr.ImportCharactersFromSubtitleFile, vm.ImportCharactersFromFileCommand));
        var charactersOuterPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        charactersOuterPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Ocr.CharactersToTrain));
        charactersOuterPanel.Children.Add(charactersPanel);
        var charactersGroup = charactersOuterPanel;

        // Merged letter combinations
        var mergedTextBox = UiUtil.MakeTextBox(710, vm, nameof(vm.MergedLetters));
        var mergedPanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        mergedPanel.Children.Add(UiUtil.MakeLabel(Se.Language.Ocr.LetterCombinationsToTrain));
        mergedPanel.Children.Add(mergedTextBox);
        var mergedGroup = mergedPanel;

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText));

        var buttonTrain = UiUtil.MakeButton(Se.Language.Ocr.StartTraining, vm.StartOrAbortTrainingCommand);
        buttonTrain[!ContentControl.ContentProperty] = new Binding(nameof(vm.TrainButtonText));
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonTrain, buttonDone);

        grid.Add(fontsGroup, 0, 0);
        grid.Add(optionsGroup, 0, 1);
        grid.Add(charactersGroup, 1, 0, 1, 2);
        grid.Add(mergedGroup, 2, 0, 1, 2);
        grid.Add(labelStatus, 3, 0, 1, 2);
        grid.Add(buttonBar, 4, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            buttonTrain.Focus(); // hack to make OnKeyDown work
        };
        KeyDown += (_, e) => vm.KeyDown(e);
        Closing += (_, _) => vm.OnClosing();
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

public class FindWindow : Window
{
    public FindWindow(FindViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Find;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var textBoxFind = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200,
            Margin = new Thickness(0, 0, 0, 3),
            Watermark = Se.Language.Edit.Find.SearchTextWatermark,
            ItemsSource = vm.SearchHistory,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SearchText)),
            MinimumPrefixLength = 0,
        };
        textBoxFind.KeyDown += vm.FindTextBoxKeyDown;

        var checkBoxWholeWord = new CheckBox
        {
            Content = Se.Language.Edit.Find.WholeWord,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.WholeWord)) { Mode = BindingMode.TwoWay }
        };

        var radioButtonCaseSensitive = new RadioButton
        {
            Content = Se.Language.Edit.Find.CaseSensitive,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 3),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindTypeNormal)) { Mode = BindingMode.TwoWay }
        };

        var radioButtonCaseInsensitive = new RadioButton
        {
            Content = Se.Language.Edit.Find.CaseInsensitive,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 3),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindTypeCanseInsensitive)) { Mode = BindingMode.TwoWay }
        };

        var radioButtonRegularExpression = new RadioButton
        {
            Content = Se.Language.General.RegularExpression,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 3),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindTypeRegularExpression)) { Mode = BindingMode.TwoWay }
        };

        var panelFindTypes = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                radioButtonCaseSensitive,
                radioButtonCaseInsensitive,
                radioButtonRegularExpression
            }
        };

        var buttonFindPrevious = UiUtil.MakeButton(Se.Language.Edit.Find.FindPrevious, vm.FindPreviousCommand)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonFindNext = UiUtil.MakeButton(Se.Language.Edit.Find.FindNext, vm.FindNextCommand)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonCount = UiUtil.MakeButton(Se.Language.General.Count, vm.CountCommand)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);

        var textBlockCountResult = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CountResult)) { Mode = BindingMode.OneWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 50, 0),
            Children =
            {
                buttonFindPrevious,
                buttonFindNext,
                buttonCount,
                textBlockCountResult
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(textBoxFind, 0, 0);
        grid.Add(checkBoxWholeWord, 1, 0);
        grid.Add(panelFindTypes, 2, 0);
        grid.Add(panelButtons, 0, 1, 3, 1);

        Content = grid;

        Activated += delegate { textBoxFind.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }
}

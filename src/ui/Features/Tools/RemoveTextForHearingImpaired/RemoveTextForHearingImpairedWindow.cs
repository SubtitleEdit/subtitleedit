using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

public class RemoveTextForHearingImpairedWindow : Window
{
    private readonly RemoveTextForHearingImpairedViewModel _vm;

    public RemoveTextForHearingImpairedWindow(RemoveTextForHearingImpairedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.RemoveTextForHearingImpaired.Title;
        Width = 910;
        Height = 640;
        MinWidth = 800;
        MinHeight = 620;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(settingsView, 0, 0);
        grid.Add(fixesView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private StackPanel MakeSettingsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var removeBetweenView = MakeRemoveBetweenView(vm);
        var removeBeforeColonView = MakeBeforeColonView(vm);
        var lineUppercaseView = MakeUppercaseLineView(vm);
        var lineContainsView = MakeLineContainsView(vm);
        var musicSymbolsView = MakeMusicSymbolsContainsView(vm);
        var interjectionsView = MakeInterjectionsView(vm);

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                removeBetweenView,
                removeBeforeColonView,
                lineUppercaseView,
                lineContainsView,
                musicSymbolsView,
                interjectionsView,
            }
        };

        return panel;
    }

    private static Border MakeRemoveBetweenView(RemoveTextForHearingImpairedViewModel vm)
    {
        var labelTitle = UiUtil.MakeLabel(Se.Language.Tools.RemoveTextForHearingImpaired.RemoveTextBetween);

        var comboBoxBrackets = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.Brackets, vm, nameof(vm.IsRemoveBracketsOn));
        var comboBoxCurlyBrackets = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.CurlyBrackets, vm, nameof(vm.IsRemoveCurlyBracketsOn));
        var comboBoxParentheses = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.Parentheses, vm, nameof(vm.IsRemoveParenthesesOn));

        var checkBoxCustom = UiUtil.MakeCheckBox(string.Empty, vm, nameof(vm.IsRemoveCustomOn));
        var textBoxCustomStart = UiUtil.MakeTextBox(30, vm, nameof(vm.CustomStart));
        var labelAnd = UiUtil.MakeLabel(Se.Language.Tools.RemoveTextForHearingImpaired.And);
        var textBoxCustomEnd = UiUtil.MakeTextBox(30, vm, nameof(vm.CustomEnd));
        var panelCustom = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                checkBoxCustom,
                textBoxCustomStart,
                labelAnd,
                textBoxCustomEnd,
            }
        };
        var checkBoxOnlySeparateLine = UiUtil
            .MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.OnlySeparateLines, vm, nameof(vm.IsOnlySeparateLine))
            .WithMarginTop(5);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelTitle, 0, 0);
        grid.Add(comboBoxBrackets, 1, 0);
        grid.Add(comboBoxCurlyBrackets, 2, 0);
        grid.Add(comboBoxParentheses, 3, 0);
        grid.Add(panelCustom, 4, 0);
        grid.Add(checkBoxOnlySeparateLine, 5, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static Border MakeBeforeColonView(RemoveTextForHearingImpairedViewModel vm)
    {
        var comboBoxBrackets = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.RemoveTextBeforeColon, vm, nameof(vm.IsRemoveTextBeforeColonOn));
        var comboBoxUppercase = UiUtil
            .MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.OnlyIfTextIsUppercase, vm, nameof(vm.IsRemoveTextBeforeColonUppercaseOn))
            .WithMarginLeft(10);
        var comboBoxSeparateLine = UiUtil
            .MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.OnlyOnSeparateLine, vm, nameof(vm.IsRemoveTextBeforeColonSeparateLineOn))
            .WithMarginLeft(10);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(comboBoxBrackets, 0, 0);
        grid.Add(comboBoxUppercase, 1, 0);
        grid.Add(comboBoxSeparateLine, 2, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static Border MakeUppercaseLineView(RemoveTextForHearingImpairedViewModel vm)
    {
        var comboBoxLineUppercase = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.IfLineIsUppercase, vm, nameof(vm.IsRemoveTextUppercaseLineOn));
        return UiUtil.MakeBorderForControl(comboBoxLineUppercase).WithMarginBottom(5);
    }

    private static Border MakeLineContainsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var comboBoxLineContains = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.IfLineContains, vm, nameof(vm.IsRemoveTextContainsOn));
        var textBoxContains = UiUtil.MakeTextBox(120, vm, nameof(vm.TextContains)).WithMarginLeft(5);
        var panelContains = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                comboBoxLineContains,
                textBoxContains,
            }
        };

        return UiUtil.MakeBorderForControl(panelContains).WithMarginBottom(5);
    }

    private static Border MakeMusicSymbolsContainsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var comboBoxMusicSymbols =
            UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.IfLineOnlyContainsMusicSymbols, vm, nameof(vm.IsRemoveOnlyMusicSymbolsOn));
        return UiUtil.MakeBorderForControl(comboBoxMusicSymbols).WithMarginBottom(5);
    }

    private static Border MakeInterjectionsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var comboBoxInterjections = UiUtil.MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.RemoveInterjections, vm, nameof(vm.IsRemoveInterjectionsOn));
        var buttonEdit = UiUtil.MakeButton(Se.Language.General.Edit, vm.EditInterjectionsCommand);

        var panelInterjections = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                comboBoxInterjections,
                buttonEdit,
            }
        };

        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language);
        var comboBoxLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        var panelLanguege = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelLanguage,
                comboBoxLanguage,
            }
        };
        
        var checkBoxOnlySeparateLine = UiUtil
            .MakeCheckBox(Se.Language.Tools.RemoveTextForHearingImpaired.OnlySeparateLines, vm, nameof(vm.IsInterjectionsSeparateLineOn))
            .WithMarginTop(5);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelInterjections, 0, 0);
        grid.Add(panelLanguege, 1, 0);
        grid.Add(checkBoxOnlySeparateLine, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeFixesView(RemoveTextForHearingImpairedViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Fixes,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<RemoveItem>((item, _) =>
                        new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = new CheckBox
                            {
                                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(RemoveItem.Apply)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveItem.IndexDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveItem.Before)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(RemoveItem.After)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        //dataGridTracks.SelectionChanged += vm.DataGridTracksSelectionChanged;

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    private readonly AutoTranslateViewModel _vm;

    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {        
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AutoTranslate;
        Width = 950;
        MinWidth = 750;
        Height = 700;
        MinHeight = 400;

        DataContext = vm;
        vm.Window = this;
        _vm = vm;

        var topBarPoweredBy = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeTextBlock(Se.Language.General.PoweredBy),
                UiUtil.MakeLink("Google Translate V1", vm.GoToAutoTranslatorUriCommand, vm, nameof(vm.AutoTranslatorLinkText))
                    .WithMarginRight(UiUtil.WindowMarginWidth),
            }
        };

        var engineCombo = UiUtil.MakeComboBox(vm.AutoTranslators, vm, nameof(vm.SelectedAutoTranslator));

        engineCombo.OnPropertyChanged(e =>
        {
            if (e.Property == SelectingItemsControl.SelectedItemProperty)
            {
                vm.AutoTranslatorChanged(e.Sender);
            }
        });

        var sourceLangCombo = UiUtil.MakeComboBox(vm.SourceLanguages!, vm, nameof(vm.SelectedSourceLanguage));
        var targetLangCombo = UiUtil.MakeComboBox(vm.TargetLanguages!, vm, nameof(vm.SelectedTargetLanguage));
        var buttonTranslate = UiUtil.MakeButton(Se.Language.General.Translate, vm.TranslateCommand);
        buttonTranslate.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsTranslateEnabled)));

        var topBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeTextBlock(Se.Language.General.Engine),
                engineCombo,
                UiUtil.MakeSeparatorForHorizontal(vm),
                UiUtil.MakeTextBlock(Se.Language.General.From),
                sourceLangCombo,
                UiUtil.MakeTextBlock(Se.Language.General.To),
                targetLangCombo,
                buttonTranslate,
            }
        };

        var contextMenu = new MenuFlyout
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.TranslateRow,
                    Command = vm.TranslateRowCommand,
                },
            }
        };

        var dataGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            CanUserSortColumns = false,
            ContextFlyout = contextMenu,
            DataContext = vm,
            IsReadOnly = true,
            AutoGenerateColumns = false,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(TranslateRow.Number)),
                    Width = new DataGridLength(50),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    Binding = new Binding(nameof(TranslateRow.Show)),
                    Width = new DataGridLength(100),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    Binding = new Binding(nameof(TranslateRow.Duration)),
                    Width = new DataGridLength(80),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    Binding = new Binding(nameof(TranslateRow.Text)),
                    Width = new DataGridLength(200, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Translation,
                    Binding = new Binding(nameof(TranslateRow.TranslatedText)),
                    Width = new DataGridLength(200, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                }
            }
        };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Rows)));
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedTranslateRow)));
        vm.RowGrid = dataGrid;

        var dataGridBorder = UiUtil.MakeBorderForControlNoPadding(dataGrid);

        StackPanel settingsBar = UiUtil.MakeControlBarLeft(

            UiUtil.MakeTextBlock(Se.Language.General.Id, vm, null, nameof(vm.ApiIdIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiIdText), nameof(vm.ApiIdIsVisible)).WithMarginRight(15),

            UiUtil.MakeTextBlock(Se.Language.General.ApiSecret, vm, null, nameof(vm.ApiSecretIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiSecretText), nameof(vm.ApiSecretIsVisible)).WithMarginRight(15),

            UiUtil.MakeTextBlock(Se.Language.General.ApiKey, vm, null, nameof(vm.ApiKeyIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ApiKeyText), nameof(vm.ApiKeyIsVisible)).WithMarginRight(15),

            UiUtil.MakeTextBlock(Se.Language.General.Url, vm, null, nameof(vm.ApiUrlIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(200, vm, nameof(vm.ApiUrlText), nameof(vm.ApiUrlIsVisible)).WithMarginRight(15),

            UiUtil.MakeTextBlock(Se.Language.General.Model, vm, null, nameof(vm.ModelIsVisible)).WithMarginRight(5),
            UiUtil.MakeTextBox(150, vm, nameof(vm.ModelText), nameof(vm.ModelIsVisible)),
            UiUtil.MakeButtonBrowse(vm.BrowseModelCommand, nameof(vm.ModelBrowseIsVisible)).WithMarginLeft(5)
        );

        var settingsLink = UiUtil.MakeLink(Se.Language.General.Settings, vm.OpenSettingsCommand).WithMarginRight(10);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsTranslateEnabled)));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var bottomGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Margin = new Thickness(10, 0, 0, 0),
            Width = double.NaN,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    },
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 8.0)
                    },
                },
            },
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsProgressEnabled)));
        bottomGrid.Children.Add(progressSlider);
        Grid.SetRow(progressSlider, 0);
        var bottomBar = UiUtil.MakeButtonBar(settingsLink, buttonOk, buttonCancel);
        bottomGrid.Children.Add(bottomBar);
        Grid.SetRow(bottomBar, 1);


        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto,Auto"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
        };

        var row = 0;
        grid.Children.Add(topBarPoweredBy);
        Grid.SetRow(topBarPoweredBy, row++);

        grid.Children.Add(topBar);
        Grid.SetRow(topBar, row++);

        grid.Children.Add(dataGridBorder);
        Grid.SetRow(dataGridBorder, row++);

        grid.Children.Add(settingsBar);
        Grid.SetRow(settingsBar, row++);

        grid.Children.Add(bottomGrid);
        Grid.SetRow(bottomGrid, row++);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);  
        _vm.KeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is AutoTranslateViewModel vm)
        {
            vm.SaveSettings();
        }
    }
}

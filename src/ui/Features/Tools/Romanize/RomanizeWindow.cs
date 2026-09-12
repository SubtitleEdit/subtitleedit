using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.Romanize;

public class RomanizeWindow : Window
{
    public RomanizeWindow(RomanizeViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.Romanize.Title;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        var languages = MakeLanguagesView(vm);
        var romanized = UiUtil.MakeBorderForControlNoPadding(MakeRomanizedView(vm));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(languages, 0);
        grid.Add(romanized, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;

        vm.RomanizeAllCommand.Execute(null);
    }

    private static StackPanel MakeLanguagesView(RomanizeViewModel vm)
    {
        return new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 8),
            Children =
            {
                new TextBlock
                {
                    Text = Se.Language.Tools.Romanize.TitleSettings,
                },
                new WrapPanel
                {
                    ItemSpacing = 8,
                    Orientation = Orientation.Horizontal,
                    Children = 
                    {
                        new CheckBox
                        {
                            Content = Se.Language.General.MergeLines,
                            IsChecked = vm.SubtitleItemsMerged ?? default,

                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(RomanizeViewModel.SubtitleItemsMerged))
                            {
                                Mode = BindingMode.OneWayToSource
                            },
                        },
                        new ComboBox
                        {
                            Background = Brushes.Transparent,
                            BorderThickness = new Thickness(0),
                            Margin = new Thickness(0, 3.50, 0, 0),
                            ItemsSource = Enum.GetValues<RomanizedLinePositions>(),
                            SelectedValue = vm.SubtitleItemsRomanizedLinePosition ?? default,
                            SelectionBoxItemTemplate = new FuncDataTemplate<RomanizedLinePositions>((item, nameScope) => new TextBlock
                            {
                                Text = string.Format("{0}: {1}", Se.Language.General.Position, item)
                            }),

                            [!ComboBox.SelectedItemProperty] = new Binding(nameof(RomanizeViewModel.SubtitleItemsRomanizedLinePosition))
                            {
                                Mode = BindingMode.OneWayToSource,
                            },
                        }
                    }
                },
                new TextBlock
                {
                    Text = Se.Language.Tools.Romanize.TitleLanguages,
                },
                new WrapPanel
                {
                    ItemSpacing = 8,
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        MakeLanguagesCheckBox(Se.Language.General.All, null, new Binding(nameof(RomanizeViewModel.Romanize))),
                        MakeLanguagesCheckBox(Se.Language.Tools.Romanize.GeezOriginal, Se.Language.Tools.Romanize.GeezRomanized, new Binding(nameof(RomanizeViewModel.RomanizeGeez))),
                        MakeLanguagesCheckBox(Se.Language.Tools.Romanize.CyrillicOriginal, Se.Language.Tools.Romanize.CyrillicRomanized, new Binding(nameof(RomanizeViewModel.RomanizeCyrillic))),
                        MakeLanguagesCheckBox(Se.Language.Tools.Romanize.DevanagariOriginal, Se.Language.Tools.Romanize.DevanagariRomanized, new Binding(nameof(RomanizeViewModel.RomanizeDevanagari))),
                        MakeLanguagesCheckBox(Se.Language.Tools.Romanize.GreekOriginal, Se.Language.Tools.Romanize.GreekRomanized, new Binding(nameof(RomanizeViewModel.RomanizeGreek))),
                        MakeLanguagesCheckBox(Se.Language.Tools.Romanize.HangulOriginal, Se.Language.Tools.Romanize.HangulRomanized, new Binding(nameof(RomanizeViewModel.RomanizeHangul))),
                        MakeLanguagesCheckBox(Se.Language.Tools.Romanize.KanaOriginal, Se.Language.Tools.Romanize.KanaRomanized, new Binding(nameof(RomanizeViewModel.RomanizeKana))),
                    },
                }
            }
        };
    }
    private static TableView MakeRomanizedView(RomanizeViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);

        dataGrid.SelectionMode = SelectionMode.Single;
        dataGrid.CanUserResizeColumns = true;
        dataGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
        dataGrid.VerticalAlignment = VerticalAlignment.Center;
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.SubtitleItems;
        dataGrid.Columns =
        [
            new SeTableViewColumn
            {
                Binding = new Binding(nameof(RomanizeSubtitleLineItem.LineNumber)),
                CellTheme = UiUtil.TableViewCellTheme,
                Header = Se.Language.General.NumberSymbol,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                CellTheme = UiUtil.TableViewCellTheme,
                CellTemplate = new FuncDataTemplate<RomanizeSubtitleLineItem>((item, _) => 
                {
                    var textbox = new TextBox
                    {
                        ContextMenu = null,
                        IsReadOnly = true,
                        IsEnabled = false,
                        VerticalContentAlignment = VerticalAlignment.Center,

                        [!TextBox.TextProperty] = new Binding(nameof(RomanizeSubtitleLineItem.TextOriginal)),
                    };

                    textbox.Resources["TextControlBackgroundDisabled"] = 
                    textbox.Resources["TextControlBorderBrushDisabled"] = Brushes.Transparent;

                    return textbox;
                }),
                Header = Se.Language.General.OriginalText,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                CellTheme = UiUtil.TableViewCellTheme,
                CellTemplate = new FuncDataTemplate<RomanizeSubtitleLineItem>((item, _) =>
                {
                    var checkbox = new CheckBox
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,

                        [!CheckBox.IsCheckedProperty] = new Binding(nameof(RomanizeSubtitleLineItem.Merged)),
                    };

                    checkbox.Click += (obj, args) =>
                    {
                        if (item.LineNumber.HasValue)
                            vm.RomanizeSingleCommand.Execute(item.LineNumber.Value - 1);
                    };

                    return checkbox;
                }),
                Header = Se.Language.General.MergeLines,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(100),
            },
            new SeTableViewColumn
            {
                CellTheme = UiUtil.TableViewCellTheme,
                CellTemplate = new FuncDataTemplate<RomanizeSubtitleLineItem>((item, _) =>
                {
                    var combobox = new ComboBox
                    {
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 1, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center,
                        ItemsSource = Enum.GetValues<RomanizedLinePositions>(),

                        [!ComboBox.SelectedItemProperty] = new Binding(nameof(RomanizeSubtitleLineItem.RomanizedLinePosition)),
                    };

                    combobox.SelectionChanged += (obj, args) =>
                    {
                        if (args.AddedItems?.Count == 1 &&
                            args.RemovedItems?.Count == 1 &&
                            item.LineNumber.HasValue &&
                            Enum.TryParse(args.AddedItems[0]!.ToString(), out RomanizedLinePositions _placement))
                        {
                            int number = item.LineNumber.Value - 1;

                            vm.SubtitleItems[number].RomanizedLinePosition = _placement;
                            vm.RomanizeSingleCommand.Execute(number);
                        }
                    };

                    return combobox;
                }),
                Header = Se.Language.General.Position,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(120),
            },
            new SeTableViewColumn
            {
                CellTheme = UiUtil.TableViewCellTheme,
                CellTemplate = new FuncDataTemplate<RomanizeSubtitleLineItem>((item, _) => new TextBox
                {
                    Background = Brushes.Transparent, 
                    BorderBrush = Brushes.Transparent, 

                    [!TextBox.TextProperty] = new Binding(nameof(RomanizeSubtitleLineItem.TextOutput)),
                }),
                Header = Se.Language.General.Romanized,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(1, GridUnitType.Star),
            }
        ];

        return dataGrid;
    }

    private static CheckBox MakeLanguagesCheckBox(string content, string? content2, Binding ischeckedproperty)
    {
        return new CheckBox
        {
            Margin = content2 is null ? new Thickness(0, 0, 16, 0) : default,
            Content = new TextBlock
            {
                Inlines = content2 is null ? [ new Run(content) { FontWeight = FontWeight.ExtraBold, }, ] : 
                [
                    new Run(string.Format("{0} ", content)) { FontWeight = FontWeight.Bold, },
                    new Run(string.Format("({0})", content2)) { FontWeight = FontWeight.Normal },
                ],
            },

            [!CheckBox.IsCheckedProperty] = ischeckedproperty
        };
    }
}

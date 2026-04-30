using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaSingleStyleWindow : Window
{
    public AssaSingleStyleWindow(AssaSingleStyleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.BatchConvertSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeSelectedStyleView(vm), 0);
        grid.Add(MakePreviewView(vm), 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Loaded += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeSelectedStyleView(AssaSingleStyleViewModel vm)
    {
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel().WithBold(); //.WithBindText(vm, nameof(vm.CurrentTitle));

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(200, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Name));
        var panelName = UiUtil.MakeHorizontalPanel(labelName, textBoxName).WithMarginBottom(10);

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = MakeSearchableFontNameBox(vm);
        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontSize));
        numericUpDownFontSize.Increment = 1;
        var panelFont = UiUtil.MakeHorizontalPanel(labelFontName, comboBoxFontName, labelFontSize, numericUpDownFontSize);

        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Bold));
        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Italic));
        var checkBoxUnderline = UiUtil.MakeCheckBox(Se.Language.General.Underline, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Underline));
        var checkBoxStrikeout = UiUtil.MakeCheckBox(Se.Language.General.Strikeout, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Strikeout));
        var panelFontStyle = UiUtil.MakeHorizontalPanel(checkBoxBold, checkBoxItalic, checkBoxUnderline, checkBoxStrikeout).WithMarginBottom(10);

        var labelScaleX = UiUtil.MakeLabel("Scale X").WithMinWidth(60);
        var numericUpDownScaleX = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ScaleX));
        numericUpDownScaleX.Increment = 1;
        var labelScaleY = UiUtil.MakeLabel("Scale Y").WithMinWidth(60);
        var numericUpDownScaleY = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ScaleY));
        numericUpDownScaleY.Increment = 1;
        var panelTransform1 = UiUtil.MakeHorizontalPanel(labelScaleX, numericUpDownScaleX, labelScaleY, numericUpDownScaleY);

        var labelSpacing = UiUtil.MakeLabel("Spacing").WithMinWidth(60);
        var numericUpDownSpacing = UiUtil.MakeNumericUpDownOneDecimal(-100, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Spacing));
        numericUpDownSpacing.Increment = 1;
        var labelAngle = UiUtil.MakeLabel("Angle").WithMinWidth(60);
        var numericUpDownAngle = UiUtil.MakeNumericUpDownOneDecimal(-360, 360, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Angle));
        numericUpDownAngle.Increment = 1;
        var panelTransform2 = UiUtil.MakeHorizontalPanel(labelSpacing, numericUpDownSpacing, labelAngle, numericUpDownAngle).WithMarginBottom(10);

        var labelColorPrimary = UiUtil.MakeLabel(Se.Language.Assa.Primary);
        var colorPickerPrimary = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorPrimary));
        var labelColorOutline = UiUtil.MakeLabel(Se.Language.General.Outline);
        var colorPickerOutline = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorOutline));
        var labelColorShadow = UiUtil.MakeLabel(Se.Language.General.Shadow);
        var colorPickerShadow = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorShadow));
        var labelColorSecondary = UiUtil.MakeLabel(Se.Language.Assa.Secondary);
        var colorPickerSecondary = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorSecondary));
        var panelColors = UiUtil.MakeHorizontalPanel(
            labelColorPrimary,
            colorPickerPrimary,
            labelColorOutline,
            colorPickerOutline,
            labelColorShadow,
            colorPickerShadow,
            labelColorSecondary,
            colorPickerSecondary
        ).WithMarginBottom(10);

        var alignmentView = MakeAlignmentView(vm);
        var marginView = MakeMarginView(vm);
        var borderView = MakeBorderView(vm);
        var panelMore = UiUtil.MakeHorizontalPanel(alignmentView, marginView, borderView);

        grid.Add(label, 0);
        grid.Add(panelName, 1);
        grid.Add(panelFont, 2);
        grid.Add(panelFontStyle, 3);
        grid.Add(panelTransform1, 4);
        grid.Add(panelTransform2, 5);
        grid.Add(panelColors, 6);
        grid.Add(panelMore, 7);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static AutoCompleteBox MakeSearchableFontNameBox(AssaSingleStyleViewModel vm)
    {
        const double fontBoxWidth = 240;
        var box = new AutoCompleteBox
        {
            DataContext = vm,
            Width = fontBoxWidth,
            MinWidth = fontBoxWidth,
            MaxWidth = fontBoxWidth,
            ItemsSource = vm.Fonts,
            MinimumPrefixLength = 0,
            PlaceholderText = Se.Language.General.SearchFontNames,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontName)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<string>((fontName, _) =>
            {
                var item = MakeFontSuggestionTextBlock(fontName, fontBoxWidth);
                item.PointerEntered += (_, _) => vm.PreviewFontName(fontName);
                return item;
            }, true),
        };
        box.DropDownClosed += (_, _) => vm.ClearPreviewFontName();
        KeepFontDropDownOpen(box, FontMatches);
        return box;
    }

    private static Control MakeFontSuggestionTextBlock(string? fontName, double width)
    {
        var border = new Border
        {
            Width = width - 8,
            Height = 18,
            Background = Brushes.Transparent,
            Margin = new Thickness(-5, -4, 0, -4),
            Padding = new Thickness(2, 0, 0, 0),
            Child = new TextBlock
            {
                Text = fontName,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            },
        };
        border.AddHandler(InputElement.PointerWheelChangedEvent, (s, e) => UiUtil.ScrollDropDownItemOnPointerWheel(e.Source ?? s, e));
        return border;
    }

    private static bool FontMatches(string? searchText, string? fontName)
    {
        return string.IsNullOrEmpty(searchText) ||
               fontName?.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) == true;
    }

    private static void KeepFontDropDownOpen(AutoCompleteBox box, Func<string?, string?, bool> filter)
    {
        var userActivated = false;
        var showAllForInitialOpen = false;
        var initialOpenText = string.Empty;
        var skipNextPointerRelease = false;
        var keepOpenForTextBoxClick = false;

        box.TextFilter = (searchText, itemText) =>
        {
            if (showAllForInitialOpen && string.Equals(searchText, initialOpenText, StringComparison.CurrentCulture))
            {
                return true;
            }

            return filter(searchText, itemText);
        };

        void OpenDropDown()
        {
            if (!userActivated)
            {
                return;
            }

            UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            Dispatcher.UIThread.Post(() =>
            {
                box.IsDropDownOpen = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            }, DispatcherPriority.Background);
            DispatcherTimer.RunOnce(() =>
            {
                box.IsDropDownOpen = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            }, TimeSpan.FromMilliseconds(10));
            DispatcherTimer.RunOnce(() =>
            {
                box.IsDropDownOpen = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            }, TimeSpan.FromMilliseconds(50));
        }

        bool IsPointerInsideFontSearchBox(PointerEventArgs e)
        {
            return e.Source is Visual visual &&
                   (ReferenceEquals(visual, box) || visual.GetVisualAncestors().Contains(box));
        }

        box.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (skipNextPointerRelease)
            {
                skipNextPointerRelease = false;
                return;
            }

            userActivated = true;
            showAllForInitialOpen = true;
            initialOpenText = box.Text ?? string.Empty;
            if (keepOpenForTextBoxClick)
            {
                e.Handled = true;
                keepOpenForTextBoxClick = false;
            }

            OpenDropDown();
        }, RoutingStrategies.Tunnel, true);
        box.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            if (box.IsDropDownOpen && e.ClickCount == 1 && IsPointerInsideFontSearchBox(e))
            {
                keepOpenForTextBoxClick = true;
                e.Handled = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
                return;
            }

            if (e.ClickCount > 1)
            {
                skipNextPointerRelease = true;
                userActivated = true;
                showAllForInitialOpen = true;
                initialOpenText = box.Text ?? string.Empty;
                Dispatcher.UIThread.Post(() =>
                {
                    box.GetVisualDescendants().OfType<TextBox>().FirstOrDefault()?.SelectAll();
                    OpenDropDown();
                });
            }
        }, RoutingStrategies.Tunnel, true);
        box.DropDownOpening += (_, _) => UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
        box.DropDownClosing += (_, e) =>
        {
            if (!keepOpenForTextBoxClick)
            {
                return;
            }

            e.Cancel = true;
            keepOpenForTextBoxClick = false;
            UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
        };
        box.DropDownClosed += (_, _) =>
        {
            keepOpenForTextBoxClick = false;
        };
        box.TextChanged += (_, _) =>
        {
            if (!string.Equals(box.Text ?? string.Empty, initialOpenText, StringComparison.CurrentCulture))
            {
                showAllForInitialOpen = false;
            }

            OpenDropDown();
        };
    }
    private static Border MakeAlignmentView(AssaSingleStyleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Alignment);

        grid.Add(label, 0, 0, 1, 3);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn7), "align"), 1, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn8), "align"), 1, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn9), "align"), 1, 2);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn4), "align"), 2, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn5), "align"), 2, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn6), "align"), 2, 2);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn1), "align"), 3, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn2), "align"), 3, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn3), "align"), 3, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeMarginView(AssaSingleStyleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Margin);
        grid.Add(label, 0);

        var labelMarginLeft = UiUtil.MakeLabel(Se.Language.General.Left);
        var numericUpDownMarginLeft = UiUtil.MakeNumericUpDownInt(0, 1000, 10, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginLeft));
        grid.Add(labelMarginLeft, 1, 0);
        grid.Add(numericUpDownMarginLeft, 1, 1);

        var labelMarginRight = UiUtil.MakeLabel(Se.Language.General.Right);
        var numericUpDownMarginRight = UiUtil.MakeNumericUpDownInt(0, 1000, 10, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginRight));
        grid.Add(labelMarginRight, 2, 0);
        grid.Add(numericUpDownMarginRight, 2, 1);

        var labelMarginVertical = UiUtil.MakeLabel(Se.Language.General.Vertical);
        var numericUpDownMarginVertical = UiUtil.MakeNumericUpDownInt(0, 1000, 10, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginVertical));
        grid.Add(labelMarginVertical, 3, 0);
        grid.Add(numericUpDownMarginVertical, 3, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeBorderView(AssaSingleStyleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.BorderStyle);
        grid.Add(label, 1, 0);

        var comboBoxBorderType = UiUtil.MakeComboBox(vm.BorderTypes, vm, nameof(vm.SelectedBorderType));
        comboBoxBorderType.SelectionChanged += vm.BorderTypeChanged;
        grid.Add(comboBoxBorderType, 2, 0, 1, 2);

        var labelOutlineWidth = UiUtil.MakeLabel(Se.Language.General.OutlineWidth);
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.OutlineWidth));
        numericUpDownOutlineWidth.Increment = 0.5m;
        grid.Add(labelOutlineWidth, 3, 0);
        grid.Add(numericUpDownOutlineWidth, 3, 1);

        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.ShadowWidth);
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ShadowWidth));
        numericUpDownShadowWidth.Increment = 0.5m;
        grid.Add(labelShadowWidth, 4, 0);
        grid.Add(numericUpDownShadowWidth, 4, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakePreviewView(AssaSingleStyleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Preview).WithBold();

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            Width = 360,
            Height = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        grid.Add(label, 0);
        grid.Add(image, 1);

        return UiUtil.MakeBorderForControl(grid);
    }
}

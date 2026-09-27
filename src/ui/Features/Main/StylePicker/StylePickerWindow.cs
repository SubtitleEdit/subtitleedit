using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Main.StylePicker;

public class StylePickerWindow : Window
{
    private const string PreviewText = "AaBbCc 123";

    public StylePickerWindow(StylePickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.StylePickerTitle;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Width = 820;

        vm.Window = this;
        DataContext = vm;

        var labelSelectionInfo = new TextBlock { Opacity = 0.8, VerticalAlignment = VerticalAlignment.Center };
        labelSelectionInfo.Bind(TextBlock.TextProperty, new Binding(nameof(vm.SelectionInfo)));
        var labelCurrentStyle = new TextBlock
        {
            Opacity = 0.8,
            Margin = new Thickness(16, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelCurrentStyle.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CurrentStyleInfo)));
        var panelInfo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelSelectionInfo, labelCurrentStyle },
        };

        var textBoxFilter = new TextBox
        {
            PlaceholderText = Se.Language.General.StylePickerFilterHint,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        textBoxFilter.Bind(TextBox.TextProperty, new Binding(nameof(vm.FilterText)) { Mode = BindingMode.TwoWay });

        var listBox = new ListBox
        {
            ItemsSource = vm.VisibleItems,
            ItemTemplate = new FuncDataTemplate<StylePickerItem>((_, _) => MakeStyleRow(), true),
            MaxHeight = 460,
            MinHeight = 200,
            Focusable = false,
        };
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedItem)) { Mode = BindingMode.TwoWay });
        listBox.SelectionChanged += (_, _) =>
        {
            if (listBox.SelectedItem != null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        };

        // Single click highlights (shows the details), double click sets - so the details can be
        // read with the mouse too.
        listBox.DoubleTapped += (_, e) =>
        {
            if (e.Source is Visual visual && visual.FindAncestorOfType<ListBoxItem>(true)?.DataContext is StylePickerItem item)
            {
                vm.Pick(item);
            }
        };

        var detailsPanel = MakeDetailsPanel(vm);

        var gridContent = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(350) },
            },
        };
        gridContent.Add(listBox, 0, 0);
        gridContent.Add(detailsPanel, 0, 1);

        var labelKeysHint = new TextBlock
        {
            Text = Se.Language.General.StylePickerKeysHint,
            TextWrapping = TextWrapping.Wrap,
            FontSize = UiUtil.ScaledFontSize(11),
            Opacity = 0.7,
        };

        var buttonManageStyles = UiUtil.MakeButton(Se.Language.General.StylePickerManageStyles);
        buttonManageStyles.Click += (_, _) => vm.ShowStylesManager();
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonManageStyles, Se.Language.General.StylePickerManageStylesHint);
        }

        var buttonSet = UiUtil.MakeButton(Se.Language.General.StylePickerSetStyle);
        buttonSet.Click += (_, _) => vm.Pick(vm.SelectedItem);
        buttonSet.Bind(IsEnabledProperty, new Binding(nameof(vm.SelectedItem)) { Converter = ObjectConverters.IsNotNull });
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel);
        buttonCancel.Click += (_, _) => vm.Cancel();
        var panelButtons = UiUtil.MakeButtonBar(buttonManageStyles, buttonSet, buttonCancel);

        var grid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(panelInfo, 0);
        grid.Add(textBoxFilter, 1);
        grid.Add(gridContent, 2);
        grid.Add(labelKeysHint, 3);
        grid.Add(panelButtons, 4);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxFilter);

        // Tunnel, so number keys, arrows and Enter are handled before the filter box types them.
        AddHandler(KeyDownEvent, (_, e) => vm.OnKeyDown(e), RoutingStrategies.Tunnel);

        // Keep a long style list reachable on small screens - the list scrolls.
        Opened += (_, _) =>
        {
            var workingArea = Screens.ScreenFromWindow(this)?.WorkingArea;
            if (workingArea != null && RenderScaling > 0)
            {
                listBox.MaxHeight = Math.Min(listBox.MaxHeight, workingArea.Value.Height / RenderScaling * 0.55);
            }
        };
    }

    private static Border MakePreviewChip(double width, double height, double fontSize, string text)
    {
        var labelPreview = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelPreview.Bind(TextBlock.FontFamilyProperty, new Binding(nameof(StylePickerItem.PreviewFontFamily)));
        labelPreview.Bind(TextBlock.FontWeightProperty, new Binding(nameof(StylePickerItem.PreviewFontWeight)));
        labelPreview.Bind(TextBlock.FontStyleProperty, new Binding(nameof(StylePickerItem.PreviewFontStyle)));
        labelPreview.Bind(TextBlock.ForegroundProperty, new Binding(nameof(StylePickerItem.PreviewForeground)));

        var chip = new Border
        {
            Width = width,
            Height = height,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(2),
            Padding = new Thickness(4, 0),
            Child = labelPreview,
            VerticalAlignment = VerticalAlignment.Center,
        };
        chip.Bind(Border.BackgroundProperty, new Binding(nameof(StylePickerItem.PreviewBackground)));
        chip.Bind(Border.BorderBrushProperty, new Binding(nameof(StylePickerItem.PreviewBorderBrush)));
        return chip;
    }

    private static Control MakeStyleRow()
    {
        var labelNumber = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            Width = 24,
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelNumber.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.NumberText)));
        labelNumber.Bind(OpacityProperty, new Binding(nameof(StylePickerItem.IsNumberKeyActive))
        {
            Converter = new FuncValueConverter<bool, double>(isActive => isActive ? 1.0 : 0.35),
        });

        var chip = MakePreviewChip(48, 30, 14, "Aa");
        chip.Margin = new Thickness(0, 0, 10, 0);

        var labelName = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelName.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.DisplayName)));
        labelName.Bind(TextBlock.FontWeightProperty, new Binding(nameof(StylePickerItem.IsCurrent))
        {
            Converter = new FuncValueConverter<bool, FontWeight>(isCurrent => isCurrent ? FontWeight.Bold : FontWeight.Normal),
        });
        labelName.Bind(TextBlock.FontStyleProperty, new Binding(nameof(StylePickerItem.IsNew))
        {
            Converter = new FuncValueConverter<bool, FontStyle>(isNew => isNew ? FontStyle.Italic : FontStyle.Normal),
        });

        var labelLineCount = new TextBlock
        {
            Opacity = 0.6,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelLineCount.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.LineCountText)));

        var labelCurrent = new TextBlock
        {
            Text = "✓",
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = UiUtil.GetAccentBrush(),
        };
        labelCurrent.Bind(IsVisibleProperty, new Binding(nameof(StylePickerItem.IsCurrent)));

        var panelName = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelName, labelLineCount, labelCurrent },
        };

        var labelSummary = new TextBlock
        {
            Opacity = 0.6,
            FontSize = UiUtil.ScaledFontSize(11),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelSummary.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.Summary)));

        var panelText = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Children = { panelName, labelSummary },
        };

        var labelShortcut = new TextBlock
        {
            Opacity = 0.6,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        labelShortcut.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.ShortcutText)));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(labelNumber, 0, 0);
        grid.Add(chip, 0, 1);
        grid.Add(panelText, 0, 2);
        grid.Add(labelShortcut, 0, 3);

        return grid;
    }

    private static Border MakeDetailsPanel(StylePickerViewModel vm)
    {
        var labelName = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            FontSize = UiUtil.ScaledFontSize(15),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelName.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.Name)));

        var preview = MakePreviewChip(double.NaN, 56, 22, PreviewText);
        preview.HorizontalAlignment = HorizontalAlignment.Stretch;

        var gridProperties = new Grid
        {
            ColumnSpacing = 10,
            RowSpacing = 4,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        var row = 0;
        AddProperty(gridProperties, ref row, Se.Language.General.Font, nameof(StylePickerItem.FontText));
        AddProperty(gridProperties, ref row, Se.Language.General.Alignment, nameof(StylePickerItem.AlignmentText));
        AddProperty(gridProperties, ref row, Se.Language.General.BorderStyle, nameof(StylePickerItem.BorderText));
        AddProperty(gridProperties, ref row, Se.Language.General.OutlineWidth, nameof(StylePickerItem.OutlineWidthText));
        AddProperty(gridProperties, ref row, Se.Language.General.ShadowWidth, nameof(StylePickerItem.ShadowWidthText));
        AddProperty(gridProperties, ref row, Se.Language.General.Margin, nameof(StylePickerItem.MarginsText));

        gridProperties.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var labelColors = MakePropertyLabel(Se.Language.General.Color);
        labelColors.VerticalAlignment = VerticalAlignment.Top;
        gridProperties.Add(labelColors, row, 0);
        var panelColors = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                MakeColorSwatch(Se.Language.Assa.Primary, nameof(StylePickerItem.PrimaryBrush), nameof(StylePickerItem.PrimaryHex)),
                MakeColorSwatch(Se.Language.Assa.Secondary, nameof(StylePickerItem.SecondaryBrush), nameof(StylePickerItem.SecondaryHex)),
                MakeColorSwatch(Se.Language.General.Outline, nameof(StylePickerItem.OutlineBrush), nameof(StylePickerItem.OutlineHex)),
                MakeColorSwatch(Se.Language.General.Shadow, nameof(StylePickerItem.ShadowBrush), nameof(StylePickerItem.ShadowHex)),
            },
        };
        gridProperties.Add(panelColors, row, 1);
        row++;

        var labelUsage = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
        };
        labelUsage.Bind(TextBlock.TextProperty, new Binding(nameof(StylePickerItem.UsageText)));

        var panel = new StackPanel
        {
            Spacing = 8,
            Children = { labelName, preview, gridProperties, labelUsage },
        };
        panel.Bind(DataContextProperty, new Binding(nameof(vm.SelectedItem)) { Source = vm });
        panel.Bind(IsVisibleProperty, new Binding(nameof(vm.SelectedItem)) { Source = vm, Converter = ObjectConverters.IsNotNull });

        var border = UiUtil.MakeBorderForControl(panel);
        border.Padding = new Thickness(10);
        border.VerticalAlignment = VerticalAlignment.Top;
        return border;
    }

    private static TextBlock MakePropertyLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static void AddProperty(Grid grid, ref int row, string label, string valuePath)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.Add(MakePropertyLabel(label), row, 0);

        var value = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        value.Bind(TextBlock.TextProperty, new Binding(valuePath));
        grid.Add(value, row, 1);
        row++;
    }

    private static Control MakeColorSwatch(string label, string brushPath, string hexPath)
    {
        var swatch = new Border
        {
            Width = 28,
            Height = 18,
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetTextColor(0.4),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        swatch.Bind(Border.BackgroundProperty, new Binding(brushPath));

        var labelName = new TextBlock
        {
            Text = label,
            FontSize = UiUtil.ScaledFontSize(10),
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        var panel = new StackPanel
        {
            Spacing = 2,
            Children = { swatch, labelName },
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            panel.Bind(ToolTip.TipProperty, new Binding(hexPath));
        }

        return panel;
    }
}

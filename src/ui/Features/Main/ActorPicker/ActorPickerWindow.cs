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

namespace Nikse.SubtitleEdit.Features.Main.ActorPicker;

public class ActorPickerWindow : Window
{
    public ActorPickerWindow(ActorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ActorPickerTitle;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Width = 460;

        vm.Window = this;
        DataContext = vm;

        var labelSelectionInfo = new TextBlock { Opacity = 0.8 };
        labelSelectionInfo.Bind(TextBlock.TextProperty, new Binding(nameof(vm.SelectionInfo)));

        var textBoxFilter = new TextBox
        {
            PlaceholderText = Se.Language.General.ActorPickerFilterHint,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        textBoxFilter.Bind(TextBox.TextProperty, new Binding(nameof(vm.FilterText)) { Mode = BindingMode.TwoWay });

        var listBox = new ListBox
        {
            ItemsSource = vm.VisibleItems,
            ItemTemplate = new FuncDataTemplate<ActorPickerItem>((_, _) => MakeActorRow(), true),
            MaxHeight = 520,
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
        listBox.Tapped += (_, e) =>
        {
            if (e.Source is Visual visual && visual.FindAncestorOfType<ListBoxItem>(true)?.DataContext is ActorPickerItem item)
            {
                vm.Pick(item);
            }
        };

        var labelKeysHint = new TextBlock
        {
            Text = Se.Language.General.ActorPickerKeysHint,
            TextWrapping = TextWrapping.Wrap,
            FontSize = UiUtil.ScaledFontSize(11),
            Opacity = 0.7,
        };

        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Actor + " - " + Se.Language.General.Remove);
        buttonRemove.Click += (_, _) => vm.PickRemoveActor();
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel);
        buttonCancel.Click += (_, _) => vm.Cancel();
        var panelButtons = UiUtil.MakeButtonBar(buttonRemove, buttonCancel);

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
        grid.Add(labelSelectionInfo, 0);
        grid.Add(textBoxFilter, 1);
        grid.Add(listBox, 2);
        grid.Add(labelKeysHint, 3);
        grid.Add(panelButtons, 4);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxFilter);

        // Tunnel, so number keys, arrows and Enter are handled before the filter box types them.
        AddHandler(KeyDownEvent, (_, e) => vm.OnKeyDown(e), RoutingStrategies.Tunnel);

        // Keep a long cast reachable on small screens - the list scrolls.
        Opened += (_, _) =>
        {
            var workingArea = Screens.ScreenFromWindow(this)?.WorkingArea;
            if (workingArea != null && RenderScaling > 0)
            {
                listBox.MaxHeight = Math.Min(listBox.MaxHeight, workingArea.Value.Height / RenderScaling * 0.6);
            }
        };
    }

    private static Control MakeActorRow()
    {
        var labelNumber = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            Width = 24,
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelNumber.Bind(TextBlock.TextProperty, new Binding(nameof(ActorPickerItem.NumberText)));
        labelNumber.Bind(OpacityProperty, new Binding(nameof(ActorPickerItem.IsNumberKeyActive))
        {
            Converter = new FuncValueConverter<bool, double>(isActive => isActive ? 1.0 : 0.35),
        });

        var labelName = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        labelName.Bind(TextBlock.TextProperty, new Binding(nameof(ActorPickerItem.DisplayName)));
        labelName.Bind(TextBlock.FontWeightProperty, new Binding(nameof(ActorPickerItem.IsCurrent))
        {
            Converter = new FuncValueConverter<bool, FontWeight>(isCurrent => isCurrent ? FontWeight.Bold : FontWeight.Normal),
        });
        labelName.Bind(TextBlock.FontStyleProperty, new Binding(nameof(ActorPickerItem.IsNew))
        {
            Converter = new FuncValueConverter<bool, FontStyle>(isNew => isNew ? FontStyle.Italic : FontStyle.Normal),
        });

        var labelLineCount = new TextBlock
        {
            Opacity = 0.6,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelLineCount.Bind(TextBlock.TextProperty, new Binding(nameof(ActorPickerItem.LineCountText)));

        var labelShortcut = new TextBlock
        {
            Opacity = 0.6,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        labelShortcut.Bind(TextBlock.TextProperty, new Binding(nameof(ActorPickerItem.ShortcutText)));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        grid.Add(labelNumber, 0, 0);
        grid.Add(labelName, 0, 1);
        grid.Add(labelLineCount, 0, 2);
        grid.Add(labelShortcut, 0, 3);

        return grid;
    }
}

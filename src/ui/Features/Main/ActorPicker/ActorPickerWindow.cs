using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Main.ActorPicker;

/// <summary>
/// The actor picker in the editor look (see <see cref="EditorChrome"/>): selection info and the
/// filter field on top, the actor list in a field frame, the keys hint under it, and remove actor
/// and Cancel in the button bar.
/// </summary>
public class ActorPickerWindow : Window
{
    public ActorPickerWindow(ActorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ActorPickerTitle;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Width = 460;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);

        vm.Window = this;
        DataContext = vm;

        var labelSelectionInfo = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        labelSelectionInfo.TextTrimming = TextTrimming.CharacterEllipsis;
        labelSelectionInfo.Bind(TextBlock.TextProperty, new Binding(nameof(vm.SelectionInfo)));

        var textBoxFilter = new TextBox
        {
            PlaceholderText = Se.Language.General.ActorPickerFilterHint,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        }.AsTextField(Se.Language.General.ActorPickerFilterHint);
        textBoxFilter.InnerLeftContent = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-magnifying-glass",
            FontSize = 11,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };
        textBoxFilter.Bind(TextBox.TextProperty, new Binding(nameof(vm.FilterText)) { Mode = BindingMode.TwoWay });

        var listBox = new ListBox
        {
            ItemsSource = vm.VisibleItems,
            ItemTemplate = new FuncDataTemplate<ActorPickerItem>((_, _) => MakeActorRow(), true),
            MaxHeight = 520,
            Focusable = false,
        };
        AutomationProperties.SetName(listBox, Se.Language.General.ActorPickerTitle);
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

        var labelKeysHint = EditorChrome.MakeWrappingDimLabel(Se.Language.General.ActorPickerKeysHint);
        labelKeysHint.FontSize = EditorChrome.ScaledFont(11.5);

        var panel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(14, 12, 14, 12),
            Children =
            {
                labelSelectionInfo,
                textBoxFilter,
                EditorChrome.MakeCompactListFrame(listBox),
                labelKeysHint,
            },
        };

        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Actor + " - " + Se.Language.General.Remove);
        buttonRemove.Click += (_, _) => vm.PickRemoveActor();
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel);
        buttonCancel.Click += (_, _) => vm.Cancel();

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeDockedButtonBar(null, buttonRemove, buttonCancel));

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
            FontWeight = FontWeight.SemiBold,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.AccentBrush,
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
            FontSize = EditorChrome.FieldFontSize,
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

        var labelLineCount = EditorChrome.MakeDimLabel(string.Empty, new Thickness(8, 0, 0, 0));
        labelLineCount.Bind(TextBlock.TextProperty, new Binding(nameof(ActorPickerItem.LineCountText)));

        var labelShortcut = EditorChrome.MakeDimLabel(string.Empty, new Thickness(12, 0, 0, 0));
        labelShortcut.Foreground = EditorChrome.FaintTextBrush;
        labelShortcut.HorizontalAlignment = HorizontalAlignment.Right;
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

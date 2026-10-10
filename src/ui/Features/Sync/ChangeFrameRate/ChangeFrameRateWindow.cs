using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public class ChangeFrameRateWindow : Window
{
    public ChangeFrameRateWindow(ChangeFrameRateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.ChangeFrameRate);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        // Where the preset "from" rate came from: the loaded video's name and detected frame rate.
        var videoIcon = new ContentControl
        {
            Width = 16,
            Height = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
        };
        Attached.SetIcon(videoIcon, IconNames.MovieOpenOutline);
        var textVideoInfo = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 420,
        }.WithBindText(vm, nameof(vm.VideoInfoText));
        var panelVideoInfo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Opacity = 0.85,
            Margin = new Thickness(0, 0, 0, 4),
            Children = { videoIcon, textVideoInfo },
        }.WithBindIsVisible(nameof(vm.HasVideo));
        ToolTip.SetTip(panelVideoInfo, vm.VideoFileName);
        ToolTip.SetTip(textVideoInfo, vm.VideoFileName);

        var labelFromFrameRate = new Label
        {
            Content = Se.Language.Sync.FromFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Editable like SE 4, so any rate can be typed - e.g. 1, 2, 5, 10 or 15 fps (#15806).
        var comboFromFrameRate = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
            IsEditable = true,
            DisplayMemberBinding = FrameRateDisplayBinding(),
        }
        .WithBindItemsSource(nameof(vm.FromFrameRates));
        BindSelectedFrameRate(comboFromFrameRate, nameof(vm.SelectedFromFrameRate));
        UiUtil.OnEditableComboBoxCommit(comboFromFrameRate, () =>
        {
            vm.CommitTypedFromFrameRate(comboFromFrameRate.Text);
            comboFromFrameRate.SelectedItem = vm.SelectedFromFrameRate;
            comboFromFrameRate.Text = FormatFrameRate(vm.SelectedFromFrameRate);
        }, handleEnter: false);

        var buttonFromFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseFromFrameRateCommand, accessibleName: Se.Language.Sync.FromFrameRate);

        var buttonSwitch = UiUtil.MakeButton(vm.SwitchFrameRatesCommand, IconNames.SwapVertical,
            $"{Se.Language.Sync.FromFrameRate} <-> {Se.Language.Sync.ToFrameRate}");

        var labelToFrameRate = new Label
        {
            Content = Se.Language.Sync.ToFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboToFrameRate = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
            IsEditable = true,
            DisplayMemberBinding = FrameRateDisplayBinding(),
        }
        .WithBindItemsSource(nameof(vm.ToFrameRates));
        BindSelectedFrameRate(comboToFrameRate, nameof(vm.SelectedToFrameRate));
        UiUtil.OnEditableComboBoxCommit(comboToFrameRate, () =>
        {
            vm.CommitTypedToFrameRate(comboToFrameRate.Text);
            comboToFrameRate.SelectedItem = vm.SelectedToFrameRate;
            comboToFrameRate.Text = FormatFrameRate(vm.SelectedToFrameRate);
        }, handleEnter: false);

        var buttonToFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseToFrameRateCommand, accessibleName: Se.Language.Sync.ToFrameRate);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;
        grid.Add(panelVideoInfo, row, 0, 1, 4);
        row++;

        grid.Add(labelFromFrameRate, row, 0);
        grid.Add(comboFromFrameRate, row, 1);
        grid.Add(buttonFromFrameRate, row, 2);
        grid.Add(buttonSwitch, row, 3, 2);
        row++;

        grid.Add(labelToFrameRate, row, 0);
        grid.Add(comboToFrameRate, row, 1);
        grid.Add(buttonToFrameRate, row, 2);
        row++;

        grid.Add(buttonPanel, row, 0, 1, 4);

        Content = grid;
        
        UiUtil.FocusOnFirstActivation(this, comboFromFrameRate); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// Binds the selected item to a <see cref="double"/> rate. Text typed into the editable combo
    /// box that matches no item makes the selection <c>null</c>, which a <see cref="double"/>
    /// can't hold - that showed "Could not convert '(null)' to System.Double" while typing
    /// (#15869). The rate keeps its value until the typed text is committed.
    /// </summary>
    private static void BindSelectedFrameRate(ComboBox comboBox, string propertyName)
    {
        comboBox.Bind(ComboBox.SelectedItemProperty, new Binding
        {
            Path = propertyName,
            Mode = BindingMode.TwoWay,
            Converter = IgnoreNullSelectionConverter.Instance,
        });
    }

    private sealed class IgnoreNullSelectionConverter : IValueConverter
    {
        public static readonly IgnoreNullSelectionConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value ?? BindingOperations.DoNothing;
    }

    private static string FormatFrameRate(double frameRate)
    {
        return frameRate.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Frame rates always print with a decimal point ("23.976"), like the toolbar combo and the
    /// video line - a bare double item would take the OS decimal separator ("23,976").
    /// </summary>
    private static Binding FrameRateDisplayBinding()
    {
        return new Binding(".")
        {
            StringFormat = "{0:0.###}",
            ConverterCulture = CultureInfo.InvariantCulture,
        };
    }
}

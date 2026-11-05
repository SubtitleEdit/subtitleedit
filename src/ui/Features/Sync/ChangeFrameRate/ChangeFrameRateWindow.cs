using Avalonia;
using Avalonia.Automation;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        // Where the preset "from" rate came from: the loaded video's name and detected frame rate.
        var videoIcon = new Icon
        {
            Value = "fa-solid fa-file-video",
            FontSize = 12,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var textVideoInfo = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 300,
        }.WithBindText(vm, nameof(vm.VideoInfoText));
        var panelVideoInfo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 0),
            Children = { videoIcon, textVideoInfo },
        }.WithBindIsVisible(nameof(vm.HasVideo));
        ToolTip.SetTip(panelVideoInfo, vm.VideoFileName);
        ToolTip.SetTip(textVideoInfo, vm.VideoFileName);

        // Editable like SE 4, so any rate can be typed - e.g. 1, 2, 5, 10 or 15 fps (#15806).
        var comboFromFrameRate = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsEditable = true,
            DisplayMemberBinding = FrameRateDisplayBinding(),
        }
        .WithBindItemsSource(nameof(vm.FromFrameRates));
        AutomationProperties.SetName(comboFromFrameRate, Se.Language.Sync.FromFrameRate);
        BindSelectedFrameRate(comboFromFrameRate, nameof(vm.SelectedFromFrameRate));
        UiUtil.OnEditableComboBoxCommit(comboFromFrameRate, () =>
        {
            vm.CommitTypedFromFrameRate(comboFromFrameRate.Text);
            comboFromFrameRate.SelectedItem = vm.SelectedFromFrameRate;
            comboFromFrameRate.Text = FormatFrameRate(vm.SelectedFromFrameRate);
        }, handleEnter: false);

        var buttonFromFrameRate = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Sync.FromFrameRate, vm.BrowseFromFrameRateCommand, 28);

        var buttonSwitch = EditorChrome.CreateToolButton("fa-solid fa-arrows-up-down",
            $"{Se.Language.Sync.FromFrameRate} <-> {Se.Language.Sync.ToFrameRate}", vm.SwitchFrameRatesCommand, 28);
        buttonSwitch.VerticalAlignment = VerticalAlignment.Center;
        buttonSwitch.Margin = new Thickness(4, 0, 0, 0);

        var comboToFrameRate = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsEditable = true,
            DisplayMemberBinding = FrameRateDisplayBinding(),
        }
        .WithBindItemsSource(nameof(vm.ToFrameRates));
        AutomationProperties.SetName(comboToFrameRate, Se.Language.Sync.ToFrameRate);
        BindSelectedFrameRate(comboToFrameRate, nameof(vm.SelectedToFrameRate));
        UiUtil.OnEditableComboBoxCommit(comboToFrameRate, () =>
        {
            vm.CommitTypedToFrameRate(comboToFrameRate.Text);
            comboToFrameRate.SelectedItem = vm.SelectedToFrameRate;
            comboToFrameRate.Text = FormatFrameRate(vm.SelectedToFrameRate);
        }, handleEnter: false);

        var buttonToFrameRate = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Sync.ToFrameRate, vm.BrowseToFrameRateCommand, 28);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }, // label
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // combo box
                new ColumnDefinition { Width = GridLength.Auto }, // browse
                new ColumnDefinition { Width = GridLength.Auto }, // switch
            },
            ColumnSpacing = 6,
            RowSpacing = 7,
        };

        grid.Add(EditorChrome.MakeDimLabel(Se.Language.Sync.FromFrameRate, new Thickness(0, 0, 6, 0)), 0, 0);
        grid.Add(comboFromFrameRate, 0, 1);
        grid.Add(buttonFromFrameRate, 0, 2);
        grid.Add(buttonSwitch, 0, 3, 2);

        grid.Add(EditorChrome.MakeDimLabel(Se.Language.Sync.ToFrameRate, new Thickness(0, 0, 6, 0)), 1, 0);
        grid.Add(comboToFrameRate, 1, 1);
        grid.Add(buttonToFrameRate, 1, 2);

        var section = EditorChrome.MakeEditorSection(Se.Language.General.FrameRate, grid, first: true);
        section.MinWidth = 340;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(panelVideoInfo, buttonCancel, buttonOk));

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

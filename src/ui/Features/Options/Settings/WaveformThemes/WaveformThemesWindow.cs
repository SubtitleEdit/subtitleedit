using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformThemes;

/// <summary>
/// Waveform color themes in the editor look (see <see cref="EditorChrome"/>): the theme picker
/// and a live preview on top, the colors below, load/export/save-as + Cancel + OK in the button
/// bar. The preview and the swatches show the theme's own colors.
/// </summary>
public class WaveformThemesWindow : Window
{
    public WaveformThemesWindow(WaveformThemesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.WaveformColorThemesDotDotDot;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var themeComboBox = new ComboBox
        {
            MinWidth = 260,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Themes)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedTheme))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        var themeLabel = Se.Language.Options.Settings.WaveformColorTheme.TrimEnd(':', ' ');
        AutomationProperties.SetName(themeComboBox, themeLabel);

        var preview = new WaveformPreviewControl(vm)
        {
            Width = 560,
            Height = 90,
        };
        var previewFrame = new Border
        {
            Child = preview,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var themeContent = new StackPanel
        {
            Spacing = 12,
            Children = { themeComboBox, previewFrame },
        };

        var colorGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,8,Auto,32,*,8,Auto"),
            RowSpacing = 6,
        };

        var leftColors = new (string label, string propName)[]
        {
            (Se.Language.Options.Settings.WaveformTextColor, nameof(vm.TextColor)),
            (Se.Language.Options.Settings.WaveformColor, nameof(vm.WaveformColor)),
            (Se.Language.Options.Settings.WaveformBackgroundColor, nameof(vm.BackgroundColor)),
            (Se.Language.Options.Settings.WaveformSelectedColor, nameof(vm.SelectedColor)),
            (Se.Language.Options.Settings.WaveformCursorColor, nameof(vm.CursorColor)),
            (Se.Language.Options.Settings.WaveformShotChangeColor, nameof(vm.ShotChangeColor)),
        };

        var rightColors = new (string label, string propName)[]
        {
            (Se.Language.Options.Settings.WaveformParagraphBackgroundColor, nameof(vm.ParagraphBackgroundColor)),
            (Se.Language.Options.Settings.WaveformParagraphSelectedBackgroundColor, nameof(vm.ParagraphSelectedBackgroundColor)),
            (Se.Language.Options.Settings.WaveformParagraphLeftColor, nameof(vm.ParagraphLeftColor)),
            (Se.Language.Options.Settings.WaveformParagraphRightColor, nameof(vm.ParagraphRightColor)),
            (Se.Language.Options.Settings.WaveformFancyHighColor, nameof(vm.FancyHighColor)),
            (Se.Language.Options.Settings.WaveformGridColor, nameof(vm.GridColor)),
        };

        var maxRows = Math.Max(leftColors.Length, rightColors.Length);
        for (var i = 0; i < maxRows; i++)
        {
            colorGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (var i = 0; i < leftColors.Length; i++)
        {
            var (label, propName) = leftColors[i];
            colorGrid.Add(MakeColorLabel(label), i, 0);
            colorGrid.Add(MakeColorButton(vm, propName, label), i, 2);
        }

        for (var i = 0; i < rightColors.Length; i++)
        {
            var (label, propName) = rightColors[i];
            colorGrid.Add(MakeColorLabel(label), i, 4);
            colorGrid.Add(MakeColorButton(vm, propName, label), i, 6);
        }

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(themeLabel, themeContent, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Color, colorGrid),
            },
        };

        var buttonLoad = EditorChrome.MakeTextToolButton(IconNames.FolderOpen, Se.Language.Options.Settings.WaveformLoadThemeDotDotDot, vm.LoadThemeCommand);
        var buttonExport = EditorChrome.MakeTextToolButton(IconNames.Export, Se.Language.Options.Settings.WaveformExportThemeDotDotDot, vm.ExportThemeCommand);
        var buttonSaveAs = EditorChrome.MakeTextToolButton(IconNames.ContentSave, Se.Language.Options.Settings.WaveformSaveAsCustomTheme, vm.SaveAsCustomThemeCommand);
        var panelLeft = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(0, 0, 16, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonLoad, buttonExport, buttonSaveAs },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(panelLeft, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, themeComboBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                vm.CancelCommand.Execute(null);
            }
            else if (UiUtil.IsHelp(e))
            {
                e.Handled = true;
                UiUtil.ShowHelp("features/audio-visualizer", "waveform-themes");
            }
        };
    }

    private static TextBlock MakeColorLabel(string text)
    {
        var label = EditorChrome.MakeDimLabel(text, new Thickness(0));
        label.FontSize = EditorChrome.FieldFontSize;
        return label;
    }

    private static Button MakeColorButton(WaveformThemesViewModel vm, string propertyName, string accessibleName)
    {
        var swatch = new Border
        {
            Width = 34,
            Height = 18,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };

        swatch.Bind(Border.BackgroundProperty, new Binding(propertyName)
        {
            Source = vm,
            Converter = new ColorToBrushConverter(),
        });

        var button = new Button
        {
            Content = swatch,
            Padding = new Thickness(4, 3),
            CornerRadius = new CornerRadius(6),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
        };
        // The swatch is all there is to see - name the button for a screen reader.
        AutomationProperties.SetName(button, accessibleName);

        button.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(button) is not Window window)
            {
                return;
            }

            var propInfo = typeof(WaveformThemesViewModel).GetProperty(propertyName);
            var currentColor = propInfo?.GetValue(vm) is Color c ? c : Colors.White;

            var pickerVm = new ColorPickerViewModel();
            pickerVm.Initialize(currentColor);
            var pickerWindow = new ColorPickerWindow(pickerVm);
            await WindowService.ShowModalAsync(window, pickerWindow);

            if (pickerVm.OkPressed)
            {
                propInfo?.SetValue(vm, pickerVm.SelectedColor);
            }
        };

        return button;
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Transport Stream settings for batch convert, in the editor look (see <see cref="EditorChrome"/>):
/// position and video size overrides, the file name ending, OK in the button bar.
/// </summary>
public class BatchConvertTsSettingsWindow : Window
{
    private const double LabelWidth = 130;

    public BatchConvertTsSettingsWindow(BatchConvertTsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.TransportStreamSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var labelInfo = EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.TransportStreamSettingsInfo);
        labelInfo.MaxWidth = 560;
        labelInfo.HorizontalAlignment = HorizontalAlignment.Left;

        // X position
        var switchOverrideX = EditorChrome.MakeSwitchRow(nameof(vm.OverrideXPosition), Se.Language.Tools.BatchConvert.TransportStreamOverrideXPosition, out var toggleOverrideX);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.HorizontalAlignments, vm, nameof(vm.SelectedHorizontalAlignment))
            .WithBindEnabled(nameof(vm.OverrideXPosition));
        comboBoxAlignment.MinWidth = 140;
        AutomationProperties.SetName(comboBoxAlignment, Se.Language.General.Alignment);
        var numericHMargin = Field(UiUtil.MakeNumericUpDownInt(0, 100, 5, 80, vm, nameof(vm.HorizontalMarginPercent)), Se.Language.File.Export.LeftRightMargin)
            .WithBindEnabled(nameof(vm.OverrideXPosition));
        var panelX = Indented(
            EditorChrome.MakeLabeledRow(Se.Language.General.Alignment, comboBoxAlignment, LabelWidth, wrap: true),
            EditorChrome.MakeLabeledRow(Se.Language.File.Export.LeftRightMargin, Percent(numericHMargin), LabelWidth, wrap: true));

        // Y position
        var switchOverrideY = EditorChrome.MakeSwitchRow(nameof(vm.OverrideYPosition), Se.Language.Tools.BatchConvert.TransportStreamOverrideYPosition);
        var numericBottomMargin = Field(UiUtil.MakeNumericUpDownInt(0, 100, 5, 80, vm, nameof(vm.BottomMarginPercent)), Se.Language.Tools.BatchConvert.TransportStreamBottomMargin)
            .WithBindEnabled(nameof(vm.OverrideYPosition));
        var panelY = Indented(EditorChrome.MakeLabeledRow(Se.Language.Tools.BatchConvert.TransportStreamBottomMargin, Percent(numericBottomMargin), LabelWidth, wrap: true));

        var position = new StackPanel { Spacing = 8, Children = { switchOverrideX, panelX, switchOverrideY, panelY } };

        // Video size
        var switchOverrideSize = EditorChrome.MakeSwitchRow(nameof(vm.OverrideVideoSize), Se.Language.Tools.BatchConvert.TransportStreamOverrideVideoSize);
        var numericWidth = Field(UiUtil.MakeNumericUpDownInt(1, 9999, 1920, 80, vm, nameof(vm.VideoWidth)), Se.Language.General.Width, "W")
            .WithBindEnabled(nameof(vm.OverrideVideoSize));
        var numericHeight = Field(UiUtil.MakeNumericUpDownInt(1, 9999, 1080, 80, vm, nameof(vm.VideoHeight)), Se.Language.General.Height, "H")
            .WithBindEnabled(nameof(vm.OverrideVideoSize));
        var buttonGetSizeFromVideo = EditorChrome.MakeActionButton("fa-solid fa-film", Se.Language.Tools.BatchConvert.TransportStreamGetSizeFromVideo, vm.GetSizeFromVideoCommand);
        var panelSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(46, 0, 0, 0), // under the switch's label
            Children =
            {
                numericWidth,
                new TextBlock { Text = "×", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center },
                numericHeight,
                buttonGetSizeFromVideo,
            }
        };
        var videoSize = new StackPanel { Spacing = 8, Children = { switchOverrideSize, panelSize } };

        // File name ending
        var textBoxFileNameEnding = EditorChrome.MakeTextField(nameof(vm.FileNameEnding), Se.Language.Tools.BatchConvert.TransportStreamFileNameEnding, 220);
        vm.GetFileNameEndingCaretIndex = () => textBoxFileNameEnding.CaretIndex;
        vm.SetFileNameEndingCaretIndex = index =>
        {
            textBoxFileNameEnding.CaretIndex = index;
            textBoxFileNameEnding.Focus();
        };
        var buttonInsertPlaceholder = new Button
        {
            Content = new TextBlock { Text = "{ }", FontSize = EditorChrome.FieldFontSize, FontWeight = FontWeight.SemiBold },
            Width = 34,
            Height = EditorChrome.ScaledSize(28),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    MakePlaceholderMenuItem(vm, Se.Language.General.TwoLetterLanguageCode, TransportStreamExportSettings.PlaceholderTwoLetter),
                    MakePlaceholderMenuItem(vm, Se.Language.Tools.BatchConvert.TwoLetterLanguageCodeUppercase, TransportStreamExportSettings.PlaceholderTwoLetterUppercase),
                    MakePlaceholderMenuItem(vm, Se.Language.General.ThreeLetterLanguageCode, TransportStreamExportSettings.PlaceholderThreeLetter),
                    MakePlaceholderMenuItem(vm, Se.Language.Tools.BatchConvert.ThreeLetterLanguageCodeUppercase, TransportStreamExportSettings.PlaceholderThreeLetterUppercase),
                }
            },
        };
        buttonInsertPlaceholder.Classes.Add("tool");
        AutomationProperties.SetName(buttonInsertPlaceholder, Se.Language.Tools.BatchConvert.TransportStreamFileNameEndingInfo);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonInsertPlaceholder, Se.Language.Tools.BatchConvert.TransportStreamFileNameEndingInfo);
        }

        var labelSample = EditorChrome.MakeBoundHint(nameof(vm.FileNameEndingSample));
        var panelFileNameEnding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { textBoxFileNameEnding, buttonInsertPlaceholder, labelSample }
        };

        var fileNameInfo = EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.TransportStreamFileNameEndingInfo);
        fileNameInfo.MaxWidth = 480;
        fileNameInfo.HorizontalAlignment = HorizontalAlignment.Left;
        fileNameInfo.Margin = new Thickness(LabelWidth + 8, 0, 0, 0);

        var outputFiles = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.Tools.BatchConvert.TransportStreamFileNameEnding, panelFileNameEnding, LabelWidth, wrap: true),
                fileNameInfo,
                EditorChrome.MakeSwitchRow(nameof(vm.OnlyTeletext), Se.Language.Tools.BatchConvert.TransportStreamOnlyTeletext),
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = EditorChrome.MakeSectionStack(new Border { Child = labelInfo, Padding = new Thickness(14, 12, 14, 0) }, EditorChrome.MakeEditorSection(Se.Language.General.Position, position, first: true), EditorChrome.MakeEditorSection(Se.Language.General.VideoResolution, videoSize), EditorChrome.MakeEditorSection(Se.Language.General.Output, outputFiles));
        sections.MinWidth = 600;

        Content = new StackPanel
        {
            Children =
            {
                EditorChrome.MakePanel(sections),
                EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk),
            }
        };

        UiUtil.FocusOnFirstActivation(this, toggleOverrideX); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A compact number field (see <see cref="EditorChrome.MakeField"/>) with an optional dim prefix.
    /// </summary>
    private static NumericUpDown Field(NumericUpDown field, string name, string? prefix = null)
    {
        field.Classes.Add("field");
        AutomationProperties.SetName(field, name);
        if (prefix != null)
        {
            field.InnerLeftContent = new TextBlock
            {
                Text = prefix,
                Foreground = EditorChrome.FaintTextBrush,
                FontSize = EditorChrome.ScaledFont(11),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
        }

        return field;
    }

    private static StackPanel Percent(NumericUpDown field)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { field, new TextBlock { Text = "%", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center } },
        };
    }

    /// <summary>
    /// The rows that belong to the switch above, indented under its label.
    /// </summary>
    private static StackPanel Indented(params Control[] rows)
    {
        var panel = new StackPanel { Spacing = 6, Margin = new Thickness(46, 0, 0, 0) };
        foreach (var row in rows)
        {
            panel.Children.Add(row);
        }

        return panel;
    }

    private static MenuItem MakePlaceholderMenuItem(BatchConvertTsSettingsViewModel vm, string header, string placeholder)
    {
        return new MenuItem
        {
            Header = header,
            Command = vm.InsertPlaceholderCommand,
            CommandParameter = placeholder,
        };
    }
}

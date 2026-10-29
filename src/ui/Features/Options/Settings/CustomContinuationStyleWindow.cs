using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// The custom continuation style in the editor look (see <see cref="EditorChrome"/>): prefix,
/// suffix and long gap sections on the left (scrolling on short screens), the preview on the
/// right, "Load defaults" + Cancel + OK in the button bar.
/// </summary>
public class CustomContinuationStyleWindow : Window
{
    private const double LabelWidth = 160;

    public CustomContinuationStyleWindow(CustomContinuationStyleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.EditContinuationStyleCustom;
        CanResize = true;
        Width = 960;
        Height = 700;
        MinWidth = 760;
        MinHeight = 480;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        var settings = new ScrollViewer
        {
            Content = MakeControls(vm, out var comboBoxFirstInput),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        columns.Add(settings, 0, 0);
        columns.Add(new Border
        {
            Child = MakePreviewView(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(columns, EditorChrome.MakeDockedButtonBar(MakeLoadDefaultsButton(vm), buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, comboBoxFirstInput); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static ComboBox MakeCombo(CustomContinuationStyleViewModel vm, string property, string accessibleName)
    {
        var comboBox = UiUtil.MakeComboBox(vm.PreAndSuffixes, vm, property);
        comboBox.MinWidth = 120;
        comboBox.HorizontalAlignment = HorizontalAlignment.Left;
        comboBox.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboBox, accessibleName);
        return comboBox;
    }

    private static StackPanel MakeSwitch(string property, string text, CustomContinuationStyleViewModel vm, string? isEnabledProperty = null)
    {
        var row = EditorChrome.MakeToggleRow(property, text, out var toggle);
        toggle.IsCheckedChanged += (s, e) => vm.StyleChanged();
        if (isEnabledProperty != null)
        {
            row.Bind(IsEnabledProperty, new Binding(isEnabledProperty));
        }

        return row;
    }

    private static StackPanel MakeControls(CustomContinuationStyleViewModel vm, out ComboBox firstInput)
    {
        var comboBoxPrefix = MakeCombo(vm, nameof(vm.SelectedPrefix), Se.Language.General.Prefix);
        firstInput = comboBoxPrefix;
        comboBoxPrefix.SelectionChanged += (s, e) => vm.StyleChanged();
        var prefixContent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Prefix, comboBoxPrefix, LabelWidth),
                MakeSwitch(nameof(vm.SelectedPrefixAddSpace), Se.Language.Options.Settings.AddSpace, vm),
            },
        };

        var comboBoxSuffix = MakeCombo(vm, nameof(vm.SelectedSuffix), Se.Language.General.Suffix);
        comboBoxSuffix.SelectionChanged += (s, e) => vm.StyleChanged();
        var suffixContent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Suffix, comboBoxSuffix, LabelWidth),
                MakeSwitch(nameof(vm.SelectedSuffixesProcessIfEndWithComma), Se.Language.Options.Settings.ProcessIfEndsWithComma, vm),
                MakeSwitch(nameof(vm.SelectedSuffixesAddSpace), Se.Language.Options.Settings.AddSpace, vm),
                MakeSwitch(nameof(vm.SelectedSuffixesRemoveComma), Se.Language.Options.Settings.RemoveComma, vm),
            },
        };

        var switchUseSpecialStyleAfterLongGaps = EditorChrome.MakeToggleRow(nameof(vm.UseSpecialStyleAfterLongGaps), Se.Language.Options.Settings.UseSpecialStyleAfterLongGaps, out var toggleLongGaps);
        toggleLongGaps.PropertyChanged += (s, e) => vm.StyleChanged();
        var numericUpDownLongGapThreshold = UiUtil.MakeNumericUpDownInt(0, 10_000, 300, 120, vm, nameof(vm.LongGapMs))
            .WithBindEnabled(nameof(vm.UseSpecialStyleAfterLongGaps))
            .AsField(Se.Language.Options.Settings.LongGapThreshold, themeForeground: true);
        var comboBoxLongPrefix = MakeCombo(vm, nameof(vm.SelectedLongGapPrefix), Se.Language.General.Prefix)
            .WithBindEnabled(nameof(vm.UseSpecialStyleAfterLongGaps));
        comboBoxLongPrefix.SelectionChanged += (s, e) => vm.StyleChanged();
        var comboBoxLongSuffix = MakeCombo(vm, nameof(vm.SelectedLongGapSuffix), Se.Language.General.Suffix)
            .WithBindEnabled(nameof(vm.UseSpecialStyleAfterLongGaps));
        comboBoxLongSuffix.SelectionChanged += (s, e) => vm.StyleChanged();
        var longGapContent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                switchUseSpecialStyleAfterLongGaps,
                EditorChrome.MakeLabeledRow(Se.Language.Options.Settings.LongGapThreshold, numericUpDownLongGapThreshold, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.Prefix, comboBoxLongPrefix, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.Suffix, comboBoxLongSuffix, LabelWidth),
                MakeSwitch(nameof(vm.SelectedLongGapSuffixesProcessIfEndWithComma), Se.Language.Options.Settings.ProcessIfEndsWithComma, vm, nameof(vm.UseSpecialStyleAfterLongGaps)),
                MakeSwitch(nameof(vm.SelectedLongGapSuffixesAddSpace), Se.Language.Options.Settings.AddSpace, vm, nameof(vm.UseSpecialStyleAfterLongGaps)),
                MakeSwitch(nameof(vm.SelectedLongGapSuffixesRemoveComma), Se.Language.Options.Settings.RemoveComma, vm, nameof(vm.UseSpecialStyleAfterLongGaps)),
            },
        };

        return new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Prefix, prefixContent, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Suffix, suffixContent),
                EditorChrome.MakeEditorSection(Se.Language.Options.Settings.AfterLongGap.TrimEnd(':', ' '), longGapContent),
            },
        };
    }

    private static SeSplitButton MakeLoadDefaultsButton(CustomContinuationStyleViewModel vm)
    {
        return new SeSplitButton
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content = Se.Language.General.LoadDefaults,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.General.None,
                        Command = vm.LoadContinuationStyleNoneCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneTrailingDots,
                        Command = vm.LoadContinuationStyleNoneTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneLeadingTrailingDots,
                        Command = vm.LoadContinuationStyleNoneLeadingTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneTrailingEllipsis,
                        Command = vm.LoadContinuationStyleNoneTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneLeadingTrailingEllipsis,
                        Command = vm.LoadContinuationStyleNoneLeadingTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleOnlyTrailingDots,
                        Command = vm.LoadContinuationStyleOnlyTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDots,
                        Command = vm.LoadContinuationStyleLeadingTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleOnlyTrailingEllipsis,
                        Command = vm.LoadContinuationStyleOnlyTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingEllipsis,
                        Command = vm.LoadContinuationStyleLeadingTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDash,
                        Command = vm.LoadContinuationStyleLeadingTrailingDashCommand
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDashDots,
                        Command = vm.LoadContinuationStyleLeadingTrailingDashDotsCommand
                    },
                 }
            }
        };
    }

    private static Grid MakePreviewView(CustomContinuationStyleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };

        var previewScroll = new ScrollViewer
        {
            Content = vm.PanelPreview,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        grid.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 0);
        grid.Add(EditorChrome.MakeListFrame(previewScroll, clearListBackground: true), 1);
        return grid;
    }
}

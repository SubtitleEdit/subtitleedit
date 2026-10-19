using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Help.CheckForUpdates;
using Nikse.SubtitleEdit.Features.Shared.PickLanguage;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.UiLogic.Common;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsPage : UserControl
{
    private const double SidePanelMinWidth = 210;
    private const double SidePanelMaxWidth = 280;
    private const string StretchInGroupClass = "stretch-in-group";

    private readonly TextBox _searchBox;
    private readonly StackPanel _contentPanel;
    private readonly SettingsViewModel _vm;
    private readonly Dictionary<SettingsSection, Button> _menuButtons = new();
    private readonly Border _sidePanel;
    private readonly Border _contentBorder;
    private readonly Border _buttonBar;

    public SettingsPage(SettingsViewModel vm)
    {
        _vm = vm;
        _vm.Sections = CreateSections();
        _vm.SelectedSection ??= _vm.Sections.FirstOrDefault();

        _searchBox = new TextBox
        {
            PlaceholderText = Se.Language.Options.Settings.SearchSettingsDotDotDot,
            Margin = new Thickness(10, 12, 10, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            // The placeholder text is not exposed as the UIA Name, so screen readers announce a
            // bare "edit" without this (#12087).
            [AutomationProperties.NameProperty] = Se.Language.Options.Settings.SearchSettingsDotDotDot,
        }.WithSearchAndClearIcons();
        _searchBox.Classes.Add("text");

        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };

        var scrollViewer = new ScrollViewer
        {
            Content = _contentPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        _vm.ScrollView = scrollViewer;
        _contentBorder = EditorChrome.MakePanel(scrollViewer);

        // One menu item per section - built from the sections so title, icon and color always
        // match the section headers in the scroll view (and the Windows-only sections follow).
        var menu = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Margin = new Thickness(8, 0, 8, 10),
        };
        foreach (var section in _vm.Sections)
        {
            var menuItem = MakeMenuItem(section, vm.ScrollToSectionCommand);
            _menuButtons[section] = menuItem;
            menu.Children.Add(menuItem);
        }

        // Side panel: the search box on top, the categories below (they scroll on a short screen)
        var menuScrollViewer = new ScrollViewer
        {
            Content = menu,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var sideGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };
        sideGrid.Add(_searchBox, 0, 0);
        sideGrid.Add(menuScrollViewer, 1, 0);
        _sidePanel = new Border
        {
            Child = sideGrid,
            MinWidth = SidePanelMinWidth,
            MaxWidth = SidePanelMaxWidth,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,*")
        };

        // The side panel (with the search box) first, so it is first in the tab order
        grid.Add(_sidePanel, 0, 0);
        grid.Add(_contentBorder, 0, 1);

        var buttonReset = new SeSplitButton
        {
            Content = Se.Language.General.Reset,
            Command = vm.ResetAllSettingsCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new Avalonia.Controls.MenuItem
                    {
                        Header = Se.Language.General.ImportDotDotDot,
                        Command = vm.ImportSettingsCommand,
                    },
                    new Avalonia.Controls.MenuItem
                    {
                        Header = Se.Language.General.ExportDotDotDot,
                        Command = vm.ExportSettingsCommand,
                    },
                }
            }
        };

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.CommandOkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);

        // Reset (with import/export) on the left, Apply, Cancel and the accent OK on the right
        _buttonBar = SettingsChrome.MakeButtonBar(buttonReset, buttonApply, buttonCancel, buttonOk);
        grid.Add(_buttonBar, 1, 0, 1, 2);

        Content = grid;
        ApplyChromeBrushes();

        UpdateVisibleSections(string.Empty);

        // Search updates immediately, without the category navigation fade.
        _searchBox.TextChanged += (_, e) => UpdateVisibleSections(_searchBox.Text ?? string.Empty);
        ActualThemeVariantChanged += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            // The editor brushes are picked when used - follow a theme changed with Apply.
            if (TopLevel.GetTopLevel(this) is Window window)
            {
                SettingsChrome.Apply(window);
            }

            ApplyChromeBrushes();
            RefreshSections();
        });
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedSection))
            {
                if (_vm.DeferSectionRefresh)
                {
                    UpdateMenuHighlight();
                }
                else
                {
                    RefreshSections();
                }
            }
        };
    }

    private void ApplyChromeBrushes()
    {
        _sidePanel.BorderBrush = EditorChrome.LineBrush;
        _contentBorder.Background = EditorChrome.PanelBrush;
        _buttonBar.Background = EditorChrome.ButtonBarBrush;
        _buttonBar.BorderBrush = EditorChrome.LineBrush;
    }

    public void RefreshSections()
    {
        UpdateVisibleSections(_searchBox.Text ?? string.Empty);
    }

    public void FocusSearchBox()
    {
        _searchBox.Focus();
    }

    private static Button MakeMenuItem(SettingsSection section, IRelayCommand command)
    {
        var label = new TextBlock
        {
            Text = section.Title,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        // Icon on a colored rounded square, matching the section header in the scroll view.
        var image = new ContentControl
        {
            FontSize = 11,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Attached.SetIcon(image, section.IconName);
        // Keep the glyph white on the colored square in the dark theme too (#12717).
        image.Classes.Add(UiTheme.IconOnAccentClassName);
        var glyph = new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(5),
            Background = section.Brush,
            VerticalAlignment = VerticalAlignment.Center,
            Child = image,
        };

        var content = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        content.Add(glyph, 0, 0);
        content.Add(label, 0, 1);

        var link = new Button
        {
            Content = content,
            Command = command,
            CommandParameter = section.Title,
            // The content is an icon + label panel, so the button gets no computed UIA Name and a
            // screen reader would announce a nameless button (#12087).
            [AutomationProperties.NameProperty] = section.Title,
        };
        link.Classes.Add(SettingsChrome.NavClass);

        return link;
    }

    /// <summary>
    /// One category at a time, like SE4: with no search filter only the selected section is in
    /// the content area, so Tab stays within a category and the next Tab after its last setting
    /// reaches the buttons, not the first setting of an unannounced next category (#12087).
    /// A search shows every section with a match, whichever category is selected.
    /// </summary>
    private void UpdateVisibleSections(string filter)
    {
        _contentPanel.Children.Clear();
        var showAllSections = !string.IsNullOrWhiteSpace(filter);

        foreach (var section in _vm.Sections)
        {
            section.Filter(filter);
            if (section.IsVisible && (showAllSections || section == _vm.SelectedSection))
            {
                _contentPanel.Children.Add(section.Build(first: _contentPanel.Children.Count == 0));
            }
            else
            {
                section.Panel = null; // not in the tree - nothing to scroll to or focus
            }
        }

        UpdateMenuHighlight();
    }

    /// <summary>
    /// The selected category is highlighted. While searching it is only when it has a match -
    /// otherwise the first category with a match is (none when nothing matches), so the side
    /// panel does not point at a category that is not in the results.
    /// </summary>
    private void UpdateMenuHighlight()
    {
        var highlighted = _vm.SelectedSection;
        if (!string.IsNullOrWhiteSpace(_searchBox.Text) && highlighted is not { IsVisible: true })
        {
            highlighted = _vm.Sections.FirstOrDefault(section => section.IsVisible);
        }

        foreach (var (section, button) in _menuButtons)
        {
            button.Classes.Set(SettingsChrome.SelectedClass, section == highlighted);
        }
    }

    private static SettingsItem MakeToolbarSetting(ToolbarSettingItem item)
    {
        if (item.ImageName == null)
        {
            return new SettingsItem(item.Name, () => EditorChrome.MakeSwitch(nameof(item.IsVisible), null, item))
            {
                IsFullWidth = true,
                IsSwitch = true,
            };
        }

        return new SettingsItem(item.Name, () =>
        {
            var icon = InitToolbar.MakeImage(item.ImageName);
            icon.Width = 32;
            icon.Height = 32;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;

            var check = new TextBlock
            {
                Text = "✓",
                FontSize = UiUtil.ScaledFontSize(16),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };
            check.Bind(IsVisibleProperty, new Binding(nameof(item.IsVisible)) { Source = item });

            var content = new StackPanel
            {
                Spacing = 6,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    icon,
                    new TextBlock
                    {
                        Text = item.Name,
                        FontSize = UiUtil.ScaledFontSize(12),
                        LineHeight = 15,
                        MaxLines = 2,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        TextAlignment = TextAlignment.Center,
                    },
                },
            };
            var toggle = new ToggleButton
            {
                Width = 120,
                Height = 85,
                Margin = new Thickness(0, 0, 8, 8),
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Content = new Grid { Children = { content, check } },
                [AutomationProperties.NameProperty] = item.Name,
            };
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(toggle, item.Name);
            }

            toggle.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(item.IsVisible))
            {
                Source = item,
                Mode = BindingMode.TwoWay,
            });

            // Tiles in the editor colors: field color with a line, the shown ones in the soft accent
            var accent = EditorChrome.AccentBrush;
            var background = EditorChrome.FieldBrush;
            var selectedBackground = EditorChrome.AccentSoftBrush;
            toggle.BorderThickness = new Thickness(1);
            var lightResources = new ResourceDictionary();
            var darkResources = new ResourceDictionary();
            toggle.Resources.ThemeDictionaries[ThemeVariant.Light] = lightResources;
            toggle.Resources.ThemeDictionaries[ThemeVariant.Dark] = darkResources;
            check.Foreground = accent;
            foreach (var state in new[] { "", "PointerOver", "Pressed" })
            {
                toggle.Resources["ToggleButtonBackground" + state] = state switch
                {
                    "PointerOver" => EditorChrome.HoverBrush,
                    "Pressed" => EditorChrome.PressedBrush,
                    _ => background,
                };
                toggle.Resources["ToggleButtonBackgroundChecked" + state] = selectedBackground;
                lightResources["ToggleButtonForeground" + state] = Brushes.Black;
                lightResources["ToggleButtonForegroundChecked" + state] = Brushes.Black;
                darkResources["ToggleButtonForeground" + state] = Brushes.White;
                darkResources["ToggleButtonForegroundChecked" + state] = Brushes.White;
                toggle.Resources["ToggleButtonBorderBrush" + state] = state == "PointerOver" ? EditorChrome.SeparatorBrush : EditorChrome.LineBrush;
                toggle.Resources["ToggleButtonBorderBrushChecked" + state] = accent;
            }

            return toggle;
        });
    }

    private List<SettingsSection> CreateSections()
    {
        var sections = new List<SettingsSection>();

        sections.Add(new SettingsSection(Se.Language.General.Rules, IconNames.PoliceBadge, "#e0705a",
        [
            new SettingsItem(Se.Language.General.Profiles, () => new StackPanel
            {
                DataContext = _vm,
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    MakeProfileComboBox(),
                    UiUtil.MakeButtonBrowse(_vm.EditProfilesCommand, accessibleName: Se.Language.General.Profiles),
                }
            }),

            new SettingsItem(Se.Language.Options.Settings.SingleLineMaxLength, () => MakeNumericUpDownInt(nameof(_vm.SingleLineMaxLength), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.Options.Settings.OptimalCharsPerSec, () => MakeNumericUpDown(nameof(_vm.OptimalCharsPerSec), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.General.MaxCharsPerSec, () => MakeNumericUpDown(nameof(_vm.MaxCharsPerSec), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.Options.Settings.MaxWordsPerMin, () => MakeNumericUpDown(nameof(_vm.MaxWordsPerMin), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.Options.Settings.MinDurationMs, () => MakeNumericUpDownInt(nameof(_vm.MinDurationMs), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.Options.Settings.MaxDurationMs, () => MakeNumericUpDownInt(nameof(_vm.MaxDurationMs), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.Options.Settings.MinGapMs + " " + Se.Language.Options.Settings.MinGapFrames, _vm, nameof(_vm.MinGapLabel), () => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    MakeNumericUpDownInt(nameof(_vm.MinGapMs), _vm.RuleValueChanged)
                        .WithBindIsVisible(_vm, nameof(_vm.IsMsMode)),
                    UiUtil.MakeButtonBrowse(_vm.CalculateMinGapMsCommand,
                            accessibleName: Se.Language.Options.Settings.MinGapCalculateDotDotDot)
                        .WithBindIsVisible(_vm, nameof(_vm.IsMsMode)),
                    MakeNumericUpDownInt(nameof(_vm.MinGapFrames), _vm.RuleValueChanged)
                        .WithBindIsVisible(_vm, nameof(_vm.UseFrameMode)),
                    new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Opacity = 0.6,
                        [!TextBlock.TextProperty] = new Binding(nameof(_vm.MinGapFramesAsMs)) { Source = _vm },
                    }.WithBindIsVisible(_vm, nameof(_vm.UseFrameMode)),
                }
            })
            {
                EnglishLabel = EnglishLabelLookup.English.Options.Settings.MinGapMs + " " + EnglishLabelLookup.English.Options.Settings.MinGapFrames,
            },
            new SettingsItem(Se.Language.General.MaxNumberOfLines,() => MakeNumericUpDownInt(nameof(_vm.MaxLines), _vm.RuleValueChanged)),
            new SettingsItem(Se.Language.Options.Settings.UnbreakSubtitlesShortThan, () => MakeNumericUpDownInt(nameof(_vm.UnbreakLinesShorterThan), _vm.RuleValueChanged)),

            new SettingsItem(Se.Language.Options.Settings.DialogStyle, () => MakeComboBoxDialogStyle()),
            new SettingsItem(Se.Language.Options.Settings.ContinuationStyle, () => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    MakComboBoxContinuationStyleComboBox(),
                    UiUtil.MakeButtonBrowse(_vm.ShowEditCustomContinuationStyleCommand, accessibleName: Se.Language.Options.Settings.EditContinuationStyleCustom)
                        .WithBindIsVisible(_vm, nameof(_vm.IsEditCustomContinuationStyleVisible))
                }
            }),
            new SettingsItem(Se.Language.Options.Settings.CpsLineLengthStyle, () => MakeComboBoxCpsLineLengthStyle()),
            new SettingsItem(Se.Language.Options.Settings.CpsLineLengthIgnoreTagsRegex, () => UiUtil.MakeEditableComboBox(250, IgnoreTagsRegexPresets, _vm, nameof(_vm.CpsLineLengthIgnoreTagsRegex))),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.General, IconNames.Cogs, "#8494a4",
        [
            MakeNumericSettingInt(Se.Language.Options.Settings.NewEmptyDefaultMs, nameof(_vm.NewEmptyDefaultMs)),
            MakeNumericSettingInt(Se.Language.Options.Settings.TimeCodeUpDownStepMs, nameof(_vm.TimeCodeUpDownStepMs), 1, 5000),
            MakeNumericSettingInt(Se.Language.Options.Settings.MoveSelectedLinesStepMs, nameof(_vm.MoveSelectedLinesStepMs), 1, 60000),
            MakeNumericSettingInt(Se.Language.Options.Settings.MoveStartEndStepMs, nameof(_vm.MoveStartEndStepMs), 1, 5000),
            MakeCheckboxSetting(Se.Language.Options.Settings.MoveLinesShortenNeighbor, nameof(_vm.MoveLinesShortenNeighbor)),
            MakeCheckboxSetting(Se.Language.Options.Settings.PromptBeforeDelete, nameof(_vm.PromptBeforeDelete)),
            new SettingsItem(Se.Language.Options.Settings.TimeCodeMode, () => MakeComboBoxTimeCodeMode()),
            MakeCheckboxSetting(Se.Language.Options.Settings.FrameModeSnapToVideoFrames, nameof(_vm.FrameModeSnapToVideoFrames),
                new Binding(nameof(_vm.IsFramesTimeCodeMode)) { Source = _vm }),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxLimitNewLines, nameof(_vm.TextBoxLimitNewLines)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxAutoWrapWhileTyping, nameof(_vm.TextBoxAutoWrapWhileTyping)),
            MakeCheckboxSetting(Se.Language.General.LockTimeCodes, nameof(_vm.LockTimeCodes)),
            MakeCheckboxSetting(Se.Language.Options.Settings.RememberPositionAndSize, nameof(_vm.RememberPositionAndSize)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TitleBarFullFileName, nameof(_vm.TitleBarFullFileName)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowRecentFiles, nameof(_vm.ShowRecentFiles)),
            MakeCheckboxSetting(Se.Language.Options.Settings.OpenLastFileOnStart, nameof(_vm.OpenLastFileOnStart),
                new Binding(nameof(_vm.ShowRecentFiles)) { Source = _vm }),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoConvertToUtf8, nameof(_vm.AutoConvertToUtf8)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ForceCrLfOnSave, nameof(_vm.ForceCrLfOnSave)),
            new SettingsItem(Se.Language.Options.Settings.FileChangedOutsideAction, () => new ComboBox
            {
                MinWidth = 250,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.FileChangedOutsideActions)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedFileChangedOutsideAction))
                {
                    Mode = BindingMode.TwoWay,
                },
                ItemTemplate = new FuncDataTemplate<FileChangedOutsideActionDisplay>((f, _) =>
                    new TextBlock { Text = f?.Name }, true),
            }),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowFormatLimitWarning, nameof(_vm.ShowFormatLimitWarning)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoTrimWhiteSpace, nameof(_vm.AutoTrimWhiteSpace)),
            MakeCheckboxSetting(Se.Language.Options.Settings.RemoveBlankLinesWhenOpening, nameof(_vm.RemoveBlankLinesWhenOpening)),
            new SettingsItem(!OperatingSystem.IsLinux(), Se.Language.Options.Settings.LinuxClipboardUseExternalTool,
                () => EditorChrome.MakeSwitch(nameof(_vm.LinuxClipboardUseExternalTool), null, _vm)) { IsSwitch = true },
            new SettingsItem(Se.Language.Options.Settings.DefaultEncoding, () => new ComboBox
            {
                Width = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.Encodings)),
                [!SelectingItemsControl.SelectedItemProperty] =
                    new Binding(nameof(_vm.DefaultEncoding)) { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                    new TextBlock { Text = f?.Name }, true)
            }),
            new SettingsItem(Se.Language.Options.Settings.SubtitleGridEnterKeyAction, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SubtitleEnterKeyActionTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSubtitleEnterKeyActionType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),
            new SettingsItem(Se.Language.Options.Settings.SubtitleSingleClickAction, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SubtitleSingleClickActionTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSubtitleSingleClickActionType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),
            new SettingsItem(Se.Language.Options.Settings.SubtitleDoubleClickAction, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SubtitleDoubleClickActionTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSubtitleDoubleClickActionType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),
            MakeCheckboxSetting(Se.Language.Options.Settings.SubtitleGridCenterSelectedRow, nameof(_vm.SubtitleGridCenterSelectedRow)),

            new SettingsItem(Se.Language.Options.Settings.SaveAsBehavior, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SaveAsBehaviorTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSaveAsBehaviorType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),

            new SettingsItem(Se.Language.Options.Settings.SaveAsAppendLanguageCode, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SaveAsAppendLanguageCode)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSaveAsAppendLanguageCode))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),

            new SettingsItem(Se.Language.Options.Settings.DefaultSaveLocation, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.DefaultSaveLocationTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedDefaultSaveLocationType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),

            new SettingsItem(Se.Language.Options.Settings.DefaultSaveLocationCustomFolder, () => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                [!Control.IsEnabledProperty] = new Binding(nameof(_vm.IsDefaultSaveLocationCustomFolderEnabled)) { Source = _vm },
                Children =
                {
                    UiUtil.MakeTextBox(250, _vm, nameof(_vm.DefaultSaveLocationCustomFolder)),
                    UiUtil.MakeButtonBrowse(_vm.BrowseDefaultSaveLocationFolderCommand, accessibleName: Se.Language.Options.Settings.DefaultSaveLocationCustomFolder),
                }
            }),

            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoSave, nameof(_vm.AutoSave)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBackupOn, nameof(_vm.AutoBackupOn)),
            MakeNumericSettingInt(Se.Language.Options.Settings.AutoBackupIntervalMinutes, nameof(_vm.AutoBackupIntervalMinutes), 1),
            MakeNumericSettingInt(Se.Language.Options.Settings.AutoBackupDeleteAfterDays, nameof(_vm.AutoBackupDeleteAfterDays), 1),
            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.SettingsBackupOn, nameof(_vm.SettingsBackupOn)),
            MakeNumericSettingInt(Se.Language.Options.Settings.SettingsBackupIntervalDays, nameof(_vm.SettingsBackupIntervalDays), 0),
            MakeNumericSettingInt(Se.Language.Options.Settings.SettingsBackupMaxCount, nameof(_vm.SettingsBackupMaxCount), 1),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.SubtitleFormats, IconNames.ClosedCaption, "#7fa8f0",
        [
            new SettingsItem(Se.Language.Options.Settings.DefaultFormat, () => new ComboBox
            {
                Width = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.DefaultSubtitleFormats)),
                [!SelectingItemsControl.SelectedItemProperty] =
                    new Binding(nameof(_vm.SelectedDefaultSubtitleFormat)) { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                    new TextBlock { Text = f?.Name }, true)
            }),

            new SettingsItem(Se.Language.Options.Settings.DefaultSaveAsFormat, () => new ComboBox
            {
                Width = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SaveSubtitleFormats)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSaveSubtitleFormat))
                    { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                    new TextBlock { Text = f?.Name }, true)
            }),

            new SettingsItem(Se.Language.Options.Settings.FavoriteSubtitleFormats, () => MakeFavoritesGrid(_vm)),
            new SettingsItem(Se.Language.Options.Settings.FavoriteLanguages, () => MakeLanguageFavoritesGrid(_vm)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WebVttUseXTimestampMap, nameof(_vm.WebVttUseXTimestampMap)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AssaAutoSetResolution, nameof(_vm.AssaAutoSetResolution)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AssaAutoSetResolutionPrompt, nameof(_vm.AssaAutoSetResolutionPrompt),
                new Binding(nameof(_vm.AssaAutoSetResolution)) { Source = _vm }),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.SyntaxColoring, IconNames.Palette, "#d966a0",
        [
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorDurationTooShort, nameof(_vm.ColorDurationTooShort)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorDurationTooLong, nameof(_vm.ColorDurationTooLong)),
            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorTextTooLong, nameof(_vm.ColorTextTooLong)),

            new SettingsItem(Se.Language.Options.Settings.ColorTextTooWide, () => EditorChrome.MakeSwitch(nameof(_vm.ColorTextTooWide), null, _vm))
            {
                IsSwitch = true,
                TrailingFactory = () => UiUtil.MakeButtonBrowse(_vm.EditTextTooWideSettingsCommand, accessibleName: Se.Language.Options.Settings.ColorTextTooWide),
            },

            MakeCheckboxSetting(Se.Language.Options.Settings.ColorTextTooManyLines, nameof(_vm.ColorTextTooManyLines),
                labelBindingPath: nameof(_vm.ColorTextTooManyLinesLabel)),
            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorCharactersPerSecond, nameof(_vm.ColorCharactersPerSecond)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorWordsPerMinute, nameof(_vm.ColorWordsPerMinute)),
            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorOverlap, nameof(_vm.ColorOverlap)),
            MakeSeparator(),
            MakeCheckboxSetting(Se.Language.Options.Settings.ColorGapTooShort, nameof(_vm.ColorGapTooShort)),
            MakeSeparator(),
            new SettingsItem(Se.Language.Options.Settings.CustomErrorRules, () => EditorChrome.MakeSwitch(nameof(_vm.ColorCustomErrorRules), null, _vm))
            {
                IsSwitch = true,
                TrailingFactory = () => UiUtil.MakeButtonBrowse(_vm.EditCustomErrorRulesCommand, accessibleName: Se.Language.Options.Settings.CustomErrorRules),
            },
            MakeSeparator(),
            new SettingsItem(Se.Language.Options.Settings.ErrorBackgroundColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.ErrorColor))),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.VideoPlayer, IconNames.PlayBox, "#a78bfa",
        [
            new SettingsItem(Se.Language.General.VideoPlayer, MakeVideoPlayerComboBox),
            new SettingsItem(Se.Language.Options.Settings.VideoControls,
                () => UiUtil.MakeButton(Se.Language.Options.Settings.VideoControlsEdit, _vm.EditVideoControlsCommand)),
            MakeCheckboxSetting(Se.Language.Options.Settings.FullscreenHideControls, nameof(_vm.FullscreenHideControls)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoOpenVideoFile, nameof(_vm.AutoOpenVideoFile)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowSecondarySubtitleDialog, nameof(_vm.ShowSecondarySubtitleDialog)),
            MakeCheckboxSetting(Se.Language.Options.Settings.RememberSecondarySubtitleFile, nameof(_vm.RememberSecondarySubtitleFile)),
            new SettingsItem(Se.Language.Options.Settings.Deinterlace,
                () => UiUtil.MakeComboBox(_vm.MpvDeinterlaceItems, _vm, nameof(_vm.MpvSelectedDeinterlace))),
            new SettingsItem(Se.Language.Options.Settings.SubtitlePreviewProperties, () => MakeMpvPreviewSettings(_vm)),
            new SettingsItem(!_vm.IsLibMpvDownloadVisible, Se.Language.Options.Settings.DownloadMpv, () => new StackPanel
            {
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        Children =
                        {
                            MakeDownloadButton(Se.Language.Options.Settings.DownloadMpv, _vm.DownloadLibMpvCommand, nameof(_vm.LibMpvStatus)),
                            new TextBlock
                            {
                                DataContext = _vm,
                                [!TextBlock.TextProperty] = new Binding(nameof(_vm.LibMpvStatus)),
                                Margin = new Thickness(0, 0, 0, 0),
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Left,
                            }
                        }
                    },
                    new TextBlock
                    {
                        DataContext = _vm,
                        [!TextBlock.TextProperty] = new Binding(nameof(_vm.LibMpvPath)),
                        Margin = new Thickness(0, 4, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = EditorChrome.FaintTextBrush,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = UiUtil.ScaledFontSize(10),
                    }
                },
            }),
            new SettingsItem(!_vm.IsLibVlcDownloadVisible, Se.Language.Options.Settings.DownloadVlc, () => new StackPanel
            {
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        Children =
                        {
                            MakeDownloadButton(Se.Language.Options.Settings.DownloadVlc, _vm.DownloadLibVlcCommand, nameof(_vm.LibVlcStatus)),
                            new TextBlock
                            {
                                DataContext = _vm,
                                [!TextBlock.TextProperty] = new Binding(nameof(_vm.LibVlcStatus)),
                                Margin = new Thickness(0, 0, 0, 0),
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Left,
                            }
                        }
                    },
                },
            }),
            new SettingsItem(!_vm.IsFfmpegLibsDownloadVisible, Se.Language.Options.Settings.DownloadFfmpegLibs, () => new StackPanel
            {
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        Children =
                        {
                            MakeDownloadButton(Se.Language.Options.Settings.DownloadFfmpegLibs, _vm.DownloadFfmpegLibsCommand, nameof(_vm.FfmpegLibsStatus)),
                            new TextBlock
                            {
                                DataContext = _vm,
                                [!TextBlock.TextProperty] = new Binding(nameof(_vm.FfmpegLibsStatus)),
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Left,
                            }
                        }
                    },
                },
            }),

        ]));

        sections.Add(new SettingsSection(Se.Language.General.WaveformSpectrogram, IconNames.Waveform, "#f0b429",
        [
            new SettingsItem(Se.Language.Options.Settings.WaveformDrawStyle,
                () => UiUtil.MakeComboBox(_vm.WaveformDrawStyles, _vm, nameof(_vm.SelectedWaveformDrawStyle))),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformAutoGenerate, nameof(_vm.WaveformAutoGenerate)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformGenerateSpectrogram, nameof(_vm.WaveformGenerateSpectrogram)),
            new SettingsItem(Se.Language.Options.Settings.WaveformSpectrogramMode,
                () => UiUtil.MakeComboBox(_vm.WaveformSpectrogramStyles, _vm, nameof(_vm.SelectedWaveformSpectrogramStyle))),
            MakeNumericSettingInt(Se.Language.Options.Settings.WaveformSpectrogramCombinedWaveformHeight, nameof(_vm.WaveformSpectrogramCombinedWaveformHeight), 10, 90),

            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformShowToolbar, nameof(_vm.WaveformShowToolbar)),
            new SettingsItem(Se.Language.Options.Settings.WaveformShowToolbarEditLabel,
                () => UiUtil.MakeButton(Se.Language.Options.Settings.WaveformShowToolbarEdit, _vm.EditWaveformToolbarPropertiesCommand)),
            MakeCheckboxSetting(Se.Language.Waveform.ShowOriginalSubtitle, nameof(_vm.WaveformShowOriginalSubtitle)),

            new SettingsItem(Se.Language.Options.Settings.WaveformSingleClickAction, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.WaveformSingleClickActionTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedWaveformSingleClickActionType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),

            new SettingsItem(Se.Language.Options.Settings.WaveformDoubleClickAction, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.WaveformDoubleClickActionTypes)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedWaveformDoubleClickActionType))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),

            new SettingsItem(Se.Language.Options.Settings.WaveformExtractAudioFormat,
                () => UiUtil.MakeComboBox(_vm.WaveformExtractAudioFormats, _vm, nameof(_vm.SelectedWaveformExtractAudioFormat))),
            new SettingsItem(Se.Language.Options.Settings.WaveformExtractAudioSampleRate,
                () => UiUtil.MakeComboBox(_vm.WaveformExtractAudioSampleRates, _vm, nameof(_vm.SelectedWaveformExtractAudioSampleRate))),
            new SettingsItem(Se.Language.Options.Settings.WaveformExtractAudioBitRate,
                () => UiUtil.MakeComboBox(_vm.WaveformExtractAudioBitRates, _vm, nameof(_vm.SelectedWaveformExtractAudioBitRate))),
            // SE 4 parity (#14804): waveform, speech to text and audio clips take only the front
            // center (dialogue) channel when the picked track is 5.1/7.1; other layouts are unaffected.
            MakeCheckboxSetting(Se.Language.Options.Settings.FfmpegUseCenterChannelOnly, nameof(_vm.FfmpegUseCenterChannelOnly)),

            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformRightClickSelectsSubtitle, nameof(_vm.WaveformRightClickSelectsSubtitle)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformAllowOverlap, nameof(_vm.WaveformAllowOverlap)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformSetVideoPositionOnMoveStartEnd, nameof(_vm.WaveformSetVideoPositionOnMoveStartEnd)),
            new SettingsItem(Se.Language.Options.Settings.WaveformSnapToShotChanges, () => EditorChrome.MakeSwitch(nameof(_vm.WaveformSnapToShotChanges), null, _vm))
            {
                IsSwitch = true,
                TrailingFactory = () => UiUtil.MakeButton(_vm.EditBeautifyTimeCodesProfileCommand, IconNames.Cogs, Se.Language.Tools.BeautifyTimeCodesProfile.Title),
            },
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformSnapToFrames, nameof(_vm.WaveformSnapToFrames)),
            new SettingsItem(Se.Language.Options.Settings.WaveformSnapToShotChangesPixels, () => UiUtil.MakeNumericUpDownInt(
                1, 100, 8, 120, _vm, nameof(_vm.WaveformSnapToShotChangesPixels))),
            new SettingsItem(Se.Language.Options.Settings.WaveformSnapToShotChangeStartMaxSeconds, () => UiUtil.MakeNumericUpDownOneDecimal(
                0, 10, 120, _vm, nameof(_vm.WaveformSnapToShotChangeStartMaxSeconds), defaultValue: 1.0m)),
            new SettingsItem(Se.Language.Options.Settings.WaveformSnapToShotChangeEndMaxSeconds, () => UiUtil.MakeNumericUpDownOneDecimal(
                0, 10, 120, _vm, nameof(_vm.WaveformSnapToShotChangeEndMaxSeconds), defaultValue: 1.5m)),
            new SettingsItem(Se.Language.Options.Settings.WaveformSnapToShotChangeSameShotEndMaxSeconds, () => UiUtil.MakeNumericUpDownOneDecimal(
                0, 10, 120, _vm, nameof(_vm.WaveformSnapToShotChangeSameShotEndMaxSeconds), defaultValue: 0.5m)),
            new SettingsItem(Se.Language.Options.Settings.WaveformGuessStartOffsetMs, () => UiUtil.MakeNumericUpDownInt(
                0, 1000, 0, 120, _vm, nameof(_vm.WaveformGuessStartOffsetMs))),
            new SettingsItem(Se.Language.Options.Settings.WaveformGuessEndOffsetMs, () => UiUtil.MakeNumericUpDownInt(
                0, 1000, 0, 120, _vm, nameof(_vm.WaveformGuessEndOffsetMs))),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformShotChangesAutoGenerate, nameof(_vm.WaveformShotChangesAutoGenerate)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformFocusOnMouseOver, nameof(_vm.WaveformFocusOnMouseOver)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformFocusTextboxAfterInsertNew, nameof(_vm.WaveformFocusTextboxAfterInsertNew)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformInvertMouseWheel, nameof(_vm.WaveformInvertMouseWheel)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformMouseWheelSetsVideoPosition, nameof(_vm.WaveformMouseWheelSetsVideoPosition)),
            new SettingsItem(Se.Language.Options.Settings.WaveformMouseWheelVideoPositionStep,
                () => UiUtil.MakeComboBox(_vm.WaveformMouseWheelVideoPositionSteps, _vm, nameof(_vm.SelectedWaveformMouseWheelVideoPositionStep))),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformCenterVideoPositionAlsoWhenPaused, nameof(_vm.WaveformCenterVideoPositionAlsoWhenPaused)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformSelectCurrentSubtitleWhilePaused, nameof(_vm.WaveformSelectCurrentSubtitleWhilePaused)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformRememberZoom, nameof(_vm.WaveformRememberZoom)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformDrawGridLines, nameof(_vm.WaveformDrawGridLines)),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformUseSkiaRenderer, nameof(_vm.WaveformUseSkiaRenderer)),
            // SE 4 parity: the per-paragraph footer in the waveform ("#43  01:10" and the
            // chars/sec line above it) can be turned off - some users find it noisy (#14707).
            // Composed from existing strings so no new translatable text is needed.
            MakeCheckboxSetting($"{Se.Language.General.Show} {Se.Language.General.NumberSymbol} + {Se.Language.General.Duration}", nameof(_vm.WaveformShowNumberAndDuration)),
            MakeCheckboxSetting($"{Se.Language.General.Show} {Se.Language.General.CharsPerSec}", nameof(_vm.WaveformShowCps)),
            new SettingsItem(Se.Language.Options.Settings.WaveformTextFontSize, () => UiUtil.MakeNumericUpDownInt(
                10 ,
                100,
                12,
                120,
                _vm,
                nameof(_vm.WaveformTextFontSize))),
            MakeCheckboxSetting(Se.Language.Options.Settings.WaveformTextFontBold, nameof(_vm.WaveformTextFontBold)),
            new SettingsItem(string.Empty, () => UiUtil.MakeButton(Se.Language.Options.Settings.WaveformColorThemesDotDotDot, _vm.OpenWaveformThemesCommand)),
            new SettingsItem(Se.Language.Options.Settings.WaveformTextColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformTextColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformParagraphBackgroundColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformParagraphBackgroundColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformBackgroundColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformBackgroundColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformParagraphSelectedBackgroundColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformParagraphSelectedBackgroundColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformSelectedColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformSelectedColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformCursorColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformCursorColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformShotChangeColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformShotChangeColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformGridColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformGridColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformParagraphLeftColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformParagraphLeftColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformParagraphRightColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformParagraphRightColor))),
            new SettingsItem(Se.Language.Options.Settings.WaveformFancyHighColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.WaveformFancyHighColor))),
            MakeCheckboxSetting(Se.Language.General.OpenAiCompatibleSttAutoTranscribeOnAudioSelection, nameof(_vm.OpenAiCompatibleSttAutoTranscribeOnAudioSelection)),
            new SettingsItem(Se.Language.Options.Settings.DownloadFfmpeg, () => new StackPanel
            {
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        Children =
                        {
                            MakeDownloadButton(Se.Language.Options.Settings.DownloadFfmpeg, _vm.DownloadFfmpegCommand, nameof(_vm.FfmpegStatus)),
                            new TextBlock
                            {
                                DataContext = _vm,
                                [!TextBlock.TextProperty] = new Binding(nameof(_vm.FfmpegStatus)),
                                Margin = new Thickness(0, 0, 0, 0),
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Left,
                            }
                        }
                    },
                    new TextBlock
                    {
                        DataContext = _vm,
                        [!TextBlock.TextProperty] = new Binding(nameof(_vm.FfmpegPath)),
                        Margin = new Thickness(0, 4, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = EditorChrome.FaintTextBrush,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = UiUtil.ScaledFontSize(10),
                    }
                }
            }),
            new SettingsItem(Se.Language.General.DiskSpace, () => new StackPanel
            {
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
                            UiUtil.MakeLabel().WithBindText(_vm, nameof(_vm.WaveformSpaceInfo)),
                            UiUtil.MakeButton(Se.Language.General.Delete, _vm.EmptyWaveformsAndSpectrogramsCommand).WithLeftAlignment(),
                        }
                    },
                }
            }),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.Tools, IconNames.Tools, "#f0885a",
        [
            MakeCheckboxSetting(Se.Language.Options.Settings.AllowSingleLetterShortcutsInTextbox, nameof(_vm.AllowSingleLetterShortcutsInTextbox)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AllowTextNavigationShortcutsInTextbox, nameof(_vm.AllowTextNavigationShortcutsInTextbox)),
            MakeCheckboxSetting(Se.Language.Options.Settings.GoToLineNumberSetsVideoPosition, nameof(_vm.GoToLineNumberAlsoSetVideoPosition)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AdjustAllTimesRememberLineSelectionChoice, nameof(_vm.AdjustAllTimesRememberLineSelectionChoice)),
            MakeCheckboxSetting(Se.Language.Options.Settings.MergeKeepEndTime, nameof(_vm.MergeKeepEndTime)),
            MakeCheckboxSetting(Se.Language.Options.Settings.MergeKeepEndTimeOnlyAssa, nameof(_vm.MergeKeepEndTimeOnlyAssa),
                new Binding(nameof(_vm.MergeKeepEndTime)) { Source = _vm }),
            MakeCheckboxSetting(Se.Language.Options.Settings.MergeAutoBreak, nameof(_vm.MergeAutoBreak)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBreakLineEndingEarly, nameof(_vm.AutoBreakLineEndingEarly)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBreakCommaBreakEarly, nameof(_vm.AutoBreakCommaBreakEarly)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBreakDashEarly, nameof(_vm.AutoBreakDashEarly)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBreakUsePixelWidth, nameof(_vm.AutoBreakUsePixelWidth)),
            MakeCheckboxSetting(Se.Language.Options.Settings.AutoBreakPreferBottomHeavy, nameof(_vm.AutoBreakPreferBottomHeavy),
                new Binding(nameof(_vm.AutoBreakUsePixelWidth)) { Source = _vm }),
            new SettingsItem(Se.Language.Options.Settings.AutoBreakPreferBottomPercent, () =>
            {
                var nud = MakeNumericUpDownInt(nameof(_vm.AutoBreakPreferBottomPercent), 0, 50);
                // The engine reads the percent only when both pixel width AND prefer-bottom-heavy
                // are on (TextSplit), so enable it only when editing it can have an effect.
                nud[!Control.IsEnabledProperty] = new MultiBinding
                {
                    Converter = BoolConverters.And,
                    Bindings =
                    {
                        new Binding(nameof(_vm.AutoBreakUsePixelWidth)) { Source = _vm },
                        new Binding(nameof(_vm.AutoBreakPreferBottomHeavy)) { Source = _vm },
                    },
                };
                return nud;
            }),
            new SettingsItem(Se.Language.Options.Settings.UseDoNotBreakAfterList, () => EditorChrome.MakeSwitch(nameof(_vm.UseNoLineBreakAfter), null, _vm))
            {
                IsSwitch = true,
                TrailingFactory = () => UiUtil.MakeButton(Se.Language.General.Edit, _vm.EditDoNotBreakAfterListCommand),
            },
            new SettingsItem(Se.Language.Options.Settings.SplitOddLinesAction, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SplitOddNumberOfLinesActions)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSplitOddNumberOfLinesAction))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),
            new SettingsItem(Se.Language.Options.Settings.SpellCheckEngine, () => new ComboBox
            {
                MinWidth = 200,
                DataContext = _vm,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.SpellCheckEngines)),
                [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedSpellCheckEngine))
                {
                    Mode = BindingMode.TwoWay,
                }
            }),
            MakeCheckboxSetting(Se.Language.Options.Settings.SpellCheckEnglishTreatInApostropheAsIng, nameof(_vm.SpellCheckEnglishTreatInApostropheAsIng)),
            MakeCheckboxSetting(Se.Language.Options.Settings.SpellCheckRememberUseAlwaysList, nameof(_vm.SpellCheckRememberUseAlwaysList)),
            MakeCheckboxSetting(Se.Language.Options.Settings.OcrUseWordSplitList, nameof(_vm.OcrUseWordSplitList)),
            MakeCheckboxSetting(Se.Language.Options.Settings.OcrGuessUnknownWords, nameof(_vm.OcrGuessUnknownWords)),
            MakeCheckboxSetting(Se.Language.Options.Settings.SpeechToTextSelectedLinesPromptFirstTimeOnly, nameof(_vm.SpeechToTextSelectedLinesPromptFistTimeOnly)),
            MakeCheckboxSetting(Se.Language.Options.Settings.MultipleReplaceShowDotDotDotButtons, nameof(_vm.MultipleReplaceShowDotDotDotButtons)),
            MakeCheckboxSetting(Se.Language.Options.Settings.GridFocusTextboxAfterInsertNew, nameof(_vm.GridFocusTextboxAfterInsertNew)),
            MakeCheckboxSetting(Se.Language.Options.Settings.UndoRedoGoToChangedLine, nameof(_vm.UndoRedoGoToChangedLine)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextToSpeechPromptMergeContinuationLines, nameof(_vm.TextToSpeechPromptMergeContinuationLines)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextToSpeechPromptSkipNoiseLines, nameof(_vm.TextToSpeechPromptSkipNoiseLines)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextToSpeechPromptDetectSpeakers, nameof(_vm.TextToSpeechPromptDetectSpeakers)),
            MakeCheckboxSetting(Se.Language.Options.Settings.FixCommonErrorsSkipStep1, nameof(_vm.FixCommonErrorsSkipStep1)),
            MakeCheckboxSetting(Se.Language.Options.Settings.FixShortDisplayTimesAllowMoveStartTime, nameof(_vm.FixShortDisplayTimesAllowMoveStartTime)),
            new SettingsItem(Se.Language.Options.Settings.MusicSymbol,
                () => UiUtil.MakeTextBox(120, _vm, nameof(_vm.MusicSymbol))),
            new SettingsItem(Se.Language.Options.Settings.MusicSymbolsToReplace,
                () => UiUtil.MakeTextBox(300, _vm, nameof(_vm.MusicSymbolReplace))),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.Appearance, IconNames.EyeSettings, "#4fc3e8",
        [
            new SettingsItem(Se.Language.General.Theme, () => UiUtil.MakeComboBox(_vm.Themes, _vm, nameof(_vm.SelectedTheme))),
            new SettingsItem(Se.Language.Options.Settings.IconTheme, () => UiUtil.MakeComboBox(_vm.IconThemes, _vm, nameof(_vm.SelectedIconTheme))),
            MakeCheckboxSetting(Se.Language.Options.Settings.MatchIconColorToDarkTheme, nameof(_vm.MatchIconColorToDarkTheme)),
            new SettingsItem(Se.Language.Options.Settings.UiScale, () => UiUtil.MakeNumericUpDownInt(
                (int)Math.Round(UiTheme.MinScale * 100.0, MidpointRounding.AwayFromZero) ,
                (int)Math.Round(UiTheme.MaxScale * 100.0, MidpointRounding.AwayFromZero),
                100,
                120,
                _vm,
                nameof(_vm.LayoutScale))),
            new SettingsItem(Se.Language.Options.Settings.FontScale, () => UiUtil.MakeNumericUpDownInt(
                (int)Math.Round(UiTheme.MinFontScale * 100.0, MidpointRounding.AwayFromZero),
                (int)Math.Round(UiTheme.MaxFontScale * 100.0, MidpointRounding.AwayFromZero),
                100,
                120,
                _vm,
                nameof(_vm.FontScale))),
            new SettingsItem(Se.Language.Options.Settings.TextSelectionOpacity, () => UiUtil.MakeNumericUpDownInt(
                UiTheme.MinTextSelectionOpacity,
                100,
                100,
                120,
                _vm,
                nameof(_vm.TextSelectionOpacity))),
            new SettingsItem(Se.Language.Options.Settings.DarkThemeForegroundColor, () => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    UiUtil.MakeColorPickerButton(_vm, nameof(_vm.DarkModeForegroundColor)),
                    UiUtil.MakeLabel(Se.Language.General.RequiresRestart).WithMarginLeft(5).WithOpacity(0.6),
                }
            }),
            new SettingsItem(Se.Language.Options.Settings.DarkThemeBackgroundColor, () => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    UiUtil.MakeColorPickerButton(_vm, nameof(_vm.DarkModeBackgroundColor)),
                    UiUtil.MakeLabel(Se.Language.General.RequiresRestart).WithMarginLeft(5).WithOpacity(0.6),
                }
            }),
            MakeCheckboxSetting(Se.Language.Options.Settings.UseFocusedButtonBackgroundColor, nameof(_vm.UseFocusedButtonBackgroundColor)),
            new SettingsItem(Se.Language.Options.Settings.FocusedButtonBackgroundColor, () => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    UiUtil.MakeColorPickerButton(_vm, nameof(_vm.FocusedButtonBackgroundColor)),
                    UiUtil.MakeLabel(Se.Language.General.RequiresRestart).WithMarginLeft(5).WithOpacity(0.6),
                }
            }),
            new SettingsItem(Se.Language.Options.Settings.UiFont, () => UiUtil.MakeComboBox(_vm.FontNames, _vm, nameof(_vm.SelectedFontName))),
            new SettingsItem(Se.Language.Options.Settings.SubtitleTextBoxAndGridFontName,
                () => UiUtil.MakeComboBox(_vm.FontNames, _vm, nameof(_vm.SubtitleTextBoxAndGridFontName))),
            MakeNumericSetting(Se.Language.Options.Settings.SubtitleGridFontSize, nameof(_vm.SubtitleGridFontSize)),
            MakeCheckboxSetting(Se.Language.Options.Settings.SubtitleGridTextSingleLine, nameof(_vm.SubtitleGridTextSingleLine)),
            new SettingsItem(Se.Language.Options.Settings.SubtitleGridTextSingleLineSeparator,
                () => UiUtil.MakeTextBox(150, _vm, nameof(_vm.SubtitleGridTextSingleLineSeparator))),
            new SettingsItem(Se.Language.Options.Settings.SubtitleGridTextDisplay, () => UiUtil.MakeComboBox(_vm.SubtitleGridTextDisplayModes, _vm, nameof(_vm.SelectedSubtitleGridTextDisplayMode))),
            new SettingsItem(Se.Language.Options.Settings.SubtitleGridShowFormatting, () => UiUtil.MakeComboBox(_vm.SubtitleGridFormattings, _vm, nameof(_vm.SubtitleGridFormatting))),
            MakeCheckboxSetting(Se.Language.Options.Settings.SubtitleGridLiveSpellCheck, nameof(_vm.SubtitleGridLiveSpellCheck)),
            MakeCheckboxSetting(Se.Language.Options.Settings.SubtitleGridCenterText, nameof(_vm.SubtitleGridCenterText)),
            MakeNumericSetting(Se.Language.Options.Settings.TextBoxFontSize, nameof(_vm.TextBoxFontSize)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxFontBold, nameof(_vm.TextBoxFontBold)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxColorTags, nameof(_vm.TextBoxColorTags)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxLiveSpellCheck, nameof(_vm.TextBoxLiveSpellCheck)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxCenterText, nameof(_vm.TextBoxCenterText)),

            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowAutoBreak, nameof(_vm.TextBoxButtonShowAutoBreak)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowUnbreak, nameof(_vm.TextBoxButtonShowUnbreak)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowItalic, nameof(_vm.TextBoxButtonShowItalic)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowColor, nameof(_vm.TextBoxButtonShowColor)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowRemoveFormatting, nameof(_vm.TextBoxButtonShowRemoveFormatting)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowAiAssistant, nameof(_vm.TextBoxButtonShowAiAssistant)),
            MakeCheckboxSetting(Se.Language.Options.Settings.TextBoxButtonShowRecordSpeech, nameof(_vm.TextBoxButtonShowRecordSpeech)),

            MakeCheckboxSetting(Se.Language.Options.Settings.ShowUpDownStartTime, nameof(_vm.ShowUpDownStartTime)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowUpDownEndTime, nameof(_vm.ShowUpDownEndTime)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowUpDownDuration, nameof(_vm.ShowUpDownDuration)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowUpDownLabels, nameof(_vm.ShowUpDownLabels)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowButtonHints, nameof(_vm.ShowButtonHints)),
            MakeCheckboxSetting(Se.Language.Options.Settings.GridCompactMode, nameof(_vm.GridCompactMode)),
            MakeCheckboxSetting(Se.Language.Options.Settings.GridAlternatingRows, nameof(_vm.GridAlternatingRows)),
            new SettingsItem(Se.Language.Options.Settings.GridAlternatingRowColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.GridAlternatingRowColor))),
            new SettingsItem(Se.Language.Options.Settings.GridAlternatingRowColorDark, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.GridAlternatingRowColorDark))),
            new SettingsItem(Se.Language.Options.Settings.ShowGridLines, () => UiUtil.MakeComboBox(_vm.GridLinesVisibilities, _vm, nameof(_vm.SelectedGridLinesVisibility))),
            new SettingsItem(Se.Language.Options.Settings.BookmarkColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.BookmarkColor))),
            new SettingsItem(Se.Language.Options.Settings.SpellCheckHighlightColor, () => UiUtil.MakeColorPickerButton(_vm, nameof(_vm.SpellCheckHighlightColor))),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowAssaLayer, nameof(_vm.ShowAssaLayer)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowHorizontalLineAboveToolbar, nameof(_vm.ShowHorizontalLineAboveToolbar)),
            MakeCheckboxSetting(Se.Language.Options.Settings.ShowPluginsMenu, nameof(_vm.ShowPluginsMenu)),
        ]));

        sections.Add(new SettingsSection(Se.Language.General.Toolbar, IconNames.DotsHorizontal, "#58c9b4",
            _vm.ToolbarItems.Select(MakeToolbarSetting)) { WrapItems = true });

        sections.Add(new SettingsSection(Se.Language.Options.Settings.Network, IconNames.Network, "#6bb84e",
        [
            new SettingsItem(Se.Language.Options.Settings.ProxyAddress, () => UiUtil.MakeTextBox(250, _vm, nameof(_vm.ProxyAddress))),
            MakeCheckboxSetting(Se.Language.Options.Settings.ProxyUseSystemCredentials, nameof(_vm.ProxyUseDefaultCredentials)),
            new SettingsItem(Se.Language.General.UserName, () => MakeProxyCredentialTextBox(nameof(_vm.ProxyUserName))),
            new SettingsItem(Se.Language.Options.Settings.Password, () => MakeProxyCredentialTextBox(nameof(_vm.ProxyPassword))),
            new SettingsItem(Se.Language.Options.Settings.ProxyDomain, () => MakeProxyCredentialTextBox(nameof(_vm.ProxyDomain))),
            new SettingsItem(Se.Language.Options.Settings.ProxyBypassList, () =>
            {
                var textBox = UiUtil.MakeTextBox(250, _vm, nameof(_vm.ProxyBypassList));
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(textBox, Se.Language.Options.Settings.ProxyBypassListHint);
                }

                return textBox;
            }),
        ]));

        var updateItems = new List<SettingsItem>();
        if (!UpdateCheckService.IsStoreManagedInstall)
        {
            // Store-managed installs (Flatpak) update through the store, so the startup check is hidden there.
            updateItems.Add(MakeCheckboxSetting(Se.Language.Options.Settings.CheckForUpdatesOnStartup, nameof(_vm.CheckForUpdatesOnStartup)));
        }

        updateItems.Add(new SettingsItem(Se.Language.Options.Settings.CheckForUpdatesChannel, () => new ComboBox
        {
            MinWidth = 200,
            DataContext = _vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.UpdateChannels)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedUpdateChannel))
            {
                Mode = BindingMode.TwoWay,
            }
        }));

        sections.Add(new SettingsSection(Se.Language.Options.Settings.Updates, IconNames.CloudDownload, "#d0a24e", updateItems));

        if (_vm.IsFileTypeAssociationsVisible)
        {
            sections.Add(new SettingsSection(Se.Language.Options.Settings.FileTypeAssociations, IconNames.FileCog, "#b98a5a",
            [
                new SettingsItem(string.Empty, () => new ItemsControl
                {
                    DataContext = _vm,
                    // The row has no label, so the list and its check boxes are named here -
                    // NVDA announced "list" and then nameless "check box, checked" (#12087).
                    [AutomationProperties.NameProperty] = Se.Language.Options.Settings.FileTypeAssociations,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.FileTypeAssociations)),
                    ItemTemplate = new FuncDataTemplate<FileTypeAssociationViewModel>((fileType, _) =>
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                new CheckBox
                                {
                                    [AutomationProperties.NameProperty] = fileType.Extension,
                                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FileTypeAssociationViewModel.IsAssociated))
                                    {
                                        Source = fileType, Mode = BindingMode.TwoWay
                                    },
                                },
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(
                                        AssetLoader.Open(new Uri(fileType.IconPath))),
                                    Width = 32,
                                    Height = 32,
                                    Margin = new Thickness(2),
                                },
                                new TextBlock
                                {
                                    Text = fileType.Extension,
                                    VerticalAlignment = VerticalAlignment.Center,
                                }
                            }
                        }, true)
                })
            ]));
        }

        sections.Add(new SettingsSection(Se.Language.Options.Settings.FilesAndLogs, IconNames.FileMultiple, "#5fa8e0",
        [
            new SettingsItem(Se.Language.Options.Settings.ShowErrorLogFile,
                () => UiUtil.MakeLink(Se.GetErrorLogFilePath(), _vm.ShowErrorLogFileCommand).WithBindEnabed(_vm, nameof(_vm.ExistsErrorLogFile)).WithEllipsis()),
            new SettingsItem(Se.Language.Options.Settings.ShowToolsLogFile,
                () => UiUtil.MakeLink(Se.GetToolsLogFilePath(), _vm.ShowToolsLogFileCommand).WithBindEnabed(_vm, nameof(_vm.ExistsToolsLogFile)).WithEllipsis()),
            new SettingsItem(Se.Language.Options.Settings.WriteToolsLog,
                () => EditorChrome.MakeSwitch(nameof(_vm.WriteToolsLog), null, _vm)) { IsSwitch = true },
            new SettingsItem(Se.Language.Options.Settings.ShowSettingsFile,
                () => UiUtil.MakeLink(Se.GetSettingsFilePath(), _vm.ShowSettingsFileCommand).WithBindEnabed(_vm, nameof(_vm.ExistsSettingsFile)).WithEllipsis()),
        ]));


        return sections;
    }

    private Grid MakeFavoritesGrid(SettingsViewModel vm)
    {
        // List of favorite formats, with buttons to add/remove/move-up/move-down
        var listBox = new ListBox
        {
            DataContext = vm,
            [AutomationProperties.NameProperty] = Se.Language.Options.Settings.FavoriteSubtitleFormats,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.FavoriteSubtitleFormats)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedFavoriteSubtitleFormat)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<string>((f, _) =>
                new TextBlock { Text = f }, true)
        };

        return MakeFavoritesList(listBox, Se.Language.Options.Settings.FavoriteSubtitleFormats,
            vm.AddFavoriteSubtitleFormatCommand,
            vm.RemoveFavoriteSubtitleFormatCommand,
            vm.MoveUpFavoriteSubtitleFormatCommand,
            vm.MoveDownFavoriteSubtitleFormatCommand);
    }

    private Grid MakeLanguageFavoritesGrid(SettingsViewModel vm)
    {
        // List of favorite languages, with buttons to add/remove/move-up/move-down
        var listBox = new ListBox
        {
            DataContext = vm,
            [AutomationProperties.NameProperty] = Se.Language.Options.Settings.FavoriteLanguages,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.FavoriteLanguages)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedFavoriteLanguage)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<PickLanguageDisplay>((l, _) =>
                new TextBlock { Text = l?.ToString() }, true)
        };

        return MakeFavoritesList(listBox, Se.Language.Options.Settings.FavoriteLanguages,
            vm.AddFavoriteLanguageCommand,
            vm.RemoveFavoriteLanguageCommand,
            vm.MoveUpFavoriteLanguageCommand,
            vm.MoveDownFavoriteLanguageCommand);
    }

    /// <summary>
    /// A favorites list in a field frame with a column of icon buttons beside it.
    /// </summary>
    private static Grid MakeFavoritesList(ListBox listBox, string listName, IRelayCommand add, IRelayCommand remove, IRelayCommand moveUp, IRelayCommand moveDown)
    {
        listBox.Height = 200;
        listBox.Background = Brushes.Transparent;

        var buttonAdd = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Add, add, 28);
        var buttonRemove = EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Remove, remove, 28);
        var buttonMoveUp = EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, moveUp, 28);
        var buttonMoveDown = EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, moveDown, 28);
        NameListButtons(listName,
            (buttonAdd, Se.Language.General.Add),
            (buttonRemove, Se.Language.General.Remove),
            (buttonMoveUp, Se.Language.General.MoveUp),
            (buttonMoveDown, Se.Language.General.MoveDown));

        var buttonStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children = { buttonAdd, buttonRemove, buttonMoveUp, buttonMoveDown }
        };

        // The list is 250 wide where there is room and shrinks with a narrow window, the
        // buttons stay beside it
        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MaxWidth = 250 },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(EditorChrome.MakeListFrame(listBox), 0, 0);
        grid.Add(buttonStack, 0, 1);

        return grid;
    }

    /// <summary>
    /// Names the Add/Remove/Move buttons next to a list after that list ("Favorite languages:
    /// Add"). An empty list is not a tab stop, so a screen reader user tabbing from one favorites
    /// list to the next heard a second "Add, Remove, Move up, Move down" with nothing saying
    /// which list it belonged to (#12087).
    /// </summary>
    private static void NameListButtons(string listName, params (Button Button, string Text)[] buttons)
    {
        foreach (var (button, text) in buttons)
        {
            AutomationProperties.SetName(button, $"{listName}: {text}");
        }
    }

    /// <summary>
    /// A "Download" button named after its setting ("Download mpv") with the install status
    /// ("Not installed", a version) as its description - the caption alone told a screen reader
    /// user nothing about what would be downloaded (#12087).
    /// </summary>
    private Button MakeDownloadButton(string settingName, IRelayCommand command, string statusProperty)
    {
        var button = UiUtil.MakeButton(Se.Language.General.Download, command);
        AutomationProperties.SetName(button, settingName);
        button.Bind(AutomationProperties.HelpTextProperty, new Binding(statusProperty) { Source = _vm });
        return button;
    }

    private Control MakeMpvPreviewSettings(SettingsViewModel vm)
    {
        var labelFontName = SettingsChrome.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.Fonts, vm, nameof(vm.MpvPreviewFontName)).WithMinWidth(150).WithLabeledBy(labelFontName);
        comboBoxFontName.Classes.Add(StretchInGroupClass);

        var labelFontSize = SettingsChrome.MakeLabel(Se.Language.General.FontSize);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.MpvPreviewFontSize)).WithLabeledBy(labelFontSize);
        numericUpDownFontSize.Increment = 1;

        var switchBold = SettingsChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.MpvPreviewFontBold), null, vm), Se.Language.General.Bold);

        var labelAlignment = SettingsChrome.MakeLabel(Se.Language.General.Alignment);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.MpvPreviewFontAlignments, vm, nameof(vm.MpvPreviewSelectedFontAlignment)).WithLabeledBy(labelAlignment);
        comboBoxAlignment.Classes.Add(StretchInGroupClass);

        var labelJustify = SettingsChrome.MakeLabel(Se.Language.Options.Settings.TextJustify);
        var comboBoxJustify = UiUtil.MakeComboBox(vm.MpvPreviewJustifyItems, vm, nameof(vm.MpvPreviewSelectedJustify)).WithLabeledBy(labelJustify);
        comboBoxJustify.Classes.Add(StretchInGroupClass);

        var switchUsePositionFromFile = SettingsChrome.MakeSwitchRow(
            EditorChrome.MakeSwitch(nameof(vm.MpvPreviewUsePositionFromFile), null, vm), Se.Language.Options.Settings.UsePositionFromSubtitleFile);

        var labelMargin = SettingsChrome.MakeLabel(Se.Language.General.Margin);
        var numericUpDownMargin = UiUtil.MakeNumericUpDownOneDecimal(0, 1000, 130, vm, nameof(vm.MpvPreviewMargin)).WithLabeledBy(labelMargin);
        numericUpDownMargin.Increment = 1;

        var switchMarginIsPartOfSubtitleArea = SettingsChrome.MakeSwitchRow(
            EditorChrome.MakeSwitch(nameof(vm.MpvPreviewMarginIsPartOfSubtitleArea), null, vm), Se.Language.Options.Settings.MarginIsPartOfSubtitleArea);

        var labelColorPrimary = SettingsChrome.MakeLabel(Se.Language.Assa.Primary);
        var colorPickerPrimary = UiUtil.MakeColorPickerButton(vm, nameof(vm.MpvPreviewColorPrimary));

        var labelColorOutline = SettingsChrome.MakeLabel(Se.Language.General.Outline);
        var colorPickerOutline = UiUtil.MakeColorPickerButton(vm, nameof(vm.MpvPreviewColorOutline));

        var labelColorShadow = SettingsChrome.MakeLabel(Se.Language.General.Shadow);
        var colorPickerShadow = UiUtil.MakeColorPickerButton(vm, nameof(vm.MpvPreviewColorShadow));

        var labelBorderStyle = SettingsChrome.MakeLabel(Se.Language.General.BorderStyle);
        var comboBoxBorderType = UiUtil.MakeComboBox(vm.MpvPreviewBorderTypes, vm, nameof(vm.MpvPreviewSelectedBorderType)).WithLabeledBy(labelBorderStyle);
        comboBoxBorderType.Classes.Add(StretchInGroupClass);

        var labelOutlineWidth = SettingsChrome.MakeLabel(Se.Language.General.OutlineWidth);
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 130, vm, nameof(vm.MpvPreviewOutlineWidth)).WithLabeledBy(labelOutlineWidth);
        numericUpDownOutlineWidth.Increment = 0.5m;

        var labelShadowWidth = SettingsChrome.MakeLabel(Se.Language.General.ShadowWidth);
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 130, vm, nameof(vm.MpvPreviewShadowWidth)).WithLabeledBy(labelShadowWidth);
        numericUpDownShadowWidth.Increment = 0.5m;

        // The fields take the rest of the group (a star column), so on a narrow window the combo
        // boxes shrink with it instead of being cut off at the right edge.
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 12,
            RowSpacing = 6,
        };
        for (var i = 0; i < 14; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        grid.Add(labelFontName, 0);
        grid.Add(comboBoxFontName, 0, 1);

        grid.Add(labelFontSize, 1);
        grid.Add(numericUpDownFontSize, 1, 1);

        grid.Add(switchBold, 2, 1);

        grid.Add(labelAlignment, 3);
        grid.Add(comboBoxAlignment, 3, 1);

        grid.Add(labelJustify, 4);
        grid.Add(comboBoxJustify, 4, 1);

        grid.Add(switchUsePositionFromFile, 5, 0, 1, 2);

        grid.Add(labelMargin, 6);
        grid.Add(numericUpDownMargin, 6, 1);

        grid.Add(switchMarginIsPartOfSubtitleArea, 7, 0, 1, 2);

        grid.Add(labelColorPrimary, 8);
        grid.Add(colorPickerPrimary, 8, 1);

        grid.Add(labelColorOutline, 9);
        grid.Add(colorPickerOutline, 9, 1);

        grid.Add(labelColorShadow, 10);
        grid.Add(colorPickerShadow, 10, 1);

        grid.Add(labelBorderStyle, 11);
        grid.Add(comboBoxBorderType, 11, 1);

        grid.Add(labelOutlineWidth, 12);
        grid.Add(numericUpDownOutlineWidth, 12, 1);

        grid.Add(labelShadowWidth, 13);
        grid.Add(numericUpDownShadowWidth, 13, 1);

        foreach (var child in grid.Children)
        {
            if (child is not Grid)
            {
                // The combo boxes as wide as the widest of them, the rest as wide as they are
                if (child.Classes.Contains(StretchInGroupClass))
                {
                    child.MinWidth = 0;
                    child.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
                else
                {
                    child.HorizontalAlignment = HorizontalAlignment.Left;
                }

                child.VerticalAlignment = VerticalAlignment.Center;
            }
        }

        return SettingsChrome.MakeGroupFrame(grid);
    }

    private ComboBox MakeVideoPlayerComboBox()
    {
        var cb = new ComboBox
        {
            Width = 300,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.VideoPlayers)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(_vm.SelectedVideoPlayer)),
            DataContext = _vm,
            ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(VideoPlayerItem.Name)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                }, true)
        };

        cb.SelectionChanged += (s, e) => _vm.VideoPlayerChanged();

        return cb;
    }

    /// <summary>
    /// Starting points for the "ignore custom tags" pattern (#16163): a client platform's own
    /// markup like &lt;=title&gt;, &lt;@&gt; or &lt;?note&gt;, and every angle-bracket tag SE
    /// does not strip itself. The field stays free text, so any other pattern can be typed.
    /// </summary>
    private static readonly string[] IgnoreTagsRegexPresets =
    [
        string.Empty,
        "<[=?@][^<>]*>",
        @"<[^\s<>][^<>]*>",
    ];

    private ComboBox MakeComboBoxCpsLineLengthStyle()
    {
        var comboBoxCpsLineLengthStyle = new ComboBox
        {
            Width = 250,
            DataContext = _vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.CpsLineLengthStrategies)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(_vm.CpsLineLengthStrategy)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };
        comboBoxCpsLineLengthStyle.SelectionChanged += (s, e) => _vm.RuleValueChanged();
        return comboBoxCpsLineLengthStyle;
    }

    private ComboBox MakeComboBoxTimeCodeMode()
    {
        return new ComboBox
        {
            Width = 250,
            DataContext = _vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.TimeCodeModes)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(_vm.SelectedTimeCodeMode)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<TimeCodeModeDisplay>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };
    }

    private ComboBox MakeComboBoxDialogStyle()
    {
        var comboBoxDialogStyle = new ComboBox
        {
            Width = 250,
            DataContext = _vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.DialogStyles)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(_vm.DialogStyle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };
        comboBoxDialogStyle.SelectionChanged += (s, e) => _vm.RuleValueChanged();
        return comboBoxDialogStyle;
    }

    private ComboBox MakeProfileComboBox()
    {
        var comboBoxProfile = new ComboBox
        {
            Width = 250,
            DataContext = _vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.Profiles)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(_vm.SelectedProfile)) { Mode = BindingMode.TwoWay }
        };
        comboBoxProfile.SelectionChanged += (s, e) => _vm.ProfileChanged();
        return comboBoxProfile;
    }

    private ComboBox MakComboBoxContinuationStyleComboBox()
    {
        var comboBoxContinuationStyle = new ComboBox
        {
            Width = 250,
            DataContext = _vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.ContinuationStyles)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(_vm.ContinuationStyle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };
        comboBoxContinuationStyle.SelectionChanged += (s, e) => _vm.ContinuationStyleChanged();

        return comboBoxContinuationStyle;
    }

    private static SettingsItem MakeSeparator()
    {
        return new SettingsItem(string.Empty, () => new Label()) { IsSeparator = true };
    }

    private SettingsItem MakeNumericSetting(string label, string bindingProperty)
    {
        return new SettingsItem(label, () => MakeNumericUpDown(bindingProperty));
    }

    private NumericUpDown MakeNumericUpDown(string bindingProperty)
    {
        return new NumericUpDown
        {
            Width = 150,
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay },
        };
    }

    private SettingsItem MakeNumericSettingInt(string label, string bindingProperty, int? minValue = null, int? maxValue = null)
    {
        return new SettingsItem(label, () => MakeNumericUpDownInt(bindingProperty, minValue, maxValue));
    }

    private NumericUpDown MakeNumericUpDownInt(string bindingProperty, int? minValue = null, int? maxValue = null)
    {
        var nud = new NumericUpDown
        {
            Width = 150,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay },
        };

        if (minValue.HasValue)
        {
            nud.Minimum = minValue.Value;
        }

        if (maxValue.HasValue)
        {
            nud.Maximum = maxValue.Value;
        }

        return nud;
    }

    private NumericUpDown MakeNumericUpDownInt(string bindingProperty, Action valueChanged)
    {
        var numericUpDown = new NumericUpDown
        {
            Width = 150,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay },
        };

        numericUpDown.ValueChanged += (s, e) => valueChanged.Invoke();
        return numericUpDown;
    }

    private TextBox MakeProxyCredentialTextBox(string bindingProperty)
    {
        var textBox = UiUtil.MakeTextBox(250, _vm, bindingProperty);
        textBox[!Control.IsEnabledProperty] = new Binding(nameof(_vm.ProxyUseDefaultCredentials))
        {
            Source = _vm,
            Converter = InverseBooleanConverter.Instance,
        };

        return textBox;
    }

    private NumericUpDown MakeNumericUpDown(string bindingProperty, Action valueChanged)
    {
        var numericUpDown = new NumericUpDown
        {
            Width = 150,
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = _vm, Mode = BindingMode.TwoWay },
        };

        numericUpDown.ValueChanged += (s, e) => valueChanged.Invoke();
        return numericUpDown;
    }

    private SettingsItem MakeCheckboxSetting(string label, string bindingProperty, Binding? bindingEnabled = null, string? labelBindingPath = null)
    {
        Control Factory()
        {
            var cb = EditorChrome.MakeSwitch(bindingProperty, null, _vm);

            if (bindingEnabled != null)
            {
                cb[!Control.IsEnabledProperty] = bindingEnabled;
            }

            return cb;
        }

        return labelBindingPath != null
            ? new SettingsItem(label, _vm, labelBindingPath, Factory) { IsSwitch = true }
            : new SettingsItem(label, Factory) { IsSwitch = true };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Dispatcher.UIThread.Invoke(() =>
        {
            _searchBox.Focus(); // hack to make OnKeyDown work
        });
    }
}

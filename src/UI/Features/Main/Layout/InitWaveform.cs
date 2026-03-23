using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class InitWaveform
{
    public class SortedControl
    {
        public int Sort { get; set; }
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }
        public Control? Control { get; set; }
    }

    public static Grid MakeWaveform(MainViewModel vm)
    {
        var languageHints = Se.Language.Main.Waveform;
        var settings = Se.Settings.Waveform;
        var shortcuts = ShortcutsMain.GetUsedShortcuts(vm);

        // Create main layout grid
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(10, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN, // Auto height
        };

        // waveform area
        if (vm.AudioVisualizer == null)
        {
            vm.AudioVisualizer = new AudioVisualizer
            {
                DrawGridLines = settings.DrawGridLines,
                WaveformColor = settings.WaveformColor.FromHexToColor(),
                WaveformBackgroundColor = settings.WaveformBackgroundColor.FromHexToColor(),
                WaveformSelectedColor = settings.WaveformSelectedColor.FromHexToColor(),
                WaveformCursorColor = settings.WaveformCursorColor.FromHexToColor(),
                WaveformParagraphLeftColor = settings.WaveformParagraphLeftColor.FromHexToColor(),
                WaveformParagraphRightColor = settings.WaveformParagraphRightColor.FromHexToColor(),
                WaveformFancyHighColor = settings.WaveformFancyHighColor.FromHexToColor(),
                ParagraphBackground = settings.ParagraphBackground.FromHexToColor(),
                ParagraphSelectedBackground = settings.ParagraphSelectedBackground.FromHexToColor(),
                InvertMouseWheel = settings.InvertMouseWheel,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = double.NaN, // Auto height
                WaveformDrawStyle = GetWaveformDrawStyle(settings.WaveformDrawStyle),
                MinGapSeconds = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0,
                FocusOnMouseOver = settings.FocusOnMouseOver,
                IsReadOnly = Se.Settings.General.LockTimeCodes,
                WaveformHeightPercentage = settings.SpectrogramCombinedWaveformHeight,
            };
            vm.AudioVisualizer.OnNewSelectionInsert += vm.AudioVisualizerOnNewSelectionInsert;
            vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerOnVideoPositionChanged;
            vm.AudioVisualizer.OnToggleSelection += vm.AudioVisualizerOnToggleSelection;
            //vm.AudioVisualizer.OnParagraphDoubleTapped += vm.OnWaveformDoubleTapped;
            vm.AudioVisualizer.OnPrimarySingleClicked += vm.AudioVisualizerOnPrimarySingleClicked;
            vm.AudioVisualizer.OnPrimaryDoubleClicked += vm.AudioVisualizerOnPrimaryDoubleClicked;
            vm.AudioVisualizer.OnDeletePressed += vm.AudioVisualizerOnDeletePressed;
            vm.AudioVisualizer.PointerReleased += vm.ControlMacPointerReleased;
            vm.AudioVisualizer.OnSelectRequested += vm.AudioVisualizerSelectRequested;
            vm.AudioVisualizer.OnSetStartAndOffsetTheRest += vm.AudioVisualizerSetStartAndOffsetTheRest;

            // Create a Flyout for the DataGrid
            var flyout = new MenuFlyout();
            vm.AudioVisualizer.FlyoutMenuOpening += vm.AudioVisualizerFlyoutMenuOpening;

            var insertSelectionMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertNewSelection,
                Command = vm.WaveformInsertNewSelectionCommand,
            };
            flyout.Items.Add(insertSelectionMenuItem);
            vm.MenuItemAudioVisualizerInsertNewSelection = insertSelectionMenuItem;

            var pasteSelectionMenuItem = new MenuItem
            {
                Header = Se.Language.General.PasteNewSelection,
                Command = vm.WaveformNewSelectionPasteFromClipboardCommand,
            };
            flyout.Items.Add(pasteSelectionMenuItem);
            vm.MenuItemAudioVisualizerPasteNewSelection = pasteSelectionMenuItem;

            var insertNewMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertAtPositionAndFocusTextBox,
                Command = vm.WaveformInsertAtPositionAndFocusTextBoxCommand,
            };
            flyout.Items.Add(insertNewMenuItem);
            vm.MenuItemAudioVisualizerInsertAtPosition = insertNewMenuItem;

            var pasteFromClipboardMenuItem = new MenuItem
            {
                Header = Se.Language.General.WaveformPasteFromClipboard,
                Command = vm.WaveformNewSelectionPasteFromClipboardCommand,
            };
            flyout.Items.Add(pasteFromClipboardMenuItem);
            vm.MenuItemAudioVisualizerPasteFromClipboardMenuItem = pasteFromClipboardMenuItem;

            var insertSubtitleFileAtPositionMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertSubtitleFileAtVideoPositionDotDotDot,
                Command = vm.InsertSubtitleFileAtVideoPositionCommand,
            };
            flyout.Items.Add(insertSubtitleFileAtPositionMenuItem);
            vm.MenuIteminsertSubtitleFileAtPositionMenuItem = insertSubtitleFileAtPositionMenuItem;

            var deleteAtPositionMenuItem = new MenuItem
            {
                Header = Se.Language.General.DeleteAtPosition,
                Command = vm.WaveformDeleteAtPositionCommand,
            };
            flyout.Items.Add(deleteAtPositionMenuItem);
            vm.MenuItemAudioVisualizerDeleteAtPosition = deleteAtPositionMenuItem;

            // Add menu items with commands
            var deleteMenuItem = new MenuItem
            {
                Header = Se.Language.General.Delete,
                Command = vm.DeleteSelectedLinesCommand
            };
            flyout.Items.Add(deleteMenuItem);
            vm.MenuItemAudioVisualizerDelete = deleteMenuItem;

            var insertBeforeMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertBefore,
                Command = vm.InsertLineBeforeCommand
            };
            flyout.Items.Add(insertBeforeMenuItem);
            vm.MenuItemAudioVisualizerInsertBefore = insertBeforeMenuItem;

            var insertAfterMenuItem = new MenuItem
            {
                Header = Se.Language.General.InsertAfter,
                Command = vm.InsertLineAfterCommand
            };
            flyout.Items.Add(insertAfterMenuItem);
            vm.MenuItemAudioVisualizerInsertAfter = insertAfterMenuItem;

            var separator1 = new Separator();
            flyout.Items.Add(separator1);
            vm.MenuItemAudioVisualizerSeparator1 = separator1;

            var splitMenuItem = new MenuItem
            {
                Header = Se.Language.General.SplitLine,
                Command = vm.SplitInWaveformCommand,
            };
            flyout.Items.Add(splitMenuItem);
            vm.MenuItemAudioVisualizerSplit = splitMenuItem;

            var splitAtPositionMenuItem = new MenuItem
            {
                Header = Se.Language.General.SplitLine,
                Command = vm.SplitAtPositionInWaveformCommand,
            };
            flyout.Items.Add(splitAtPositionMenuItem);
            vm.MenuItemAudioVisualizerSplitAtPosition = splitAtPositionMenuItem;

            var MergeWithPreviousMenuItem = new MenuItem
            {
                Header = Se.Language.General.MergeBefore,
                Command = vm.MergeWithLineBeforeCommand,
            };
            flyout.Items.Add(MergeWithPreviousMenuItem);
            vm.MenuItemAudioVisualizerMergeWithPrevious = MergeWithPreviousMenuItem;

            var MergeWithNextMenuItem = new MenuItem
            {
                Header = Se.Language.General.MergeAfter,
                Command = vm.MergeWithLineAfterCommand,
            };
            flyout.Items.Add(MergeWithNextMenuItem);
            vm.MenuItemAudioVisualizerMergeWithNext = MergeWithNextMenuItem;

            flyout.Items.Add(new Separator());

            var menuItemFilterByLayer = new MenuItem
            {
                Header = Se.Language.General.FilterByLayer,
                Command = vm.ShowPickLayerFilterCommand,
            }.BindIsVisible(vm, nameof(vm.IsFormatAssa));
            flyout.Items.Add(menuItemFilterByLayer);

            var menuItemGuessTimeCodes = new MenuItem
            {
                Header = Se.Language.Waveform.GuessTimeCodesDotDotDot,
                Command = vm.ShowWaveformGuessTimeCodesCommand,
            };
            flyout.Items.Add(menuItemGuessTimeCodes);

            var menuItemAddShotChange = new MenuItem
            {
                Header = Se.Language.Waveform.ToggleShotChange,
                Command = vm.ToggleShotChangesAtVideoPositionCommand,
            };
            flyout.Items.Add(menuItemAddShotChange);

            var menuItemSeekSilence = new MenuItem
            {
                Header = Se.Language.Waveform.SeekSilenceDotDotDot,
                Command = vm.ShowWaveformSeekSilenceCommand,
            };
            flyout.Items.Add(menuItemSeekSilence);

            var menuItemSpeechToTextSelectedLines = new MenuItem
            {
                Header = Se.Language.Waveform.SpeechToTextSelectedLinesDotDotDot,
                Command = vm.SpeechToTextSelectedLinesCommand,
            };
            flyout.Items.Add(menuItemSpeechToTextSelectedLines);
            vm.MenuItemAudioVisualizerSpeechToTextSelectedLines = menuItemSpeechToTextSelectedLines;

            var separatorDisplayMode = new Separator();
            separatorDisplayMode.DataContext = vm;
            separatorDisplayMode.Bind(Separator.IsVisibleProperty, new Binding(nameof(vm.ShowWaveformDisplayModeSeparator)));
            flyout.Items.Add(separatorDisplayMode);

            var showOnlyWaveformMenuItem = new MenuItem
            {
                Header = Se.Language.Waveform.ShowOnlyWaveform,
                Command = vm.WaveformShowOnlyWaveformCommand,
            }.BindIsVisible(vm, nameof(vm.ShowWaveformOnlyWaveform));
            flyout.Items.Add(showOnlyWaveformMenuItem);

            var showOnlySpectrogramMenuItem = new MenuItem
            {
                Header = Se.Language.Waveform.ShowOnlySpectrogram,
                Command = vm.WaveformShowOnlySpectrogramCommand,
            }.BindIsVisible(vm, nameof(vm.ShowWaveformOnlySpectrogram));
            flyout.Items.Add(showOnlySpectrogramMenuItem);

            var showWaveformAndSpectrogramMenuItem = new MenuItem
            {
                Header = Se.Language.Waveform.ShowWaveformAndSpectrogram,
                Command = vm.WaveformShowWaveformAndSpectrogramCommand,
            }.BindIsVisible(vm, nameof(vm.ShowWaveformWaveformAndSpectrogram));
            flyout.Items.Add(showWaveformAndSpectrogramMenuItem);

            vm.AudioVisualizer.MenuFlyout = flyout;
        }
        else
        {
            vm.AudioVisualizer.RemoveControlFromParent();
        }

        Grid.SetRow(vm.AudioVisualizer, 0);
        mainGrid.Children.Add(vm.AudioVisualizer);

        // Footer
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        controlsPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsWaveformToolbarVisible)));

        var settingPlay = GetToolbarSettingFor(SeWaveformToolbarItemType.Play);
        var buttonPlay = new Button
        {
            Margin = new Thickness(settingPlay.LeftMargin, 0, settingPlay.RightMargin, 0),
            FontSize = settingPlay.FontSize,
            Command = vm.TogglePlayPauseCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PlayPauseHint, shortcuts, nameof(vm.TogglePlayPauseCommand)),
        };
        Attached.SetIcon(buttonPlay, IconNames.Play);
        vm.ButtonWaveformPlay = buttonPlay;

        var settingPlaySelection = GetToolbarSettingFor(SeWaveformToolbarItemType.PlaySelection);
        var buttonPlaySelectedLines = new Button
        {
            Margin = new Thickness(settingPlaySelection.LeftMargin, 0, settingPlaySelection.RightMargin, 0),
            FontSize = settingPlaySelection.FontSize,
            Command = vm.PlaySelectedLinesWithoutLoopCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PlaySelectionHint, shortcuts, nameof(vm.PlaySelectedLinesWithoutLoopCommand)),
        };
        Attached.SetIcon(buttonPlaySelectedLines, IconNames.PlayPlaylist);

        var settingSelectionRepeat = GetToolbarSettingFor(SeWaveformToolbarItemType.Repeat);
        var buttonPlaySelectedLinesRepeat = new Button
        {
            Margin = new Thickness(settingSelectionRepeat.LeftMargin, 0, settingSelectionRepeat.RightMargin, 0),
            FontSize = settingSelectionRepeat.FontSize,
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            Command = vm.PlaySelectedLinesWithLoopCommand,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PlaySelectedRepeatHint, shortcuts, nameof(vm.PlaySelectedLinesWithLoopCommand)),
        };
        Attached.SetIcon(buttonPlaySelectedLinesRepeat, IconNames.Refresh);

        var settingPlayNext = GetToolbarSettingFor(SeWaveformToolbarItemType.PlayNext);
        var buttonPlayNext = new Button
        {
            Margin = new Thickness(settingPlayNext.LeftMargin, 0, settingPlayNext.RightMargin, 0),
            FontSize = settingPlayNext.FontSize,
            Command = vm.PlayNextCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PlayNextHint, shortcuts, nameof(vm.PlayNextCommand)),
        };
        Attached.SetIcon(buttonPlayNext, IconNames.SkipNext);

        var settingPlayNew = GetToolbarSettingFor(SeWaveformToolbarItemType.New);
        var buttonNew = new Button
        {
            Margin = new Thickness(settingPlayNew.LeftMargin, 0, settingPlayNew.RightMargin, 0),
            FontSize = settingPlayNew.FontSize,
            Command = vm.WaveformInsertAtPositionAndFocusTextBoxCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.NewHint, shortcuts, nameof(vm.WaveformInsertAtPositionAndFocusTextBoxCommand)),
        };
        Attached.SetIcon(buttonNew, IconNames.Plus);

        var settingOffsetTheRest = GetToolbarSettingFor(SeWaveformToolbarItemType.SetStartAndOffsetTheRest);
        var buttonSetStartAndOffsetTheRest = new Button
        {
            Margin = new Thickness(settingOffsetTheRest.LeftMargin, 0, settingOffsetTheRest.RightMargin, 0),
            FontSize = settingOffsetTheRest.FontSize,
            Command = vm.WaveformSetStartAndOffsetTheRestCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetStartAndOffsetTheRestHint, shortcuts, nameof(vm.WaveformSetStartAndOffsetTheRestCommand)),
        };
        Attached.SetIcon(buttonSetStartAndOffsetTheRest, IconNames.ArrowExpandRight);

        var settingSetStart = GetToolbarSettingFor(SeWaveformToolbarItemType.SetStart);
        var buttonSetStart = new Button
        {
            Margin = new Thickness(settingSetStart.LeftMargin, 0, settingSetStart.RightMargin, 0),
            FontSize = settingSetStart.FontSize,
            Command = vm.WaveformSetStartCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetStartHint, shortcuts, nameof(vm.WaveformSetStartCommand)),
        };
        Attached.SetIcon(buttonSetStart, IconNames.RayStart);

        var settingSetEnd = GetToolbarSettingFor(SeWaveformToolbarItemType.SetStart);
        var buttonSetEnd = new Button
        {
            Margin = new Thickness(settingSetEnd.LeftMargin, 0, settingSetEnd.RightMargin, 0),
            FontSize = settingSetEnd.FontSize,
            Command = vm.WaveformSetEndCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SetEndHint, shortcuts, nameof(vm.WaveformSetEndCommand)),
        };
        Attached.SetIcon(buttonSetEnd, IconNames.RayEnd);

        var settingRemoveBlank = GetToolbarSettingFor(SeWaveformToolbarItemType.RemoveBlankLines);
        var buttonRemoveBlankLines = new Button
        {
            Margin = new Thickness(settingRemoveBlank.LeftMargin, 0, settingRemoveBlank.RightMargin, 0),
            FontSize = settingRemoveBlank.FontSize,
            Command = vm.RemoveBlankLinesCommand,
            FontWeight = FontWeight.Bold,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.RemoveBlankLines, shortcuts, nameof(vm.RemoveBlankLinesCommand)),
        };
        Attached.SetIcon(buttonRemoveBlankLines, IconNames.CardRemoveOutline);

        var settingHorizontalZoom = GetToolbarSettingFor(SeWaveformToolbarItemType.HorizontalZoom);
        var iconHorizontal = new Icon
        {
            Value = IconNames.ArrowLeftRightBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0),
            FontSize = settingHorizontalZoom.FontSize,
        };
        var sliderHorizontalZoom = new Slider
        {
            Minimum = 0.1,
            Maximum = 20.0,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Value = 1,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ZoomHorizontalHint, shortcuts),
        };
        sliderHorizontalZoom.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };
        sliderHorizontalZoom.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.AudioVisualizer) + "." + nameof(vm.AudioVisualizer.ZoomFactor)));

        var labelHorizontalZoom = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 10,
            Margin = new Thickness(0, -15, 0, 0),
        };
        labelHorizontalZoom.Bind(TextBlock.TextProperty, new Binding(nameof(vm.AudioVisualizer) + "." + nameof(vm.AudioVisualizer.ZoomFactor))
        {
            StringFormat = "{0:0}%",
            Converter = new Avalonia.Data.Converters.FuncValueConverter<double, int>(v => (int)(v * 100)),
        });

        var panelHorizontalZoom = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(settingHorizontalZoom.LeftMargin, 0, settingHorizontalZoom.RightMargin, 0),
            Children = { sliderHorizontalZoom, labelHorizontalZoom },
        };

        var settingVerticalZoom = GetToolbarSettingFor(SeWaveformToolbarItemType.VerticalZoom);
        var iconVertical = new Icon
        {
            Value = IconNames.ArrowUpDownBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0),
            FontSize = settingVerticalZoom.FontSize,
        };
        var sliderVerticalZoom = new Slider
        {
            Minimum = 0.1,
            Maximum = 20.0,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0),
            Value = 1,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ZoomVerticalHint, shortcuts),
        };
        sliderVerticalZoom.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };
        sliderVerticalZoom.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.AudioVisualizer) + "." + nameof(vm.AudioVisualizer.VerticalZoomFactor)));

        var labelVerticalZoom = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 10,
            Margin = new Thickness(0, -15, 0, 0),
        };
        labelVerticalZoom.Bind(TextBlock.TextProperty, new Binding(nameof(vm.AudioVisualizer) + "." + nameof(vm.AudioVisualizer.VerticalZoomFactor))
        {
            StringFormat = "{0:0}%",
            Converter = new Avalonia.Data.Converters.FuncValueConverter<double, int>(v => (int)(v * 100)),
        });

        var panelVerticalZoom = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(settingVerticalZoom.LeftMargin, 0, settingVerticalZoom.RightMargin, 0),
            Children = { sliderVerticalZoom, labelVerticalZoom },
        };

        var settingPosition = GetToolbarSettingFor(SeWaveformToolbarItemType.VideoPositionSlider);
        var sliderPosition = new Slider
        {
            Minimum = 0,
            Width = 160,
            Value = 0,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(settingPosition.LeftMargin, 0, settingPosition.RightMargin, 0),
            Focusable = true,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.VideoPosition, shortcuts),
        };
        sliderPosition.TemplateApplied += (s, e) =>
        {
            if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
            {
                thumb.Width = 14;
                thumb.Height = 14;
            }
        };

        if (vm.VideoPlayerControl != null)
        {
            sliderPosition.Bind(RangeBase.MaximumProperty, new Binding(nameof(vm.VideoPlayerControl) + "." + nameof(vm.VideoPlayerControl.Duration)));
            sliderPosition.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.VideoPlayerControl) + "." + nameof(vm.VideoPlayerControl.Position)));
        }
        else
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var vp = vm.GetVideoPlayerControl(); // videoPlayerUndockedViewModel.VideoPlayerControl;
                    sliderPosition.DataContext = vp;
                    sliderPosition.Bind(RangeBase.MaximumProperty, new Binding(nameof(vp.Duration)));
                    sliderPosition.Bind(RangeBase.ValueProperty, new Binding(nameof(vp.Position)));
                });
            });

        }

        var settingSpeed = GetToolbarSettingFor(SeWaveformToolbarItemType.PlaybackSpeed);
        var labelSpeed = UiUtil.MakeLabel(Se.Language.General.Speed);
        var comboBoxSpeed = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            FontSize = 12,
            MaxHeight = 22,
            MinHeight = 22,
            Padding = new Thickness(2, 2, 0, 2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
        };
        comboBoxSpeed.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Speeds)));
        comboBoxSpeed.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedSpeed)) { Mode = BindingMode.TwoWay });
        comboBoxSpeed.SelectionChanged += (s, e) =>
        {
            if (vm.AudioVisualizer != null && comboBoxSpeed.SelectedItem is string s1 && s1.EndsWith("x") &&
                double.TryParse(s1.Trim('x'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double speed))
            {
                vm.GetVideoPlayerControl()?.SetSpeed(speed);
            }
        };
        var panelSpeed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(settingSpeed.LeftMargin, 0, settingSpeed.RightMargin, 0),
            Children =
            {
                labelSpeed,
                comboBoxSpeed
            }
        };

        var settingAutoSelectOnPlay = GetToolbarSettingFor(SeWaveformToolbarItemType.AutoSelectOnPlay);
        var toggleButtonAutoSelectOnPlay = new ToggleButton
        {
            DataContext = vm,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.SelectCurrentSubtitleWhilePlaying)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(settingAutoSelectOnPlay.LeftMargin, 0, settingAutoSelectOnPlay.RightMargin, 0),
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SelectCurrentLineWhilePlayingHint, shortcuts),
        };
        Attached.SetIcon(toggleButtonAutoSelectOnPlay, IconNames.AnimationPlay);
        toggleButtonAutoSelectOnPlay.IsCheckedChanged += (s, e) => vm.AutoSelectOnPlayCheckedChanged();

        var settingCenter = GetToolbarSettingFor(SeWaveformToolbarItemType.Center);
        var toggleButtonCenter = new ToggleButton
        {
            DataContext = vm,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.WaveformCenter)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(settingCenter.LeftMargin, 0, settingCenter.RightMargin, 0),
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.CenterWaveformHint, shortcuts),
        };
        Attached.SetIcon(toggleButtonCenter, IconNames.AlignHorizontalCenter);
        toggleButtonCenter.IsCheckedChanged += (s, e) => vm.WaveformCenterCheckedChanged();

        var settingMore = GetToolbarSettingFor(SeWaveformToolbarItemType.More);
        var buttonMore = new Button
        {
            Margin = new Thickness(settingMore.LeftMargin, 0, settingMore.RightMargin, 0),
        };
        Attached.SetIcon(buttonMore, "fa-ellipsis-v");

        var flyoutMore = new MenuFlyout();
        buttonMore.Flyout = flyoutMore;
        buttonMore.Click += (s, e) => flyoutMore.ShowAt(buttonMore, true);
        var menuItemResetZoom = new MenuItem
        {
            Header = string.Format(languageHints.ResetZoomAndSpeed, UiUtil.MakeShortcutsString(shortcuts, nameof(vm.ResetWaveformZoomAndSpeedCommand))),
            Command = vm.ResetWaveformZoomAndSpeedCommand,
        };
        flyoutMore.Items.Add(menuItemResetZoom);
        var menuItemHideControls = new MenuItem
        {
            Header = string.Format(languageHints.HideWaveformToolbar, string.Empty),
            Command = vm.HideWaveformToolbarCommand,
        };
        flyoutMore.Items.Add(menuItemHideControls);

        var sortableButtons = MakeCustomSortableButtons(
            settings,
            buttonPlay,
            buttonPlaySelectedLines,
            buttonPlaySelectedLinesRepeat,
            buttonPlayNext,
            buttonNew,
            buttonSetStartAndOffsetTheRest,
            buttonSetStart,
            buttonSetEnd,
            buttonRemoveBlankLines,
            iconHorizontal,
            panelHorizontalZoom,
            iconVertical,
            panelVerticalZoom,
            sliderPosition,
            panelSpeed,
            toggleButtonAutoSelectOnPlay,
            toggleButtonCenter,
            buttonMore
        );
        foreach (var sortedButton in sortableButtons)
        {
            if (sortedButton.Control != null)
            {
                controlsPanel.Children.Add(sortedButton.Control);
            }
        }

        mainGrid.Children.Add(controlsPanel);
        Grid.SetRow(controlsPanel, 1);

        DragDrop.SetAllowDrop(vm.AudioVisualizer, true);
        vm.AudioVisualizer.AddHandler(DragDrop.DragOverEvent, vm.VideoOnDragOver, RoutingStrategies.Bubble);
        vm.AudioVisualizer.AddHandler(DragDrop.DropEvent, vm.VideoOnDrop, RoutingStrategies.Bubble);

        return mainGrid;
    }

    private static SeWaveformToolbarItem GetToolbarSettingFor(SeWaveformToolbarItemType type)
    {
        return Se.Settings.Waveform.ToolbarItems.First(p => p.Type == type);
    }

    private static List<SortedControl> MakeCustomSortableButtons(
        SeWaveform settings,
        Button buttonPlay,
        Button buttonPlaySelectedLines,
        Button buttonPlaySelectedLinesRepeat,
        Button buttonPlayNext,
        Button buttonNew,
        Button buttonSetStartAndOffsetTheRest,
        Button buttonSetStart,
        Button buttonSetEnd,
        Button buttonRemoveBlankLines,
        Icon iconHorizontal,
        StackPanel panelHorizontalZoom,
        Icon iconVertical,
        StackPanel panelVerticalZoom,
        Slider sliderPosition,
        StackPanel panelSpeed,
        ToggleButton toggleButtonAutoSelectOnPlay,
        ToggleButton toggleButtonCenter,
        Button buttonMore)
    {
        var toolbarButtonForSort = new List<SortedControl>();

        foreach (var item in settings.ToolbarItems)
        {
            if (!item.IsVisible)
            {
                continue;
            }

            switch (item.Type)
            {
                case SeWaveformToolbarItemType.Play:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonPlay });
                    break;
                case SeWaveformToolbarItemType.PlaySelection:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonPlaySelectedLines });
                    break;
                case SeWaveformToolbarItemType.Repeat:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonPlaySelectedLinesRepeat });
                    break;
                case SeWaveformToolbarItemType.PlayNext:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonPlayNext });
                    break;
                case SeWaveformToolbarItemType.New:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonNew });
                    break;
                case SeWaveformToolbarItemType.SetStartAndOffsetTheRest:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonSetStartAndOffsetTheRest });
                    break;
                case SeWaveformToolbarItemType.SetStart:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonSetStart });
                    break;
                case SeWaveformToolbarItemType.SetEnd:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonSetEnd });
                    break;
                case SeWaveformToolbarItemType.RemoveBlankLines:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonRemoveBlankLines });
                    break;
                case SeWaveformToolbarItemType.HorizontalZoom:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = iconHorizontal });
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = panelHorizontalZoom });
                    break;
                case SeWaveformToolbarItemType.VerticalZoom:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = iconVertical });
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = panelVerticalZoom });
                    break;
                case SeWaveformToolbarItemType.VideoPositionSlider:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = sliderPosition });
                    break;
                case SeWaveformToolbarItemType.PlaybackSpeed:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = panelSpeed });
                    break;
                case SeWaveformToolbarItemType.AutoSelectOnPlay:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = toggleButtonAutoSelectOnPlay });
                    break;
                case SeWaveformToolbarItemType.Center:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = toggleButtonCenter });
                    break;
                case SeWaveformToolbarItemType.More:
                    toolbarButtonForSort.Add(new SortedControl { Sort = item.SortOrder, Control = buttonMore });
                    break;
            }
        }

        return toolbarButtonForSort.OrderBy(p => p.Sort).ToList();
    }

    public static WaveformDrawStyle GetWaveformDrawStyle(string waveformDrawStyle)
    {
        if (Enum.TryParse<WaveformDrawStyle>(waveformDrawStyle, ignoreCase: true, out var value))
        {
            return value;
        }

        return WaveformDrawStyle.Classic;
    }
}
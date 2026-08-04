using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInWindow : Window
{
    private readonly BurnInViewModel _vm;
    private ComboBox? _comboBoxFontName;

    public BurnInWindow(BurnInViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.BurnIn.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        // Compact control sizing, scoped to this window: with the default Fluent heights the
        // many settings rows made the window taller than small/scaled screens, clipping the
        // bottom rows and drawing the progress bar over the buttons.
        AddCompactControlStyles();

        var subtitleSettingsView = MakeSubtitlesView(vm);
        var videoSettingsView = MakeVideoSettingsView(vm);
        var cutView = MakeCutView(vm);
        var previewView = MakePreviewView(vm);
        var audioSettingsView = MakeAudioSettingsView(vm);
        var batchView = MakeBatchView(vm);
        var targetFileSizeView = MakeTargetFileSizeView(vm);
        var videoInfoView = MakeVideoInfoView(vm);
        var progressView = MakeProgressView(vm);

        // The left column (subtitle + video settings + target size) is taller than the middle
        // column's cut/preview/audio/video-info rows. Keeping all three boxes in one packed
        // panel preserves the v5.1.0 look (no gaps), and the preview row's MinHeight below
        // guarantees the panel fits in rows 0-3 - so it can never overflow into the
        // progress-bar row (which used to draw the bar through the "File size in MB" field)
        // - and the preview box never gets shorter than its label + player, so the player
        // cannot spill over the audio settings box.
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Top,
            Children = { subtitleSettingsView, videoSettingsView, targetFileSizeView },
        };

        var buttonGenerate = new SplitButton
        {
            Content = Se.Language.General.Generate,
            Command = vm.GenerateCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.PromptForFfmpegParamsAndGenerate,
                        Command = vm.PromptFfmpegParametersAndGenerateCommand,
                    },
                }
            }
        };
        buttonGenerate.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.IsGenerating)) { Converter = new InverseBooleanConverter() });

        var buttonBatchMode = UiUtil.MakeButton(Se.Language.General.BatchMode, vm.BatchModeCommand)
            .WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter())
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonHelp = UiUtil.MakeButton(Se.Language.General.Help, vm.HelpCommand);
        var buttonSingleMode = UiUtil.MakeButton(Se.Language.General.SingleMode, vm.SingleModeCommand)
            .WithBindIsVisible(nameof(vm.IsSingleModeVisible))
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonGenerate,
            buttonHelp,
            buttonBatchMode,
            buttonSingleMode,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        // The preview column grows in single mode; the batch column grows in batch mode
        // (toggled in UpdateGrowAreas). The preview/batch row is a star row so the preview
        // (single mode) and the batch list (batch mode) also grow vertically.
        var previewColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
        var batchColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // cut
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 400 }, // preview + batch list (never smaller than the preview box needs)
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // audio
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // video info + target file size
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // subtitle/video settings
                previewColumn, // cut/preview/audio settings
                batchColumn, // batch mode
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(leftPanel, 0, 0, 4, 1);  // rows 0-3 (cut + preview + audio + video info)
        grid.Add(cutView, 0, 1);
        grid.Add(previewView, 1, 1);
        grid.Add(audioSettingsView, 2, 1);
        grid.Add(videoInfoView, 3, 1);
        grid.Add(batchView, 0, 2, 4, 1);
        grid.Add(progressView, 4, 0, 1, 3);
        grid.Add(buttonPanel, 5, 0, 1, 3);

        Content = grid;

        void UpdateGrowAreas()
        {
            // Steer extra space to whichever area is in use: the preview (single mode) or the batch list (batch mode).
            previewColumn.Width = new GridLength(1, vm.IsBatchMode ? GridUnitType.Auto : GridUnitType.Star);
            batchColumn.Width = new GridLength(1, vm.IsBatchMode ? GridUnitType.Star : GridUnitType.Auto);

            // In batch mode the file list is the focus, so keep the preview small; in single mode let it grow.
            var player = vm.VideoPlayerControl;
            if (player == null)
            {
                return;
            }

            if (vm.IsBatchMode)
            {
                player.MinWidth = 0;
                player.MinHeight = 0;
                player.Width = 240;
                player.Height = 135;
                player.HorizontalAlignment = HorizontalAlignment.Left;
                player.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                player.Width = double.NaN;
                player.Height = double.NaN;
                player.MinWidth = 480;
                player.MinHeight = 270;
                player.HorizontalAlignment = HorizontalAlignment.Stretch;
                player.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsBatchMode))
            {
                UpdateGrowAreas();
                LockMinimumToContentSize(); // batch mode needs more width; re-fit and re-lock the minimum
            }
            else if (e.PropertyName == nameof(vm.IsGenerating))
            {
                // The progress row only exists while generating; re-lock the minimum so the
                // window (and the button row) always fit the content with the bar shown.
                LockMinimumToContentSize(heightOnly: true);
            }
        };
        UpdateGrowAreas();

        Activated += delegate { _comboBoxFontName?.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => vm.Loaded();
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Opened += (_, _) => LockMinimumToContentSize();

        Closing += delegate { UiUtil.SaveWindowPosition(this); };

        // LockMinimumToContentSize sets Width/Height from a callback posted at Loaded priority, so
        // restoring in a plain Loaded handler would be overwritten. Post at Background (which runs
        // after Loaded) so the saved size wins, clamped by the content minimum set just before.
        Loaded += delegate
        {
            Dispatcher.UIThread.Post(() => UiUtil.RestoreWindowPosition(this), DispatcherPriority.Background);
        };
    }

    private void LockMinimumToContentSize(bool heightOnly = false)
    {
        // Re-fit the window to the current mode's content (single mode is narrower; batch mode
        // needs more width for the file list), then lock that size in as the new minimum while
        // still allowing the user to enlarge the window further.
        //
        // heightOnly re-fits only the height: the width (and the minimum locked for it) is the
        // user's, and clearing MinWidth here would leave it at zero - the callback below never
        // restores it - so the window could be dragged narrower than its content, which is the
        // clipping this whole method exists to prevent.
        if (!heightOnly)
        {
            MinWidth = 0;
        }

        MinHeight = 0;
        SizeToContent = heightOnly ? SizeToContent.Height : SizeToContent.WidthAndHeight;
        Dispatcher.UIThread.Post(() =>
        {
            var width = ClientSize.Width;
            var height = ClientSize.Height;
            // Any difference between the window size and the client size (title bar, borders) has
            // to be added on top of the content, or the minimum sits below it and the bottom row
            // (buttons) gets clipped. Avalonia measures windows by client area, so this is
            // normally zero; Width/Height are also NaN until something assigns them, hence the
            // guard - NaN here would wipe out the minimum entirely.
            var chromeWidth = Width - width;
            var chromeHeight = Height - height;
            if (double.IsNaN(chromeWidth) || chromeWidth < 0)
            {
                chromeWidth = 0;
            }

            if (double.IsNaN(chromeHeight) || chromeHeight < 0)
            {
                chromeHeight = 0;
            }

            SizeToContent = SizeToContent.Manual;
            if (width > 0 && height > 0)
            {
                if (heightOnly)
                {
                    // Used when generating starts/stops: the progress row only exists while
                    // generating, so re-lock the minimum for that state. Only enlarge - never
                    // shrink the window back down automatically.
                    MinHeight = height + chromeHeight;
                    if (Height < MinHeight)
                    {
                        Height = MinHeight;
                    }
                }
                else
                {
                    MinWidth = width + chromeWidth;
                    MinHeight = height + chromeHeight;
                    Width = width + chromeWidth;
                    Height = height + chromeHeight;
                }
            }
        }, DispatcherPriority.Loaded);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.CleanupPreview();
    }

    /// <summary>
    /// Shrinks combo boxes and numeric up/downs (font, min-height, padding) for every control
    /// in this window - the settings rows are what drive the window height, and each row's
    /// height is its tallest control.
    /// </summary>
    private void AddCompactControlStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 26.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
            },
        });

        Styles.Add(new Style(x => x.OfType<NumericUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 26.0),
            },
        });

        // NumericUpDown's height is really decided by its templated TextBox and spinner.
        Styles.Add(new Style(x => x.OfType<NumericUpDown>().Descendant().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.MinHeightProperty, 24.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(6, 2)),
            },
        });

        Styles.Add(new Style(x => x.OfType<NumericUpDown>().Descendant().OfType<ButtonSpinner>())
        {
            Setters =
            {
                new Setter(TemplatedControl.MinHeightProperty, 24.0),
            },
        });

        // Same treatment for the time code up/down (cut from/to). Its inner TextBox keeps its
        // template padding - UpdateMinWidth accounts for those exact values in the width math.
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 26.0),
            },
        });

        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.MinHeightProperty, 24.0),
            },
        });

        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<ButtonSpinner>())
        {
            Setters =
            {
                new Setter(TemplatedControl.MinHeightProperty, 24.0),
            },
        });
    }

    private Border MakeSubtitlesView(BurnInViewModel vm)
    {
        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName))
            .WithMinWidth(200);
        comboBoxFontName.SelectionChanged += vm.ComboBoxChanged;
        _comboBoxFontName = comboBoxFontName;

        var labelFontSizeFactor = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FontSizeFactor);
        var numericUpDownFontSizeFactor = UiUtil.MakeNumericUpDownTwoDecimals(0.1m, 1.0m, 150, vm, nameof(vm.FontFactor));
        numericUpDownFontSizeFactor.ValueChanged += vm.NumericUpDownChanged;
        var labelFontSizeFactorInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.FontFactorText));
        var panelFontSizeFactor = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                numericUpDownFontSizeFactor,
                labelFontSizeFactorInfo
            }
        };

        var checkBoxUseBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.FontIsBold));
        checkBoxUseBold.IsCheckedChanged += (_, _) => vm.ParameterChanged();

        var labelTextColor = UiUtil.MakeLabel(Se.Language.General.TextColor);
        var colorPickerTextColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontTextColor), true);

        var labelOutline = UiUtil.MakeLabel(string.Empty)
            .WithBindText(vm, nameof(vm.FontOutlineText));
        var textBoxBoxWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 130, vm, nameof(vm.SelectedFontOutline));
        textBoxBoxWidth.ValueChanged += vm.NumericUpDownChanged;
        var colorPickerBoxColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontOutlineColor), true);
        var panelBox = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colorPickerBoxColor,
                UiUtil.MakeLabel(Se.Language.General.Width).WithMarginLeft(5),
                textBoxBoxWidth,
            }
        };

        var labelShadow = UiUtil.MakeLabel(Se.Language.General.Shadow)
            .WithBindText(vm, nameof(vm.FontShadowText));
        var textBoxShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 130, vm, nameof(vm.SelectedFontShadowWidth));
        textBoxShadowWidth.ValueChanged += vm.NumericUpDownChanged;
        var colorPickerShadowColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontShadowColor), true);
        var panelShadow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colorPickerShadowColor,
                UiUtil.MakeLabel(Se.Language.General.Width).WithMarginLeft(5),
                textBoxShadowWidth,
            }
        };

        var labelBoxType = UiUtil.MakeLabel(Se.Language.Video.BurnIn.BoxType);
        var comboBoxBoxType = UiUtil.MakeComboBox(vm.FontBoxTypes, vm, nameof(vm.SelectedFontBoxType));
        comboBoxBoxType.SelectionChanged += vm.BoxTypeChanged;

        var labelAlignment = UiUtil.MakeLabel(Se.Language.General.Alignment);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.FontAlignments, vm, nameof(vm.SelectedFontAlignment));
        comboBoxAlignment.SelectionChanged += vm.ComboBoxChanged;

        var labelMargin = UiUtil.MakeLabel(Se.Language.General.Margin);
        var labelMarginHorizontal = UiUtil.MakeLabel(Se.Language.General.Horizontal);
        var textBoxMarginHorizontal = UiUtil.MakeNumericUpDownInt(0, 1000, 0, 130, vm, nameof(vm.FontMarginHorizontal));
        textBoxMarginHorizontal.ValueChanged += vm.NumericUpDownChanged;
        var labelMarginVertical = UiUtil.MakeLabel(Se.Language.General.Vertical).WithMarginLeft(5);
        var textBoxMarginVertical = UiUtil.MakeNumericUpDownInt(0, 1000, 0, 130, vm, nameof(vm.FontMarginVertical));
        textBoxMarginVertical.ValueChanged += vm.NumericUpDownChanged;
        var panelMargin = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelMarginHorizontal,
                textBoxMarginHorizontal,
                labelMarginVertical,
                textBoxMarginVertical
            }
        };

        var labelEffect = UiUtil.MakeLabel(Se.Language.General.Effect);
        var labelSelectedEffect = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.DisplayEffect)).WithMarginRight(3);
        var buttonEffect = UiUtil.MakeButtonBrowse(vm.ShowEffectsCommand, accessibleName: Se.Language.General.Effect);
        var panelEffect = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                buttonEffect,
                labelSelectedEffect,
            }
        };

        var labelLogo = UiUtil.MakeLabel(Se.Language.General.Logo);
        var buttonLogo = UiUtil.MakeButtonBrowse(vm.ShowLogoCommand, accessibleName: Se.Language.General.Logo);
        var labelLogoInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.LogoInfo)).WithMarginRight(3);
        var panelLogo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                buttonLogo,
                labelLogoInfo,
            }
        };


        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var fontPanel = UiUtil.MakeHorizontalPanel(comboBoxFontName, checkBoxUseBold);

        grid.Add(labelFontName, 0, 0);
        grid.Add(fontPanel, 0, 1);

        grid.Add(labelFontSizeFactor, 1, 0);
        grid.Add(panelFontSizeFactor, 1, 1);

        grid.Add(labelBoxType, 2, 0);
        grid.Add(comboBoxBoxType, 2, 1);

        grid.Add(labelTextColor, 3, 0);
        grid.Add(colorPickerTextColor, 3, 1);

        grid.Add(labelOutline, 4, 0);
        grid.Add(panelBox, 4, 1);

        grid.Add(labelShadow, 5, 0);
        grid.Add(panelShadow, 5, 1);

        grid.Add(labelAlignment, 6, 0);
        grid.Add(comboBoxAlignment, 6, 1);

        grid.Add(labelMargin, 7, 0);
        grid.Add(panelMargin, 7, 1);

        grid.Add(labelEffect, 8, 0);
        grid.Add(panelEffect, 8, 1);

        var panel = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            Background = Brushes.Black,
            Opacity = 0.8,
            Children =
            {
                new Label
                {
                    Content = Se.Language.Video.AssaStyleWillBeUsed,
                    FontWeight = FontWeight.Bold,
                    FontSize = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                },
            }
        }.WithBindVisible(vm, nameof(vm.ShowAssaOnlyBox));
        grid.Add(panel, 0, 0, 9, 2);

        grid.Add(labelLogo, 9, 0);
        grid.Add(panelLogo, 9, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeVideoSettingsView(BurnInViewModel vm)
    {
        var labelResolution = UiUtil.MakeLabel(Se.Language.General.Resolution);
        var textBoxWidth = UiUtil.MakeNumericUpDownInt(0, 10_000, 0, 130, vm, nameof(vm.VideoWidth));
        var labelX = UiUtil.MakeLabel("x");
        var textBoxHeight = UiUtil.MakeNumericUpDownInt(0, 10_000, 0, 130, vm, nameof(vm.VideoHeight));
        var buttonResolution = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand, accessibleName: Se.Language.General.Resolution);
        var panelResolution = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                textBoxWidth,
                labelX,
                textBoxHeight,
                buttonResolution,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution), new InverseBooleanConverter());

        var labelSourceResolution = UiUtil.MakeLabel("Use source resolution").WithBindVisible(vm, nameof(vm.UseSourceResolution));
        var buttonResolutionSource = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand, accessibleName: Se.Language.General.Resolution);
        var panelResolutionSource = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelSourceResolution,
                buttonResolutionSource,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution));

        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding);
        var comboBoxEncoding = UiUtil.MakeComboBox(vm.VideoEncodings, vm, nameof(vm.SelectedVideoEncoding));
        comboBoxEncoding.SelectionChanged += vm.VideoEncodingChanged;

        var labelPreset = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoPresetText));
        var comboBoxPreset = UiUtil.MakeComboBox(vm.VideoPresets, vm, nameof(vm.SelectedVideoPreset));

        var labelCrf = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoCrfText));
        var comboBoxCrf = UiUtil.MakeComboBox(vm.VideoCrf, vm, nameof(vm.SelectedVideoCrf));
        var labelCrfHint = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoCrfHint)).WithMarginLeft(5);
        labelCrfHint.FontSize = 10;
        labelCrfHint.Opacity = 0.7;
        var panelCrf = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                comboBoxCrf,
                labelCrfHint
            }
        };

        var labelPixelFormat = UiUtil.MakeLabel(Se.Language.Video.BurnIn.PixelFormat);
        var comboBoxPixelFormat = UiUtil.MakeComboBox(vm.VideoPixelFormats, vm, nameof(vm.SelectedVideoPixelFormat));

        var labelVideoExtension = UiUtil.MakeLabel(Se.Language.General.VideoExtension);
        var comboBoxVideoExtension = UiUtil.MakeComboBox(vm.VideoExtensions, vm, nameof(vm.SelectedVideoExtension));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelResolution, 0, 0);
        grid.Add(panelResolution, 0, 1);
        grid.Add(panelResolutionSource, 0, 1);

        grid.Add(labelEncoding, 1, 0);
        grid.Add(comboBoxEncoding, 1, 1);

        grid.Add(labelPreset, 2, 0);
        grid.Add(comboBoxPreset, 2, 1);

        grid.Add(labelCrf, 3, 0);
        grid.Add(panelCrf, 3, 1);

        grid.Add(labelPixelFormat, 4, 0);
        grid.Add(comboBoxPixelFormat, 4, 1);

        grid.Add(labelVideoExtension, 5, 0);
        grid.Add(comboBoxVideoExtension, 5, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeCutView(BurnInViewModel vm)
    {
        var checkBoxCut = UiUtil.MakeCheckBox(Se.Language.General.Cut, vm, nameof(vm.IsCutActive));

        var buttonCutFrom = UiUtil.MakeButtonBrowse(vm.BrowseCutFromCommand, accessibleName: Se.Language.Video.BurnIn.FromTime);
        buttonCutFrom.VerticalAlignment = VerticalAlignment.Center;
        var labelFromTime = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FromTime);
        var timeUpDownFrom = new TimeCodeUpDown
        {
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.CutFrom)),
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonCutTo = UiUtil.MakeButtonBrowse(vm.BrowseCutToCommand, accessibleName: Se.Language.Video.BurnIn.ToTime);
        buttonCutTo.VerticalAlignment = VerticalAlignment.Center;
        var labelToTime = UiUtil.MakeLabel(Se.Language.Video.BurnIn.ToTime);
        var timeUpDownTo = new TimeCodeUpDown
        {
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.CutTo)),
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxCut, 0, 0);

        grid.Add(labelFromTime, 1, 0);
        grid.Add(timeUpDownFrom, 1, 1);
        grid.Add(buttonCutFrom, 1, 2);

        grid.Add(labelToTime, 2, 0);
        grid.Add(timeUpDownTo, 2, 1);
        grid.Add(buttonCutTo, 2, 2);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakePreviewView(BurnInViewModel vm)
    {

        var labelPreview = UiUtil.MakeLabel(Se.Language.General.Preview);

        // Live preview: the loaded video plays in an embedded mpv player while the
        // current style/effects are generated as an ASSA subtitle and rendered on top
        // via libass (sub-add/sub-reload) - see BurnInViewModel.LoadVideoPreview.
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = true;
        vm.VideoPlayerControl.FullScreenCommand = vm.PreviewFullScreenCommand;
        vm.VideoPlayerControl.MinWidth = 480;
        vm.VideoPlayerControl.MinHeight = 270;
        vm.VideoPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.VideoPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player grows
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelPreview, 0, 0);
        grid.Add(vm.VideoPlayerControl, 1, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeAudioSettingsView(BurnInViewModel vm)
    {
        var labelAudioEncoding = UiUtil.MakeLabel(Se.Language.Video.BurnIn.AudioEncoding);
        var comboBoxAudioEncoding = UiUtil.MakeComboBox(vm.AudioEncodings, vm, nameof(vm.SelectedAudioEncoding));

        var checkBoxStereo = UiUtil.MakeCheckBox(Se.Language.General.Stereo, vm, nameof(vm.AudioIsStereo));

        var labelSampleRate = UiUtil.MakeLabel(Se.Language.Video.BurnIn.SampleRate);
        var comboBoxSampleRate = UiUtil.MakeComboBox(vm.AudioSampleRates, vm, nameof(vm.SelectedAudioSampleRate));

        var labelBitRate = UiUtil.MakeLabel(Se.Language.Video.BurnIn.BitRate);
        var comboBoxBitRate = UiUtil.MakeComboBox(vm.AudioBitRates, vm, nameof(vm.SelectedAudioBitRate));

        // Only as many rows as are used: RowSpacing applies to empty Auto rows too, so the
        // unused rows here used to leave a band of dead space at the bottom of the box.
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelAudioEncoding, 0, 0);
        grid.Add(UiUtil.MakeHorizontalPanel(comboBoxAudioEncoding, checkBoxStereo), 0, 1);

        grid.Add(labelSampleRate, 1, 0);
        grid.Add(comboBoxSampleRate, 1, 1);

        grid.Add(labelBitRate, 2, 0);
        grid.Add(comboBoxBitRate, 2, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeBatchView(BurnInViewModel vm)
    {
        // No header sorting: the batch queue is processed top-to-bottom (jobs are
        // selected/scrolled by index while generating), so the list order is meaningful.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.MinWidth = 550; // the batch column is Auto while measuring; star columns have no intrinsic width
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.JobItems;
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FileName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.InputVideoFileNameShort)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Size,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.Resolution)),
            Width = new GridLength(90), // was content-sized (Auto) on the DataGrid
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.SubtitleFile,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.SubtitleFileNameShort)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.Status)),
            Width = new GridLength(110), // was content-sized (Auto) on the DataGrid
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedJobItem)) { Source = vm });
        vm.BatchGrid = dataGrid;

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.AddDotDotDot, vm.AddCommand);
        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Remove, vm.RemoveCommand);
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand);
        var buttonPickSubtitle = UiUtil.MakeButton(Se.Language.General.PickSubtitleFile, vm.PickSubtitleCommand);

        var panelFileControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonAdd,
                buttonRemove,
                buttonClear,
                UiUtil.MakeSeparatorForHorizontal(vm),
                buttonPickSubtitle,
            }
        };

        var buttonOutputProperties = UiUtil.MakeButton(Se.Language.Video.BurnIn.OutputProperties, vm.OutputPropertiesCommand);
        var labelOutputPropertiesFolder = UiUtil.MakeLink(string.Empty, vm.OpenOutputFolderCommand)
            .WithFilePathText(vm, nameof(vm.OutputFolder))
            .WithBindVisible(vm, nameof(vm.UseOutputFolderVisible));
        var labelOutputPropertiesUseSourceFolder = UiUtil.MakeLabel(Se.Language.General.UseSourceFolder)
            .WithBindVisible(vm, nameof(vm.UseSourceFolderVisible));

        var panelFileControls2 = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonOutputProperties,
                labelOutputPropertiesFolder,
                labelOutputPropertiesUseSourceFolder,
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0);
        grid.Add(panelFileControls, 1, 0);
        grid.Add(panelFileControls2, 2, 0);

        return UiUtil.MakeBorderForControl(grid)
            .WithBindIsVisible(nameof(vm.IsBatchMode))
            .WithMarginBottom(5);
    }

    private static Border MakeTargetFileSizeView(BurnInViewModel vm)
    {
        var checkBoxUseTargetFileSize = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.TargetFileSize, vm, nameof(vm.UseTargetFileSize));
        checkBoxUseTargetFileSize.IsCheckedChanged += (_, _) => vm.CheckBoxTargetFileChanged();

        // "Match source video size" derives the target from each input file's own size (works per-file
        // in batch mode); when on, the fixed-MB field is irrelevant and is disabled.
        var checkBoxMatchSource = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.MatchSourceVideoSize, vm, nameof(vm.MatchSourceVideoSize))
            .WithMarginLeft(10);

        var labelTargetFileSize = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FileSizeMb).WithMarginLeft(10);
        var numericUpDownTargetFileSize = UiUtil.MakeNumericUpDownInt(1, 1000_000_000, 0, 150, vm, nameof(vm.TargetFileSize));
        numericUpDownTargetFileSize.ValueChanged += vm.NumericUpDownTargetFileSizeChanged;
        numericUpDownTargetFileSize.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.MatchSourceVideoSize)) { Converter = new InverseBooleanConverter() });
        var labelVideoBitRate = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.TargetVideoBitRateInfo));
        labelVideoBitRate.FontSize = 10;
        labelVideoBitRate.Opacity = 0.7;
        var panelTargetFileSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                numericUpDownTargetFileSize,
                labelVideoBitRate
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxUseTargetFileSize, 0, 0, 1, 2);
        grid.Add(checkBoxMatchSource, 1, 0, 1, 2);
        grid.Add(labelTargetFileSize, 2, 0);
        grid.Add(panelTargetFileSize, 2, 1);

        // Visible in batch mode too, so each file can target its own source size (issue #11802).
        // MarginBottom(5) matches every other settings box so the borders line up across columns.
        return UiUtil.MakeBorderForControl(grid)
            .WithMarginBottom(5)
            .WithMarginRight(5);
    }

    private static Border MakeVideoInfoView(BurnInViewModel vm)
    {
        var labelVideoFile = UiUtil.MakeLabel(Se.Language.General.VideoFile);
        var labelVideoFileName = UiUtil.MakeFilePathLabel(vm, nameof(vm.VideoFileName));

        var labelVideoSize = UiUtil.MakeLabel(Se.Language.Video.BurnIn.VideoFileSize);
        var labelVideoSizeValue = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoFileSize));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelVideoFile, 0, 0);
        grid.Add(labelVideoFileName, 0, 1);

        grid.Add(labelVideoSize, 1, 0);
        grid.Add(labelVideoSizeValue, 1, 1);

        return UiUtil.MakeBorderForControl(grid)
            .WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter())
            .WithMarginBottom(5)
            .WithMarginRight(5);
    }

    private static Grid MakeProgressView(BurnInViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Margin = new Thickness(0, 4, 5, 0);
        progressBar.VerticalAlignment = VerticalAlignment.Top;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var statusText = new TextBlock
        {
            Margin = new Thickness(5, 18, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }
}

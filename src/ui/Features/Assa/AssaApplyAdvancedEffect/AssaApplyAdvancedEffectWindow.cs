using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public class AssaApplyAdvancedEffectWindow : Window
{
    private const double TileWidth = 136;
    private const double TileHeight = 77;

    public AssaApplyAdvancedEffectWindow(AssaApplyAdvancedEffectViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ApplyAdvancedEffectTitle;
        CanResize = true;
        Width = 1280;
        Height = 820;
        MinWidth = 1000;
        MinHeight = 640;
        vm.Window = this;
        DataContext = vm;

        var accent = GetAccentColor();

        // ── Left: search, categories, effect gallery ──────────────────────────
        var searchBox = new TextBox
        {
            Watermark = Se.Language.Assa.AdvancedEffectSearchWatermark,
            InnerLeftContent = new Icon
            {
                Value = IconNames.Find,
                FontSize = 16,
                Opacity = 0.6,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            },
            [!TextBox.TextProperty] = new Binding(nameof(vm.SearchText)) { Mode = BindingMode.TwoWay },
        }.WithAccessibleName(Se.Language.General.Search);

        var categoryList = MakeCategoryList(vm).WithAccessibleName(Se.Language.General.Category);
        var gallery = MakeGallery(vm).WithAccessibleName(Se.Language.Assa.ChooseEffect);
        gallery.AddHandler(KeyDownEvent, (_, e) => MoveGallerySelectionVertically(gallery, e), RoutingStrategies.Tunnel);

        var noMatches = new TextBlock
        {
            Text = Se.Language.Assa.AdvancedEffectNoMatches,
            Opacity = 0.6,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.HasNoMatches)),
        };

        var galleryHost = new Grid();
        galleryHost.Children.Add(gallery);
        galleryHost.Children.Add(noMatches);

        var leftGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*"),
            RowSpacing = 8,
        };
        leftGrid.Add(searchBox, 0);
        leftGrid.Add(categoryList, 1);
        leftGrid.Add(MakeCard(galleryHost, new Thickness(4)), 2);

        // ── Right: preview, customize, apply to ───────────────────────────────
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;
        vm.VideoPlayerControl.Bind(IsVisibleProperty, new Binding(nameof(vm.HasVideo)));

        var samplePreview = new EffectPreviewControl
        {
            IsPlaying = true,
            [!EffectPreviewControl.AdvancedEffectProperty] = new Binding(nameof(vm.SelectedOverrideTag)),
            [!EffectPreviewControl.PreviewTextProperty] = new Binding(nameof(vm.PreviewText)),
        };
        var sampleCaption = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x99, 0, 0, 0)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 3),
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = Se.Language.Assa.AdvancedEffectPreviewNoVideo,
                Foreground = Brushes.White,
                FontSize = UiUtil.ScaledFontSize(11),
            },
        };
        var samplePreviewHost = new Border
        {
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = new Grid { Children = { samplePreview, sampleCaption } },
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.HasVideo)) { Converter = BoolConverters.Not },
        };

        var previewArea = new Grid { MinHeight = 240 };
        previewArea.Children.Add(vm.VideoPlayerControl);
        previewArea.Children.Add(samplePreviewHost);

        var comboBoxLeft = UiUtil.MakeComboBoxBindText(
            vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxLeft.Width = double.NaN;
        comboBoxLeft.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxLeft.WithAccessibleName(Se.Language.General.Lines); // subtitle line picker under the video, no visible label (#12087)
        vm.ComboBoxLeft = comboBoxLeft;
        comboBoxLeft.SelectionChanged += vm.ComboBoxParagraphsChanged;

        var buttonPlay = UiUtil.MakeButton(Se.Language.General.PlayCurrent, vm.PlayAndBackCommand);
        buttonPlay.Bind(IsVisibleProperty, new Binding(nameof(vm.HasVideo)));

        var lineRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8,
        };
        lineRow.Add(comboBoxLeft, 0, 0);
        lineRow.Add(buttonPlay, 0, 1);

        var previewGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            RowSpacing = 8,
        };
        previewGrid.Add(previewArea, 0);
        previewGrid.Add(lineRow, 1);

        var settingsHost = new ContentControl();
        var customizeCard = MakeCard(MakeCustomizeSection(vm, accent, settingsHost), new Thickness(16, 12));
        var applyToCard = MakeCard(MakeApplyToSection(vm, accent), new Thickness(16, 12));

        // Customize + Apply to scroll when they would squeeze the video (and its controls)
        var optionsScrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new StackPanel
            {
                Spacing = 10,
                Children = { customizeCard, applyToCard },
            },
        };

        var rightGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            RowSpacing = 10,
        };
        rightGrid.Add(MakeCard(previewGrid, new Thickness(8)), 0);
        rightGrid.Add(optionsScrollViewer, 1);
        rightGrid.SizeChanged += (_, e) => optionsScrollViewer.MaxHeight = e.NewSize.Height * 0.45;

        // ── Main layout ───────────────────────────────────────────────────────
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("5*,4*"),
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 14,
            RowSpacing = 12,
        };
        mainGrid.Add(leftGrid, 0, 0);
        mainGrid.Add(rightGrid, 0, 1);
        mainGrid.Add(buttonPanel, 1, 0, 1, 2);

        Content = mainGrid;

        settingsHost.Content = MakeSettings(vm.SelectedOverrideTag);
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.SelectedOverrideTag))
            {
                settingsHost.Content = MakeSettings(vm.SelectedOverrideTag);
            }
        };

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, gallery);
        AddHandler(KeyDownEvent, vm.OnKeyDownHandler,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }

    private static Color GetAccentColor()
    {
        return Application.Current != null &&
               Application.Current.TryGetResource("SystemAccentColor", Application.Current.ActualThemeVariant, out var value) &&
               value is Color color
            ? color
            : Color.FromRgb(0x00, 0x78, 0xD4);
    }

    private static ListBox MakeCategoryList(AssaApplyAdvancedEffectViewModel vm)
    {
        return new ListBox
        {
            Background = Brushes.Transparent,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Categories)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedCategory)) { Mode = BindingMode.TwoWay },
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal }),
            ItemContainerTheme = new ControlTheme(typeof(ListBoxItem))
            {
                BasedOn = Application.Current?.FindResource(typeof(ListBoxItem)) as ControlTheme,
                Setters =
                {
                    new Setter(ListBoxItem.PaddingProperty, new Thickness(10, 5)),
                    new Setter(ListBoxItem.MarginProperty, new Thickness(0, 0, 6, 6)),
                    new Setter(ListBoxItem.CornerRadiusProperty, new CornerRadius(14)),
                    new Setter(AutomationProperties.NameProperty, new Binding(nameof(AdvancedEffectCategoryItem.Name))),
                },
            },
            ItemTemplate = new FuncDataTemplate<AdvancedEffectCategoryItem>((_, _) =>
            {
                var dot = new Border
                {
                    Width = 22,
                    Height = 22,
                    CornerRadius = new CornerRadius(11),
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Border.BackgroundProperty] = new Binding(nameof(AdvancedEffectCategoryItem.Brush)),
                };
                var icon = new ContentControl
                {
                    FontSize = 13,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                icon.Bind(Attached.IconProperty, new Binding(nameof(AdvancedEffectCategoryItem.IconName)));
                icon.Classes.Add(UiTheme.IconOnAccentClassName);
                dot.Child = icon;

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children =
                    {
                        dot,
                        new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeight.SemiBold,
                            [!TextBlock.TextProperty] = new Binding(nameof(AdvancedEffectCategoryItem.Name)),
                        },
                        new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Opacity = 0.55,
                            FontSize = UiUtil.ScaledFontSize(11),
                            [!TextBlock.TextProperty] = new Binding(nameof(AdvancedEffectCategoryItem.Count)),
                        },
                    },
                };
            }),
        };
    }

    private static ListBox MakeGallery(AssaApplyAdvancedEffectViewModel vm)
    {
        return new ListBox
        {
            Background = Brushes.Transparent,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.FilteredEffects)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedGalleryEffect)) { Mode = BindingMode.TwoWay },
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center }),
            ItemContainerTheme = new ControlTheme(typeof(ListBoxItem))
            {
                BasedOn = Application.Current?.FindResource(typeof(ListBoxItem)) as ControlTheme,
                Setters =
                {
                    new Setter(ListBoxItem.PaddingProperty, new Thickness(5)),
                    new Setter(ListBoxItem.MarginProperty, new Thickness(3)),
                    new Setter(ListBoxItem.CornerRadiusProperty, new CornerRadius(10)),
                    new Setter(AutomationProperties.NameProperty, new Binding(nameof(IAdvancedEffectDisplay.Name))),
                },
            },
            ItemTemplate = new FuncDataTemplate<IAdvancedEffectDisplay>((effect, _) =>
            {
                var preview = new EffectPreviewControl
                {
                    [!EffectPreviewControl.AdvancedEffectProperty] = new Binding("."),
                    // Animate only the tile under the mouse and the chosen one
                    [!EffectPreviewControl.IsPlayingProperty] = new MultiBinding
                    {
                        Converter = BoolConverters.Or,
                        Bindings =
                        {
                            new Binding(nameof(ListBoxItem.IsPointerOver)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(ListBoxItem) } },
                            new Binding(nameof(ListBoxItem.IsSelected)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(ListBoxItem) } },
                        },
                    },
                };

                var frame = new Border
                {
                    Width = TileWidth,
                    Height = TileHeight,
                    CornerRadius = new CornerRadius(8),
                    ClipToBounds = true,
                    Child = preview,
                };

                var name = new TextBlock
                {
                    Width = TileWidth,
                    FontSize = UiUtil.ScaledFontSize(12),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MaxLines = 2,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 5, 0, 1),
                    [!TextBlock.TextProperty] = new Binding(nameof(IAdvancedEffectDisplay.Name)),
                };

                var tile = new StackPanel { Children = { frame, name } };
                if (Se.Settings.Appearance.ShowHints)
                {
                    tile.Bind(ToolTip.TipProperty, new Binding(nameof(IAdvancedEffectDisplay.Description)));
                }

                return tile;
            }),
        };
    }

    /// <summary>
    /// Up/Down in the wrapped tile grid: jump to the nearest tile in the row above/below
    /// (the list box itself treats the wrap panel as one long row).
    /// </summary>
    private static void MoveGallerySelectionVertically(ListBox gallery, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None || (e.Key != Key.Up && e.Key != Key.Down))
        {
            return;
        }

        var count = gallery.ItemCount;
        var currentIndex = gallery.SelectedIndex;
        if (count == 0)
        {
            return;
        }

        if (currentIndex < 0 || gallery.ContainerFromIndex(currentIndex) is not { } current)
        {
            gallery.SelectedIndex = 0;
            e.Handled = true;
            return;
        }

        var down = e.Key == Key.Down;
        var currentTop = current.Bounds.Top;
        var currentCenter = current.Bounds.Center.X;
        var bestIndex = -1;
        var bestRowDistance = double.MaxValue;
        var bestColumnDistance = double.MaxValue;
        for (var i = 0; i < count; i++)
        {
            if (gallery.ContainerFromIndex(i) is not { } candidate)
            {
                continue;
            }

            var rowDistance = down ? candidate.Bounds.Top - currentTop : currentTop - candidate.Bounds.Top;
            if (rowDistance < 1)
            {
                continue;
            }

            var columnDistance = System.Math.Abs(candidate.Bounds.Center.X - currentCenter);
            if (rowDistance < bestRowDistance - 1 ||
                (System.Math.Abs(rowDistance - bestRowDistance) <= 1 && columnDistance < bestColumnDistance))
            {
                bestIndex = i;
                bestRowDistance = rowDistance;
                bestColumnDistance = columnDistance;
            }
        }

        e.Handled = true;
        if (bestIndex < 0)
        {
            return;
        }

        gallery.SelectedIndex = bestIndex;
        gallery.ScrollIntoView(bestIndex);
        gallery.ContainerFromIndex(bestIndex)?.Focus(NavigationMethod.Directional);
    }

    private static Control MakeCustomizeSection(AssaApplyAdvancedEffectViewModel vm, Color accent, ContentControl settingsHost)
    {
        var sectionTitle = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                new Icon { Value = IconNames.Tune, FontSize = 16, Foreground = new SolidColorBrush(accent) },
                new TextBlock
                {
                    Text = Se.Language.Assa.AdvancedEffectCustomize.ToUpperInvariant(),
                    FontSize = UiUtil.ScaledFontSize(11),
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(accent),
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

        var name = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(16),
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.SelectedOverrideTag)}.{nameof(IAdvancedEffectDisplay.Name)}"),
        };

        var description = new TextBlock
        {
            Opacity = 0.7,
            TextWrapping = TextWrapping.Wrap,
            FontSize = UiUtil.ScaledFontSize(12),
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.SelectedOverrideTag)}.{nameof(IAdvancedEffectDisplay.Description)}"),
        };

        settingsHost.Margin = new Thickness(0, 6, 0, 0);

        return new StackPanel
        {
            Spacing = 4,
            Children = { sectionTitle, name, description, settingsHost },
        };
    }

    private static Control MakeApplyToSection(AssaApplyAdvancedEffectViewModel vm, Color accent)
    {
        var title = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                new Icon { Value = IconNames.FormatListChecks, FontSize = 16, Foreground = new SolidColorBrush(accent) },
                new TextBlock
                {
                    Text = Se.Language.General.ApplyTo.ToUpperInvariant(),
                    FontSize = UiUtil.ScaledFontSize(11),
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(accent),
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

        var segments = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 4,
            LineSpacing = 4,
            Children =
            {
                MakeSegment(Se.Language.Sync.AdjustAll, nameof(vm.AdjustAll)),
                MakeSegment(Se.Language.Sync.AdjustSelectedLines, nameof(vm.AdjustSelectedLines)),
                MakeSegment(Se.Language.Sync.AdjustSelectedLinesAndForward, nameof(vm.AdjustSelectedLinesAndForward)),
            },
        };

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                title,
                segments,
                UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SelectionInfo)).WithOpacity(0.6),
            },
        };
    }

    private static RadioButton MakeSegment(string text, string isCheckedPath)
    {
        // A radio button wearing the toggle button look - a segmented control
        var radioButton = new RadioButton
        {
            Content = text,
            GroupName = "AdvancedEffectApplyTo",
            Padding = new Thickness(12, 5),
            CornerRadius = new CornerRadius(14),
            [!ToggleButton.IsCheckedProperty] = new Binding(isCheckedPath) { Mode = BindingMode.TwoWay },
        };

        if (Application.Current?.FindResource(typeof(ToggleButton)) is ControlTheme toggleTheme)
        {
            radioButton.Theme = toggleTheme;
        }

        return radioButton;
    }

    private static Control MakeSettings(IAdvancedEffectDisplay? effect)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 12,
            RowSpacing = 6,
        };

        void AddRow(string label, Control control)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var row = grid.RowDefinitions.Count - 1;
            grid.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center }, row, 0);
            control.HorizontalAlignment = HorizontalAlignment.Left;
            grid.Add(control.WithAccessibleName(label), row, 1);
        }

        var l = Se.Language.Assa;
        switch (effect)
        {
            case AdvancedEffectSnow snow:
                AddRow(l.AdvancedEffectSnowFlakeCount, UiUtil.MakeNumericUpDownInt(10, 3000, 200, 140, snow, nameof(AdvancedEffectSnow.FlakeCount)));
                break;
            case AdvancedEffectStarfield starfield:
                AddRow(l.AdvancedEffectStarfieldStarCount, UiUtil.MakeNumericUpDownInt(50, 3000, 650, 140, starfield, nameof(AdvancedEffectStarfield.StarCount)));
                AddRow(Se.Language.General.Speed, UiUtil.MakeNumericUpDownOneDecimal(0.1m, 10.0m, 140, starfield, nameof(AdvancedEffectStarfield.SpeedMultiplier)));
                break;
            case AdvancedEffectConfetti confetti:
                AddRow(l.AdvancedEffectAmount, UiUtil.MakeNumericUpDownInt(10, 1000, 150, 140, confetti, nameof(AdvancedEffectConfetti.PieceCount)));
                break;
            case AdvancedEffectFireflies fireflies:
                AddRow(l.AdvancedEffectAmount, UiUtil.MakeNumericUpDownInt(5, 500, 60, 140, fireflies, nameof(AdvancedEffectFireflies.FireflyCount)));
                break;
            case AdvancedEffectHearts hearts:
                AddRow(l.AdvancedEffectAmount, UiUtil.MakeNumericUpDownInt(10, 1000, 120, 140, hearts, nameof(AdvancedEffectHearts.HeartCount)));
                break;
            case AdvancedEffectMatrix matrix:
                AddRow(l.AdvancedEffectAmount, UiUtil.MakeNumericUpDownInt(5, 200, 55, 140, matrix, nameof(AdvancedEffectMatrix.ColumnCount)));
                break;
            case AdvancedEffectKaraoke karaoke:
                AddRow(l.AdvancedEffectKaraokeRightToLeft, UiUtil.MakeCheckBox(karaoke, nameof(AdvancedEffectKaraoke.RightToLeft)));
                break;
            case AdvancedEffectFancyKaraoke fancyKaraoke:
            {
                AddRow(l.AdvancedEffectFancyKaraokeAutoDetectActiveWord, UiUtil.MakeCheckBox(fancyKaraoke, nameof(AdvancedEffectFancyKaraoke.AutoDetectActiveWord)));
                AddRow(l.AdvancedEffectKaraokeRightToLeft, UiUtil.MakeCheckBox(fancyKaraoke, nameof(AdvancedEffectFancyKaraoke.RightToLeft)));
                var applyGlow = UiUtil.MakeCheckBox(fancyKaraoke, nameof(AdvancedEffectFancyKaraoke.ApplyGlow));
                var glowColor = UiUtil.MakeColorPickerButton(fancyKaraoke, nameof(AdvancedEffectFancyKaraoke.GlowColor), false);
                glowColor.Bind(IsVisibleProperty, new Binding(nameof(CheckBox.IsChecked)) { Source = applyGlow, Mode = BindingMode.OneWay });
                AddRow(l.AdvancedEffectFancyKaraokeGlow, new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { applyGlow, glowColor } });
                AddRow(l.AdvancedEffectFancyKaraokeActiveColor, UiUtil.MakeColorPickerButton(fancyKaraoke, nameof(AdvancedEffectFancyKaraoke.ActiveWordColor), false));
                AddRow(l.AdvancedEffectFancyKaraokeInactiveColor, UiUtil.MakeColorPickerButton(fancyKaraoke, nameof(AdvancedEffectFancyKaraoke.InactiveWordColor), true));
                break;
            }
            case AdvancedEffectHighlightBox highlightBox:
                AddRow(l.AdvancedEffectBoxColor, UiUtil.MakeColorPickerButton(highlightBox, nameof(AdvancedEffectHighlightBox.BoxColor), false));
                AddRow(l.AdvancedEffectFancyKaraokeActiveColor, UiUtil.MakeColorPickerButton(highlightBox, nameof(AdvancedEffectHighlightBox.ActiveTextColor), false));
                AddRow(l.AdvancedEffectUpperCase, UiUtil.MakeCheckBox(highlightBox, nameof(AdvancedEffectHighlightBox.UpperCase)));
                break;
            case AdvancedEffectSingleWordPop singleWordPop:
                AddRow(Se.Language.General.TextColor, UiUtil.MakeColorPickerButton(singleWordPop, nameof(AdvancedEffectSingleWordPop.TextColor), false));
                AddRow(l.AdvancedEffectTextSizePercent, UiUtil.MakeNumericUpDownInt(50, 500, 150, 140, singleWordPop, nameof(AdvancedEffectSingleWordPop.TextSizePercent)));
                AddRow(l.AdvancedEffectUpperCase, UiUtil.MakeCheckBox(singleWordPop, nameof(AdvancedEffectSingleWordPop.UpperCase)));
                break;
            case AdvancedEffectBoldCaptions boldCaptions:
                AddRow(l.AdvancedEffectHighlightColor, UiUtil.MakeColorPickerButton(boldCaptions, nameof(AdvancedEffectBoldCaptions.HighlightColor), false));
                AddRow(l.AdvancedEffectUpperCase, UiUtil.MakeCheckBox(boldCaptions, nameof(AdvancedEffectBoldCaptions.UpperCase)));
                break;
            case AdvancedEffectSmoothFill smoothFill:
                AddRow(l.AdvancedEffectFillColor, UiUtil.MakeColorPickerButton(smoothFill, nameof(AdvancedEffectSmoothFill.FillColor), false));
                AddRow(Se.Language.General.TextColor, UiUtil.MakeColorPickerButton(smoothFill, nameof(AdvancedEffectSmoothFill.BaseColor), false));
                AddRow(l.AdvancedEffectUpperCase, UiUtil.MakeCheckBox(smoothFill, nameof(AdvancedEffectSmoothFill.UpperCase)));
                break;
            case AdvancedEffectBlurInWords blurInWords:
                AddRow(l.AdvancedEffectRise, UiUtil.MakeCheckBox(blurInWords, nameof(AdvancedEffectBlurInWords.Rise)));
                AddRow(l.AdvancedEffectUpperCase, UiUtil.MakeCheckBox(blurInWords, nameof(AdvancedEffectBlurInWords.UpperCase)));
                break;
            case AdvancedEffectLowerThird lowerThird:
                AddRow(l.AdvancedEffectLowerThirdAccentColor, UiUtil.MakeColorPickerButton(lowerThird, nameof(AdvancedEffectLowerThird.AccentColor), false));
                break;
            case AdvancedEffectWordSpacing wordSpacing:
                AddRow(l.AdvancedEffectWordSpacingPixels, UiUtil.MakeNumericUpDownOneDecimal(0m, 100m, 140, wordSpacing, nameof(AdvancedEffectWordSpacing.SpacingPixels)));
                break;
            default:
                return new TextBlock
                {
                    Text = l.AdvancedEffectNoSettings,
                    Opacity = 0.55,
                    FontStyle = FontStyle.Italic,
                    TextWrapping = TextWrapping.Wrap,
                };
        }

        return grid;
    }

    private static Border MakeCard(Control child, Thickness padding)
    {
        return new Border
        {
            Child = child,
            Padding = padding,
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
            Background = new SolidColorBrush(Color.FromArgb(0x0A, 0x80, 0x80, 0x80)),
        };
    }
}

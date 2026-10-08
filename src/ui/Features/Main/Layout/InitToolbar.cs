using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitToolbar
{
    public static Border Make(MainViewModel vm)
    {
        var toolbar = CreateToolbar(vm);

        return new Border
        {
            Child = toolbar,
        };
    }

    private static ToolbarPanel CreateToolbar(MainViewModel vm)
    {

        var stackPanelLeft = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            Margin = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Top,
        };

        var appearance = Se.Settings.Appearance;
        var isLastSeparator = true;
        var languageHints = Se.Language.Main.Toolbar;
        var shortcuts = ShortcutsMain.GetUsedShortcuts(vm);

        if (appearance.ToolbarShowFileNew)
        {
            var shortcut = shortcuts.FirstOrDefault(s => s.Name == nameof(vm.CommandFileNewCommand));
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("New"),
                Command = vm.CommandFileNewCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.NewHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.NewHint, shortcuts, nameof(vm.CommandFileNewCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowFileOpen)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Open"),
                Command = vm.CommandFileOpenCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.OpenHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.OpenHint, shortcuts, nameof(vm.CommandFileOpenCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowVideoFileOpen)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("OpenVideo"),
                Command = vm.CommandVideoOpenCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.OpenVideoHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.OpenVideoHint, shortcuts, nameof(vm.CommandVideoOpenCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSave)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Save"),
                Command = vm.CommandFileSaveCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SaveHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SaveHint, shortcuts, nameof(vm.CommandFileSaveCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSaveAs)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("SaveAs"),
                Command = vm.CommandFileSaveAsCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SaveAsHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SaveAsHint, shortcuts, nameof(vm.CommandFileSaveAsCommand)),
            });
            isLastSeparator = false;
        }

        if (!isLastSeparator)
        {
            stackPanelLeft.Children.Add(MakeSeparator());
            isLastSeparator = true;
        }

        if (appearance.ToolbarShowFind)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Find"),
                Command = vm.ShowFindCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.FindHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.FindHint, shortcuts, nameof(vm.ShowFindCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowReplace)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Replace"),
                Command = vm.ShowReplaceCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.ReplaceHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ReplaceHint, shortcuts, nameof(vm.ShowReplaceCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowMultipleReplace)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("MultipleReplace"),
                Command = vm.ShowMultipleReplaceCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.MultipleReplaceHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.MultipleReplaceHint, shortcuts, nameof(vm.ShowMultipleReplaceCommand)),
            });
            isLastSeparator = false;
        }


        if (!isLastSeparator)
        {
            stackPanelLeft.Children.Add(MakeSeparator());
            isLastSeparator = true;
        }

        if (appearance.ToolbarShowSpellCheck)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("SpellCheck"),
                Command = vm.ShowSpellCheckCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SpellCheckHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SpellCheckHint, shortcuts, nameof(vm.ShowSpellCheckCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowFixCommonErrors)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("FixCommonErrors"),
                Command = vm.ShowToolsFixCommonErrorsCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.FixCommonErrorsHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.FixCommonErrorsHint, shortcuts, nameof(vm.ShowToolsFixCommonErrorsCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowRemoveTextForHi)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("RemoveTextForHi"),
                Command = vm.ShowToolsRemoveTextForHearingImpairedCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.RemoveTextForHiHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.RemoveTextForHiHint, shortcuts, nameof(vm.ShowToolsRemoveTextForHearingImpairedCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowVisualSync)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("VisualSync"),
                Command = vm.ShowVisualSyncCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.VisualSyncHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.VisualSyncHint, shortcuts, nameof(vm.ShowVisualSyncCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowPointSync)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("PointSync"),
                Command = vm.ShowPointSyncViaOtherCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.PointSyncHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.PointSyncHint, shortcuts, nameof(vm.ShowPointSyncViaOtherCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowBeautifyTimeCodes)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("BeautifyTimeCodes"),
                Command = vm.ShowBeautifyTimeCodesCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.BeautifyTimeCodesHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.BeautifyTimeCodesHint, shortcuts, nameof(vm.ShowBeautifyTimeCodesCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowBurnIn)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("BurnIn"),
                Command = vm.ShowVideoBurnInCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.BurnInHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.BurnInHint, shortcuts, nameof(vm.ShowVideoBurnInCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowAutoTranslate)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AutoTranslate"),
                Command = vm.ShowAutoTranslateCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.AutoTranslateHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AutoTranslateHint, shortcuts, nameof(vm.ShowAutoTranslateCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSpeechToText)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("SpeechToText"),
                Command = vm.ShowSpeechToTextWhisperCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SpeechToTextHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SpeechToTextHint, shortcuts, nameof(vm.ShowSpeechToTextWhisperCommand)),
            });
            isLastSeparator = false;
        }


        if (appearance.ToolbarShowSettings)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Settings"),
                Command = vm.CommandShowSettingsCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SettingsHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SettingsHint, shortcuts, nameof(vm.CommandShowSettingsCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowLayout)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Layout"),
                Command = vm.CommandShowLayoutCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.LayoutHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.LayoutHint, shortcuts, nameof(vm.CommandShowLayoutCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSourceView)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("SourceView"),
                Command = vm.ShowSourceViewCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = Se.Language.Options.Shortcuts.SourceView,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(Se.Language.Options.Shortcuts.SourceView + " {0}", shortcuts, nameof(vm.ShowSourceViewCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowHelp)
        {
            if (!isLastSeparator)
            {
                stackPanelLeft.Children.Add(MakeSeparator());
                isLastSeparator = true;
            }

            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("Help"),
                Command = vm.ShowHelpCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.HelpHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.HelpHint, shortcuts, nameof(vm.ShowHelpCommand)),
            });
            isLastSeparator = false;
        }

        // The format specific icons below only appear for ASSA/SSA/WebVTT, and each one can be
        // hidden from settings just like the icons above. A format only gets its separator when
        // at least one of its own icons is still enabled.
        var showAssaIcons = appearance.ToolbarShowStyleManager || appearance.ToolbarShowProperties ||
                            appearance.ToolbarShowAttachments || appearance.ToolbarShowAssaDraw;
        var showSsaIcons = appearance.ToolbarShowStyleManager || appearance.ToolbarShowProperties ||
                           appearance.ToolbarShowAttachments;
        var showWebVttIcons = appearance.ToolbarShowStyleManager;

        if (!isLastSeparator)
        {
            if (showAssaIcons)
            {
                var assaSeparator = MakeSeparator();
                stackPanelLeft.Children.Add(assaSeparator);
                assaSeparator.DataContext = vm;
                assaSeparator.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsFormatAssa)) { Mode = BindingMode.OneWay });
            }

            if (showSsaIcons)
            {
                var ssaSeparator = MakeSeparator();
                stackPanelLeft.Children.Add(ssaSeparator);
                ssaSeparator.DataContext = vm;
                ssaSeparator.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsFormatSsa)) { Mode = BindingMode.OneWay });
            }

            if (showWebVttIcons)
            {
                var webVttSeparator = MakeSeparator();
                stackPanelLeft.Children.Add(webVttSeparator);
                webVttSeparator.DataContext = vm;
                webVttSeparator.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsFormatWebVtt)) { Mode = BindingMode.OneWay });
            }

            isLastSeparator = true;
        }

        if (appearance.ToolbarShowStyleManager)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaStyle"),
                Command = vm.ShowWebVttStylesCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.WebVttStylesHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.WebVttStylesHint, shortcuts, nameof(vm.ShowWebVttStylesCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatWebVtt))
                {
                    Source = vm,
                },
            });

            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaStyle"),
                Command = vm.ShowAssaStylesCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.AssaStylesHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaStylesHint, shortcuts, nameof(vm.ShowAssaStylesCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
                {
                    Source = vm,
                },
            });

            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaStyle"),
                Command = vm.ShowSsaStylesCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SsaStylesHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SsaStylesHint, shortcuts, nameof(vm.ShowSsaStylesCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatSsa))
                {
                    Source = vm,
                },
            });
        }

        if (appearance.ToolbarShowProperties)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaProperties"),
                Command = vm.ShowSsaPropertiesCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SsaPropertiesHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SsaPropertiesHint, shortcuts, nameof(vm.ShowSsaPropertiesCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatSsa))
                {
                    Source = vm,
                },
            });
        }

        if (appearance.ToolbarShowAttachments)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaAttachments"),
                Command = vm.ShowSsaAttachmentsCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.SsaAttachmentsHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SsaAttachmentsHint, shortcuts, nameof(vm.ShowSsaAttachmentsCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatSsa))
                {
                    Source = vm,
                },
            });
        }

        if (appearance.ToolbarShowProperties)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaProperties"),
                Command = vm.ShowAssaPropertiesCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.AssaPropertiesHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaPropertiesHint, shortcuts, nameof(vm.ShowAssaPropertiesCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
                {
                    Source = vm,
                },
            });
        }

        if (appearance.ToolbarShowAttachments)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaAttachments"),
                Command = vm.ShowAssaAttachmentsCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.AssaAttachmentsHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaAttachmentsHint, shortcuts, nameof(vm.ShowAssaAttachmentsCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
                {
                    Source = vm,
                },
            });
        }

        if (appearance.ToolbarShowAssaDraw)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = MakeImage("AssaDraw"),
                Command = vm.ShowAssaDrawCommand,
                Background = Brushes.Transparent,
                [AutomationProperties.NameProperty] = languageHints.AssaDrawHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaDrawHint, shortcuts, nameof(vm.ShowAssaDrawCommand)),
                [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
                {
                    Source = vm,
                },
            });
        }

        var stackPanelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            Margin = new Thickness(2, 6, 8, 6),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // When the window is too narrow for both the icons and the right panel, the right
        // panel's items are hidden in this order so they never draw on top of the icons (#15462).
        var toolbarPanel = new ToolbarPanel(stackPanelLeft, stackPanelRight);
        const int rankLabels = 1;
        const int rankFrameRate = 2;
        const int rankEncoding = 3;
        const int rankFormatProperties = 4;
        const int rankFormat = 5;

        // One properties/options button for every format with format-specific settings (EBU STL
        // options, DCinema/timed-text/WebVTT properties, ...) - the same dialogs as the File menu's
        // "<format> properties..." item. Placed left of the format selector: the right-aligned
        // panel grows leftwards, so the selector keeps its position when the button appears.
        // Unlike the icons on the left, this one stands among text labels and combo boxes rather
        // than among other icons, where the untouched 32 px artwork reads as oversized - size it
        // to the controls beside it.
        var formatPropertiesImage = MakeImage("Settings");
        formatPropertiesImage.Width = 22;
        formatPropertiesImage.Height = 22;

        var formatPropertiesButton = new Button
        {
            Content = formatPropertiesImage,
            Command = vm.FilePropertiesShowCommand,
            Background = Brushes.Transparent,
            [!AutomationProperties.NameProperty] = new Binding(nameof(vm.FilePropertiesText)) { Source = vm },
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFilePropertiesVisible)) { Source = vm },
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            formatPropertiesButton[!ToolTip.TipProperty] = new Binding(nameof(vm.FilePropertiesText)) { Source = vm };
        }
        // Wrapped, as the button's own visibility is bound to the current format.
        var formatPropertiesHost = new Panel { Children = { formatPropertiesButton } };
        stackPanelRight.Children.Add(formatPropertiesHost);
        toolbarPanel.AddCollapsible(formatPropertiesHost, rankFormatProperties);

        // subtitle formats
        var labelFormat = new TextBlock
        {
            Text = Se.Language.General.Format,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 3, 0),
        };
        stackPanelRight.Children.Add(labelFormat);
        toolbarPanel.AddCollapsible(labelFormat, rankLabels);
        var comboBoxSubtitleFormat = new ComboBox
        {
            Width = 200,
            [AutomationProperties.NameProperty] = Se.Language.General.Format,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.SubtitleFormats)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedSubtitleFormat)),
            DataContext = vm,
            ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleFormat.Name)),
                    Width = 150,
                }, true)
        };
        comboBoxSubtitleFormat.SelectionChanged += vm.ComboBoxSubtitleFormatChanged;
        comboBoxSubtitleFormat.KeyDown += vm.ComboBoxSubtitleFormatKeyDown;
        // Tunnel phase so we see the event before ComboBox consumes a left-click to open
        // its dropdown (matters for Mac Ctrl+Click, which Avalonia delivers as left+Ctrl).
        comboBoxSubtitleFormat.AddHandler(InputElement.PointerPressedEvent,
            vm.ComboBoxSubtitleFormatPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        stackPanelRight.Children.Add(comboBoxSubtitleFormat);
        toolbarPanel.AddCollapsible(comboBoxSubtitleFormat, rankFormat);
        isLastSeparator = false;

        if (appearance.ToolbarShowEncoding)
        {
            var labelEncoding = new TextBlock
            {
                Text = Se.Language.General.Encoding,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 3, 0),
            };
            stackPanelRight.Children.Add(labelEncoding);
            toolbarPanel.AddCollapsible(labelEncoding, rankLabels);
            var comboBoxEncoding = new ComboBox
            {
                Width = 200,
                [AutomationProperties.NameProperty] = Se.Language.General.Encoding,
                [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
                [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
                DataContext = vm,
            };
            stackPanelRight.Children.Add(comboBoxEncoding);
            toolbarPanel.AddCollapsible(comboBoxEncoding, rankEncoding);
        }

        if (appearance.ToolbarShowFrameRate)
        {
            var labelFrameRate = new TextBlock
            {
                Text = Se.Language.General.FrameRate,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 3, 0),
            };
            stackPanelRight.Children.Add(labelFrameRate);
            toolbarPanel.AddCollapsible(labelFrameRate, rankLabels);
            // Editable like SE 4, so any rate can be typed - e.g. 1, 2, 5, 10 or 15 fps (#15806).
            // A typed rate is applied on Enter or when focus leaves the combo box.
            var comboBoxFrameRate = new ComboBox
            {
                Width = 110,
                IsEditable = true,
                [AutomationProperties.NameProperty] = Se.Language.General.FrameRate,
                [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.FrameRates)),
                [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedFrameRate)),
                DataContext = vm,
            };
            stackPanelRight.Children.Add(comboBoxFrameRate);
            toolbarPanel.AddCollapsible(comboBoxFrameRate, rankFrameRate);
            comboBoxFrameRate.SelectionChanged += vm.ComboBoxFrameRateSelectionChanged;
            UiUtil.OnEditableComboBoxCommit(comboBoxFrameRate, () =>
            {
                vm.CommitTypedFrameRate(comboBoxFrameRate.Text);
                comboBoxFrameRate.Text = vm.SelectedFrameRate; // shows the applied rate, or restores it after invalid input
            }, handleEnter: true);
            comboBoxFrameRate.AddHandler(InputElement.KeyDownEvent, (_, e) =>
            {
                if (e.Key == Key.Escape && !comboBoxFrameRate.IsDropDownOpen && comboBoxFrameRate.Text != vm.SelectedFrameRate)
                {
                    vm.SetSelectedFrameRate(Se.Settings.General.CurrentFrameRate);
                    comboBoxFrameRate.Text = vm.SelectedFrameRate;
                    e.Handled = true;
                }
            }, RoutingStrategies.Bubble, handledEventsToo: true);

            // SE 4 had a "..." button right next to the combo box for reading the frame rate
            // out of a video file without opening it in the player.
            var buttonFrameRateFromVideo = new Button
            {
                Content = "...",
                Command = vm.GetFrameRateFromVideoFileCommand,
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                [AutomationProperties.NameProperty] = languageHints.GetFrameRateFromVideoFileHint,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.GetFrameRateFromVideoFileHint, shortcuts),
            };
            stackPanelRight.Children.Add(buttonFrameRateFromVideo);
            toolbarPanel.AddCollapsible(buttonFrameRateFromVideo, rankFrameRate);
        }

        toolbarPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
        toolbarPanel.VerticalAlignment = VerticalAlignment.Center;

        // SE 4 drew toolbar icons as flat, borderless buttons. The Classic theme's
        // global Button style (UiTheme.ApplyWindowsClassicGray) adds a 1px border to
        // every button; this style is scoped to the toolbar panel so it strips the
        // border from the toolbar buttons only - buttons elsewhere keep their border.
        // Pastel's Button style colors the border too, which boxes in every icon.
        if (UiTheme.ThemeName == UiTheme.ThemeNameClassic || UiTheme.ThemeName == UiTheme.ThemeNamePastel)
        {
            toolbarPanel.Styles.Add(new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.BorderThicknessProperty, new Thickness(0)),
                    new Setter(Button.BorderBrushProperty, Brushes.Transparent),
                },
            });
        }

        return toolbarPanel;
    }

    // Public so other windows (e.g. the spell-check completed dialog) can reuse the exact same
    // themed/recolored toolbar icons using the current theme folder.
    public static Image MakeImage(string image)
    {
        var filePath = Path.Combine(UiTheme.ImageFolder, image + ".png");
        try
        {
            return new Image
            {
                Source = MakeOneColor(filePath),
                Stretch = Stretch.Uniform,
            };
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            // The unpacked theme folder can lag behind the app (an icon added between releases
            // without a version bump made every startup crash here, since version.txt said the
            // folder was current). Repair by re-unpacking Themes.zip and retry once; if the icon
            // is still missing, show a blank image rather than killing the MainView build.
            if (Logic.Initializers.ThemeInitializer.TryRepair(filePath))
            {
                try
                {
                    return new Image
                    {
                        Source = MakeOneColor(filePath),
                        Stretch = Stretch.Uniform,
                    };
                }
                catch (Exception retryException)
                {
                    Se.LogError(retryException, $"Theme image \"{filePath}\" still missing after re-unpacking Themes.zip");
                }
            }
            else
            {
                Se.LogError(e, $"Could not load theme image \"{filePath}\"");
            }

            return new Image { Stretch = Stretch.Uniform };
        }
    }

    private static unsafe Bitmap MakeOneColor(string filePath)
    {
        if (!UiTheme.IsDarkThemeEnabled() || !Se.Settings.Appearance.MatchIconColorToDarkTheme)
        {
            return new Bitmap(filePath);
        }

        var foregroundColor = UiTheme.GetDarkThemeForegroundColor();

        using var decodedBitmap = SKBitmap.Decode(filePath);
        using var skBitmap = decodedBitmap.ColorType == SKColorType.Bgra8888
            ? decodedBitmap
            : decodedBitmap.Copy(SKColorType.Bgra8888);

        var width = skBitmap.Width;
        var height = skBitmap.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        byte* srcBase = (byte*)skBitmap.GetPixels();
        byte* dstBase = (byte*)result.GetPixels();
        int srcStride = skBitmap.RowBytes;
        int dstStride = result.RowBytes;

        for (var y = 0; y < height; y++)
        {
            uint* srcRow = (uint*)(srcBase + y * srcStride);
            uint* dstRow = (uint*)(dstBase + y * dstStride);

            for (var x = 0; x < width; x++)
            {
                uint pixel = srcRow[x];
                byte b = (byte)(pixel & 0xFF);
                byte g = (byte)((pixel >> 8) & 0xFF);
                byte r = (byte)((pixel >> 16) & 0xFF);
                byte a = (byte)(pixel >> 24);

                var intensity = (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;

                byte newR = (byte)(foregroundColor.R * intensity);
                byte newG = (byte)(foregroundColor.G * intensity);
                byte newB = (byte)(foregroundColor.B * intensity);

                dstRow[x] = (uint)(a << 24) | (uint)(newR << 16) | (uint)(newG << 8) | newB;
            }
        }

        return result.ToAvaloniaBitmap();
    }

    private static Border MakeSeparator()
    {
        return new Border
        {
            Width = 1,
            Background = Brushes.Gray,
            Margin = new Thickness(5, 5, 5, 5),
        };
    }
}

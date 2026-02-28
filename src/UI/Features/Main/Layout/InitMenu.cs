using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitMenu
{
    public static void Make(MainViewModel vm)
    {
        var l = Se.Language.Main.Menu;

        vm.MenuReopen = new MenuItem
        {
            Header = l.Reopen,
            Command = vm.CommandFileReopenCommand,
        };

        UpdateRecentFiles(vm);

        var menu = vm.Menu;
        menu.Height = 30;
        menu.DataContext = vm;
        menu.Items.Clear();
        menu.Opened += (s, e) => DisplayShortcuts(menu, vm);

        menu.Items.Add(new MenuItem
        {
            Header = l.File,
            Items =
            {
                new MenuItem
                {
                    Header = l.New,
                    Command = vm.CommandFileNewCommand,
                },
                new MenuItem
                {
                    Header = l.NewKeepVideo,
                    Command = vm.CommandFileNewKeepVideoCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.IsVideoLoaded)),
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Open,
                    Command = vm.CommandFileOpenCommand,
                },
                new MenuItem
                {
                    Header = l.OpenKeepVideo,
                    Command = vm.CommandFileOpenKeepVideoCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.IsVideoLoaded)),
                },
                new MenuItem
                {
                    Header = l.OpenOriginal,
                    Command = vm.FileOpenOriginalCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnOriginalText)) { Converter = new InverseBooleanConverter() }
                },
                new MenuItem
                {
                    Header = l.CloseOriginal,
                    Command = vm.FileCloseOriginalCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.ShowColumnOriginalText))
                },
                vm.MenuReopen,
                new MenuItem
                {
                    Header = l.RestoreAutoBackup,
                    Command = vm.ShowRestoreAutoBackupCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Save,
                    Command = vm.CommandFileSaveCommand,
                },
                new MenuItem
                {
                    Header = l.SaveAs,
                    Command = vm.CommandFileSaveAsCommand,
                },
                new Separator(),
                new MenuItem
                {
                    [!MenuItem.HeaderProperty] = new Binding(nameof(vm.FilePropertiesText)),
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.IsFilePropertiesVisible)),
                    Command = vm.FilePropertiesShowCommand,
                    DataContext = vm,
                },
                new MenuItem
                {
                    Header = l.OpenContainingFolder,
                    Command = vm.OpenContainingFolderCommand,
                },
                new MenuItem
                {
                    Header = l.Compare,
                    Command = vm.ShowCompareCommand,
                },
                new MenuItem
                {
                    Header = l.Statistics,
                    Command = vm.ShowStatisticsCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Import,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.SubtitleWithManuallyChosenEncodingDotDotDot,
                            Command = vm.ShowImportSubtitleWithManuallyChosenEncodingCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.ImagedBasedSubtitleForOcrDotDotDot,
                            Command = vm.ImportImageSubtitleForOcrCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.ImagedBasedSubtitleForEditDotDotDot,
                            Command = vm.ImportImageSubtitleForEditCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.ImagesForOcrDotDotDot,
                            Command = vm.ImportImagesCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.PlainTextDotDotDot,
                            Command = vm.ImportPlainTextCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.TimeCodesDotDotDot,
                            Command = vm.ImportTimeCodesCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Import.FormattingDotDotDot,
                            Command = vm.ImportStylingCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.Export,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = Se.Language.General.BluRaySup,
                            Command = vm.ExportBluRaySupCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.BdnXml,
                            Command = vm.ExportBdnXmlCommand,
                        },
                        new MenuItem
                        {
                            Header = new CapMakerPlus().Name,
                            Command = vm.ExportCapMakerPlusCommand,
                        },
                        new MenuItem
                        {
                            Header = Cavena890.NameOfFormat,
                            Command = vm.ExportCavena890Command,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDCinemaInteropPng,
                            Command = vm.ExportDCinemaInteropPngCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportDCinemaSmpte2014Png,
                            Command = vm.ExportDCinemaSmpte2014PngCommand,
                        },
                        new MenuItem
                        {
                            Header = Ebu.NameOfFormat,
                            Command = vm.ExportEbuStlCommand,
                        },
                        new MenuItem
                        {
                            Header = "DOST/png",
                            Command = vm.ExportDostPngCommand,
                        },
                        new MenuItem
                        {
                            Header = "FCP/png",
                            Command = vm.ExportFcpPngCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.ImagesWithTimeCode,
                            Command = vm.ExportImagesWithTimeCodeCommand,
                        },
                        new MenuItem
                        {
                            Header = Pac.NameOfFormat,
                            Command = vm.ExportPacCommand,
                        },
                        new MenuItem
                        {
                            Header = new PacUnicode().Name,
                            Command = vm.ExportPacUnicodeCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.TitleExportVobSub,
                            Command = vm.ExportVobSubCommand,
                        },
                        new MenuItem
                        {
                            Header = "WebVTT png",
                            Command = vm.ExportWebVttThumbnailsCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.CustomTextFormatsDotDotDot,
                            Command = vm.ShowExportCustomTextFormatCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.File.Export.PlainTextDotDotDot,
                            Command = vm.ShowExportPlainTextCommand,
                        },
                    }
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Exit,
                    Command = vm.CommandExitCommand,
                }
            }
        });

        menu.Items.Add(new MenuItem
        {
            Header = l.Edit,
            Items =
            {
                new MenuItem
                {
                    Header = l.Undo,
                    Command = vm.UndoCommand,
                },
                new MenuItem
                {
                    Header = l.Redo,
                    Command = vm.RedoCommand,
                },
                new MenuItem
                {
                    Header = l.ShowHistory,
                    Command = vm.ShowHistoryCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Find,
                    Command = vm.ShowFindCommand,
                },
                new MenuItem
                {
                    Header = l.FindNext,
                    Command = vm.FindNextCommand,
                },
                new MenuItem
                {
                    Header = l.Replace,
                    Command = vm.ShowReplaceCommand,
                },
                new MenuItem
                {
                    Header = l.MultipleReplace,
                    Command = vm.ShowMultipleReplaceCommand,
                },
                new MenuItem
                {
                    Header = l.GoToLineNumber,
                    Command = vm.ShowGoToLineCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.RightToLeftMode,
                    Command = vm.RightToLeftToggleCommand,
                    Icon = new Projektanker.Icons.Avalonia.Icon
                    {
                        Value = IconNames.CheckBold,
                        VerticalAlignment = VerticalAlignment.Center,
                        [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsRightToLeftEnabled)),
                    }
                },
                new MenuItem
                {
                    Header = l.FixRightToLeftViaUnicodeControlCharacters,
                    Command = vm.FixRightToLeftViaUnicodeControlCharactersCommand,
                },
                new MenuItem
                {
                    Header = l.RemoveUnicodeControlCharacters,
                    Command = vm.RemoveUnicodeControlCharactersCommand,
                },
                new MenuItem
                {
                    Header = l.ReverseRightToLeftStartEnd,
                    Command = vm.ReverseRightToLeftStartEndCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.ModifySelectionDotDotDot,
                    Command = vm.ShowModifySelectionCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.General.InvertSelection,
                    Command = vm.InverseSelectionCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.General.SelectAll,
                    Command = vm.SelectAllLinesCommand,
                },
            }
        });

        var menuItemTools = new MenuItem
        {
            Header = l.Tools,
        };
        menu.Items.Add(menuItemTools);
        var tools = new List<MenuItem>
        {
            new MenuItem
            {
                Header = l.AdjustDurations,
                Command = vm.ShowToolsAdjustDurationsCommand,
            },
            new MenuItem
            {
                Header = l.ApplyDurationLimits,
                Command = vm.ShowApplyDurationLimitsCommand,
            },
            new MenuItem
            {
                Header = l.BatchConvert,
                Command = vm.ShowToolsBatchConvertCommand,
            },
            new MenuItem
            {
                Header = l.BridgeGaps,
                Command = vm.ShowBridgeGapsCommand,
            },
            new MenuItem
            {
                Header = l.ApplyMinGap,
                Command = vm.ShowApplyMinGapCommand,
            },
            new MenuItem
            {
                Header = l.ChangeCasing,
                Command = vm.ShowToolsChangeCasingCommand,
            },
            new MenuItem
            {
                Header = l.ChangeFormatting,
                Command = vm.ShowToolsChangeFormattingCommand,
            },
            new MenuItem
            {
                Header = l.FixCommonErrors,
                Command = vm.ShowToolsFixCommonErrorsCommand,
            },
            new MenuItem
            {
                Header = l.CheckAndFixNetflixErrors,
                Command = vm.ShowToolsFixNetflixErrorsCommand,
            },
            new MenuItem
            {
                Header = l.MakeEmptyTranslationFromCurrentSubtitle,
                Command = vm.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand,
            },
            new MenuItem
            {
                Header = l.MergeLinesWithSameText,
                Command = vm.ShowToolsMergeLinesWithSameTextCommand,
            },
            new MenuItem
            {
                Header = l.MergeLinesWithSameTimeCodes,
                Command = vm.ShowToolsMergeLinesWithSameTimeCodesCommand,
            },
            new MenuItem
            {
                Header = l.SplitBreakLongLines,
                Command = vm.ShowToolsSplitBreakLongLinesCommand,
            },
            new MenuItem
            {
                Header = l.MergeShortLines,
                Command = vm.ShowToolsMergeShortLinesCommand,
            },
            new MenuItem
            {
                Header = l.SortSubtitles,
                Command = vm.ShowSortByCommand,
            },
            new MenuItem
            {
                Header = l.RemoveTextForHearingImpaired,
                Command = vm.ShowToolsRemoveTextForHearingImpairedCommand,
            },
        };
        foreach (var item in tools.OrderBy(p => p.Header?.ToString()?.TrimStart('_', ' ')))
        {
            menuItemTools.Items.Add(item);
        }
        menuItemTools.Items.Add(new Separator());
        menuItemTools.Items.Add(new MenuItem
        {
            Header = l.JoinSubtitles,
            Command = vm.ShowToolsJoinCommand,
        });
        menuItemTools.Items.Add(new MenuItem
        {
            Header = l.SplitSubtitle,
            Command = vm.ShowToolsSplitCommand,
        });


        menu.Items.Add(new MenuItem
        {
            Header = l.SpellCheckTitle,
            Items =
            {
                new MenuItem
                {
                    Header = l.SpellCheck,
                    Command = vm.ShowSpellCheckCommand,
                },
                new MenuItem
                {
                    Header = l.FindDoubleWords,
                    Command = vm.ShowFindDoubleWordsCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.AddNameToNamesList,
                    Command = vm.ShowAddToNameListCommand,
                },
                new MenuItem
                {
                    Header = l.GetDictionaries,
                    Command = vm.ShowSpellCheckDictionariesCommand,
                },
            }
        });

        var menuItemAudioTracks = new MenuItem
        {
            Header = l.AudioTracks,
        };
        menuItemAudioTracks.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsAudioTracksVisible)));
        vm.AudioTraksMenuItem = menuItemAudioTracks;

        var videoMore = new List<MenuItem>
        {
            new MenuItem
            {
                Header = Se.Language.Video.GenerateBlankVideoDotDotDot,
                Command = vm.VideoGenerateBlankCommand,
            },
            new MenuItem
            {
                Header = Se.Language.Video.ReEncodeVideoForBetterSubtitlingDotDotDot,
                Command = vm.VideoReEncodeCommand,
            },
            new MenuItem
            {
                Header = Se.Language.Video.CutVideoDotDotDot,
                Command = vm.VideoCutCommand,
            },
            new MenuItem
            {
                Header = Se.Language.Video.EmbedSubtitlesDotDotDot,
                Command = vm.VideoEmbedCommand,
            },
            new MenuItem
            {
                Header = Se.Language.Main.Menu.SetVideoOffset,
                [!MenuItem.HeaderProperty] = new Binding(nameof(vm.SetVideoOffsetText)),
                Command = vm.ShowVideoSetOffsetCommand,
            },
            new MenuItem
            {
                Header = l.SmpteTiming,
                Command = vm.ToggleSmpteTimingCommand,
                Icon = new Projektanker.Icons.Avalonia.Icon
                {
                    Value = IconNames.CheckBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsSmpteTimingEnabled)),
                }
            },
        };

        var menuItemVideoMore = new MenuItem
        {
            Header = Se.Language.General.More,
            [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.IsVideoLoaded)),
        };

        foreach (var item in videoMore.OrderBy(p => p.Header?.ToString()?.TrimStart('_', ' ')))
        {
            menuItemVideoMore.Items.Add(item);
        }

        menu.Items.Add(new MenuItem
        {
            Header = l.Video,
            Items =
            {
                new MenuItem
                {
                    Header = l.OpenVideo,
                    Command = vm.CommandVideoOpenCommand,
                },
                new MenuItem
                {
                    Header = l.OpenVideoFromUrl,
                    Command = vm.ShowVideoOpenFromUrlCommand,
                },
                new MenuItem
                {
                    Header = l.CloseVideoFile,
                    Command = vm.CommandVideoCloseCommand,
                },
                menuItemAudioTracks,
                new Separator(),
                new MenuItem
                {
                    Header = l.SpeechToText,
                    Command = vm.ShowSpeechToTextWhisperCommand,
                },
                new MenuItem
                {
                    Header = l.TextToSpeech,
                    Command = vm.ShowVideoTextToSpeechCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.GenerateBurnIn,
                    Command = vm.ShowVideoBurnInCommand,
                },
                new MenuItem
                {
                    Header = l.GenerateTransparent,
                    Command = vm.ShowVideoTransparentSubtitlesCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.GenerateImportShotChanges,
                    Command = vm.ShowShotChangesSubtitlesCommand,
                },
                new MenuItem
                {
                    Header = l.ListShotChanges,
                    Command = vm.ShowShotChangesListCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.ShowShotChangesListMenuItem)),
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.UndockVideoControls,
                    Command = vm.VideoUndockControlsCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.AreVideoControlsUndocked)) {  Converter = new InverseBooleanConverter() },
                },
                new MenuItem
                {
                    Header = l.DockVideoControls,
                    Command = vm.VideoRedockControlsCommand,
                    [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.AreVideoControlsUndocked)),
                },

                menuItemVideoMore,
            },
        });

        menu.Items.Add(new MenuItem
        {
            Header = l.Synchronization,
            Items =
            {
                new MenuItem
                {
                    Header = l.AdjustAllTimes,
                    Command = vm.ShowSyncAdjustAllTimesCommand,
                },
                new MenuItem
                {
                    Header = l.VisualSync,
                    Command = vm.ShowVisualSyncCommand,
                },
                new MenuItem
                {
                    Header = l.PointSync,
                    Command = vm.ShowPointSyncCommand,
                },
                new MenuItem
                {
                    Header = l.PointSyncViaOther,
                    Command = vm.ShowPointSyncViaOtherCommand,
                },
                new MenuItem
                {
                    Header = l.ChangeFrameRate,
                    Command = vm.ShowSyncChangeFrameRateCommand,
                },
                new MenuItem
                {
                    Header = l.ChangeSpeed,
                    Command = vm.ShowSyncChangeSpeedCommand,
                },
            }
        });

        menu.Items.Add(new MenuItem
        {
            Header = l.Translate,
            Items =
            {
                new MenuItem
                {
                    Header = l.AutoTranslate,
                    Command = vm.ShowAutoTranslateCommand,
                },
                new MenuItem
                {
                    Header = l.TranslateViaCopyPaste,
                    Command = vm.ShowTranslateViaCopyPasteCommand,
                },
            }
        });

        menu.Items.Add(new MenuItem
        {
            Header = l.Options,
            Items =
            {
                new MenuItem
                {
                    Header = l.Settings,
                    Command = vm.CommandShowSettingsCommand,
                },
                new MenuItem
                {
                    Header = l.Shortcuts,
                    Command = vm.CommandShowSettingsShortcutsCommand,
                },
                new MenuItem
                {
                    Header = l.WordLists,
                    Command = vm.ShowWordListsCommand,
                },
                new MenuItem
                {
                    Header = l.ChooseLanguage,
                    Command = vm.CommandShowSettingsLanguageCommand,
                },
            },
        });

        menu.Items.Add(new MenuItem
        {
            Header = l.HelpTitle,
            Items =
            {
                new MenuItem
                {
                    Header = l.About,
                    Command = vm.ShowAboutCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Help,
                    Command = vm.ShowHelpCommand,
                },
            }
        });


        var assaTools = new List<MenuItem>
        {
            new MenuItem
            {
                Header = l.AssaProgressBar,
                Command = vm.ShowAssaGenerateProgressBarCommand,
            },
            new MenuItem
            {
                Header = l.AssaChangeResolution,
                Command = vm.ShowAssaChangeResolutionCommand,
            },
            new MenuItem
            {
                Header = l.AssaGenerateBackground,
                Command = vm.ShowAssaGenerateBackgroundCommand,
            },
            new MenuItem
            {
                Header = l.AssaImageColorPicker,
                Command = vm.ShowAssaImageColorPickerCommand,
            },
            new MenuItem
            {
                Header = l.AssaSetPosition,
                Command = vm.ShowAssaSetPositionCommand,
            },
            new MenuItem
            {
                Header = l.AssaApplyCustomOverrideTags,
                Command = vm.ShowAssaApplyCustomOverrideTagsCommand,
            },
            new MenuItem
            {
                Header = l.AssaDraw,
                Command = vm.ShowAssaDrawCommand,
            },
            new MenuItem
            {
                Header = l.AssaProperties,
                Command = vm.ShowAssaPropertiesCommand,
            },
            new MenuItem
            {
                Header = l.AssaAttachments,
                Command = vm.ShowAssaAttachmentsCommand,
            },
            new MenuItem
            {
                Header = l.AssaStyles,
                Command = vm.ShowAssaStylesCommand,
            },
        };

        var menuItemAssaTools = new MenuItem
        {
            Header = l.AssaTools,
            [!MenuItem.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa)),
        };

        foreach (var item in assaTools.OrderBy(p => p.Header?.ToString()?.TrimStart('_', ' ')))
        {
            menuItemAssaTools.Items.Add(item);
        }

        menuItemAssaTools.Items.Add(new Separator());
        menuItemAssaTools.Items.Add(new MenuItem
        {
            Header = l.FilterLayersForDisplayDotDotDot,
            Command = vm.ShowPickLayerFilterCommand,
        });

        menu.Items.Add(menuItemAssaTools);
    }

    public static void UpdateRecentFiles(MainViewModel vm)
    {
        var files = Se.Settings.File.RecentFiles.Where(p => !string.IsNullOrEmpty(p.SubtitleFileName) && System.IO.File.Exists(p.SubtitleFileName)).ToList();
        vm.MenuReopen.Items.Clear();
        if (files.Count > 0)
        {
            foreach (var file in files)
            {
                var header = file.SubtitleFileName;

                if (!string.IsNullOrEmpty(file.SubtitleFileNameOriginal) && System.IO.File.Exists(file.SubtitleFileNameOriginal))
                {
                    header += " + ";
                    if (System.IO.Path.GetDirectoryName(file.SubtitleFileName) == System.IO.Path.GetDirectoryName(file.SubtitleFileNameOriginal))
                    {
                        header += System.IO.Path.GetFileName(file.SubtitleFileNameOriginal);
                    }
                    else
                    {
                        header += file.SubtitleFileNameOriginal;
                    }
                }

                var item = new MenuItem
                {
                    Header = header,
                    Command = vm.CommandFileReopenCommand,
                };
                item.CommandParameter = file;
                vm.MenuReopen.Items.Add(item);
            }

            vm.MenuReopen.Items.Add(new Separator());

            var clearItem = new MenuItem
            {
                Header = Se.Language.Main.Menu.ClearRecentFiles,
                Command = vm.CommandFileClearRecentFilesCommand,
            };
            vm.MenuReopen.Items.Add(clearItem);

            vm.MenuReopen.IsVisible = true;
        }
        else
        {
            vm.MenuReopen.IsVisible = false;
        }
    }

    private static void DisplayShortcuts(Menu menu, MainViewModel vm)
    {
        List<ShortCut> availableShortcuts = ShortcutsMain.GetUsedShortcuts(vm);

        foreach (var item in menu.Items.OfType<MenuItem>())
        {
            item.InputGesture = GetKeyGesture(availableShortcuts, item.Command);

            foreach (var subItem in item.Items.OfType<MenuItem>())
            {
                subItem.InputGesture = GetKeyGesture(availableShortcuts, subItem.Command);
            }
        }
    }

    private static KeyGesture? GetKeyGesture(List<ShortCut> availableShortcuts, System.Windows.Input.ICommand? command)
    {
        if (command is IRelayCommand relay)
        {
            foreach (var shortcut in availableShortcuts)
            {
                if (ReferenceEquals(shortcut.Action, relay))
                {
                    return ToKeyGesture(shortcut);
                }
            }
        }

        return null;
    }

    private static KeyGesture? ToKeyGesture(ShortCut shortcut)
    {
        if (shortcut.Keys == null || shortcut.Keys.Count == 0)
        {
            return null;
        }

        var modifiers = KeyModifiers.None;
        Key? keyValue = null;

        foreach (var key in shortcut.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var k = key.Trim();
            var kLower = k.ToLowerInvariant();

            // Support combined tokens like "CtrlShift"
            if (kLower.Contains("ctrl") || kLower.Contains("control"))
            {
                modifiers |= KeyModifiers.Control;
            }
            if (kLower.Contains("shift"))
            {
                modifiers |= KeyModifiers.Shift;
            }
            if (kLower.Contains("alt"))
            {
                modifiers |= KeyModifiers.Alt;
            }
            if (kLower.Contains("win") || kLower.Contains("meta"))
            {
                // Map Win/Command to Meta so it renders appropriately across platforms
                modifiers |= KeyModifiers.Meta;
            }

            // If the whole token is not just a modifier, try parse as a key
            var isModifierOnly =
                kLower is "ctrl" or "control" or "shift" or "alt" or "win" or "meta" ||
                kLower == "ctrlshift" || kLower == "ctrlalt" || kLower == "shiftalt" ||
                kLower == "ctrlshiftalt" || kLower == "winshift" || kLower == "winctrl" ||
                kLower == "winalt" || kLower == "winctrlshift" || kLower == "metashift" ||
                kLower == "metactrl" || kLower == "metaalt" || kLower == "metactrlshift";

            if (!isModifierOnly && Enum.TryParse<Key>(k, ignoreCase: true, out var parsedKey))
            {
                keyValue = parsedKey;
            }
        }

        if (keyValue == null)
        {
            return null;
        }

        return new KeyGesture(keyValue.Value, modifiers);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
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

    private static Grid CreateToolbar(MainViewModel vm)
    {
        var path = Path.Combine(Se.ThemesFolder, UiTheme.ThemeName);
        if (!Directory.Exists(path))
        {
            path = Path.Combine(Se.ThemesFolder, "Black");
        }

        var stackPanelLeft = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Margin = new Thickness(3),
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
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "New.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileNewCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.NewHint, shortcuts, nameof(vm.CommandFileNewCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowFileOpen)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Open.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileOpenCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.OpenHint, shortcuts, nameof(vm.CommandFileOpenCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSave)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Save.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileSaveCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SaveHint, shortcuts, nameof(vm.CommandFileSaveCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSaveAs)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "SaveAs.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandFileSaveAsCommand,
                Background = Brushes.Transparent,
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
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Find.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowFindCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.FindHint, shortcuts, nameof(vm.ShowFindCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowReplace)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Replace.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowReplaceCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.ReplaceHint, shortcuts, nameof(vm.ShowReplaceCommand)),
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
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "SpellCheck.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowSpellCheckCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SpellCheckHint, shortcuts, nameof(vm.ShowSpellCheckCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowSettings)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Settings.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandShowSettingsCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.SettingsHint, shortcuts, nameof(vm.CommandShowSettingsCommand)),
            });
            isLastSeparator = false;
        }

        if (appearance.ToolbarShowLayout)
        {
            stackPanelLeft.Children.Add(new Button
            {
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Layout.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.CommandShowLayoutCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.LayoutHint, shortcuts, nameof(vm.CommandShowLayoutCommand)),
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
                Content = new Image
                {
                    Source = MakeOneColor(System.IO.Path.Combine(path, "Help.png")),
                    Width = 32,
                    Height = 32,
                },
                Command = vm.ShowHelpCommand,
                Background = Brushes.Transparent,
                [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.HelpHint, shortcuts, nameof(vm.ShowHelpCommand)),
            });
            isLastSeparator = false;
        }

        if (!isLastSeparator)
        {
            var assaSeparator = MakeSeparator();
            stackPanelLeft.Children.Add(assaSeparator);
            assaSeparator.DataContext = vm;
            assaSeparator.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsFormatAssa)) { Mode = BindingMode.TwoWay });
            isLastSeparator = true;
        }

        stackPanelLeft.Children.Add(new Button
        {
            Content = new Image
            {
                Source = MakeOneColor(System.IO.Path.Combine(path, "AssaStyle.png")),
                Width = 32,
                Height = 32,
            },
            Command = vm.ShowAssaStylesCommand,
            Background = Brushes.Transparent,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaStylesHint, shortcuts, nameof(vm.ShowAssaStylesCommand)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
            {
                Source = vm,
            },
        });

        stackPanelLeft.Children.Add(new Button
        {
            Content = new Image
            {
                Source = MakeOneColor(System.IO.Path.Combine(path, "AssaProperties.png")),
                Width = 32,
                Height = 32,
            },
            Command = vm.ShowAssaPropertiesCommand,
            Background = Brushes.Transparent,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaPropertiesHint, shortcuts, nameof(vm.ShowAssaPropertiesCommand)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
            {
                Source = vm,
            },
        });

        stackPanelLeft.Children.Add(new Button
        {
            Content = new Image
            {
                Source = MakeOneColor(System.IO.Path.Combine(path, "AssaAttachments.png")),
                Width = 32,
                Height = 32,
            },
            Command = vm.ShowAssaAttachmentsCommand,
            Background = Brushes.Transparent,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaAttachmentsHint, shortcuts, nameof(vm.ShowAssaAttachmentsCommand)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
            {
                Source = vm,
            },
        });

        stackPanelLeft.Children.Add(new Button
        {
            Content = new Image
            {
                Source = MakeOneColor(System.IO.Path.Combine(path, "AssaDraw.png")),
                Width = 32,
                Height = 32,
            },
            Command = vm.ShowAssaDrawCommand,
            Background = Brushes.Transparent,
            [ToolTip.TipProperty] = UiUtil.MakeToolTip(languageHints.AssaDrawHint, shortcuts, nameof(vm.ShowAssaDrawCommand)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsFormatAssa))
            {
                Source = vm,
            },
        });

        var stackPanelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Margin = new Thickness(3),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };

        // subtitle formats
        stackPanelRight.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Format,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
        });
        var comboBoxSubtitleFormat = new ComboBox
        {
            Width = 200,
            Height = 30,
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
        comboBoxSubtitleFormat.PointerPressed += vm.ComboBoxSubtitleFormatPointerPressed;   
        stackPanelRight.Children.Add(comboBoxSubtitleFormat);
        isLastSeparator = false;

        if (appearance.ToolbarShowEncoding)
        {
            stackPanelRight.Children.Add(new TextBlock
            {
                Text = Se.Language.General.Encoding,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
            });
            var comboBoxEncoding = new ComboBox
            {
                Width = 200,
                Height = 30,
                [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
                [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
                DataContext = vm,
            };
            stackPanelRight.Children.Add(comboBoxEncoding);
        }

        if (appearance.ToolbarShowFrameRate)
        {
            stackPanelRight.Children.Add(new TextBlock
            {
                Text = Se.Language.General.FrameRate,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
            });
            var comboBoxFrameRate = new ComboBox
            {
                Width = 110,
                Height = 30,
                [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.FrameRates)),
                [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedFrameRate)),
                DataContext = vm,
            };
            stackPanelRight.Children.Add(comboBoxFrameRate);
            comboBoxFrameRate.SelectionChanged += vm.ComboBoxFrameRateSelectionChanged;
        }

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };

        grid.Add(stackPanelLeft, 0, 0);
        grid.Add(stackPanelRight, 0, 1);

        return grid;
    }

    private static unsafe Bitmap MakeOneColor(string filePath)
    {
        if (!UiTheme.IsDarkThemeEnabled())
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

        for (int y = 0; y < height; y++)
        {
            uint* srcRow = (uint*)(srcBase + y * srcStride);
            uint* dstRow = (uint*)(dstBase + y * dstStride);

            for (int x = 0; x < width; x++)
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
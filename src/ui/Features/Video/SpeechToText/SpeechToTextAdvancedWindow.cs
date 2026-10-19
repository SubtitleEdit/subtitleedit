using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class SpeechToTextAdvancedWindow : Window
{
    public SpeechToTextAdvancedWindow(SpeechToText.SpeechToTextAdvancedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.AdvancedWhisperSettings;
        Width = 1050;
        Height = 660;
        MinWidth = 800;
        MinHeight = 400;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.SpeechToText);
        vm.Window = this;
        DataContext = vm;

        var textBoxParameters = new TextBox
        {
            AcceptsReturn = true,
            // A tab is never meaningful inside a whisper command line, and accepting it trapped
            // the keyboard in this box: Tab and Shift+Tab both typed a tab character instead of
            // moving to the next control, with no way out but the mouse (#14313).
            AcceptsTab = false,
            // Long single-line parameter strings used to trigger a horizontal
            // scrollbar that overlapped the text inside this otherwise-tiny
            // multi-line textbox (#11181). Reserve enough vertical room so the
            // scrollbar sits below the text, not on top of it.
            MinHeight = 60,
            TextWrapping = TextWrapping.Wrap,
            VerticalContentAlignment = VerticalAlignment.Top,
            DataContext = vm,
        }.WithAccessibleName(Se.Language.General.Parameters);
        EditorChrome.AsTextField(textBoxParameters);
        textBoxParameters.Bind(TextBox.TextProperty, new Binding(nameof(vm.Parameters))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        // One flat button per engine: shows that engine's help (and its parameters' defaults)
        var buttonVerticalPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
        };
        foreach (var engine in vm.Engines)
        {
            var button = new Button
            {
                Content = new TextBlock { Text = engine.Name, FontSize = 12.5, TextTrimming = TextTrimming.CharacterEllipsis },
                Command = vm.EngineClickedCommand,
                CommandParameter = engine,
                Height = 30,
                Padding = new Thickness(10, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, engine.Name);
            buttonVerticalPanel.Children.Add(button);
        }

        var engineList = new Border
        {
            Child = new ScrollViewer
            {
                Content = EditorChrome.MakeEditorSection(Se.Language.General.Engine, buttonVerticalPanel, first: true),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            },
            Width = 240,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var textBoxHelp = new TextBox
        {
            IsReadOnly = true,
            Margin = new Thickness(0),
            FontSize = 12,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(10, 8),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,  
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,   
            FontFamily = new FontFamily("Consolas,Courier New,monospace"), 
        };
        textBoxHelp.Bind(TextBox.TextProperty, new Binding(nameof(vm.HelpText))
        {
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        textBoxHelp.WithAccessibleName(Se.Language.General.Help); // read-only engine help; no visible label (#12087)
        var scrollViewer = new ScrollViewer
        {
            Content = textBoxHelp,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        // The help text is a read-only TextBox, which moves its caret for the arrows and
        // Home/End - and that drags the ScrollViewer along - but does nothing for PageUp and
        // PageDown, leaving them dead in the one pane long enough to need them (#14313).
        textBoxHelp.AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.PageUp)
            {
                scrollViewer.PageUp();
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                scrollViewer.PageDown();
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        var buttonXxlOptions = new SeSplitButton
        {
            Content = Se.Language.Video.AudioToText.WhisperXxlStandard,
            Command = vm.WhisperXxlSettingStandardCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.AudioToText.WhisperXxlStandardAsia,
                        Command = vm.WhisperXxlSettingStandardAsiaCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Video.AudioToText.WhisperXxlSentence,
                        Command = vm.WhisperXxlSettingSentenceCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Video.AudioToText.WhisperXxlSingleWords,
                        Command = vm.WhisperXxlSettingOneWordCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Video.AudioToText.WhisperXxlHighlightWord,
                        Command = vm.WhisperXxlSettingHighLightWordCommand,
                    },
                }
            }
        }.WithBindIsVisible(nameof(vm.IsWhisperXxlVisible));
        AutomationProperties.SetName(buttonXxlOptions, Se.Language.Video.AudioToText.WhisperXxlStandard);

        // These switch parameters on. As plain buttons there was no way to press one back off
        // again, so anything they set - word-level output above all - stayed on for good.
        static ToggleButton MakeToggleButton(string text, IRelayCommand command, string activePath, string visiblePath)
        {
            var toggleButton = new ToggleButton
            {
                Content = text,
                Margin = new Thickness(0),
                Padding = new Thickness(12, 0),
                Height = 30,
                FontSize = 12.5,
                MinWidth = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Command = command,
            }.WithBindIsVisible(visiblePath);
            toggleButton.Classes.Add("tool");

            // One way only: the parameter text is what says whether the switch is set, the
            // button just shows it.
            toggleButton.Bind(ToggleButton.IsCheckedProperty, new Binding(activePath)
            {
                Mode = BindingMode.OneWay,
            });

            return toggleButton;
        }

        var buttonStandardCrispAsr = UiUtil.MakeButton(Se.Language.Video.AudioToText.WhisperXxlStandard, vm.StandardCrispAsrCommand)
            .WithBindIsVisible(nameof(vm.IsCrispAsrVisible));
        buttonStandardCrispAsr.Margin = new Thickness(0);

        // Presets and parameter switches for the selected engine on the left, OK/Cancel on the right
        var presets = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonXxlOptions,
                MakeToggleButton(Se.Language.Video.AudioToText.EnableVad, vm.EnableVadCppCommand,
                    nameof(vm.IsVadCppActive), nameof(vm.IsWhisperCppVisible)),
                MakeToggleButton(Se.Language.Video.AudioToText.EnableVad, vm.EnableVadCTranslate2Command,
                    nameof(vm.IsVadCTranslate2Active), nameof(vm.IsWhisperCTranslate2Visible)),
                buttonStandardCrispAsr,
                MakeToggleButton(Se.Language.Video.AudioToText.EnableVad, vm.EnableVadCrispAsrCommand,
                    nameof(vm.IsVadCrispAsrActive), nameof(vm.IsCrispAsrVisible)),
                MakeToggleButton(Se.Language.Video.AudioToText.WhisperXxlHighlightWord, vm.EnableHighlightWordsCrispAsrCommand,
                    nameof(vm.IsHighlightWordsCrispAsrActive), nameof(vm.IsCrispAsrVisible)),
                MakeToggleButton(Se.Language.Video.AudioToText.WhisperXxlHighlightWord, vm.EnableWordLevelCppCommand,
                    nameof(vm.IsWordLevelCppActive), nameof(vm.IsWhisperCppVisible)),
                MakeToggleButton(Se.Language.Video.AudioToText.WhisperXxlHighlightWord, vm.WhisperCTranslate2HighLightWordCommand,
                    nameof(vm.IsHighlightWordsCTranslate2Active), nameof(vm.IsWhisperCTranslate2Visible)),
            },
        };

        var buttonBar = EditorChrome.MakeFlushButtonBar(presets, UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand), EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Ok, vm.OkCommand)));

        var parameters = new Border
        {
            Child = EditorChrome.MakeEditorSection(Se.Language.General.Parameters, textBoxParameters, first: true),
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };

        var help = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        help.Add(SpeechToTextChrome.ListHeader(Se.Language.General.Help), 0, 0);
        help.Add(EditorChrome.MakeListFrame(scrollViewer), 1, 0);

        var middle = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        middle.Add(engineList, 0, 0);
        middle.Add(help, 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(parameters, 0, 0);
        grid.Add(middle, 1, 0);
        grid.Add(buttonBar, 2, 0);

        Content = grid;

        // Focus something so the window-level KeyDown below is reachable - but only on the first
        // activation, or Alt+Tabbing back would throw focus away from wherever the user left it.
        UiUtil.FocusOnFirstActivation(this, textBoxParameters);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}

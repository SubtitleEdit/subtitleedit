using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// The Cast dialog. Visual layout:
//
//   ┌──────────────────────────────────────────────────────────────────────┐
//   │ <icon> Cast - assign a voice to each <actor|voice>                   │
//   │ "<count>"                                                            │
//   ├──────────────────────────────────────────────────────────────────────┤
//   │ ┌───────────┬───────────┬───────────┬────────────────────────┬─────┐ │
//   │ │ Actor     │ Engine    │ Voice     │ Instruction            │ ▶  │ │
//   │ ├───────────┼───────────┼───────────┼────────────────────────┼─────┤ │
//   │ │ Joe       │ Edge TTS  │ AriaNeu…  │                        │ ▶  │ │
//   │ │ Maria     │ ElevenLabs│ Rachel    │                        │ ▶  │ │
//   │ └───────────┴───────────┴───────────┴────────────────────────┴─────┘ │
//   ├──────────────────────────────────────────────────────────────────────┤
//   │ [Apply default to all] [Clear all]              [OK] [Cancel]        │
//   └──────────────────────────────────────────────────────────────────────┘
public class ActorVoiceMappingWindow : Window
{
    private readonly ActorVoiceMappingViewModel _vm;

    public ActorVoiceMappingWindow(ActorVoiceMappingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ActorVoicesTitle;
        Width = 950;
        Height = 560;
        MinWidth = 700;
        MinHeight = 400;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = BuildHeader(vm);
        var grid = BuildGrid(vm);
        var buttons = BuildButtons(vm);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 12,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        root.Add(header, 0, 0);
        root.Add(grid, 1, 0);
        root.Add(buttons, 2, 0);

        Content = root;

        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
    }

    private static Control BuildHeader(ActorVoiceMappingViewModel vm)
    {
        var icon = new Icon
        {
            Value = IconNames.PoliceBadge,
            FontSize = 28,
            Foreground = UiUtil.GetTextColor(0.8d),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var title = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.ActorVoicesTitle,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var subtitle = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.ActorVoicesSubtitle,
            FontSize = 12,
            Foreground = UiUtil.GetTextColor(0.6d),
            TextWrapping = TextWrapping.Wrap,
        };

        var status = new TextBlock
        {
            FontSize = 11,
            Foreground = UiUtil.GetTextColor(0.55d),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)) { Mode = BindingMode.OneWay },
        };

        var textBlock = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { title, subtitle, status },
        };

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { icon, textBlock },
        };

        return new Border
        {
            Padding = new Thickness(4, 0),
            Child = row,
        };
    }

    private static Border BuildGrid(ActorVoiceMappingViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        tableView[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Rows));
        tableView[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedRow)) { Mode = BindingMode.TwoWay };
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.TextToSpeech.ActorOrVoice,
            Binding = new Binding(nameof(ActorVoiceRow.Actor)),
            Width = new GridLength(180),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Engine,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ActorVoiceRow>((row, _) =>
            {
                if (row == null)
                {
                    return new TextBlock();
                }

                var combo = new ComboBox
                {
                    ItemsSource = vm.Engines,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(4, 2),
                    MinWidth = 160,
                    DisplayMemberBinding = new Binding(nameof(ITtsEngine.Name)),
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(ActorVoiceRow.SelectedEngine))
                    {
                        Mode = BindingMode.TwoWay,
                    },
                };
                return combo;
            }),
            Width = new GridLength(200),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Model,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ActorVoiceRow>((row, _) =>
            {
                if (row == null)
                {
                    return new TextBlock();
                }

                // Hidden for engines without a model concept (Piper, Edge TTS, ...).
                // Empty cell is a clearer "n/a" than a disabled combo full of nothing.
                var combo = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(4, 2),
                    MinWidth = 140,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(ActorVoiceRow.Models)),
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(ActorVoiceRow.SelectedModel))
                    {
                        Mode = BindingMode.TwoWay,
                    },
                    [!Visual.IsVisibleProperty] = new Binding(nameof(ActorVoiceRow.HasModel))
                    {
                        Mode = BindingMode.OneWay,
                    },
                };
                return combo;
            }),
            Width = new GridLength(170),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Voice,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ActorVoiceRow>((row, _) =>
            {
                if (row == null)
                {
                    return new TextBlock();
                }

                // Single IsEnabled binding via the row's combined IsVoiceComboReady
                // (= IsVoiceComboEnabled && !IsBusy). Avalonia's Bind() replaces an
                // existing binding on the same property, so binding twice (once to
                // IsVoiceComboEnabled and once to !IsBusy) would silently drop the
                // first.
                var combo = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(4, 2),
                    MinWidth = 200,
                    [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(ActorVoiceRow.Voices)),
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(ActorVoiceRow.SelectedVoice))
                    {
                        Mode = BindingMode.TwoWay,
                    },
                    [!InputElement.IsEnabledProperty] = new Binding(nameof(ActorVoiceRow.IsVoiceComboReady))
                    {
                        Mode = BindingMode.OneWay,
                    },
                    DisplayMemberBinding = new Binding(nameof(Voice.Name)),
                };

                var busy = new TextBlock
                {
                    Text = "…",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(4),
                    Opacity = 0.6,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(ActorVoiceRow.IsBusy))
                    {
                        Mode = BindingMode.OneWay,
                    },
                };

                return new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Children = { combo, busy },
                };
            }),
            Width = new GridLength(1, GridUnitType.Star),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ActorVoiceRow>((row, _) =>
            {
                if (row == null)
                {
                    return new TextBlock();
                }

                // Per-row "..." button → opens ActorVoiceRowSettingsWindow for voice
                // instruction + OmniVoice keyword picker. Disabled for engines that have
                // no advanced settings so the user doesn't open an empty dialog.
                var settingsBtn = UiUtil.MakeButton(vm.ShowRowSettingsCommand, IconNames.Settings,
                    Se.Language.Video.TextToSpeech.ActorVoicesRowSettingsTitle);
                settingsBtn.CommandParameter = row;
                settingsBtn.Margin = new Thickness(4, 2);
                settingsBtn.Bind(InputElement.IsEnabledProperty, new Binding(nameof(ActorVoiceRow.HasAdvancedSettings))
                {
                    Mode = BindingMode.OneWay,
                });

                var testBtn = UiUtil.MakeButton(vm.TestRowCommand, "fa-solid fa-play", Se.Language.Video.TextToSpeech.TestVoice);
                testBtn.CommandParameter = row;
                testBtn.Margin = new Thickness(4, 2);
                testBtn.Bind(InputElement.IsEnabledProperty, new Binding(nameof(ActorVoiceRow.SelectedVoice))
                {
                    Mode = BindingMode.OneWay,
                    Converter = new NotNullConverter(),
                });

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 4,
                    Children = { settingsBtn, testBtn },
                };
            }),
            Width = new GridLength(110),
        });

        return UiUtil.MakeBorderForControl(tableView);
    }

    private static StackPanel BuildButtons(ActorVoiceMappingViewModel vm)
    {
        var applyDefault = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.ApplyDefaultToAll, vm.ApplyDefaultToAllCommand);
        var clearAll = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearAllCommand);
        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return UiUtil.MakeButtonBar(applyDefault, clearAll, ok, cancel);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }
}

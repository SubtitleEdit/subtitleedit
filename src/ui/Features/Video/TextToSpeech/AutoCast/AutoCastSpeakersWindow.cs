using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

// The speakers dialog, shown between diarization and cloning:
//
//   ┌──────────────────────────────────────────────────────────────────────┐
//   │ <icon> Voices found in the video                                     │
//   │ Give a speaker a name...  same name twice = one voice                │
//   ├──────────────────────────────────────────────────────────────────────┤
//   │ ┌────────────┬───────┬────────┬──────────────────────────────────┐   │
//   │ │ Name       │ Lines │ Audio  │ Says                             │   │
//   │ ├────────────┼───────┼────────┼──────────────────────────────────┤   │
//   │ │ [Speaker 1]│  42   │ 03:12  │ "I told you this would happen."  │   │
//   │ │ [Speaker 2]│  18   │ 01:04  │ "And nobody listened."           │   │
//   │ └────────────┴───────┴────────┴──────────────────────────────────┘   │
//   ├──────────────────────────────────────────────────────────────────────┤
//   │ Engine [OmniVoice TTS ▾]                          [OK] [Cancel]      │
//   └──────────────────────────────────────────────────────────────────────┘
public class AutoCastSpeakersWindow : Window
{
    public AutoCastSpeakersWindow(AutoCastSpeakersViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.AutoCastSpeakersTitle;
        Width = 820;
        Height = 480;
        MinWidth = 600;
        MinHeight = 360;
        CanResize = true;

        vm.Window = this;
        DataContext = vm;

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 12,
        };

        root.Add(BuildHeader(vm), 0, 0);
        root.Add(BuildTable(vm), 1, 0);
        root.Add(BuildButtons(vm), 2, 0);

        Content = root;

        KeyDown += (_, e) => vm.OnKeyDown(e);
        // SaveWindowPosition is the only writer of the stored position, so without this the
        // Restore above could never find anything: this resizable table dialog reopened at its
        // hard-coded default every run. The select-lines dialogs pair the two handlers.
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
    }

    private static Control BuildHeader(AutoCastSpeakersViewModel vm)
    {
        var icon = new Icon
        {
            Value = IconNames.AccountVoice,
            FontSize = 28,
            Foreground = UiUtil.GetTextColor(0.8d),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var title = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.AutoCastSpeakersTitle,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.AutoCastSpeakersSubtitle,
            FontSize = 12,
            Foreground = UiUtil.GetTextColor(0.6d),
            TextWrapping = TextWrapping.Wrap,
        };

        var summary = new TextBlock
        {
            FontSize = 11,
            Foreground = UiUtil.GetTextColor(0.55d),
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SummaryText)) { Mode = BindingMode.OneWay },
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                icon,
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 2,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { title, subtitle, summary },
                },
            },
        };
    }

    private static Border BuildTable(AutoCastSpeakersViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        tableView[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Rows));

        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(180),
            // A text box rather than a label: naming a speaker is the whole point of this dialog,
            // and typing the same name into two rows is how they are merged.
            CellTemplate = new FuncDataTemplate<AutoCastSpeakerRow>((_, _) => new TextBox
            {
                Margin = new Thickness(4, 2),
                [!TextBox.TextProperty] = new Binding(nameof(AutoCastSpeakerRow.Name)) { Mode = BindingMode.TwoWay },
            }),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.TextToSpeech.AutoCastLines,
            Binding = new Binding(nameof(AutoCastSpeakerRow.LineCount)),
            Width = new GridLength(70),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.TextToSpeech.AutoCastAudio,
            Binding = new Binding(nameof(AutoCastSpeakerRow.TotalDurationDisplay)),
            Width = new GridLength(80),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.TextToSpeech.AutoCastSays,
            Binding = new Binding(nameof(AutoCastSpeakerRow.SampleText)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.BindSelectedItem(tableView, vm, nameof(vm.SelectedRow));

        return new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = tableView,
        };
    }

    private static Control BuildButtons(AutoCastSpeakersViewModel vm)
    {
        var engineLabel = UiUtil.MakeLabel(Se.Language.General.Engine);
        var engineCombo = new ComboBox
        {
            MinWidth = 220,
            VerticalAlignment = VerticalAlignment.Center,
            DisplayMemberBinding = new Binding(nameof(ITtsEngine.Name)),
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Engines)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedEngine)) { Mode = BindingMode.TwoWay },
        };

        var enginePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { engineLabel, engineCombo },
        };

        var buttons = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var panel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        panel.Add(enginePanel, 0, 0);
        panel.Add(buttons, 0, 1);
        return panel;
    }
}

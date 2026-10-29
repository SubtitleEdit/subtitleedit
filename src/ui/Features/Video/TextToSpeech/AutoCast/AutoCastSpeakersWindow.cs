using Avalonia;
using Avalonia.Automation;
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

// The speakers dialog, shown between diarization and cloning - in the editor look (see
// EditorChrome): header on the panel color, the table in a field frame, button bar at the bottom:
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
//   │ Engine [OmniVoice TTS ▾]                          [Cancel] [OK]      │
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);

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
        };

        root.Add(new Border
        {
            Child = BuildHeader(vm),
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(16, 12),
        }, 0, 0);
        root.Add(new Border
        {
            Child = BuildTable(vm),
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(14, 12, 14, 14),
        }, 1, 0);
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
            FontSize = 26,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var title = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.AutoCastSpeakersTitle,
            FontSize = EditorChrome.ScaledFont(15),
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.AutoCastSpeakersSubtitle,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };

        var summary = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(11.5),
            Foreground = EditorChrome.FaintTextBrush,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SummaryText)) { Mode = BindingMode.OneWay },
        };

        // A grid rather than a horizontal stack panel, so the subtitle gets the window width and
        // wraps instead of running off the right edge.
        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(icon, 0, 0);
        grid.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { title, subtitle, summary },
        }, 0, 1);
        return grid;
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
            CellTemplate = new FuncDataTemplate<AutoCastSpeakerRow>((_, _) =>
            {
                var textBox = new TextBox
                {
                    Margin = new Thickness(4, 2),
                    [!TextBox.TextProperty] = new Binding(nameof(AutoCastSpeakerRow.Name)) { Mode = BindingMode.TwoWay },
                };
                textBox.Classes.Add("text");
                AutomationProperties.SetName(textBox, Se.Language.General.Name);
                return textBox;
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
            Header = Se.Language.General.Audio,
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
        AutomationProperties.SetName(tableView, Se.Language.Video.TextToSpeech.AutoCastSpeakersTitle);

        return EditorChrome.MakeListFrame(tableView);
    }

    private static Border BuildButtons(AutoCastSpeakersViewModel vm)
    {
        var engineCombo = new ComboBox
        {
            MinWidth = 220,
            VerticalAlignment = VerticalAlignment.Center,
            DisplayMemberBinding = new Binding(nameof(ITtsEngine.Name)),
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Engines)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedEngine)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(engineCombo, Se.Language.General.Engine);

        var enginePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { EditorChrome.MakeDimLabel(Se.Language.General.Engine, new Thickness(0)), engineCombo },
        };

        return EditorChrome.MakeButtonBar(enginePanel, UiUtil.MakeButtonCancel(vm.CancelCommand), EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand)));
    }
}

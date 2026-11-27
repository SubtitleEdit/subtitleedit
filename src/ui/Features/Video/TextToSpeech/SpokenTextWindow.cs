using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

internal sealed class SpokenTextWindow : Window
{
    internal sealed class Row(Paragraph paragraph, string? spokenText)
    {
        public Paragraph Paragraph { get; } = paragraph;
        public string SubtitleText => Paragraph.Text;
        public string Label => $"{Paragraph.StartTime}  {Utilities.UnbreakLine(SubtitleText)}";
        public string? SpokenText { get; set; } = spokenText;
    }

    public SpokenTextWindow(IReadOnlyList<Row> rows)
    {
        var language = Se.Language.Video.TextToSpeech;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = language.SpokenText;
        Width = 820;
        Height = 455;
        MinWidth = 580;
        MinHeight = 350;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        var lines = new ListBox { ItemsSource = rows, DisplayMemberBinding = new Binding(nameof(Row.Label)) };
        var subtitle = new TextBox { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, MinHeight = 68 };
        subtitle.Bind(TextBox.TextProperty, new Binding($"SelectedItem.{nameof(Row.SubtitleText)}") { Source = lines });
        var spoken = new TextBox
        {
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            MinHeight = 100,
            PlaceholderText = language.SpokenTextPlaceholder,
        };
        spoken.Bind(TextBox.TextProperty, new Binding($"SelectedItem.{nameof(Row.SpokenText)}")
        {
            Source = lines,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        lines.SelectedIndex = 0;

        var fields = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock { Text = Se.Language.General.Text },
                subtitle.WithAccessibleName(Se.Language.General.Text),
                new TextBlock { Text = language.SpokenText },
                spoken.WithAccessibleName(language.SpokenText),
                new TextBlock { Text = language.SpokenTextHint, TextWrapping = TextWrapping.Wrap },
            },
        };

        var body = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 12, Margin = new Thickness(12) };
        body.Children.Add(lines);
        Grid.SetColumn(fields, 1);
        body.Children.Add(fields);

        var ok = UiUtil.MakeButtonOk(null);
        ok.Click += (_, _) => Close(true);
        var cancel = UiUtil.MakeButtonCancel(null);
        cancel.Click += (_, _) => Close(false);
        var buttons = EditorChrome.MakeButtonBar(null, cancel, ok);

        var layout = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Bottom);
        layout.Children.Add(buttons);
        layout.Children.Add(body);
        Content = layout;
        UiUtil.FocusOnFirstActivation(this, lines);
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close(false);
            }
        };
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The override tag chips above the text box in layout 14: one chip per tag of the block the
/// current line starts with, each with an × that removes it.
/// </summary>
internal static class AssaTagChips
{
    private static readonly FontFamily MonospaceFont = new("Cascadia Mono,Consolas,Menlo,Monaco,monospace");

    /// <summary>
    /// The tags of the current line's leading override block as chips, each with an × that
    /// removes it, plus a + that opens "Apply override tags". Hidden for formats other than
    /// ASSA/SSA and for lines without tags (the + stays for ASSA).
    /// </summary>
    internal static Control Make(MainViewModel vm)
    {
        var panel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var addButton = new Button
        {
            Content = "+",
            Padding = new Thickness(7, 0),
            Margin = new Thickness(2, 1),
            MinHeight = 0,
            FontSize = EditorChrome.ScaledFont(11),
            [AutomationProperties.NameProperty] = Se.Language.Main.Menu.AssaApplyCustomOverrideTags.Replace("_", string.Empty),
        };
        ToolTip.SetTip(addButton, Se.Language.Main.Menu.AssaApplyCustomOverrideTags.Replace("_", string.Empty));
        addButton.Click += async (_, _) => await vm.ShowAssaApplyCustomOverrideTagsCommand.ExecuteAsync(null);

        var shown = new List<string>();

        void Refresh()
        {
            var line = vm.SelectedSubtitle;
            var tags = vm.IsFormatAssa || vm.IsFormatSsa ? AssaOverrideTags.GetLeadingTags(line?.Text) : [];
            if (panel.Children.Count > 0 && tags.SequenceEqual(shown))
            {
                addButton.IsVisible = vm.IsFormatAssa && line != null;
                return;
            }

            shown = tags;
            panel.Children.Clear();
            for (var i = 0; i < tags.Count; i++)
            {
                panel.Children.Add(MakeChip(vm, tags[i], i));
            }

            addButton.IsVisible = vm.IsFormatAssa && line != null;
            panel.Children.Add(addButton);
        }

        SubtitleLineViewModel? watchedLine = null;
        void OnLineChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SubtitleLineViewModel.Text))
            {
                Refresh();
            }
        }

        void Watch(SubtitleLineViewModel? line)
        {
            if (watchedLine != null)
            {
                watchedLine.PropertyChanged -= OnLineChanged;
            }

            watchedLine = line;
            if (watchedLine != null)
            {
                watchedLine.PropertyChanged += OnLineChanged;
            }
        }

        void OnVmChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MainViewModel.SelectedSubtitle) or nameof(MainViewModel.IsFormatAssa) or nameof(MainViewModel.IsFormatSsa))
            {
                Watch(vm.SelectedSubtitle);
                Refresh();
            }
        }

        panel.AttachedToVisualTree += (_, _) =>
        {
            vm.PropertyChanged += OnVmChanged;
            Watch(vm.SelectedSubtitle);
            panel.Children.Clear();
            Refresh();
        };
        panel.DetachedFromVisualTree += (_, _) =>
        {
            vm.PropertyChanged -= OnVmChanged;
            Watch(null);
        };

        return panel;
    }

    private static Control MakeChip(MainViewModel vm, string tag, int index)
    {
        var remove = new TextBlock
        {
            Text = "×",
            Margin = new Thickness(5, 0, 0, 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            Foreground = EditorChrome.WarningTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(remove, Se.Language.Assa.LayoutRemoveTag);
        remove.PointerPressed += (_, e) =>
        {
            var line = vm.SelectedSubtitle;
            if (line != null)
            {
                line.Text = AssaOverrideTags.RemoveLeadingTag(line.Text, index);
            }

            e.Handled = true;
        };

        var text = new TextBlock
        {
            Text = tag,
            FontFamily = MonospaceFont,
            FontSize = EditorChrome.ScaledFont(11),
            Foreground = EditorChrome.WarningTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };

        return new Border
        {
            CornerRadius = new CornerRadius(9),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.WarningTextBrush,
            Background = EditorChrome.FieldBrush,
            Padding = new Thickness(7, 0, 6, 1),
            Margin = new Thickness(2, 1),
            Child = new StackPanel { Orientation = Orientation.Horizontal, Children = { text, remove } },
        };
    }
}

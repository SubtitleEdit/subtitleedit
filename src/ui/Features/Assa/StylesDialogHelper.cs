using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// Logic shared by the ASSA and SSA styles windows (AssaStylesViewModel, SsaStylesViewModel).
/// The SSA window started as a copy of the ASSA one and kept missing its later fixes, so
/// behavior both need lives here instead of in two copies.
/// </summary>
public static class StylesDialogHelper
{

    /// <summary>
    /// Default and smallest width of the style lists pane, left of the splitter. The lists
    /// show name, font name, size, usages/category and a color sample, so a narrow pane
    /// cuts the names off (#16251).
    /// </summary>
    public const double DefaultListsWidth = 480;
    public const double MinListsWidth = 300;

    /// <summary>
    /// "Font name" list column.
    /// </summary>
    public static SeTableViewColumn MakeFontNameColumn(double width = 130) => new()
    {
        Header = Se.Language.General.FontName,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        Binding = new Binding(nameof(StyleDisplay.FontName)),
        Width = new GridLength(width),
    };

    /// <summary>
    /// Color sample list column: "Aa" in the primary color on the outline color, so the
    /// colors of every style show without selecting it - as SE4's advanced styles list did (#16251).
    /// </summary>
    public static SeTableViewColumn MakeColorSampleColumn() => new()
    {
        Header = Se.Language.General.Color,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        CellTemplate = new FuncDataTemplate<StyleDisplay>((_, _) => new Border
        {
            Width = 46,
            Height = 18,
            CornerRadius = new CornerRadius(3),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(nameof(StyleDisplay.ColorOutline)) { Converter = ColorToBrush },
            Child = new TextBlock
            {
                Text = "Aa",
                FontSize = EditorChrome.ScaledFont(11.5),
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.ForegroundProperty] = new Binding(nameof(StyleDisplay.ColorPrimary)) { Converter = ColorToBrush },
                [!TextBlock.FontStyleProperty] = new Binding(nameof(StyleDisplay.Italic)) { Converter = ItalicToFontStyle },
            },
        }),
        NameBinding = new Binding(nameof(StyleDisplay.ColorPrimary)) { Converter = ColorToName },
        Width = new GridLength(58),
    };

    private static readonly IValueConverter ColorToBrush = new FuncValueConverter<Color, IBrush>(c => new SolidColorBrush(c));
    private static readonly IValueConverter ItalicToFontStyle = new FuncValueConverter<bool, FontStyle>(italic => italic ? FontStyle.Italic : FontStyle.Normal);
    private static readonly IValueConverter ColorToName = new FuncValueConverter<Color, string>(c => c.FromColorToHex(false));

    /// <summary>
    /// Puts the lists pane and the right side in <paramref name="content"/> with a draggable
    /// splitter between them. The pane width is remembered across windows (ASSA and SSA share it)
    /// and saved when <paramref name="window"/> closes.
    /// </summary>
    public static void AddListsSplitter(Window window, Grid content, Control lists, Control right)
    {
        var width = Se.Settings.Assa.StylesListsWidth >= MinListsWidth ? Se.Settings.Assa.StylesListsWidth : DefaultListsWidth;
        content.ColumnDefinitions.Clear();
        content.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(width)) { MinWidth = MinListsWidth });
        content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        content.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)) { MinWidth = 560 });

        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = EditorChrome.LineBrush,
        };

        content.Add(lists, 0, 0);
        content.Add(splitter, 0, 1);
        content.Add(right, 0, 2);

        window.Closing += delegate
        {
            var actual = content.ColumnDefinitions[0].ActualWidth;
            if (actual >= MinListsWidth)
            {
                Se.Settings.Assa.StylesListsWidth = (int)Math.Round(actual);
            }
        };
    }
    /// <summary>
    /// Asks how to resolve a name clash: returns Custom1 (overwrite), Custom2 (keep both) or
    /// anything else (cancel), plus whether the answer applies to all remaining clashes.
    /// The bool argument tells whether more than one style clashes.
    /// </summary>
    internal delegate Task<(MessageBoxResult Answer, bool ForAll)> AskOverwrite(string message, bool hasMoreConflicts);

    /// <summary>
    /// <paramref name="name"/>, or with "_2", "_3", ... appended when the name is already taken
    /// in <paramref name="styles"/> (case-insensitive).
    /// </summary>
    public static string MakeUniqueName(string name, IEnumerable<StyleDisplay> styles)
    {
        var list = styles as ICollection<StyleDisplay> ?? styles.ToList();
        var newName = name;
        var count = 2;
        while (list.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            newName = name + "_" + count;
            count++;
        }

        return newName;
    }

    /// <summary>
    /// Copies styles between the file and storage lists. A name clash asks whether to
    /// overwrite the existing style or keep both - always adding a "_2" copy made it
    /// impossible to update a saved style from an edited file style (#15312). An overwrite
    /// keeps the target's position, name, category and default flag.
    /// Only the target styles matching <paramref name="isInConflictScope"/> can clash - in storage
    /// that is the category the copies land in, as names only need to be unique per category (#15332).
    /// </summary>
    public static Task CopyStyles(
        Window window,
        List<StyleDisplay> sourceStyles,
        ObservableCollection<StyleDisplay> target,
        Func<StyleDisplay, bool> isInConflictScope,
        string alreadyExistsFormat,
        Func<SsaStyle, StyleDisplay> makeNew,
        Action<StyleDisplay>? onOverwritten = null)
    {
        return CopyStyles(sourceStyles, target, isInConflictScope, alreadyExistsFormat, makeNew, onOverwritten, async (message, hasMoreConflicts) =>
        {
            if (hasMoreConflicts)
            {
                return await MessageBox.ShowWithDoNotAskAgain(
                    window,
                    Se.Language.General.OverwriteQuestion,
                    message,
                    Se.Language.Assa.DoThisForAllConflictingStyles,
                    MessageBoxButtons.Cancel,
                    MessageBoxIcon.Question,
                    Se.Language.Assa.Overwrite,
                    Se.Language.Assa.KeepBoth);
            }

            var answer = await MessageBox.Show(
                window,
                Se.Language.General.OverwriteQuestion,
                message,
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                Se.Language.Assa.Overwrite,
                Se.Language.Assa.KeepBoth);
            return (answer, false);
        });
    }

    internal static async Task CopyStyles(
        List<StyleDisplay> sourceStyles,
        ObservableCollection<StyleDisplay> target,
        Func<StyleDisplay, bool> isInConflictScope,
        string alreadyExistsFormat,
        Func<SsaStyle, StyleDisplay> makeNew,
        Action<StyleDisplay>? onOverwritten,
        AskOverwrite ask)
    {
        var conflictCount = sourceStyles.Count(s => target.Any(t => isInConflictScope(t) && t.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase)));
        MessageBoxResult? answerForAll = null;

        foreach (var item in sourceStyles)
        {
            var existing = target.FirstOrDefault(p => isInConflictScope(p) && p.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                target.Add(makeNew(item.ToSsaStyle()));
                continue;
            }

            var answer = answerForAll;
            if (answer == null)
            {
                var (result, forAll) = await ask(string.Format(alreadyExistsFormat, item.Name), conflictCount > 1);
                answer = result;
                if (forAll)
                {
                    answerForAll = result;
                }
            }

            if (answer == MessageBoxResult.Custom1)
            {
                existing.CopyFormattingFrom(item);
                onOverwritten?.Invoke(existing);
            }
            else if (answer == MessageBoxResult.Custom2)
            {
                var style = item.ToSsaStyle();
                style.Name = MakeUniqueName(style.Name, target.Where(isInConflictScope));
                target.Add(makeNew(style));
            }
            else
            {
                return;
            }
        }
    }

    /// <summary>
    /// Asks before deleting styles (when "prompt before delete" is on). True = go ahead.
    /// </summary>
    public static async Task<bool> ConfirmDeleteStyles(Window? window, string message)
    {
        if (window == null || !Se.Settings.General.PromptBeforeDelete)
        {
            return true;
        }

        var answer = await MessageBox.Show(
            window,
            Se.Language.Assa.DeleteStylesQuestion,
            message,
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        return answer == MessageBoxResult.Yes;
    }

    /// <summary>
    /// "Replace style with..." candidates: the other file styles, plus storage styles whose
    /// name is not already in the file.
    /// </summary>
    public static List<StyleDisplay> GetReplaceWithCandidates(
        IEnumerable<StyleDisplay> fileStyles,
        IEnumerable<StyleDisplay> storageStyles,
        ISet<string> oldNames)
    {
        var files = fileStyles.ToList();
        var candidates = files.Where(p => !oldNames.Contains(p.Name)).ToList();
        candidates.AddRange(storageStyles.Where(s => files.All(f => !f.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase))));
        return candidates;
    }

    /// <summary>
    /// Replaces the <paramref name="replacedStyles"/> with <paramref name="target"/>: the target is
    /// added to the file styles if missing (via <paramref name="makeNew"/>), every line using a
    /// replaced style is re-pointed to it, and the replaced styles are removed (never the target).
    /// Returns the target as it is in the file styles.
    /// </summary>
    public static StyleDisplay ReplaceStylesWith(
        Subtitle subtitle,
        ObservableCollection<StyleDisplay> fileStyles,
        IReadOnlyList<StyleDisplay> replacedStyles,
        StyleDisplay target,
        Func<SsaStyle, StyleDisplay> makeNew)
    {
        var oldNames = replacedStyles.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var targetInFile = fileStyles.FirstOrDefault(f => f.Name.Equals(target.Name, StringComparison.OrdinalIgnoreCase));
        if (targetInFile == null)
        {
            targetInFile = makeNew(target.ToSsaStyle());
            fileStyles.Add(targetInFile);
        }

        RepointParagraphsToStyle(subtitle, oldNames, targetInFile.Name);

        foreach (var old in replacedStyles.Where(s => !s.Name.Equals(targetInFile.Name, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            fileStyles.Remove(old);
        }

        return targetInFile;
    }

    /// <summary>
    /// Re-points every paragraph that uses one of <paramref name="oldNames"/> (its style, via Extra,
    /// optionally prefixed with '*') to <paramref name="targetName"/>. Used by "Replace style with...".
    /// </summary>
    public static void RepointParagraphsToStyle(Subtitle subtitle, ISet<string> oldNames, string targetName)
    {
        foreach (var paragraph in subtitle.Paragraphs)
        {
            if (paragraph.Extra != null && oldNames.Contains(paragraph.Extra.TrimStart('*')))
            {
                paragraph.Extra = targetName;
            }
        }
    }

    /// <summary>
    /// Ctrl+Up/Ctrl+Down reorder the selected styles, as in SE 4. Attach tunneled, because the
    /// ListBox underneath TableView handles Ctrl+Arrow itself (move focus without changing
    /// the selection) and a bubbling handler would never see the key.
    /// </summary>
    public static void HandleMoveKeyDown(KeyEventArgs e, Action<ListMoveDirection> move)
    {
        if (e.KeyModifiers != KeyModifiers.Control || e.Source is TextBox)
        {
            return;
        }

        if (e.Key == Key.Up)
        {
            move(ListMoveDirection.Up);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            move(ListMoveDirection.Down);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Adds the "move up/down/to top/to bottom" block to a list's context menu (styles,
    /// attachments). The lists are saved in list order, so this is real reordering, not a view
    /// sort. The items are shown while the bool property <paramref name="isVisiblePropertyName"/>
    /// of <paramref name="vm"/> is true.
    /// </summary>
    public static void AddMoveMenuItems(MenuFlyout flyout, object vm, string isVisiblePropertyName, ICommand moveUp, ICommand moveDown, ICommand moveToTop, ICommand moveToBottom)
    {
        var separator = new Separator();
        separator.Bind(Visual.IsVisibleProperty, new Binding(isVisiblePropertyName) { Source = vm });
        flyout.Items.Add(separator);

        var items = new (string Header, ICommand Command, KeyGesture? Gesture)[]
        {
            (Se.Language.General.MoveUp, moveUp, new KeyGesture(Key.Up, KeyModifiers.Control)),
            (Se.Language.General.MoveDown, moveDown, new KeyGesture(Key.Down, KeyModifiers.Control)),
            (Se.Language.General.MoveToTop, moveToTop, null),
            (Se.Language.General.MoveToBottom, moveToBottom, null),
        };

        foreach (var (header, command, gesture) in items)
        {
            var menuItem = new MenuItem
            {
                Header = header,
                DataContext = vm,
                Command = command,
                InputGesture = gesture,
            };
            menuItem.Bind(Visual.IsVisibleProperty, new Binding(isVisiblePropertyName) { Source = vm });
            flyout.Items.Add(menuItem);
        }
    }

    /// <summary>
    /// The font names for the style editor's font combo box. Fonts collected in SE's own Fonts
    /// folder come first - they may not be installed on the system, but a mux/render with the
    /// collected files will resolve them.
    /// </summary>
    public static List<string> GetStyleEditorFontNames()
    {
        var fonts = FontHelper.GetFontsFolderFontNames();
        fonts.AddRange(FontHelper.GetLibAssaFonts());
        return fonts;
    }
}

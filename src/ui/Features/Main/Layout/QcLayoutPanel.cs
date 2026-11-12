using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The quality control panel of layout 14: the errors of the whole file with a Fix button where
/// one applies, the bookmarks, and a bar to play (or speak) the lines being checked.
/// </summary>
internal static class QcLayoutPanel
{
    /// <summary>One error on one line, with the command that fixes it (null: no automatic fix).</summary>
    internal sealed record Issue(SubtitleLineViewModel Line, LineError Error, IRelayCommand? Fix);

    /// <summary>Edits come in bursts (typing, a drag); the lists are rebuilt once they settle.</summary>
    internal static readonly TimeSpan RefreshDelay = TimeSpan.FromMilliseconds(400);

    private static readonly ConditionalWeakTable<Control, DispatcherTimer> RefreshTimers = new();

    /// <summary>Whether an edit has queued a rebuild of the lists that has not run yet.</summary>
    internal static bool IsRefreshPending(Control panel) => RefreshTimers.TryGetValue(panel, out var timer) && timer.IsEnabled;

    /// <summary>Runs a queued rebuild now - for tests, as the headless platform does not tick timers.</summary>
    internal static void RunPendingRefresh(Control panel)
    {
        if (RefreshTimers.TryGetValue(panel, out var timer) && timer.IsEnabled)
        {
            timer.Stop();
            if (timer.Tag is Action refresh)
            {
                refresh();
            }
        }
    }

    /// <summary>
    /// The command that fixes an error on the selected line, using the same commands as the
    /// shortcuts: auto break for text that is too long or has too many lines, the max-CPS
    /// duration for timing that is too short, too long or too fast, and "extend to next" (end at
    /// the next start minus the minimum gap) for overlaps and short gaps. Gap-in-range and custom
    /// rules have no automatic fix.
    /// </summary>
    internal static IRelayCommand? GetFix(MainViewModel vm, LineErrorType type)
    {
        return type switch
        {
            LineErrorType.TooManyLines or LineErrorType.LineTooLong or LineErrorType.LineTooWide => vm.AutoBreakCommand,
            LineErrorType.CharactersPerSecond or LineErrorType.DurationTooShort or LineErrorType.DurationTooLong => vm.SetDurationMaxCpsSelectedLinesCommand,
            LineErrorType.Overlap or LineErrorType.GapTooShort => vm.ExtendSelectedToNextCommand,
            _ => null,
        };
    }

    /// <summary>Every error in the file, in line order, with neighbours taken from the subtitle (as "List errors" does).</summary>
    internal static List<Issue> FindIssues(MainViewModel vm)
    {
        var issues = new List<Issue>();
        var lines = vm.Subtitles;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var prev = i > 0 ? lines[i - 1] : null;
            var next = i < lines.Count - 1 ? lines[i + 1] : null;
            if (!line.HasErrors(prev, next))
            {
                continue;
            }

            foreach (var error in line.GetErrorList(prev, next))
            {
                issues.Add(new Issue(line, error, GetFix(vm, error.Type)));
            }
        }

        return issues;
    }

    internal static Control Make(MainViewModel vm)
    {
        var issuesTitle = MakeTitle();
        var issuesList = new ListBox
        {
            Background = Brushes.Transparent,
            [AutomationProperties.NameProperty] = Se.Language.Main.QcIssues,
        };
        var noIssues = new TextBlock
        {
            Text = Se.Language.ErrorList.NoErrors,
            Foreground = EditorChrome.SuccessTextBrush,
            Margin = new Thickness(10, 6),
            IsVisible = false,
        };

        var bookmarksTitle = MakeTitle();
        var bookmarksList = new ListBox
        {
            Background = Brushes.Transparent,
            [AutomationProperties.NameProperty] = Se.Language.Main.QcBookmarks,
        };
        var noBookmarks = new TextBlock
        {
            Text = Se.Language.Main.QcNoBookmarks,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(10, 6),
            IsVisible = false,
        };

        var addBookmark = MakeSmallButton("+", Se.Language.Options.Shortcuts.AddOrEditBookmark, vm.AddOrEditBookmarkCommand);
        var removeBookmark = MakeSmallButton("−", Se.Language.Main.QcRemoveBookmark, vm.RemoveBookmarkSelectedLinesCommand);
        var bookmarksHeader = new DockPanel { Margin = new Thickness(0, 0, 6, 0) };
        var bookmarkButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center, Children = { addBookmark, removeBookmark } };
        DockPanel.SetDock(bookmarkButtons, Dock.Right);
        bookmarksHeader.Children.Add(bookmarkButtons);
        bookmarksHeader.Children.Add(bookmarksTitle);

        // The template also runs with a null item while the list swaps its items.
        issuesList.ItemTemplate = new FuncDataTemplate<Issue>((issue, _) => issue == null ? new Panel() : MakeIssueRow(vm, issue), supportsRecycling: false);
        bookmarksList.ItemTemplate = new FuncDataTemplate<SubtitleLineViewModel>((line, _) => line == null ? new Panel() : MakeBookmarkRow(line), supportsRecycling: false);

        // Picking a row goes to its line; a Fix button handles its own click.
        issuesList.SelectionChanged += (_, _) =>
        {
            if (issuesList.SelectedItem is Issue issue)
            {
                vm.SelectAndScrollToSubtitle(issue.Line);
            }
        };
        bookmarksList.SelectionChanged += (_, _) =>
        {
            if (bookmarksList.SelectedItem is SubtitleLineViewModel line)
            {
                vm.SelectAndScrollToSubtitle(line);
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(2, GridUnitType.Star) { MinHeight = 60 },
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(1, GridUnitType.Star) { MinHeight = 50 },
                new RowDefinition(GridLength.Auto),
            },
            Background = EditorChrome.PanelBrush,
        };
        grid.Add(issuesTitle, 0, 0);
        grid.Add(new Panel { Children = { issuesList, noIssues } }, 1, 0);
        grid.Add(new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ResizeDirection = GridResizeDirection.Rows,
        }, 2, 0);
        grid.Add(bookmarksHeader, 3, 0);
        grid.Add(new Panel { Children = { bookmarksList, noBookmarks } }, 4, 0);
        grid.Add(MakePlayBar(vm), 5, 0);

        void Refresh()
        {
            var issues = FindIssues(vm);
            issuesList.ItemsSource = issues;
            issuesTitle.Text = string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Se.Language.Main.QcIssues, issues.Count);
            noIssues.IsVisible = issues.Count == 0;

            var bookmarks = vm.Subtitles.Where(p => p.Bookmark != null).ToList();
            bookmarksList.ItemsSource = bookmarks;
            bookmarksTitle.Text = string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Se.Language.Main.QcBookmarks, bookmarks.Count);
            noBookmarks.IsVisible = bookmarks.Count == 0;
        }

        var refreshTimer = new DispatcherTimer { Interval = RefreshDelay, Tag = (Action)Refresh };
        refreshTimer.Tick += (_, _) =>
        {
            refreshTimer.Stop();
            Refresh();
        };
        RefreshTimers.AddOrUpdate(grid, refreshTimer);

        void ScheduleRefresh()
        {
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        var watched = new List<SubtitleLineViewModel>();
        void OnLineChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(SubtitleLineViewModel.Text) or nameof(SubtitleLineViewModel.StartTime)
                or nameof(SubtitleLineViewModel.EndTime) or nameof(SubtitleLineViewModel.Duration) or nameof(SubtitleLineViewModel.Bookmark))
            {
                ScheduleRefresh();
            }
        }

        void WatchLines()
        {
            foreach (var line in watched)
            {
                line.PropertyChanged -= OnLineChanged;
            }

            watched.Clear();
            watched.AddRange(vm.Subtitles);
            foreach (var line in watched)
            {
                line.PropertyChanged += OnLineChanged;
            }
        }

        void OnSubtitlesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            WatchLines();
            ScheduleRefresh();
        }

        grid.AttachedToVisualTree += (_, _) =>
        {
            vm.Subtitles.CollectionChanged += OnSubtitlesChanged;
            WatchLines();
            Refresh();
        };
        grid.DetachedFromVisualTree += (_, _) =>
        {
            refreshTimer.Stop();
            vm.Subtitles.CollectionChanged -= OnSubtitlesChanged;
            foreach (var line in watched)
            {
                line.PropertyChanged -= OnLineChanged;
            }

            watched.Clear();
        };

        return grid;
    }

    private static TextBlock MakeTitle()
    {
        return new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 6, 8, 4),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    /// <summary>A colored dot, the line number and error, the detail under it, and Fix when there is one.</summary>
    private static Control MakeIssueRow(MainViewModel vm, Issue issue)
    {
        var dot = new Border
        {
            Width = 9,
            Height = 9,
            CornerRadius = new CornerRadius(5),
            Background = LineError.GetBrush(issue.Error.Type),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };

        var title = new TextBlock
        {
            Text = "#" + issue.Line.Number.ToString(CultureInfo.InvariantCulture) + "  " + issue.Error.Label,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        var detail = new TextBlock
        {
            Text = issue.Error.Detail,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = EditorChrome.ScaledFont(11),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var row = new DockPanel { Background = Brushes.Transparent };
        ToolTip.SetTip(row, LineError.GetHint(issue.Error.Type));
        DockPanel.SetDock(dot, Dock.Left);
        row.Children.Add(dot);
        if (issue.Fix is { } fix)
        {
            var fixButton = new Button
            {
                Content = Se.Language.General.Fix,
                Padding = new Thickness(8, 1),
                MinHeight = 0,
                FontSize = EditorChrome.ScaledFont(11),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0),
                [AutomationProperties.NameProperty] = Se.Language.General.Fix + " " + title.Text,
            };
            fixButton.Click += (_, e) =>
            {
                e.Handled = true;
                ApplyFix(vm, issue.Line, fix);
            };
            DockPanel.SetDock(fixButton, Dock.Right);
            row.Children.Add(fixButton);
        }

        row.Children.Add(new StackPanel { Children = { title, detail }, VerticalAlignment = VerticalAlignment.Center });
        return row;
    }

    /// <summary>
    /// Selects the line, then runs the fix on it. Selecting posts the grid update (which posts
    /// more of its own), so the fix is posted at a lower priority to run after all of it, and
    /// only if the line really is the selection by then.
    /// </summary>
    internal static void ApplyFix(MainViewModel vm, SubtitleLineViewModel line, IRelayCommand fix)
    {
        vm.SelectAndScrollToSubtitle(line);
        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(vm.SelectedSubtitle, line) && fix.CanExecute(null))
            {
                fix.Execute(null);
            }
        }, DispatcherPriority.Background);
    }

    private static Control MakeBookmarkRow(SubtitleLineViewModel line)
    {
        var note = string.IsNullOrWhiteSpace(line.Bookmark) ? null : line.Bookmark;
        var text = (line.Text ?? string.Empty).Replace("\r\n", " ").Replace('\n', ' ');
        var title = new TextBlock
        {
            Text = "⚑ #" + line.Number.ToString(CultureInfo.InvariantCulture) + "  " + (note ?? text),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        var panel = new StackPanel { Children = { title } };
        if (note != null)
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = EditorChrome.DimTextBrush,
                FontSize = EditorChrome.ScaledFont(11),
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
        }

        return panel;
    }

    /// <summary>Play the line (once or in a loop), play the previous/next line, change speed, or have it read aloud.</summary>
    private static Control MakePlayBar(MainViewModel vm)
    {
        var bar = new WrapPanel { Margin = new Thickness(6, 4, 6, 6) };
        bar.Children.Add(MakeSmallButton("▶ " + Se.Language.General.Play, Se.Language.General.PlaySelectedLines, vm.PlaySelectedLinesWithoutLoopCommand));
        bar.Children.Add(MakeSmallButton("⟳", Se.Language.General.PlaySelectedLinesWithLoop, vm.PlaySelectedLinesWithLoopCommand));
        bar.Children.Add(MakeSmallButton("◀", Se.Language.General.PlayPrevious, vm.PlayPreviousCommand));
        bar.Children.Add(MakeSmallButton("▶▶", Se.Language.General.PlayNext, vm.PlayNextCommand));
        bar.Children.Add(MakeSmallButton("−", Se.Language.Options.Shortcuts.PlaybackSpeedSlower, vm.PlaybackSlowerCommand));
        bar.Children.Add(MakeSmallButton("+", Se.Language.Options.Shortcuts.PlaybackSpeedFaster, vm.PlaybackFasterCommand));
        bar.Children.Add(MakeSmallButton("🔊 " + Se.Language.Video.TextToSpeech.SpeakFromCurrentLineDotDotDot.Replace("_", string.Empty),
            Se.Language.Video.TextToSpeech.SpeakFromCurrentLineHint, vm.SpeakFromCurrentLineCommand));
        return bar;
    }

    private static Button MakeSmallButton(string content, string tip, System.Windows.Input.ICommand command)
    {
        var button = new Button
        {
            Content = content,
            Command = command,
            Padding = new Thickness(7, 2),
            Margin = new Thickness(2),
            MinHeight = 0,
            FontSize = EditorChrome.ScaledFont(11),
            [AutomationProperties.NameProperty] = tip,
        };
        ToolTip.SetTip(button, tip);
        return button;
    }
}

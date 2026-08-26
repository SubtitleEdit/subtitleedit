using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.FlowEditing;

public sealed class FlowEditingView : Border
{
    private const int LinesBefore = 4;
    private const int LinesAfter = 4;
    private const double FlowFontSizeIncrease = 2.0;

    private readonly MainViewModel _vm;
    private readonly FlowPasteManager _pasteManager = new();
    private readonly StackPanel _itemsPanel;
    private readonly ScrollViewer _scrollViewer;

    private readonly List<FlowEditingItem> _items = new();
    private readonly Dictionary<FlowEditingItem, TextBox> _textBoxes = new();
    private readonly Dictionary<FlowEditingItem, Border> _rowBorders = new();
    private readonly Dictionary<FlowEditingItem, TextBlock> _numberBlocks = new();
    private readonly HashSet<SubtitleLineViewModel> _selectedSources = new();

    private readonly INotifyCollectionChanged? _observableSubtitles;

    private SubtitleLineViewModel? _selectionAnchorSource;

    private SubtitleLineViewModel? _pendingFocusSource;
    private bool _pendingFocusAtStart;

    // Batch tools can raise many collection changes in a very short time.
    // Coalesce those notifications into one Flow rebuild instead of rebuilding
    // the complete view once for every changed subtitle.
    private bool _refreshQueued;
    private int _refreshGeneration;

    // Flow timing mode: true = SE optimal CPS, false = shortest CPS-compliant duration.
    private bool _useOptimalReadingSpeed = false;

    // Exact clipboard text produced by Flow's Copy subtitle command.
    // If the clipboard still matches this value, Paste before/after treats
    // the copied time codes as relative to the new insertion point rather
    // than as absolute external TXT/SRT time codes.
    private string? _flowCopiedClipboardText;

    // Live Teletext writing guard. Flow keeps the last valid visible text for
    // each currently rendered subtitle so an extra character cannot silently
    // create an illegal third line or exceed the 36/37-character rule.
    private readonly Dictionary<FlowEditingItem, string> _lastValidTeletextText =
        new();

    private bool _applyingLiveTeletextRule;

    public FlowEditingView(MainViewModel vm)
    {
        _vm = vm;

        Padding = new Thickness(4);
        BorderThickness = new Thickness(1);
        BorderBrush = new SolidColorBrush(
            Color.FromArgb(70, 128, 128, 128));
        CornerRadius = new CornerRadius(4);
        MinHeight = 220;

        _itemsPanel = new StackPanel
        {
            Spacing = 3,
        };

        _scrollViewer = new ScrollViewer
        {
            Content = _itemsPanel,
            VerticalScrollBarVisibility =
                Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility =
                Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        };

        Child = _scrollViewer;

        _vm.PropertyChanged += VmOnPropertyChanged;

        _observableSubtitles =
            _vm.Subtitles as INotifyCollectionChanged;

        if (_observableSubtitles != null)
        {
            _observableSubtitles.CollectionChanged +=
                SubtitlesOnCollectionChanged;
        }

        DetachedFromVisualTree += (_, _) => Detach();
    }

    public void Refresh()
    {
        // A direct refresh supersedes any deferred refresh already queued.
        _refreshQueued = false;
        _refreshGeneration++;

        DisposeItems();

        _itemsPanel.Children.Clear();
        _textBoxes.Clear();
        _rowBorders.Clear();
        _numberBlocks.Clear();
        _lastValidTeletextText.Clear();

        var subtitles = _vm.Subtitles.ToList();

        _selectedSources.RemoveWhere(
            source => !subtitles.Contains(source));

        if (_selectedSources.Count == 0 &&
            _vm.SelectedSubtitle != null &&
            subtitles.Contains(_vm.SelectedSubtitle))
        {
            _selectedSources.Add(
                _vm.SelectedSubtitle);

            _selectionAnchorSource =
                _vm.SelectedSubtitle;
        }

        if (subtitles.Count == 0)
        {
            var emptyState =
                new TextBlock
                {
                    Text = "No subtitles",
                    Opacity = 0.65,
                    Margin = new Thickness(8),
                };

            var pastePlainTextMenuItem =
                new MenuItem
                {
                    Header = "Paste plain text",
                };

            pastePlainTextMenuItem.Click +=
                async (_, _) =>
                    await PasteIntoEmptyDocumentAsync();

            emptyState.ContextMenu =
                new ContextMenu
                {
                    ItemsSource =
                        new[]
                        {
                            pastePlainTextMenuItem,
                        },
                };

            _itemsPanel.Children.Add(
                emptyState);

            return;
        }

        var selectedIndex =
            _vm.SelectedSubtitle == null
                ? 0
                : subtitles.IndexOf(_vm.SelectedSubtitle);

        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        for (var i = 0; i < subtitles.Count; i++)

        {
            var subtitle = subtitles[i];

            var item =
                new FlowEditingItem(subtitle);

            _items.Add(item);

            _itemsPanel.Children.Add(
                MakeRow(
                    item,
                    _selectedSources.Contains(
                        subtitle) ||
                    ReferenceEquals(
                        subtitle,
                        _vm.SelectedSubtitle)));
        }

        ApplyPendingFocus();
    }

    private Control MakeRow(
        FlowEditingItem item,
        bool isCurrent)
    {
        var number = new TextBlock
        {
            Width = 48,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(2, 7, 8, 0),
            Opacity = isCurrent ? 1.0 : 0.65,
            FontWeight = isCurrent
                ? FontWeight.SemiBold
                : FontWeight.Normal,
            FontSize =
                Se.Settings.Appearance.SubtitleTextBoxFontSize,
        };

        number.Bind(
            TextBlock.TextProperty,
            new Binding(nameof(FlowEditingItem.Number))
            {
                Source = item,
            });

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 38,

            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),

            FocusAdorner = null,

            Padding = new Thickness(5, 3),

            FontSize =
                Se.Settings.Appearance.SubtitleTextBoxFontSize +
                FlowFontSizeIncrease,

            FontWeight = FontWeight.Normal,
        };

        textBox.Bind(
            TextBox.TextProperty,
            new Binding(nameof(FlowEditingItem.Text))
            {
                Source = item,
                Mode = BindingMode.TwoWay,
            });

        textBox.Bind(
            TextBox.ForegroundProperty,
            new Binding(nameof(FlowEditingItem.Foreground))
            {
                Source = item,
                Mode = BindingMode.OneWay,
            });

        _lastValidTeletextText[item] =
            item.Text ?? string.Empty;

        // Enforce the EBU Teletext writing width while the user types.
        // With an explicit colour code the usable width is 36 characters;
        // without colour it is 37. Flow may automatically rebalance one
        // overlong line into two legal lines, but it never silently creates a
        // third line or loses text.
        textBox.TextChanged +=
            async (_, _) => await ApplyLiveTeletextWritingRuleAsync(
                item,
                textBox);

        textBox.GotFocus +=
            (_, _) =>
            {
                if (!_selectedSources.Contains(
                        item.Source))
                {
                    SelectItem(item);
                }
            };

        textBox.AddHandler(
            InputElement.PointerPressedEvent,
            (_, e) => HandlePointerSelection(
                item,
                textBox,
                e),
            Avalonia.Interactivity.RoutingStrategies.Tunnel);

        // Flow paste is handled before the TextBox inserts raw clipboard text.
        textBox.AddHandler(
            InputElement.KeyDownEvent,
            async (_, e) => await TextBoxOnPasteKeyDownAsync(item, textBox, e),
            Avalonia.Interactivity.RoutingStrategies.Tunnel,
            handledEventsToo: true);

        // Return must be handled before the TextBox inserts a third line.
        textBox.AddHandler(
            InputElement.KeyDownEvent,
            (_, e) => TextBoxOnReturnKeyDown(item, textBox, e),
            Avalonia.Interactivity.RoutingStrategies.Tunnel,
            handledEventsToo: true);

        // Arrow navigation must be handled before the TextBox moves the caret.
        textBox.AddHandler(
            InputElement.KeyDownEvent,
            (_, e) => TextBoxOnArrowKeyDown(item, textBox, e),
            Avalonia.Interactivity.RoutingStrategies.Tunnel,
            handledEventsToo: true);

        // Backspace at the start of a Flow subtitle belongs to the subtitle
        // workflow, not to the TextBox. Handle it before the TextBox can consume
        // the key so moving/merging back into the previous subtitle is reliable.
        textBox.AddHandler(
            InputElement.KeyDownEvent,
            (_, e) => TextBoxOnBackspaceKeyDown(item, textBox, e),
            Avalonia.Interactivity.RoutingStrategies.Tunnel,
            handledEventsToo: true);

        if (!string.IsNullOrEmpty(
                Se.Settings.Appearance
                    .SubtitleTextBoxAndGridFontName))
        {
            textBox.FontFamily =
                new FontFamily(
                    Se.Settings.Appearance
                        .SubtitleTextBoxAndGridFontName);
        }

        var timeCode = new TextBlock
        {
            FontSize = 10,
            Opacity = 0.5,
            Margin = new Thickness(5, 0, 0, 3),
        };

        timeCode.Bind(
            TextBlock.TextProperty,
            new Binding(nameof(FlowEditingItem.TimeCode))
            {
                Source = item,
            });

        var content = new StackPanel();

        content.Children.Add(textBox);
        content.Children.Add(timeCode);

        var grid = new Grid
        {
            ColumnDefinitions =
                new ColumnDefinitions("Auto,*"),
        };

        grid.Children.Add(number);

        Grid.SetColumn(content, 1);
        grid.Children.Add(content);

        var rowBorder = new Border
        {
            Child = grid,
            Padding = new Thickness(5, 3),
            Margin = new Thickness(0, 1),
            CornerRadius = new CornerRadius(4),
            Background = GetRowBackground(isCurrent),
        };

        rowBorder.AddHandler(
            InputElement.PointerPressedEvent,
            (_, e) => HandlePointerSelection(
                item,
                rowBorder,
                e),
            Avalonia.Interactivity.RoutingStrategies.Tunnel);

        rowBorder.ContextMenu =
            CreateFlowContextMenu(
                item,
                textBox,
                includeTextEditingItems: false);

        textBox.ContextMenu =
            CreateFlowContextMenu(
                item,
                textBox,
                includeTextEditingItems: true);

        _textBoxes[item] = textBox;
        _rowBorders[item] = rowBorder;
        _numberBlocks[item] = number;

        return rowBorder;
    }

    private ContextMenu CreateFlowContextMenu(
        FlowEditingItem item,
        TextBox textBox,
        bool includeTextEditingItems)
    {
        var items =
            new List<object>();

        var insertBeforeMenuItem =
            new MenuItem
            {
                Header = "Insert subtitle before",
            };

        insertBeforeMenuItem.Click +=
            async (_, _) =>
            {
                SelectItem(item);

                await CreateSubtitleBeforeAsync(
                    item);
            };

        var insertAfterMenuItem =
            new MenuItem
            {
                Header = "Insert subtitle after",
            };

        insertAfterMenuItem.Click +=
            async (_, _) =>
            {
                SelectItem(item);

                await CreateSubtitleAfterAsync(
                    item);
            };

        var pasteBeforeMenuItem =
            new MenuItem
            {
                Header = "Paste subtitles before",
            };

        pasteBeforeMenuItem.Click +=
            async (_, _) =>
            {
                SelectItem(item);

                await PasteBeforeSubtitleAsync(
                    item);
            };

        var pasteAfterMenuItem =
            new MenuItem
            {
                Header = "Paste subtitles after",
            };

        pasteAfterMenuItem.Click +=
            async (_, _) =>
            {
                SelectItem(item);

                await PasteAfterSubtitleAsync(
                    item);
            };

        items.Add(
            insertBeforeMenuItem);

        items.Add(
            insertAfterMenuItem);

        items.Add(
            new Separator());

        items.Add(
            pasteBeforeMenuItem);

        items.Add(
            pasteAfterMenuItem);

        items.Add(
            new Separator());

        var optimalTimingMenuItem =
            new MenuItem
            {
                Header = "Optimal reading speed",
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = _useOptimalReadingSpeed,
            };

        optimalTimingMenuItem.Click +=
            (_, _) =>
            {
                _useOptimalReadingSpeed = true;
            };

        var minimumTimingMenuItem =
            new MenuItem
            {
                Header = "Minimum compliant duration",
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = !_useOptimalReadingSpeed,
            };

        minimumTimingMenuItem.Click +=
            (_, _) =>
            {
                _useOptimalReadingSpeed = false;
            };

        var timingMenuItem =
            new MenuItem
            {
                Header = "Flow timing",
                ItemsSource =
                    new List<object>
                    {
                        optimalTimingMenuItem,
                        minimumTimingMenuItem,
                    },
            };

        items.Add(
            timingMenuItem);

        items.Add(
            new Separator());

        var copySelectedMenuItem =
            new MenuItem
            {
                Header =
                    _selectedSources.Count > 1
                        ? "Copy selected subtitles"
                        : "Copy subtitle",
            };

        copySelectedMenuItem.Click +=
            async (_, _) =>
            {
                if (!_selectedSources.Contains(
                        item.Source))
                {
                    SelectItem(item);
                }

                await CopySelectedSubtitlesAsync();
            };

        items.Add(
            copySelectedMenuItem);

        var deleteSelectedMenuItem =
            new MenuItem
            {
                Header =
                    _selectedSources.Count > 1
                        ? "Delete selected subtitles"
                        : "Delete subtitle",
            };

        deleteSelectedMenuItem.Click +=
            (_, _) =>
            {
                if (!_selectedSources.Contains(
                        item.Source))
                {
                    SelectItem(item);
                }

                DeleteSelectedSubtitles();
            };

        items.Add(
            deleteSelectedMenuItem);

        return new ContextMenu
        {
            ItemsSource =
                items,
        };
    }


    private async System.Threading.Tasks.Task CopySelectedSubtitlesAsync()
    {
        if (_selectedSources.Count == 0)
        {
            return;
        }

        var clipboard =
            TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard == null)
        {
            return;
        }

        var selected =
            _vm.Subtitles
                .Where(
                    subtitle =>
                        _selectedSources.Contains(
                            subtitle))
                .ToList();

        if (selected.Count == 0)
        {
            return;
        }

        var text =
            string.Join(
                Environment.NewLine +
                Environment.NewLine,
                selected.Select(
                    subtitle =>
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} --> {1}{2}{3}",
                            FormatClipboardTimeCode(
                                subtitle.StartTime),
                            FormatClipboardTimeCode(
                                subtitle.EndTime),
                            Environment.NewLine,
                            FlowTextParser.Parse(
                                subtitle.Text)
                                .Text)));

        await clipboard.SetTextAsync(
            text);

        _flowCopiedClipboardText =
            text;
    }

    private static string FormatClipboardTimeCode(
        TimeSpan time)
    {
        return time.ToString(
            @"hh\:mm\:ss\.fff",
            CultureInfo.InvariantCulture);
    }

    private void HandlePointerSelection(
        FlowEditingItem item,
        Control control,
        PointerPressedEventArgs e)
    {
        var point =
            e.GetCurrentPoint(
                control);

        var isRightClick =
            point.Properties.IsRightButtonPressed;

        if (isRightClick)
        {
            if (!_selectedSources.Contains(
                    item.Source))
            {
                SelectItem(item);
            }

            return;
        }

        var toggle =
            e.KeyModifiers.HasFlag(
                KeyModifiers.Control) ||
            e.KeyModifiers.HasFlag(
                KeyModifiers.Meta);

        var range =
            e.KeyModifiers.HasFlag(
                KeyModifiers.Shift);

        if (range)
        {
            SelectRangeTo(
                item.Source);

            // PointerPressed tunnels through both the Flow row and the TextBox.
            // Without marking the event handled, Shift/Cmd selection runs twice
            // when clicking directly in the text. On macOS this made Cmd-click
            // add and immediately remove the same subtitle again.
            e.Handled = true;

            return;
        }

        if (toggle)
        {
            ToggleSelection(
                item.Source);

            // Prevent the same Cmd/Ctrl-click from being processed a second
            // time by the nested TextBox handler.
            e.Handled = true;

            return;
        }

        SelectItem(item);
    }

    private void ToggleSelection(
        SubtitleLineViewModel source)
    {
        if (_selectedSources.Contains(source))
        {
            if (_selectedSources.Count > 1)
            {
                _selectedSources.Remove(source);
            }
        }
        else
        {
            _selectedSources.Add(source);
        }

        _selectionAnchorSource =
            source;

        _vm.SelectedSubtitle =
            source;

        UpdateSelectionVisuals();
    }

    private void SelectRangeTo(
        SubtitleLineViewModel source)
    {
        var subtitles =
            _vm.Subtitles.ToList();

        var anchor =
            _selectionAnchorSource ??
            _vm.SelectedSubtitle ??
            source;

        var anchorIndex =
            subtitles.IndexOf(anchor);

        var sourceIndex =
            subtitles.IndexOf(source);

        if (anchorIndex < 0 ||
            sourceIndex < 0)
        {
            _selectedSources.Clear();
            _selectedSources.Add(source);
        }
        else
        {
            _selectedSources.Clear();

            var start =
                Math.Min(
                    anchorIndex,
                    sourceIndex);

            var end =
                Math.Max(
                    anchorIndex,
                    sourceIndex);

            for (var i = start; i <= end; i++)
            {
                if (!subtitles[i].IsReferenceOnly)
                {
                    _selectedSources.Add(
                        subtitles[i]);
                }
            }
        }

        _vm.SelectedSubtitle =
            source;

        UpdateSelectionVisuals();
    }

    private void DeleteSelectedSubtitles()
    {
        if (_selectedSources.Count == 0)
        {
            return;
        }

        var subtitles =
            _vm.Subtitles;

        var indices =
            _selectedSources
                .Select(subtitles.IndexOf)
                .Where(index => index >= 0)
                .OrderByDescending(index => index)
                .ToList();

        if (indices.Count == 0)
        {
            return;
        }

        var firstIndex =
            indices.Min();

        foreach (var index in indices)
        {
            if (index >= 0 &&
                index < subtitles.Count &&
                !subtitles[index].IsReferenceOnly)
            {
                subtitles.RemoveAt(
                    index);
            }
        }

        RenumberSubtitles();

        _selectedSources.Clear();
        _selectionAnchorSource =
            null;

        if (subtitles.Count > 0)
        {
            var newIndex =
                Math.Min(
                    firstIndex,
                    subtitles.Count - 1);

            var selected =
                subtitles[newIndex];

            _selectedSources.Add(
                selected);

            _selectionAnchorSource =
                selected;

            _vm.SelectedSubtitle =
                selected;
        }
        else
        {
            _vm.SelectedSubtitle =
                null;
        }

        Refresh();
    }

    private static IBrush GetRowBackground(bool isCurrent)
    {
        return isCurrent
            ? new SolidColorBrush(
                Color.FromArgb(
                    78,
                    160,
                    160,
                    160))
            : Brushes.Transparent;
    }

    private async System.Threading.Tasks.Task TextBoxOnPasteKeyDownAsync(
        FlowEditingItem item,
        TextBox textBox,
        KeyEventArgs e)
    {
        var isPaste =
            e.Key == Key.V &&
            (e.KeyModifiers.HasFlag(KeyModifiers.Meta) ||
             e.KeyModifiers.HasFlag(KeyModifiers.Control));

        if (!isPaste)
        {
            return;
        }

        // Flow owns Cmd/Ctrl+V only at the end of a subtitle. Inside the text,
        // keep the TextBox's normal paste behaviour.
        if (textBox.SelectionStart != textBox.SelectionEnd ||
            textBox.CaretIndex < (textBox.Text ?? string.Empty).Length)
        {
            return;
        }

        e.Handled = true;

        await PasteAfterSubtitleAsync(
            item);
    }

    private async System.Threading.Tasks.Task<bool> PasteIntoEmptyDocumentAsync()
    {
        if (_vm.Subtitles.Count != 0)
        {
            return false;
        }

        var clipboard =
            TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard == null)
        {
            return false;
        }

        var clipboardText =
            await clipboard.TryGetTextAsync();

        if (string.IsNullOrWhiteSpace(
                clipboardText))
        {
            return false;
        }

        var plan =
            _pasteManager.BuildPlan(
                clipboardText,
                TimeSpan.Zero,
                nextExistingSubtitleStart: null,
                hasColor: _vm.IsFormatEbu,
                plainTextDurationCalculator:
                    CalculateSeOptimalDurationMilliseconds,
                plainTextHasColor: false);

        if (!plan.Success ||
            plan.Items.Count == 0)
        {
            return false;
        }

        SubtitleLineViewModel? lastInserted =
            null;

        var presentationTemplate =
            new SubtitleLineViewModel();

        foreach (var pasteItem in plan.Items)
        {
            var newSubtitle =
                new SubtitleLineViewModel
                {
                    Text =
                        pasteItem.Text,
                };

            if (_vm.IsFormatEbu)
            {
                ApplyNewEbuFlowPresentation(
                    newSubtitle,
                    presentationTemplate,
                    presentationTemplate,
                    pasteItem.Text);
            }

            newSubtitle.SetStartTimeOnly(
                pasteItem.StartTime);

            newSubtitle.EndTime =
                pasteItem.EndTime;

            _vm.Subtitles.Add(
                newSubtitle);

            lastInserted =
                newSubtitle;
        }

        RenumberSubtitles();

        if (lastInserted == null)
        {
            return false;
        }

        _selectedSources.Clear();
        _selectedSources.Add(
            lastInserted);

        _selectionAnchorSource =
            lastInserted;

        _pendingFocusSource =
            lastInserted;

        _pendingFocusAtStart =
            false;

        _vm.SelectedSubtitle =
            lastInserted;

        Refresh();
        CenterSelectedSubtitleInFlow();

        return true;
    }

    internal static SubtitleLineViewModel CreateEmptyEbuSubtitle(
        TimeSpan startTime)
    {
        var frameRate =
            Se.Settings.General.CurrentFrameRate;

        if (frameRate <= 0)
        {
            frameRate =
                Se.Settings.General.DefaultFrameRate;
        }

        var duration =
            TimeSpan.FromMilliseconds(
                SubtitleFormat.FramesToMilliseconds(
                    5,
                    frameRate));

        var subtitle =
            new SubtitleLineViewModel
            {
                Text = string.Empty,
            };

        var presentationTemplate =
            new SubtitleLineViewModel();

        ApplyNewEbuFlowPresentation(
            subtitle,
            presentationTemplate,
            presentationTemplate,
            string.Empty);

        var doubleHeight =
            Configuration.Settings.SubtitleSettings
                .EbuStlTeletextUseDoubleHeight;

        subtitle.MarginV =
            TeletextRowHelper
                .GetBottomStartRow(
                    1,
                    doubleHeight)
                .ToString(
                    CultureInfo.InvariantCulture);

        subtitle.SetStartTimeOnly(
            startTime);

        subtitle.EndTime =
            startTime +
            duration;

        return subtitle;
    }

    private async System.Threading.Tasks.Task<bool> PasteBeforeSubtitleAsync(
        FlowEditingItem item)
    {
        var subtitles =
            _vm.Subtitles;

        var sourceIndex =
            subtitles.IndexOf(
                item.Source);

        if (sourceIndex < 0)
        {
            return false;
        }

        var clipboard =
            TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard == null)
        {
            return false;
        }

        var clipboardText =
            await clipboard.TryGetTextAsync();

        if (string.IsNullOrWhiteSpace(
                clipboardText))
        {
            ShowPasteWarning(
                item,
                "Clipboard contains no text.");

            return false;
        }

        var parsedSource =
            FlowTextParser.Parse(
                item.Source.Text);

        var hasColor =
            _vm.IsFormatEbu ||
            !string.IsNullOrWhiteSpace(
                parsedSource.ColorToken);

        var gapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        var gap =
            TimeSpan.FromMilliseconds(
                gapMs);

        var latestAllowedEnd =
            item.Source.StartTime -
            gap;

        if (latestAllowedEnd <= TimeSpan.Zero)
        {
            ShowPasteWarning(
                item,
                "Not enough time before the first subtitle.");

            return false;
        }

        var isInternalFlowCopy =
            !string.IsNullOrEmpty(
                _flowCopiedClipboardText) &&
            string.Equals(
                clipboardText,
                _flowCopiedClipboardText,
                StringComparison.Ordinal);

        FlowPastePlan plan;

        if (isInternalFlowCopy)
        {
            var copiedPlan =
                _pasteManager.BuildPlan(
                    clipboardText,
                    TimeSpan.Zero,
                    nextExistingSubtitleStart: null,
                    hasColor,
                    CalculateSeOptimalDurationMilliseconds,
                    !string.IsNullOrWhiteSpace(
                        parsedSource.ColorToken));

            if (!copiedPlan.Success ||
                copiedPlan.Items.Count == 0)
            {
                ShowPasteWarning(
                    item,
                    copiedPlan.ErrorMessage ??
                    "Paste not possible.");

                return false;
            }

            var lastOriginalEnd =
                copiedPlan.Items[^1].EndTime;

            var shiftToInsertion =
                latestAllowedEnd -
                lastOriginalEnd;

            var shiftedItems =
                copiedPlan.Items
                    .Select(
                        pasteItem =>
                            new FlowPasteItem(
                                pasteItem.StartTime +
                                shiftToInsertion,
                                pasteItem.EndTime +
                                shiftToInsertion,
                                pasteItem.Text,
                                pasteItem.HasExplicitTimeCodes))
                    .ToList();

            plan =
                FlowPastePlan.Successful(
                    shiftedItems,
                    hasExplicitTimeCodes: false);
        }
        else
        {
            plan =
                _pasteManager.BuildPlan(
                    clipboardText,
                    TimeSpan.Zero,
                    nextExistingSubtitleStart: null,
                    hasColor,
                    CalculateSeOptimalDurationMilliseconds,
                    !string.IsNullOrWhiteSpace(
                        parsedSource.ColorToken));
        }

        if (!plan.Success ||
            plan.Items.Count == 0)
        {
            ShowPasteWarning(
                item,
                plan.ErrorMessage ??
                "Paste not possible.");

            return false;
        }

        IReadOnlyList<FlowPasteItem> itemsToInsert =
            plan.Items;

        if (!isInternalFlowCopy &&
            !plan.HasExplicitTimeCodes)
        {
            var firstStart =
                plan.Items[0].StartTime;

            var lastEnd =
                plan.Items[^1].EndTime;

            var blockDuration =
                lastEnd -
                firstStart;

            var targetStart =
                latestAllowedEnd -
                blockDuration;

            if (targetStart < TimeSpan.Zero)
            {
                ShowPasteWarning(
                    item,
                    "Not enough time before the first subtitle.");

                return false;
            }

            var shift =
                targetStart -
                firstStart;

            itemsToInsert =
                plan.Items
                    .Select(
                        pasteItem =>
                            new FlowPasteItem(
                                pasteItem.StartTime + shift,
                                pasteItem.EndTime + shift,
                                pasteItem.Text,
                                pasteItem.HasExplicitTimeCodes))
                    .ToList();
        }

        if (!isInternalFlowCopy &&
            plan.HasExplicitTimeCodes &&
            itemsToInsert[^1].EndTime >
            latestAllowedEnd)
        {
            ShowPasteWarning(
                item,
                "The pasted time-coded subtitles do not fit before the selected subtitle.");

            return false;
        }

        if (itemsToInsert[0].StartTime <
            TimeSpan.Zero)
        {
            ShowPasteWarning(
                item,
                "Not enough time before the first subtitle.");

            return false;
        }

        if (!plan.HasExplicitTimeCodes &&
            sourceIndex > 0)
        {
            var previous =
                subtitles[
                    sourceIndex - 1];

            if (!previous.IsReferenceOnly)
            {
                var earliestAllowedStart =
                    previous.EndTime +
                    gap;

                if (itemsToInsert[0].StartTime <
                    earliestAllowedStart)
                {
                    var requiredShift =
                        earliestAllowedStart -
                        itemsToInsert[0].StartTime;

                    var affectedCount =
                        subtitles.Count -
                        sourceIndex;

                    var approved =
                        await ConfirmPasteShiftAsync(
                            before: true,
                            Math.Max(
                                0.0,
                                (itemsToInsert[^1].EndTime -
                                 itemsToInsert[0].StartTime)
                                .TotalSeconds),
                            Math.Max(
                                0.0,
                                (latestAllowedEnd -
                                 earliestAllowedStart)
                                .TotalSeconds),
                            requiredShift,
                            affectedCount);

                    if (!approved)
                    {
                        return false;
                    }

                    ShiftFollowingSubtitles(
                        sourceIndex,
                        requiredShift);

                    latestAllowedEnd =
                        item.Source.StartTime -
                        gap;

                    var blockEnd =
                        itemsToInsert[^1].EndTime;

                    var reanchor =
                        latestAllowedEnd -
                        blockEnd;

                    itemsToInsert =
                        itemsToInsert
                            .Select(
                                pasteItem =>
                                    new FlowPasteItem(
                                        pasteItem.StartTime + reanchor,
                                        pasteItem.EndTime + reanchor,
                                        pasteItem.Text,
                                        pasteItem.HasExplicitTimeCodes))
                            .ToList();
                }
            }
        }

        var finalPlan =
            FlowPastePlan.Successful(
                itemsToInsert,
                plan.HasExplicitTimeCodes &&
                !isInternalFlowCopy);

        InsertPastePlanBefore(
            item,
            finalPlan);

        return true;
    }

    private async System.Threading.Tasks.Task<bool> PasteAfterSubtitleAsync(
        FlowEditingItem item)
    {
        var subtitles =
            _vm.Subtitles;

        var sourceIndex =
            subtitles.IndexOf(
                item.Source);

        if (sourceIndex < 0)
        {
            return false;
        }

        var clipboard =
            TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard == null)
        {
            return false;
        }

        var clipboardText =
            await clipboard.TryGetTextAsync();

        if (string.IsNullOrWhiteSpace(
                clipboardText))
        {
            ShowPasteWarning(
                item,
                "Clipboard contains no text.");

            return false;
        }

        var parsedSource =
            FlowTextParser.Parse(
                item.Source.Text);

        var hasColor =
            _vm.IsFormatEbu ||
            !string.IsNullOrWhiteSpace(
                parsedSource.ColorToken);

        var gapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        var gap =
            TimeSpan.FromMilliseconds(
                gapMs);

        var insertionStart =
            item.Source.EndTime +
            gap;

        SubtitleLineViewModel? nextSubtitle =
            null;

        if (sourceIndex + 1 <
            subtitles.Count)
        {
            var candidate =
                subtitles[
                    sourceIndex + 1];

            if (!candidate.IsReferenceOnly)
            {
                nextSubtitle =
                    candidate;
            }
        }

        var isInternalFlowCopy =
            !string.IsNullOrEmpty(
                _flowCopiedClipboardText) &&
            string.Equals(
                clipboardText,
                _flowCopiedClipboardText,
                StringComparison.Ordinal);

        FlowPastePlan plan;

        if (isInternalFlowCopy)
        {
            // Parse Flow's own copied TC block at zero so the original duration
            // and spacing can be retained, then re-anchor the whole block at the
            // requested insertion point.
            var copiedPlan =
                _pasteManager.BuildPlan(
                    clipboardText,
                    TimeSpan.Zero,
                    nextExistingSubtitleStart: null,
                    hasColor,
                    CalculateSeOptimalDurationMilliseconds,
                    !string.IsNullOrWhiteSpace(
                        parsedSource.ColorToken));

            if (!copiedPlan.Success ||
                copiedPlan.Items.Count == 0)
            {
                ShowPasteWarning(
                    item,
                    copiedPlan.ErrorMessage ??
                    "Paste not possible.");

                return false;
            }

            var firstOriginalStart =
                copiedPlan.Items[0].StartTime;

            var shiftToInsertion =
                insertionStart -
                firstOriginalStart;

            var shiftedItems =
                copiedPlan.Items
                    .Select(
                        pasteItem =>
                            new FlowPasteItem(
                                pasteItem.StartTime +
                                shiftToInsertion,
                                pasteItem.EndTime +
                                shiftToInsertion,
                                pasteItem.Text,
                                pasteItem.HasExplicitTimeCodes))
                    .ToList();

            plan =
                FlowPastePlan.Successful(
                    shiftedItems,
                    hasExplicitTimeCodes: false);
        }
        else
        {
            // External time-coded text keeps its absolute TC behaviour.
            plan =
                _pasteManager.BuildPlan(
                    clipboardText,
                    TimeSpan.Zero,
                    nextExistingSubtitleStart: null,
                    hasColor,
                    CalculateSeOptimalDurationMilliseconds,
                    !string.IsNullOrWhiteSpace(
                        parsedSource.ColorToken),
                    plainTextInsertionStart:
                        insertionStart);
        }

        if (!plan.Success ||
            plan.Items.Count == 0)
        {
            ShowPasteWarning(
                item,
                plan.ErrorMessage ??
                "Paste not possible.");

            return false;
        }

        if (nextSubtitle != null)
        {
            var requiredNextStart =
                plan.Items[^1].EndTime +
                gap;

            if (nextSubtitle.StartTime <
                requiredNextStart)
            {
                var shift =
                    requiredNextStart -
                    nextSubtitle.StartTime;

                var availableSeconds =
                    Math.Max(
                        0.0,
                        (nextSubtitle.StartTime -
                         gap -
                         insertionStart)
                        .TotalSeconds);

                var requiredSeconds =
                    Math.Max(
                        0.0,
                        (plan.Items[^1].EndTime -
                         insertionStart)
                        .TotalSeconds);

                var approved =
                    await ConfirmPasteShiftAsync(
                        before: false,
                        requiredSeconds,
                        availableSeconds,
                        shift,
                        subtitles.Count -
                        (sourceIndex + 1));

                if (!approved)
                {
                    return false;
                }

                ShiftFollowingSubtitles(
                    sourceIndex + 1,
                    shift);
            }
        }

        InsertPastePlanAfter(
            item,
            plan);

        return true;
    }

    private async System.Threading.Tasks.Task<bool> ConfirmPasteShiftAsync(
        bool before,
        double requiredSeconds,
        double availableSeconds,
        TimeSpan shift,
        int affectedCount)
    {
        var owner =
            TopLevel.GetTopLevel(this)
            as Window;

        if (owner == null)
        {
            return false;
        }

        var message =
            string.Format(
                CultureInfo.InvariantCulture,
                "Not enough time to paste subtitles {0}.{1}{1}" +
                "Required: {2:0.00} s{1}" +
                "Available: {3:0.00} s{1}" +
                "Shift required: +{4:0.00} s{1}{1}" +
                "This will shift {5} following subtitle{6}.",
                before ? "before" : "after",
                Environment.NewLine,
                requiredSeconds,
                availableSeconds,
                shift.TotalSeconds,
                Math.Max(
                    0,
                    affectedCount),
                affectedCount == 1
                    ? string.Empty
                    : "s");

        var result =
            await MessageBox.Show(
                owner,
                "Flow timing conflict",
                message,
                MessageBoxButtons.Custom2,
                MessageBoxIcon.Warning,
                "Cancel",
                "Paste and shift");

        return result ==
               MessageBoxResult.Custom2;
    }


    private void InsertPastePlanBefore(
        FlowEditingItem currentItem,
        FlowPastePlan plan)
    {
        var subtitles =
            _vm.Subtitles;

        var sourceIndex =
            subtitles.IndexOf(
                currentItem.Source);

        if (sourceIndex < 0)
        {
            return;
        }

        var insertIndex =
            sourceIndex;

        SubtitleLineViewModel? lastInserted =
            null;

        foreach (var pasteItem in plan.Items)
        {
            var newSubtitle =
                new SubtitleLineViewModel(
                    currentItem.Source,
                    generateNewId: true)
                {
                    Text =
                        pasteItem.Text,
                };

            if (_vm.IsFormatEbu)
            {
                var positionTemplate =
                    GetLowestTeletextTemplate(
                        sourceIndex,
                        before: true);

                ApplyNewEbuFlowPresentation(
                    newSubtitle,
                    positionTemplate,
                    currentItem.Source,
                    pasteItem.Text);
            }

            newSubtitle.SetStartTimeOnly(
                pasteItem.StartTime);

            newSubtitle.EndTime =
                pasteItem.EndTime;

            subtitles.Insert(
                insertIndex,
                newSubtitle);

            insertIndex++;
            lastInserted =
                newSubtitle;
        }

        RenumberSubtitles();

        if (lastInserted == null)
        {
            return;
        }

        _pendingFocusSource =
            lastInserted;

        _pendingFocusAtStart =
            false;

        _vm.SelectedSubtitle =
            lastInserted;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        lastInserted));

            if (targetItem != null)
            {
                FocusTextBox(
                    targetItem,
                    focusAtStart: false);
            }

            CenterSelectedSubtitleInFlow();
        });
    }

    private void InsertPastePlanAfter(
        FlowEditingItem currentItem,
        FlowPastePlan plan)
    {
        var subtitles = _vm.Subtitles;
        var sourceIndex =
            subtitles.IndexOf(currentItem.Source);

        if (sourceIndex < 0)
        {
            return;
        }

        var insertIndex =
            sourceIndex + 1;

        SubtitleLineViewModel? lastInserted =
            null;

        foreach (var pasteItem in plan.Items)
        {
            var newSubtitle =
                new SubtitleLineViewModel(
                    currentItem.Source,
                    generateNewId: true)
                {
                    Text = pasteItem.Text,
                };

            if (_vm.IsFormatEbu)
            {
                var positionTemplate =
                    GetLowestTeletextTemplate(
                        sourceIndex,
                        before: false);

                ApplyNewEbuFlowPresentation(
                    newSubtitle,
                    positionTemplate,
                    currentItem.Source,
                    pasteItem.Text);
            }

            newSubtitle.SetStartTimeOnly(
                pasteItem.StartTime);

            newSubtitle.EndTime =
                pasteItem.EndTime;

            subtitles.Insert(
                insertIndex,
                newSubtitle);

            insertIndex++;
            lastInserted =
                newSubtitle;
        }

        RenumberSubtitles();

        if (lastInserted == null)
        {
            return;
        }

        _pendingFocusSource =
            lastInserted;

        _pendingFocusAtStart =
            false;

        _vm.SelectedSubtitle =
            lastInserted;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        lastInserted));

            if (targetItem != null)
            {
                FocusTextBox(
                    targetItem,
                    focusAtStart: false);
            }

            CenterSelectedSubtitleInFlow();
        });
    }

    private async System.Threading.Tasks.Task ApplyLiveTeletextWritingRuleAsync(
        FlowEditingItem item,
        TextBox textBox)
    {
        if (_applyingLiveTeletextRule ||
            !_vm.IsFormatEbu)
        {
            return;
        }

        var visibleText =
            NormalizeFlowTypingText(
                textBox.Text ??
                string.Empty);

        var parsed =
            FlowTextParser.Parse(
                item.Source.Text);

        var maxCharacters =
            string.IsNullOrWhiteSpace(
                parsed.ColorToken)
                ? 37
                : 36;

        if (IsValidTeletextTypingText(
                visibleText,
                maxCharacters))
        {
            _lastValidTeletextText[item] =
                visibleText;

            return;
        }

        // Live typing is deliberately NOT a rebalance operation. Previously
        // written words must stay where the user put them. Only the word that
        // currently crosses a Teletext boundary may move.
        if (TryWrapOverflowingFirstLine(
                visibleText,
                maxCharacters,
                out var wrappedText))
        {
            _applyingLiveTeletextRule = true;

            try
            {
                textBox.Text =
                    wrappedText;

                textBox.CaretIndex =
                    wrappedText.Length;

                // If a bottom-anchored one-line EBU subtitle (TT 22 in
                // double-height mode) is automatically wrapped back to two
                // lines by the live 36/37 rule, restore the correct bottom
                // two-line start row (TT 20). Deliberately higher positions
                // are not changed.
                if (_vm.IsFormatEbu)
                {
                    var doubleHeight =
                        Configuration.Settings.SubtitleSettings
                            .EbuStlTeletextUseDoubleHeight;

                    var oneLineBottomRow =
                        TeletextRowHelper.GetBottomStartRow(
                            1,
                            doubleHeight);

                    if (int.TryParse(
                            item.Source.MarginV,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out var currentRow) &&
                        currentRow == oneLineBottomRow)
                    {
                        item.Source.MarginV =
                            TeletextRowHelper
                                .GetBottomStartRow(
                                    2,
                                    doubleHeight)
                                .ToString(
                                    CultureInfo.InvariantCulture);
                    }
                }

                _lastValidTeletextText[item] =
                    wrappedText;
            }
            finally
            {
                _applyingLiveTeletextRule = false;
            }

            return;
        }

        if (!TryMoveOverflowingSecondLineWord(
                visibleText,
                maxCharacters,
                out var currentText,
                out var overflowWord))
        {
            // A single word can itself be longer than the Teletext width.
            // Never cut such a word in the middle. Keep it intact; validation
            // can flag the exceptional overlong word later.
            _lastValidTeletextText[item] =
                visibleText;

            return;
        }

        var previousValid =
            _lastValidTeletextText.TryGetValue(
                item,
                out var remembered)
                ? remembered
                : currentText;

        _applyingLiveTeletextRule = true;

        try
        {
            textBox.Text =
                currentText;

            _lastValidTeletextText[item] =
                currentText;
        }
        finally
        {
            _applyingLiveTeletextRule = false;
        }

        var source =
            item.Source;

        var created =
            await CreateSubtitleAfterAsync(
                item,
                focusAtStart: false);

        if (!created)
        {
            // User cancelled a required ripple shift. Restore the last legal
            // subtitle instead of losing or cutting the word being typed.
            _applyingLiveTeletextRule = true;

            try
            {
                textBox.Text =
                    previousValid;

                textBox.CaretIndex =
                    previousValid.Length;

                _lastValidTeletextText[item] =
                    previousValid;
            }
            finally
            {
                _applyingLiveTeletextRule = false;
            }

            return;
        }

        var updatedSourceIndex =
            _vm.Subtitles.IndexOf(
                source);

        if (updatedSourceIndex < 0 ||
            updatedSourceIndex + 1 >=
            _vm.Subtitles.Count)
        {
            return;
        }

        var newSubtitle =
            _vm.Subtitles[
                updatedSourceIndex + 1];

        // CreateSubtitleAfterAsync already gives a new EBU Flow subtitle the
        // correct colour/alignment/TT position. Put the COMPLETE overflowing
        // word into it; never just the last character that crossed the limit.
        newSubtitle.Text =
            FlowTextParser.ApplyEditedText(
                newSubtitle.Text,
                RebalanceTeletextVisibleText(
                    overflowWord,
                    maxCharacters));

        ApplySeOptimalDurationKeepingStart(
            newSubtitle);

        _pendingFocusSource =
            newSubtitle;

        _pendingFocusAtStart =
            false;

        _vm.SelectedSubtitle =
            newSubtitle;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        newSubtitle));

            if (targetItem != null &&
                _textBoxes.TryGetValue(
                    targetItem,
                    out var targetTextBox))
            {
                var targetText =
                    targetTextBox.Text ??
                    string.Empty;

                // Cursor must continue directly behind the word that Flow moved
                // into the new subtitle.
                targetTextBox.CaretIndex =
                    targetText.Length;

                targetTextBox.SelectionStart =
                    targetText.Length;

                targetTextBox.SelectionEnd =
                    targetText.Length;

                targetTextBox.Focus();
            }

            CenterSelectedSubtitleInFlow();
        });
    }

    private static string NormalizeFlowTypingText(
        string text)
    {
        return (text ?? string.Empty)
            .Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
            .Replace(
                '\r',
                '\n');
    }

    private static bool TryWrapOverflowingFirstLine(
        string text,
        int maxCharacters,
        out string wrappedText)
    {
        wrappedText =
            text;

        var normalized =
            NormalizeFlowTypingText(
                text);

        var lines =
            normalized.Split('\n');

        if (lines.Length != 1 ||
            lines[0].Length <= maxCharacters)
        {
            return false;
        }

        var line =
            lines[0];

        // The word currently being typed is everything after the final space.
        // Move that whole word to line two. Do not rebalance older words.
        var wordStart =
            FindCurrentWordStart(
                line);

        if (wordStart <= 0)
        {
            // One single word is longer than the allowed width. Never split it.
            return false;
        }

        var firstLine =
            line[..wordStart]
                .TrimEnd();

        var currentWord =
            line[wordStart..]
                .TrimStart();

        if (currentWord.Length == 0)
        {
            return false;
        }

        wrappedText =
            firstLine +
            Environment.NewLine +
            currentWord;

        return true;
    }

    private static bool TryMoveOverflowingSecondLineWord(
        string text,
        int maxCharacters,
        out string currentText,
        out string overflowWord)
    {
        currentText =
            text;

        overflowWord =
            string.Empty;

        var normalized =
            NormalizeFlowTypingText(
                text);

        var lines =
            normalized.Split('\n');

        if (lines.Length != 2 ||
            lines[1].Length <= maxCharacters)
        {
            return false;
        }

        var secondLine =
            lines[1];

        if (FlowSentenceBoundaryHelper.IsPreferredBoundary(
                lines[0],
                secondLine))
        {
            currentText = lines[0].TrimEnd();
            overflowWord = secondLine.TrimStart();
            return overflowWord.Length > 0;
        }

        var wordStart =
            FindCurrentWordStart(
                secondLine);

        if (wordStart <= 0)
        {
            // The second line consists of one single overlong word.
            // Do not cut it; leave it intact in the current subtitle.
            return false;
        }

        var remainingSecondLine =
            secondLine[..wordStart]
                .TrimEnd();

        overflowWord =
            secondLine[wordStart..]
                .TrimStart();

        if (overflowWord.Length == 0)
        {
            return false;
        }

        currentText =
            lines[0];

        if (remainingSecondLine.Length > 0)
        {
            currentText +=
                Environment.NewLine +
                remainingSecondLine;
        }

        return true;
    }

    private static int FindCurrentWordStart(
        string line)
    {
        if (string.IsNullOrEmpty(
                line))
        {
            return 0;
        }

        var index =
            line.Length - 1;

        while (index >= 0 &&
               !char.IsWhiteSpace(
                   line[index]))
        {
            index--;
        }

        return index + 1;
    }


    private static bool IsValidTeletextTypingText(
        string text,
        int maxCharacters)
    {
        var normalized =
            (text ?? string.Empty)
                .Replace(
                    "\r\n",
                    "\n",
                    StringComparison.Ordinal)
                .Replace(
                    '\r',
                    '\n');

        var lines =
            normalized.Split('\n');

        if (lines.Length > 2)
        {
            return false;
        }

        return lines.All(
            line =>
                line.Length <=
                maxCharacters);
    }

    private void ShowPasteWarning(
        FlowEditingItem item,
        string message)
    {
        if (!_textBoxes.TryGetValue(
                item,
                out var textBox))
        {
            return;
        }

        ToolTip.SetTip(
            textBox,
            message);

        ToolTip.SetIsOpen(
            textBox,
            true);

        DispatcherTimer.RunOnce(
            () =>
            {
                ToolTip.SetIsOpen(
                    textBox,
                    false);
            },
            TimeSpan.FromSeconds(3.0));
    }

    private async void TextBoxOnReturnKeyDown(
        FlowEditingItem item,
        TextBox textBox,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter &&
            e.Key != Key.Return)
        {
            return;
        }

        e.Handled = true;

        var originalText =
            textBox.Text ??
            string.Empty;

        var originalSourceText =
            item.Source.Text;

        var originalMarginV =
            item.Source.MarginV;

        var originalCaretIndex =
            textBox.CaretIndex;

        var originalSelectionStart =
            textBox.SelectionStart;

        var originalSelectionEnd =
            textBox.SelectionEnd;

        if (originalCaretIndex == 0 &&
            originalSelectionStart == originalSelectionEnd &&
            GetPlainLineCount(originalText) == 1)
        {
            return;
        }

        var selectionStart =
            Math.Min(
                originalSelectionStart,
                originalSelectionEnd);

        var selectionEnd =
            Math.Max(
                originalSelectionStart,
                originalSelectionEnd);

        var text =
            selectionStart == selectionEnd
                ? originalText
                : originalText.Remove(
                    selectionStart,
                    selectionEnd - selectionStart);

        var caretIndex =
            selectionStart == selectionEnd
                ? originalCaretIndex
                : selectionStart;

        var lineCount =
            GetPlainLineCount(
                text);

        // First Return: create the second text line inside the SAME subtitle.
        // Insert it here so modifiers and a text selection cannot bypass the
        // Flow two-line limit.
        if (lineCount < 2)
        {
            var updatedText =
                text.Insert(
                    caretIndex,
                    Environment.NewLine);

            textBox.Text =
                updatedText;

            var updatedCaretIndex =
                caretIndex +
                Environment.NewLine.Length;

            textBox.CaretIndex =
                updatedCaretIndex;

            textBox.SelectionStart =
                updatedCaretIndex;

            textBox.SelectionEnd =
                updatedCaretIndex;

            if (_vm.IsFormatEbu)
            {
                var source =
                    item.Source;

                var oldMarginV =
                    source.MarginV;

                var oldLineCount =
                    GetPlainLineCount(
                        originalText);

                Dispatcher.UIThread.Post(() =>
                {
                    var doubleHeight =
                        Configuration.Settings.SubtitleSettings
                            .EbuStlTeletextUseDoubleHeight;

                    var adjustedRow =
                        TeletextRowHelper
                            .GetRowKeepingBottomEdge(
                                oldMarginV,
                                oldLineCount,
                                newLineCount: 2,
                                doubleHeight);

                    if (adjustedRow.HasValue)
                    {
                        source.MarginV =
                            adjustedRow.Value.ToString(
                                CultureInfo.InvariantCulture);
                    }
                });
            }

            return;
        }

        // Second Return: the subtitle already has two text lines, so Flow may
        // not insert a third line.
        if (selectionStart != selectionEnd)
        {
            textBox.Text =
                text;

            textBox.CaretIndex =
                caretIndex;

            textBox.SelectionStart =
                caretIndex;

            textBox.SelectionEnd =
                caretIndex;
        }

        // At the end of the second line there is no text to split off.
        // If that second line is empty (the common "Return, Return" workflow),
        // remove the trailing line break first so the previous subtitle becomes
        // a true one-line subtitle again, then create the next subtitle.
        if (caretIndex >=
            text.Length)
        {
            var normalizedText =
                text.Replace(
                    "\r\n",
                    "\n",
                    StringComparison.Ordinal)
                    .Replace(
                        '\r',
                        '\n');

            if (normalizedText.EndsWith(
                    "\n",
                    StringComparison.Ordinal))
            {
                normalizedText =
                    normalizedText.TrimEnd(
                        '\n');

                textBox.Text =
                    normalizedText;

                item.Source.Text =
                    FlowTextParser.ApplyEditedText(
                        item.Source.Text,
                        normalizedText);

                if (_vm.IsFormatEbu)
                {
                    var doubleHeight =
                        Configuration.Settings.SubtitleSettings
                            .EbuStlTeletextUseDoubleHeight;

                    item.Source.MarginV =
                        TeletextRowHelper
                            .GetBottomStartRow(
                                1,
                                doubleHeight)
                            .ToString(
                                CultureInfo.InvariantCulture);
                }
            }

            var created =
                await CreateSubtitleAfterAsync(
                    item,
                    focusAtStart: true);

            if (!created)
            {
                textBox.Text =
                    originalText;

                item.Source.Text =
                    originalSourceText;

                item.Source.MarginV =
                    originalMarginV;

                textBox.CaretIndex =
                    originalCaretIndex;

                textBox.SelectionStart =
                    originalSelectionStart;

                textBox.SelectionEnd =
                    originalSelectionEnd;

                textBox.Focus();
            }

            return;
        }

        // Return inside an existing two-line subtitle moves the text after the
        // caret into a newly created subtitle, using SE's existing split logic.
        SplitAtCaret(
            item,
            textBox);
    }

    private SubtitleLineViewModel GetLowestTeletextTemplate(
        int sourceIndex,
        bool before)
    {
        var subtitles =
            _vm.Subtitles;

        var fallback =
            subtitles[sourceIndex];

        SubtitleLineViewModel? lowest =
            null;

        var lowestRow =
            int.MinValue;

        foreach (var subtitle in subtitles)
        {
            if (subtitle.IsReferenceOnly)
            {
                continue;
            }

            var row =
                TryGetTeletextRow(
                    subtitle.MarginV);

            if (!row.HasValue)
            {
                continue;
            }

            // Teletext row numbers increase from top to bottom.
            // New Flow subtitles therefore use the lowest valid TT position
            // found anywhere in the current subtitle file, independent of the
            // subtitle before/after the insertion point.
            if (row.Value > lowestRow)
            {
                lowestRow =
                    row.Value;

                lowest =
                    subtitle;
            }
        }

        return lowest ??
               fallback;
    }

    private static int? TryGetTeletextRow(
        string? marginV)
    {
        if (!int.TryParse(
                marginV,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var row) ||
            row < 1 ||
            row > TeletextRowHelper.BottomRow)
        {
            return null;
        }

        return TeletextRowHelper.NormalizeStartRow(
            row,
            Configuration.Settings.SubtitleSettings
                .EbuStlTeletextUseDoubleHeight);
    }

    private static void ApplyNewEbuFlowPresentation(
        SubtitleLineViewModel newSubtitle,
        SubtitleLineViewModel positionTemplate,
        SubtitleLineViewModel colorSource,
        string visibleText)
    {
        var sourceColor =
            FlowTextParser.Parse(
                colorSource.Text)
                .ColorToken;

        newSubtitle.Text =
            string.IsNullOrWhiteSpace(
                sourceColor)
                ? visibleText
                : $"<font color=\"yellow\">{visibleText}</font>";

        var doubleHeight =
            Configuration.Settings.SubtitleSettings
                .EbuStlTeletextUseDoubleHeight;

        var templateRow =
            TryGetTeletextRow(
                positionTemplate.MarginV);

        newSubtitle.MarginV =
            templateRow.HasValue
                ? templateRow.Value.ToString(
                    CultureInfo.InvariantCulture)
                : TeletextRowHelper
                    .GetBottomStartRow(
                        1,
                        doubleHeight)
                    .ToString(
                        CultureInfo.InvariantCulture);

        var targetLineCount =
            GetPlainLineCount(
                newSubtitle.Text);

        var templateLineCount =
            GetPlainLineCount(
                positionTemplate.Text);

        var newRow =
            TeletextRowHelper.GetRowKeepingBottomEdge(
                newSubtitle.MarginV,
                templateLineCount,
                targetLineCount,
                doubleHeight);

        if (newRow.HasValue)
        {
            newSubtitle.MarginV =
                newRow.Value.ToString(
                    CultureInfo.InvariantCulture);
        }
        else if (targetLineCount == 1)
        {
            var normalized =
                TeletextRowHelper.NormalizeStartRow(
                    int.TryParse(
                        newSubtitle.MarginV,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var row)
                        ? row
                        : TeletextRowHelper.GetBottomStartRow(
                            1,
                            doubleHeight),
                    doubleHeight);

            newSubtitle.MarginV =
                normalized.ToString(
                    CultureInfo.InvariantCulture);
        }
    }

    private async System.Threading.Tasks.Task<bool> CreateSubtitleBeforeAsync(
        FlowEditingItem currentItem)
    {
        var source =
            currentItem.Source;

        if (source.IsReferenceOnly)
        {
            return false;
        }

        var subtitles =
            _vm.Subtitles;

        var sourceIndex =
            subtitles.IndexOf(
                source);

        if (sourceIndex < 0)
        {
            return false;
        }

        var gapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        var defaultDurationMs =
            Math.Max(
                1.0,
                Se.Settings.General.NewEmptyDefaultMs);

        double desiredStartMs;

        if (sourceIndex > 0)
        {
            var previous =
                subtitles[
                    sourceIndex - 1];

            desiredStartMs =
                previous.IsReferenceOnly
                    ? Math.Max(
                        0.0,
                        source.StartTime.TotalMilliseconds -
                        gapMs -
                        defaultDurationMs)
                    : previous.EndTime.TotalMilliseconds +
                      gapMs;
        }
        else
        {
            desiredStartMs =
                Math.Max(
                    0.0,
                    source.StartTime.TotalMilliseconds -
                    gapMs -
                    defaultDurationMs);
        }

        var desiredEndMs =
            desiredStartMs +
            defaultDurationMs;

        var requiredSourceStartMs =
            desiredEndMs +
            gapMs;

        var shiftMs =
            Math.Max(
                0.0,
                requiredSourceStartMs -
                source.StartTime.TotalMilliseconds);

        if (shiftMs > 0.5)
        {
            var availableMs =
                Math.Max(
                    0.0,
                    source.StartTime.TotalMilliseconds -
                    desiredStartMs -
                    gapMs);

            var approved =
                await ConfirmInsertShiftAsync(
                    currentItem,
                    before: true,
                    defaultDurationMs,
                    availableMs,
                    shiftMs,
                    subtitles.Count -
                    sourceIndex);

            if (!approved)
            {
                return false;
            }

            ShiftFollowingSubtitles(
                sourceIndex,
                TimeSpan.FromMilliseconds(
                    shiftMs));

            // Source moved together with the following block. Recalculate the
            // insertion window once, now that enough room has been created.
            sourceIndex =
                subtitles.IndexOf(
                    source);

            desiredStartMs =
                sourceIndex > 0 &&
                !subtitles[sourceIndex - 1].IsReferenceOnly
                    ? subtitles[sourceIndex - 1]
                        .EndTime.TotalMilliseconds +
                      gapMs
                    : Math.Max(
                        0.0,
                        source.StartTime.TotalMilliseconds -
                        gapMs -
                        defaultDurationMs);

            desiredEndMs =
                desiredStartMs +
                defaultDurationMs;
        }

        var newSubtitle =
            new SubtitleLineViewModel(
                source,
                generateNewId: true)
            {
                Text = string.Empty,
            };

        if (_vm.IsFormatEbu)
        {
            var positionTemplate =
                GetLowestTeletextTemplate(
                    sourceIndex,
                    before: true);

            ApplyNewEbuFlowPresentation(
                newSubtitle,
                positionTemplate,
                source,
                string.Empty);
        }

        newSubtitle.SetStartTimeOnly(
            TimeSpan.FromMilliseconds(
                desiredStartMs));

        newSubtitle.EndTime =
            TimeSpan.FromMilliseconds(
                desiredEndMs);

        subtitles.Insert(
            sourceIndex,
            newSubtitle);

        RenumberSubtitles();

        _pendingFocusSource =
            newSubtitle;

        _pendingFocusAtStart =
            true;

        _vm.SelectedSubtitle =
            newSubtitle;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        newSubtitle));

            if (targetItem != null)
            {
                FocusTextBox(
                    targetItem,
                    focusAtStart: true);
            }

            CenterSelectedSubtitleInFlow();
        });

        return true;
    }

    private async System.Threading.Tasks.Task<bool> CreateSubtitleAfterAsync(
        FlowEditingItem currentItem,
        bool focusAtStart = true)
    {
        var source =
            currentItem.Source;

        if (source.IsReferenceOnly)
        {
            return false;
        }

        var subtitles =
            _vm.Subtitles;

        var sourceIndex =
            subtitles.IndexOf(
                source);

        if (sourceIndex < 0)
        {
            return false;
        }

        var gapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        var defaultDurationMs =
            Math.Max(
                1.0,
                Se.Settings.General.NewEmptyDefaultMs);

        var startMs =
            source.EndTime.TotalMilliseconds +
            gapMs;

        var endMs =
            startMs +
            defaultDurationMs;

        if (sourceIndex + 1 <
            subtitles.Count)
        {
            var next =
                subtitles[
                    sourceIndex + 1];

            if (!next.IsReferenceOnly)
            {
                var requiredNextStartMs =
                    endMs +
                    gapMs;

                var shiftMs =
                    Math.Max(
                        0.0,
                        requiredNextStartMs -
                        next.StartTime.TotalMilliseconds);

                if (shiftMs > 0.5)
                {
                    var availableMs =
                        Math.Max(
                            0.0,
                            next.StartTime.TotalMilliseconds -
                            startMs -
                            gapMs);

                    var approved =
                        await ConfirmInsertShiftAsync(
                            currentItem,
                            before: false,
                            defaultDurationMs,
                            availableMs,
                            shiftMs,
                            subtitles.Count -
                            (sourceIndex + 1));

                    if (!approved)
                    {
                        return false;
                    }

                    ShiftFollowingSubtitles(
                        sourceIndex + 1,
                        TimeSpan.FromMilliseconds(
                            shiftMs));
                }
            }
        }

        var newSubtitle =
            new SubtitleLineViewModel(
                source,
                generateNewId: true)
            {
                Text = string.Empty,
            };

        if (_vm.IsFormatEbu)
        {
            var positionTemplate =
                GetLowestTeletextTemplate(
                    sourceIndex,
                    before: false);

            ApplyNewEbuFlowPresentation(
                newSubtitle,
                positionTemplate,
                source,
                string.Empty);
        }

        newSubtitle.SetStartTimeOnly(
            TimeSpan.FromMilliseconds(
                startMs));

        newSubtitle.EndTime =
            TimeSpan.FromMilliseconds(
                endMs);

        subtitles.Insert(
            sourceIndex + 1,
            newSubtitle);

        RenumberSubtitles();

        _pendingFocusSource =
            newSubtitle;

        _pendingFocusAtStart =
            focusAtStart;

        _vm.SelectedSubtitle =
            newSubtitle;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        newSubtitle));

            if (targetItem != null)
            {
                FocusTextBox(
                    targetItem,
                    focusAtStart);
            }

            CenterSelectedSubtitleInFlow();
        });

        return true;
    }

    private async System.Threading.Tasks.Task<bool> ConfirmInsertShiftAsync(
        FlowEditingItem item,
        bool before,
        double requiredDurationMs,
        double availableDurationMs,
        double shiftMs,
        int affectedCount)
    {
        var owner =
            TopLevel.GetTopLevel(this)
            as Window;

        if (owner == null)
        {
            return false;
        }

        var message =
            string.Format(
                CultureInfo.InvariantCulture,
                "Not enough time to insert a subtitle {0}.{1}{1}" +
                "Required duration: {2:0.00} s{1}" +
                "Available: {3:0.00} s{1}" +
                "Shift required: +{4:0.00} s{1}{1}" +
                "This will shift {5} following subtitle{6}.",
                before ? "before" : "after",
                Environment.NewLine,
                requiredDurationMs / 1000.0,
                availableDurationMs / 1000.0,
                shiftMs / 1000.0,
                Math.Max(
                    0,
                    affectedCount),
                affectedCount == 1
                    ? string.Empty
                    : "s");

        var result =
            await MessageBox.Show(
                owner,
                "Flow timing conflict",
                message,
                MessageBoxButtons.Custom2,
                MessageBoxIcon.Warning,
                "Cancel",
                "Insert and shift");

        return result ==
               MessageBoxResult.Custom2;
    }

    private static int GetSafeWordSplitIndex(
        string text,
        int requestedIndex)
    {
        if (string.IsNullOrEmpty(
                text))
        {
            return 0;
        }

        var index =
            Math.Clamp(
                requestedIndex,
                0,
                text.Length);

        if (index <= 0 ||
            index >= text.Length)
        {
            return index;
        }

        // Already between words / beside a line break: keep the exact caret.
        if (char.IsWhiteSpace(
                text[index - 1]) ||
            char.IsWhiteSpace(
                text[index]))
        {
            return index;
        }

        // Caret is inside a word. Move the split to the beginning of that word
        // so the complete word continues in the next subtitle.
        var wordStart =
            index;

        while (wordStart > 0 &&
               !char.IsWhiteSpace(
                   text[wordStart - 1]))
        {
            wordStart--;
        }

        if (wordStart > 0)
        {
            return wordStart;
        }

        // The whole left side is one word. Do not cut it. Move the split to the
        // end of that word instead, if there is more text afterwards.
        var wordEnd =
            index;

        while (wordEnd < text.Length &&
               !char.IsWhiteSpace(
                   text[wordEnd]))
        {
            wordEnd++;
        }

        while (wordEnd < text.Length &&
               char.IsWhiteSpace(
                   text[wordEnd]))
        {
            wordEnd++;
        }

        return wordEnd < text.Length
            ? wordEnd
            : requestedIndex;
    }

    private bool SplitAtCaret(
        FlowEditingItem currentItem,
        TextBox textBox)
    {
        var source = currentItem.Source;

        if (source.IsReferenceOnly)
        {
            return false;
        }

        var subtitles = _vm.Subtitles;
        var sourceIndex = subtitles.IndexOf(source);

        if (sourceIndex < 0)
        {
            return false;
        }

        var text = textBox.Text ?? string.Empty;
        var caretIndex =
            GetSafeWordSplitIndex(
                text,
                textBox.CaretIndex);

        // Do not create an empty subtitle before or after the current one.
        if (caretIndex <= 0 || caretIndex >= text.Length)
        {
            return false;
        }

        var visibleBefore = text[..caretIndex].Trim();
        var visibleAfter = text[caretIndex..].Trim();

        if (visibleBefore.Length == 0 ||
            visibleAfter.Length == 0)
        {
            return false;
        }

        var originalStartMs = source.StartTime.TotalMilliseconds;
        var originalEndMs = source.EndTime.TotalMilliseconds;

        // Keep the hidden EBU/HTML colour tag when synchronizing the visible
        // Flow text back to the source subtitle.
        var sourceText =
            FlowTextParser.ApplyEditedText(
                source.Text,
                text);

        source.Text = sourceText;

        // The Flow caret index belongs to the visible text (font tags stripped),
        // while SplitManager expects an index in Source.Text. After
        // ApplyEditedText the visible text occurs as one contiguous substring,
        // so translate the caret into the tagged source string.
        var visibleTextStart =
            sourceText.IndexOf(
                text,
                StringComparison.Ordinal);

        if (visibleTextStart < 0)
        {
            return false;
        }

        var sourceCaretIndex =
            visibleTextStart + caretIndex;

        var originalMarginV = source.MarginV;
        var originalLineCount = GetPlainLineCount(source.Text);

        // Beta 23's SplitManager already provides the behaviour we need here:
        // proportional timing based on text length, configured minimum gap,
        // tag handling and automatic breaking of overlong split halves.
        var splitManager = new SplitManager();
        splitManager.Split(
            subtitles,
            source,
            sourceCaretIndex,
            string.Empty);

        if (sourceIndex + 1 >= subtitles.Count)
        {
            return false;
        }

        var newSubtitle = subtitles[sourceIndex + 1];

        if (ReferenceEquals(newSubtitle, source))
        {
            return false;
        }

        if (_vm.IsFormatEbu)
        {
            // Both halves must obey the same Teletext rule set as the normal
            // EBU editor: max two lines, 37 chars without colour and 36 with
            // colour. Rebalance before timing so CPS uses the final text.
            RebalanceTeletextSubtitle(source);
            RebalanceTeletextSubtitle(newSubtitle);
        }

        RedistributeSplitTiming(
            source,
            newSubtitle,
            originalStartMs,
            originalEndMs);

        // The SE reading-speed duration may need more room than the old subtitle
        // window provided. Keep the new Flow structure and, if necessary, ask
        // whether all following existing subtitles should be shifted together.
        Dispatcher.UIThread.Post(
            async () =>
            {
                await OfferShiftFollowingSubtitlesAsync(
                    newSubtitle);
            });

        RenumberSubtitles();

        if (_vm.IsFormatEbu)
        {
            AdjustTeletextRowAfterSplit(
                source,
                originalMarginV,
                originalLineCount);

            AdjustTeletextRowAfterSplit(
                newSubtitle,
                originalMarginV,
                originalLineCount);
        }

        _pendingFocusSource = newSubtitle;
        _pendingFocusAtStart = true;

        _vm.SelectedSubtitle = newSubtitle;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        newSubtitle));

            if (targetItem != null)
            {
                FocusTextBox(
                    targetItem,
                    focusAtStart: true);
            }

            CenterSelectedSubtitleInFlow();
        });

        return true;
    }

    private static bool IsValidTeletextVisibleText(
        string text,
        int maxCharacters)
    {
        var normalized =
            text.Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
                .Replace(
                    '\r',
                    '\n');

        var lines =
            normalized
                .Split('\n')
                .Where(line => line.Length > 0)
                .ToArray();

        return
            lines.Length >= 1 &&
            lines.Length <= 2 &&
            lines.All(
                line => line.Length <= maxCharacters);
    }

    private static void RebalanceTeletextSubtitle(
        SubtitleLineViewModel subtitle)
    {
        var parsed =
            FlowTextParser.Parse(
                subtitle.Text);

        var maxCharacters =
            string.IsNullOrWhiteSpace(
                parsed.ColorToken)
                ? 37
                : 36;

        var rebalanced =
            RebalanceTeletextVisibleText(
                parsed.Text,
                maxCharacters);

        if (rebalanced == parsed.Text)
        {
            return;
        }

        subtitle.Text =
            FlowTextParser.ApplyEditedText(
                subtitle.Text,
                rebalanced);
    }

    private static string RebalanceTeletextVisibleText(
        string text,
        int maxCharacters)
    {
        if (maxCharacters <= 0)
        {
            return text;
        }

        var normalized =
            text.Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
                .Replace(
                    '\r',
                    '\n');

        var existingLines =
            normalized
                .Split('\n')
                .Select(line => line.Trim())
                .ToList();

        // A split may leave a leading/trailing empty line (for example when
        // the caret was directly beside an existing line break). Teletext must
        // count only real text rows, otherwise a one-line result incorrectly
        // remains on the two-line TT position.
        while (existingLines.Count > 0 &&
               existingLines[0].Length == 0)
        {
            existingLines.RemoveAt(0);
        }

        while (existingLines.Count > 0 &&
               existingLines[^1].Length == 0)
        {
            existingLines.RemoveAt(
                existingLines.Count - 1);
        }

        normalized =
            string.Join(
                Environment.NewLine,
                existingLines);

        if (existingLines.Count <= 2 &&
            existingLines.All(
                line => line.Length <= maxCharacters))
        {
            return normalized;
        }

        if (FlowSentenceBoundaryHelper.TrySplitAtPreferredBoundary(
                normalized,
                out var beforeBoundary,
                out var afterBoundary))
        {
            var preferredLines = new[] { beforeBoundary, afterBoundary }
                .SelectMany(part => part
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Replace('\r', '\n')
                    .Split('\n'))
                .SelectMany(line => WrapTeletextLine(line, maxCharacters))
                .ToArray();

            if (preferredLines.Length <= 2)
            {
                return string.Join(Environment.NewLine, preferredLines);
            }

            return normalized;
        }

        // Rebalance only the visible text. Colour tags are restored afterwards
        // by FlowTextParser.ApplyEditedText.
        var words =
            normalized
                .Split(
                    new[] { ' ', '\t', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        var flattened =
            string.Join(
                " ",
                words);

        if (flattened.Length <= maxCharacters)
        {
            return flattened;
        }

        // Find the best word boundary that keeps both lines within the current
        // Teletext width (37 without colour, 36 with colour). Prefer the most
        // balanced split.
        var bestSplit = -1;
        var bestDifference = int.MaxValue;

        for (var i = 1; i < words.Length; i++)
        {
            var first =
                string.Join(
                    " ",
                    words.Take(i));

            var second =
                string.Join(
                    " ",
                    words.Skip(i));

            if (first.Length > maxCharacters ||
                second.Length > maxCharacters)
            {
                continue;
            }

            var difference =
                Math.Abs(
                    first.Length -
                    second.Length);

            if (difference < bestDifference)
            {
                bestDifference = difference;
                bestSplit = i;
            }
        }

        if (bestSplit > 0)
        {
            return
                string.Join(
                    " ",
                    words.Take(bestSplit)) +
                Environment.NewLine +
                string.Join(
                    " ",
                    words.Skip(bestSplit));
        }

        // If no word-boundary split can satisfy the limit, keep a strict
        // two-line Teletext result by using the last possible boundary in the
        // first maxCharacters characters. This only splits a word when there
        // is no legal word-boundary alternative.
        if (flattened.Length <= maxCharacters * 2)
        {
            var splitIndex =
                Math.Min(
                    maxCharacters,
                    flattened.Length);

            var preferredSpace =
                flattened.LastIndexOf(
                    ' ',
                    splitIndex - 1,
                    splitIndex);

            if (preferredSpace > 0)
            {
                splitIndex =
                    preferredSpace;
            }

            var first =
                flattened[..splitIndex]
                    .TrimEnd();

            var second =
                flattened[splitIndex..]
                    .TrimStart();

            if (first.Length <= maxCharacters &&
                second.Length <= maxCharacters)
            {
                return
                    first +
                    Environment.NewLine +
                    second;
            }

            // Never split a word merely to satisfy the Teletext width.
            // A genuinely overlong single word stays intact and can be flagged
            // by validation, but Flow must not change its spelling.
        }

        // More than two legal Teletext lines are required. Auto-flow will later
        // turn this into additional subtitles; until then do not silently lose
        // or truncate text.
        return normalized;
    }

    private static IEnumerable<string> WrapTeletextLine(
        string line,
        int maxCharacters)
    {
        var words = line.Split(
            new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        var current = string.Empty;
        foreach (var word in words)
        {
            if (current.Length == 0)
            {
                current = word;
            }
            else if (current.Length + 1 + word.Length <= maxCharacters)
            {
                current += " " + word;
            }
            else
            {
                yield return current;
                current = word;
            }
        }

        if (current.Length > 0)
        {
            yield return current;
        }
    }

    private void RedistributeSplitTiming(
        SubtitleLineViewModel first,
        SubtitleLineViewModel second,
        double originalStartMs,
        double originalEndMs)
    {
        // Re-use the same optimal-CPS timing calculation as SE5's
        // "Import plain text" workflow. Flow only decides where the two
        // subtitles start; SE5 decides how long each one should be displayed.
        var firstDurationMs =
            CalculateSeOptimalDurationMilliseconds(
                first);

        var secondDurationMs =
            CalculateSeOptimalDurationMilliseconds(
                second);

        var gapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        var firstStartMs =
            originalStartMs;

        var firstEndMs =
            firstStartMs +
            firstDurationMs;

        var secondStartMs =
            firstEndMs +
            gapMs;

        var secondEndMs =
            secondStartMs +
            secondDurationMs;

        first.SetStartTimeOnly(
            TimeSpan.FromMilliseconds(
                firstStartMs));

        first.EndTime =
            TimeSpan.FromMilliseconds(
                firstEndMs);

        second.SetStartTimeOnly(
            TimeSpan.FromMilliseconds(
                secondStartMs));

        second.EndTime =
            TimeSpan.FromMilliseconds(
                secondEndMs);
    }

    private double CalculateSeOptimalDurationMilliseconds(
        SubtitleLineViewModel subtitle)
    {
        // TimeCodeCalculator counts SubtitleLineViewModel.Text.Length.
        // Give it only the visible Flow text so EBU colour/alignment tags do
        // not artificially increase reading time.
        var visibleText =
            FlowTextParser.Parse(
                subtitle.Text)
                .Text;

        var timingProbe =
            new SubtitleLineViewModel(
                subtitle,
                generateNewId: true)
            {
                Text =
                    visibleText,
            };

        var timingList =
            new List<SubtitleLineViewModel>
            {
                timingProbe,
            };

        var selectedCps =
            _useOptimalReadingSpeed
                ? Se.Settings.General
                    .SubtitleOptimalCharactersPerSeconds
                : Se.Settings.General
                    .SubtitleMaximumCharactersPerSeconds;

        TimeCodeCalculator.CalculateTimeCodes(
            timingList,
            selectedCps,
            Se.Settings.General
                .SubtitleMaximumCharactersPerSeconds,
            (int)Math.Round(
                Math.Max(
                    0.0,
                    Se.Settings.General.MinimumBetweenLines
                        .GetMilliseconds())),
            Se.Settings.General
                .SubtitleMinimumDisplayMilliseconds,
            Se.Settings.General
                .SubtitleMaximumDisplayMilliseconds);

        return Math.Max(
            1.0,
            (timingProbe.EndTime -
             timingProbe.StartTime)
            .TotalMilliseconds);
    }

    private double CalculateSeOptimalDurationMilliseconds(
        string visibleText)
    {
        return CalculateSeOptimalDurationMilliseconds(
            new SubtitleLineViewModel
            {
                Text =
                    visibleText,
            });
    }

    private void ApplySeOptimalDurationKeepingStart(
        SubtitleLineViewModel subtitle)
    {
        var durationMs =
            CalculateSeOptimalDurationMilliseconds(
                subtitle);

        subtitle.EndTime =
            subtitle.StartTime +
            TimeSpan.FromMilliseconds(
                durationMs);
    }


    private async System.Threading.Tasks.Task OfferShiftFollowingSubtitlesAsync(
        SubtitleLineViewModel changedSubtitle)
    {
        var subtitles =
            _vm.Subtitles;

        var changedIndex =
            subtitles.IndexOf(
                changedSubtitle);

        if (changedIndex < 0 ||
            changedIndex + 1 >= subtitles.Count)
        {
            return;
        }

        var next =
            subtitles[
                changedIndex + 1];

        if (next.IsReferenceOnly)
        {
            return;
        }

        var gapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        var requiredNextStart =
            changedSubtitle.EndTime +
            TimeSpan.FromMilliseconds(
                gapMs);

        if (next.StartTime >=
            requiredNextStart)
        {
            return;
        }

        var shift =
            requiredNextStart -
            next.StartTime;

        var available =
            Math.Max(
                0.0,
                (next.StartTime -
                 changedSubtitle.StartTime)
                .TotalSeconds);

        var required =
            Math.Max(
                0.0,
                (requiredNextStart -
                 changedSubtitle.StartTime)
                .TotalSeconds);

        var owner =
            TopLevel.GetTopLevel(this)
            as Window;

        if (owner == null)
        {
            return;
        }

        var message =
            string.Format(
                CultureInfo.InvariantCulture,
                "Not enough time before the next subtitle.{0}{0}" +
                "Required: {1:0.00} s{0}" +
                "Available: {2:0.00} s{0}" +
                "Shift required: {3:0.00} s{0}{0}" +
                "Do you want to shift all following subtitles?",
                Environment.NewLine,
                required,
                available,
                shift.TotalSeconds);

        var result =
            await MessageBox.Show(
                owner,
                "Flow timing conflict",
                message,
                MessageBoxButtons.Custom2,
                MessageBoxIcon.Warning,
                "Cancel",
                "Shift following subtitles");

        if (result !=
            MessageBoxResult.Custom2)
        {
            return;
        }

        ShiftFollowingSubtitles(
            changedIndex + 1,
            shift);
    }

    private void ShiftFollowingSubtitles(
        int firstIndex,
        TimeSpan shift)
    {
        if (shift <= TimeSpan.Zero)
        {
            return;
        }

        var subtitles =
            _vm.Subtitles;

        for (var i = firstIndex;
             i < subtitles.Count;
             i++)
        {
            var subtitle =
                subtitles[i];

            if (subtitle.IsReferenceOnly)
            {
                continue;
            }

            var oldStart =
                subtitle.StartTime;

            var oldEnd =
                subtitle.EndTime;

            subtitle.SetStartTimeOnly(
                oldStart + shift);

            subtitle.EndTime =
                oldEnd + shift;
        }

        QueueRefresh();
    }

    private void RenumberSubtitles()
    {
        var number = 1;

        foreach (var subtitle in _vm.Subtitles)
        {
            if (subtitle.IsReferenceOnly)
            {
                continue;
            }

            subtitle.Number = number;
            number++;
        }
    }

    private static void AdjustTeletextRowAfterSplit(
        SubtitleLineViewModel subtitle,
        string originalMarginV,
        int originalLineCount)
    {
        var newLineCount =
            GetPlainLineCount(subtitle.Text);

        var newRow =
            TeletextRowHelper.GetRowKeepingBottomEdge(
                originalMarginV,
                originalLineCount,
                newLineCount,
                Configuration.Settings.SubtitleSettings
                    .EbuStlTeletextUseDoubleHeight);

        if (newRow.HasValue)
        {
            subtitle.MarginV =
                newRow.Value.ToString(
                    CultureInfo.InvariantCulture);
        }
    }

        private void TextBoxOnArrowKeyDown(
        FlowEditingItem item,
        TextBox textBox,
        KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        if (textBox.SelectionStart != textBox.SelectionEnd)
        {
            return;
        }

        var text = textBox.Text ?? string.Empty;
        var caretIndex = textBox.CaretIndex;

        if (e.Key == Key.Left && caretIndex == 0)
        {
            if (MoveToAdjacentSubtitle(item, direction: -1, focusAtStart: false))
            {
                e.Handled = true;
            }
            return;
        }

        if (e.Key == Key.Right && caretIndex >= text.Length)
        {
            if (MoveToAdjacentSubtitle(item, direction: 1, focusAtStart: true))
            {
                e.Handled = true;
            }
        }
    }

    private void TextBoxOnBackspaceKeyDown(
        FlowEditingItem item,
        TextBox textBox,
        KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None ||
            textBox.SelectionStart != textBox.SelectionEnd)
        {
            return;
        }

        var isBackspace =
            e.Key == Key.Back ||
            (OperatingSystem.IsMacOS() && e.Key == Key.Delete);

        if (isBackspace && textBox.CaretIndex == 0)
        {
            e.Handled = true;

            if (string.IsNullOrEmpty(
                    textBox.Text))
            {
                DeleteEmptySubtitleAndFocusPrevious(
                    item);

                return;
            }

            MergeWithPrevious(
                item);
        }
    }

    private bool DeleteEmptySubtitleAndFocusPrevious(
        FlowEditingItem currentItem)
    {
        var subtitles =
            _vm.Subtitles;

        var currentIndex =
            subtitles.IndexOf(
                currentItem.Source);

        if (currentIndex <= 0 ||
            currentItem.Source.IsReferenceOnly)
        {
            return false;
        }

        var previous =
            subtitles[currentIndex - 1];

        subtitles.RemoveAt(
            currentIndex);

        RenumberSubtitles();

        _selectedSources.Clear();
        _selectedSources.Add(
            previous);

        _selectionAnchorSource =
            previous;

        _pendingFocusSource =
            previous;

        _pendingFocusAtStart =
            false;

        _vm.SelectedSubtitle =
            previous;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        previous));

            if (targetItem != null &&
                _textBoxes.TryGetValue(
                    targetItem,
                    out var targetTextBox))
            {
                var targetTextLength =
                    (targetTextBox.Text ??
                     string.Empty).Length;

                targetTextBox.CaretIndex =
                    targetTextLength;

                targetTextBox.SelectionStart =
                    targetTextLength;

                targetTextBox.SelectionEnd =
                    targetTextLength;

                targetTextBox.Focus();
            }

            CenterSelectedSubtitleInFlow();
        });

        return true;
    }

    private bool MergeWithPrevious(
        FlowEditingItem currentItem)
    {
        var subtitles = _vm.Subtitles.ToList();

        var currentIndex =
            subtitles.IndexOf(currentItem.Source);

        if (currentIndex <= 0)
        {
            return false;
        }

        var previous =
            subtitles[currentIndex - 1];

        if (previous.IsReferenceOnly ||
            currentItem.Source.IsReferenceOnly)
        {
            return false;
        }

        var previousParsed =
            FlowTextParser.Parse(previous.Text);

        var currentParsed =
            FlowTextParser.Parse(currentItem.Source.Text);

        var preserveSentenceBoundary =
            _vm.IsFormatEbu &&
            FlowSentenceBoundaryHelper.IsPreferredBoundary(
                previousParsed.Text,
                currentParsed.Text);

        var actualGapMs =
            currentItem.Source.StartTime.TotalMilliseconds -
            previous.EndTime.TotalMilliseconds;

        var allowedGapMs =
            Math.Max(
                0.0,
                Se.Settings.General.MinimumBetweenLines
                    .GetMilliseconds());

        // Flow Backspace removes a normal subtitle boundary. It must not absorb
        // a deliberate larger timing gap into one long subtitle duration.
        if (actualGapMs > allowedGapMs + 0.5)
        {
            ShowMergeGapWarning(
                currentItem,
                actualGapMs,
                allowedGapMs);

            // Return true so the Backspace key is consumed even though no merge
            // took place. Both subtitles and their timing stay untouched.
            return true;
        }

        var previousVisibleText =
            previousParsed.Text;

        if (_vm.IsFormatEbu)
        {
            var mergedVisibleText = preserveSentenceBoundary
                ? previousParsed.Text.TrimEnd() + Environment.NewLine +
                  currentParsed.Text.TrimStart()
                : (previousParsed.Text.TrimEnd() + " " +
                   currentParsed.Text.TrimStart()).Trim();

            var hasColor =
                !string.IsNullOrWhiteSpace(previousParsed.ColorToken) ||
                !string.IsNullOrWhiteSpace(currentParsed.ColorToken);

            var maxCharacters =
                hasColor ? 36 : 37;

            var rebalanced =
                preserveSentenceBoundary
                    ? mergedVisibleText
                    : mergedVisibleText.Length <= maxCharacters
                    ? mergedVisibleText
                    : RebalanceTeletextVisibleText(
                        mergedVisibleText,
                        maxCharacters);

            var stopAtNextCompleteSentence =
                preserveSentenceBoundary &&
                FlowSentenceBoundaryHelper
                    .HasContentAfterFirstCompleteSentence(
                        currentParsed.Text);

            if (stopAtNextCompleteSentence ||
                !IsValidTeletextVisibleText(
                    rebalanced,
                    maxCharacters))
            {
                // Word-like Backspace across a subtitle boundary:
                // pull as many COMPLETE words as possible into the previous
                // two-line Teletext subtitle. Any overflow remains in the
                // current subtitle instead of blocking the operation.
                if (RedistributeBackspaceAcrossTeletextBoundary(
                        previous,
                        currentItem.Source,
                        previousParsed,
                        currentParsed,
                        maxCharacters,
                        preserveSentenceBoundary))
                {
                    return true;
                }

                if (preserveSentenceBoundary)
                {
                    FocusPreviousSubtitleAtProtectedBoundary(previous);
                }

                return true;
            }
        }

        if (_vm.IsFormatEbu &&
            !preserveSentenceBoundary)
        {
            var mergedVisibleText =
                (previousParsed.Text.TrimEnd() + " " +
                 currentParsed.Text.TrimStart()).Trim();

            var hasColor =
                !string.IsNullOrWhiteSpace(previousParsed.ColorToken) ||
                !string.IsNullOrWhiteSpace(currentParsed.ColorToken);

            var maxCharacters =
                hasColor ? 36 : 37;

            if (mergedVisibleText.Length <=
                maxCharacters)
            {
                previous.Text =
                    FlowTextParser.ApplyEditedText(
                        previous.Text,
                        previousParsed.Text.TrimEnd());

                currentItem.Source.Text =
                    FlowTextParser.ApplyEditedText(
                        currentItem.Source.Text,
                        currentParsed.Text.TrimStart());
            }
        }

        var visibleCharactersBeforeJoin =
            preserveSentenceBoundary
                ? CountCharactersWithoutLineBreaks(
                    previousParsed.Text + currentParsed.Text)
                : CountCharactersWithoutLineBreaks(
                    previousVisibleText);

         // Make sure the Flow row is also the selected subtitle in MainViewModel.
        // The normal SE merge command operates on SelectedSubtitle.
        if (!ReferenceEquals(
                _vm.SelectedSubtitle,
                currentItem.Source))
        {
            _vm.SelectedSubtitle =
                currentItem.Source;
        }

        // Re-use Subtitle Edit's existing merge logic for timing,
        // removal of the second subtitle, renumbering and selection.
        if (preserveSentenceBoundary)
        {
            _vm.MergeWithLineBeforeKeepBreaksCommand.Execute(null);
        }
        else
        {
            _vm.MergeWithLineBeforeCommand.Execute(null);
        }

        if (!_vm.Subtitles.Contains(previous))
        {
            return false;
        }

        // The text structure has changed, so recalculate the merged subtitle
        // with the same optimal reading-speed timing used by SE5 plain-text
        // import. A later Flow step handles any conflict with following TCs.
        ApplySeOptimalDurationKeepingStart(
            previous);

        if (_vm.IsFormatEbu)
        {
            // The normal SE merge updates the text but does not know about our
            // Flow-specific bottom-anchored Teletext convention. Re-apply the
            // correct TT start row from the FINAL merged line count:
            // 1 line -> TT 22, 2 lines -> TT 20 in double-height mode.
            var doubleHeight =
                Configuration.Settings.SubtitleSettings
                    .EbuStlTeletextUseDoubleHeight;

            var mergedLineCount =
                Math.Clamp(
                    GetPlainLineCount(
                        previous.Text),
                    1,
                    2);

            previous.MarginV =
                TeletextRowHelper
                    .GetBottomStartRow(
                        mergedLineCount,
                        doubleHeight)
                    .ToString(
                        CultureInfo.InvariantCulture);
        }

        Dispatcher.UIThread.Post(
            async () =>
            {
                await OfferShiftFollowingSubtitlesAsync(
                    previous);
            });

        _pendingFocusSource = previous;
        _pendingFocusAtStart = false;

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        previous));

            if (targetItem == null ||
                !_textBoxes.TryGetValue(
                    targetItem,
                    out var mergedTextBox))
            {
                Refresh();

                Dispatcher.UIThread.Post(() =>
                    FocusMergedTextAtJoin(
                        previous,
                        visibleCharactersBeforeJoin));

                return;
            }

            FocusMergedTextAtJoin(
                previous,
                visibleCharactersBeforeJoin);
        });

        return true;
    }

    private void FocusPreviousSubtitleAtProtectedBoundary(
        SubtitleLineViewModel previous)
    {
        _selectedSources.Clear();
        _selectedSources.Add(previous);
        _selectionAnchorSource = previous;
        _pendingFocusSource = previous;
        _pendingFocusAtStart = false;
        _vm.SelectedSubtitle = previous;

        UpdateSelectionVisuals();

        var previousItem = _items.FirstOrDefault(
            item => ReferenceEquals(item.Source, previous));

        if (previousItem != null)
        {
            _pendingFocusSource = null;
            FocusTextBox(previousItem, focusAtStart: false);
        }

        CenterSelectedSubtitleInFlow();
    }

    private bool RedistributeBackspaceAcrossTeletextBoundary(
        SubtitleLineViewModel previous,
        SubtitleLineViewModel current,
        FlowTextInfo previousParsed,
        FlowTextInfo currentParsed,
        int maxCharacters,
        bool preserveSentenceBoundary)
    {
        var combined = preserveSentenceBoundary
            ? currentParsed.Text.Trim()
            : (previousParsed.Text.TrimEnd() + " " +
               currentParsed.Text.TrimStart()).Trim();

        var words =
            combined
                .Replace(
                    "\r\n",
                    "\n",
                    StringComparison.Ordinal)
                .Replace(
                    '\r',
                    '\n')
                .Split(
                    new[] { ' ', '\t', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return false;
        }

        var bestPrefixWordCount =
            0;

        var bestPreviousText =
            string.Empty;

        if (preserveSentenceBoundary)
        {
            // A protected boundary is semantic, not spare line capacity. Move
            // exactly the next complete sentence when it fits, then stop even
            // if the second Teletext line still has room.
            var sentenceWordCount =
                FlowSentenceBoundaryHelper
                    .GetFirstCompleteSentenceWordCount(words);

            var candidate =
                string.Join(
                    " ",
                    words.Take(sentenceWordCount));

            var candidateFlow =
                previousParsed.Text.TrimEnd() +
                Environment.NewLine + candidate;

            if (IsValidTeletextVisibleText(
                    candidateFlow,
                    maxCharacters))
            {
                bestPrefixWordCount = sentenceWordCount;
                bestPreviousText = candidateFlow;
            }
        }
        else
        {
            // Find the longest word prefix that fits legally into the previous
            // subtitle. Existing words are never split.
            for (var count = 1;
                 count <= words.Length;
                 count++)
            {
                var candidate =
                    string.Join(
                        " ",
                        words.Take(count));

                var candidateFlow =
                    RebalanceTeletextVisibleText(
                    candidate,
                    maxCharacters);

                if (!IsValidTeletextVisibleText(
                        candidateFlow,
                        maxCharacters))
                {
                    break;
                }

                bestPrefixWordCount =
                    count;

                bestPreviousText =
                    candidateFlow;
            }
        }

        if (bestPrefixWordCount <= 0)
        {
            return false;
        }

        // Everything fits: fall through to the normal SE merge path by
        // returning false only when there is no remainder.
        if (bestPrefixWordCount >=
            words.Length)
        {
            return false;
        }

        var remainingWords =
            words.Skip(
                bestPrefixWordCount)
                .ToArray();

        var remainingText =
            string.Join(
                " ",
                remainingWords);

        // The current subtitle may itself require a normal two-line wrap.
        // Keep the word order stable.
        var currentText =
            RebalanceTeletextVisibleText(
                remainingText,
                maxCharacters);

        previous.Text =
            FlowTextParser.ApplyEditedText(
                previous.Text,
                bestPreviousText);

        current.Text =
            FlowTextParser.ApplyEditedText(
                current.Text,
                currentText);

        // Keep both subtitles bottom-anchored after their line counts change.
        var doubleHeight =
            Configuration.Settings.SubtitleSettings
                .EbuStlTeletextUseDoubleHeight;

        previous.MarginV =
            TeletextRowHelper
                .GetBottomStartRow(
                    GetPlainLineCount(
                        previous.Text),
                    doubleHeight)
                .ToString(
                    CultureInfo.InvariantCulture);

        current.MarginV =
            TeletextRowHelper
                .GetBottomStartRow(
                    GetPlainLineCount(
                        current.Text),
                    doubleHeight)
                .ToString(
                    CultureInfo.InvariantCulture);

        ApplySeOptimalDurationKeepingStart(
            previous);

        ApplySeOptimalDurationKeepingStart(
            current);

        // The previous subtitle may now run into the current subtitle, and
        // the current subtitle may run into its follower. Reuse Flow's normal
        // user-confirmed ripple handling for both boundaries.
        Dispatcher.UIThread.Post(
            async () =>
            {
                await OfferShiftFollowingSubtitlesAsync(
                    previous);

                await OfferShiftFollowingSubtitlesAsync(
                    current);
            });

        _pendingFocusSource =
            previous;

        _pendingFocusAtStart =
            false;

        _vm.SelectedSubtitle =
            previous;

        Refresh();

        Dispatcher.UIThread.Post(() =>
        {
            var targetItem =
                _items.FirstOrDefault(
                    x => ReferenceEquals(
                        x.Source,
                        previous));

            if (targetItem != null &&
                _textBoxes.TryGetValue(
                    targetItem,
                    out var targetTextBox))
            {
                var targetText =
                    targetTextBox.Text ??
                    string.Empty;

                targetTextBox.CaretIndex =
                    targetText.Length;

                targetTextBox.SelectionStart =
                    targetText.Length;

                targetTextBox.SelectionEnd =
                    targetText.Length;

                targetTextBox.Focus();
            }

            CenterSelectedSubtitleInFlow();
        });

        return true;
    }

    private void ShowMergeTextTooLongWarning(
        FlowEditingItem item,
        int maxCharacters)
    {
        if (!_textBoxes.TryGetValue(
                item,
                out var textBox))
        {
            return;
        }

        var message =
            string.Format(
                CultureInfo.InvariantCulture,
                "Merge not possible – too many characters (max {0} per line)",
                maxCharacters);

        ToolTip.SetTip(
            textBox,
            message);

        ToolTip.SetIsOpen(
            textBox,
            true);

        DispatcherTimer.RunOnce(
            () =>
            {
                ToolTip.SetIsOpen(
                    textBox,
                    false);
            },
            TimeSpan.FromSeconds(2.5));
    }

    private void ShowMergeGapWarning(
        FlowEditingItem item,
        double actualGapMs,
        double allowedGapMs)
    {
        if (!_textBoxes.TryGetValue(
                item,
                out var textBox))
        {
            return;
        }

        var message =
            string.Format(
                CultureInfo.InvariantCulture,
                "Gap too large to merge ({0:0.00} s; max {1:0.00} s)",
                actualGapMs / 1000.0,
                allowedGapMs / 1000.0);

        ToolTip.SetTip(
            textBox,
            message);

        ToolTip.SetIsOpen(
            textBox,
            true);

        DispatcherTimer.RunOnce(
            () =>
            {
                ToolTip.SetIsOpen(
                    textBox,
                    false);
            },
            TimeSpan.FromSeconds(2.5));
    }

    private void FocusMergedTextAtJoin(
        SubtitleLineViewModel source,
        int visibleCharactersBeforeJoin)
    {
        var targetItem =
            _items.FirstOrDefault(
                x => ReferenceEquals(
                    x.Source,
                    source));

        if (targetItem == null ||
            !_textBoxes.TryGetValue(
                targetItem,
                out var textBox))
        {
            return;
        }

        textBox.Focus();

        var text =
            textBox.Text ?? string.Empty;

        var caretIndex =
            GetCaretIndexForVisibleCharacterCount(
                text,
                visibleCharactersBeforeJoin);

        textBox.CaretIndex = caretIndex;
        textBox.SelectionStart = caretIndex;
        textBox.SelectionEnd = caretIndex;
    }

    private static int CountCharactersWithoutLineBreaks(
        string text)
    {
        var count = 0;

        foreach (var ch in text)
        {
            if (ch != '\r' &&
                ch != '\n')
            {
                count++;
            }
        }

        return count;
    }

    private static int GetCaretIndexForVisibleCharacterCount(
        string text,
        int visibleCharacterCount)
    {
        if (visibleCharacterCount <= 0)
        {
            return 0;
        }

        var count = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\r' &&
                text[i] != '\n')
            {
                count++;
            }

            if (count >= visibleCharacterCount)
            {
                return i + 1;
            }
        }

        return text.Length;
    }

    private static int GetPlainLineCount(
        string? sourceText)
    {
        var text =
            FlowTextParser.Parse(
                sourceText ?? string.Empty).Text;

        text =
            text.Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal);

        if (text.Length == 0)
        {
            return 1;
        }

        return text.Split('\n').Length;
    }

    private bool MoveToAdjacentSubtitle(
        FlowEditingItem currentItem,
        int direction,
        bool focusAtStart)
    {
        var subtitles = _vm.Subtitles.ToList();

        var currentIndex =
            subtitles.IndexOf(currentItem.Source);

        if (currentIndex < 0)
        {
            return false;
        }

        var targetIndex =
            currentIndex + direction;

        if (targetIndex < 0 ||
            targetIndex >= subtitles.Count)
        {
            return false;
        }

        var targetSource =
            subtitles[targetIndex];

        var targetItem =
            _items.FirstOrDefault(
                item => ReferenceEquals(
                    item.Source,
                    targetSource));

        if (targetItem != null)
        {
            _vm.SelectedSubtitle = targetSource;

            UpdateSelectionVisuals();

            FocusTextBox(
                targetItem,
                focusAtStart);

            return true;
        }

        _pendingFocusSource = targetSource;
        _pendingFocusAtStart = focusAtStart;

        _vm.SelectedSubtitle = targetSource;

        return true;
    }

    private void FocusTextBox(
        FlowEditingItem item,
        bool focusAtStart)
    {
        if (!_textBoxes.TryGetValue(
                item,
                out var textBox))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            textBox.Focus();

            var textLength =
                (textBox.Text ?? string.Empty).Length;

            var caretIndex =
                focusAtStart
                    ? 0
                    : textLength;

            textBox.CaretIndex = caretIndex;
            textBox.SelectionStart = caretIndex;
            textBox.SelectionEnd = caretIndex;
        });
    }

    private void ApplyPendingFocus()
    {
        if (_pendingFocusSource == null)
        {
            return;
        }

        var targetItem =
            _items.FirstOrDefault(
                item => ReferenceEquals(
                    item.Source,
                    _pendingFocusSource));

        if (targetItem == null)
        {
            return;
        }

        var focusAtStart =
            _pendingFocusAtStart;

        _pendingFocusSource = null;

        FocusTextBox(
            targetItem,
            focusAtStart);
    }

    private void SelectItem(
        FlowEditingItem item)
    {
        _selectedSources.Clear();

        _selectedSources.Add(
            item.Source);

        _selectionAnchorSource =
            item.Source;

        if (!ReferenceEquals(
                _vm.SelectedSubtitle,
                item.Source))
        {
            _vm.SelectedSubtitle =
                item.Source;
        }

        UpdateSelectionVisuals();
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var item in _items)
        {
            var isCurrent =
                _selectedSources.Contains(
                    item.Source);

            if (_rowBorders.TryGetValue(
                    item,
                    out var border))
            {
                border.Background =
                    GetRowBackground(isCurrent);
            }

            if (_numberBlocks.TryGetValue(
                    item,
                    out var number))
            {
                number.Opacity =
                    isCurrent ? 1.0 : 0.65;

                number.FontWeight =
                    isCurrent
                        ? FontWeight.SemiBold
                        : FontWeight.Normal;
            }
        }
    }

    private bool IsSubtitleCurrentlyVisible(
        SubtitleLineViewModel? subtitle)
    {
        if (subtitle == null)
        {
            return false;
        }

        return _items.Any(
            item => ReferenceEquals(
                item.Source,
                subtitle));
    }

      private void VmOnPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        if (e.PropertyName != nameof(MainViewModel.SelectedSubtitle) &&
            e.PropertyName != nameof(MainViewModel.SelectedSubtitleIndex))
        {
            return;
        }

        // A click in the main subtitle grid can update SelectedSubtitleIndex
        // before/without the Flow view seeing SelectedSubtitle. Synchronize
        // the selected object explicitly from the grid index.
        if (e.PropertyName == nameof(MainViewModel.SelectedSubtitleIndex) &&
            _vm.SelectedSubtitleIndex.HasValue)
        {
            var index = _vm.SelectedSubtitleIndex.Value;

            if (index >= 0 &&
                index < _vm.Subtitles.Count)
            {
                var selected = _vm.Subtitles[index];

                if (!ReferenceEquals(
                        _vm.SelectedSubtitle,
                        selected))
                {
                    _vm.SelectedSubtitle = selected;
                    return;
                }
            }
        }

        if (IsSubtitleCurrentlyVisible(
                _vm.SelectedSubtitle))
        {
            if (_vm.SelectedSubtitle != null &&
                !_selectedSources.Contains(
                    _vm.SelectedSubtitle))
            {
                _selectedSources.Clear();

                _selectedSources.Add(
                    _vm.SelectedSubtitle);

                _selectionAnchorSource =
                    _vm.SelectedSubtitle;
            }

            UpdateSelectionVisuals();
            ApplyPendingFocus();
            CenterSelectedSubtitleInFlow();
            return;
        }

        Refresh();

        Dispatcher.UIThread.Post(
            CenterSelectedSubtitleInFlow);
    }
    private void CenterSelectedSubtitleInFlow()
    {
        var selected =
            _vm.SelectedSubtitle;

        if (selected == null)
        {
            return;
        }

        var item =
            _items.FirstOrDefault(
                x => ReferenceEquals(
                    x.Source,
                    selected));

        if (item == null ||
            !_rowBorders.TryGetValue(
                item,
                out var border))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var point =
                border.TranslatePoint(
                    new Point(0, 0),
                    _itemsPanel);

            if (!point.HasValue)
            {
                return;
            }

            var targetOffset =
                point.Value.Y +
                border.Bounds.Height / 2 -
                _scrollViewer.Viewport.Height / 2;

            var maxOffset =
                Math.Max(
                    0,
                    _scrollViewer.Extent.Height -
                    _scrollViewer.Viewport.Height);

            targetOffset =
                Math.Max(
                    0,
                    Math.Min(
                        targetOffset,
                        maxOffset));

            _scrollViewer.Offset =
                new Vector(
                    _scrollViewer.Offset.X,
                    targetOffset);
        });
    }
    private void QueueRefresh()
    {
        if (!IsVisible)
        {
            return;
        }

        if (_refreshQueued)
        {
            return;
        }

        _refreshQueued = true;
        var generation = ++_refreshGeneration;

        Dispatcher.UIThread.Post(
            () =>
            {
                if (generation != _refreshGeneration)
                {
                    return;
                }

                _refreshQueued = false;

                if (!IsVisible)
                {
                    return;
                }

                Refresh();
            },
            DispatcherPriority.Background);
    }

    private void SubtitlesOnCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        if (IsVisible)
        {
            QueueRefresh();
        }
    }

    private void DisposeItems()
    {
        foreach (var item in _items)
        {
            item.Dispose();
        }

        _items.Clear();

        _textBoxes.Clear();
        _rowBorders.Clear();
        _numberBlocks.Clear();
    }

    private void Detach()
    {
        DisposeItems();

        _vm.PropertyChanged -=
            VmOnPropertyChanged;

        if (_observableSubtitles != null)
        {
            _observableSubtitles.CollectionChanged -=
                SubtitlesOnCollectionChanged;
        }
    }
}

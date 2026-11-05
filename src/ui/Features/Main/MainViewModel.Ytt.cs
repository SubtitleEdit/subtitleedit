using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ytt;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

// YouTube timed text (srv3/.ytt): the styles window, and the grid and text box menu items that
// set a line's pen/position/window style or wrap the selected text in a pen.
public partial class MainViewModel
{
    private const string YttMenuTag = "Ytt";
    private static readonly Regex YttSpanTagRegex = new("</?s\\b[^>]*>", RegexOptions.Compiled);

    [ObservableProperty] private bool _isFormatYtt;
    [ObservableProperty] private bool _areYttContentMenuItemsVisible;

    public MenuItem MenuItemYtt { get; set; } = new();

    /// <summary>The grid and text box read pens from here - see <see cref="YttPenLookup"/>.</summary>
    private void UpdateYttPenLookup(string? header)
    {
        if (SelectedSubtitleFormat is YouTubeTimedText)
        {
            YttPenLookup.SetHeader(header);
        }
        else
        {
            YttPenLookup.Clear();
        }
    }

    private void SetYttHeader(string header)
    {
        _subtitle.Header = header;
        UpdateYttPenLookup(header);

        // the grid draws pens from the header, so re-render the rows
        foreach (var row in Subtitles)
        {
            row.RefreshTextFormatting();
        }

        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowYttStyles()
    {
        if (Window == null || !IsFormatYtt)
        {
            return;
        }

        var selectedPen = GetFirstYttPenOfSelectedLine();
        var result = await ShowDialogAsync<YttStylesWindow, YttStylesViewModel>(vm =>
        {
            // GetUpdateSubtitle: the usage counts come from the grid's current lines
            var subtitle = GetUpdateSubtitle();
            subtitle.Header = _subtitle.Header;
            vm.Initialize(subtitle, _subtitleFileName ?? string.Empty, selectedPen);
        });

        if (result.OkPressed)
        {
            SetYttHeader(result.Header);
        }
    }

    private int? GetFirstYttPenOfSelectedLine()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return null;
        }

        var spanPen = YttText.Parse(selected.Text, out _).Select(r => r.PenId).FirstOrDefault(p => p != null);
        return spanPen ?? YttHead.ParseInt(selected.Style);
    }

    /// <summary>The grid context menu's YouTube submenu: line pen, position and window style.</summary>
    private void FillYttMenu()
    {
        MenuItemYtt.Items.Clear();
        var head = YttHead.Parse(_subtitle.Header);

        var linePen = new MenuItem { Header = Se.Language.File.Ytt.LinePen };
        linePen.Items.Add(new MenuItem { Header = Se.Language.File.Ytt.Default, Command = SetYttLinePenCommand, CommandParameter = string.Empty });
        foreach (var pen in head.Pens.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            linePen.Items.Add(new MenuItem
            {
                Header = string.Format(Se.Language.File.Ytt.PenX, pen.Id) + " - " + new YttPenDisplay(pen).Description,
                Command = SetYttLinePenCommand,
                CommandParameter = pen.Id.ToString(CultureInfo.InvariantCulture),
            });
        }

        MenuItemYtt.Items.Add(linePen);

        var position = new MenuItem { Header = Se.Language.File.Ytt.SetPosition };
        position.Items.Add(new MenuItem { Header = Se.Language.File.Ytt.Default, Command = SetYttPositionCommand, CommandParameter = string.Empty });
        foreach (var wp in head.WindowPositions.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            position.Items.Add(new MenuItem
            {
                Header = string.Format(Se.Language.File.Ytt.WindowPositionX, wp.Id) + " - " + new YttPositionDisplay(wp).Description,
                Command = SetYttPositionCommand,
                CommandParameter = wp.Id.ToString(CultureInfo.InvariantCulture),
            });
        }

        position.Items.Add(new Separator());
        var anchorNames = YttStylesViewModel.AnchorPointNames;
        for (var anchorPoint = 0; anchorPoint < anchorNames.Length; anchorPoint++)
        {
            position.Items.Add(new MenuItem
            {
                Header = anchorNames[anchorPoint],
                Command = SetYttPositionCommand,
                CommandParameter = "ap" + anchorPoint.ToString(CultureInfo.InvariantCulture),
            });
        }

        MenuItemYtt.Items.Add(position);

        var windowStyle = new MenuItem { Header = Se.Language.File.Ytt.WindowStyle };
        windowStyle.Items.Add(new MenuItem { Header = Se.Language.File.Ytt.Default, Command = SetYttWindowStyleCommand, CommandParameter = string.Empty });
        foreach (var ws in head.WindowStyles.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            windowStyle.Items.Add(new MenuItem
            {
                Header = string.Format(Se.Language.File.Ytt.WindowStyleX, ws.Id) + " - " + new YttWindowStyleDisplay(ws).Description,
                Command = SetYttWindowStyleCommand,
                CommandParameter = ws.Id.ToString(CultureInfo.InvariantCulture),
            });
        }

        MenuItemYtt.Items.Add(windowStyle);
        MenuItemYtt.Items.Add(new Separator());
        MenuItemYtt.Items.Add(new MenuItem { Header = Se.Language.File.Ytt.StylesDotDotDot, Command = ShowYttStylesCommand });
    }

    [RelayCommand]
    private void SetYttLinePen(string? penId)
    {
        var selectedItems = SubtitleGridSelectedItems.Cast<SubtitleLineViewModel>().ToList();
        var style = string.IsNullOrEmpty(penId) ? null : penId;
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in selectedItems)
            {
                row.Style = style!;
                row.RefreshTextFormatting();
            }

            RefreshSubtitlePreview();
        });
    }

    /// <summary>
    /// Sets the window position of the selected lines: an existing wp id, "ap0"-"ap8" for an anchor
    /// point at its default spot (found or added, with a matching justification), or empty for none.
    /// </summary>
    [RelayCommand]
    private void SetYttPosition(string? parameter)
    {
        var selectedItems = SubtitleGridSelectedItems.Cast<SubtitleLineViewModel>().ToList();
        string? region = null;
        string? effect = null;
        var setEffect = false;
        if (parameter != null && parameter.StartsWith("ap", StringComparison.Ordinal) &&
            int.TryParse(parameter.AsSpan(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var anchorPoint))
        {
            var head = YttHead.Parse(_subtitle.Header);
            YttWindowPosition.GetDefaultCoordinates(anchorPoint, out var horizontal, out var vertical);
            region = head.FindOrAddWindowPosition(new YttWindowPosition { AnchorPoint = anchorPoint, Horizontal = horizontal, Vertical = vertical })
                .ToString(CultureInfo.InvariantCulture);
            var justify = anchorPoint % 3 == 0 ? 0 : anchorPoint % 3 == 2 ? 1 : 2;
            effect = head.FindOrAddWindowStyle(new YttWindowStyle { Justify = justify }).ToString(CultureInfo.InvariantCulture);
            setEffect = true;
            SetYttHeader(head.ToHeader());
        }
        else if (!string.IsNullOrEmpty(parameter))
        {
            region = parameter;
        }

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in selectedItems)
            {
                row.Region = region!;
                if (setEffect)
                {
                    row.Effect = effect!;
                }
            }

            RefreshSubtitlePreview();
        });
    }

    [RelayCommand]
    private void SetYttWindowStyle(string? windowStyleId)
    {
        var selectedItems = SubtitleGridSelectedItems.Cast<SubtitleLineViewModel>().ToList();
        var effect = string.IsNullOrEmpty(windowStyleId) ? null : windowStyleId;
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in selectedItems)
            {
                row.Effect = effect!;
            }

            RefreshSubtitlePreview();
        });
    }

    /// <summary>The text box context menu's YouTube submenu - rebuilt on each opening, like the spell check items.</summary>
    private void AddYttTextBoxMenuItems(MenuFlyout flyout)
    {
        foreach (var item in flyout.Items.Where(item => item is Control { Tag: YttMenuTag }).ToList())
        {
            flyout.Items.Remove(item);
        }

        if (!IsFormatYtt || SelectedSubtitle == null)
        {
            return;
        }

        var head = YttHead.Parse(_subtitle.Header);
        var youTube = new MenuItem { Header = Se.Language.File.Ytt.YouTube, Tag = YttMenuTag };

        var setPen = new MenuItem { Header = Se.Language.File.Ytt.SetPenForSelection };
        foreach (var pen in head.Pens.Where(p => p.Id != 0).OrderBy(p => p.Id))
        {
            var penId = pen.Id;
            var menuItem = new MenuItem { Header = string.Format(Se.Language.File.Ytt.PenX, pen.Id) + " - " + new YttPenDisplay(pen).Description };
            menuItem.Click += (_, _) => WrapYttSelection("<s p=\"" + penId.ToString(CultureInfo.InvariantCulture) + "\">");
            setPen.Items.Add(menuItem);
        }

        if (setPen.Items.Count > 0)
        {
            setPen.Items.Add(new Separator());
        }

        setPen.Items.Add(new MenuItem { Header = Se.Language.File.Ytt.StylesDotDotDot, Command = ShowYttStylesCommand });
        youTube.Items.Add(setPen);

        var karaoke = new MenuItem { Header = Se.Language.File.Ytt.InsertKaraokeTime };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(karaoke, Se.Language.File.Ytt.InsertKaraokeTimeHint);
        }

        karaoke.Click += (_, _) => InsertYttKaraokeTime();
        youTube.Items.Add(karaoke);

        var remove = new MenuItem { Header = Se.Language.File.Ytt.RemovePenTags };
        remove.Click += (_, _) =>
        {
            var tb = EditTextBox;
            if (tb.SelectionLength > 0)
            {
                var start = Math.Min(tb.SelectionStart, tb.SelectionEnd);
                var length = tb.SelectionLength;
                var selected = YttSpanTagRegex.Replace(tb.Text.Substring(start, length), string.Empty);
                tb.Text = tb.Text.Remove(start, length).Insert(start, selected);
                tb.Select(start, selected.Length);
            }
            else
            {
                tb.Text = YttSpanTagRegex.Replace(tb.Text, string.Empty);
            }
        };
        youTube.Items.Add(remove);
        youTube.Items.Add(new Separator());
        youTube.Items.Add(new MenuItem { Header = Se.Language.File.Ytt.StylesDotDotDot, Command = ShowYttStylesCommand });

        flyout.Items.Add(new Separator { Tag = YttMenuTag });
        flyout.Items.Add(youTube);
    }

    /// <summary>Wraps the selection (or the whole text) in an s tag.</summary>
    private void WrapYttSelection(string startTag)
    {
        var tb = EditTextBox;
        var text = tb.Text ?? string.Empty;
        var start = 0;
        var length = text.Length;
        if (tb.SelectionLength > 0)
        {
            start = Math.Min(tb.SelectionStart, tb.SelectionEnd);
            length = tb.SelectionLength;
        }

        var inner = text.Substring(start, length);
        tb.Text = text.Remove(start, length).Insert(start, startTag + inner + "</s>");
        tb.Select(start, startTag.Length + inner.Length + 4);
        tb.Focus();
    }

    /// <summary>Wraps the selection in an s tag that appears at the video position (karaoke).</summary>
    private void InsertYttKaraokeTime()
    {
        var row = SelectedSubtitle;
        if (row == null)
        {
            return;
        }

        var offset = 0;
        var vp = GetVideoPlayerControl();
        if (vp != null && !string.IsNullOrEmpty(_videoFileName))
        {
            offset = (int)Math.Round(Math.Max(0, vp.Position * 1000 - row.StartTime.TotalMilliseconds));
        }

        WrapYttSelection("<s t=\"" + offset.ToString(CultureInfo.InvariantCulture) + "\">");
    }
}

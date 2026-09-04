using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>
/// Maps the rows of "List errors" onto <see cref="ReportExport"/> (#14379 - the list had to be
/// screenshotted to be shared or handed to an AI). Every writer takes the rows exactly as the
/// window shows them, so an active summary-card filter is part of what is exported.
/// </summary>
public static class ErrorListExport
{
    public static string ToTabSeparated(IEnumerable<ErrorListItem> items)
    {
        return ReportExport.ToTabSeparated(MakeData(items, string.Empty, null));
    }

    public static string ToPlainText(IEnumerable<ErrorListItem> items, string summary, string? subtitleFileName)
    {
        return ReportExport.ToPlainText(MakeData(items, summary, subtitleFileName));
    }

    public static byte[] ToXlsx(IEnumerable<ErrorListItem> items)
    {
        return ReportExport.ToXlsx(MakeData(items, string.Empty, null));
    }

    public static string ToHtml(IEnumerable<ErrorListItem> items, string summary, string? subtitleFileName)
    {
        return ReportExport.ToHtml(MakeData(items, summary, subtitleFileName));
    }

    private static ReportExportData MakeData(IEnumerable<ErrorListItem> items, string summary, string? subtitleFileName)
    {
        var l = Se.Language.ErrorList;
        var list = items.ToList();
        return new ReportExportData
        {
            Title = l.Title,
            Summary = summary,
            FileName = subtitleFileName,
            CategoryHeader = Se.Language.General.Error,
            DetailHeader = l.Detail,
            AllLabel = l.All,
            AllColor = LineError.AllColor,
            Chips = Enum.GetValues<LineErrorType>().Select(type => new ReportExportChip
            {
                Id = ((int)type).ToString(),
                Label = LineError.GetLabel(type),
                Hint = LineError.GetHint(type),
                Color = LineError.GetColor(type),
                Count = list.Count(p => p.Type == type),
            }).ToList(),
            Rows = list.Select(item => new ReportExportRow
            {
                Number = item.Number,
                Category = item.Category,
                Show = item.Show,
                Hide = item.Hide,
                Detail = item.Detail,
                Text = item.Text,
                ChipId = ((int)item.Type).ToString(),
                Color = LineError.GetColor(item.Type),
            }).ToList(),
        };
    }
}

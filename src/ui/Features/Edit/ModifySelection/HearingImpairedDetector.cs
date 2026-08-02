using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

/// <summary>
/// Matches the lines that "Remove text for hearing impaired" would change, so hearing impaired
/// text can be selected - e.g. cut into a separate file before aligning and merged back after (#13002).
/// Everything except the ticked options comes from Tools > Remove text for hearing impaired, so the
/// selection stays in sync with what that tool would actually remove.
/// </summary>
public class HearingImpairedDetector
{
    private readonly IEnumerable<SubtitleLineViewModel> _lines;

    // Everything is built on first use and then reused: language auto detect, the name list and the
    // interjections are all too expensive to redo per line, and IsMatch runs for every line on the
    // preview timer - and for every rule the dialog offers, most of which are never selected.
    private Subtitle? _subtitle;
    private string? _twoLetterLanguageCode;
    private RemoveTextForHI? _removeTextForHi;
    private int _signature = -1;

    public HearingImpairedDetector(IEnumerable<SubtitleLineViewModel> lines)
    {
        _lines = lines;
    }

    public bool IsMatch(string text, HearingImpairedRuleOptions options)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var signature = options.GetSignature();
        if (signature == 0)
        {
            return false;
        }

        if (_removeTextForHi == null || signature != _signature)
        {
            _signature = signature;
            _removeTextForHi = MakeRemoveTextForHi(options);
        }

        // No subtitle/index is passed on purpose: the dash and dialog fixes that need them are
        // formatting, not hearing impaired text. Ignore white-space-only differences, like the
        // "Remove text for hearing impaired" window does.
        var newText = _removeTextForHi.RemoveTextFromHearImpaired(text, _twoLetterLanguageCode ?? "en");
        return text.RemoveChar(' ') != newText.RemoveChar(' ');
    }

    private RemoveTextForHI MakeRemoveTextForHi(HearingImpairedRuleOptions options)
    {
        if (_subtitle == null)
        {
            _subtitle = new Subtitle();
            _subtitle.Paragraphs.AddRange(_lines.Select(p =>
                new Paragraph(p.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)));

            var languageCode = _subtitle.Paragraphs.Count > 0
                ? LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle)
                : string.Empty;
            _twoLetterLanguageCode = string.IsNullOrEmpty(languageCode) ? "en" : languageCode;
        }

        var saved = Se.Settings.Tools.RemoveTextForHi;

        var settings = new RemoveTextForHISettings(_subtitle)
        {
            OnlyIfInSeparateLine = saved.IsOnlySeparateLine,
            RemoveTextBetweenSquares = options.Brackets,
            RemoveTextBetweenBrackets = options.CurlyBrackets,
            RemoveTextBetweenParentheses = options.Parentheses,
            RemoveTextBetweenQuestionMarks = false,
            RemoveTextBetweenCustomTags = options.Custom,
            CustomStart = saved.CustomStart,
            CustomEnd = saved.CustomEnd,
            RemoveTextBeforeColon = options.TextBeforeColon,
            RemoveTextBeforeColonOnlyUppercase = saved.IsRemoveTextBeforeColonUppercaseOn,
            ColonSeparateLine = saved.IsRemoveTextBeforeColonSeparateLineOn,
            RemoveIfAllUppercase = options.UppercaseLine,
            UppercaseWhitelist = SplitList(saved.UppercaseWhitelist),
            RemoveWhereContains = options.LineContains,
            RemoveIfTextContains = SplitList(saved.TextContains),
            RemoveIfOnlyMusicSymbols = options.MusicSymbols,
            RemoveInterjections = options.Interjections,
            RemoveInterjectionsOnlySeparateLine = options.Interjections && saved.IsInterjectionsSeparateLineOn,
        };

        var removeTextForHi = new RemoveTextForHI(settings);
        var interjections = saved.Interjections.FirstOrDefault(p => p.LanguageCode == _twoLetterLanguageCode) ??
                            saved.Interjections.FirstOrDefault(p => p.LanguageCode == "en");
        removeTextForHi.ReloadInterjection(
            interjections?.Interjections ?? new List<string>(),
            interjections?.SkipStartList ?? new List<string>());

        return removeTextForHi;
    }

    private static List<string> SplitList(string? commaSeparated)
    {
        return (commaSeparated ?? string.Empty)
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();
    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

/// <summary>
/// The actor conversion shared by the "Convert actors" dialog and batch convert.
/// </summary>
public static class ConvertActorsHelper
{
    public static ActorConverter MakeConverter(SubtitleFormat format, string languageCode, ConvertActorType toType)
    {
        return new ActorConverter(format, languageCode)
        {
            ToSquare = toType == ConvertActorType.InlineSquareBrackets,
            ToParentheses = toType == ConvertActorType.InlineParentheses,
            ToColon = toType == ConvertActorType.InlineColon,
            ToActor = toType == ConvertActorType.Actor,
        };
    }

    /// <summary>
    /// Converts one paragraph from <paramref name="fromType"/>, or returns null when the paragraph
    /// has no actor of that kind.
    /// </summary>
    public static ActorConverterResult? ConvertParagraph(ActorConverter converter, Paragraph p, ConvertActorType fromType, int? changeCasing, SKColor? color)
    {
        if (fromType == ConvertActorType.InlineSquareBrackets && Contains(p.Text, '[', ']'))
        {
            return converter.FixActors(p, '[', ']', changeCasing, color);
        }

        if (fromType == ConvertActorType.InlineParentheses && Contains(p.Text, '(', ')'))
        {
            return converter.FixActors(p, '(', ')', changeCasing, color);
        }

        if (fromType == ConvertActorType.InlineColon && p.Text.Contains(':'))
        {
            return converter.FixActorsFromBeforeColon(p, ':', changeCasing, color);
        }

        if (fromType == ConvertActorType.Actor && !string.IsNullOrEmpty(p.Actor))
        {
            return converter.FixActorsFromActor(p, changeCasing, color);
        }

        return null;
    }

    /// <summary>
    /// Converts all paragraphs of <paramref name="subtitle"/> in place - what the dialog does with
    /// every suggested conversion ticked ("only names" leaves the non-name conversions out, as the
    /// dialog leaves them unticked).
    /// </summary>
    public static void ConvertSubtitle(Subtitle subtitle, SubtitleFormat format, ConvertActorType fromType, ConvertActorType toType, int? changeCasing, SKColor? color, bool onlyNames)
    {
        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        var converter = MakeConverter(format, languageCode, toType);

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var oldText = p.Text;
            var result = ConvertParagraph(converter, p, fromType, changeCasing, color);
            if (result == null || result.Skip || (onlyNames && !result.Selected))
            {
                continue;
            }

            var newText = result.Paragraph.Text;
            var newActor = result.Paragraph.Actor ?? string.Empty;
            if (newText == oldText && newActor == (p.Actor ?? string.Empty) && result.NextParagraph == null)
            {
                continue;
            }

            p.Text = newText;
            p.Actor = newActor;

            if (converter.ToActor && result.NextParagraph != null)
            {
                var next = new Paragraph(p, true)
                {
                    Text = result.NextParagraph.Text,
                    Actor = result.NextParagraph.Actor ?? string.Empty,
                };
                subtitle.Paragraphs.Insert(i + 1, next);
                i++;
            }
        }

        subtitle.Renumber();
    }

    private static bool Contains(string text, char start, char end)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var startIdx = text.IndexOf(start);
        if (startIdx < 0)
        {
            return false;
        }

        return text.IndexOf(end) > startIdx;
    }
}

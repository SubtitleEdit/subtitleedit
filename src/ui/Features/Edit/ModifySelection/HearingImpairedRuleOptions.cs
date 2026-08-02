using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

/// <summary>
/// Which kinds of hearing impaired text the "Hearing impaired (SDH)" rule looks for.
/// Starts out as whatever Tools > Remove text for hearing impaired is set to remove, and can then
/// be narrowed or widened for this run only - see <see cref="HearingImpairedDetector"/>.
/// </summary>
public class HearingImpairedRuleOptions
{
    public bool Brackets { get; set; }
    public bool CurlyBrackets { get; set; }
    public bool Parentheses { get; set; }
    public bool Custom { get; set; }
    public bool TextBeforeColon { get; set; }
    public bool UppercaseLine { get; set; }
    public bool LineContains { get; set; }
    public bool MusicSymbols { get; set; }
    public bool Interjections { get; set; }

    public static HearingImpairedRuleOptions FromSettings()
    {
        var settings = Se.Settings.Tools.RemoveTextForHi;

        return new HearingImpairedRuleOptions
        {
            Brackets = settings.IsRemoveBracketsOn,
            CurlyBrackets = settings.IsRemoveCurlyBracketsOn,
            Parentheses = settings.IsRemoveParenthesesOn,
            Custom = settings.IsRemoveCustomOn,
            TextBeforeColon = settings.IsRemoveTextBeforeColonOn,
            UppercaseLine = settings.IsRemoveTextUppercaseLineOn,
            LineContains = settings.IsRemoveTextContainsOn,
            MusicSymbols = settings.IsRemoveOnlyMusicSymbolsOn,
            Interjections = settings.IsRemoveInterjectionsOn,
        };
    }

    public void CopyFrom(HearingImpairedRuleOptions options)
    {
        Brackets = options.Brackets;
        CurlyBrackets = options.CurlyBrackets;
        Parentheses = options.Parentheses;
        Custom = options.Custom;
        TextBeforeColon = options.TextBeforeColon;
        UppercaseLine = options.UppercaseLine;
        LineContains = options.LineContains;
        MusicSymbols = options.MusicSymbols;
        Interjections = options.Interjections;
    }

    // Cheap value identity, so the detector knows when to rebuild the engine without comparing
    // it field by field on every line.
    public int GetSignature()
    {
        var signature = 0;
        signature |= Brackets ? 1 : 0;
        signature |= CurlyBrackets ? 1 << 1 : 0;
        signature |= Parentheses ? 1 << 2 : 0;
        signature |= Custom ? 1 << 3 : 0;
        signature |= TextBeforeColon ? 1 << 4 : 0;
        signature |= UppercaseLine ? 1 << 5 : 0;
        signature |= LineContains ? 1 << 6 : 0;
        signature |= MusicSymbols ? 1 << 7 : 0;
        signature |= Interjections ? 1 << 8 : 0;
        return signature;
    }
}

using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public class AiReviewPromptTemplate
{
    public string Name { get; }
    public string Prompt { get; }

    public AiReviewPromptTemplate(string name, string prompt)
    {
        Name = name;
        Prompt = prompt;
    }

    public override string ToString() => Name;
}

/// <summary>
/// Built-in starting points for the review prompt (issue #15290). Picking one only fills the
/// prompt box - the user can still edit it, and the JSON protocol is appended as always.
/// </summary>
public static class AiReviewPromptTemplates
{
    public const string SpeechToTextPrompt =
        "You are cleaning up {language} subtitles produced by automatic speech recognition." +
        "\n\nFix words the recognizer misheard (words that sound alike but make no sense in context), wrong or missing punctuation, and wrong casing. " +
        "Remove stutters and words or phrases repeated by recognition loops (e.g. \"I I I think\", the same sentence twice). " +
        "Remove stray filler sounds like \"uh\" and \"um\" unless they matter." +
        "\n\nDo not rephrase correct speech, do not change meaning, and do not summarize. Keep names, slang and intentional dialect. " +
        "Keep all formatting tags (like <i>) and line breaks exactly as they are.";

    public const string MachineTranslationPrompt =
        "You are reviewing {language} subtitles that were machine translated." +
        "\n\nFix grammar, unnatural or overly literal wording, wrong word choices, inconsistent terms and wrong forms of address. " +
        "Keep the meaning, tone and each character's voice; prefer the smallest change that makes the line sound natural." +
        "\n\nDo not add or remove information and do not shorten lines. Keep names as they are. " +
        "Keep all formatting tags (like <i>) and line breaks exactly as they are.";

    public const string SdhPrompt =
        "You are a proofreader for {language} subtitles for the deaf and hard of hearing (SDH)." +
        "\n\nFix typos, spelling, grammar and punctuation, and make sound descriptions in brackets consistent in style (e.g. [door slams], [laughs]). " +
        "Keep speaker labels and music notes." +
        "\n\nDo not rephrase dialogue or change meaning. Keep all formatting tags (like <i>) and line breaks exactly as they are.";

    public static List<AiReviewPromptTemplate> GetAll()
    {
        var l = Se.Language.Tools.AiReview;
        return new List<AiReviewPromptTemplate>
        {
            new(l.TemplateStandard, SeAiReview.DefaultPrompt),
            new(l.TemplateSpeechToText, SpeechToTextPrompt),
            new(l.TemplateMachineTranslation, MachineTranslationPrompt),
            new(l.TemplateSdh, SdhPrompt),
        };
    }
}

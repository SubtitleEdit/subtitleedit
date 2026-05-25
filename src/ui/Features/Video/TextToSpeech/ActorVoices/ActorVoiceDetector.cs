using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// Pulls the set of distinct actor names (ASSA) or voice names (WebVTT) out of a subtitle, so the
// TTS window can decide whether to expose the "Cast" button and what to pre-populate the cast
// dialog with. WebVTT path also offers a helper to strip the <v ...> wrapper before sending text
// to the engine, and to look up which voice owns a given paragraph.
public static class ActorVoiceDetector
{
    public enum CastKind
    {
        None,
        AssaActors,
        WebVttVoices,
    }

    // Returns which cast model applies to this subtitle. Format is passed in by the caller
    // (typically MainViewModel.SelectedSubtitleFormat) rather than read from subtitle.OriginalFormat
    // — the editor normalises subtitles to ASSA internally, so OriginalFormat doesn't reliably
    // reflect what the user is currently working with. Also requires actual actor/voice data so
    // the Cast button only shows when there's something real to assign.
    public static CastKind Detect(Subtitle? subtitle, SubtitleFormat? format)
    {
        if (subtitle == null || format == null || subtitle.Paragraphs.Count == 0)
        {
            return CastKind.None;
        }

        if (format is WebVTT)
        {
            return WebVTT.GetVoices(subtitle).Count > 0 ? CastKind.WebVttVoices : CastKind.None;
        }

        if (format is AdvancedSubStationAlpha || format is SubStationAlpha)
        {
            return subtitle.Paragraphs.Any(p => !string.IsNullOrWhiteSpace(p.Actor))
                ? CastKind.AssaActors
                : CastKind.None;
        }

        return CastKind.None;
    }

    public static IReadOnlyList<string> GetNames(Subtitle? subtitle, CastKind kind)
    {
        if (subtitle == null || kind == CastKind.None)
        {
            return Array.Empty<string>();
        }

        if (kind == CastKind.WebVttVoices)
        {
            return WebVTT.GetVoices(subtitle);
        }

        return subtitle.Paragraphs
            .Select(p => (p.Actor ?? string.Empty).Trim())
            .Where(a => a.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    // Returns the actor/voice name for a paragraph, or empty if none. For WebVTT it reads the
    // first <v ...> tag in the paragraph text; for ASSA it returns the Actor field.
    public static string GetParagraphActor(Paragraph paragraph, CastKind kind)
    {
        if (kind == CastKind.AssaActors)
        {
            return (paragraph.Actor ?? string.Empty).Trim();
        }

        if (kind == CastKind.WebVttVoices)
        {
            return ExtractWebVttVoice(paragraph.Text);
        }

        return string.Empty;
    }

    // Removes every WebVTT <v Name> wrapper from a paragraph so the engine never speaks the
    // tag. WebVTT.RemoveTag only strips one occurrence per call, so we loop until none remain
    // (otherwise paragraphs with multiple speakers leak unstripped "<v Bob>...</v>" markup
    // into the synthesized audio). Other markup (bold, italic, etc.) is left intact — the
    // engines already ignore tags they don't understand.
    public static string StripWebVttVoiceTags(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains("<v ", StringComparison.Ordinal))
        {
            return text;
        }

        var current = text;
        while (current.Contains("<v ", StringComparison.Ordinal))
        {
            var stripped = WebVTT.RemoveTag("v", current);
            if (stripped == current)
            {
                // RemoveTag couldn't make progress — bail out so we don't infinite-loop on
                // malformed input.
                break;
            }
            current = stripped;
        }
        return current;
    }

    // Drops engines that aren't usable right now — missing API key, missing runtime/models, etc.
    // Shared between the Cast dialog and the Review window so both surface exactly the same set
    // of engines. An engine in the raw list but not installable would otherwise show up in the
    // Cast dropdown, the user picks it, and BuildCastContextAsync silently swallows the
    // GetVoices exception → row falls back to the global voice with no UI feedback.
    public static IEnumerable<ITtsEngine> FilterUsableEngines(IEnumerable<ITtsEngine> engines)
    {
        var s = Se.Settings.Video.TextToSpeech;
        foreach (var engine in engines)
        {
            switch (engine)
            {
                case ElevenLabs when string.IsNullOrWhiteSpace(s.ElevenLabsApiKey):
                case Murf when string.IsNullOrWhiteSpace(s.MurfApiKey):
                case AzureSpeech when string.IsNullOrWhiteSpace(s.AzureApiKey):
                case GoogleSpeech when string.IsNullOrWhiteSpace(s.GoogleKeyFile):
                case MistralSpeech when string.IsNullOrWhiteSpace(s.MistralApiKey):
                    continue;

                case Qwen3TtsCpp when !File.Exists(Qwen3TtsCpp.GetExecutableFileName())
                    || (!Qwen3TtsCpp.IsModelsInstalled(Qwen3TtsCpp.ModelKey06B)
                        && !Qwen3TtsCpp.IsModelsInstalled(Qwen3TtsCpp.ModelKey17BBase)):
                    continue;

                case KokoroTtsCpp when !File.Exists(KokoroTtsCpp.GetExecutableFileName())
                    || !KokoroTtsCpp.AreModelsInstalled():
                    continue;
            }

            yield return engine;
        }
    }

    private static string ExtractWebVttVoice(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var start = text.IndexOf("<v ", StringComparison.Ordinal);
        if (start < 0)
        {
            return string.Empty;
        }

        var end = text.IndexOf('>', start);
        if (end <= start + 3)
        {
            return string.Empty;
        }

        return text.Substring(start + 3, end - start - 3).Trim();
    }
}

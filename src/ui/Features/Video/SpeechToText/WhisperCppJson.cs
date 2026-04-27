using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public static class WhisperCppJson
{
    public static List<TranscriptionSegment> GetTranscription(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var wrapper = JsonSerializer.Deserialize<WhisperResult>(jsonContent, options);

            return wrapper?.Transcription ?? [];
        }
        catch (JsonException ex)
        {
            Se.LogError($"Error parsing JSON: {ex.Message}");
            return [];
        }
    }

    public static List<SubtitleLineViewModel> ToSubtitleLineViewModels(string jsonContent)
    {
        var transcription = GetTranscription(jsonContent);
        var result = new List<SubtitleLineViewModel>();

        foreach (var segment in transcription)
        {
            result.Add(segment.ToSubtitleLineViewModel());
        }

        return result;
    }

    public static List<SubtitleLineViewModel> ToSubtitleLineViewModelsWithActiveWords(string jsonContent)
    {
        var transcription = GetTranscription(jsonContent);
        var result = new List<SubtitleLineViewModel>();

        foreach (var segment in transcription)
        {
            result.AddRange(segment.ToUnderlineActiveWords());
        }

        return result;
    }
}

public class WhisperResult
{
    [JsonPropertyName("transcription")]
    public List<TranscriptionSegment> Transcription { get; set; } = [];
}

public class TranscriptionSegment
{
    [JsonPropertyName("timestamps")]
    public TimeRange Timestamps { get; set; } = new TimeRange();

    [JsonPropertyName("offsets")]
    public OffsetRange Offsets { get; set; } = new OffsetRange();

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("tokens")]
    public List<Token> Tokens { get; set; } = [];

    public SubtitleLineViewModel ToSubtitleLineViewModel()
    {
        var startMs = TimeCode.ParseToMilliseconds(Timestamps.From);
        var endMs = TimeCode.ParseToMilliseconds(Timestamps.To);

        return new SubtitleLineViewModel
        {
            Text = Text,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs)
        };
    }

    public List<SubtitleLineViewModel> ToUnderlineActiveWords()
    {
        var result = new List<SubtitleLineViewModel>();

        if (Tokens == null || Tokens.Count == 0)
        {
            return result;
        }

        foreach (var token in Tokens)
        {
            var startMs = TimeCode.ParseToMilliseconds(token.Timestamps.From);
            var endMs = TimeCode.ParseToMilliseconds(token.Timestamps.To);

            // Build text with underlined active word
            var textBuilder = new StringBuilder();
            foreach (var t in Tokens)
            {
                if (t == token)
                {
                    textBuilder.Append("<u>");
                    textBuilder.Append(CleanText(t.Text));
                    textBuilder.Append("</u>");
                }
                else
                {
                    textBuilder.Append(CleanText(t.Text));
                }
            }

            var viewModel = new SubtitleLineViewModel
            {
                Text = textBuilder.ToString().Replace("<u></u>", string.Empty),
                StartTime = TimeSpan.FromMilliseconds(startMs),
                EndTime = TimeSpan.FromMilliseconds(endMs)
            };

            result.Add(viewModel);
        }

        return result;
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var begin = text.IndexOf('[');
        if (begin < 0)
        {
            return text;
        }

        var end = text.IndexOf(']', begin);
        if (end < 0)
        {
            return text;
        }

        text = text.Remove(begin, end - begin + 1);

        begin = text.IndexOf('[');
        if (begin < 0)
        {
            return text;
        }

        end = text.IndexOf(']', begin);
        if (end < 0)
        {
            return text;
        }

        text = text.Remove(begin, end - begin + 1);

        return text;
    }
}

public class Token
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("timestamps")]
    public TimeRange Timestamps { get; set; } = new TimeRange();

    [JsonPropertyName("offsets")]
    public OffsetRange Offsets { get; set; } = new OffsetRange();

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("p")]
    public double Probability { get; set; }

    [JsonPropertyName("t_dtw")]
    public int TDtw { get; set; }
}

public class TimeRange
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;
}

public class OffsetRange
{
    [JsonPropertyName("from")]
    public int From { get; set; }

    [JsonPropertyName("to")]
    public int To { get; set; }
}

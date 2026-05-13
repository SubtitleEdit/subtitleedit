using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

public class OpenAiCompatibleSttResponse
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("segments")]
    public List<OpenAiCompatibleSegment>? Segments { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }
}

public class OpenAiCompatibleSegment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("seek")]
    public int Seek { get; set; }

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("tokens")]
    public List<int>? Tokens { get; set; }

    [JsonPropertyName("avg_logprob")]
    public double AvgLogprob { get; set; }

    [JsonPropertyName("no_speech_prob")]
    public double NoSpeechProb { get; set; }

    [JsonPropertyName("words")]
    public List<OpenAiCompatibleWord>? Words { get; set; }
}

public class OpenAiCompatibleWord
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("probability")]
    public double Probability { get; set; }
}

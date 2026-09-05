using System.Text.Json;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.GoogleCloud;

namespace UITests.Features.Video.SpeechToText.GoogleCloud;

public class GoogleCloudSttServiceTests
{
    [Theory]
    [InlineData("us", "us-speech.googleapis.com")]
    [InlineData(" EU ", "eu-speech.googleapis.com")]
    [InlineData("global", "speech.googleapis.com")]
    [InlineData("", "speech.googleapis.com")]
    [InlineData(null, "speech.googleapis.com")]
    public void GetSpeechHost_PicksRegionalHost(string? region, string expected)
    {
        Assert.Equal(expected, GoogleCloudSttService.GetSpeechHost(region));
    }

    [Fact]
    public void DeriveBucketName_IsLowercaseAndWithinLimit()
    {
        Assert.Equal("my-project-123-subtitle-edit-stt", GoogleCloudSttService.DeriveBucketName("My_Project.123"));
        Assert.True(GoogleCloudSttService.DeriveBucketName(new string('x', 100)).Length <= 63);
    }

    [Fact]
    public void BuildRequestBody_UsesAutoLanguageAndWordTimings()
    {
        var body = GoogleCloudSttService.BuildRequestBody(new GoogleCloudSttSettings { Model = "chirp_3" }, "gs://b/o.mp3", null);
        using var doc = JsonDocument.Parse(body);
        var config = doc.RootElement.GetProperty("config");
        Assert.Equal("auto", config.GetProperty("languageCodes")[0].GetString());
        Assert.Equal("chirp_3", config.GetProperty("model").GetString());
        Assert.True(config.GetProperty("features").GetProperty("enableWordTimeOffsets").GetBoolean());
        Assert.Equal("gs://b/o.mp3", doc.RootElement.GetProperty("files")[0].GetProperty("uri").GetString());
        Assert.True(doc.RootElement.GetProperty("recognitionOutputConfig").TryGetProperty("inlineResponseConfig", out _));
    }

    [Fact]
    public void BuildRequestBody_PerChunkLanguageBeatsSetting()
    {
        var body = GoogleCloudSttService.BuildRequestBody(new GoogleCloudSttSettings { Language = "da-DK" }, "gs://b/o", "en-US");
        Assert.Contains("\"languageCodes\":[\"en-US\"]", body);
    }

    private const string OperationJson = """
        {
          "name": "projects/p/locations/us/operations/1",
          "done": true,
          "response": {
            "totalBilledDuration": "1080s",
            "results": {
              "gs://b/o.mp3": {
                "inlineResult": {
                  "transcript": {
                    "results": [
                      {
                        "alternatives": [{
                          "transcript": "Hello there.",
                          "words": [
                            { "word": "Hello", "startOffset": "0.500s", "endOffset": "0.900s" },
                            { "word": "there.", "startOffset": "1s", "endOffset": "1.400s" }
                          ]
                        }],
                        "resultEndOffset": "2s",
                        "languageCode": "en-US"
                      },
                      {
                        "alternatives": [{
                          "transcript": "Corrupt words.",
                          "words": [
                            { "word": "Corrupt", "startOffset": "6324s", "endOffset": "6325s" },
                            { "word": "words.", "startOffset": "5s", "endOffset": "4s" }
                          ]
                        }],
                        "resultEndOffset": "7.5s"
                      },
                      { "alternatives": [{ "transcript": "   " }] }
                    ]
                  }
                }
              }
            }
          }
        }
        """;

    [Fact]
    public void ParseResponse_MapsWordsToSegmentsAndDropsImpossibleTimings()
    {
        var response = GoogleCloudSttService.ParseResponse(OperationJson);

        Assert.NotNull(response.Segments);
        Assert.Equal(2, response.Segments!.Count);

        Assert.Equal("Hello there.", response.Segments[0].Text);
        Assert.Equal(0.5, response.Segments[0].Start, 3);
        Assert.Equal(1.4, response.Segments[0].End, 3);
        Assert.Equal(2, response.Segments[0].Words!.Count);

        // Both words fail the range check, so the segment falls back to the
        // previous end and Google's resultEndOffset.
        Assert.Equal("Corrupt words.", response.Segments[1].Text);
        Assert.Null(response.Segments[1].Words);
        Assert.Equal(1.4, response.Segments[1].Start, 3);
        Assert.Equal(7.5, response.Segments[1].End, 3);

        Assert.Equal("en", response.Language);
        Assert.Equal(1080, response.Duration);
        Assert.Equal("Hello there. Corrupt words.", response.Text);
    }

    [Fact]
    public void ParseResponse_ThrowsOnPerFileError()
    {
        const string json = """{"done":true,"response":{"results":{"gs://b/o":{"error":{"code":3,"message":"bad audio"}}}}}""";
        var ex = Assert.Throws<HttpRequestException>(() => GoogleCloudSttService.ParseResponse(json));
        Assert.Contains("bad audio", ex.Message);
    }

    [Fact]
    public void ParseDuration_ReadsProtoDurationStrings()
    {
        using var doc = JsonDocument.Parse("""["1.500s","12s","x"]""");
        Assert.Equal(1.5, GoogleCloudSttService.ParseDuration(doc.RootElement[0]));
        Assert.Equal(12, GoogleCloudSttService.ParseDuration(doc.RootElement[1]));
        Assert.Equal(-1, GoogleCloudSttService.ParseDuration(doc.RootElement[2]));
    }
}

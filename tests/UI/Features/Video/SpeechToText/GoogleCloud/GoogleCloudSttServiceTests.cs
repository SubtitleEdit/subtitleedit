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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BuildRequestBody_SendsProcessingStrategyOnlyWhenAsked(bool dynamicBatching)
    {
        var body = GoogleCloudSttService.BuildRequestBody(
            new GoogleCloudSttSettings { Model = "chirp_3", DynamicBatching = dynamicBatching }, "gs://b/o.flac", "tr-TR");

        using var doc = JsonDocument.Parse(body);
        var present = doc.RootElement.TryGetProperty("processingStrategy", out var strategy);

        Assert.Equal(dynamicBatching, present);
        if (dynamicBatching)
        {
            // Bills at roughly a fifth of the standard rate.
            Assert.Equal("DYNAMIC_BATCHING", strategy.GetString());
        }
    }

    /// <summary>
    /// totalBilledDuration is the natural bound for the word range check, but it is not
    /// guaranteed to be present. Without a fallback the check lets through exactly what it
    /// exists to catch. The offset here is a real one: 6,324 s inside an 1,080 s chunk.
    /// </summary>
    [Fact]
    public void ParseResponse_DropsImpossibleWordsWhenNoBilledDurationIsReported()
    {
        const string json = """
        {
          "response": {
            "results": {
              "gs://b/o.flac": {
                "inlineResult": {
                  "transcript": {
                    "results": [
                      {
                        "alternatives": [
                          {
                            "transcript": "bir iki",
                            "words": [
                              { "word": "bir", "startOffset": "1.0s", "endOffset": "1.4s" },
                              { "word": "iki", "startOffset": "6324.0s", "endOffset": "6324.5s" }
                            ]
                          }
                        ],
                        "resultEndOffset": "1080s"
                      }
                    ]
                  }
                }
              }
            }
          }
        }
        """;

        var response = GoogleCloudSttService.ParseResponse(json);

        var segment = Assert.Single(response.Segments!);
        Assert.NotNull(segment.Words);
        var word = Assert.Single(segment.Words!);
        Assert.Equal("bir", word.Word);
    }

    [Fact]
    public void ParseResponse_KeepsWordsWithinTheReportedDuration()
    {
        const string json = """
        {
          "response": {
            "totalBilledDuration": "1080s",
            "results": {
              "gs://b/o.flac": {
                "inlineResult": {
                  "transcript": {
                    "results": [
                      {
                        "alternatives": [
                          {
                            "transcript": "bir iki",
                            "words": [
                              { "word": "bir", "startOffset": "1.0s", "endOffset": "1.4s" },
                              { "word": "iki", "startOffset": "1079.0s", "endOffset": "1079.6s" }
                            ]
                          }
                        ],
                        "resultEndOffset": "1080s"
                      }
                    ]
                  }
                }
              }
            }
          }
        }
        """;

        var response = GoogleCloudSttService.ParseResponse(json);

        var segment = Assert.Single(response.Segments!);
        Assert.Equal(2, segment.Words!.Count);
        Assert.Equal(1.0, segment.Start, 3);
        Assert.Equal(1079.6, segment.End, 3);
    }

    /// <summary>
    /// proto3 JSON omits zero-valued fields, so a word starting at 0.000s arrives with no
    /// startOffset at all. Verified against the live API: in a 160 word response exactly
    /// one word, the first, has no startOffset. Treating that absence as an invalid offset
    /// silently dropped the first word of every result.
    /// </summary>
    [Fact]
    public void ParseResponse_KeepsAWordWhoseStartOffsetIsOmittedBecauseItIsZero()
    {
        const string json = """
        {
          "response": {
            "totalBilledDuration": "65s",
            "results": {
              "gs://b/o.flac": {
                "inlineResult": {
                  "transcript": {
                    "results": [
                      {
                        "alternatives": [
                          {
                            "transcript": "Ay vay",
                            "words": [
                              { "word": "Ay", "endOffset": "1.560s" },
                              { "word": "vay", "startOffset": "1.560s", "endOffset": "1.800s" }
                            ]
                          }
                        ]
                      }
                    ]
                  }
                }
              }
            }
          }
        }
        """;

        var response = GoogleCloudSttService.ParseResponse(json);

        var segment = Assert.Single(response.Segments!);
        Assert.Equal(2, segment.Words!.Count);
        Assert.Equal("Ay", segment.Words[0].Word);
        Assert.Equal(0.0, segment.Words[0].Start, 3);
        Assert.Equal(0.0, segment.Start, 3);
    }
}

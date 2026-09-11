using Nikse.SubtitleEdit.UiLogic.Grammar;

namespace UITests.Features.Tools.GrammarCheck;

public class LanguageToolClientTests
{
    // Trimmed to the fields Subtitle Edit reads, otherwise as a LanguageTool 6.x server answers.
    private const string CheckJson = """
        {
          "software": { "name": "LanguageTool", "version": "6.4" },
          "language": { "name": "English (US)", "code": "en-US" },
          "matches": [
            {
              "message": "The pronoun 'He' is usually used with a third-person verb.",
              "shortMessage": "Agreement error",
              "replacements": [ { "value": "goes" }, { "value": "went" }, { "value": "goes" } ],
              "offset": 3,
              "length": 2,
              "context": { "text": "He go to school.", "offset": 3, "length": 2 },
              "rule": {
                "id": "HE_VERB_AGR",
                "description": "Subject-verb agreement error",
                "issueType": "grammar",
                "category": { "id": "GRAMMAR", "name": "Grammar" }
              }
            },
            {
              "message": "Use 'an' instead of 'a' if the following word starts with a vowel sound.",
              "shortMessage": "",
              "replacements": [ { "value": "an" } ],
              "offset": 23,
              "length": 1,
              "rule": {
                "id": "EN_A_VS_AN",
                "description": "Use of 'a' vs. 'an'",
                "issueType": "misspelling",
                "category": { "id": "MISC", "name": "Miscellaneous" }
              }
            }
          ]
        }
        """;

    [Fact]
    public void ParseMatches_ReadsOffsetsRuleAndReplacements()
    {
        var matches = LanguageToolClient.ParseMatches(CheckJson);

        Assert.Equal(2, matches.Count);
        Assert.Equal(3, matches[0].Offset);
        Assert.Equal(2, matches[0].Length);
        Assert.Equal("HE_VERB_AGR", matches[0].RuleId);
        Assert.Equal("grammar", matches[0].IssueType);
        Assert.Equal("GRAMMAR", matches[0].CategoryId);
        Assert.Equal("Agreement error", matches[0].ShortMessage);
        Assert.Equal("EN_A_VS_AN", matches[1].RuleId);
        Assert.Equal(new[] { "an" }, matches[1].Replacements);
    }

    [Fact]
    public void ParseMatches_DropsDuplicateReplacements()
    {
        var matches = LanguageToolClient.ParseMatches(CheckJson);

        Assert.Equal(new[] { "goes", "went" }, matches[0].Replacements);
    }

    [Fact]
    public void ParseMatches_NoMatches_ReturnsEmpty()
    {
        Assert.Empty(LanguageToolClient.ParseMatches("""{"matches":[]}"""));
        Assert.Empty(LanguageToolClient.ParseMatches("""{"software":{"name":"LanguageTool"}}"""));
    }

    [Fact]
    public void ParseLanguages_ReadsNameCodeAndLongCode()
    {
        var json = """
            [
              { "name": "English", "code": "en", "longCode": "en" },
              { "name": "English (US)", "code": "en", "longCode": "en-US" },
              { "name": "German", "code": "de" }
            ]
            """;

        var languages = LanguageToolClient.ParseLanguages(json);

        Assert.Equal(3, languages.Count);
        Assert.Equal("en-US", languages[1].LongCode);
        Assert.Equal("en", languages[1].Code);
        Assert.Equal("English (US)", languages[1].ToString());

        // no longCode in the payload - fall back to the plain code rather than dropping the language
        Assert.Equal("de", languages[2].LongCode);
    }

    [Theory]
    [InlineData("https://languagetool.example.org", "https://languagetool.example.org/v2/check")]
    [InlineData("https://languagetool.example.org/", "https://languagetool.example.org/v2/check")]
    [InlineData("https://languagetool.example.org/v2", "https://languagetool.example.org/v2/check")]
    [InlineData("https://languagetool.example.org/v2/check", "https://languagetool.example.org/v2/check")]
    [InlineData("  https://languagetool.example.org/v2/check  ", "https://languagetool.example.org/v2/check")]
    [InlineData("languagetool.example.org", "https://languagetool.example.org/v2/check")]
    [InlineData("http://localhost:8010", "http://localhost:8010/v2/check")]
    public void GetEndpointUrl_AcceptsWhateverTheUserPasted(string serverUrl, string expected)
    {
        Assert.Equal(expected, LanguageToolClient.GetEndpointUrl(serverUrl, "/v2/check"));
    }

    [Fact]
    public void GetEndpointUrl_EmptyServer_UsesThePublicApi()
    {
        Assert.Equal(LanguageToolClient.DefaultServerUrl + "/v2/languages",
            LanguageToolClient.GetEndpointUrl(string.Empty, "/v2/languages"));
    }
}

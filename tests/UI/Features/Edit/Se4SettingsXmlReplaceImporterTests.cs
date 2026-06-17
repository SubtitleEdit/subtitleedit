using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using System.Linq;

namespace UITests.Features.Edit;

public class Se4SettingsXmlReplaceImporterTests
{
    // The classic SE 4 Settings.xml shape: groups carry <Enabled> and rules
    // live under a <Rules> wrapper as <MultipleSearchAndReplaceSetting>.
    private const string Se4SettingsXml = @"<?xml version=""1.0""?>
<Settings>
  <MultipleSearchAndReplaceGroups>
    <MultipleSearchAndReplaceGroup>
      <Name>Fix OCR errors</Name>
      <Enabled>true</Enabled>
      <Rules>
        <MultipleSearchAndReplaceSetting>
          <Enabled>true</Enabled>
          <FindWhat>teh</FindWhat>
          <ReplaceWith>the</ReplaceWith>
          <SearchType>Normal</SearchType>
          <Description>typo</Description>
        </MultipleSearchAndReplaceSetting>
        <MultipleSearchAndReplaceSetting>
          <Enabled>false</Enabled>
          <FindWhat>\d+</FindWhat>
          <ReplaceWith>#</ReplaceWith>
          <SearchType>RegularExpression</SearchType>
        </MultipleSearchAndReplaceSetting>
      </Rules>
    </MultipleSearchAndReplaceGroup>
  </MultipleSearchAndReplaceGroups>
</Settings>";

    [Fact]
    public void ImportFromXml_ParsesGroupsAndRules()
    {
        var categories = Se4SettingsXmlReplaceImporter.ImportFromXml(Se4SettingsXml);

        var category = Assert.Single(categories);
        Assert.Equal("Fix OCR errors", category.Name);
        Assert.True(category.IsActive);
        Assert.Equal(2, category.Rules.Count);

        var first = category.Rules[0];
        Assert.Equal("teh", first.Find);
        Assert.Equal("the", first.ReplaceWith);
        Assert.Equal("typo", first.Description);
        Assert.True(first.Active);
        Assert.Equal(MultipleReplaceType.CaseInsensitive, first.Type);

        var second = category.Rules[1];
        Assert.Equal(@"\d+", second.Find);
        Assert.False(second.Active);
        Assert.Equal(MultipleReplaceType.RegularExpression, second.Type);
    }

    [Fact]
    public void ImportFromXml_SupportsVariantNaming()
    {
        // Alternate shape some SE builds emit: <Group>/<IsActive> with <Rule>/<Active>.
        const string xml = @"<Settings>
  <MultipleSearchAndReplaceGroups>
    <Group>
      <Name>Casing</Name>
      <IsActive>true</IsActive>
      <Rules>
        <Rule>
          <Active>true</Active>
          <FindWhat>i</FindWhat>
          <ReplaceWith>I</ReplaceWith>
          <SearchType>CaseSensitive</SearchType>
        </Rule>
      </Rules>
    </Group>
  </MultipleSearchAndReplaceGroups>
</Settings>";

        var categories = Se4SettingsXmlReplaceImporter.ImportFromXml(xml);

        var category = Assert.Single(categories);
        Assert.Equal("Casing", category.Name);
        var rule = Assert.Single(category.Rules);
        Assert.Equal("i", rule.Find);
        Assert.Equal(MultipleReplaceType.CaseSensitive, rule.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not xml at all")]
    [InlineData("<Settings></Settings>")]
    public void ImportFromXml_ReturnsEmptyOnMissingOrInvalid(string xml)
    {
        Assert.Empty(Se4SettingsXmlReplaceImporter.ImportFromXml(xml));
    }
}
